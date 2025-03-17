using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Services;

public class StudentService: IStudentService
{
    private readonly IStudentRepository _studentRepository;
    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<string> AddStudent(AddStudentRequest request)
    {
        var studentId = await _studentRepository.AddStudent(request);
        if (studentId == null)
        {
            throw new Exception("failed to add student");
        }

        return studentId;
    }

    public async Task<int> DeleteStudentById(string id)
    {
        var count = await _studentRepository.DeleteStudentById(id);
        if (count == 0)
        {
            throw new Exception("student not found");
        }

        return count;
    }

    public async Task<Student?> GetStudentById(string id)
    {
        var student = await _studentRepository.GetStudentById(id);
        if (student == null)
        {
            throw new Exception("student not found");
        }

        return student;
    }

    public async Task<PagedResult<SimplifiedStudent>> GetStudents(int page, int pageSize, string? search, StudentFilter? filter)
    {
        var result = await _studentRepository.GetStudents(page, pageSize, search, filter);
        return result;
    }

    public async Task<int> UpdateStudentById(string id, UpdateStudentRequest request)
    {
        var count = await _studentRepository.UpdateStudentById(id, request);
        if (count == 0)
        {
            throw new Exception("student not found");
        }

        return count;
    }
}
