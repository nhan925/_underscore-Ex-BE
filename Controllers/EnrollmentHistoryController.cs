using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/enrollment_history")]
[Authorize]
public class EnrollmentHistoryController : Controller
{
    private readonly IEnrollmentHistoryService _enrollmentHistoryService;
    private readonly ILogger<EnrollmentHistoryController> _logger;

    public EnrollmentHistoryController(IEnrollmentHistoryService enrollmentHistoryService, ILogger<EnrollmentHistoryController> logger)
    {
        _enrollmentHistoryService = enrollmentHistoryService;
        _logger = logger;
    }

    [HttpGet("{semester_id}")]
    public async Task<IActionResult> GetEnrollmentHistoryBySemester(int semester_id)
    {
        using (_logger.BeginScope("GetEnrollmentHistoryBySemester request"))
        {
            _logger.LogInformation("Fetching all enrollment histories of semester id {SemesterId}", semester_id);

            var histories = await _enrollmentHistoryService.GetEnrollmentHistoryBySemester(semester_id);

            _logger.LogInformation("Successfully retrieved {Count} enrollment histories", histories.Count());
            return Ok(histories);
        }
    }
}
