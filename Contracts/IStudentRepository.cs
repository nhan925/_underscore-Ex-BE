using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Contracts;

public interface IStudentRepository
{
    Task<Student?> GetStudentById(string id);

    Task<int> UpdateStudentById(string id, UpdateStudentRequest newStudent);

    Task<int> DeleteStudentById(string id);

    Task<PagedResult<Student>> GetStudents(int page, int pageSize, string? search);

    Task<string> AddStudent(AddStudentRequest request);
}
