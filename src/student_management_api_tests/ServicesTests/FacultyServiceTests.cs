using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Models.DTO;
using student_management_api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace student_management_api_tests.ServicesTests;

public class FacultyServiceTests
{
    private readonly Mock<IFacultyRepository> _mockFacultyRepository;
    private readonly FacultyService _facultyService;

    public FacultyServiceTests()
    {
        _mockFacultyRepository = new Mock<IFacultyRepository>();
        _facultyService = new FacultyService(_mockFacultyRepository.Object);
    }

    #region GetAllFaculties Tests
    [Fact]
    public async Task GetAllFaculties_ReturnsAllFaculties()
    {
        // Arrange
        var expectedFaculties = new List<Faculty>
        {
            new Faculty
            {
                Id = 1,
                Name = "Computer Science"
            },
            new Faculty
            {
                Id = 2,
                Name = "Engineering"
            },
            new Faculty
            {
                Id = 3,
                Name = "Business"
            }
        };

        _mockFacultyRepository
            .Setup(repo => repo.GetAllFaculties())
            .ReturnsAsync(expectedFaculties);

        // Act
        var result = await _facultyService.GetAllFaculties();

        // Assert
        Assert.Equal(expectedFaculties, result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Computer Science", result[0].Name);
        Assert.Equal("Engineering", result[1].Name);
        Assert.Equal("Business", result[2].Name);
    }

    [Fact]
    public async Task GetAllFaculties_NoFacultiesExist_ReturnsEmptyList()
    {
        // Arrange
        _mockFacultyRepository
            .Setup(repo => repo.GetAllFaculties())
            .ReturnsAsync((List<Faculty>)null);

        // Act
        var result = await _facultyService.GetAllFaculties();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    #endregion

    #region UpdateFaculty Tests
    [Fact]
    public async Task UpdateFaculty_ValidFaculty_ReturnsUpdatedRowCount()
    {
        // Arrange
        var faculty = new Faculty
        {
            Id = 1,
            Name = "Updated Computer Science"
        };

        int expectedRowsAffected = 1;

        _mockFacultyRepository
            .Setup(repo => repo.UpdateFaculty(faculty))
            .ReturnsAsync(expectedRowsAffected);

        // Act
        var result = await _facultyService.UpdateFaculty(faculty);

        // Assert
        Assert.Equal(expectedRowsAffected, result);
        _mockFacultyRepository.Verify(
            repo => repo.UpdateFaculty(faculty),
            Times.Once()
        );
    }

    [Fact]
    public async Task UpdateFaculty_FacultyNotFound_ReturnsZero()
    {
        // Arrange
        var faculty = new Faculty
        {
            Id = 999, // Non-existent ID
            Name = "Non-existent Faculty"
        };

        int expectedRowsAffected = 0;

        _mockFacultyRepository
            .Setup(repo => repo.UpdateFaculty(faculty))
            .ReturnsAsync(expectedRowsAffected);

        // Act
        var result = await _facultyService.UpdateFaculty(faculty);

        // Assert
        Assert.Equal(expectedRowsAffected, result);
        _mockFacultyRepository.Verify(
            repo => repo.UpdateFaculty(faculty),
            Times.Once()
        );
    }
    #endregion

    #region AddFaculty Tests
    [Fact]
    public async Task AddFaculty_ValidName_ReturnsFacultyId()
    {
        // Arrange
        string facultyName = "Physics";
        int expectedFacultyId = 10;

        _mockFacultyRepository
            .Setup(repo => repo.AddFaculty(facultyName))
            .ReturnsAsync(expectedFacultyId);

        // Act
        var result = await _facultyService.AddFaculty(facultyName);

        // Assert
        Assert.Equal(expectedFacultyId, result);
        _mockFacultyRepository.Verify(
            repo => repo.AddFaculty(facultyName),
            Times.Once()
        );
    }

    [Fact]
    public async Task AddFaculty_InsertFailed_ThrowsException()
    {
        // Arrange
        string facultyName = "Physics";
        _mockFacultyRepository
            .Setup(repo => repo.AddFaculty(facultyName))
            .ThrowsAsync(new OperationFailedException("failed_to_add_faculty"));

        // Act & Assert
        await Assert.ThrowsAsync<OperationFailedException>(
            () => _facultyService.AddFaculty(facultyName)
        );
    }
    #endregion
}