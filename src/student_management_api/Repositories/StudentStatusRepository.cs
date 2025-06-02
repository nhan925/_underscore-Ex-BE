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

public class StudentStatusRepository : IStudentStatusRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _culture;
    private readonly string _cultureSuffix;
    private readonly IExternalTranslationService _translationService;

    public StudentStatusRepository(IDbConnection db,IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
        _translationService = translationService;
    }

    public async Task<List<StudentStatus>> GetAllStudentStatuses()
    {
        string query = $"SELECT id, name{_cultureSuffix} AS name, is_referenced FROM student_statuses";
        var result = await _db.QueryAsync<StudentStatus>(query);
        return result.ToList();
    }

    public async Task<int> UpdateStudentStatus(StudentStatus studentStatus)
    {
        string query = $"UPDATE student_statuses SET name = @Name, name_vi = @NameVi, need_to_review = true WHERE id = @Id";
        var queryParams = new
        {
            Id = studentStatus.Id,
            Name = await _translationService.TranslateAsync(studentStatus.Name!, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(studentStatus.Name!, _culture, "vi"),
        };

        var count = await _db.ExecuteAsync(query, queryParams);
        if (count == 0)
        {
            throw new NotFoundException(_localizer["student_status_not_found"]);
        }

        return count;
    }

    public async Task<int> AddStudentStatus(string name)
    {
        string query = $"INSERT INTO student_statuses (name, name_vi, need_to_review) VALUES (@Name, @NameVi, true) RETURNING id";
        var queryParams = new
        {
            Name = await _translationService.TranslateAsync(name, _culture, "en"),
            NameVi = await _translationService.TranslateAsync(name, _culture, "vi"),
        };

        var id = await _db.QueryFirstOrDefaultAsync<int>(query, queryParams);

        if (id == 0)
        {
            throw new Exception(_localizer["failed_to_add_student_status"]);
        }

        return id;
    }

    // Check if the status is referenced in the configuration
    private async Task<bool> IsStatusReferencedAsync(int statusId)
    {
        var sql = "SELECT is_referenced FROM student_statuses WHERE id = @StatusId;";

        return await _db.QuerySingleAsync<bool>(sql, new { StatusId = statusId });
    }

    public async Task<int> ReferenceStudentStatuses(List<int> statusIds)
    {
        // Set all statuses to false first
        var sqlReset = "UPDATE student_statuses SET is_referenced = FALSE;";
        await _db.ExecuteAsync(sqlReset);

        // If there are statusIds to update, set them to true
        if (statusIds != null && statusIds.Count > 0)
        {
            var sqlUpdate = "UPDATE student_statuses SET is_referenced = TRUE WHERE id = ANY(@StatusIds);";
            return await _db.ExecuteAsync(sqlUpdate, new { StatusIds = statusIds.ToArray() });
        }

        return 0; // No updates needed if statusIds is empty
    }
}
