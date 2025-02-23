using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using AnimeQuiz.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AnimeQuiz.Controllers
{
    [Route("Staff")]
    public class StaffPageController : Controller
    {
        private readonly IStaffService _staffService;

        // dependency injection of service interfaces
        public StaffPageController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        // GET: Staff
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<StaffDto> staffDtos = await _staffService.ListStaffs();
            return View(staffDtos);
        }

        // GET: Staff/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            StaffDto? staffDto = await _staffService.FindStaff(id);

            return staffDto != null
                ? View(staffDto)
                : View("Error", new ErrorViewModel() { Errors = ["Staff not found."] }); ;
        }

        // GET: Staff/New
        [HttpGet("New")]
        public ActionResult New()
        {
            return View();
        }

        // POST: Staff/Add
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] AddStaffRequest request)
        {
            (ServiceResponse response, StaffDto? staffDto) = await _staffService.AddStaff(request);

            return (response.Status == ServiceStatus.Created && staffDto != null)
                ? RedirectToAction("Details", "StaffPage", new { id = staffDto.StaffId })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // GET: Staff/{id}/Edit
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            StaffDto? staffDto = await _staffService.FindStaff(id);

            return staffDto != null
                ? View(staffDto)
                : View("Error", new ErrorViewModel() { Errors = ["Staff not found."] }); ;
        }

        // POST: Staff/{id}/Update
        [HttpPost("Staff/{id}/Update")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateStaffRequest request)
        {
            ServiceResponse response = await _staffService.UpdateStaff(id, request);

            return response.Status == ServiceStatus.Updated
                ? RedirectToAction("Details", "StaffPage", new { id = id })
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }

        // GET: Staff/{id}/ConfirmDelete
        [HttpGet("Staff/{id}/ConfirmDelete")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            StaffDto? staffDto = await _staffService.FindStaff(id);

            return staffDto != null
                ? View(staffDto)
                : View("Error", new ErrorViewModel() { Errors = ["Staff not found."] }); ;
        }

        // POST: Staff/{id}/Delete
        [HttpPost("Staff/{id}/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _staffService.DeleteStaff(id);

            return response.Status == ServiceStatus.Deleted
                ? RedirectToAction("Index", "StaffPage")
                : View("Error", new ErrorViewModel() { Errors = response.Messages });
        }
    }
}
