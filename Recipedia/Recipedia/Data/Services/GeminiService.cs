using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Recipedia.Data.Services
{
	public class GeminiService : IGeminiService
	{
		private readonly HttpClient _http;
		private readonly string _apiKey;
		private readonly string _baseUrl;

		private static readonly string[] Categories = { "Dessert, Dinner, Lunch, Snack, Breakfast, Drink" };
		private static readonly string[] Difficulties = { "Easy, Medium, Hard" };

		public GeminiService(HttpClient http, IConfiguration config)
		{
			_http = http;
			_apiKey = config["Gemini:ApiKey"];
			_baseUrl = config["Gemini:BaseUrl"];

		}

		public async Task<string> GenerateRecipeAsync(string ingredients, string category, string difficulty)
		{
			Random random = new Random();
			var categoryValue = category ?? Categories[random.Next(Categories.Length)];
			var difficultyValue = difficulty ?? Difficulties[random.Next(Difficulties.Length)];

			var prompt = $"Generate a recipe with a title, ingredients, instructions, cook time. " +
						$"Ingredients: {ingredients}." +
						$"Category: {categoryValue}" +
						$"Category: {difficultyValue}";

			var requestBody = new
			{
				model = "gemini-2.0-flash",
				messages = new[]
				{
					new { role = "system", content = "You are a recipe generator." },
					new { role = "user", content = prompt }
				}
				//add other paramters here
			};
			var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}chat/completions");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
			httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

			var response = await _http.SendAsync(httpRequest);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			// parse out the content from response JSON
			using var doc = JsonDocument.Parse(json);
			var content = doc.RootElement
				.GetProperty("choices")[0]
				.GetProperty("message")
				.GetProperty("content")
				.GetString();

			return content ?? "No Content";

		}
		
	}
}
