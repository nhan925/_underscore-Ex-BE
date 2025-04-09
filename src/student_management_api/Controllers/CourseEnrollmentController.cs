using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.CourseEnrollment;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/course-enrollments")]
[Authorize]
public class CourseEnrollmentController : Controller
{
    private readonly ICourseEnrollmentService _courseEnrollmentService;
    private readonly ILogger<CourseEnrollmentController> _logger;

    public CourseEnrollmentController(ICourseEnrollmentService courseEnrollmentService, ILogger<CourseEnrollmentController> logger)
    {
        _courseEnrollmentService = courseEnrollmentService;
        _logger = logger;
    }

    [HttpGet("history/{semester_id}")]
    public async Task<IActionResult> GetEnrollmentHistoryBySemester(int semester_id)
    {
        using (_logger.BeginScope("GetEnrollmentHistoryBySemester request"))
        {
            _logger.LogInformation("Fetching all enrollment history of semester id {SemesterId}", semester_id);

            var history = await _courseEnrollmentService.GetEnrollmentHistoryBySemester(semester_id);

            _logger.LogInformation("Successfully retrieved {Count} histories", history.Count());
            return Ok(history);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAndUnregisterClass([FromQuery] string action, [FromBody] CourseEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (action == "register")
        {
            using (_logger.BeginScope("RegisterClass request"))
            {
                _logger.LogInformation("Registering class with request: {@Request}", request);
                try
                {
                    await _courseEnrollmentService.RegisterClass(request);
                    _logger.LogInformation("Successfully registered class");

                    return Ok(new { message = "Successfully registered class" });
                }
                catch
                {
                    _logger.LogWarning("Failed to register class");
                    throw;
                }
            }
        }
        else if (action == "unregister")
        {
            using (_logger.BeginScope("UnregisterClass request"))
            {
                _logger.LogInformation("Unregistering class with request: {@Request}", request);
                try
                {
                    await _courseEnrollmentService.UnregisterClass(request);
                    _logger.LogInformation("Successfully unregistered class");

                    return Ok(new { message = "Successfully unregistered class" });
                }
                catch
                {
                    _logger.LogWarning("Failed to unregister class");
                    throw;
                }
            }
        }
        else
        {
            return BadRequest(new { message = "Invalid action" });
        }
    }

    [HttpPut("update-grade")]
    public async Task<IActionResult> UpdateStudentGrade([FromBody] UpdateStudentGradeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using (_logger.BeginScope("UpdateStudentGrade request"))
        {
            _logger.LogInformation("Updating student grade with request: {@Request}", request);

            try
            {
                await _courseEnrollmentService.UpdateStudentGrade(request.StudentId, request.CourseId, request.Grade);
                _logger.LogInformation("Successfully updated student grade");

                return Ok(new { message = "Successfully updated student grade" });
            }
            catch
            {
                _logger.LogWarning("Failed to update student grade");
                throw;
            }
        }
    }
}
