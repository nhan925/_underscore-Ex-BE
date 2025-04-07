using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllCourses();

    Task<Course> GetCourseById(string id);

    Task<int> UpdateCourseById(string id, Course course);

    Task<int> AddCourse(Course course);

    Task<string> DeleteCourseById(string id);

    Task<bool> CheckStudentExistFromCourse(string id);
}
