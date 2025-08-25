using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamentos User -> Addresses
            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne()
                .HasForeignKey(a => a.UserId);

            // Relacionamentos Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne()
                .HasForeignKey<Payment>(p => p.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipping)
                .WithOne()
                .HasForeignKey<Shipping>(s => s.OrderId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId);

            // Relacionamentos Cart
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne()
                .HasForeignKey(ci => ci.CartId);

            // Configurações de tipos de dados para decimal
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Shipping>().Property(s => s.ShippingCost).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CartItem>().Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");
        }
    }
}