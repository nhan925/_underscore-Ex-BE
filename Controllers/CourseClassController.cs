using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;
using student_management_api.Services;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize]
public class CourseClassController : Controller
{
    private readonly ICourseClassService _courseClassService;
    private readonly ILogger<CourseClassController> _logger;
    public CourseClassController(ICourseClassService courseClassService, ILogger<CourseClassController> logger)
    {
        _courseClassService = courseClassService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddCourseClass([FromBody] CourseClass courseClass)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (_logger.BeginScope("AddCourseClass request"))
        {
            _logger.LogInformation("Adding new CourseClass");
            var courseClassId = await _courseClassService.AddCourseClass(courseClass);

            _logger.LogInformation("CourseClass added successfully with ID {CourseClassId}", courseClassId);
            return Ok(new { CourseClassId = courseClassId });
        }
    }

    [HttpGet("{semesterId}")]
    public async Task<IActionResult> GetAllCourseClassesBySemester(int semesterId)
    {
        using (_logger.BeginScope("GetAllCourseClassesBySemester request with SemesterId: {SemesterId}", semesterId))
        {
            _logger.LogInformation("Fetching all course classes for semester with ID: {SemesterId}", semesterId);
            var courseClasses = await _courseClassService.GetAllCourseClassesBySemester(semesterId);


            _logger.LogInformation("Successfully retrieved {Count} course classes for semester with ID: {SemesterId}", courseClasses.Count, semesterId);
            return Ok(courseClasses);
        }
    }
}
