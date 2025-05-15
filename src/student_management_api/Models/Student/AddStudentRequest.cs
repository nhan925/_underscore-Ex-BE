using student_management_api.Models.DTO;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace student_management_api.Models.Student;

public class AddStudentRequest
{
    [Required]
    public string? FullName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string? Gender { get; set; }

    [Required]
    public int FacultyId { get; set; }

    [Required]
    public int IntakeYear { get; set; }

    [Required]
    public int ProgramId { get; set; }

    [Required]
    public List<Address>? Addresses { get; set; }

    [Required]
    public IdentityInfo? IdentityInfo { get; set; }

    [EmailAddress, Required]
    public string? Email { get; set; }

    [Phone, Required]
    public string? PhoneNumber { get; set; }

    [Required]
    public int StatusId { get; set; }

    [Required]
    public string? Nationality { get; set; }
}
