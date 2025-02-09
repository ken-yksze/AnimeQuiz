using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;

        // dependency injection of service interfaces
        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;

        }

        /// <summary>
        /// Add Singers to a Music
        /// </summary>
        /// <param name="id">The id of the Music</param>
        /// <param name="request">The request object, including a list of Singer Ids (int)</param>
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
        /// POST: api/Music/1/Singer
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { SingerIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpPost(template: "{id}/Singer")]
        [Consumes("application/json")]
        public async Task<ActionResult> AddSingersToMusic(int id, [FromBody] AddSingersToMusicRequest request)
        {
            ServiceResponse response = await _musicService.AddSingersToMusic(id, request);

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
        /// Remove Singers from a Music
        /// </summary>
        /// <param name="id">The id of the Music</param>
        /// <param name="request">The request object, including a list of Singer Ids (int)</param>
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
        /// DELETE: api/Music/1/Singer
        /// Headers: { "Content-Type": "application/json" }
        /// Body: { SingerIds: [ 1, 2, 3 ] }
        /// ->
        /// Response Code: 200 OK
        /// Response Body: ["3 records are affected."]
        /// </example>
        [HttpDelete(template: "{id}/Singer")]
        [Consumes("application/json")]
        public async Task<ActionResult> RemoveSingersFromMusic(int id, [FromBody] RemoveSingersFromMusicRequest request)
        {
            ServiceResponse response = await _musicService.RemoveSingersFromMusic(id, request);

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
