using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class StudentStatusRepository : IStudentStatusRepository
{
    private readonly IDbConnection _db;
    public StudentStatusRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<StudentStatus>> GetAllStudentStatuses()
    {
        string query = "SELECT * FROM student_statuses";
        var result = await _db.QueryAsync<StudentStatus>(query);
        return result.ToList();
    }
}
