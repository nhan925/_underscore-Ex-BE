using Moq;
using Xunit;
using student_management_api.Services;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.CourseClass;
using student_management_api.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace student_management_api_tests.ServicesTests;

public class CourseClassServiceTests
{
    private readonly Mock<ICourseClassRepository> _mockRepository;
    private readonly CourseClassService _service;

    public CourseClassServiceTests()
    {
        _mockRepository = new Mock<ICourseClassRepository>();
        _service = new CourseClassService(_mockRepository.Object);
    }

    #region AddCourseClass Tests
    [Fact]
    public async Task AddCourseClass_ValidCourseClass_ReturnsSuccessMessage()
    {
        // Arrange
        var courseClass = new CourseClass
        {
            Id = "1",
            CourseId = "101",
            SemesterId = 1,
            LecturerId = "L1",
            MaxStudents = 30,
            Schedule = "Mon-Wed",
            Room = "A101"
        };
        _mockRepository.Setup(repo => repo.AddCourseClass(courseClass))
            .ReturnsAsync("Success");

        // Act
        var result = await _service.AddCourseClass(courseClass);

        // Assert
        Assert.Equal("Success", result);
        _mockRepository.Verify(repo => repo.AddCourseClass(courseClass), Times.Once);
    }
    #endregion

    #region GetAllCourseClassesBySemester Tests
    [Fact]
    public async Task GetAllCourseClassesBySemester_ValidSemesterId_ReturnsCourseClasses()
    {
        // Arrange
        int semesterId = 1;
        var expectedClasses = new List<GetCourseClassResult>
        {
            new GetCourseClassResult
            {
                Id = "1",
                MaxStudents = 30,
                Schedule = "Mon-Wed",
                Room = "A101",
                Course = new Course { Id = "101", Name = "Math" },
                Semester = new Semester { Id = 1, SemesterNum = 1 },
                Lecturer = new Lecturer { Id = "L1", FullName = "John Doe" }
            }
        };
        _mockRepository.Setup(repo => repo.GetAllCourseClassesBySemester(semesterId))
            .ReturnsAsync(expectedClasses);

        // Act
        var result = await _service.GetAllCourseClassesBySemester(semesterId);

        // Assert
        Assert.Equal(expectedClasses, result);
        _mockRepository.Verify(repo => repo.GetAllCourseClassesBySemester(semesterId), Times.Once);
    }

    [Fact]
    public async Task GetAllCourseClassesBySemester_NoClassesFound_ReturnsEmptyList()
    {
        // Arrange
        int semesterId = 1;
        _mockRepository.Setup(repo => repo.GetAllCourseClassesBySemester(semesterId))
            .ReturnsAsync((List<GetCourseClassResult>?)null);

        // Act
        var result = await _service.GetAllCourseClassesBySemester(semesterId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(repo => repo.GetAllCourseClassesBySemester(semesterId), Times.Once);
    }
    #endregion

    #region GetStudentsInClass Tests
    [Fact]
    public async Task GetStudentsInClass_ValidRequest_ReturnsStudents()
    {
        // Arrange
        var request = new GetStudentsInClassRequest
        {
            ClassId = "1",
            CourseId = "101",
            SemesterId = 1
        };
        var expectedStudents = new List<StudentInClass>
        {
            new StudentInClass { Id = "S1", FullName = "Alice", Grade = 90, Status = "Active" }
        };
        _mockRepository.Setup(repo => repo.GetStudentsInClass(request))
            .ReturnsAsync(expectedStudents);

        // Act
        var result = await _service.GetStudentsInClass(request);

        // Assert
        Assert.Equal(expectedStudents, result);
        _mockRepository.Verify(repo => repo.GetStudentsInClass(request), Times.Once);
    }

    [Fact]
    public async Task GetStudentsInClass_NoStudentsFound_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetStudentsInClassRequest
        {
            ClassId = "1",
            CourseId = "101",
            SemesterId = 1
        };
        _mockRepository.Setup(repo => repo.GetStudentsInClass(request))
            .ReturnsAsync((List<StudentInClass>?)null);

        // Act
        var result = await _service.GetStudentsInClass(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(repo => repo.GetStudentsInClass(request), Times.Once);
    }
    #endregion
}
