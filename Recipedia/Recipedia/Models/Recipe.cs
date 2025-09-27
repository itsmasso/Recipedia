using System.ComponentModel.DataAnnotations;
namespace Recipedia.Models
{
	public class Recipe
	{
		public int Id { get; set; }
		[Required]
		public string Title { get; set; } = string.Empty;
		[Required]
		public string Ingredients { get; set; } = string.Empty;
		[Required]
		public string Instructions { get; set; } = string.Empty;
		public string? Category { get; set; } 
		public int? CookTimeMinutes { get; set; }
		public string? Difficulty { get; set; }
		public string? ImageUrl { get; set; }
	}
}
