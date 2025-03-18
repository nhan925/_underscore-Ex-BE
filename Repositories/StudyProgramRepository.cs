using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class StudyProgramRepository : IStudyProgramRepository
{
    private readonly IDbConnection _db;
    public StudyProgramRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<StudyProgram>> GetAllPrograms()
    {
        string query = "SELECT * FROM programs";
        var result = await _db.QueryAsync<StudyProgram>(query);
        return result.ToList();
    }

    public async Task<int> UpdateProgram(StudyProgram program)
    {
        string query = "UPDATE programs SET name = @Name WHERE id = @Id";
        var count = await _db.ExecuteAsync(query, program);
        if (count == 0)
        {
            throw new Exception("program not found");
        }

        return count;
    }

    public async Task<int> AddProgram(string name)
    {
        string query = "INSERT INTO programs (name) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception("failed to add program");
        }

        return id;
    }
}
