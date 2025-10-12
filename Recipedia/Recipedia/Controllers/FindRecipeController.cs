using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipedia.Data;
using Recipedia.Data.Services;
using Recipedia.Models;
using System.Diagnostics;

namespace Recipedia.Controllers
{
    public class FindRecipeController : Controller
    {
        private readonly GoogleSearchEngineService _googleCSEService;
        private readonly SpoonacularService _spoonacularService;
        private readonly RecipediaAppContext _context;
        private readonly UserManager<User> _userManager;

        public FindRecipeController(
            GoogleSearchEngineService googleCSEService,
            SpoonacularService spoonacularService,
            RecipediaAppContext context,
            UserManager<User> userManager)
        {
            _googleCSEService = googleCSEService;
            _spoonacularService = spoonacularService;
            _context = context;
            _userManager = userManager;
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

            //Get saved URLs for the current user only
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                var savedUrls = await _context.Recipes
                    .Where(r => r.UserId == userId)
                    .Select(r => r.Url)
                    .ToListAsync();

                ViewBag.SavedUrls = savedUrls;
            }
            else
            {
                //Not logged in - no saved recipes
                ViewBag.SavedUrls = new List<string>();
            }

            return View(nameof(Index), recipes);
        }
    }
}