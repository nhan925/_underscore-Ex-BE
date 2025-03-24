using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IStudentStatusRepository
{
    Task<List<StudentStatus>> GetAllStudentStatuses();

    Task<int> UpdateStudentStatus(StudentStatus studentStatus);

    Task<int> AddStudentStatus(string name);

    // Update all student statuses that are referenced in the configuration, others are set to false
    Task<int> ReferenceStudentStatuses(List<int> statusIds);
}
