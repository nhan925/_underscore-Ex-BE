using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly IDbConnection _db;
    public FacultyRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Faculty>> GetAllFaculties()
    {
        string query = "SELECT * FROM faculties";
        var result = await _db.QueryAsync<Faculty>(query);
        return result.ToList();
    }
}
