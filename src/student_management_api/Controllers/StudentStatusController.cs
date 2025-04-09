using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using System;
using System.Threading.Tasks;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student-status")]
[Authorize]
public class StudentStatusController : Controller
{
    private readonly IStudentStatusService _studentStatusService;
    private readonly ILogger<StudentStatusController> _logger;

    public StudentStatusController(IStudentStatusService studentStatusService, ILogger<StudentStatusController> logger)
    {
        _studentStatusService = studentStatusService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatuses()
    {
        using (_logger.BeginScope("GetStatuses request"))
        {
            _logger.LogInformation("Fetching all student statuses");

            var statuses = await _studentStatusService.GetAllStudentStatuses();

            _logger.LogInformation("Successfully retrieved {Count} student statuses", statuses.Count);
            return Ok(statuses);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStudentStatus([FromBody] StudentStatus studentStatus)
    {
        using (_logger.BeginScope("UpdateStudentStatus request for StudentStatusId: {StudentStatusId}", studentStatus.Id))
        {
            _logger.LogInformation("Updating student status with ID {StudentStatusId}", studentStatus.Id);

            var count = await _studentStatusService.UpdateStudentStatus(studentStatus);

            _logger.LogInformation("Student status with ID {StudentStatusId} updated successfully", studentStatus.Id);
            return Ok(new { message = "Update student status successfully" });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddStudentStatus(string name)
    {
        using (_logger.BeginScope("AddStudentStatus request for StudentStatusName: {StudentStatusName}", name))
        {
            _logger.LogInformation("Adding new student status: {StudentStatusName}", name);

            var id = await _studentStatusService.AddStudentStatus(name);

            _logger.LogInformation("Student status {StudentStatusName} added successfully with ID {StudentStatusId}", name, id);
            return Ok(new { id = id });
        }
    }
}
