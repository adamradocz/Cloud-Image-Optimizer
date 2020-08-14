using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ImageOptimizer.Models;

namespace ImageOptimizer.Data
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserMonthlyOptimization> UserMonthlyOptimizations { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ConvertedImage> ConvertedImages { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.ForSqlServerUseIdentityColumns();
        }
    }
}
