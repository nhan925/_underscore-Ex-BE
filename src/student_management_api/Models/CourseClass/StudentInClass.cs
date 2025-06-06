using DocumentFormat.OpenXml.Wordprocessing;

namespace student_management_api.Models.CourseClass;

public class StudentInClass
{
    public string? Id { get; set; }

    public string? FullName { get; set; }

    public float? Grade { get; set; }

    public string? Status { get; set; }
}
