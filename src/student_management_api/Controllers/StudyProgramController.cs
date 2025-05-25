using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Models.DTO;
using System;
using System.Threading.Tasks;
using student_management_api.Localization;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/study-program")]
[Authorize]
public class StudyProgramController : ControllerBase
{
    private readonly IStudyProgramService _programService;
    private readonly ILogger<StudyProgramController> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public StudyProgramController(IStudyProgramService programService, ILogger<StudyProgramController> logger, IStringLocalizer<Messages> localizer)
    {
        _programService = programService;
        _logger = logger;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrograms()
    {
        using (_logger.BeginScope("GetPrograms request"))
        {
            _logger.LogInformation("Fetching all study programs");

            var programs = await _programService.GetAllPrograms();

            _logger.LogInformation("Successfully retrieved {Count} study programs", programs.Count);
            return Ok(programs);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProgram([FromBody] StudyProgram program)
    {
        using (_logger.BeginScope("UpdateProgram request for StudyProgramId: {StudyProgramId}", program.Id))
        {
            _logger.LogInformation("Updating study program with ID {StudyProgramId}", program.Id);

            var count = await _programService.UpdateProgram(program);

            _logger.LogInformation("Study program with ID {StudyProgramId} updated successfully", program.Id);
            return Ok(new { message = _localizer["update_program_successfully"] });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddProgram(string name)
    {
        using (_logger.BeginScope("AddProgram request for StudyProgramName: {StudyProgramName}", name))
        {
            _logger.LogInformation("Adding new study program: {StudyProgramName}", name);

            var id = await _programService.AddProgram(name);

            _logger.LogInformation("Study program {StudyProgramName} added successfully with ID {StudyProgramId}", name, id);
            return Ok(new { id = id });
        }
    }
}
