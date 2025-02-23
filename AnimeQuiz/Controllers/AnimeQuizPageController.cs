using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using AnimeQuiz.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("AnimeQuiz")]
    public class AnimeQuizPageController : Controller
    {
        private readonly IAnimeQuizService _animeQuizService;

        // dependency injection of service interfaces
        public AnimeQuizPageController(IAnimeQuizService animeQuizService)
        {
            _animeQuizService = animeQuizService;
        }

        // GET: AnimeQuiz?numOfQuestions=8
        [HttpGet]
        public async Task<IActionResult> Index(int numOfQuestions = 8)
        {
            (ServiceResponse response, AnimeQuizDto? animeQuizDto) = await _animeQuizService.GenerateAnimeQuiz(numOfQuestions);

            return (response.Status == ServiceStatus.Ok && animeQuizDto != null)
                ? View(animeQuizDto)
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // GET: AnimeQuiz/Result?score=8&total=8
        [HttpGet("Result")]
        public IActionResult Result(int? score = null, int? total = null)
        {
            return (score != null && score >= 0 && total != null && total >= 0 && score <= total)
                ? View()
                : View("Error", new ErrorViewModel() { Errors = ["No valid score and total provided."] });
        }
    }
}
