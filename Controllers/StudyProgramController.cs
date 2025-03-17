using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/study-program")]
[Authorize]
public class StudyProgramController : Controller
{
    private readonly IStudyProgramService _programService;

    public StudyProgramController(IStudyProgramService programService)
    {
        _programService = programService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrograms()
    {
        try
        {
            var programs = await _programService.GetAllPrograms();
            return Ok(programs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProgram([FromBody] StudyProgram program)
    {
        try
        {
            var count = await _programService.UpdateProgram(program);
            return Ok(new { message = "update program successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{name}")]
    public async Task<IActionResult> AddProgram([FromBody] string name)
    {
        try
        {
            var count = await _programService.AddProgram(name);
            return Ok(new { message = "add program successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
