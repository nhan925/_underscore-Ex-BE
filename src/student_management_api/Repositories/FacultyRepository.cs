using Dapper;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Localization;
using student_management_api.Models.DTO;
using System.Data;
using System.Globalization;

namespace student_management_api.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _cultureSuffix;

    public FacultyRepository(IDbConnection db, IStringLocalizer<Messages> localizer)
    {
        _db = db;
        _localizer = localizer;
        _cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "" : $"_{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}";
    }

    public async Task<List<Faculty>> GetAllFaculties()
    {
        string query = $"SELECT id, name{_cultureSuffix} AS name FROM faculties";
        var result = await _db.QueryAsync<Faculty>(query);
        return result.ToList();
    }

    public async Task<int> UpdateFaculty(Faculty faculty)
    {
        string query = $"UPDATE faculties SET name{_cultureSuffix} = @Name WHERE id = @Id";
        var count = await _db.ExecuteAsync(query, faculty);
        if (count == 0)
        {
            throw new NotFoundException(_localizer["faculty_not_found"]);
        }

        return count;
    }

    public async Task<int> AddFaculty(string name)
    {
        string query = $"INSERT INTO faculties (name{_cultureSuffix}) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception(_localizer["failed_to_add_faculty"]);
        }

        return id;
    }
}
