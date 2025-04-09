using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
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
    private readonly IConfigurationService _configurationService;
    private readonly ICourseEnrollmentService _courseEnrollmentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IStudentService studentService,
        IConfigurationService configurationService,
        ICourseEnrollmentService courseEnrollmentService,
        ILogger<StudentController> logger
    )
    {
        _studentService = studentService;
        _configurationService = configurationService;
        _courseEnrollmentService = courseEnrollmentService;
        _logger = logger;
    }

    #region ValidateStudentInfomationsInRequestFunctions
    private async Task<(bool, IActionResult)> ValidateStudentEmail(string email)
    {
        _logger.LogInformation("Checking email domain for {Email}", email);
        var isValidEmail = await _configurationService.CheckEmailDomain(email);

        if (!isValidEmail)
        {
            _logger.LogWarning("Invalid email domain for {Email}", email);
            return (false, BadRequest(new { message = "Invalid email domain" }));
        }

        return (true, Ok());
    }

    private async Task<(bool, IActionResult)> ValidateStudentPhoneNumber(string phoneNumber)
    {
        _logger.LogInformation("Checking phone number for {PhoneNumber}", phoneNumber);
        var isValidPhoneNumber = await _configurationService.CheckPhoneNumber(phoneNumber);

        if (!isValidPhoneNumber)
        {
            _logger.LogWarning("Invalid phone number for {PhoneNumber}", phoneNumber);
            return (false, BadRequest(new { message = "Invalid phone number" }));
        }

        return (true, Ok());
    }

    private async Task<(bool, IActionResult)> ValidateStudentStatus(int? currentStatus, int? nextStatus)
    {
        _logger.LogInformation("Checking student status transition from {CurrentStatus} to {NextStatus}", currentStatus, nextStatus);
        var nextStatuses = await _configurationService.GetNextStatuses((int)currentStatus);

        if (!nextStatuses.Any(s => s.Id == nextStatus))
        {
            _logger.LogWarning("Invalid student status transition from {CurrentStatus} to {NextStatus}", currentStatus, nextStatus);
            return (false, BadRequest(new { message = "Invalid student status transition" }));
        }

        return (true, Ok());
    }

    private async Task<(bool, IActionResult)> ValidateStudentInformationsInRequest(Object request, string? studentId = null)
    {
        if (request == null)
        {
            return (false, BadRequest(new { message = "Invalid request" }));
        }

        if (request is AddStudentRequest addStudentRequest)
        {
            var emailDomainValidationResult = await ValidateStudentEmail(addStudentRequest.Email);
            if (!emailDomainValidationResult.Item1)
            {
                return (false, emailDomainValidationResult.Item2);
            }

            var phoneNumberValidationResult = await ValidateStudentPhoneNumber(addStudentRequest.PhoneNumber);
            if (!phoneNumberValidationResult.Item1)
            {
                return (false, phoneNumberValidationResult.Item2);
            }

            return (true, Ok());
        }
        else if (request is UpdateStudentRequest updateStudentRequest)
        {
            if (updateStudentRequest.Email != null)
            {
                var result = await ValidateStudentEmail(updateStudentRequest.Email);
                if (!result.Item1)
                {
                    return (false, result.Item2);
                }
            }

            if (updateStudentRequest.PhoneNumber != null)
            {
                var result = await ValidateStudentPhoneNumber(updateStudentRequest.PhoneNumber);
                if (!result.Item1)
                {
                    return (false, result.Item2);
                }
            }

            if (updateStudentRequest.StatusId != null)
            {
                var currentStudent = await _studentService.GetStudentById(studentId);
                if (currentStudent == null)
                {
                    return (false, NotFound(new { message = "Student not found" }));
                }

                var statusValidationResult = await ValidateStudentStatus(currentStudent.StatusId, updateStudentRequest.StatusId);
                if (!statusValidationResult.Item1)
                {
                    return (false, statusValidationResult.Item2);
                }
            }

            return (true, Ok());
        }
        else
        {
            return (false, BadRequest(new { message = "Invalid request" }));
        }
    }
    #endregion

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
            var testResult = await ValidateStudentInformationsInRequest(request, id);
            if (!testResult.Item1) // If validation fails
            {
                return testResult.Item2;
            }

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
            var testResult = await ValidateStudentInformationsInRequest(request);
            if (!testResult.Item1) // If validation fails
            {
                return testResult.Item2;
            }

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
                jsonContent = await _studentService.ConvertExcelToJson(memoryStream);
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

            if (requests.Any(r => ValidateStudentInformationsInRequest(r).Result.Item1 == false)) // If validation fails
            {
                return BadRequest(new { message = "Invalid student information in file" });
            }

            await _studentService.AddStudents(requests);
            _logger.LogInformation("Students added successfully from file: {FileName}", file.FileName);
            return Ok(new { message = "Students added successfully" });
        }
    }

    [HttpGet("export/{format}")]
    public async Task<IActionResult> ExportStudents(string format)
    {
        using (_logger.BeginScope("ExportStudents request, Format: {Format}", format))
        {
            _logger.LogInformation("Exporting students to {Format}", format);

            Stream fileStream;
            string fileName;
            string contentType;

            if (format.ToLower() == "json")
            {
                fileStream = await _studentService.ExportToJson();
                fileName = "students.json";
                contentType = "application/json";
            }
            else if (format.ToLower() == "excel")
            {
                fileStream = await _studentService.ExportToExcel();
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

    [HttpGet("transcript/{student_id}")]
    public async Task<IActionResult> GetStudentTranscriptById(string student_id)
    {
        using (_logger.BeginScope("GetStudentTranscriptById request for StudentId: {StudentId}", student_id))
        {           
            // Load HTML template
            _logger.LogInformation("Loading HTML template for transcript");
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "transcript_template.html");
            var htmlTemplate = System.IO.File.ReadAllText(templatePath);

            _logger.LogInformation("Fetching transcript for student with ID: {StudentId}", student_id);
            var transcriptStream = await _courseEnrollmentService.GetTranscriptOfStudentById(student_id, htmlTemplate);
            if (transcriptStream == null)
            {
                _logger.LogWarning("Transcript not found for student with ID: {StudentId}", student_id);
                return NotFound(new { message = "Transcript not found" }); 
            }

            _logger.LogInformation("Transcript fetched successfully for student with ID: {StudentId}", student_id);

            return File(transcriptStream, "application/pdf", $"{student_id}_transcript.pdf");
        }
    }
}
