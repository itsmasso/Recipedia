using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Recipedia.Models;

namespace Recipedia.Data.Services
{
	public class GeminiService : IGeminiService
	{
		private readonly HttpClient _http;
		private readonly string _apiKey;

		private static readonly string[] Categories = { "Dessert", "Dinner", "Lunch", "Snack", "Breakfast", "Drink" };
		private static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

		public GeminiService(HttpClient http, IConfiguration config)
		{
			_http = http;
			_apiKey = config["Gemini:ApiKey"];
		}

		public async Task<GeneratedRecipeResultDTO> GenerateRecipeAsync(string ingredients, string category, string difficulty)
		{
			try
			{
				Random random = new Random();

				if (category == "RandomCategory" || string.IsNullOrEmpty(category))
					category = Categories[random.Next(Categories.Length)];

				if (difficulty == "RandomDifficulty" || string.IsNullOrEmpty(difficulty))
					difficulty = Difficulties[random.Next(Difficulties.Length)];

				var prompt = $@"
					Generate a {difficulty} {category} recipe using these ingredients: {ingredients}.
					Specify the estimated cook time in minutes.
					Return the response ONLY in this strict JSON format with no additional text:
					Do NOT include Category or Difficulty in the JSON. They will be handled separately:

					{{
					  ""Title"": ""Recipe title"",
					  ""Ingredients"": [""ingredient1"", ""ingredient2""],
					  ""Instructions"": [""step1"", ""step2""],
					  ""CookTimeMinutes"": 0
					}}";

				//Create request body
				var requestBody = new
				{
					contents = new[]
					{
						new
						{
							parts = new[]
							{
								new { text = prompt }
							}
						}
					},
					generationConfig = new
					{
						temperature = 0.7,
						topK = 40,
						topP = 0.95,
						maxOutputTokens = 1024,
					}
				};

				//Create HTTP request
				string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
				var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
				httpRequest.Headers.Add("X-goog-api-key", _apiKey);
				httpRequest.Content = new StringContent(
					JsonSerializer.Serialize(requestBody),
					Encoding.UTF8,
					"application/json"
				);

				//send request
				var response = await _http.SendAsync(httpRequest);
				var responseContent = await response.Content.ReadAsStringAsync();

				response.EnsureSuccessStatusCode();

				//Parse Gemini API response
				using var doc = JsonDocument.Parse(responseContent);
				var content = doc.RootElement
					.GetProperty("candidates")[0]
					.GetProperty("content")
					.GetProperty("parts")[0]
					.GetProperty("text")
					.GetString();

				if (string.IsNullOrWhiteSpace(content))
				{
					return CreateFallbackRecipe("No content generated");
				}
				Debug.WriteLine(content);
				//Clean and parse JSON response
				var jsonStart = content.IndexOf('{');
				var jsonEnd = content.LastIndexOf('}');
				if (jsonStart >= 0 && jsonEnd > jsonStart)
				{
					content = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
				}

                var generatedRecipe = JsonSerializer.Deserialize<GeneratedRecipeResultDTO>(content)
                      ?? new GeneratedRecipeResultDTO();

                generatedRecipe.Difficulty = difficulty;
                generatedRecipe.Category = category;

                return generatedRecipe ?? CreateFallbackRecipe("Failed to parse recipe");
			}
			catch (HttpRequestException ex)
			{
				Debug.WriteLine($"HTTP Error: {ex.Message}");
				return CreateFallbackRecipe($"API Error: {ex.Message}");
			}
			catch (JsonException ex)
			{
				Debug.WriteLine($"JSON Parse Error: {ex.Message}");
				return CreateFallbackRecipe($"Parse Error: {ex.Message}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"General Error: {ex.Message}");
				return CreateFallbackRecipe($"Error: {ex.Message}");
			}
		}

		private GeneratedRecipeResultDTO CreateFallbackRecipe(string error)
		{
			return new GeneratedRecipeResultDTO
			{
				Title = "Sample Recipe (API Error)",
				Ingredients = new List<string>
				{
					"2 cups flour",
					"1 cup sugar",
					"2 eggs",
					"1 cup milk"
				},
				Instructions = new List<string>
				{
					"Mix dry ingredients in a bowl",
					"Add wet ingredients and mix well",
					"Cook according to recipe type",
					$"Note: {error}"
				}
			};
		}
	}
}