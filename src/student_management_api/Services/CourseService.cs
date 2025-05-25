using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Exceptions;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
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
            throw new ForbiddenException("The course has been deleted, you cannot delete it again");
        }


        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var createdVietNamTime = TimeZoneInfo.ConvertTimeFromUtc(course.CreatedAt, vietnamTimeZone);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        var timeDifference = timeNow.Subtract(createdVietNamTime).TotalMinutes;

        if (timeDifference > 30)
        {
            throw new ForbiddenException("Exceeded 30 minutes, cannot delete");
        }

        return await _courseRepository.DeleteCourseById(id);
    }

    public async Task<bool> CheckStudentExistFromCourse(string id)
    {
        return await _courseRepository.CheckStudentExistFromCourse(id);
    }
}