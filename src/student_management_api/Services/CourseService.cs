using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
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
        if (DateTime.Now.Subtract(course.CreatedAt).Minutes > 30)
        {
            throw new Exception("Đã quá 30 phút, không thể xóa!");
        }
        return await _courseRepository.DeleteCourseById(id);
    }

    public async Task<bool> CheckStudentExistFromCourse(string id)
    {
        return await _courseRepository.CheckStudentExistFromCourse(id);
    }
}