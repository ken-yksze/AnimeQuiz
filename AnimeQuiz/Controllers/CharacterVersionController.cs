using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterVersionController : ControllerBase
    {
        private readonly ICharacterVersionService _characterVersionService;

        // dependency injection of service interfaces
        public CharacterVersionController(ICharacterVersionService characterVersionService)
        {
            _characterVersionService = characterVersionService;

        }

        /// <summary>
        /// Return a single CharacterVersion specified by its {id}
        /// </summary>
        /// <param name="id">The CharacterVersion id</param>
        /// <returns>
        /// 200 OK
        /// {CharacterVersionDto}
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// GET api/CharacterVersion/1 -> {CharacterVersionDto}
        /// </example>
        [HttpGet(template: "{id}")]
        public async Task<ActionResult<CharacterVersionDto>> FindCharacterVersion(int id)
        {
            CharacterVersionDto? characterVersionDto = await _characterVersionService.FindCharacterVersion(id);
            return characterVersionDto == null ? NotFound($"CharacterVersion with id {id} not found.") : Ok(characterVersionDto);
        }

        /// <summary>
        /// Update a CharacterVersion
        /// </summary>
        /// <param name="id">The id of the CharacterVersion to update</param>
        /// <param name="request">The required information to update the CharacterVersion (VersionName)</param>
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
        /// PUT: api/CharacterVersion/1
        /// Request Header: { "Content-Type": "application/json" }
        /// Request Body: {UpdateCharacterVersionRequest}
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpPut(template: "{id}")]
        [Consumes("application/json")]
        public async Task<ActionResult> UpdateCharacterVersion(int id, [FromBody] UpdateCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.UpdateCharacterVersion(id, request);

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
        /// Adds Images to an CharacterVersion
        /// </summary>
        /// <param name="request">The required information to add Images to an CharacterVersion (ImageFiles)</param>
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
        /// POST: api/CharacterVersion/1/Image
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddImagesToCharacterVersionRequest}
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/Image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddImagesToCharacterVersion(int id, [FromForm] AddImagesToCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.AddImagesToCharacterVersion(id, request);

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
        /// Remove Images from an CharacterVersion
        /// </summary>
        /// <param name="id">The id of the CharacterVersion</param>
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
        /// DELETE: api/CharacterVersion/1/Image
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { ImageIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/Image")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveImagesFromCharacterVersion(int id, [FromBody] RemoveImagesFromCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.RemoveImagesFromCharacterVersion(id, request);

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
        /// Add VoiceActors to a CharacterVersion
        /// </summary>
        /// <param name="id">The id of the CharacterVersion</param>
        /// <param name="request">The request object, including a list of VoiceActor Ids (int)</param>
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
        /// POST: api/CharacterVersion/1/VoiceActor
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { VoiceActorIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/VoiceActor")]
        [Consumes("application/json")]
        public async Task<ActionResult> AddVoiceActorsToCharacterVersion(int id, [FromBody] AddVoiceActorsToCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.AddVoiceActorsToCharacterVersion(id, request);

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
        /// Remove VoiceActors from a CharacterVersion
        /// </summary>
        /// <param name="id">The id of the CharacterVersion</param>
        /// <param name="request">The request object, including a list of VoiceActor Ids (int)</param>
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
        /// DELETE: api/CharacterVersion/1/VoiceActor
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { VoiceActorIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/VoiceActor")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveVoiceActorsFromCharacterVersion(int id, [FromBody] RemoveVoiceActorsFromCharacterVersionRequest request)
        {
            ServiceResponse response = await _characterVersionService.RemoveVoiceActorsFromCharacterVersion(id, request);

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
    }
}
