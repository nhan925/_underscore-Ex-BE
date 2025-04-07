using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace student_management_api.Services;

public class StudentService: IStudentService
{
    private readonly IStudentRepository _studentRepository;
    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<string> AddStudent(AddStudentRequest request)
    {
        var studentId = await _studentRepository.AddStudent(request);
        if (studentId == null)
        {
            throw new Exception("failed to add student");
        }

        return studentId;
    }

    public async Task<int> DeleteStudentById(string id)
    {
        var count = await _studentRepository.DeleteStudentById(id);
        if (count == 0)
        {
            throw new Exception("student not found");
        }

        return count;
    }

    public async Task<Student?> GetStudentById(string id)
    {
        var student = await _studentRepository.GetStudentById(id);
        if (student == null)
        {
            throw new Exception("student not found");
        }

        return student;
    }

    public async Task<PagedResult<SimplifiedStudent>> GetStudents(int page, int pageSize, string? search, StudentFilter? filter)
    {
        var result = await _studentRepository.GetStudents(page, pageSize, search, filter);
        return result;
    }

    public async Task<int> UpdateStudentById(string id, UpdateStudentRequest request)
    {
        var count = await _studentRepository.UpdateStudentById(id, request);
        if (count == 0)
        {
            throw new Exception("student not found");
        }

        return count;
    }

    public async Task AddStudents(List<AddStudentRequest> requests)
    {
        await _studentRepository.AddStudents(requests);
    }

    public Stream ExportToExcel()
    {
        var students = _studentRepository.GetAllStudents().Result;
        var memoryStream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            // Students Sheet
            var studentSheet = CreateSheet(workbook, "Students",
                "ID", "Full Name", "Date of Birth", "Gender", "Faculty ID", "Intake Year",
                "Program ID", "Email", "Phone Number", "Status ID", "Nationality");

            int studentRow = 2;
            foreach (var student in students)
            {
                AddRow(studentSheet, studentRow++, student.Id, student.FullName, student.DateOfBirth?.ToString("yyyy-MM-dd"),
                       student.Gender, student.FacultyId, student.IntakeYear, student.ProgramId,
                       student.Email, student.PhoneNumber, student.StatusId, student.Nationality);
            }

            // Addresses Sheet
            var addressSheet = CreateSheet(workbook, "Addresses",
                "Student ID", "Type", "Other", "Village", "District", "City", "Country");

            int addressRow = 2;
            foreach (var student in students)
            {
                foreach (var address in student.Addresses)
                {
                    AddRow(addressSheet, addressRow++, student.Id, address.Type, address.Other,
                           address.Village, address.District, address.City, address.Country);
                }
            }

            // 3️⃣ IdentityInfo Sheet
            var identitySheet = CreateSheet(workbook, "IdentityInfo",
                "Student ID", "Type", "Number", "Date of Issue", "Place of Issue", "Expiry Date",
                "Has Chip", "Note", "Country of Issue");

            int identityRow = 2;
            foreach (var student in students)
            {
                var idInfo = student.IdentityInfo;
                if (idInfo != null)
                {
                    idInfo.AdditionalInfo ??= new Dictionary<string, string>(); // Ensure it's not null
                    AddRow(identitySheet, identityRow++, student.Id, idInfo.Type, idInfo.Number,
                           idInfo.DateOfIssue?.ToString("yyyy-MM-dd"), idInfo.PlaceOfIssue,
                           idInfo.ExpiryDate?.ToString("yyyy-MM-dd"),
                           idInfo.AdditionalInfo.GetValueOrDefault("has_chip", ""),
                           idInfo.AdditionalInfo.GetValueOrDefault("note", ""),
                           idInfo.AdditionalInfo.GetValueOrDefault("country_of_issue", ""));
                }
            }

            workbook.SaveAs(memoryStream);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    // Helper function to create a sheet with headers
    private IXLWorksheet CreateSheet(XLWorkbook workbook, string sheetName, params string[] headers)
    {
        var sheet = workbook.Worksheets.Add(sheetName);
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }
        return sheet;
    }

    // Helper function to add a row
    private void AddRow(IXLWorksheet sheet, int row, params object[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            sheet.Cell(row, i + 1).Value = values[i]?.ToString() ?? "";
        }
    }

    public async Task<string> ImportExcelToJson(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var students = new List<Student>();

        // 1️⃣ Read Students Sheet
        var studentSheet = workbook.Worksheet("Students");
        var studentRows = studentSheet.RowsUsed().Skip(1); // Skip header

        foreach (var row in studentRows)
        {
            var student = new Student
            {
                Id = row.Cell(1).GetString(),
                FullName = row.Cell(2).GetString(),
                DateOfBirth = DateTime.TryParse(row.Cell(3).GetString(), out var dob) ? dob : (DateTime?)null,
                Gender = row.Cell(4).GetString(),
                FacultyId = int.Parse(row.Cell(5).GetString()),
                IntakeYear = int.Parse(row.Cell(6).GetString()),
                ProgramId = int.Parse(row.Cell(7).GetString()),
                Email = row.Cell(8).GetString(),
                PhoneNumber = row.Cell(9).GetString(),
                StatusId = int.Parse(row.Cell(10).GetString()),
                Nationality = row.Cell(11).GetString(),
                Addresses = new List<Address>(),
                IdentityInfo = new IdentityInfo()
            };
            students.Add(student);
        }

        // 2️⃣ Read Addresses Sheet
        var addressSheet = workbook.Worksheet("Addresses");
        var addressRows = addressSheet.RowsUsed().Skip(1);

        foreach (var row in addressRows)
        {
            var studentId = row.Cell(1).GetString();
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                student.Addresses.Add(new Address
                {
                    Type = row.Cell(2).GetString(),
                    Other = row.Cell(3).GetString(),
                    Village = row.Cell(4).GetString(),
                    District = row.Cell(5).GetString(),
                    City = row.Cell(6).GetString(),
                    Country = row.Cell(7).GetString()
                });
            }
        }

        // 3️⃣ Read IdentityInfo Sheet
        var identitySheet = workbook.Worksheet("IdentityInfo");
        var identityRows = identitySheet.RowsUsed().Skip(1);

        foreach (var row in identityRows)
        {
            var studentId = row.Cell(1).GetString();
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                student.IdentityInfo = new IdentityInfo
                {
                    Type = row.Cell(2).GetString(),
                    Number = row.Cell(3).GetString(),
                    DateOfIssue = DateTime.TryParse(row.Cell(4).GetString(), out var doi) ? doi : (DateTime?)null,
                    PlaceOfIssue = row.Cell(5).GetString(),
                    ExpiryDate = DateTime.TryParse(row.Cell(6).GetString(), out var exp) ? exp : (DateTime?)null,
                    AdditionalInfo = CreateAdditionalInfo(row, (7, "has_chip"), (8, "note"), (9, "country_of_issue"))
                };
            }
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(students, jsonOptions);
    }

    private Dictionary<string, string>? CreateAdditionalInfo(IXLRow row, params (int colIndex, string key)[] mappings)
    {
        var result = new Dictionary<string, string>();

        foreach (var (colIndex, key) in mappings)
        {
            var value = row.Cell(colIndex).GetString();
            if (!string.IsNullOrEmpty(value))
            {
                result[key] = value;
            }
        }

        return result.Count > 0 ? result : null;
    }


    public Stream ExportToJson()
    {
        var students = _studentRepository.GetAllStudents().Result;

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Fix Unicode
        };

        var json = JsonSerializer.Serialize(students, jsonOptions);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return memoryStream;
    }
}
