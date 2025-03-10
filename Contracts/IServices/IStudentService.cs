using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Contracts.IServices;

public interface IStudentService
{
    Task<int> DeleteStudentById(string id);

    Task<PagedResult<Student>> GetStudents(int page, int pageSize, string? search);

    Task<int> UpdateStudentById(string id, UpdateStudentRequest request);

    Task<string> AddStudent(AddStudentRequest request);

    Task<Student?> GetStudentById(string id);
}
