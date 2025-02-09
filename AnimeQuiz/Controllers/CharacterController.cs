using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        // dependency injection of service interfaces
        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        /// <summary>
        /// Returns a list of Characters
        /// </summary>
        /// <returns>
        /// 200 OK
        /// [ {CharacterDto}, {CharacterDto}, ... ]
        /// </returns>
        /// <example>
        /// GET: api/Character -> [ {CharacterDto}, {CharacterDto}, ... ]
        /// </example>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacterDto>>> ListCharacters()
        {
            IEnumerable<CharacterDto> characterDtos = await _characterService.ListCharacters();
            return Ok(characterDtos);
        }

        /// <summary>
        /// Return a single Character specified by its {id}
        /// </summary>
        /// <param name="id">The Character id</param>
        /// <returns>
        /// 200 OK
        /// {CharacterDto}
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// GET api/Character/1 -> {CharacterDto}
        /// </example>
        [HttpGet(template: "{id}")]
        public async Task<ActionResult<CharacterDto>> FindCharacter(int id)
        {
            CharacterDto? characterDto = await _characterService.FindCharacter(id);
            return characterDto == null ? NotFound($"Character with id {id} not found.") : Ok(characterDto);
        }

        /// <summary>
        /// Update a Character
        /// </summary>
        /// <param name="id">The id of the Character to update</param>
        /// <param name="request">The required information to update the Character (CharacterName)</param>
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
        /// PUT: api/Character/1
        /// Request Header: { "Content-Type": "application/json" }
        /// Request Body: {UpdateCharacterRequest}
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpPut(template: "{id}")]
        [Consumes("application/json")]
        public async Task<ActionResult> UpdateCharacter(int id, [FromBody] UpdateCharacterRequest request)
        {
            ServiceResponse response = await _characterService.UpdateCharacter(id, request);

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
        /// Adds a Character
        /// </summary>
        /// <param name="request">The required information to add the Character (CharacterName, ImageFiles)</param>
        /// <returns>
        /// 201 Created
        /// Location: api/Character/{CharacterId}
        /// {CharacterDto}
        /// or
        /// 400 Bad Request
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Character
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddCharacterRequest}
        /// ->
        /// Response Code: 201 Created
        /// Response Headers: Location: api/Character/{CharacterId}
        /// </example>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddCharacter([FromForm] AddCharacterRequest request)
        {
            (ServiceResponse response, CharacterDto? characterDto) = await _characterService.AddCharacter(request);

            switch (response.Status)
            {
                case ServiceStatus.BadRequest:
                    return BadRequest(response.Messages);
                case ServiceStatus.Error:
                    return StatusCode(500, response.Messages);
                default:
                    // Status = Created
                    return Created($"api/Character/{characterDto?.CharacterId}", characterDto);
            }
        }

        /// <summary>
        /// Deletes the Character
        /// </summary>
        /// <param name="id">The id of the Character to delete</param>
        /// <returns>
        /// 204 No Content
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Character/1
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpDelete(template: "{id}")]
        public async Task<ActionResult> DeleteCharacter(int id)
        {
            ServiceResponse response = await _characterService.DeleteCharacter(id);

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
        /// Add a Version to Character
        /// </summary>
        /// <param name="id">The id of the Character</param>
        /// <param name="request">The required information to add a Version to Character (VersionName, ImageFiles)</param>
        /// <returns>
        /// 201 Created
        /// Location: api/CharacterVersion/{CharacterVersionId}
        /// {CharacterVersionDto}
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
        /// POST: api/Character/1/Version
        /// Request Headers: { "Content-Type": "multipart/form-data" }
        /// Request Form Data: {AddVersionToCharacterRequest}
        /// ->
        /// Response Code: 201 Created
        /// Response Headers: Location: api/CharacterVersion/{CharacterVersionId}
        /// </example>
        [HttpPost(template: "{id}/Version")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddVersionToCharacter(int id, [FromForm] AddVersionToCharacterRequest request)
        {
            (ServiceResponse response, CharacterVersionDto? characterVersionDto) = await _characterService.AddVersionToCharacter(id, request);

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
                    // Status = Created
                    return Created($"api/CharacterVersion/{characterVersionDto?.CharacterVersionId}", characterVersionDto);
            }
        }

        /// <summary>
        /// Remove Versions from a Character
        /// </summary>
        /// <param name="id">The id of the Character</param>
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
        /// DELETE: api/Character/1/Version
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { CharacterVersionIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/Version")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveVersionsFromCharacter(int id, [FromBody] RemoveVersionsFromCharacterRequest request)
        {
            ServiceResponse response = await _characterService.RemoveVersionsFromCharacter(id, request);

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
