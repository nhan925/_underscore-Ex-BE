using Dapper;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Resources;
using student_management_api.Models.DTO;
using System.Data;
using System.Globalization;
using student_management_api.Localization;

namespace student_management_api.Repositories;

public class StudyProgramRepository : IStudyProgramRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _culture;
    private readonly string _cultureSuffix;
    private readonly IExternalTranslationService _translationService;

    public StudyProgramRepository(IDbConnection db, IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
        _translationService = translationService;
    }

    public async Task<List<StudyProgram>> GetAllPrograms()
    {
        string query = $"SELECT id, name{_cultureSuffix} AS name FROM programs";
        var result = await _db.QueryAsync<StudyProgram>(query);
        return result.ToList();
    }

    public async Task<int> UpdateProgram(StudyProgram program)
    {
        string query = $"UPDATE programs SET name = @Name, name_vi = @NameVi, need_to_review = true WHERE id = @Id";
        var queryParams = new
        {
            Id = program.Id,
            Name = await _translationService.TranslateAsync(program.Name!, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(program.Name!, _culture, "vi"),
        };

        var count = await _db.ExecuteAsync(query, queryParams);
        if (count == 0)
        {
            throw new NotFoundException(_localizer["program_not_found"]);
        }

        return count;
    }

    public async Task<int> AddProgram(string name)
    {
        string query = $"INSERT INTO programs (name, name_vi, need_to_review) VALUES (@Name, @NameVi, true) RETURNING id";
        var queryParams = new
        {
            Name = await _translationService.TranslateAsync(name, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(name, _culture, "vi"),
        };

        var id = await _db.QueryFirstOrDefaultAsync<int>(query, queryParams);

        if (id == 0)
        {
            throw new Exception(_localizer["failed_to_add_program"]);
        }

        return id;
    }
}
