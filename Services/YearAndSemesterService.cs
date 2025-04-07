using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class YearAndSemesterService : IYearAndSemesterService
{
    private readonly IYearAndSemesterRepository _yearAndSemesterRepository;
    public YearAndSemesterService(IYearAndSemesterRepository yearAndSemesterRepository)
    {
        _yearAndSemesterRepository = yearAndSemesterRepository;
    }

    public async Task<List<Year>> GetAllYears()
    {
        var years = await _yearAndSemesterRepository.GetAllYears();
        return years ?? new();
    }

    public async Task<List<Semester>> GetSemestersByYear(int id)
    {
        var semesters = await _yearAndSemesterRepository.GetSemestersByYear(id);
        return semesters ?? new();
    }
}
