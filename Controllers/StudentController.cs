using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize]
public class StudentController : Controller
{
    private readonly IStudentRepository _studentRepository;

    public StudentController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        var student = await _studentRepository.GetStudentById(id);
        if (student == null)
        {
            return NotFound(new { message = "student not found" });
        }

        return Ok(student);
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var students = await _studentRepository.GetStudents(page, pageSize, search);
        return Ok(students);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudentById(string id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedCount = await _studentRepository.UpdateStudentById(id, request);
        if (updatedCount == 0)
        {
            return NotFound(new { message = "student not found or no changes made" });
        }

        return Ok(new { message = "student updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudentById(string id)
    {
        var deletedCount = await _studentRepository.DeleteStudentById(id);
        if (deletedCount == 0)
        {
            return NotFound(new { message = "student not found" });
        }

        return Ok(new { message = "student deleted successfully" });
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
            var studentId = await _studentRepository.AddStudent(request);
            return Ok(new { StudentId = studentId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
