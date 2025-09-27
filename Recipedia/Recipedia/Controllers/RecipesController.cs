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
		private readonly GeminiService _geminiService;
		public RecipesController(RecipediaAppContext context, GeminiService geminiService)
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

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Recipe recipe)
		{
			if (ModelState.IsValid)
			{
				_context.Recipes.Add(recipe);
				_context.SaveChanges();
				return RedirectToAction(nameof(Index)); //re direct user back to recipes list
			}
			return View(recipe);
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
			

			return View();
		}

	}
}
