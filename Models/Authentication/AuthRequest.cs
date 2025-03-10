using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.Authentication;

public class AuthRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
