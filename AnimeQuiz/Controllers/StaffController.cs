using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        // dependency injection of service interfaces
        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        /// <summary>
        /// Returns a list of Staffs
        /// </summary>
        /// <returns>
        /// 200 OK
        /// [ {StaffDto}, {StaffDto}, ... ]
        /// </returns>
        /// <example>
        /// GET: api/Staff -> [ {StaffDto}, {StaffDto}, ... ]
        /// </example>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDto>>> ListStaffs()
        {
            IEnumerable<StaffDto> staffDtos = await _staffService.ListStaffs();
            return Ok(staffDtos);
        }

        /// <summary>
        /// Return a single Staff specified by its {id}
        /// </summary>
        /// <param name="id">The Staff id</param>
        /// <returns>
        /// 200 OK
        /// {StaffDto}
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// GET api/Staff/1 -> {StaffDto}
        /// </example>
        [HttpGet(template: "{id}")]
        public async Task<ActionResult<StaffDto>> FindStaff(int id)
        {
            StaffDto? staffDto = await _staffService.FindStaff(id);
            return staffDto == null ? NotFound($"Staff with id {id} not found.") : Ok(staffDto);
        }

        /// <summary>
        /// Update a Staff
        /// </summary>
        /// <param name="id">The id of the Staff to update</param>
        /// <param name="request">The required information to update the Staff (StaffName)</param>
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
        /// PUT: api/Staff/1
        /// Request Header: { "Content-Type": "application/json" }
        /// Request Body: {UpdateStaffRequest}
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpPut(template: "{id}")]
        [Consumes("application/json")]
        public async Task<ActionResult> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
        {
            ServiceResponse response = await _staffService.UpdateStaff(id, request);

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
        /// Adds a Staff
        /// </summary>
        /// <param name="request">The required information to add the Staff (StaffName)</param>
        /// <returns>
        /// 201 Created
        /// Location: api/Staff/{StaffId}
        /// {StaffDto}
        /// or
        /// 400 Bad Request
        /// or
        /// 409 Conflict
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// POST: api/Staff
        /// Request Headers: { "Content-Type": "application/json" }
        /// Request Body: {AddStaffRequest}
        /// ->
        /// Response Code: 201 Created
        /// Response Headers: Location: api/Staff/{StaffId}
        /// </example>
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult> AddStaff([FromBody] AddStaffRequest request)
        {
            (ServiceResponse response, StaffDto? staffDto) = await _staffService.AddStaff(request);

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
                    return Created($"api/Staff/{staffDto?.StaffId}", staffDto);
            }
        }

        /// <summary>
        /// Deletes the Staff
        /// </summary>
        /// <param name="id">The id of the Staff to delete</param>
        /// <returns>
        /// 204 No Content
        /// or
        /// 404 Not Found
        /// or
        /// 500 Internal Server Error
        /// </returns>
        /// <example>
        /// DELETE: api/Staff/1
        /// ->
        /// Response Code: 204 No Content
        /// </example>
        [HttpDelete(template: "{id}")]
        public async Task<ActionResult> DeleteStaff(int id)
        {
            ServiceResponse response = await _staffService.DeleteStaff(id);

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
    }
}
