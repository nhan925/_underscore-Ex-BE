using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using student_management_api.Services;

namespace student_management_api_tests.ServicesTests;

public class StudentStatusServiceTests
{
    private readonly Mock<IStudentStatusRepository> _mockStudentStatusRepository;
    private readonly StudentStatusService _studentStatusService;

    public StudentStatusServiceTests()
    {
        _mockStudentStatusRepository = new Mock<IStudentStatusRepository>();
        _studentStatusService = new StudentStatusService(_mockStudentStatusRepository.Object);
    }

    #region GetAllStudentStatuses Tests

    [Fact]
    public async Task GetAllStudentStatuses_WhenStatusesExist_ReturnsStatuses()
    {
        // Arrange
        var expectedStatuses = new List<StudentStatus>
            {
                new StudentStatus { Id = 1, Name = "Active" },
                new StudentStatus { Id = 2, Name = "Inactive" }
            };

        _mockStudentStatusRepository.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(expectedStatuses);

        // Act
        var result = await _studentStatusService.GetAllStudentStatuses();

        // Assert
        Assert.Equal(expectedStatuses.Count, result.Count);
        Assert.Equal(expectedStatuses[0].Id, result[0].Id);
        Assert.Equal(expectedStatuses[0].Name, result[0].Name);
        Assert.Equal(expectedStatuses[1].Id, result[1].Id);
        Assert.Equal(expectedStatuses[1].Name, result[1].Name);
    }

    [Fact]
    public async Task GetAllStudentStatuses_WhenNoStatusesExist_ReturnsEmptyList()
    {
        // Arrange
        _mockStudentStatusRepository.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(new List<StudentStatus>());

        // Act
        var result = await _studentStatusService.GetAllStudentStatuses();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region UpdateStudentStatus Tests

    [Fact]
    public async Task UpdateStudentStatus_WhenStatusExists_ReturnsUpdatedCount()
    {
        // Arrange
        var studentStatus = new StudentStatus { Id = 1, Name = "Active" };
        _mockStudentStatusRepository.Setup(repo => repo.UpdateStudentStatus(studentStatus))
            .ReturnsAsync(1);

        // Act
        var result = await _studentStatusService.UpdateStudentStatus(studentStatus);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task UpdateStudentStatus_WhenStatusNotFound_ThrowsException()
    {
        // Arrange
        var studentStatus = new StudentStatus { Id = 999, Name = "Nonexistent" };
        _mockStudentStatusRepository.Setup(repo => repo.UpdateStudentStatus(studentStatus))
            .ThrowsAsync(new Exception("student status not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _studentStatusService.UpdateStudentStatus(studentStatus));
        Assert.Equal("student status not found", exception.Message);
    }

    #endregion

    #region AddStudentStatus Tests

    [Fact]
    public async Task AddStudentStatus_WhenStatusAdded_ReturnsNewId()
    {
        // Arrange
        var statusName = "Active";
        _mockStudentStatusRepository.Setup(repo => repo.AddStudentStatus(statusName))
            .ReturnsAsync(1);

        // Act
        var result = await _studentStatusService.AddStudentStatus(statusName);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task AddStudentStatus_WhenAddFails_ThrowsException()
    {
        // Arrange
        var statusName = "FailedStatus";
        _mockStudentStatusRepository.Setup(repo => repo.AddStudentStatus(statusName))
            .ThrowsAsync(new Exception("failed to add student status"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _studentStatusService.AddStudentStatus(statusName));
        Assert.Equal("failed to add student status", exception.Message);
    }

    #endregion
}
