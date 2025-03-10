using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace student_management_api.Models.Student;

public class AddStudentRequest
{
    [Required]
    public string FullName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; }

    [Required]
    public int FacultyId { get; set; }

    [Required]
    public int IntakeYear { get; set; }

    [Required]
    public string Program { get; set; }

    [Required]
    public string Address { get; set; }

    [EmailAddress, Required]
    public string Email { get; set; }

    [Phone, Required]
    public string PhoneNumber { get; set; }

    [Required]
    public int StatusId { get; set; }
}
