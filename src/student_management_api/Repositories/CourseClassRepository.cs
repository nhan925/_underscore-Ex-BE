using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class CourseClassRepository : ICourseClassRepository
{
    private readonly IDbConnection _db;
    public CourseClassRepository(IDbConnection db)
    {
        _db = db;
    }
    public async Task<string> AddCourseClass(CourseClass courseClass)
    {
        var query = "INSERT INTO classes (id, course_id, semester_id, lecturer_id, max_students, schedule, room) " +
                    "VALUES (@CourseClassId, @CourseId, @SemesterId, @LecturerId, @MaxStudents, @Schedule, @Room)";
        var parameters = new
        {
            CourseClassId = courseClass.Id,
            CourseId = courseClass.CourseId,
            SemesterId = courseClass.SemesterId,
            LecturerId = courseClass.LecturerId,
            MaxStudents = courseClass.MaxStudents,
            Schedule = courseClass.Schedule,
            Room = courseClass.Room
        };
        var courseClassCount = await _db.ExecuteAsync(query, parameters);
        if (courseClassCount == 0)
        {
            throw new Exception("Failed to add course class");
        }

        return courseClass.Id;
    }

    public async Task<List<GetCourseClassResult>> GetAllCourseClassesBySemester(int semesterId)
    {
        
        var courseClassQuery = "SELECT * FROM classes WHERE semester_id = @SemesterId";
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

            var courseQuery = "SELECT * FROM courses WHERE id = @CourseId";
            var course = await _db.QueryFirstOrDefaultAsync<Course>(courseQuery, new { CourseId = courseClass.CourseId });
            courseClassResult.Course = course;

            var semesterQuery = "SELECT * FROM semesters WHERE id = @SemesterId";
            var semesterResult = await _db.QueryFirstOrDefaultAsync<Semester>(semesterQuery, new { SemesterId = courseClass.SemesterId });
            courseClassResult.Semester = semesterResult;

            var lecturerQuery = "SELECT * FROM lecturers WHERE id = @LecturerId";
            var lecturer = await _db.QueryFirstOrDefaultAsync<Lecturer>(lecturerQuery, new { LecturerId = courseClass.LecturerId });
            courseClassResult.Lecturer = lecturer;

            courseClassResults.Add(courseClassResult);
        }

        return courseClassResults;
    }

    public async Task<List<StudentInClass>> GetStudentsInClass(GetStudentsInClassRequest request)
    {
        string sql = @"
            SELECT student_id, grade, status, (SELECT full_name FROM students WHERE id = student_id) full_name
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
