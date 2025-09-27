using Microsoft.EntityFrameworkCore;
using Recipedia.Models;

namespace Recipedia.Data
{
    public class RecipediaAppContext : DbContext
    {
        public RecipediaAppContext(DbContextOptions<RecipediaAppContext> options) : base(options) { }
        public DbSet<Recipe> Recipes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>().HasData(
        new Recipe { Id = 1, Title = "Spaghetti Bolognese" },
        new Recipe { Id = 2, Title = "Chicken Curry" },
        new Recipe { Id = 3, Title = "Grilled Cheese Sandwich" },
        new Recipe { Id = 4, Title = "Veggie Stir Fry" }
    );
        }
    }

}
