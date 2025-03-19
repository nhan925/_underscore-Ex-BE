using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Text.Json;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(string id)
    {
        using (_logger.BeginScope("GetStudentById request for StudentId: {StudentId}", id))
        {
            _logger.LogInformation("Fetching student with ID: {StudentId}", id);
            var student = await _studentService.GetStudentById(id);
            return Ok(student);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] StudentFilter? filter = null)
    {
        using (_logger.BeginScope("GetStudents request, Page: {Page}, PageSize: {PageSize}", page, pageSize))
        {
            _logger.LogInformation("Fetching students with search term: {Search}", search);
            var students = await _studentService.GetStudents(page, pageSize, search, filter);
            return Ok(students);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudentById(string id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (_logger.BeginScope("UpdateStudentById request for StudentId: {StudentId}", id))
        {
            _logger.LogInformation("Updating student with ID: {StudentId}", id);
            var updatedCount = await _studentService.UpdateStudentById(id, request);

            if (updatedCount == 0)
                return NotFound(new { message = "student not found or no changes made" });

            _logger.LogInformation("Student with ID {StudentId} updated successfully", id);
            return Ok(new { message = "student updated successfully" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudentById(string id)
    {
        using (_logger.BeginScope("DeleteStudentById request for StudentId: {StudentId}", id))
        {
            _logger.LogInformation("Deleting student with ID: {StudentId}", id);
            var deletedCount = await _studentService.DeleteStudentById(id);

            if (deletedCount == 0)
                return NotFound(new { message = "student not found" });

            _logger.LogInformation("Student with ID {StudentId} deleted successfully", id);
            return Ok(new { message = "student deleted successfully" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (_logger.BeginScope("AddStudent request"))
        {
            _logger.LogInformation("Adding new student");
            var studentId = await _studentService.AddStudent(request);
            _logger.LogInformation("Student added successfully with ID {StudentId}", studentId);
            return Ok(new { StudentId = studentId });
        }
    }

    [HttpPost("import/{format}")]
    public async Task<IActionResult> AddStudentsFromFile(IFormFile file, string format)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        using (_logger.BeginScope("AddStudentsFromFile request, Format: {Format}", format))
        {
            _logger.LogInformation("Processing student import file: {FileName}", file.FileName);

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
                memoryStream.Position = 0;
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
            _logger.LogInformation("Students added successfully from file: {FileName}", file.FileName);
            return Ok(new { message = "Students added successfully" });
        }
    }

    [HttpGet("export/{format}")]
    public IActionResult ExportStudents(string format)
    {
        using (_logger.BeginScope("ExportStudents request, Format: {Format}", format))
        {
            _logger.LogInformation("Exporting students to {Format}", format);

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
                return BadRequest(new { message = "Invalid format" });
            }

            _logger.LogInformation("Students exported successfully as {Format}", format);
            return File(fileStream, contentType, fileName);
        }
    }
}
