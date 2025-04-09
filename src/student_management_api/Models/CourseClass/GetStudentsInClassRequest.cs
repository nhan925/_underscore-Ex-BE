using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.CourseClass;

public class GetStudentsInClassRequest
{
    [Required]
    public string ClassId { get; set; }

    [Required]
    public string CourseId { get; set; }

    [Required]
    public int SemesterId { get; set; }
}
