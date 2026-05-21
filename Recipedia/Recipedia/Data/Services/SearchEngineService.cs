using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Recipedia.Models;

namespace Recipedia.Data.Services
{
    public class SearchEngineService
    {
        // Services and config
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchEngineService> _logger;
        private readonly string? _apiKey;

        public SearchEngineService(HttpClient httpClient, IConfiguration config, ILogger<SearchEngineService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["Tavily:ApiKey"];
        }

        /// <summary>
        /// Searches Google Custom Search for recipe pages that match the verified ingredients.
        /// </summary>
        public async Task<List<WebRecipeResultDTO>> SearchRecipesAsync(string ingredients)
        {
            var apiKey = _apiKey?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Tavily API key is missing.");
                return new List<WebRecipeResultDTO>();
            }

            var requestBody = new TavilySearchRequest
            {
                Query = $"{ingredients} recipe",
                SearchDepth = "basic",
                Topic = "general",
                MaxResults = 10,
                IncludeImages = true,
                IncludeImageDescriptions = false,
                IncludeAnswer = false,
                IncludeRawContent = false,
                IncludeFavicon = false,
                Country = "united states"
            };


            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.tavily.com/search");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Tavily search failed. StatusCode: {StatusCode}, ResponseBody: {ResponseBody}",
                    (int)response.StatusCode,
                    json);

                return new List<WebRecipeResultDTO>();
            }

            var tavilyResponse = JsonSerializer.Deserialize<TavilySearchResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            return tavilyResponse?.Results?
                .Where(result => !string.IsNullOrWhiteSpace(result.Url))
                .Select(result => new WebRecipeResultDTO
                {
                    Title = result.Title ?? "",
                    Url = result.Url ?? "",
                    ImageUrl = result.Images?.FirstOrDefault() ?? result.Favicon ?? "",
                    Source = GetDomainName(result.Url ?? "")
                })
                .ToList() ?? new List<WebRecipeResultDTO>();
        }

        private string GetDomainName(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host;

                //Remove 'www.' if present
                if (host.StartsWith("www."))
                    host = host.Substring(4);

                return host;
            }
            catch
            {
                return url;
            }
        }
    }
}

public class TavilySearchRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = "";

    [JsonPropertyName("search_depth")]
    public string SearchDepth { get; set; } = "basic";

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "general";

    [JsonPropertyName("max_results")]
    public int MaxResults { get; set; } = 10;

    [JsonPropertyName("include_images")]
    public bool IncludeImages { get; set; }

    [JsonPropertyName("include_image_descriptions")]
    public bool IncludeImageDescriptions { get; set; }

    [JsonPropertyName("include_answer")]
    public bool IncludeAnswer { get; set; }

    [JsonPropertyName("include_raw_content")]
    public bool IncludeRawContent { get; set; }

    [JsonPropertyName("include_favicon")]
    public bool IncludeFavicon { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

public class TavilySearchResponse
{
    [JsonPropertyName("results")]
    public List<TavilyResult>? Results { get; set; }
}
public class TavilyResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }

    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}
