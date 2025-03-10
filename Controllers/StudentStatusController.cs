using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;

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
}
