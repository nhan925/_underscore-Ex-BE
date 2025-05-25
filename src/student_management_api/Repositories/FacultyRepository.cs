using Dapper;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Models.DTO;
using System.Data;
using student_management_api.Localization;

namespace student_management_api.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;

    public FacultyRepository(IDbConnection db, IStringLocalizer<Messages> localizer)
    {
        _db = db;
        _localizer = localizer; 
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
            throw new NotFoundException(_localizer["faculty_not_found"]);
        }

        return count;
    }

    public async Task<int> AddFaculty(string name)
    {
        string query = "INSERT INTO faculties (name) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception(_localizer["failed_to_add_faculty"]);
        }

        return id;
    }
}
