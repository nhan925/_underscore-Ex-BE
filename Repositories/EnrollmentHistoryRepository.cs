using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class EnrollmentHistoryRepository : IEnrollmentHistoryRepository
{
    private readonly IDbConnection _db;

    public EnrollmentHistoryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId)
    {
        string sql = "SELECT * FROM enrollment_history WHERE semester_id = @SemesterId";
        var history = await _db.QueryAsync<EnrollmentHistory>(sql, new { SemesterId = semesterId });

        return history.ToList();
    }
}
