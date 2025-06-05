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

public class CourseRepository: ICourseRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _culture;
    private readonly string _cultureSuffix;
    private readonly IExternalTranslationService _translationService;

    public CourseRepository(IDbConnection db, IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
        _translationService = translationService;
    }

    public async Task<List<Course>> GetAllCourses()
    {
        var sql = @$"
        SELECT 
            c.id, c.name{_cultureSuffix} AS name, c.credits, c.faculty_id,
            c.description{_cultureSuffix} AS description, c.created_at,
            c.is_active,
            cp.prerequisite_id
        FROM courses c
        LEFT JOIN course_prerequisites cp 
            ON c.id = cp.course_id;
        ";

        var courseDict = new Dictionary<string, Course>();

        await _db.QueryAsync<Course, string, Course>(
            sql,
            (course, prereqId) =>
            {
                if (!courseDict.TryGetValue(course.Id!, out var existing))
                {
                    existing = course;
                    existing.PrerequisitesId = new List<string>();
                    courseDict.Add(existing.Id!, existing);
                }

                if (!string.IsNullOrEmpty(prereqId))
                {
                    existing.PrerequisitesId!.Add(prereqId);
                }

                return existing;
            },
            splitOn: "prerequisite_id"
        );

        return courseDict.Values.ToList();
    }

    public async Task<Course> GetCourseById(string id)
    {
        var sql = @$"
        SELECT
            c.id,
            c.name{_cultureSuffix} AS name,
            c.credits,
            c.faculty_id,
            c.description{_cultureSuffix} AS description,
            c.created_at,
            c.is_active,
            cp.prerequisite_id
        FROM courses c
        LEFT JOIN course_prerequisites cp 
            ON c.id = cp.course_id
        WHERE c.id = @Id;
        ";

        var courseDict = new Dictionary<string, Course>();

        await _db.QueryAsync<Course, string, Course>(
            sql,
            (course, prereqId) =>
            {
                if (!courseDict.TryGetValue(course.Id!, out var existing))
                {
                    existing = course;
                    existing.PrerequisitesId = new List<string>();
                    courseDict.Add(existing.Id!, existing);
                }

                if (!string.IsNullOrEmpty(prereqId))
                    existing.PrerequisitesId!.Add(prereqId);

                return existing;
            },
            new { Id = id },
            splitOn: "prerequisite_id"
        );

        var course = courseDict.Values.FirstOrDefault();
        if (course == null)
        {
            throw new NotFoundException($"{_localizer["course_not_found"]}, ID: {id}");
        }

        return course;
    }

    public async Task<int> UpdateCourseById(Course course)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        // Lấy thông tin khóa học hiện tại để kiểm tra credits
        var currentCourseSql = "SELECT credits FROM courses WHERE id = @Id";
        var currentCredits = await _db.ExecuteScalarAsync<int>(currentCourseSql, new { Id = course.Id });

        // Kiểm tra xem có sinh viên đăng ký và credits có thay đổi không
        var hasStudents = await CheckStudentExistFromCourse(course.Id!);
        if (hasStudents && currentCredits != course.Credits)
        {
            throw new ForbiddenException(_localizer["cannot_change_credits_for_a_course_that_has_students_enrolled"]);
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                // 1. Cập nhật bảng courses
                var updateSql = @$"
                UPDATE courses
                SET 
                    name                        = @Name,
                    name_vi                     = @NameVi,    
                    credits                     = @Credits,
                    faculty_id                  = @FacultyId,
                    description                 = @Description,
                    description_vi              = @DescriptionVi,
                    need_to_review              = @NeedToReview
                WHERE id = @Id;
                ";
                var updateParams = new
                {
                    Id = course.Id,
                    Name = await _translationService.TranslateAsync(course.Name!, _culture, "en"),
                    NameVi = await _translationService.TranslateAsync(course.Name!, _culture, "vi"),
                    Credits = course.Credits,
                    FacultyId = course.FacultyId,
                    Description = await _translationService.TranslateAsync(course.Description!, _culture, "en"),
                    DescriptionVi = await _translationService.TranslateAsync(course.Description!, _culture, "vi"),
                    NeedToReview = true,
                };
                var rowsAffected = await _db.ExecuteAsync(updateSql, updateParams, transaction);

                // 2. Xóa hết các prerequisite cũ
                var deleteSql = "DELETE FROM course_prerequisites WHERE course_id = @Id;";
                await _db.ExecuteAsync(deleteSql, new { course.Id }, transaction);

                // 3. Thêm lại các prerequisite mới (nếu có)
                if (course.PrerequisitesId?.Any() == true)
                {
                    var insertSql = @"
                INSERT INTO course_prerequisites (course_id, prerequisite_id) 
                VALUES (@CourseId, @PrerequisiteId);
                ";
                    var insertParams = course.PrerequisitesId
                        .Select(pid => new { CourseId = course.Id, PrerequisiteId = pid });
                    await _db.ExecuteAsync(insertSql, insertParams, transaction);
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public async Task<int> AddCourse(Course course)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                // 1. Kiểm tra Prerequisites có tồn tại trong bảng courses không
                if (course.PrerequisitesId?.Any() == true)
                {
                    foreach (var prerequisiteId in course.PrerequisitesId)
                    {
                        var checkSql = @"SELECT COUNT(*) FROM courses WHERE id = @Id";

                        var exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Id = prerequisiteId }, transaction);

                        if (exists == 0)
                        {
                            throw new NotFoundException(_localizer["prerequisite_course_not_exists"]);
                        }
                    }
                }

                // 2. Thêm vào bảng courses
                var insertCourseSql = @$"
                INSERT INTO courses (id, name, credits, faculty_id, description, name_vi, description_vi, need_to_review)
                VALUES (@Id, @Name, @Credits, @FacultyId, @Description, @NameVi, @DescriptionVi, @NeedToReview);
                ";

                var courseParams = new
                {
                    Id = course.Id,
                    Name = await _translationService.TranslateAsync(course.Name!, _culture, "en"),
                    NameVi = await _translationService.TranslateAsync(course.Name!, _culture, "vi"),
                    Credits = course.Credits,
                    FacultyId = course.FacultyId,
                    Description = await _translationService.TranslateAsync(course.Description!, _culture, "en"),
                    DescriptionVi = await _translationService.TranslateAsync(course.Description!, _culture, "vi"),
                    NeedToReview = true,
                };

                var rowsAffected = await _db.ExecuteAsync(insertCourseSql, courseParams, transaction);

                // 3. Thêm các prerequisite (nếu có)
                if (course.PrerequisitesId?.Any() == true)
                {
                    var insertPrereqSql = @"
                    INSERT INTO course_prerequisites (course_id, prerequisite_id)
                    VALUES (@CourseId, @PrerequisiteId);
                    ";

                    var insertParams = course.PrerequisitesId
                        .Select(pid => new { CourseId = course.Id, PrerequisiteId = pid });

                    await _db.ExecuteAsync(insertPrereqSql, insertParams, transaction);
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public async Task<string> DeleteCourseById(string id)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using var transaction = _db.BeginTransaction();
        try
        {
            // 1. Kiểm tra nếu có class thuộc course này
            var classCheckSql = "SELECT COUNT(*) FROM classes WHERE course_id = @Id";
            var classCount = await _db.ExecuteScalarAsync<int>(classCheckSql, new { Id = id }, transaction);

            if (classCount > 0)
            {
                var deactivateSql = "UPDATE courses SET is_active = false WHERE id = @Id";
                await _db.ExecuteAsync(deactivateSql, new { Id = id }, transaction);

                transaction.Commit();
                return _localizer["classes_exist_for_this_course_The_course_has_been_marked_as_inactive"];
            }

            // 2. Kiểm tra nếu có học sinh đã đăng ký học lớp thuộc course này
            var enrollmentCheckSql = "SELECT COUNT(*) FROM course_enrollments WHERE course_id = @Id";
            var enrollmentCount = await _db.ExecuteScalarAsync<int>(enrollmentCheckSql, new { Id = id }, transaction);

            if (enrollmentCount > 0)
            {
                var deactivateSql = "UPDATE courses SET is_active = false WHERE id = @Id";
                await _db.ExecuteAsync(deactivateSql, new { Id = id }, transaction);

                transaction.Commit();
                return _localizer["classes_exist_for_this_course_The_course_has_been_marked_as_inactive"];
            }

            // 3. Xóa khóa học
            var deleteCourseSql = "DELETE FROM courses WHERE id = @Id";
            await _db.ExecuteAsync(deleteCourseSql, new { Id = id }, transaction);

            transaction.Commit();
            return _localizer["course_deleted_successfully"];
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> CheckStudentExistFromCourse(string id)
    {
        var sql = @"
        SELECT COUNT(*)
        FROM courses c
        JOIN course_enrollments eh ON c.id = eh.course_id
        WHERE c.id = @Id;
        ";

        var count = await _db.ExecuteScalarAsync<int>(sql, new { Id = id });

        return count > 0;
    }
}
