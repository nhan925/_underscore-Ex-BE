using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface IEnrollmentHistoryService
{
    Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId);
}
