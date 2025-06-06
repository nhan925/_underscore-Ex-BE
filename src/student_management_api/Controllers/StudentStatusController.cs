using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Models.DTO;
using System;
using System.Threading.Tasks;
using student_management_api.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student-status")]
[Authorize]
public class StudentStatusController : ControllerBase
{
    private readonly IStudentStatusService _studentStatusService;
    private readonly ILogger<StudentStatusController> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public StudentStatusController(IStudentStatusService studentStatusService, ILogger<StudentStatusController> logger, IStringLocalizer<Messages> localizer)
    {
        _studentStatusService = studentStatusService;
        _logger = logger;
        _localizer = localizer;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all student statuses",
        Description = "Endpoint to retrieve all student statuses. Returns a list of student statuses with their IDs and names."
    )]
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
    [SwaggerOperation(
        Summary = "Update a student status",
        Description = "Endpoint to update an existing student status. Requires a valid StudentStatus object with an ID."
    )]
    public async Task<IActionResult> UpdateStudentStatus([FromBody] StudentStatus studentStatus)
    {
        using (_logger.BeginScope("UpdateStudentStatus request for StudentStatusId: {StudentStatusId}", studentStatus.Id))
        {
            _logger.LogInformation("Updating student status with ID {StudentStatusId}", studentStatus.Id);

            var count = await _studentStatusService.UpdateStudentStatus(studentStatus);

            _logger.LogInformation("Student status with ID {StudentStatusId} updated successfully", studentStatus.Id);
            return Ok(new { message = _localizer["update_student_status_successfully"].Value });
        }
    }

    [HttpPost("{name}")]
    [SwaggerOperation(
        Summary = "Add a new student status",
        Description = "Endpoint to add a new student status by name. Returns the ID of the newly created status."
    )]
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
