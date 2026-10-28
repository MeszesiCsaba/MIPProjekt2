using System.Data.Entity;

namespace AdministratieMagazine
{
    public class StoreContext : DbContext

    {
        public StoreContext() : base("name=StoreConnection")
        {
            Database.SetInitializer<StoreContext>(null);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> Categories { get; set; }
        public DbSet<SalesHistory> SalesHistory { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<ProductCategory>().ToTable("Categories");
            modelBuilder.Entity<SalesHistory>().ToTable("SalesHistory");

            modelBuilder.Entity<Product>()
                .HasRequired(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SalesHistory>()
                .HasRequired(sale => sale.Product)
                .WithMany(product => product.SalesHistory)
                .HasForeignKey(sale => sale.ProductId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}