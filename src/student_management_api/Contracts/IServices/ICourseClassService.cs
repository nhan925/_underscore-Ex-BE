using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface ICourseClassService
{
    Task<List<GetCourseClassResult>> GetAllCourseClassesBySemester(int semesterId);

    Task<string> AddCourseClass(CourseClass courseClass);

    Task<List<StudentInClass>> GetStudentsInClass(GetStudentsInClassRequest request);
}
