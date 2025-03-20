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

    public async Task<int> UpdateFaculty(Faculty faculty)
    {
        string query = "UPDATE faculties SET name = @Name WHERE id = @Id";
        var count = await _db.ExecuteAsync(query, faculty);
        if (count == 0)
        {
            throw new Exception("faculty not found");
        }

        return count;
    }

    public async Task<int> AddFaculty(string name)
    {
        string query = "INSERT INTO faculties (name) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception("Failed to add faculty");
        }

        return id;
    }
}
