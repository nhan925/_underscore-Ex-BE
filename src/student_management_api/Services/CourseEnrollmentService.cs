using DinkToPdf;
using DinkToPdf.Contracts;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class CourseEnrollmentService : ICourseEnrollmentService
{
    private readonly ICourseEnrollmentRepository _courseEnrollmentRepository;

    private readonly IStudentRepository _studentRepository;

    private readonly IConverter _pdfConverter;

    public CourseEnrollmentService(ICourseEnrollmentRepository courseEnrollmentRepository, IStudentRepository studentRepository, IConverter converter)
    {
        _courseEnrollmentRepository = courseEnrollmentRepository;
        _studentRepository = studentRepository;
        _pdfConverter = converter;
    }

    public async Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId)
    {
        var history = await _courseEnrollmentRepository.GetEnrollmentHistoryBySemester(semesterId);
        return history;
    }

    public async Task<Stream> GetTranscriptOfStudentById(string studentId, string htmlTemplate)
    {
        var student = await _studentRepository.GetStudentById(studentId);
        if (student == null)
        {
            throw new Exception("student not found");
        }
        
        var transcript = await _courseEnrollmentRepository.GetTranscriptOfStudentById(studentId);

        // Prepare course rows
        var courseRows = string.Join("\n", transcript.Courses!.Select(c =>
            $"<tr><td>{c.Id}</td><td>{c.Name}</td><td>{c.Credits}</td><td>{c.Grade}</td></tr>"
        ));

        // Fill placeholders
        var htmlContent = htmlTemplate
            .Replace("{{school_name}}", "Trường Đại học TKPM")
            .Replace("{{student_name}}", student.FullName)
            .Replace("{{student_id}}", student.Id)
            .Replace("{{intake_year}}", student.IntakeYear.ToString())
            .Replace("{{dob}}", student.DateOfBirth!.Value.ToString("d"))
            .Replace("{{course_rows}}", courseRows)
            .Replace("{{total_credits}}", transcript.TotalCredits.ToString())
            .Replace("{{gpa}}", transcript.GPA.ToString("0.00"));

        // Generate PDF
        var pdf = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings { Top = 5, Bottom = 5, Left = 5, Right = 5 }
            },
            Objects = {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8", LoadImages = true }
                }
            }
        };

        byte[] pdfBytes = _pdfConverter.Convert(pdf);

        return new MemoryStream(pdfBytes);
    }

    public async Task RegisterClass(CourseEnrollmentRequest request) =>
        await _courseEnrollmentRepository.RegisterClass(request);

    public async Task UnregisterClass(CourseEnrollmentRequest request) =>
        await _courseEnrollmentRepository.UnregisterClass(request);

    public async Task UpdateStudentGrade(string studentId, string courseId, float? grade)
    {
        var affectedRows = await _courseEnrollmentRepository.UpdateStudentGrade(studentId, courseId, grade);
        if (affectedRows == 0)
        {
            throw new Exception("Failed to update student grade or Student not found");
        }
    }
}
