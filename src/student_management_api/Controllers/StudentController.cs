using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Text.Json;
using student_management_api.Localization;
using student_management_api.Models.CourseEnrollment;
using System.Globalization;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<StudentController> _logger;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ICourseEnrollmentService _courseEnrollmentService;

    public StudentController(
        IStudentService studentService,
        IConfigurationService configurationService,
        ILogger<StudentController> logger,
        IStringLocalizer<Messages> localizer,
        ICourseEnrollmentService courseEnrollmentService
    )
    {
        _studentService = studentService;
        _configurationService = configurationService;
        _logger = logger;
        _localizer = localizer;
        _courseEnrollmentService = courseEnrollmentService;
    }

    #region ValidateStudentInfomationsInRequestFunctions
    private async Task<(bool, IActionResult)> ValidateStudentEmail(string email)
    {
        _logger.LogInformation("Checking email domain for {Email}", email);
        var isValidEmail = await _configurationService.CheckEmailDomain(email);

        if (!isValidEmail)
        {
            _logger.LogWarning("Invalid email domain for {Email}", email);
            return (false, BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_email_domain"])));
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
            return (false, BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_phone_number"])));
        }

        return (true, Ok());
    }

    private async Task<(bool, IActionResult)> ValidateStudentStatus(int currentStatus, int? nextStatus)
    {
        _logger.LogInformation("Checking student status transition from {CurrentStatus} to {NextStatus}", currentStatus, nextStatus);
        var nextStatuses = await _configurationService.GetNextStatuses((int)currentStatus);

        if (!nextStatuses.Any(s => s.Id == nextStatus))
        {
            _logger.LogWarning("Invalid student status transition from {CurrentStatus} to {NextStatus}", currentStatus, nextStatus);
            return (false, BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_student_status_transition"])));
        }

        return (true, Ok());
    }

    private async Task<(bool, IActionResult)> ValidateStudentInformationsInRequest(Object request, string? studentId = null)
    {
        if (request == null)
        {
            return (false, BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_request"])));
        }

        if (request is AddStudentRequest addStudentRequest)
        {
            var emailDomainValidationResult = await ValidateStudentEmail(addStudentRequest.Email!);
            if (!emailDomainValidationResult.Item1)
            {
                return (false, emailDomainValidationResult.Item2);
            }

            var phoneNumberValidationResult = await ValidateStudentPhoneNumber(addStudentRequest.PhoneNumber!);
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
                var currentStudent = await _studentService.GetStudentById(studentId!);
                if (currentStudent == null)
                {
                    return (false, NotFound(new ErrorResponse<string>(status: 404, message: _localizer["student_not_found"])));
                }

                var statusValidationResult = await ValidateStudentStatus((int)currentStudent.StatusId!, updateStudentRequest.StatusId);
                if (!statusValidationResult.Item1)
                {
                    return (false, statusValidationResult.Item2);
                }
            }

            return (true, Ok());
        }
        else
        {
            return (false, BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_request_type"])));
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
        {
            return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
        }

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
            {
                return NotFound(new ErrorResponse<string>(status: 404, message: _localizer["student_not_found_or_no_changes_made"]));
            }

            _logger.LogInformation("Student with ID {StudentId} updated successfully", id);
            return Ok(new { message = _localizer["student_updated_successfully"].Value });
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
            {
                return NotFound(new ErrorResponse<string>(status: 404, message: _localizer["student_not_found_or_already_deleted"]));
            }

            _logger.LogInformation("Student with ID {StudentId} deleted successfully", id);
            return Ok(new { message = _localizer["student_deleted_successfully"].Value });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
        }

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
        {
            return BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["no_file_uploaded"]));
        }

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
                jsonContent = _studentService.ConvertExcelToJson(memoryStream);
            }
            else if (format == "json")
            {
                memoryStream.Position = 0;
                using var reader = new StreamReader(memoryStream);
                jsonContent = await reader.ReadToEndAsync();
            }
            else
            {
                return BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_format_Supported_formats_are_json_and_excel"]));
            }

            var requests = JsonSerializer.Deserialize<List<AddStudentRequest>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (requests == null || !requests.Any())
            {
                return BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_or_empty_file"]));
            }

            var errors = new List<int>();

            for (int i = 0; i < requests.Count; i++)
            {
                var result = await ValidateStudentInformationsInRequest(requests[i]);
                if (!result.Item1)
                {
                    errors.Add(i);
                }
            }

            if (errors.Any())
            {
                return BadRequest(new ErrorResponse<object>(
                    status: 400,
                    message: _localizer["invalid_student_information_found"],
                    details: new
                    {
                        InvalidEntries = errors.Select(e => new { Index = e, Request = requests[e] })
                    }
                ));
            }

            await _studentService.AddStudents(requests);
            _logger.LogInformation("Students added successfully from file: {FileName}", file.FileName);
            return Ok(new { message = _localizer["students_added_successfully"].Value });
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
                return BadRequest(new ErrorResponse<string>(status: 400, message: _localizer["invalid_format_Supported_formats_are_json_and_excel"]));
            }

            _logger.LogInformation("Students exported successfully as {Format}", format);
            return File(fileStream, contentType, fileName);
        }
    }

    [HttpPut("update-grade")]
    public async Task<IActionResult> UpdateStudentGrade([FromBody] UpdateStudentGradeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
        }

        using (_logger.BeginScope("UpdateStudentGrade request"))
        {
            _logger.LogInformation("Updating student grade with request: {@Request}", request);

            try
            {
                await _courseEnrollmentService.UpdateStudentGrade(request.StudentId!, request.CourseId!, request.Grade);
                _logger.LogInformation("Successfully updated student grade");

                return Ok(new { message = _localizer["successfully_updated_student_grade"].Value });
            }
            catch
            {
                _logger.LogWarning("Failed to update student grade");
                throw;
            }
        }
    }

    [HttpGet("{id}/transcript")]
    public async Task<IActionResult> GetStudentTranscriptById(string id)
    {
        using (_logger.BeginScope("GetStudentTranscriptById request for StudentId: {StudentId}", id))
        {
            // Load HTML template
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"transcript_template-{culture}.html");
            var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

            _logger.LogInformation("Fetching transcript for student with ID: {StudentId}", id);
            var transcriptStream = await _courseEnrollmentService.GetTranscriptOfStudentById(id, htmlTemplate);
            if (transcriptStream == null)
            {
                _logger.LogWarning("Transcript not found for student with ID: {StudentId}", id);
                return NotFound(new ErrorResponse<string>(status: 404, message: $"{_localizer["transcript_not_found"]}, ID: {id}"));
            }

            _logger.LogInformation("Transcript fetched successfully for student with ID: {StudentId}", id);

            return File(transcriptStream, "application/pdf", $"{id}_transcript.pdf");
        }
    }
}
