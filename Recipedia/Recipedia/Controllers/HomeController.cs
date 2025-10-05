using Microsoft.AspNetCore.Mvc;
using Recipedia.Data.Services;
using Recipedia.Models;
using System.Diagnostics;

namespace Recipedia.Controllers
{
	public class HomeController : Controller
	{
		private readonly GoogleSearchEngineService _googleCSEService;
		public HomeController(GoogleSearchEngineService googleCSEService)
		{
			_googleCSEService = googleCSEService;
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

			var recipes = await _googleCSEService.SearchRecipesAsync(ingredients);
         
            return View(nameof(Index), recipes);
		}

	}
}
