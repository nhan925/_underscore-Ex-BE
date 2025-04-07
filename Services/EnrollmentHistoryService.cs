using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class EnrollmentHistoryService : IEnrollmentHistoryService
{
    private readonly IEnrollmentHistoryRepository _enrollmentHistoryRepository;

    public EnrollmentHistoryService(IEnrollmentHistoryRepository enrollmentHistoryRepository)
    {
        _enrollmentHistoryRepository = enrollmentHistoryRepository;
    }

    public async Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId)
    {
        var enrollmentHistory = await _enrollmentHistoryRepository.GetEnrollmentHistoryBySemester(semesterId);

        return enrollmentHistory;
    }
}
