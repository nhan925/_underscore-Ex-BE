using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Console;
using Npgsql;
using NpgsqlTypes;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Localization;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace student_management_api.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _cultureSuffix;

    public StudentRepository(IDbConnection db, IStringLocalizer<Messages> localizer)
    {
        _db = db;
        _localizer = localizer;
        _cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "" : $"_{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}";
    }

    public async Task<int> DeleteStudentById(string id)
    {
        string query = "DELETE FROM students WHERE id = @Id";
        return await _db.ExecuteAsync(query, new { Id = id });
    }

    public async Task<Student?> GetStudentById(string id)
    {
        string studentQuery = $"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, faculty_id, intake_year, email, phone_number, status_id, program_id, nationality{_cultureSuffix} AS nationality, created_at " +
                              $"FROM students WHERE id = @Id";
        string addressQuery = $"SELECT student_id, other{_cultureSuffix} AS other, village{_cultureSuffix} AS village, district{_cultureSuffix} AS district, city{_cultureSuffix} AS city, country{_cultureSuffix} AS country, type " +
                              $"FROM addresses WHERE student_id = @Id";
        string identityInfoQuery = $"SELECT student_id, number, place_of_issue{_cultureSuffix} AS place_of_issue, date_of_issue, expiry_date, additional_info{_cultureSuffix} AS additional_info, type " +
                              $"FROM identity_info WHERE student_id = @Id";

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
        StringBuilder queryBuilder = new StringBuilder($"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, faculty_id, intake_year, program_id, status_id FROM students");
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
                    sqlBuilder.Append($"gender{_cultureSuffix} = @Gender, ");
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
                    sqlBuilder.Append($"nationality{_cultureSuffix} = @Nationality, ");
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
                    throw new Exception(_localizer["update_student_failed"]);
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string deleteAddressQuery = "DELETE FROM addresses WHERE student_id = @StudentId";
                    await _db.ExecuteAsync(deleteAddressQuery, new { StudentId = id }, transaction);

                    string addressQuery = @$"
                INSERT INTO addresses (student_id, other{_cultureSuffix}, village{_cultureSuffix}, district{_cultureSuffix}, city{_cultureSuffix}, country{_cultureSuffix}, type) 
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

                    string identityInfoQuery = @$"
                INSERT INTO identity_info (student_id, number, place_of_issue{_cultureSuffix}, date_of_issue, expiry_date, additional_info{_cultureSuffix}, type) 
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
                throw;
            }
        }
    }

    public async Task<string> GenerateStudentId(int intakeYear, int facultyId)
    {
        string shortYear = (intakeYear % 100).ToString("D2"); // XX (last 2 digits of intake year)
        string facultyCode = facultyId.ToString("D2"); // YY (faculty ID, always 2 digits)

        string sequenceName = $"student_seq_{intakeYear}_{facultyId}"; // Unique sequence for each intake & faculty

        // Ensure the sequence exists (each faculty + year gets its own sequence)
        string createSequenceQuery = $@"
            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = '{sequenceName}') THEN
                    EXECUTE 'CREATE SEQUENCE {sequenceName} START 1';
                END IF;
            END $$;";

        await _db.ExecuteAsync(createSequenceQuery); // Ensure the sequence exists

        // Get the next sequence value
        string getNextValueQuery = $"SELECT nextval('{sequenceName}')";

        int sequenceNumber = await _db.QuerySingleAsync<int>(getNextValueQuery);

        string sequence = sequenceNumber.ToString("D4"); // Ensure 4-digit sequence

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
                string studentQuery = @$"
                INSERT INTO students (id, full_name, date_of_birth, gender{_cultureSuffix}, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality{_cultureSuffix}) 
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
                    throw new Exception(_localizer["add_student_failed"]);
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string addressQuery = @$"
                    INSERT INTO addresses (student_id, other{_cultureSuffix}, village{_cultureSuffix}, district{_cultureSuffix}, city{_cultureSuffix}, country{_cultureSuffix}, type) 
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
                    string identityInfoQuery = @$"
                    INSERT INTO identity_info (student_id, number, place_of_issue{_cultureSuffix}, date_of_issue, expiry_date, additional_info{_cultureSuffix}, type) 
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
                throw;
            }
        }
    }

    public async Task AddStudents(List<AddStudentRequest> requests)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                var studentValues = new List<string>();
                var addressValues = new List<string>();
                var identityValues = new List<string>();

                var studentParameters = new List<NpgsqlParameter>();
                var addressParameters = new List<NpgsqlParameter>();
                var identityParameters = new List<NpgsqlParameter>();

                int index = 0;

                foreach (var request in requests)
                {
                    string studentId = await GenerateStudentId(request.IntakeYear, request.FacultyId);

                    // Student Table
                    studentValues.Add($"(@Id{index}, @FullName{index}, @DateOfBirth{index}, @Gender{index}, @FacultyId{index}, @IntakeYear{index}, " +
                        $"@ProgramId{index}, @Email{index}, @PhoneNumber{index}, @StatusId{index}, @Nationality{index})");

                    studentParameters.Add(new NpgsqlParameter($"@Id{index}", studentId));
                    studentParameters.Add(new NpgsqlParameter($"@FullName{index}", request.FullName));
                    studentParameters.Add(new NpgsqlParameter($"@DateOfBirth{index}", request.DateOfBirth));
                    studentParameters.Add(new NpgsqlParameter($"@Gender{index}", request.Gender));
                    studentParameters.Add(new NpgsqlParameter($"@FacultyId{index}", request.FacultyId));
                    studentParameters.Add(new NpgsqlParameter($"@IntakeYear{index}", request.IntakeYear));
                    studentParameters.Add(new NpgsqlParameter($"@ProgramId{index}", request.ProgramId));
                    studentParameters.Add(new NpgsqlParameter($"@Email{index}", request.Email));
                    studentParameters.Add(new NpgsqlParameter($"@PhoneNumber{index}", request.PhoneNumber));
                    studentParameters.Add(new NpgsqlParameter($"@StatusId{index}", request.StatusId));
                    studentParameters.Add(new NpgsqlParameter($"@Nationality{index}", request.Nationality));

                    // Address Table
                    int addressCount = 0;
                    foreach (var address in request.Addresses!)
                    {
                        addressValues.Add($"(@StudentId{index}_{addressCount}, @Other{index}_{addressCount}, @Village{index}_{addressCount}, @District{index}_{addressCount}, " +
                            $"@City{index}_{addressCount}, @Country{index}_{addressCount}, @Type{index}_{addressCount})");

                        addressParameters.Add(new NpgsqlParameter($"@StudentId{index}_{addressCount}", studentId));
                        addressParameters.Add(new NpgsqlParameter($"@Other{index}_{addressCount}", address.Other));
                        addressParameters.Add(new NpgsqlParameter($"@Village{index}_{addressCount}", address.Village));
                        addressParameters.Add(new NpgsqlParameter($"@District{index}_{addressCount}", address.District));
                        addressParameters.Add(new NpgsqlParameter($"@City{index}_{addressCount}", address.City));
                        addressParameters.Add(new NpgsqlParameter($"@Country{index}_{addressCount}", address.Country));
                        addressParameters.Add(new NpgsqlParameter($"@Type{index}_{addressCount}", address.Type));

                        addressCount++;
                    }

                    // Identity Info Table
                    identityValues.Add($"(@StudentId{index}, @Number{index}, @PlaceOfIssue{index}, @DateOfIssue{index}, @ExpiryDate{index}, @AdditionalInfo{index}, @Type{index})");

                    identityParameters.Add(new NpgsqlParameter($"@StudentId{index}", studentId));
                    identityParameters.Add(new NpgsqlParameter($"@Number{index}", request.IdentityInfo!.Number));
                    identityParameters.Add(new NpgsqlParameter($"@PlaceOfIssue{index}", request.IdentityInfo.PlaceOfIssue));
                    identityParameters.Add(new NpgsqlParameter($"@DateOfIssue{index}", request.IdentityInfo.DateOfIssue));
                    identityParameters.Add(new NpgsqlParameter($"@ExpiryDate{index}", request.IdentityInfo.ExpiryDate));
                    identityParameters.Add(new NpgsqlParameter($"@Type{index}", request.IdentityInfo.Type));

                    var additionalInfoParam = new NpgsqlParameter($"@AdditionalInfo{index}", NpgsqlDbType.Jsonb);
                    additionalInfoParam.Value = request.IdentityInfo.AdditionalInfo is null
                        ? (object)DBNull.Value  // Ensure it's cast to object
                        : JsonSerializer.Serialize(request.IdentityInfo.AdditionalInfo);

                    identityParameters.Add(additionalInfoParam);

                    index++;
                }

                // Insert into Students Table
                if (studentValues.Count > 0)
                {
                    string studentQuery = $@"
                    INSERT INTO students (id, full_name, date_of_birth, gender{_cultureSuffix}, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality{_cultureSuffix}) 
                    VALUES {string.Join(", ", studentValues)}";

                    using (var studentCmd = new NpgsqlCommand(studentQuery, (NpgsqlConnection?)_db, (NpgsqlTransaction?)transaction))
                    {
                        studentCmd.Parameters.AddRange(studentParameters.ToArray());
                        await studentCmd.ExecuteNonQueryAsync();
                    }
                }

                // Insert into Addresses Table
                if (addressValues.Count > 0)
                {
                    string addressQuery = $@"
                    INSERT INTO addresses (student_id, other{_cultureSuffix}, village{_cultureSuffix}, district{_cultureSuffix}, city{_cultureSuffix}, country{_cultureSuffix}, type)  
                    VALUES {string.Join(", ", addressValues)}";

                    using (var addressCmd = new NpgsqlCommand(addressQuery, (NpgsqlConnection?)_db, (NpgsqlTransaction?)transaction))
                    {
                        addressCmd.Parameters.AddRange(addressParameters.ToArray());
                        await addressCmd.ExecuteNonQueryAsync();
                    }
                }

                // Insert into Identity Infos Table
                if (identityValues.Count > 0)
                {
                    string identityQuery = $@"
                    INSERT INTO identity_info (student_id, number, place_of_issue{_cultureSuffix}, date_of_issue, expiry_date, additional_info{_cultureSuffix}, type)  
                    VALUES {string.Join(", ", identityValues)}";

                    using (var identityCmd = new NpgsqlCommand(identityQuery, (NpgsqlConnection?)_db, (NpgsqlTransaction?)transaction))
                    {
                        identityCmd.Parameters.AddRange(identityParameters.ToArray());
                        await identityCmd.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }


    public async Task<List<Student>> GetAllStudents()
    {
        string studentQuery = $"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, faculty_id, intake_year, email, phone_number, status_id, program_id, nationality{_cultureSuffix} AS nationality, created_at " +
                              $"FROM students";
        string addressQuery = $"SELECT student_id, other{_cultureSuffix} AS other, village{_cultureSuffix} AS village, district{_cultureSuffix} AS district, city{_cultureSuffix} AS city, country{_cultureSuffix} AS country, type " +
                              $"FROM addresses WHERE student_id = ANY(@StudentIds)";
        string identityInfoQuery = $"SELECT student_id, number, place_of_issue{_cultureSuffix} AS place_of_issue, date_of_issue, expiry_date, additional_info{_cultureSuffix} AS additional_info, type " +
                              $"FROM identity_info WHERE student_id = ANY(@StudentIds)";

        var students = (await _db.QueryAsync<Student>(studentQuery)).ToList();
        
        if (!students.Any())
        {
            return new(); // Return empty list if no students found
        }

        var studentIds = students.Select(s => s.Id).ToArray();

        var addresses = await _db.QueryAsync<FullAddress>(addressQuery, new { StudentIds = studentIds });
        var identityInfos = await _db.QueryAsync<FullIdentityInfo>(identityInfoQuery, new { StudentIds = studentIds });

        // Map addresses and identity info to students
        var addressLookup = addresses?
            .GroupBy(a => a.StudentId!)
            .ToDictionary(g => g.Key, g => g.Select(fa => new Address(fa)).ToList());
        var identityLookup = identityInfos?.ToDictionary(i => i.StudentId!, i => new IdentityInfo(i));

        foreach (var student in students)
        {
            student.Addresses = addressLookup != null ? addressLookup[student.Id!] : new(); 
            student.IdentityInfo = identityLookup != null ? identityLookup[student.Id!] : new(); 
        }

        return students;
    }

}
