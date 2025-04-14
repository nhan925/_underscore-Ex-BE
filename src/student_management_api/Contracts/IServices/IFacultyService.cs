namespace student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

public interface IFacultyService
{
    Task<List<Faculty>> GetAllFaculties();

    Task<int> UpdateFaculty(Faculty faculty);

    Task<int> AddFaculty(string name);
}
