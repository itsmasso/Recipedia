using System.ComponentModel.DataAnnotations;

namespace Recipedia.Models
{
	public class GenerateRecipeInputModel
	{
		public string IngredientsTags { get; set; }
		public string Category { get; set; }
		public string Difficulty { get; set; }

	}
}
