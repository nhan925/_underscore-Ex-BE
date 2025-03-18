using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

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

    [HttpPut]
    public async Task<IActionResult> UpdateFaculty([FromBody] Faculty faculty)
    {
        try
        {
            var count = await _facultytService.UpdateFaculty(faculty);
            return Ok(new { message = "update faculty successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddFaculty(string name)
    {
        try
        {
            var id = await _facultytService.AddFaculty(name);
            return Ok(new { id = id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
