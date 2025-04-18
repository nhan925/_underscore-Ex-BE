﻿using student_management_api.Models.DTO;
using System.ComponentModel.DataAnnotations;


namespace student_management_api.Models.CourseClass;

public class GetCourseClassResult
{
    [Required]
    public string Id { get; set; }

    [Required]
    public DTO.Course Course { get; set; }

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
