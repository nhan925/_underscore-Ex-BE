using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/faculty")]
[Authorize]
public class FacultyController : Controller
{
    private readonly IFacultyRepository _facultytRepository;

    public FacultyController(IFacultyRepository facultytRepository)
    {
        _facultytRepository = facultytRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        var faculties = await _facultytRepository.GetAllFaculties();
        return Ok(faculties);
    }
}
