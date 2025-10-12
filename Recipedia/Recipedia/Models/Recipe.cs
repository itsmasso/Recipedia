using System.ComponentModel.DataAnnotations;
namespace Recipedia.Models
{
	public class Recipe
	{
		public int Id { get; set; }
		[Required]
		public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        //Foreign key to user
        [Required]
        public string UserId { get; set; }
        //Navigation property
        public User User { get; set; }
    }
}
