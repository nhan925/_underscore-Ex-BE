using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/faculty")]
[Authorize]
public class FacultyController : Controller
{
    private readonly IFacultyService _facultytService;

    public FacultyController(IFacultyService facultytService)
    {
        _facultytService = facultytService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        try
        {
            var faculties = await _facultytService.GetAllFaculties();
            return Ok(faculties);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
