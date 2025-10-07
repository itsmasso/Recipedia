using Microsoft.AspNetCore.Mvc;
using Recipedia.Data;
using Recipedia.Data.Services;
using Recipedia.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Recipedia.Controllers
{
	public class HomeController : Controller
	{
		private readonly GoogleSearchEngineService _googleCSEService;
        private readonly SpoonacularService _spoonacularService;
        private readonly RecipediaAppContext _context;
        public HomeController(GoogleSearchEngineService googleCSEService, SpoonacularService spoonacularService, RecipediaAppContext context)
		{
			_googleCSEService = googleCSEService;
            _spoonacularService = spoonacularService;
            _context = context;
        }

		public IActionResult Index()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Search(string ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredients))
                return View(nameof(Index), new List<WebRecipeResultDTO>());

            var allIngredients = ingredients
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .ToList();

            var verifiedIngredients = new List<string>();

            foreach (var ing in allIngredients)
            {
                if (await _spoonacularService.IsValidIngredientAsync(ing))
                    verifiedIngredients.Add(ing);
                else
                    Debug.WriteLine($"Omitting unverified ingredient: {ing}");
            }

            if (!verifiedIngredients.Any())
            {
                Debug.WriteLine("No verified ingredients. Returning empty results.");
                return View(nameof(Index), new List<WebRecipeResultDTO>());
            }

            ViewBag.VerifiedIngredients = verifiedIngredients;

            var recipes = await _googleCSEService.SearchRecipesAsync(string.Join(",", verifiedIngredients));

            var savedUrls = _context.Recipes.Select(r => r.Url).ToList();
            ViewBag.SavedUrls = savedUrls;

            return View(nameof(Index), recipes);
        }


    }
}
