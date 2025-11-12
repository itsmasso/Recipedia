using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipedia.Data;
using Recipedia.Data.Services;
using Recipedia.Models;
using Rotativa.AspNetCore;
using System.Text.Json;

namespace Recipedia.Controllers
{
	public class RecipesController : Controller
	{
		private readonly RecipediaAppContext _context;
		private readonly IGeminiService _geminiService;
        private readonly UserManager<User> _userManager;
        public RecipesController(RecipediaAppContext context, IGeminiService geminiService, UserManager<User> userManager)
        {
            _context = context;
            _geminiService = geminiService;
            _userManager = userManager;
        }

        // shows the current recipes the user had saved
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(recipes);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecipe([FromBody] WebRecipeResultDTO webRecipeResult)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Please sign in to save recipes", redirectToLogin = true });
            }
            if (webRecipeResult != null)
            {
                var userId = _userManager.GetUserId(User);
                var existingRecipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Url == webRecipeResult.Url && r.UserId == userId);

                if (existingRecipe == null)
                {
                    var recipe = new Recipe
                    {
                        Title = webRecipeResult.Title,
                        Url = webRecipeResult.Url,
                        ImageUrl = webRecipeResult.ImageUrl,
                        Source = webRecipeResult.Source,
                        UserId = userId
                    };
                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Recipe saved successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "You've already saved this recipe" });
                }
            }

            return Json(new { success = false, message = "Invalid recipe data" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = _userManager.GetUserId(User);
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RecipeDetails(int? id)
		{
			if(id == null)
			{
				return NotFound();
			}

			var recipe = await _context.Recipes.FirstOrDefaultAsync(recipe => recipe.Id == id);
			if(recipe == null)
			{
				return NotFound();
			}

			return View(recipe);
		}
		
		public IActionResult GenerateRecipe()
		{
			return View();
		}
	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GenerateRecipe(GenerateRecipeInputModel input)
		{
            if (!ModelState.IsValid)
			{
				return View(input);
			}

			//call and fetch gemini
			var recipe = await _geminiService.GenerateRecipeAsync(input.IngredientsTags, input.Category, input.Difficulty);
            if (recipe == null)
            {
                ViewBag.Error = "Failed to generate recipe. Please try again.";
                return View();
            }
			TempData["RecipeForPdf"] = JsonSerializer.Serialize(recipe);
            return View("GeneratedRecipeResult", recipe);
		}

        [HttpGet]
        public IActionResult DownloadRecipePdf()
        {
            if (TempData["RecipeForPdf"] is string json)
            {
                var recipe = JsonSerializer.Deserialize<GeneratedRecipeResultDTO>(json);
                TempData.Keep("RecipeForPdf");

                var title = recipe?.Title ?? "recipe";
                var fileName = $"{SanitizeFileName(title)}.pdf";

                return new ViewAsPdf("GeneratedRecipeResultPdf", recipe)
                {
                    FileName = fileName,
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                    CustomSwitches = "--enable-local-file-access --disable-smart-shrinking --print-media-type"
                };
            }

            return RedirectToAction("Index");
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "recipe";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            return string.IsNullOrWhiteSpace(sanitized) ? "recipe" : sanitized;
        }


    }
}
