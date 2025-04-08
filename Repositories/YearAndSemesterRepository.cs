using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class YearAndSemesterRepository : IYearAndSemesterRepository
{
    private readonly IDbConnection _db;
    public YearAndSemesterRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Year>> GetAllYears()
    {
        var query = "SELECT * FROM years";
        var result = await _db.QueryAsync<Year>(query);
        return result.ToList();
    }

    public async Task<List<Semester>> GetSemestersByYear(int yearId)
    {
        var query = "SELECT * FROM semesters WHERE semesters.year_id = @YearId";
        var result = await _db.QueryAsync<Semester>(query, new { YearId = yearId });
        return result.ToList();
    }
}
