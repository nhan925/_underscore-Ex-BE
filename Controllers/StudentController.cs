using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using student_management_api.Repositories;
using System.Text.Json;

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
    public async Task<IActionResult> GetStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null, [FromQuery] StudentFilter? filter = null)
    {
        try
        {
            var students = await _studentService.GetStudents(page, pageSize, search, filter);
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

    [HttpPost("import/{format}")]
    public async Task<IActionResult> AddStudentsFromFile(IFormFile file, string format)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        try
        {
            // Read file into memory to avoid stream reuse issues
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset position

            string jsonContent;
            format = format.ToLowerInvariant(); // Normalize format

            if (format == "excel")
            {
                jsonContent = await _studentService.ImportExcelToJson(memoryStream);
            }
            else if (format == "json")
            {
                memoryStream.Position = 0; // Reset position for reading JSON
                using var reader = new StreamReader(memoryStream);
                jsonContent = await reader.ReadToEndAsync();
            }
            else
            {
                return BadRequest(new { message = "Invalid format" });
            }

            var requests = JsonSerializer.Deserialize<List<AddStudentRequest>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (requests == null || !requests.Any())
            {
                return BadRequest(new { message = "Invalid or empty file" });
            }

            await _studentService.AddStudents(requests);
            return Ok(new { message = "Students added successfully" });
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }



    [HttpGet("export/{format}")]
    public IActionResult ExportStudents(string format)
    {
        try
        {
            Stream fileStream;
            string fileName;
            string contentType;

            if (format.ToLower() == "json")
            {
                fileStream = _studentService.ExportToJson();
                fileName = "students.json";
                contentType = "application/json";
            }
            else if (format.ToLower() == "excel")
            {
                fileStream = _studentService.ExportToExcel();
                fileName = "students.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else
            {
                return BadRequest(new { message = "invalid format" });
            }

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
