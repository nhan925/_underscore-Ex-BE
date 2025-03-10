using student_management_api.Models.DTO;

namespace student_management_api.Contracts;

public interface IFacultyRepository
{
    Task<List<Faculty>> GetAllFaculties();
}
