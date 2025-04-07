namespace student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;

public interface IYearAndSemesterRepository
{
    Task<List<Year>> GetAllYears();

    Task<List<Semester>> GetSemestersByYear(int id);
}
