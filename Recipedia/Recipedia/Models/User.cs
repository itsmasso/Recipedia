using Microsoft.AspNetCore.Identity;

namespace Recipedia.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
