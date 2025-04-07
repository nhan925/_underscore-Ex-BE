namespace student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

public interface IYearAndSemesterService
{
    Task<List<Year>> GetAllYears();

    Task<List<Semester>> GetSemestersByYear(int id);
}
