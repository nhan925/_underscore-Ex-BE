using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;
using student_management_api.Resources;

namespace student_management_api.Services;

public class CourseEnrollmentService : ICourseEnrollmentService
{
    private readonly ICourseEnrollmentRepository _courseEnrollmentRepository;

    private readonly IStudentRepository _studentRepository;

    private readonly IConverter _pdfConverter;

    private readonly IStringLocalizer<Messages> _localizer;

    public CourseEnrollmentService(ICourseEnrollmentRepository courseEnrollmentRepository, IStudentRepository studentRepository, IConverter converter, IStringLocalizer<Messages> localizer)
    {
        _courseEnrollmentRepository = courseEnrollmentRepository;
        _studentRepository = studentRepository;
        _pdfConverter = converter;
        _localizer = localizer;
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
            throw new NotFoundException(_localizer["student_not_found"]);
        }
        
        var transcript = await _courseEnrollmentRepository.GetTranscriptOfStudentById(studentId);

        // Prepare course rows
        var courseRows = string.Join("\n", transcript.Courses!.Select(c =>
            $"<tr><td>{c.Id}</td><td>{c.Name}</td><td>{c.Credits}</td><td>{c.Grade}</td></tr>"
        ));

        // Fill placeholders
        var htmlContent = htmlTemplate
            .Replace("{{school_name}}", _localizer["tkpm_university"])
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
            throw new NotFoundException(_localizer["failed_to_update_student_grade"]);
        }
    }
}
