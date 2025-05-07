using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using student_management_api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace student_management_api_tests.ServicesTests;

public class LecturersServiceTests
{
    private readonly Mock<ILecturersRepository> _mockLecturersRepository;
    private readonly LecturersService _lecturersService;

    public LecturersServiceTests()
    {
        _mockLecturersRepository = new Mock<ILecturersRepository>();
        _lecturersService = new LecturersService(_mockLecturersRepository.Object);
    }

    #region GetAllLecturers Tests
    [Fact]
    public async Task GetAllLecturers_ReturnsAllLecturers()
    {
        // Arrange
        var expectedLecturers = new List<Lecturer>
        {
            new Lecturer
            {
                Id = "LC001",
                FullName = "Dr. John Smith",
                DateOfBirth = new DateTime(1980, 5, 15),
                Gender = "Male",
                Email = "john.smith@example.edu",
                PhoneNumber = "1234567890",
                FacultyId = 1
            },
            new Lecturer
            {
                Id = "LC002",
                FullName = "Dr. Jane Doe",
                DateOfBirth = new DateTime(1985, 8, 22),
                Gender = "Female",
                Email = "jane.doe@example.edu",
                PhoneNumber = "0987654321",
                FacultyId = 2
            }
        };

        _mockLecturersRepository
            .Setup(repo => repo.GetAllLecturers())
            .ReturnsAsync(expectedLecturers);

        // Act
        var result = await _lecturersService.GetAllLecturers();

        // Assert
        Assert.Equal(expectedLecturers, result);
        Assert.Equal(2, result.Count);
        Assert.Equal("LC001", result[0].Id);
        Assert.Equal("Dr. John Smith", result[0].FullName);
        Assert.Equal("LC002", result[1].Id);
        Assert.Equal("Dr. Jane Doe", result[1].FullName);
        _mockLecturersRepository.Verify(repo => repo.GetAllLecturers(), Times.Once());
    }

    [Fact]
    public async Task GetAllLecturers_WhenNoLecturers_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Lecturer>();

        _mockLecturersRepository
            .Setup(repo => repo.GetAllLecturers())
            .ReturnsAsync(emptyList);

        // Act
        var result = await _lecturersService.GetAllLecturers();

        // Assert
        Assert.Empty(result);
        Assert.IsType<List<Lecturer>>(result);
        _mockLecturersRepository.Verify(repo => repo.GetAllLecturers(), Times.Once());
    }

    [Fact]
    public async Task GetAllLecturers_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Database connection error");

        _mockLecturersRepository
            .Setup(repo => repo.GetAllLecturers())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _lecturersService.GetAllLecturers());

        Assert.Equal(expectedException.Message, exception.Message);
        _mockLecturersRepository.Verify(repo => repo.GetAllLecturers(), Times.Once());
    }
    #endregion
}