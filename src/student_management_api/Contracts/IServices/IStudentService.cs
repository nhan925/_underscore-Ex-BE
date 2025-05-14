using student_management_api.Models.DTO;
using student_management_api.Models.Student;

namespace student_management_api.Contracts.IServices;

public interface IStudentService
{
    Task<int> DeleteStudentById(string id);

    Task<PagedResult<SimplifiedStudent>> GetStudents(int page, int pageSize, string? search, StudentFilter? filter);

    Task<int> UpdateStudentById(string id, UpdateStudentRequest request);

    Task<string> AddStudent(AddStudentRequest request);

    Task<Student?> GetStudentById(string id);

    Task AddStudents(List<AddStudentRequest> requests);

    Task<Stream> ExportToExcel();

    Task<Stream> ExportToJson();

    string ConvertExcelToJson(Stream fileStream);
}
