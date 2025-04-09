using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class CourseClassService : ICourseClassService
{
    private readonly ICourseClassRepository _courseClassRepository;
    public CourseClassService(ICourseClassRepository courseClassRepository)
    {
        _courseClassRepository = courseClassRepository;
    }

    public async Task<string> AddCourseClass(CourseClass courseClass)
    {
        return await _courseClassRepository.AddCourseClass(courseClass);
    }

    public async Task<List<GetCourseClassResult>> GetAllCourseClassesBySemester(int semesterId)
    {
        var courseClasses = await _courseClassRepository.GetAllCourseClassesBySemester(semesterId);
        return courseClasses ?? new();
    }

    public async Task<List<StudentInClass>> GetStudentsInClass(GetStudentsInClassRequest request)
    {
        var students = await _courseClassRepository.GetStudentsInClass(request);
        return students ?? new();
    }
}
