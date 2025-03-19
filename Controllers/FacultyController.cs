using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/faculty")]
[Authorize]
public class FacultyController : Controller
{
    private readonly IFacultyService _facultyService;
    private readonly ILogger<FacultyController> _logger;

    public FacultyController(IFacultyService facultyService, ILogger<FacultyController> logger)
    {
        _facultyService = facultyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        using (_logger.BeginScope("GetFaculties request"))
        {
            _logger.LogInformation("Fetching all faculties");

            var faculties = await _facultyService.GetAllFaculties();

            _logger.LogInformation("Successfully retrieved {Count} faculties", faculties.Count());
            return Ok(faculties);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFaculty([FromBody] Faculty faculty)
    {
        using (_logger.BeginScope("UpdateFaculty request for FacultyId: {FacultyId}", faculty.Id))
        {
            _logger.LogInformation("Updating faculty with ID {FacultyId}", faculty.Id);

            var count = await _facultyService.UpdateFaculty(faculty);

            _logger.LogInformation("Faculty with ID {FacultyId} updated successfully", faculty.Id);
            return Ok(new { message = "Update faculty successfully" });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddFaculty(string name)
    {
        using (_logger.BeginScope("AddFaculty request for FacultyName: {FacultyName}", name))
        {
            _logger.LogInformation("Adding new faculty: {FacultyName}", name);

            var id = await _facultyService.AddFaculty(name);

            _logger.LogInformation("Faculty {FacultyName} added successfully with ID {FacultyId}", name, id);
            return Ok(new { id = id });
        }
    }
}
