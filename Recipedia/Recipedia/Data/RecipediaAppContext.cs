using Microsoft.EntityFrameworkCore;
using Recipedia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Recipedia.Data
{
    public class RecipediaAppContext : IdentityDbContext<User>
    {
        public RecipediaAppContext(DbContextOptions<RecipediaAppContext> options) : base(options) { }
        public DbSet<Recipe> Recipes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
