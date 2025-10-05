using Microsoft.AspNetCore.Mvc;
using Recipedia.Data;
using Recipedia.Models;
using Microsoft.EntityFrameworkCore;
using Recipedia.Data.Services;
namespace Recipedia.Controllers
{
	public class RecipesController : Controller
	{
		private readonly RecipediaAppContext _context;
		private readonly IGeminiService _geminiService;
		public RecipesController(RecipediaAppContext context, IGeminiService geminiService)
		{
			_context = context;
			_geminiService = geminiService;
		}

		// GET: /Recipes
		public IActionResult Index()
		{
			var recipes = _context.Recipes.ToList();
			return View(recipes);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteRecipe(int id)
		{
			var recipe = await _context.Recipes.FindAsync(id);
			if(recipe != null)
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

            return View("GeneratedRecipeResult", recipe);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SaveRecipe(GeneratedRecipeResultDTO generatedRecipe)
		{
			if (ModelState.IsValid)
			{
                //Map DTO to Entity
                var recipe = new Recipe
                {
                    Title = generatedRecipe.Title,
                    Ingredients = string.Join(", ", generatedRecipe.Ingredients ?? new List<string>()),
                    Instructions = string.Join("\n", generatedRecipe.Instructions ?? new List<string>()),
                    Category = generatedRecipe.Category,
                    CookTimeMinutes = generatedRecipe.CookTimeMinutes,
                    Difficulty = generatedRecipe.Difficulty,
					//add default image here
                };

                _context.Recipes.Add(recipe);
                _context.SaveChanges();
                
            }
            return RedirectToAction(nameof(Index)); //re direct user back to recipes list

        }
	


	}
}
