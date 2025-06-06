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

public class FacultyRepository : IFacultyRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _culture;
    private readonly string _cultureSuffix;
    private readonly IExternalTranslationService _translationService;

    public FacultyRepository(IDbConnection db, IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
        _translationService = translationService;
    }

    public async Task<List<Faculty>> GetAllFaculties()
    {
        string query = $"SELECT id, name{_cultureSuffix} AS name FROM faculties";
        var result = await _db.QueryAsync<Faculty>(query);
        return result.ToList();
    }

    public async Task<int> UpdateFaculty(Faculty faculty)
    {
        string query = $"UPDATE faculties SET name = @Name, name_vi = @NameVi, need_to_review = true WHERE id = @Id";
        var queryParams = new
        {
            Id = faculty.Id,
            Name = await _translationService.TranslateAsync(faculty.Name!, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(faculty.Name!, _culture, "vi"),
        };

        var count = await _db.ExecuteAsync(query, queryParams);
        if (count == 0)
        {
            throw new NotFoundException(_localizer["faculty_not_found"]);
        }

        return count;
    }

    public async Task<int> AddFaculty(string name)
    {
        string query = $"INSERT INTO faculties (name, name_vi, need_to_review) VALUES (@Name, @NameVi, true) RETURNING id";
        var queryParams = new
        {
            Name = await _translationService.TranslateAsync(name, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(name, _culture, "vi"),
        };

        var id = await _db.QueryFirstOrDefaultAsync<int>(query, queryParams);

        if (id == 0)
        {
            throw new OperationFailedException(_localizer["failed_to_add_faculty"]);
        }

        return id;
    }
}
