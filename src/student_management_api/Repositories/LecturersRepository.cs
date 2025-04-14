using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class LecturersRepository : ILecturersRepository
{
    private readonly IDbConnection _db;
    public LecturersRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Lecturer>> GetAllLecturers()
    {
        string sql = "SELECT * FROM lecturers";
        var lecturers = await _db.QueryAsync<Lecturer>(sql);
        
        return lecturers.ToList();
    }
}
