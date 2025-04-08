using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface ICourseService
{
    Task<List<Course>> GetAllCourses();

    Task<Course> GetCourseById(string Id);

    Task<int> UpdateCourseById(Course course);

    Task<int> AddCourse(Course course);

    Task<string> DeleteCourseById(string id);

    Task<bool> CheckStudentExistFromCourse(string id);
}
