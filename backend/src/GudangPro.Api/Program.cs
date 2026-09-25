using System.Text;
using GudangPro.Application.Services;
using GudangPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Support dynamic PORT environment variable (Render, Railway, Koyeb, etc.)
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
    {
        builder.WebHost.UseUrls($"http://+:{port}");
    }

    // Serilog (PRD §10.1)
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // DbContext: Support both PostgreSQL (Supabase/Neon) and SQL Server
    var connString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    var isPostgres = connString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                     connString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                     connString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

    if (isPostgres)
    {
        connString = NormalizePostgresConnectionString(connString);
    }

    if (connString.Contains("db.", StringComparison.OrdinalIgnoreCase) && connString.Contains(".supabase.co", StringComparison.OrdinalIgnoreCase))
    {
        Log.Warning("NOTE: 'db.*.supabase.co' is IPv6-only. On Render (IPv4-only), use Supabase Connection Pooler host ('aws-0-*.pooler.supabase.co:5432') with username 'postgres.[PROJECT-REF]'.");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        if (isPostgres)
        {
            options.UseNpgsql(connString, sql => sql.MigrationsAssembly("GudangPro.Infrastructure"));
        }
        else
        {
            options.UseSqlServer(connString, sql => sql.MigrationsAssembly("GudangPro.Infrastructure"));
        }
    });

    // Application Services DI
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IAlertService, AlertService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddScoped<IStockService, StockService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();

    // JWT Authentication (PRD §7.1, §10.1)
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "GudangPro-Super-Secret-Key-For-JWT-2026-Min-32-Chars-Very-Long!";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GudangProApi";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GudangProFrontend";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // CORS for Angular frontend (PRD §9.2)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                var uri = new Uri(origin);
                return uri.Host == "localhost" 
                    || uri.Host == "127.0.0.1" 
                    || uri.Host.EndsWith("vercel.app") 
                    || uri.Host.EndsWith("onrender.com");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger with JWT Bearer support (PRD §9.7, §10.1)
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "GudangPro API",
            Version = "v1",
            Description = "Sistem Manajemen Inventori Gudang untuk UKM Manufaktur — REST API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header menggunakan skema Bearer. Masukkan token Anda."
        });

        c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    var app = builder.Build();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    // Otomatis migrate & seed database saat start (non-blocking jika database lambat start)
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Log.Information("Checking database schema and seed data...");
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(db);
        Log.Information("Database successfully initialized and ready.");
    }
    catch (Exception dbEx)
    {
        Log.Error(dbEx, "Database initialization warning: Could not initialize database on startup. Please check connection string / IPv4 pooler settings.");
    }

    // Swagger UI enabled for demo testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GudangPro API v1");
        c.RoutePrefix = string.Empty; // Swagger di root
    });

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health & DB connectivity check endpoint
    app.MapGet("/api/health", async (AppDbContext db) =>
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync();
            return Results.Ok(new
            {
                status = canConnect ? "Healthy" : "DatabaseUnreachable",
                database = canConnect ? "Connected" : "Cannot Connect",
                provider = db.Database.ProviderName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Results.Json(new
            {
                status = "DatabaseError",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            }, statusCode: 503);
        }
    });

    var serverPort = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    Log.Information("GudangPro API listening on http://0.0.0.0:{Port}", serverPort);
    app.Run($"http://0.0.0.0:{serverPort}");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static string NormalizePostgresConnectionString(string input)
{
    if (string.IsNullOrWhiteSpace(input)) return input;

    // Check if it's a URI format (postgres:// or postgresql://)
    if (input.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        input.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(input);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

            // Auto-trim square brackets if user wrote [YOUR-PASSWORD] or [mypassword]
            if (password.StartsWith("[") && password.EndsWith("]") && password.Length >= 2)
            {
                password = password[1..^1];
            }

            if (password.Equals("YOUR-PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("Supabase password is still set to placeholder 'YOUR-PASSWORD'!");
            }

            var host = uri.Host;
            var dbPort = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            if (string.IsNullOrEmpty(database)) database = "postgres";

            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = dbPort,
                Database = database,
                Username = username,
                Password = password,
                SslMode = Npgsql.SslMode.Require
            };
            return builder.ConnectionString;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to parse postgresql URI, falling back to ADO.NET builder");
        }
    }

    // If it's ADO.NET key=value format (e.g. Host=...;Username=...;Password=...)
    try
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(input);
        if (!string.IsNullOrEmpty(builder.Password) && builder.Password.StartsWith("[") && builder.Password.EndsWith("]") && builder.Password.Length >= 2)
        {
            builder.Password = builder.Password[1..^1];
        }
        builder.SslMode = Npgsql.SslMode.Require;
        return builder.ConnectionString;
    }
    catch
    {
        return input;
    }
}