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

    public async Task<int> UpdateStudentStatus(StudentStatus studentStatus)
    {
        string query = "UPDATE student_statuses SET name = @Name WHERE id = @Id";
        var count = await _db.ExecuteAsync(query, studentStatus);
        if (count == 0)
        {
            throw new Exception("student status not found");
        }

        return count;
    }

    public async Task<int> AddStudentStatus(string name)
    {
        string query = "INSERT INTO student_statuses (name) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception("failed to add student status");
        }

        return id;
    }
}
