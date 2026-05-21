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
		// Services and config
		private readonly HttpClient _http;
		private readonly ILogger<GeminiService> _logger;
		private readonly string? _apiKey;

		private static readonly string[] Categories = { "Dessert", "Dinner", "Lunch", "Snack", "Breakfast", "Drink" };
		private static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

		public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
		{
			_http = http;
			_logger = logger;
			_apiKey = config["Gemini:ApiKey"];
		}

		/// <summary>
		/// Generates a recipe from the submitted inputs and returns a fallback recipe if Gemini cannot provide valid JSON.
		/// </summary>
		public async Task<GeneratedRecipeResultDTO> GenerateRecipeAsync(string ingredients, string category, string difficulty)
		{
			try
			{
				Random random = new Random();
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    return CreateFallbackRecipe("Gemini API key is missing.");
                }
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
						responseMimeType = "application/json"
					}
				};

				//Create HTTP request
				string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
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

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini request failed. StatusCode: {StatusCode}, ResponseBody: {ResponseBody}", (int)response.StatusCode, responseContent);
                    return CreateFallbackRecipe($"API Error {(int)response.StatusCode}: {responseContent}");
                }

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
				_logger.LogInformation("Gemini generated recipe content: {Content}", content);
				content = CleanGeneratedJson(content);

                var generatedRecipe = JsonSerializer.Deserialize<GeneratedRecipeResultDTO>(content)
                      ?? new GeneratedRecipeResultDTO();

                generatedRecipe.Difficulty = difficulty;
                generatedRecipe.Category = category;

                return generatedRecipe ?? CreateFallbackRecipe("Failed to parse recipe");
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Gemini HTTP request failed.");
				return CreateFallbackRecipe($"API Error: {ex.Message}");
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Gemini JSON parse failed.");
				return CreateFallbackRecipe($"Parse Error: {ex.Message}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Gemini recipe generation failed.");
				return CreateFallbackRecipe($"Error: {ex.Message}");
			}
		}

		/// <summary>
		/// Removes Markdown code fences and extracts the JSON object from a model response.
		/// </summary>
		private string CleanGeneratedJson(string content)
		{
			var cleaned = content.Trim();

			if (cleaned.StartsWith("```"))
			{
				var firstNewLine = cleaned.IndexOf('\n');
				if (firstNewLine >= 0)
					cleaned = cleaned.Substring(firstNewLine + 1).Trim();

				if (cleaned.EndsWith("```"))
					cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
			}

			var jsonStart = cleaned.IndexOf('{');
			var jsonEnd = cleaned.LastIndexOf('}');
			if (jsonStart >= 0 && jsonEnd > jsonStart)
				return cleaned.Substring(jsonStart, jsonEnd - jsonStart + 1);

			_logger.LogError("Gemini response did not contain a JSON object. Content: {Content}", content);
			return cleaned;
		}

		/// <summary>
		/// Creates a placeholder recipe with the error message attached to the instructions.
		/// </summary>
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
