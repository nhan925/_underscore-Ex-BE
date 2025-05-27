using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Localization;
using student_management_api.Models.Course;
using student_management_api.Models.CourseEnrollment;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Data;
using System.Globalization;
using System.Text;

namespace student_management_api.Repositories;

public class CourseEnrollmentRepository : ICourseEnrollmentRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _cultureSuffix;

    public CourseEnrollmentRepository(IDbConnection db, IStringLocalizer<Messages> localizer)
    {
        _db = db;
        _localizer = localizer;
        _cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "" : $"_{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}";
    }

    private async Task LogEnrollmentHistory(CourseEnrollmentRequest request, string action, IDbTransaction transaction)
    {
        var historySql = "INSERT INTO enrollment_history (student_id, course_id, class_id, semester_id, action) " +
            "VALUES (@StudentId, @CourseId, @ClassId, @SemesterId, @Action)";
        var historyParameters = new
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            ClassId = request.ClassId,
            SemesterId = request.SemesterId,
            Action = action
        };
        var historyAffectedRows = await _db.ExecuteAsync(historySql, historyParameters, transaction);
        if (historyAffectedRows == 0)
        {
            throw new Exception(_localizer["failed_to_log_enrollment_history"]);
        }
    }

    public async Task RegisterClass(CourseEnrollmentRequest request)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                var endDateSql = "SELECT end_date FROM semesters WHERE id = @SemesterId";
                var endDate = await _db.QueryFirstOrDefaultAsync<DateTime>(endDateSql, new { SemesterId = request.SemesterId });
                if (DateTime.Now > endDate)
                {
                    throw new Exception(_localizer["cannot_register_after_the_semester_has_ended"]);
                }

                var maxStudents = "SELECT max_students FROM classes WHERE id = @ClassId AND course_id = @CourseId AND semester_id = @SemesterId";
                var maxStudentsCount = await _db.QueryFirstOrDefaultAsync<int>(maxStudents, new
                {
                    ClassId = request.ClassId,
                    CourseId = request.CourseId,
                    SemesterId = request.SemesterId
                });
                
                var currentStudents = "SELECT COUNT(*) FROM course_enrollments WHERE class_id = @ClassId AND course_id = @CourseId AND semester_id = @SemesterId";
                var currentStudentsCount = await _db.QueryFirstOrDefaultAsync<int>(currentStudents, new
                {
                    ClassId = request.ClassId,
                    CourseId = request.CourseId,
                    SemesterId = request.SemesterId
                });

                if (currentStudentsCount >= maxStudentsCount)
                {
                    throw new Exception(_localizer["class_is_full"]);
                }

                var prerequisiteSql = "SELECT prerequisite_id FROM course_prerequisites WHERE course_id = @CourseId";
                var prerequisites = await _db.QueryAsync<string>(prerequisiteSql, new { CourseId = request.CourseId });
                
                if (prerequisites.Any()) // have prerequisites
                {
                    var checkPrerequisiteSql = "SELECT COUNT(*) FROM course_enrollments " +
                        "WHERE student_id = @StudentId AND course_id = ANY(@CourseIds) AND status = 'passed'";
                    var checkPrerequisiteParameters = new
                    {
                        StudentId = request.StudentId,
                        CourseIds = prerequisites.ToArray()
                    };
                    var passedPrerequisitesCount = await _db.ExecuteScalarAsync<int>(checkPrerequisiteSql, checkPrerequisiteParameters, transaction);
                    
                    if (passedPrerequisitesCount != prerequisites.Count())
                    {
                        throw new ForbiddenException(_localizer["student_has_not_passed_all_prerequisites"]);
                    }
                }

                // begin registering
                var tryUpdateSql = "UPDATE course_enrollments SET grade = null, semester_id = @SemesterId, class_id = @ClassId " +
                        "WHERE student_id = @StudentId AND course_id = @CourseId AND status in ('failed', 'passed')";
                var tryUpdateParameters = new
                {
                    SemesterId = request.SemesterId,
                    ClassId = request.ClassId,
                    StudentId = request.StudentId,
                    CourseId = request.CourseId
                };
                var tryUpdateAffectedRows = await _db.ExecuteAsync(tryUpdateSql, tryUpdateParameters, transaction);

                if (tryUpdateAffectedRows == 0)
                {
                    var insertSql = "INSERT INTO course_enrollments (student_id, course_id, class_id, semester_id) " +
                        "VALUES (@StudentId, @CourseId, @ClassId, @SemesterId)";
                    var insertParameters = new
                    {
                        StudentId = request.StudentId,
                        CourseId = request.CourseId,
                        ClassId = request.ClassId,
                        SemesterId = request.SemesterId
                    };
                    var insertAffectedRows = await _db.ExecuteAsync(insertSql, insertParameters, transaction);
                    if (insertAffectedRows == 0)
                    {
                        throw new Exception(_localizer["failed_to_register_for_the_course"]);
                    }
                }

                await LogEnrollmentHistory(request, "register", transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public async Task UnregisterClass(CourseEnrollmentRequest request)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                var getSemesterStartDate = "SELECT start_date FROM semesters WHERE id = @SemesterId";
                var startDate = await _db.QueryFirstOrDefaultAsync<DateTime>(getSemesterStartDate, 
                    new { SemesterId = request.SemesterId });

                if (DateTime.Now > startDate)
                {
                    throw new ForbiddenException(_localizer["cannot_unregister_after_the_semester_has_started"]);
                }

                var sql = "DELETE FROM course_enrollments WHERE student_id = @StudentId AND course_id = @CourseId AND status = 'enrolled'";
                var parameters = new
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId
                };
                var affectedRows = await _db.ExecuteAsync(sql, parameters, transaction);
                if (affectedRows == 0)
                {
                    throw new NotFoundException(_localizer["no_enrollment_found_to_unregister_or_the_student_has_completed_the_course"]);
                }

                await LogEnrollmentHistory(request, "cancel", transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public async Task<List<EnrollmentHistory>> GetEnrollmentHistoryBySemester(int semesterId)
    {
        string sql = $"SELECT student_id, created_at, course_id, class_id, semester_id, action{_cultureSuffix} AS action FROM enrollment_history WHERE semester_id = @SemesterId";
        var history = await _db.QueryAsync<EnrollmentHistory>(sql, new { SemesterId = semesterId });

        return history.ToList();
    }

    public async Task<Transcript> GetTranscriptOfStudentById(string studentId)
    {
        string getTranscriptSql = $"SELECT c.id, c.name{_cultureSuffix} AS name, c.credits, ce.grade FROM courses c JOIN course_enrollments ce ON c.id = ce.course_id " +
            "WHERE ce.student_id = @StudentId AND ce.status = 'passed'";
        var coursesWithGrade = await _db.QueryAsync<SimplifiedCourseWithGrade>(getTranscriptSql, new { StudentId = studentId });

        var totalCredits = 0;
        var gpa = 0.0f;
        
        if (coursesWithGrade.Any())
        {
            totalCredits = coursesWithGrade.Sum(c => c.Credits);
            gpa = coursesWithGrade.Sum(c => c.Credits * c.Grade) / totalCredits;
        }
        
        return new()
        {
            TotalCredits = totalCredits,
            GPA = gpa,
            Courses = coursesWithGrade.ToList()
        };
    }

    public async Task<int> UpdateStudentGrade(string studentId, string courseId, float? grade)
    {
        var sql = "UPDATE course_enrollments SET grade = @Grade WHERE student_id = @StudentId AND course_id = @CourseId";
        var parameters = new DynamicParameters();

        parameters.Add("@StudentId", studentId);
        parameters.Add("@CourseId", courseId);
        parameters.Add("@Grade", grade.HasValue ? Math.Round(grade.Value, 1) : null);

        var affectedRows = await _db.ExecuteAsync(sql, parameters);
        return affectedRows;
    }
}
