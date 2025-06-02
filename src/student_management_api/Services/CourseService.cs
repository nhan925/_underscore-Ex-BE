using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Models.DTO;
using student_management_api.Resources;

namespace student_management_api.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    private readonly IStringLocalizer<Messages> _localizer;

    public CourseService(ICourseRepository courseRepository, IStringLocalizer<Messages> localizer)
    {
        _courseRepository = courseRepository;
        _localizer = localizer;
    }

    public async Task<List<Course>> GetAllCourses()
    {
        var courses = await _courseRepository.GetAllCourses();
        return courses ?? new();
    }

    public async Task<Course> GetCourseById(string Id)
    {
        var course = await _courseRepository.GetCourseById(Id);
        return course ?? new();
    }

    public async Task<int> UpdateCourseById(Course course)
    {
        return await _courseRepository.UpdateCourseById(course);
    }

    public async Task<int> AddCourse(Course course)
    {
        return await _courseRepository.AddCourse(course);
    }

    public async Task<string> DeleteCourseById(string id)
    {
        var course = await _courseRepository.GetCourseById(id);

        if (!course.IsActive)
        {
            throw new ForbiddenException(_localizer["course_has_been_deleted"]);
        }


        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var createdVietNamTime = TimeZoneInfo.ConvertTimeFromUtc(course.CreatedAt, vietnamTimeZone);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        var timeDifference = timeNow.Subtract(createdVietNamTime).TotalMinutes;

        if (timeDifference > 30)
        {
            throw new ForbiddenException(_localizer["exceeded_30_minutes_cannot_delete"]);
        }

        return await _courseRepository.DeleteCourseById(id);
    }

    public async Task<bool> CheckStudentExistFromCourse(string id)
    {
        return await _courseRepository.CheckStudentExistFromCourse(id);
    }
}