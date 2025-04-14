using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/lecturers")]
[Authorize]
public class LecturersController : ControllerBase
{
    private readonly ILecturersService _lecturersService;
    private readonly ILogger<LecturersController> _logger;

    public LecturersController(ILecturersService lecturersService, ILogger<LecturersController> logger)
    {
        _lecturersService = lecturersService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLecturers()
    {
        using (_logger.BeginScope("GetAllLecturers request"))
        {
            _logger.LogInformation("Fetching all lecturers");

            var lecturers = await _lecturersService.GetAllLecturers();

            _logger.LogInformation("Successfully retrieved {Count} lecturers", lecturers.Count);
            return Ok(lecturers);
        }
    }
}
