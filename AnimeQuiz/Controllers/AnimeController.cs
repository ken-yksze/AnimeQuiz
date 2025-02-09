using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimeController : ControllerBase
    {
        private readonly IAnimeService _animeService;

        // dependency injection of service interfaces
        public AnimeController(IAnimeService animeService)
        {
            _animeService = animeService;
        }

        /// <summary>
        /// Returns a list of Animes
        /// </summary>
        /// <returns>
        /// 200 OK
        /// [ {AnimeDto}, {AnimeDto}, ... ]
        /// </returns>
        /// <example>
        /// GET: api/Anime -> [ {AnimeDto}, {AnimeDto}, ... ]
        /// </example>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnimeDto>>> ListAnimes()
        {
            IEnumerable<AnimeDto> animeDtos = await _animeService.ListAnimes();
            return Ok(animeDtos);
        }

        /// <summary>
        /// Return a single Anime specified by its {id}
        /// </summary>
        /// <param name="id">The Anime id</param>
        /// <returns>
        /// 200 OK
        /// {AnimeDto}
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// GET api/Anime/1 -> {AnimeDto}
        /// </example>
        [HttpGet(template: "{id}")]
        public async Task<ActionResult<AnimeDto>> FindAnime(int id)
        {
            AnimeDto? animeDto = await _animeService.FindAnime(id);
            return animeDto == null ? NotFound($"Anime with id {id} not found.") : Ok(animeDto);
        }

        /// <summary>
        /// Update an Anime
        /// </summary>
        /// <param name="id">The id of the Anime to update</param>
        /// <param name="request">The required information to update the Anime (AnimeName)</param>
        /// <returns>
        /// 204 No Content
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 409 Conflict
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// PUT: api/Anime/1
        /// Request Header: { "Content-Type": "application/json" }
        /// Request Body: {UpdateAnimeRequest}
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpPut(template: "{id}")]
        [Consumes("application/json")]
        public async Task<ActionResult> UpdateAnime(int id, [FromBody] UpdateAnimeRequest request)
        {
            ServiceResponse response = await _animeService.UpdateAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Conflict:
                    return Conflict(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Updated
                    return NoContent();
            }
        }

        /// <summary>
        /// Adds an Anime
        /// </summary>
        /// <param name="request">The required information to add the Anime (AnimeName, ImageFiles)</param>
        /// <returns>
        /// 201 Created
        /// Location: api/Anime/{AnimeId}
        /// {AnimeDto}
        /// or
        /// 400 Bad Request
        /// or
        /// 409 Conflict
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Anime
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddAnimeRequest}
        /// ->
        /// Response Code: 201 Created
        /// Response Headers: Location: api/Anime/{AnimeId}
        /// </example>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddAnime([FromForm] AddAnimeRequest request)
        {
            (ServiceResponse response, AnimeDto? animeDto) = await _animeService.AddAnime(request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.Conflict:
                    return Conflict(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Created
                    return Created($"api/Anime/{animeDto?.AnimeId}", animeDto);
            }
        }

        /// <summary>
        /// Deletes the Anime
        /// </summary>
        /// <param name="id">The id of the Anime to delete</param>
        /// <returns>
        /// 204 No Content
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Anime/1
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpDelete(template: "{id}")]
        public async Task<ActionResult> DeleteAnime(int id)
        {
            ServiceResponse response = await _animeService.DeleteAnime(id);

            switch (response.Status)
            {
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Deleted
                    return NoContent();
            }
        }

        /// <summary>
        /// Add CharacterVersions to an Anime
        /// </summary>
        /// <param name="id">The id of the Anime</param>
        /// <param name="request">The request object, including a list of CharacterVersion Ids (int)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Anime/1/CharacterVersion
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { CharacterVersionIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/CharacterVersion")]
        [Consumes("application/json")]
        public async Task<ActionResult> AddCharacterVersionsToAnime(int id, [FromBody] AddCharacterVersionsToAnimeRequest request)
        {
            ServiceResponse response = await _animeService.AddCharacterVersionsToAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Created
                    return Ok(response.Messages);
            }
        }

        /// <summary>
        /// Remove CharacterVersions from an Anime
        /// </summary>
        /// <param name="id">The id of the Anime</param>
        /// <param name="request">The request object, including a list of CharacterVersion Ids (int)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Anime/1/CharacterVersion
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { CharacterVersionIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/CharacterVersion")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveCharacterVersionsFromAnime(int id, [FromBody] RemoveCharacterVersionsFromAnimeRequest request)
        {
            ServiceResponse response = await _animeService.RemoveCharacterVersionsFromAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Deleted
                    return Ok(response.Messages);
            }
        }

        /// <summary>
        /// Adds Images to an Anime
        /// </summary>
        /// <param name="request">The required information to add Images to an Anime (ImageFiles)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Anime/1/Image
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddImagesToAnimeRequest}
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/Image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddImagesToAnime(int id, [FromForm] AddImagesToAnimeRequest request)
        {
            ServiceResponse response = await _animeService.AddImagesToAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Created
                    return Ok(response.Messages);
            }
        }

        /// <summary>
        /// Remove Images from an Anime
        /// </summary>
        /// <param name="id">The id of the Anime</param>
        /// <param name="request">The request object, including a list of Image Ids (int)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Anime/1/Image
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { ImageIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/Image")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveImagesFromAnime(int id, [FromBody] RemoveImagesFromAnimeRequest request)
        {
            ServiceResponse response = await _animeService.RemoveImagesFromAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Deleted
                    return Ok(response.Messages);
            }
        }

        /// <summary>
        /// Adds Musics to an Anime
        /// </summary>
        /// <param name="request">The required information to add Musics to an Anime (MusicFiles)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Anime/1/Music
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddMusicsToAnimeRequest}
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/Music")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddMusicsToAnime(int id, [FromForm] AddMusicsToAnimeRequest request)
        {
            ServiceResponse response = await _animeService.AddMusicsToAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Created
                    return Ok(response.Messages);
            }
        }

        /// <summary>
        /// Remove Musics from an Anime
        /// </summary>
        /// <param name="id">The id of the Anime</param>
        /// <param name="request">The request object, including a list of Music Ids (int)</param>
        /// <returns>
        /// 200 OK
        /// [string, ...] Including a message for no. of affected records
        /// or
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Anime/1/Music
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { MusicIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/Music")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveMusicsFromAnime(int id, [FromBody] RemoveMusicsFromAnimeRequest request)
        {
            ServiceResponse response = await _animeService.RemoveMusicsFromAnime(id, request);

            switch (response.Status)
            {
                case ServiceStatus.NotFound:
                    return NotFound(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Deleted
                    return Ok(response.Messages);
            }
        }
    }
}
