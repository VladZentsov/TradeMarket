using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Data
{
    public class TradeMarketDbContext : DbContext, ITradeMarketDbContext
    {
        //public TradeMarketDbContext()
        //{
        //    Database.EnsureCreated();
        //}
        //public TradeMarketDbContext(DbContextOptions<TradeMarketDbContext> options) : base(options)
        //{
        //    Database.EnsureCreated();
        //}

        public TradeMarketDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TradeMarket;Trusted_Connection=True;");
        //}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Receipt>().HasKey(c => c.Id);
            modelBuilder.Entity<ReceiptDetail>().HasKey(c => c.Id);
            modelBuilder.Entity<Product>().HasKey(c => c.Id);
            modelBuilder.Entity<Person>().HasKey(c => c.Id);
            modelBuilder.Entity<ProductCategory>().HasKey(c => c.Id);


            modelBuilder.Entity<Customer>().HasOne(c => c.Person);
            modelBuilder.Entity<Receipt>().HasOne(r=>r.Customer).WithMany(c=>c.Receipts).HasForeignKey(r=>r.CustomerId);
            modelBuilder.Entity<ReceiptDetail>().HasOne(rd=>rd.Receipt).WithMany(r=>r.ReceiptDetails).HasForeignKey(rd=>rd.ReceiptId);
            modelBuilder.Entity<ReceiptDetail>().HasOne(rd => rd.Product).WithMany(p => p.ReceiptDetails).HasForeignKey(rd => rd.ProductId);
            modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(pc => pc.Products).HasForeignKey(p => p.ProductCategoryId);
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<Receipt> Receipts { get; set; }
        public virtual DbSet<ReceiptDetail> ReceiptsDetails { get; set; }
    }
}
