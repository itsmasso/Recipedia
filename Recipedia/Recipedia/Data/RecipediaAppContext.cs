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
        }
    }

}
