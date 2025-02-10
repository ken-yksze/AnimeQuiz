using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimeQuizController : ControllerBase
    {
        private readonly IAnimeQuizService _animeQuizService;

        // dependency injection of service interfaces
        public AnimeQuizController(IAnimeQuizService animeQuizService)
        {
            _animeQuizService = animeQuizService;
        }

        /// <summary>
        /// Generate anime quiz
        /// </summary>
        /// <param name="numOfQuestions">The number of question generated</param>
        /// <returns>
        /// 200 OK
        /// {AnimeQuizDto}
        /// 404 Bad Request
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// GET: api/AnimeQuiz?numOfQuestions=32 -> {AnimeQuizDto}
        /// </example>
        [HttpGet]
        public async Task<ActionResult<AnimeQuizDto>> GenerateAnimeQuiz([Range(2, 512, ErrorMessage = "Number of questions should be within 2 to 512.")] int numOfQuestions = 8)
        {
            (ServiceResponse response, AnimeQuizDto? animeQuizDto) = await _animeQuizService.GenerateAnimeQuiz(numOfQuestions);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    return Ok(animeQuizDto);
            }
        }
    }
}
