using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.CourseEnrollment;

public class CourseEnrollmentRequest
{
    [Required]
    public string StudentId { get; set; }

    [Required]
    public string ClassId { get; set; }

    [Required]
    public string CourseId { get; set; }

    [Required]
    public int SemesterId { get; set; }
}
