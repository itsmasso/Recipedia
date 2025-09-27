using Recipedia.Models;

namespace Recipedia.Data.Services
{
	public interface IGeminiService
	{
		Task<GeneratedRecipeResultDTO> GenerateRecipeAsync(string ingredients, string category, string difficulty);
	}
}
