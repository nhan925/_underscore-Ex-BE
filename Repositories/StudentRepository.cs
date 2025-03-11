using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Data;
using System.Text;

namespace student_management_api.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnection _db;

    public StudentRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<int> DeleteStudentById(string id)
    {
        string query = "DELETE FROM students WHERE id = @Id";
        return await _db.ExecuteAsync(query, new { Id = id });
    }

    public async Task<Student?> GetStudentById(string id)
    {
        string query = "SELECT * FROM students WHERE id = @Id";
        return await _db.QueryFirstOrDefaultAsync<Student>(query, new { Id = id });
    }

    public async Task<PagedResult<Student>> GetStudents(int page, int pageSize, string? search)
    {
        StringBuilder queryBuilder = new StringBuilder("SELECT * FROM students");
        StringBuilder countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM students");

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryBuilder.Append(" WHERE id ILIKE @Search OR full_name ILIKE @Search");
            countQueryBuilder.Append(" WHERE id ILIKE @Search OR full_name ILIKE @Search");
        }

        queryBuilder.Append(" ORDER BY id LIMIT @PageSize OFFSET @Offset");

        var parameters = new
        {
            Search = $"%{search}%",
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        };

        var students = await _db.QueryAsync<Student>(queryBuilder.ToString(), parameters);
        int totalCount = await _db.ExecuteScalarAsync<int>(countQueryBuilder.ToString(), parameters);

        return new PagedResult<Student>
        {
            Items = students.ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<int> UpdateStudentById(string id, UpdateStudentRequest request)
    {
        var sqlBuilder = new StringBuilder("UPDATE students SET ");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(request.FullName))
        {
            sqlBuilder.Append("full_name = @FullName, ");
            parameters.Add("FullName", request.FullName);
        }
        if (request.DateOfBirth.HasValue)
        {
            sqlBuilder.Append("date_of_birth = @DateOfBirth, ");
            parameters.Add("DateOfBirth", request.DateOfBirth);
        }
        if (!string.IsNullOrEmpty(request.Gender))
        {
            sqlBuilder.Append("gender = @Gender, ");
            parameters.Add("Gender", request.Gender);
        }
        if (request.FacultyId.HasValue)
        {
            sqlBuilder.Append("faculty_id = @FacultyId, ");
            parameters.Add("FacultyId", request.FacultyId);
        }
        if (request.IntakeYear.HasValue)
        {
            sqlBuilder.Append("intake_year = @IntakeYear, ");
            parameters.Add("IntakeYear", request.IntakeYear);
        }
        if (!string.IsNullOrEmpty(request.Program))
        {
            sqlBuilder.Append("program = @Program, ");
            parameters.Add("Program", request.Program);
        }
        if (!string.IsNullOrEmpty(request.Address))
        {
            sqlBuilder.Append("address = @Address, ");
            parameters.Add("Address", request.Address);
        }
        if (!string.IsNullOrEmpty(request.Email))
        {
            sqlBuilder.Append("email = @Email, ");
            parameters.Add("Email", request.Email);
        }
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            sqlBuilder.Append("phone_number = @PhoneNumber, ");
            parameters.Add("PhoneNumber", request.PhoneNumber);
        }
        if (request.StatusId.HasValue)
        {
            sqlBuilder.Append("status_id = @StatusId, ");
            parameters.Add("StatusId", request.StatusId);
        }

        // Remove last comma
        if (sqlBuilder.ToString().EndsWith(", "))
        {
            sqlBuilder.Length -= 2;
        }

        sqlBuilder.Append(" WHERE id = @Id");
        parameters.Add("Id", id);

        return await _db.ExecuteAsync(sqlBuilder.ToString(), parameters);
    }

    public async Task<string> GenerateStudentId(int intakeYear, int facultyId)
    {
        string shortYear = (intakeYear % 100).ToString("D2"); // Get last 2 digits of intake year (XX)
        string facultyCode = facultyId.ToString("D2"); // Ensure 2-digit faculty ID (YY)

        string query = @"
        SELECT COUNT(*) 
        FROM students 
        WHERE intake_year = @IntakeYear AND faculty_id = @FacultyId";

        int count = await _db.QuerySingleAsync<int>(query, new { IntakeYear = intakeYear, FacultyId = facultyId });

        string sequence = (count + 1).ToString("D4"); // ZZZZ → Ensure 4 digits

        return $"{shortYear}{facultyCode}{sequence}"; // Format: XXYYZZZZ
    }

    public async Task<string> AddStudent(AddStudentRequest request)
    {
        var studentId = await GenerateStudentId(request.IntakeYear, request.FacultyId); // Auto-generate ID

        string query = @"
        INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program, address, email, phone_number, status_id) 
        VALUES (@Id, @FullName, @DateOfBirth, @Gender, @FacultyId, @IntakeYear, @Program, @Address, @Email, @PhoneNumber, @StatusId)";

        var parameters = new
        {
            Id = studentId,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.FacultyId,
            request.IntakeYear,
            request.Program,
            request.Address,
            request.Email,
            request.PhoneNumber,
            request.StatusId
        };

        var count = await _db.ExecuteAsync(query, parameters);

        if (count == 0)
        {
            throw new Exception("add student failed");
        }

        return studentId;
    }
}
