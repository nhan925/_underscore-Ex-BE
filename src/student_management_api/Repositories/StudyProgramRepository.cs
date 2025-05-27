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

public class StudyProgramRepository : IStudyProgramRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _cultureSuffix;

    public StudyProgramRepository(IDbConnection db, IStringLocalizer<Messages> localizer)
    {
        _db = db;
        _localizer = localizer;
        _cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "" : $"_{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}";
    }

    public async Task<List<StudyProgram>> GetAllPrograms()
    {
        string query = $"SELECT id, name{_cultureSuffix} AS name FROM programs";
        var result = await _db.QueryAsync<StudyProgram>(query);
        return result.ToList();
    }

    public async Task<int> UpdateProgram(StudyProgram program)
    {
        string query = $"UPDATE programs SET name{_cultureSuffix} = @Name WHERE id = @Id";
        var count = await _db.ExecuteAsync(query, program);
        if (count == 0)
        {
            throw new NotFoundException(_localizer["program_not_found"]);
        }

        return count;
    }

    public async Task<int> AddProgram(string name)
    {
        string query = $"INSERT INTO programs (name{_cultureSuffix}) VALUES (@Name) RETURNING id";
        var id = await _db.QueryFirstOrDefaultAsync<int>(query, new { Name = name });

        if (id == 0)
        {
            throw new Exception(_localizer["failed_to_add_program"]);
        }

        return id;
    }
}
