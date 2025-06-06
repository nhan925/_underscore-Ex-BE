namespace student_management_api.Models.DTO;

public class EnrollmentHistory
{
    public string? StudentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CourseId { get; set; }

    public string? ClassId { get; set; }

    public int SemesterId { get; set; }

    public string? Action { get; set; }
}
