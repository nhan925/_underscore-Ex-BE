using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Models.CourseEnrollment;
using System.Globalization;
using student_management_api.Localization;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/course-enrollments")]
[Authorize]
public class CourseEnrollmentController : ControllerBase
{
    private readonly ICourseEnrollmentService _courseEnrollmentService;
    private readonly ILogger<CourseEnrollmentController> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public CourseEnrollmentController(ICourseEnrollmentService courseEnrollmentService, ILogger<CourseEnrollmentController> logger, IStringLocalizer<Messages> localizer)
    {
        _courseEnrollmentService = courseEnrollmentService;
        _logger = logger;
        _localizer = localizer;
    }

    [HttpGet("history/{semester_id}")]
    public async Task<IActionResult> GetEnrollmentHistoryBySemester(int semester_id)
    {
        using (_logger.BeginScope("GetEnrollmentHistoryBySemester request"))
        {
            _logger.LogInformation("Fetching all enrollment history of semester id {SemesterId}", semester_id);

            var history = await _courseEnrollmentService.GetEnrollmentHistoryBySemester(semester_id);

            _logger.LogInformation("Successfully retrieved {Count} histories", history.Count);
            return Ok(history);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAndUnregisterClass([FromQuery] string action, [FromBody] CourseEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
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

                    return Ok(new { message = _localizer["successfully_registered_class"] });
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

                    return Ok(new { message = _localizer["successfully_unregistered_class"] });
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
            return BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_action_specified_Use_register_or_unregister"]));
        }
    }

    [HttpPut("update-grade")]
    public async Task<IActionResult> UpdateStudentGrade([FromBody] UpdateStudentGradeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
        }

        using (_logger.BeginScope("UpdateStudentGrade request"))
        {
            _logger.LogInformation("Updating student grade with request: {@Request}", request);

            try
            {
                await _courseEnrollmentService.UpdateStudentGrade(request.StudentId!, request.CourseId!, request.Grade);
                _logger.LogInformation("Successfully updated student grade");

                return Ok(new { message = _localizer["successfully_updated_student_grade"] });
            }
            catch
            {
                _logger.LogWarning("Failed to update student grade");
                throw;
            }
        }
    }

    [HttpGet("transcript/{student_id}")]
    public async Task<IActionResult> GetStudentTranscriptById(string student_id)
    {
        using (_logger.BeginScope("GetStudentTranscriptById request for StudentId: {StudentId}", student_id))
        {
            // Load HTML template
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"transcript_template-{culture}.html");
            var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

            _logger.LogInformation("Fetching transcript for student with ID: {StudentId}", student_id);
            var transcriptStream = await _courseEnrollmentService.GetTranscriptOfStudentById(student_id, htmlTemplate);
            if (transcriptStream == null)
            {
                _logger.LogWarning("Transcript not found for student with ID: {StudentId}", student_id);
                return NotFound(new ErrorResponse<string>(status: 404, message: $"{_localizer["transcript_not_found"]}, ID: {student_id}"));
            }

            _logger.LogInformation("Transcript fetched successfully for student with ID: {StudentId}", student_id);

            return File(transcriptStream, "application/pdf", $"{student_id}_transcript.pdf");
        }
    }
}
