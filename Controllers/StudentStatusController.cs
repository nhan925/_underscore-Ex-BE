using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student-status")]
[Authorize]
public class StudentStatusController : Controller
{
    private readonly IStudentStatusRepository _studentStatusRepository;

    public StudentStatusController(IStudentStatusRepository studentStatusRepository)
    {
        _studentStatusRepository = studentStatusRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await _studentStatusRepository.GetAllStudentStatuses();
        return Ok(statuses);
    }
}
