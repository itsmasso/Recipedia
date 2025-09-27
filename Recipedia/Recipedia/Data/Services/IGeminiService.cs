namespace Recipedia.Data.Services
{
	public interface IGeminiService
	{
		Task<string> GenerateRecipeAsync(string ingredients, string category, string difficulty);
	}
}
