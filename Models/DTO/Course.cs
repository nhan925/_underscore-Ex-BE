using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.DTO;

public class Course
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int Credits { get; set; }

    [Required]
    public int FacultyId { get; set; }

    [Required]
    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<string> PrerequisitesId { get; set; }

    public bool IsActive { get; set; } = true;
}