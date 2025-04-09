using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Contracts.IRepositories;

public interface ICourseEnrollmentRepository
{
    Task RegisterClass(CourseEnrollmentRequest request);

    Task UnregisterClass(CourseEnrollmentRequest request);

    Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId);

    Task<Transcript> GetTranscriptOfStudentById(string studentId);

    Task<int> UpdateStudentGrade(string studentId, string courseId, float grade);
}
