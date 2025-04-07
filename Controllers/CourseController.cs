﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Services;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/course")]
[Authorize]


public class CourseController: Controller
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CourseController> _logger;

    public CourseController(ICourseService courseService, ILogger<CourseController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        using (_logger.BeginScope("GetAllCourses request"))
        {
            _logger.LogInformation("Fetching all courses");

            var courses = await _courseService.GetAllCourses();

            _logger.LogInformation("Successfully retrieved {Count} courses", courses.Count());
            return Ok(courses);
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(string id)
    {
        using (_logger.BeginScope("GetCourseById request - CourseId: {CourseId}", id))
        {
            _logger.LogInformation("Fetching course with ID: {CourseId}", id);

            var course = await _courseService.GetCourseById(id);

            if (course == null || string.IsNullOrEmpty(course.Id))
            {
                _logger.LogWarning("Course not found with ID: {CourseId}", id);
                return NotFound(new { Message = $"Course with ID {id} not found." });
            }

            _logger.LogInformation("Successfully retrieved course with ID: {CourseId}", id);
            return Ok(course);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourseById(string id, [FromBody] Course course)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (_logger.BeginScope("UpdateCourseById request - CourseId: {CourseId}", id))
        {
            if (id != course.Id)
            {
                _logger.LogWarning("Course ID in route does not match ID in body");
                return BadRequest(new { Message = "Course ID mismatch between route and body." });
            }

            // Kiểm tra nếu khóa học đã có sinh viên đăng ký
            var hasStudents = await _courseService.CheckStudentExistFromCourse(id);
            if (hasStudents)
            {
                _logger.LogWarning("Cannot update course with ID: {CourseId} because students are enrolled.", id);
                return BadRequest(new { Message = $"Course with ID {id} cannot be updated because students are enrolled." });
            }

            _logger.LogInformation("Updating course with ID: {CourseId}", id);

            var affectedRows = await _courseService.UpdateCourseById(id, course);

            if (affectedRows == 0)
            {
                _logger.LogWarning("Update failed or course not found with ID: {CourseId}", id);
                return NotFound(new { Message = $"Course with ID {id} not found or no change made." });
            }

            _logger.LogInformation("Successfully updated course with ID: {CourseId}", id);
            return Ok(affectedRows); // Trả về số dòng bị ảnh hưởng
        }
    }

    [HttpGet("{id}/has-students")]
    public async Task<IActionResult> CheckStudentExistFromCourse(string id)
    {
        using (_logger.BeginScope("CheckStudentExistFromCourse request - CourseId: {CourseId}", id))
        {
            _logger.LogInformation("Checking if students are enrolled in course: {CourseId}", id);

            var hasStudents = await _courseService.CheckStudentExistFromCourse(id);

            _logger.LogInformation("Course {CourseId} has students: {HasStudents}", id, hasStudents);

            return Ok(hasStudents); // Trả về true hoặc false
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCourse([FromBody] Course course)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using (_logger.BeginScope("AddCourse request - CourseId: {CourseId}", course.Id))
        {
            try
            {
                _logger.LogInformation("Attempting to add course with ID: {CourseId}", course.Id);

                var affectedRows = await _courseService.AddCourse(course);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Successfully added course with ID: {CourseId}", course.Id);
                    return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course); // 201 Created
                }
                else
                {
                    _logger.LogWarning("Course creation failed for ID: {CourseId}", course.Id);
                    return StatusCode(500, new { Message = "Failed to create course." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding course with ID: {CourseId}", course.Id);
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseById(string id)
    {
        using (_logger.BeginScope("DeleteCourseById request - CourseId: {CourseId}", id))
        {
            _logger.LogInformation("Processing request to delete course with ID: {CourseId}", id);

            try
            {
                var resultMessage = await _courseService.DeleteCourseById(id);

                _logger.LogInformation("Result of course deletion: {Message}", resultMessage);

                return Ok(new { Message = resultMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course with ID: {CourseId}", id);
                return Ok(new { Message = "Lỗi khi xóa khóa học.", Details = ex.Message });
            }
        }
    }
}
