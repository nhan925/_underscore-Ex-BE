using Dapper;
using Microsoft.Extensions.Localization;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Resources;
using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;
using System.Data;
using System.Globalization;
using student_management_api.Localization;

namespace student_management_api.Repositories;

public class CourseClassRepository : ICourseClassRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _cultureSuffix;
    private readonly string _culture;

    public CourseClassRepository(IDbConnection db, IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
    }

    // Use this because the AI Model have problems with translating day of week in schedule
    private string TranslateDayOfWeekInSchedule(string text, string sourceLanguage, string targetLanguage)
    {
        if (sourceLanguage == targetLanguage)
        {
            return text; // No translation needed if both languages are the same
        }

        var dayOfWeekMap = new Dictionary<string, string>
        {
            { "Monday", "Thứ 2" },
            { "Tuesday", "Thứ 3" },
            { "Wednesday", "Thứ 4" },
            { "Thursday", "Thứ 5" },
            { "Friday", "Thứ 6" },
            { "Saturday", "Thứ 7" },
            { "Sunday", "Chủ nhật" }
        };

        if (targetLanguage == "vi")
        {
            foreach (var kvp in dayOfWeekMap)
            {
                text = text.Replace(kvp.Key, kvp.Value);
            }
        }
        else if (targetLanguage == "en")
        {
            foreach (var kvp in dayOfWeekMap)
            {
                text = text.Replace(kvp.Value, kvp.Key);
            }
        }

        return text;
    }

    public async Task<string> AddCourseClass(CourseClass courseClass)
    {
        var query = $"INSERT INTO classes (id, course_id, semester_id, lecturer_id, max_students, schedule, room, schedule_vi, need_to_review) " +
                     "VALUES (@CourseClassId, @CourseId, @SemesterId, @LecturerId, @MaxStudents, @Schedule, @Room, @ScheduleVi, @NeedToReview)";
        var parameters = new
        {
            CourseClassId = courseClass.Id,
            CourseId = courseClass.CourseId,
            SemesterId = courseClass.SemesterId,
            LecturerId = courseClass.LecturerId,
            MaxStudents = courseClass.MaxStudents,
            Schedule = TranslateDayOfWeekInSchedule(courseClass.Schedule!, _culture, "en"),
            Room = courseClass.Room,
            ScheduleVi = TranslateDayOfWeekInSchedule(courseClass.Schedule!, _culture, "vi"),
            NeedToReview = true,
        };

        try
        {
            var courseClassCount = await _db.ExecuteAsync(query, parameters);

            if (courseClassCount == 0)
            {
                throw new Exception(_localizer["failed_to_add_class"]);
            }
        }
        catch(Exception ex)
        {
            if (ex.Message.Contains("duplicate key value violates unique constraint \"classes_semester_id_schedule_room_key\""))
            {
                throw new ForbiddenException(_localizer["class_with_duplicate_schedule_and_room"]);
            }
            else if (ex.Message.Contains("duplicate key value violates unique constraint \"classes_pkey\""))
            {
                throw new ForbiddenException(_localizer["class_id_already_exists"]);
            }
            else
            {
                throw;
            }
        }

        return courseClass.Id!;
    }

    public async Task<List<GetCourseClassResult>> GetAllCourseClassesBySemester(int semesterId)
    {
        
        var courseClassQuery = $"SELECT id, course_id, semester_id, lecturer_id, max_students, schedule{_cultureSuffix} AS schedule, room, created_at, is_active " +
                               $"FROM classes WHERE semester_id = @SemesterId";
        var courseClasses = await _db.QueryAsync<CourseClass>(courseClassQuery, new { SemesterId = semesterId });

        var courseClassResults = new List<GetCourseClassResult>();

        foreach (var courseClass in courseClasses)
        {
            
            var courseClassResult = new GetCourseClassResult
            {
                Id = courseClass.Id,
                MaxStudents = courseClass.MaxStudents,
                Schedule = courseClass.Schedule,
                Room = courseClass.Room
            };

            var courseQuery = $"SELECT id, name{_cultureSuffix} AS name, credits, faculty_id, description{_cultureSuffix} AS description, created_at, is_active " +
                              $"FROM courses WHERE id = @CourseId";
            var course = await _db.QueryFirstOrDefaultAsync<Course>(courseQuery, new { CourseId = courseClass.CourseId });
            courseClassResult.Course = course;

            var semesterQuery = "SELECT * FROM semesters WHERE id = @SemesterId";
            var semesterResult = await _db.QueryFirstOrDefaultAsync<Semester>(semesterQuery, new { SemesterId = courseClass.SemesterId });
            courseClassResult.Semester = semesterResult;

            var lecturerQuery = $"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, email, phone_number, faculty_id " +
                                $"FROM lecturers WHERE id = @LecturerId";
            var lecturer = await _db.QueryFirstOrDefaultAsync<Lecturer>(lecturerQuery, new { LecturerId = courseClass.LecturerId });
            courseClassResult.Lecturer = lecturer;

            courseClassResults.Add(courseClassResult);
        }

        return courseClassResults;
    }

    public async Task<GetCourseClassResult> GetCourseClassByIdAndCourseAndSemester(string classId, string courseId, int semesterId)
    {
        var courseClassQuery = $"SELECT id, course_id, semester_id, lecturer_id, max_students, schedule{_cultureSuffix} AS schedule, room, created_at, is_active " +
                               $"FROM classes WHERE id = @ClassId AND course_id = @CourseId AND semester_id = @SemesterId";
        var courseQuery = $"SELECT id, name{_cultureSuffix} AS name, credits, faculty_id, description{_cultureSuffix} AS description, created_at, is_active " +
                          $"FROM courses WHERE id = @CourseId";
        var semesterQuery = "SELECT * FROM semesters WHERE id = @SemesterId";
        var lecturerQuery = $"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, email, phone_number, faculty_id " +
                            $"FROM lecturers WHERE id = @LecturerId";

        var courseClass = await _db.QueryFirstOrDefaultAsync<CourseClass>(courseClassQuery, new { ClassId = classId, CourseId = courseId, SemesterId = semesterId });
        
        if (courseClass == null)
        {
            throw new NotFoundException(_localizer["class_not_found"]); // Throwing NotFoundException if the class is not found
        }

        var course = await _db.QueryFirstOrDefaultAsync<Course>(courseQuery, new { CourseId = courseClass.CourseId });
        var semester = await _db.QueryFirstOrDefaultAsync<Semester>(semesterQuery, new { SemesterId = courseClass.SemesterId });
        var lecturer = await _db.QueryFirstOrDefaultAsync<Lecturer>(lecturerQuery, new { LecturerId = courseClass.LecturerId });

        var courseClassResult = new GetCourseClassResult
        {
            Id = courseClass.Id,
            MaxStudents = courseClass.MaxStudents,
            Schedule = courseClass.Schedule,
            Room = courseClass.Room,
            Course = course,
            Semester = semester,
            Lecturer = lecturer
        };

        return courseClassResult;
    }

    public async Task<List<StudentInClass>> GetStudentsInClass(GetStudentsInClassRequest request)
    {
        string sql = @$"
            SELECT student_id AS id, grade, status{_cultureSuffix} AS status, (SELECT full_name FROM students WHERE id = student_id) full_name
            FROM course_enrollments
            WHERE course_id = @CourseId AND class_id = @ClassId AND semester_id = @SemesterId";
        
        var parameters = new
        {
            CourseId = request.CourseId,
            ClassId = request.ClassId,
            SemesterId = request.SemesterId
        };

        var students = await _db.QueryAsync<StudentInClass>(sql, parameters);

        return students.ToList();
    }
}
