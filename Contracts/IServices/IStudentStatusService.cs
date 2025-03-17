namespace student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

public interface IStudentStatusService
{
    Task<List<StudentStatus>> GetAllStudentStatuses();

    Task<int> UpdateStudentStatus(StudentStatus studentStatus);

    Task<int> AddStudentStatus(string name);
}
