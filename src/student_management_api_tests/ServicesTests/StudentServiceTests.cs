using ClosedXML.Excel;
using Microsoft.Extensions.Localization;
using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Localization;
using student_management_api.Models.DTO;
using student_management_api.Models.Student;
using student_management_api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace student_management_api_tests.ServicesTests;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly StudentService _studentService;
    private readonly Mock<IStringLocalizer<Messages>> _mockLocalizer;

    public StudentServiceTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockLocalizer = new Mock<IStringLocalizer<Messages>>();
        _studentService = new StudentService(_mockStudentRepository.Object, _mockLocalizer.Object);
    }

    #region AddStudent Tests

    [Fact]
    public async Task AddStudent_WhenSuccessful_ReturnsStudentId()
    {
        // Arrange
        var request = new AddStudentRequest { FullName = "Jane Smith", Email = "jane.smith@example.com" };
        var expectedId = "ST12345";

        _mockStudentRepository
            .Setup(repo => repo.AddStudent(It.IsAny<AddStudentRequest>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _studentService.AddStudent(request);

        // Assert
        Assert.Equal(expectedId, result);
        _mockStudentRepository.Verify(repo => repo.AddStudent(request), Times.Once);
    }

    [Fact]
    public async Task AddStudent_WhenRepositoryFails_ThrowsException()
    {
        // Arrange
        var request = new AddStudentRequest { FullName = "Jane Smith" };

        _mockStudentRepository
            .Setup(repo => repo.AddStudent(It.IsAny<AddStudentRequest>()))
            .ReturnsAsync((string)null!);

        // Act
        await Assert.ThrowsAsync<Exception>(() =>
            _studentService.AddStudent(request));
    }

    #endregion

    #region DeleteStudentById Tests

    [Fact]
    public async Task DeleteStudentById_WhenStudentExists_ReturnsDeleteCount()
    {
        // Arrange
        var studentId = "ST12345";
        var expectedCount = 1;

        _mockStudentRepository
            .Setup(repo => repo.DeleteStudentById(studentId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _studentService.DeleteStudentById(studentId);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockStudentRepository.Verify(repo => repo.DeleteStudentById(studentId), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentById_WhenStudentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = "ST12345";

        _mockStudentRepository
            .Setup(repo => repo.DeleteStudentById(studentId))
            .ReturnsAsync(0);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _studentService.DeleteStudentById(studentId));
    }

    #endregion

    #region GetStudentById Tests

    [Fact]
    public async Task GetStudentById_WhenStudentExists_ReturnsStudent()
    {
        // Arrange
        var studentId = "ST12345";
        var expectedStudent = new Student
        {
            Id = studentId,
            FullName = "Jane Smith",
            Email = "jane.smith@example.com"
        };

        _mockStudentRepository
            .Setup(repo => repo.GetStudentById(studentId))
            .ReturnsAsync(expectedStudent);

        // Act
        var result = await _studentService.GetStudentById(studentId);

        // Assert
        Assert.Equal(expectedStudent, result);
        Assert.Equal(expectedStudent.FullName, result!.FullName);
        Assert.Equal(expectedStudent.Email, result.Email);
    }

    [Fact]
    public async Task GetStudentById_WhenStudentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = "ST12345";

        _mockStudentRepository
            .Setup(repo => repo.GetStudentById(studentId))
            .ReturnsAsync((Student)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _studentService.GetStudentById(studentId));
    }

    #endregion

    #region GetStudents Tests

    [Fact]
    public async Task GetStudents_ValidParameters_ReturnsPagedResult()
    {
        // Arrange
        int page = 1;
        int pageSize = 10;
        string search = "Smith";
        var filter = new StudentFilter { FacultyIds = new() { 1 } };

        var expectedResult = new PagedResult<SimplifiedStudent>
        {
            Items = new List<SimplifiedStudent>
        {
            new SimplifiedStudent { Id = "ST12345", FullName = "Jane Smith" },
            new SimplifiedStudent { Id = "ST67890", FullName = "John Smith" }
        },
            TotalCount = 2
        };

        _mockStudentRepository
            .Setup(repo => repo.GetStudents(page, pageSize, search, filter))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _studentService.GetStudents(page, pageSize, search, filter);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedResult.TotalCount, result.TotalCount);
        Assert.Equal(expectedResult.Items.Count, result.Items.Count);
    }

    #endregion

    #region UpdateStudentById Tests

    [Fact]
    public async Task UpdateStudentById_WhenStudentExists_ReturnsUpdateCount()
    {
        // Arrange
        var studentId = "ST12345";
        var request = new UpdateStudentRequest
        {
            FullName = "Jane Smith Updated",
            Email = "jane.updated@example.com"
        };
        var expectedCount = 1;

        _mockStudentRepository
            .Setup(repo => repo.UpdateStudentById(studentId, request))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _studentService.UpdateStudentById(studentId, request);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockStudentRepository.Verify(repo => repo.UpdateStudentById(studentId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentById_WhenStudentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var studentId = "ST12345";
        var request = new UpdateStudentRequest { FullName = "Jane Smith Updated" };

        _mockStudentRepository
            .Setup(repo => repo.UpdateStudentById(studentId, request))
            .ReturnsAsync(0);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _studentService.UpdateStudentById(studentId, request));
    }

    #endregion

    #region AddStudents Tests

    [Fact]
    public async Task AddStudents_WithValidRequests_CallsRepositoryMethodOnce()
    {
        // Arrange
        var requests = new List<AddStudentRequest>
        {
            new AddStudentRequest { FullName = "Jane Smith" },
            new AddStudentRequest { FullName = "John Doe" }
        };

        _mockStudentRepository
            .Setup(repo => repo.AddStudents(It.IsAny<List<AddStudentRequest>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _studentService.AddStudents(requests);

        // Assert
        _mockStudentRepository.Verify(repo => repo.AddStudents(requests), Times.Once);
    }

    #endregion

    #region ExportToExcel Tests

    [Fact]
    public async Task ExportToExcel_WhenCalled_ReturnsValidExcelWorkbookStream()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student
            {
                Id = "ST12345",
                FullName = "Jane Smith",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gender = "Female",
                FacultyId = 1,
                IntakeYear = 2020,
                ProgramId = 2,
                Email = "jane.smith@example.com",
                PhoneNumber = "1234567890",
                StatusId = 1,
                Nationality = "USA",
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Type = "Home",
                        Village = "Beverly Hills",
                        District = "Los Angeles",
                        City = "Los Angeles",
                        Country = "USA"
                    }
                },
                IdentityInfo = new IdentityInfo
                {
                    Type = "Passport",
                    Number = "AB123456",
                    DateOfIssue = new DateTime(2018, 1, 1),
                    PlaceOfIssue = "Los Angeles",
                    ExpiryDate = new DateTime(2028, 1, 1),
                    AdditionalInfo = new Dictionary<string, string>
                    {
                        { "has_chip", "Yes" },
                        { "note", "Valid" },
                        { "country_of_issue", "USA" }
                    }
                }
            }
        };

        _mockStudentRepository
            .Setup(repo => repo.GetAllStudents())
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.ExportToExcel();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MemoryStream>(result);

        // Test that the stream contains a valid Excel workbook
        result.Position = 0;
        using (var workbook = new XLWorkbook(result))
        {
            Assert.Equal(3, workbook.Worksheets.Count); // Should have 3 sheets
            Assert.True(workbook.Worksheets.Contains("Students"));
            Assert.True(workbook.Worksheets.Contains("Addresses"));
            Assert.True(workbook.Worksheets.Contains("IdentityInfo"));
        }
    }

    #endregion

    #region ConvertExcelToJson Tests

    [Fact]
    public async Task ConvertExcelToJson_WithValidExcelData_ReturnsDeserializableJsonString()
    {
        // Arrange
        // Create a test Excel file in memory
        using var workbook = new XLWorkbook();

        // Create Students sheet
        var studentSheet = workbook.Worksheets.Add("Students");
        studentSheet.Cell(1, 1).Value = "ID";
        studentSheet.Cell(1, 2).Value = "Full Name";
        studentSheet.Cell(1, 3).Value = "Date of Birth";
        studentSheet.Cell(1, 4).Value = "Gender";
        studentSheet.Cell(1, 5).Value = "Faculty ID";
        studentSheet.Cell(1, 6).Value = "Intake Year";
        studentSheet.Cell(1, 7).Value = "Program ID";
        studentSheet.Cell(1, 8).Value = "Email";
        studentSheet.Cell(1, 9).Value = "Phone Number";
        studentSheet.Cell(1, 10).Value = "Status ID";
        studentSheet.Cell(1, 11).Value = "Nationality";

        studentSheet.Cell(2, 1).Value = "ST12345";
        studentSheet.Cell(2, 2).Value = "Jane Smith";
        studentSheet.Cell(2, 3).Value = "1995-05-15";
        studentSheet.Cell(2, 4).Value = "Female";
        studentSheet.Cell(2, 5).Value = "1";
        studentSheet.Cell(2, 6).Value = "2020";
        studentSheet.Cell(2, 7).Value = "2";
        studentSheet.Cell(2, 8).Value = "jane.smith@example.com";
        studentSheet.Cell(2, 9).Value = "1234567890";
        studentSheet.Cell(2, 10).Value = "1";
        studentSheet.Cell(2, 11).Value = "USA";

        // Create Addresses sheet
        var addressSheet = workbook.Worksheets.Add("Addresses");
        addressSheet.Cell(1, 1).Value = "Student ID";
        addressSheet.Cell(1, 2).Value = "Type";
        addressSheet.Cell(1, 3).Value = "Other";
        addressSheet.Cell(1, 4).Value = "Village";
        addressSheet.Cell(1, 5).Value = "District";
        addressSheet.Cell(1, 6).Value = "City";
        addressSheet.Cell(1, 7).Value = "Country";

        addressSheet.Cell(2, 1).Value = "ST12345";
        addressSheet.Cell(2, 2).Value = "Home";
        addressSheet.Cell(2, 3).Value = "";
        addressSheet.Cell(2, 4).Value = "Beverly Hills";
        addressSheet.Cell(2, 5).Value = "Los Angeles";
        addressSheet.Cell(2, 6).Value = "Los Angeles";
        addressSheet.Cell(2, 7).Value = "USA";

        // Create IdentityInfo sheet
        var identitySheet = workbook.Worksheets.Add("IdentityInfo");
        identitySheet.Cell(1, 1).Value = "Student ID";
        identitySheet.Cell(1, 2).Value = "Type";
        identitySheet.Cell(1, 3).Value = "Number";
        identitySheet.Cell(1, 4).Value = "Date of Issue";
        identitySheet.Cell(1, 5).Value = "Place of Issue";
        identitySheet.Cell(1, 6).Value = "Expiry Date";
        identitySheet.Cell(1, 7).Value = "Has Chip";
        identitySheet.Cell(1, 8).Value = "Note";
        identitySheet.Cell(1, 9).Value = "Country of Issue";

        identitySheet.Cell(2, 1).Value = "ST12345";
        identitySheet.Cell(2, 2).Value = "Passport";
        identitySheet.Cell(2, 3).Value = "AB123456";
        identitySheet.Cell(2, 4).Value = "2018-01-01";
        identitySheet.Cell(2, 5).Value = "Los Angeles";
        identitySheet.Cell(2, 6).Value = "2028-01-01";
        identitySheet.Cell(2, 7).Value = "Yes";
        identitySheet.Cell(2, 8).Value = "Valid";
        identitySheet.Cell(2, 9).Value = "USA";

        var memStream = new MemoryStream();
        workbook.SaveAs(memStream);
        memStream.Position = 0;

        // Act
        var jsonResult = _studentService.ConvertExcelToJson(memStream);

        // Assert
        Assert.NotNull(jsonResult);
        Assert.Contains("Jane Smith", jsonResult);
        Assert.Contains("ST12345", jsonResult);
        Assert.Contains("AB123456", jsonResult);

        // Verify we can deserialize the JSON
        var students = JsonSerializer.Deserialize<List<Student>>(jsonResult);
        Assert.NotNull(students);
        Assert.Single(students);
        Assert.Equal("Jane Smith", students[0].FullName);
        Assert.Single(students[0].Addresses);
        Assert.Equal("Passport", students[0].IdentityInfo.Type);
    }

    #endregion

    #region ExportToJson Tests

    [Fact]
    public async Task ExportToJson_WhenCalled_ReturnsStreamWithCorrectJsonData()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student
            {
                Id = "ST12345",
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                Addresses = new List<Address>
                {
                    new Address { Type = "Home", City = "New York" }
                }
            }
        };

        _mockStudentRepository
            .Setup(repo => repo.GetAllStudents())
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.ExportToJson();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MemoryStream>(result);

        // Verify content
        result.Position = 0;
        using var reader = new StreamReader(result);
        var jsonContent = await reader.ReadToEndAsync();

        Assert.Contains("Jane Smith", jsonContent);
        Assert.Contains("ST12345", jsonContent);
        Assert.Contains("jane.smith@example.com", jsonContent);
        Assert.Contains("New York", jsonContent);
    }

    #endregion
}
