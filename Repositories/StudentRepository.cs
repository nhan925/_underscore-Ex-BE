using Dapper;
using Microsoft.Extensions.Logging.Console;
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
        string studentQuery = "SELECT * FROM students WHERE id = @Id";
        string addressQuery = "SELECT * FROM addresses WHERE student_id = @Id";
        string identityInfoQuery = "SELECT * FROM identity_info WHERE student_id = @Id";

        var student = await _db.QueryFirstOrDefaultAsync<Student>(studentQuery, new { Id = id });
        if (student != null)
        {
            var addresses = await _db.QueryAsync<Address>(addressQuery, new { Id = id });
            var identityInfo = await _db.QueryFirstOrDefaultAsync<IdentityInfo>(identityInfoQuery, new { Id = id });

            student.Addresses = addresses.ToList();
            student.IdentityInfo = identityInfo ?? new();
        }

        return student;
    }

    public async Task<PagedResult<SimplifiedStudent>> GetStudents(int page, int pageSize, string? search, StudentFilter? filter)
    {
        StringBuilder queryBuilder = new StringBuilder("SELECT id, full_name, date_of_birth, gender, faculty_id, intake_year, program_id, status_id FROM students");
        StringBuilder countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM students");

        var hasWhere = false;

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryBuilder.Append(" WHERE (id ILIKE @Search OR full_name ILIKE @Search)");
            countQueryBuilder.Append(" WHERE (id ILIKE @Search OR full_name ILIKE @Search)");
            hasWhere = true;
        }

        // Add filter conditions
        if (filter != null)
        {
            if (filter.FacultyIds != null && filter.FacultyIds.Count > 0)
            {
                queryBuilder.Append(hasWhere ? " AND" : " WHERE").Append(" faculty_id = ANY(@FacultyIds)");
                countQueryBuilder.Append(hasWhere ? " AND" : " WHERE").Append(" faculty_id = ANY(@FacultyIds)");
                hasWhere = true;
            }

            // Add more filter conditions here
        }

        queryBuilder.Append(" ORDER BY id LIMIT @PageSize OFFSET @Offset");

        var parameters = new
        {
            Search = $"%{search}%",
            PageSize = pageSize,
            Offset = (page - 1) * pageSize,
            FacultyIds = filter?.FacultyIds
        };

        var students = await _db.QueryAsync<SimplifiedStudent>(queryBuilder.ToString(), parameters);
        int totalCount = await _db.ExecuteScalarAsync<int>(countQueryBuilder.ToString(), parameters);

        return new PagedResult<SimplifiedStudent>
        {
            Items = students.ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<int> UpdateStudentById(string id, UpdateStudentRequest request)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
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

                if (request.ProgramId.HasValue)
                {
                    sqlBuilder.Append("program_id = @ProgramId, ");
                    parameters.Add("ProgramId", request.ProgramId);
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

                if (!string.IsNullOrEmpty(request.Nationality))
                {
                    sqlBuilder.Append("nationality = @Nationality, ");
                    parameters.Add("Nationality", request.Nationality);
                }

                // Remove last comma
                if (sqlBuilder.ToString().EndsWith(", "))
                {
                    sqlBuilder.Length -= 2;
                }

                sqlBuilder.Append(" WHERE id = @Id");
                parameters.Add("Id", id);

                var studentCount = await _db.ExecuteAsync(sqlBuilder.ToString(), parameters, transaction);

                if (studentCount == 0)
                {
                    throw new Exception("update student failed");
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string deleteAddressQuery = "DELETE FROM addresses WHERE student_id = @StudentId";
                    await _db.ExecuteAsync(deleteAddressQuery, new { StudentId = id }, transaction);

                    string addressQuery = @"
                INSERT INTO addresses (student_id, other, village, district, city, country, type) 
                VALUES (@StudentId, @Other, @Village, @District, @City, @Country, @Type)";

                    foreach (var address in request.Addresses)
                    {
                        var addressParameters = new
                        {
                            StudentId = id,
                            address.Other,
                            address.Village,
                            address.District,
                            address.City,
                            address.Country,
                            address.Type
                        };

                        await _db.ExecuteAsync(addressQuery, addressParameters, transaction);
                    }
                }

                if (request.IdentityInfo != null)
                {
                    string deleteIdentityInfoQuery = "DELETE FROM identity_info WHERE student_id = @StudentId";
                    await _db.ExecuteAsync(deleteIdentityInfoQuery, new { StudentId = id }, transaction);

                    string identityInfoQuery = @"
                INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type) 
                VALUES (@StudentId, @Number, @PlaceOfIssue, @DateOfIssue, @ExpiryDate, @AdditionalInfo, @Type)";

                    var identityInfoParameters = new
                    {
                        StudentId = id,
                        request.IdentityInfo.Number,
                        request.IdentityInfo.PlaceOfIssue,
                        request.IdentityInfo.DateOfIssue,
                        request.IdentityInfo.ExpiryDate,
                        request.IdentityInfo.AdditionalInfo,
                        request.IdentityInfo.Type
                    };

                    await _db.ExecuteAsync(identityInfoQuery, identityInfoParameters, transaction);
                }

                transaction.Commit();
                return studentCount;
            }
            catch
            {
                transaction.Rollback();
                throw new Exception("update student failed");
            }
        }
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
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        var studentId = await GenerateStudentId(request.IntakeYear, request.FacultyId); // Auto-generate ID

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                string studentQuery = @"
                INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality) 
                VALUES (@Id, @FullName, @DateOfBirth, @Gender, @FacultyId, @IntakeYear, @ProgramId, @Email, @PhoneNumber, @StatusId, @Nationality)";

                var studentParameters = new
                {
                    Id = studentId,
                    request.FullName,
                    request.DateOfBirth,
                    request.Gender,
                    request.FacultyId,
                    request.IntakeYear,
                    request.ProgramId,
                    request.Email,
                    request.PhoneNumber,
                    request.StatusId,
                    request.Nationality
                };

                var studentCount = await _db.ExecuteAsync(studentQuery, studentParameters, transaction);

                if (studentCount == 0)
                {
                    throw new Exception("add student failed");
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string addressQuery = @"
                    INSERT INTO addresses (student_id, other, village, district, city, country, type) 
                    VALUES (@StudentId, @Other, @Village, @District, @City, @Country, @Type)";

                    foreach (var address in request.Addresses)
                    {
                        var addressParameters = new
                        {
                            StudentId = studentId,
                            address.Other,
                            address.Village,
                            address.District,
                            address.City,
                            address.Country,
                            address.Type
                        };

                        await _db.ExecuteAsync(addressQuery, addressParameters, transaction);
                    }
                }

                if (request.IdentityInfo != null)
                {
                    string identityInfoQuery = @"
                    INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type) 
                    VALUES (@StudentId, @Number, @PlaceOfIssue, @DateOfIssue, @ExpiryDate, @AdditionalInfo, @Type)";

                    var identityInfoParameters = new
                    {
                        StudentId = studentId,
                        request.IdentityInfo.Number,
                        request.IdentityInfo.PlaceOfIssue,
                        request.IdentityInfo.DateOfIssue,
                        request.IdentityInfo.ExpiryDate,
                        request.IdentityInfo.AdditionalInfo,
                        request.IdentityInfo.Type
                    };

                    await _db.ExecuteAsync(identityInfoQuery, identityInfoParameters, transaction);
                }

                transaction.Commit();
                return studentId;
            }
            catch
            {
                transaction.Rollback();
                throw new Exception("add student failed"); ;
            }
        }
    }
}
