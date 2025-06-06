using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using Swashbuckle.AspNetCore.Annotations;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/year")]
[Authorize]
public class YearAndSemesterController : ControllerBase
{
    private readonly IYearAndSemesterService _yearAndSemesterService;
    private readonly ILogger<YearAndSemesterController> _logger;
    public YearAndSemesterController(IYearAndSemesterService yearAndSemesterService, ILogger<YearAndSemesterController> logger)
    {
        _yearAndSemesterService = yearAndSemesterService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all years",
        Description = "Endpoint to retrieve all academic years."
    )]
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

    [HttpGet("{yearId}/semesters")]
    [SwaggerOperation(
        Summary = "Get semesters by year",
        Description = "Endpoint to retrieve all semesters for a specific academic year. Requires valid YearId."
    )]
    public async Task<IActionResult> GetSemestersByYear(int yearId)
    {
        using (_logger.BeginScope("GetSemesterByYear request with YearId: {YearId}", yearId))
        {
            _logger.LogInformation("Fetching semesters for year with ID: {YearId}", yearId);
            var semesters = await _yearAndSemesterService.GetSemestersByYear(yearId);
            return Ok(semesters);
        }
    }
}
