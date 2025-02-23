using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using AnimeQuiz.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("CharacterVersion")]
    public class CharacterVersionPageController : Controller
    {
        private readonly ICharacterVersionService _characterVersionService;

        // dependency injection of service interfaces
        public CharacterVersionPageController(ICharacterVersionService characterVersionService)
        {
            _characterVersionService = characterVersionService;
        }

        // GET: CharacterVersion/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            CharacterVersionDto? characterVersionDto = await _characterVersionService.FindCharacterVersion(id);

            return characterVersionDto != null
                ? View(characterVersionDto)
                : View("Error", new ErrorViewModel() { Errors = ["CharacterVersion not found."] }); ;
        }

        // GET: CharacterVersion/{id}/Edit
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            CharacterVersionDto? characterVersionDto = await _characterVersionService.FindCharacterVersion(id);

            return characterVersionDto != null
                ? View(characterVersionDto)
                : View("Error", new ErrorViewModel() { Errors = ["CharacterVersion not found."] }); ;
        }

        // POST: CharacterVersion/{id}/Update
        [HttpPost("{id}/Update")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.UpdateCharacterVersion(id, request);

            return response.Status == ServiceStatus.Updated
                ? RedirectToAction("Details", "CharacterVersionPage", new { id = id })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // GET: CharacterVersion/{id}/NewImages
        [HttpGet("{id}/NewImages")]
        public async Task<IActionResult> NewImages(int id)
        {
            CharacterVersionDto? characterVersionDto = await _characterVersionService.FindCharacterVersion(id);

            return characterVersionDto != null
                ? View(characterVersionDto)
                : View("Error", new ErrorViewModel() { Errors = ["CharacterVersion not found."] });
        }

        // POST: CharacterVersion/{id}/AddImages
        [HttpPost("{id}/AddImages")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImages(int id, [FromForm] AddImagesToCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.AddImagesToCharacterVersion(id, request);

            return response.Status == ServiceStatus.Created
                ? RedirectToAction("Details", "CharacterVersionPage", new { id = id })
                : PartialView("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // POST: CharacterVersion/{id}/RemoveImages
        [HttpPost("{id}/RemoveImages")]
        public async Task<IActionResult> RemoveImages(int id, [FromForm] RemoveImagesFromCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.RemoveImagesFromCharacterVersion(id, request);

            return response.Status == ServiceStatus.Deleted
                ? RedirectToAction("Details", "CharacterVersionPage", new { id = id })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // GET: CharacterVersion/{id}/NewVoiceActors
        [HttpGet("{id}/NewVoiceActors")]
        public async Task<IActionResult> NewVoiceActors(int id)
        {
            CharacterVersionDto? characterVersionDto = await _characterVersionService.FindCharacterVersion(id);
            IEnumerable<StaffDto> staffDtos = await _characterVersionService.FindAvailableVoiceActorsForCharacter(id);

            return characterVersionDto != null
                ? View(new CharacterAvailableVoiceActor { CharacterVersionDto = characterVersionDto, StaffDtos = staffDtos })
                : View("Error", new ErrorViewModel() { Errors = ["CharacterVersion not found."] }); ;
        }

        // POST: CharacterVersion/{id}/AddVoiceActors
        [HttpPost("{id}/AddVoiceActors")]
        public async Task<IActionResult> AddVoiceActors(int id, [FromForm] AddVoiceActorsToCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.AddVoiceActorsToCharacterVersion(id, request);

            return response.Status == ServiceStatus.Created
                ? RedirectToAction("Details", "CharacterVersionPage", new { id = id })
                : PartialView("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // POST: CharacterVersion/{id}/RemoveVoiceActors
        [HttpPost("{id}/RemoveVoiceActors")]
        public async Task<IActionResult> RemoveVoiceActors(int id, [FromForm] RemoveVoiceActorsFromCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.RemoveVoiceActorsFromCharacterVersion(id, request);

            return response.Status == ServiceStatus.Deleted
                ? RedirectToAction("Details", "CharacterVersionPage", new { id = id })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }
    }
}
