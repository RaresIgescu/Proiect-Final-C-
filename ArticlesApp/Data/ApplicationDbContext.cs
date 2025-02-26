using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Data
{
    // PASUL 3: useri si roluri
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ArticleOrder> ArticleOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder
modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // definire primary key compus
            modelBuilder.Entity<ArticleOrder>()
            .HasKey(ab => new {
                ab.Id,
                ab.ArticleId,
                ab.OrderId
            });
            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<ArticleOrder>()
            .HasOne(ab => ab.Article)
            .WithMany(ab => ab.ArticleOrders)
            .HasForeignKey(ab => ab.ArticleId);
            modelBuilder.Entity<ArticleOrder>()
            .HasOne(ab => ab.Order)
            .WithMany(ab => ab.ArticleOrders)
            .HasForeignKey(ab => ab.OrderId);
        }
    }
}

