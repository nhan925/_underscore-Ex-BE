using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Console;
using Npgsql;
using NpgsqlTypes;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using student_management_api.Resources;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using student_management_api.Localization;

namespace student_management_api.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnection _db;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly string _culture;
    private readonly string _cultureSuffix;
    private readonly IExternalTranslationService _translationService;

    public StudentRepository(IDbConnection db, IStringLocalizer<Messages> localizer, IExternalTranslationService translationService)
    {
        _db = db;
        _localizer = localizer;
        _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _cultureSuffix = _culture == "en" ? "" : $"_{_culture}";
        _translationService = translationService;
    }

    // Manually translated methods to ensure they are not auto-translated by AI for Gender
    private string TranslateGender(string gender, string sourceLanguage, string targetLanguage)
    {
        if (sourceLanguage == targetLanguage)
        {
            return gender;
        }

        var genderTranslations = new Dictionary<string, string>
        {
            { "Male", "Nam" },
            { "Female", "Nữ" },
            { "Other", "Khác" }
        };

        if (targetLanguage == "vi")
        {
            foreach (var kvp in genderTranslations)
            {
                gender = gender.Replace(kvp.Key, kvp.Value);
            }
        }
        else if (targetLanguage == "en")
        {
            foreach (var kvp in genderTranslations)
            {
                gender = gender.Replace(kvp.Value, kvp.Key);
            }
        }

        return gender;
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
                var needToReview = false;

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
                    sqlBuilder.Append($"gender = @Gender, ");
                    parameters.Add("Gender", TranslateGender(request.Gender, _culture, "en"));

                    sqlBuilder.Append($"gender_vi = @GenderVi, ");
                    parameters.Add("GenderVi", TranslateGender(request.Gender, _culture, "vi"));

                    needToReview = true; // Mark as needing review if the content is translated by AI
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
                    sqlBuilder.Append($"nationality = @Nationality, ");
                    parameters.Add("Nationality", await _translationService.TranslateAsync(request.Nationality, _culture, "en"));

                    sqlBuilder.Append($"nationality_vi = @NationalityVi, ");
                    parameters.Add("NationalityVi", await _translationService.TranslateAsync(request.Nationality, _culture, "vi"));

                    needToReview = true; // Mark as needing review if the content is translated by AI
                }

                if (needToReview)
                {
                    sqlBuilder.Append($"need_to_review = true, ");
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
                    throw new OperationFailedException(_localizer["update_student_failed"]);
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string deleteAddressQuery = "DELETE FROM addresses WHERE student_id = @StudentId";
                    await _db.ExecuteAsync(deleteAddressQuery, new { StudentId = id }, transaction);

                    string addressQuery = @$"
                INSERT INTO addresses (student_id, other, village, district, city, country, type, other_vi, village_vi, district_vi, city_vi, country_vi, need_to_review) 
                VALUES (@StudentId, @Other, @Village, @District, @City, @Country, @Type, @OtherVi, @VillageVi, @DistrictVi, @CityVi, @CountryVi, true)";

                    foreach (var address in request.Addresses)
                    {
                        var addressParameters = new
                        {
                            StudentId = id,
                            Other = await _translationService.TranslateAsync(address.Other!, _culture, "en"),
                            Village = await _translationService.TranslateAsync(address.Village!, _culture, "en"),
                            District = await _translationService.TranslateAsync(address.District!, _culture, "en"),
                            City = await _translationService.TranslateAsync(address.City!, _culture, "en"),
                            Country = await _translationService.TranslateAsync(address.Country!, _culture, "en"),
                            address.Type,
                            OtherVi = await _translationService.TranslateAsync(address.Other!, _culture, "vi"),
                            VillageVi = await _translationService.TranslateAsync(address.Village!, _culture, "vi"),
                            DistrictVi = await _translationService.TranslateAsync(address.District!, _culture, "vi"),
                            CityVi = await _translationService.TranslateAsync(address.City!, _culture, "vi"),
                            CountryVi = await _translationService.TranslateAsync(address.Country!, _culture, "vi"),
                        };

                        await _db.ExecuteAsync(addressQuery, addressParameters, transaction);
                    }
                }

                if (request.IdentityInfo != null)
                {
                    string deleteIdentityInfoQuery = "DELETE FROM identity_info WHERE student_id = @StudentId";
                    await _db.ExecuteAsync(deleteIdentityInfoQuery, new { StudentId = id }, transaction);

                    string identityInfoQuery = @$"
                INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type, place_of_issue_vi, additional_info_vi, need_to_review) 
                VALUES (@StudentId, @Number, @PlaceOfIssue, @DateOfIssue, @ExpiryDate, @AdditionalInfo, @Type, @PlaceOfIssueVi, @AdditionalInfoVi, true)";

                    var identityInfoParameters = new
                    {
                        StudentId = id,
                        request.IdentityInfo.Number,
                        PlaceOfIssue = await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "en"),
                        request.IdentityInfo.DateOfIssue,
                        request.IdentityInfo.ExpiryDate,
                        AdditionalInfo = request.IdentityInfo.AdditionalInfo,
                        request.IdentityInfo.Type,
                        PlaceOfIssueVi = await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "vi"),
                        AdditionalInfoVi = request.IdentityInfo.AdditionalInfo,
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
                INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality, gender_vi, nationality_vi, need_to_review) 
                VALUES (@Id, @FullName, @DateOfBirth, @Gender, @FacultyId, @IntakeYear, @ProgramId, @Email, @PhoneNumber, @StatusId, @Nationality, @GenderVi, @NationalityVi, true)";

                var studentParameters = new
                {
                    Id = studentId,
                    request.FullName,
                    request.DateOfBirth,
                    Gender = TranslateGender(request.Gender!, _culture, "en"),
                    GenderVi = TranslateGender(request.Gender!, _culture, "vi"),
                    request.FacultyId,
                    request.IntakeYear,
                    request.ProgramId,
                    request.Email,
                    request.PhoneNumber,
                    request.StatusId,
                    Nationality = await _translationService.TranslateAsync(request.Nationality!, _culture, "en"),
                    NationalityVi = await _translationService.TranslateAsync(request.Nationality!, _culture, "vi"),
                };

                var studentCount = await _db.ExecuteAsync(studentQuery, studentParameters, transaction);

                if (studentCount == 0)
                {
                    throw new OperationFailedException(_localizer["add_student_failed"]);
                }

                if (request.Addresses != null && request.Addresses.Any())
                {
                    string addressQuery = @$"
                        INSERT INTO addresses (student_id, other, village, district, city, country, type, other_vi, village_vi, district_vi, city_vi, country_vi, need_to_review) 
                        VALUES (@StudentId, @Other, @Village, @District, @City, @Country, @Type, @OtherVi, @VillageVi, @DistrictVi, @CityVi, @CountryVi, true)";

                    foreach (var address in request.Addresses)
                    {
                        var addressParameters = new
                        {
                            StudentId = studentId,
                            Other = await _translationService.TranslateAsync(address.Other!, _culture, "en"),
                            Village = await _translationService.TranslateAsync(address.Village!, _culture, "en"),
                            District = await _translationService.TranslateAsync(address.District!, _culture, "en"),
                            City = await _translationService.TranslateAsync(address.City!, _culture, "en"),
                            Country = await _translationService.TranslateAsync(address.Country!, _culture, "en"),
                            address.Type,
                            OtherVi = await _translationService.TranslateAsync(address.Other!, _culture, "vi"),
                            VillageVi = await _translationService.TranslateAsync(address.Village!, _culture, "vi"),
                            DistrictVi = await _translationService.TranslateAsync(address.District!, _culture, "vi"),
                            CityVi = await _translationService.TranslateAsync(address.City!, _culture, "vi"),
                            CountryVi = await _translationService.TranslateAsync(address.Country!, _culture, "vi"),
                        };

                        await _db.ExecuteAsync(addressQuery, addressParameters, transaction);
                    }
                }

                if (request.IdentityInfo != null)
                {
                    string identityInfoQuery = @$"
                        INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type, place_of_issue_vi, additional_info_vi, need_to_review) 
                        VALUES (@StudentId, @Number, @PlaceOfIssue, @DateOfIssue, @ExpiryDate, @AdditionalInfo, @Type, @PlaceOfIssueVi, @AdditionalInfoVi, true)";

                    var identityInfoParameters = new
                    {
                        StudentId = studentId,
                        request.IdentityInfo.Number,
                        PlaceOfIssue = await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "en"),
                        request.IdentityInfo.DateOfIssue,
                        request.IdentityInfo.ExpiryDate,
                        AdditionalInfo = request.IdentityInfo.AdditionalInfo,
                        request.IdentityInfo.Type,
                        PlaceOfIssueVi = await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "vi"),
                        AdditionalInfoVi = request.IdentityInfo.AdditionalInfo,
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
                        $"@ProgramId{index}, @Email{index}, @PhoneNumber{index}, @StatusId{index}, @Nationality{index}, @NationalityVi{index}, @GenderVi{index}, true)");

                    studentParameters.Add(new NpgsqlParameter($"@Id{index}", studentId));
                    studentParameters.Add(new NpgsqlParameter($"@FullName{index}", request.FullName));
                    studentParameters.Add(new NpgsqlParameter($"@DateOfBirth{index}", request.DateOfBirth));
                    studentParameters.Add(new NpgsqlParameter($"@Gender{index}", TranslateGender(request.Gender!, _culture, "en")));
                    studentParameters.Add(new NpgsqlParameter($"@GenderVi{index}", TranslateGender(request.Gender!, _culture, "vi")));
                    studentParameters.Add(new NpgsqlParameter($"@FacultyId{index}", request.FacultyId));
                    studentParameters.Add(new NpgsqlParameter($"@IntakeYear{index}", request.IntakeYear));
                    studentParameters.Add(new NpgsqlParameter($"@ProgramId{index}", request.ProgramId));
                    studentParameters.Add(new NpgsqlParameter($"@Email{index}", request.Email));
                    studentParameters.Add(new NpgsqlParameter($"@PhoneNumber{index}", request.PhoneNumber));
                    studentParameters.Add(new NpgsqlParameter($"@StatusId{index}", request.StatusId));
                    studentParameters.Add(new NpgsqlParameter($"@Nationality{index}", await _translationService.TranslateAsync(request.Nationality!, _culture, "en")));
                    studentParameters.Add(new NpgsqlParameter($"@NationalityVi{index}", await _translationService.TranslateAsync(request.Nationality!, _culture, "vi")));

                    // Address Table
                    int addressCount = 0;
                    foreach (var address in request.Addresses!)
                    {
                        addressValues.Add($"(@StudentId{index}_{addressCount}, @Other{index}_{addressCount}, @Village{index}_{addressCount}, @District{index}_{addressCount}, " +
                            $"@City{index}_{addressCount}, @Country{index}_{addressCount}, @Type{index}_{addressCount}, " +
                            $"@OtherVi{index}_{addressCount}, @VillageVi{index}_{addressCount}, @DistrictVi{index}_{addressCount}, @CityVi{index}_{addressCount}, @CountryVi{index}_{addressCount}, true)");

                        addressParameters.Add(new NpgsqlParameter($"@StudentId{index}_{addressCount}", studentId));
                        addressParameters.Add(new NpgsqlParameter($"@Other{index}_{addressCount}", await _translationService.TranslateAsync(address.Other!, _culture, "en")));
                        addressParameters.Add(new NpgsqlParameter($"@Village{index}_{addressCount}", await _translationService.TranslateAsync(address.Village!, _culture, "en")));
                        addressParameters.Add(new NpgsqlParameter($"@District{index}_{addressCount}", await _translationService.TranslateAsync(address.District!, _culture, "en")));
                        addressParameters.Add(new NpgsqlParameter($"@City{index}_{addressCount}", await _translationService.TranslateAsync(address.City!, _culture, "en")));
                        addressParameters.Add(new NpgsqlParameter($"@Country{index}_{addressCount}", await _translationService.TranslateAsync(address.Country!, _culture, "en")));
                        addressParameters.Add(new NpgsqlParameter($"@Type{index}_{addressCount}", address.Type));
                        addressParameters.Add(new NpgsqlParameter($"@OtherVi{index}_{addressCount}", await _translationService.TranslateAsync(address.Other!, _culture, "vi")));
                        addressParameters.Add(new NpgsqlParameter($"@VillageVi{index}_{addressCount}", await _translationService.TranslateAsync(address.Village!, _culture, "vi")));
                        addressParameters.Add(new NpgsqlParameter($"@DistrictVi{index}_{addressCount}", await _translationService.TranslateAsync(address.District!, _culture, "vi")));
                        addressParameters.Add(new NpgsqlParameter($"@CityVi{index}_{addressCount}", await _translationService.TranslateAsync(address.City!, _culture, "vi")));
                        addressParameters.Add(new NpgsqlParameter($"@CountryVi{index}_{addressCount}", await _translationService.TranslateAsync(address.Country!, _culture, "vi")));

                        addressCount++;
                    }

                    // Identity Info Table
                    identityValues.Add($"(@StudentId{index}, @Number{index}, @PlaceOfIssue{index}, @DateOfIssue{index}, @ExpiryDate{index}, @AdditionalInfo{index}, @Type{index}, @PlaceOfIssueVi{index}, @AdditionalInfoVi{index}, true)");

                    identityParameters.Add(new NpgsqlParameter($"@StudentId{index}", studentId));
                    identityParameters.Add(new NpgsqlParameter($"@Number{index}", request.IdentityInfo!.Number));
                    identityParameters.Add(new NpgsqlParameter($"@PlaceOfIssue{index}", await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "en")));
                    identityParameters.Add(new NpgsqlParameter($"@PlaceOfIssueVi{index}", await _translationService.TranslateAsync(request.IdentityInfo.PlaceOfIssue!, _culture, "vi")));
                    identityParameters.Add(new NpgsqlParameter($"@DateOfIssue{index}", request.IdentityInfo.DateOfIssue));
                    identityParameters.Add(new NpgsqlParameter($"@ExpiryDate{index}", request.IdentityInfo.ExpiryDate));
                    identityParameters.Add(new NpgsqlParameter($"@Type{index}", request.IdentityInfo.Type));

                    var additionalInfoParam = new NpgsqlParameter($"@AdditionalInfo{index}", NpgsqlDbType.Jsonb);
                    additionalInfoParam.Value = request.IdentityInfo.AdditionalInfo is null
                        ? (object)DBNull.Value  // Ensure it's cast to object
                        : JsonSerializer.Serialize(request.IdentityInfo.AdditionalInfo);

                    var additionalInfoViParam = new NpgsqlParameter($"@AdditionalInfoVi{index}", NpgsqlDbType.Jsonb);
                    additionalInfoViParam.Value = request.IdentityInfo.AdditionalInfo is null
                        ? (object)DBNull.Value  // Ensure it's cast to object
                        : JsonSerializer.Serialize(request.IdentityInfo.AdditionalInfo);

                    identityParameters.Add(additionalInfoParam);
                    identityParameters.Add(additionalInfoViParam);

                    index++;
                }

                // Insert into Students Table
                if (studentValues.Count > 0)
                {
                    string studentQuery = $@"
                    INSERT INTO students (id, full_name, date_of_birth, gender, faculty_id, intake_year, program_id, email, phone_number, status_id, nationality, nationality_vi, gender_vi, need_to_review) 
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
                    INSERT INTO addresses (student_id, other, village, district, city, country, type, other_vi, village_vi, district_vi, city_vi, country_vi, need_to_review)  
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
                    INSERT INTO identity_info (student_id, number, place_of_issue, date_of_issue, expiry_date, additional_info, type, place_of_issue_vi, additional_info_vi, need_to_review)  
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
