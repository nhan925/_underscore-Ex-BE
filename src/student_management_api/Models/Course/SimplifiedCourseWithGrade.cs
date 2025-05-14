using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.Course;

public class SimplifiedCourseWithGrade
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public int Credits { get; set; }

    public float Grade { get; set; }
}
