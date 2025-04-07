using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/year")]
[Authorize]
public class YearAndSemesterController : Controller
{
    private readonly IYearAndSemesterService _yearAndSemesterService;
    private readonly ILogger<YearAndSemesterController> _logger;
    public YearAndSemesterController(IYearAndSemesterService yearAndSemesterService, ILogger<YearAndSemesterController> logger)
    {
        _yearAndSemesterService = yearAndSemesterService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllYears()
    {
        using (_logger.BeginScope("GetAllYears request"))
        {
            _logger.LogInformation("Fetching all years");

            var years = await _yearAndSemesterService.GetAllYears();

            _logger.LogInformation("Successfully retrieved {Count} years", years.Count());
            return Ok(years);
        }
    }

    [HttpGet("{id}/semesters")]
    public async Task<IActionResult> GetSemestersByYear(int id)
    {
        using (_logger.BeginScope("GetSemesterByYear request with YearId: {YearId}", id))
        {
            _logger.LogInformation("Fetching semesters for year with ID: {YearId}", id);
            var semesters = await _yearAndSemesterService.GetSemestersByYear(id);
            return Ok(semesters);
        }
    }
}
