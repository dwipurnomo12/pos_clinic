using Microsoft.EntityFrameworkCore;
using pos.Models;

namespace pos.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Customer> Customers { get; set; }  
        public DbSet<Supplier> Suppliers{ get; set; }

    }
}
