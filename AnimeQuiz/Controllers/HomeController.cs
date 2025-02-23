using AnimeQuiz.Interfaces;
using AnimeQuiz.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnimeQuiz.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAnimeQuizService _animeQuizService;

        public HomeController(ILogger<HomeController> logger, IAnimeQuizService animeQuizService)
        {
            _logger = logger;
            _animeQuizService = animeQuizService;
        }

        public async Task<IActionResult> Index()
        {
            int totalAvailable = await _animeQuizService.GetTotalAvailable();
            return View(totalAvailable);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
