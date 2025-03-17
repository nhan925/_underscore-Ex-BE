using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Services;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student-status")]
[Authorize]
public class StudentStatusController : Controller
{
    private readonly IStudentStatusService _studentStatusService;

    public StudentStatusController(IStudentStatusService studentStatusService)
    {
        _studentStatusService = studentStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatuses()
    {
        try
        {
            var statuses = await _studentStatusService.GetAllStudentStatuses();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStudentStatus([FromBody] StudentStatus studentStatus)
    {
        try
        {
            var count = await _studentStatusService.UpdateStudentStatus(studentStatus);
            return Ok(new { message = "update student status successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddStudentStatus([FromBody] string name)
    {
        try
        {
            var count = await _studentStatusService.AddStudentStatus(name);
            return Ok(new { message = "add student status successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
