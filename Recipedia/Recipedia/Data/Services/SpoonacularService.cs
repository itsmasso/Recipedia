using System.Diagnostics;
using System.Text.Json;

namespace Recipedia.Data.Services
{
    public class SpoonacularService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public SpoonacularService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Spoonacular:ApiKey"];
        }

        public async Task<bool> IsValidIngredientAsync(string ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredient)) return false;

            try
            {
                var url = $"https://api.spoonacular.com/food/ingredients/autocomplete?query={ingredient}&number=5&apiKey={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);

                var results = JsonSerializer.Deserialize<List<SpoonIngredient>>(response) ?? new List<SpoonIngredient>();

                var isValid = results.Any();

                Debug.WriteLine(isValid
                    ? $"Ingredient '{ingredient}' is valid."
                    : $"Ingredient '{ingredient}' is NOT valid.");

                return isValid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying ingredient '{ingredient}': {ex.Message}");
                return false;
            }
        }

        private class SpoonIngredient
        {
            public string Name { get; set; }
        }
    }
}
