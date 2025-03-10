﻿using System.ComponentModel.DataAnnotations;

namespace student_management_api.Models.DTO;

public class Student
{
    public string Id { get; set; } // 8-digit student ID (e.g., 22010001)

    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public int? FacultyId { get; set; }

    public int? IntakeYear { get; set; }

    public string? Program { get; set; }

    public string? Address { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public int? StatusId { get; set; }
}
