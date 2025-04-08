using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface ICourseClassRepository
{
    Task<List<GetCourseClassResult>> GetAllCourseClassesBySemester(int semesterId);

    Task<string> AddCourseClass(CourseClass courseClass);
}
