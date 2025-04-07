using System.ComponentModel.DataAnnotations;


namespace student_management_api.Models.DTO;

public class GetCourseClassRequest
{
    [Required]
    public string Id { get; set; }

    [Required]
    public Course Course { get; set; }

    [Required]
    public Semester Semester { get; set; }

    [Required]
    public Lecturer Lecturer { get; set; }

    [Required]
    public int MaxStudents { get; set; }

    [Required]
    public string Schedule { get; set; }

    [Required]
    public string Room { get; set; }
}
