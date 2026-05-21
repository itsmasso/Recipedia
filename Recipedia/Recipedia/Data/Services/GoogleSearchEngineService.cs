using System.Text.Json;
using System.Text.Json.Serialization;
using Recipedia.Models;

namespace Recipedia.Data.Services
{
    public class GoogleSearchEngineService
    {
        // Services and config
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleSearchEngineService> _logger;
        private readonly string? _apiKey;
        private readonly string? _cseId;

        public GoogleSearchEngineService(HttpClient httpClient, IConfiguration config, ILogger<GoogleSearchEngineService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["GoogleCSE:ApiKey"];
            _cseId = config["GoogleCSE:CseId"];
        }

        /// <summary>
        /// Searches Google Custom Search for recipe pages that match the verified ingredients.
        /// </summary>
        public async Task<List<WebRecipeResultDTO>> SearchRecipesAsync(string ingredients)
        {
            var apiKey = _apiKey?.Trim();
            var cseId = _cseId?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(cseId))
            {
                _logger.LogError(
                    "Google CSE configuration is missing. ApiKeyConfigured: {ApiKeyConfigured}, CseIdConfigured: {CseIdConfigured}",
                    !string.IsNullOrWhiteSpace(apiKey),
                    !string.IsNullOrWhiteSpace(cseId));

                return new List<WebRecipeResultDTO>();
            }

            string query = $"{ingredients} recipe";

            string url = $"https://www.googleapis.com/customsearch/v1?key={Uri.EscapeDataString(apiKey)}&cx={Uri.EscapeDataString(cseId)}&q={Uri.EscapeDataString(query)}&num=10";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Google CSE request failed. StatusCode: {StatusCode}, ApiKeyConfigured: {ApiKeyConfigured}, CseIdConfigured: {CseIdConfigured}, CseIdLength: {CseIdLength}, ResponseBody: {ResponseBody}",
                    (int)response.StatusCode,
                    !string.IsNullOrWhiteSpace(apiKey),
                    !string.IsNullOrWhiteSpace(cseId),
                    cseId.Length,
                    json);

                return new List<WebRecipeResultDTO>();
            }

            var googleResponse = JsonSerializer.Deserialize<GoogleCseResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var results = googleResponse?.Items?.Select(x => new WebRecipeResultDTO
            {            
                Title = x.Title ?? "",
                Url = x.Link ?? "",
                ImageUrl = GetOgImage(x),
                Source = GetDomainName(x.Link ?? "")

            }).ToList() ?? new List<WebRecipeResultDTO>();
      

            return results;
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

        /// <summary>
        /// Selects the best available preview image from the Google result metadata.
        /// </summary>
        private string GetOgImage(GoogleCseItem item)
        {
            var pagemap = item?.Pagemap;
            if (pagemap == null)
                return "";

            string? imageUrl =
                pagemap.Metatags?.FirstOrDefault()?.OgImage ??
                pagemap.Metatags?.FirstOrDefault()?.TwitterImage ??
                pagemap.Metatags?.FirstOrDefault()?.TwitterImageSrc ??
                pagemap.Metatags?.FirstOrDefault()?.ThumbnailUrl ??
                pagemap.CseImage?.FirstOrDefault()?.Src ??
                pagemap.CseThumbnail?.FirstOrDefault()?.Src;

            return imageUrl ?? "";
        }
    }
}

    public class GoogleCseResponse
    {
        public List<GoogleCseItem>? Items { get; set; }
    }
    public class GoogleCseItem
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public GooglePageMap? Pagemap { get; set; }
    }
    public class GooglePageMap
    {
        public List<GoogleMetaTag>? Metatags { get; set; }
        public List<GoogleImageObject>? CseImage { get; set; }
        public List<GoogleImageObject>? CseThumbnail { get; set; }
    }

    public class GoogleMetaTag
    {
        [JsonPropertyName("og:image")]
        public string? OgImage { get; set; }

        [JsonPropertyName("twitter:image")]
        public string? TwitterImage { get; set; }

        [JsonPropertyName("twitter:image:src")]
        public string? TwitterImageSrc { get; set; }

        [JsonPropertyName("thumbnailurl")]
        public string? ThumbnailUrl { get; set; }


    }

    public class GoogleImageObject
    {
        public string? Src { get; set; }
    }


