﻿using student_management_api.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.Student;

public class UpdateStudentRequest
{
    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public int? FacultyId { get; set; }

    public int? IntakeYear { get; set; }

    public int? ProgramId { get; set; }

    public List<Address>? Addresses { get; set; }

    public IdentityInfo? IdentityInfo { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public int? StatusId { get; set; }

    public string? Nationality { get; set; }
}
