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

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        if (connString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
            connString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
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

    // Otomatis migrate & seed database saat start
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(db);
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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}