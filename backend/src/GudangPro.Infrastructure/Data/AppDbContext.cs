using GudangPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<UserWarehouse> UserWarehouses => Set<UserWarehouse>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<StockTransactionLine> StockTransactionLines => Set<StockTransactionLine>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Username).IsUnique();
            b.Property(u => u.Username).HasMaxLength(50).IsRequired();
            b.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            b.Property(u => u.Role).HasMaxLength(30).IsRequired();
        });

        // Warehouse
        modelBuilder.Entity<Warehouse>(b =>
        {
            b.HasKey(w => w.Id);
            b.HasIndex(w => w.Code).IsUnique();
            b.Property(w => w.Code).HasMaxLength(20).IsRequired();
            b.Property(w => w.Name).HasMaxLength(100).IsRequired();
        });

        // UserWarehouse (many-to-many)
        modelBuilder.Entity<UserWarehouse>(b =>
        {
            b.HasKey(uw => new { uw.UserId, uw.WarehouseId });
            b.HasOne(uw => uw.User).WithMany(u => u.UserWarehouses).HasForeignKey(uw => uw.UserId);
            b.HasOne(uw => uw.Warehouse).WithMany(w => w.UserWarehouses).HasForeignKey(uw => uw.WarehouseId);
        });

        // Category & Unit
        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.Name).IsUnique();
            b.Property(c => c.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Unit>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Name).IsUnique();
            b.Property(u => u.Name).HasMaxLength(20).IsRequired();
        });

        // Item
        modelBuilder.Entity<Item>(b =>
        {
            b.HasKey(i => i.Id);
            // FR-MST-02: Kode barang unik
            b.HasIndex(i => i.Code).IsUnique();
            b.Property(i => i.Code).HasMaxLength(30).IsRequired();
            b.Property(i => i.Name).HasMaxLength(150).IsRequired();

            b.HasOne(i => i.Category).WithMany(c => c.Items).HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(i => i.Unit).WithMany(u => u.Items).HasForeignKey(i => i.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        // StockLevel (per barang per gudang)
        modelBuilder.Entity<StockLevel>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => new { s.ItemId, s.WarehouseId }).IsUnique();

            b.HasOne(s => s.Item).WithMany(i => i.StockLevels).HasForeignKey(s => s.ItemId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(s => s.Warehouse).WithMany(w => w.StockLevels).HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Cascade);

            // Concurrency token via rowversion (PRD §9.3, §10.3)
            b.Property(s => s.RowVersion).IsRowVersion();
        });

        // StockTransaction
        modelBuilder.Entity<StockTransaction>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.TransactionNo).IsUnique();
            b.Property(t => t.TransactionNo).HasMaxLength(40).IsRequired();
            b.Property(t => t.ReferenceNo).HasMaxLength(50).IsRequired();
            b.Property(t => t.CreatedBy).HasMaxLength(100).IsRequired();
            b.Property(t => t.Type).HasConversion<string>().HasMaxLength(10);

            b.HasOne(t => t.Warehouse).WithMany(w => w.Transactions).HasForeignKey(t => t.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(t => t.CancelledOf).WithMany().HasForeignKey(t => t.CancelledOfId).OnDelete(DeleteBehavior.Restrict);
        });

        // StockTransactionLine
        modelBuilder.Entity<StockTransactionLine>(b =>
        {
            b.HasKey(l => l.Id);
            b.HasOne(l => l.Transaction).WithMany(t => t.Lines).HasForeignKey(l => l.TransactionId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(l => l.Item).WithMany(i => i.TransactionLines).HasForeignKey(l => l.ItemId).OnDelete(DeleteBehavior.Restrict);
        });

        // Alert
        modelBuilder.Entity<Alert>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Level).HasMaxLength(20).IsRequired();
            b.HasOne(a => a.Item).WithMany(i => i.Alerts).HasForeignKey(a => a.ItemId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Warehouse).WithMany().HasForeignKey(a => a.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog (PRD §7.8: tidak boleh diubah/dihapus)
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.Action).HasMaxLength(30).IsRequired();
            b.Property(l => l.EntityName).HasMaxLength(50).IsRequired();
            b.Property(l => l.Username).HasMaxLength(50).IsRequired();
            b.HasOne(l => l.User).WithMany(u => u.AuditLogs).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}