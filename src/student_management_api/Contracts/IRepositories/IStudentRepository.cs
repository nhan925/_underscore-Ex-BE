using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Contracts.IRepositories;

public interface IStudentRepository
{
    Task<Student?> GetStudentById(string id);

    Task<int> UpdateStudentById(string id, UpdateStudentRequest newStudent);

    Task<int> DeleteStudentById(string id);

    Task<PagedResult<SimplifiedStudent>> GetStudents(int page, int pageSize, string? search, StudentFilter? filter);

    Task<string> AddStudent(AddStudentRequest request);

    Task AddStudents(List<AddStudentRequest> requests);

    Task<List<Student>> GetAllStudents();
}
