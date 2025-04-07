using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class CourseEnrollmentService : ICourseEnrollmentService
{
    private readonly ICourseEnrollmentRepository _courseEnrollmentRepository;

    public CourseEnrollmentService(ICourseEnrollmentRepository courseEnrollmentRepository)
    {
        _courseEnrollmentRepository = courseEnrollmentRepository;
    }

    public async Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId)
    {
        var history = await _courseEnrollmentRepository.GetEnrollmentHistoryBySemester(semesterId);
        return history;
    }

    public async Task RegisterClass(CourseEnrollmentRequest request) =>
        await _courseEnrollmentRepository.RegisterClass(request);

    public async Task UnregisterClass(CourseEnrollmentRequest request) =>
        await _courseEnrollmentRepository.UnregisterClass(request);
}
