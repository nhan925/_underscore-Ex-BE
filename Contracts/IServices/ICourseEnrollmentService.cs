using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface ICourseEnrollmentService
{
    Task RegisterClass(CourseEnrollmentRequest request);

    Task UnregisterClass(CourseEnrollmentRequest request);

    Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId);
}
