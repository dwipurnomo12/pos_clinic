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
        public DbSet<IncomingItem> IncomingItems { get; set; }
        public DbSet<ItemReturned> ItemReturneds { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<Finance> Finances { get; set; }
        public DbSet<FinancialHistory> FinancialHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // add relation one to many item to incoming item
            modelBuilder.Entity<IncomingItem>()
                .HasOne(i => i.Item)
                .WithMany(i => i.IncomingItems)
                .HasForeignKey(i => i.ItemId);

            // add default data for finance
            modelBuilder.Entity<Finance>().HasData(new Finance
            {
                Id = 1,
                Nominal = 0,
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
