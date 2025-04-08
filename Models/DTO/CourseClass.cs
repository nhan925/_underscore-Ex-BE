using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.CourseClass;

public class CourseClass
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string CourseId { get; set; }

    [Required]
    public int SemesterId { get; set; }

    [Required]
    public string LecturerId { get; set; }

    [Required]
    public int MaxStudents { get; set; }

    [Required]
    public string Schedule { get; set; }

    [Required]
    public string Room { get; set; }
}