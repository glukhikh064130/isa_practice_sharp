using Database.entities;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AppCtx : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Deal> Deals { get; set; }

        public AppCtx(DbContextOptions<AppCtx> opts) : base(opts)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Product>()
                .HasMany(p => p.Customers)
                .WithMany(c => c.Products)
                .UsingEntity<Deal>(
                    j => j
                        .HasOne(d => d.Customer)
                        .WithMany(c => c.Deals)
                        .HasForeignKey(d => d.CustomerId),
                    j => j
                        .HasOne(d => d.Product)
                        .WithMany(p => p.Deals)
                        .HasForeignKey(d => d.ProductId),
                    j =>
                    {
                        j.Property(d => d.Date).HasDefaultValueSql("CURRENT_TIMESTAMP");
                        j.Property(d => d.Amount).HasDefaultValue(1);
                        j.HasKey(d => new { d.ProductId, d.CustomerId });
                        j.ToTable("deals");
                    });

            modelBuilder.Entity<Product>().HasData(new Product[]
            {
                new() { Id = 1, Good = "hat", Price = 10.0, Category = "clothes" },
                new() { Id = 2, Good = "bmw", Price = 1000.0, Category = "cars" },
                new() { Id = 3, Good = "audi", Price = 1100.0, Category = "cars" },
                new() { Id = 4, Good = "fiat", Price = 800.0, Category = "cars" }
            });

            modelBuilder.Entity<Customer>().HasData(new Customer[]
            {
                new() { Id = 1, Name = "Ignat" },
                new() { Id = 2, Name = "Ivan" },
            });
        }
    }
}