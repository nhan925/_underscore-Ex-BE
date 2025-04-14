using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Repositories;

namespace student_management_api.Services;

public class StudentStatusService: IStudentStatusService
{
    private readonly IStudentStatusRepository _studentStatusRepository;
    public StudentStatusService(IStudentStatusRepository studentStatusRepository)
    {
        _studentStatusRepository = studentStatusRepository;
    }

    public async Task<List<StudentStatus>> GetAllStudentStatuses()
    {
        var studentStatuses = await _studentStatusRepository.GetAllStudentStatuses();
        return studentStatuses ?? new();
    }

    public async Task<int> UpdateStudentStatus(StudentStatus studentStatus)
    {
        return await _studentStatusRepository.UpdateStudentStatus(studentStatus);
    }

    public async Task<int> AddStudentStatus(string name)
    {
        return await _studentStatusRepository.AddStudentStatus(name);
    }
}
