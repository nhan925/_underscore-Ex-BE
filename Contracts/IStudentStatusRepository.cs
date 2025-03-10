using student_management_api.Models.DTO;

namespace student_management_api.Contracts;

public interface IStudentStatusRepository
{
    Task<List<StudentStatus>> GetAllStudentStatuses();
}
