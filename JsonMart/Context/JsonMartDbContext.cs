using JsonMart.Entities;
using Microsoft.EntityFrameworkCore;

namespace JsonMart.Context;

public class JsonMartDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<StockEntity> Stocks { get; set; }

    public JsonMartDbContext(DbContextOptions<JsonMartDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);

        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<ProductEntity>()
            .HasOne(p => p.Stock)
            .WithOne(s => s.Product)
            .HasForeignKey<StockEntity>(s => s.ProductId);
    }
}