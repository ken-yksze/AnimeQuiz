using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using AnimeQuiz.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("Music")]
    public class MusicPageController : Controller
    {
        private readonly IMusicService _musicService;

        // dependency injection of service interfaces
        public MusicPageController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        // GET: Music/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            MusicDto? musicDto = await _musicService.FindMusic(id);

            return musicDto != null
                ? View(musicDto)
                : View("Error", new ErrorViewModel() { Errors = ["Music not found."] }); ;
        }

        // GET: Music/{id}/NewSingers
        [HttpGet("{id}/NewSingers")]
        public async Task<IActionResult> NewSingers(int id)
        {
            MusicDto? musicDto = await _musicService.FindMusic(id);
            IEnumerable<StaffDto> staffDtos = await _musicService.FindAvailableSingersForMusic(id);

            return musicDto != null
                ? View(new MusicAvailableSinger { MusicDto = musicDto, StaffDtos = staffDtos })
                : View("Error", new ErrorViewModel() { Errors = ["Music not found."] }); ;
        }

        // POST: Music/{id}/AddSingers
        [HttpPost("{id}/AddSingers")]
        public async Task<IActionResult> AddSingers(int id, [FromForm] AddSingersToMusicRequest request)
        {
            ServiceResponse response = await _musicService.AddSingersToMusic(id, request);

            return response.Status == ServiceStatus.Created
                ? RedirectToAction("Details", "MusicPage", new { id = id })
                : PartialView("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // POST: Music/{id}/RemoveSingers
        [HttpPost("{id}/RemoveSingers")]
        public async Task<IActionResult> RemoveSingers(int id, [FromForm] RemoveSingersFromMusicRequest request)
        {
            ServiceResponse response = await _musicService.RemoveSingersFromMusic(id, request);

            return response.Status == ServiceStatus.Deleted
                ? RedirectToAction("Details", "MusicPage", new { id = id })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }
    }
}
