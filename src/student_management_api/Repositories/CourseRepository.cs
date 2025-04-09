using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class CourseRepository: ICourseRepository
{
    private readonly IDbConnection _db;
    public CourseRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<List<Course>> GetAllCourses()
    {
        var sql = @"
        SELECT 
            c.id, c.name, c.credits, c.faculty_id,
            c.description, c.created_at,
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
                if (!courseDict.TryGetValue(course.Id, out var existing))
                {
                    existing = course;
                    existing.PrerequisitesId = new List<string>();
                    courseDict.Add(existing.Id, existing);
                }

                if (!string.IsNullOrEmpty(prereqId))
                {
                    existing.PrerequisitesId.Add(prereqId);
                }

                return existing;
            },
            splitOn: "prerequisite_id"
        );

        return courseDict.Values.ToList();
    }

    public async Task<Course> GetCourseById(string id)
    {
        var sql = @"
        SELECT
            c.id,
            c.name,
            c.credits,
            c.faculty_id,
            c.description,
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
                if (!courseDict.TryGetValue(course.Id, out var existing))
                {
                    existing = course;
                    existing.PrerequisitesId = new List<string>();
                    courseDict.Add(existing.Id, existing);
                }

                if (!string.IsNullOrEmpty(prereqId))
                    existing.PrerequisitesId.Add(prereqId);

                return existing;
            },
            new { Id = id },
            splitOn: "prerequisite_id"
        );

        var course = courseDict.Values.FirstOrDefault();
        if (course == null)
            throw new Exception($"Course with ID {id} not found.");

        return course;
    }

    public async Task<int> UpdateCourseById(Course course)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                // 1. Cập nhật bảng courses
                var updateSql = @"
            UPDATE courses
            SET 
                name         = @Name,
                credits      = @Credits,
                faculty_id   = @FacultyId,
                description  = @Description
            WHERE id = @Id;
            ";

                var updateParams = new
                {
                    course.Id,
                    course.Name,
                    course.Credits,
                    course.FacultyId,
                    course.Description
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
                            throw new Exception($"Prerequisite course ID {prerequisiteId} does not exist.");
                        }
                    }
                }

                // 2. Thêm vào bảng courses
                var insertCourseSql = @"
                INSERT INTO courses (id, name, credits, faculty_id, description)
                VALUES (@Id, @Name, @Credits, @FacultyId, @Description);
                ";

                var courseParams = new
                {
                    course.Id,
                    course.Name,
                    course.Credits,
                    course.FacultyId,
                    course.Description
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
                return "Đã có lớp học thuộc khóa học này. Khóa học được đánh dấu dừng hoạt động";
            }

            // 2. Kiểm tra nếu có học sinh đã đăng ký học lớp thuộc course này
            var enrollmentCheckSql = "SELECT COUNT(*) FROM course_enrollments WHERE course_id = @Id";
            var enrollmentCount = await _db.ExecuteScalarAsync<int>(enrollmentCheckSql, new { Id = id }, transaction);

            if (enrollmentCount > 0)
            {
                var deactivateSql = "UPDATE courses SET is_active = false WHERE id = @Id";
                await _db.ExecuteAsync(deactivateSql, new { Id = id }, transaction);

                transaction.Commit();
                return "Đã có học sinh học lớp học này. Khóa học được đánh dấu dừng hoạt động";
            }

            // 3. Xóa khóa học
            var deleteCourseSql = "DELETE FROM courses WHERE id = @Id";
            await _db.ExecuteAsync(deleteCourseSql, new { Id = id }, transaction);

            transaction.Commit();
            return "Xóa khóa học thành công";
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception("Error to delete this course " + ex.Message);
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
