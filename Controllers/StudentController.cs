using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        try
        {
            var student = await _studentService.GetStudentById(id);
            return Ok(student);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        try
        {
            var students = await _studentService.GetStudents(page, pageSize, search);
            return Ok(students);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudentById(string id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedCount = await _studentService.UpdateStudentById(id, request);
            if (updatedCount == 0)
            {
                return NotFound(new { message = "student not found or no changes made" });
            }

            return Ok(new { message = "student updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudentById(string id)
    {
        try
        {
            var deletedCount = await _studentService.DeleteStudentById(id);
            if (deletedCount == 0)
            {
                return NotFound(new { message = "student not found" });
            }
            return Ok(new { message = "student deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var studentId = await _studentService.AddStudent(request);
            return Ok(new { StudentId = studentId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
