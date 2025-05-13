using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using student_management_api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace student_management_api_tests.ServicesTests;

public class StudyProgramServiceTests
{
    private readonly Mock<IStudyProgramRepository> _mockStudyProgramRepository;
    private readonly StudyProgramService _studyProgramService;

    public StudyProgramServiceTests()
    {
        _mockStudyProgramRepository = new Mock<IStudyProgramRepository>();
        _studyProgramService = new StudyProgramService(_mockStudyProgramRepository.Object);
    }

    #region GetAllPrograms Tests

    [Fact]
    public async Task GetAllPrograms_WhenProgramsExist_ReturnsPrograms()
    {
        // Arrange
        var expectedPrograms = new List<StudyProgram>
            {
                new StudyProgram { Id = 1, Name = "Program 1" },
                new StudyProgram { Id = 2, Name = "Program 2" }
            };
        _mockStudyProgramRepository.Setup(repo => repo.GetAllPrograms())
            .ReturnsAsync(expectedPrograms);

        // Act
        var result = await _studyProgramService.GetAllPrograms();

        // Assert
        Assert.Equal(expectedPrograms.Count, result.Count);
        Assert.Equal(expectedPrograms[0].Id, result[0].Id);
        Assert.Equal(expectedPrograms[0].Name, result[0].Name);
        Assert.Equal(expectedPrograms[1].Id, result[1].Id);
        Assert.Equal(expectedPrograms[1].Name, result[1].Name);
    }

    [Fact]
    public async Task GetAllPrograms_WhenNoProgramsExist_ReturnsEmptyList()
    {
        // Arrange
        _mockStudyProgramRepository.Setup(repo => repo.GetAllPrograms())
            .ReturnsAsync(new List<StudyProgram>());

        // Act
        var result = await _studyProgramService.GetAllPrograms();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region UpdateProgram Tests

    [Fact]
    public async Task UpdateProgram_WhenProgramIsValid_ReturnsAffectedRows()
    {
        // Arrange
        var programToUpdate = new StudyProgram { Id = 1, Name = "Updated Program" };
        var expectedAffectedRows = 1; 

        _mockStudyProgramRepository.Setup(repo => repo.UpdateProgram(programToUpdate))
            .ReturnsAsync(expectedAffectedRows);

        // Act
        var result = await _studyProgramService.UpdateProgram(programToUpdate);

        // Assert
        Assert.Equal(expectedAffectedRows, result);
    }

    [Fact]
    public async Task UpdateProgram_WhenProgramNotFound_ThrowsException()
    {
        // Arrange
        var programToUpdate = new StudyProgram { Id = 999, Name = "Nonexistent Program" };

        _mockStudyProgramRepository.Setup(repo => repo.UpdateProgram(programToUpdate))
            .ThrowsAsync(new Exception("program not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _studyProgramService.UpdateProgram(programToUpdate));
        Assert.Equal("program not found", exception.Message);
    }

    #endregion

    #region AddProgram Tests

    [Fact]
    public async Task AddProgram_WhenProgramAdded_ReturnsNewId()
    {
        // Arrange
        var programName = "New Program";
        var expectedNewId = 1; 

        _mockStudyProgramRepository.Setup(repo => repo.AddProgram(programName))
            .ReturnsAsync(expectedNewId);

        // Act
        var result = await _studyProgramService.AddProgram(programName);

        // Assert
        Assert.Equal(expectedNewId, result);
    }

    [Fact]
    public async Task AddProgram_WhenAddFails_ThrowsException()
    {
        // Arrange
        var programName = "Failed Program";

        _mockStudyProgramRepository.Setup(repo => repo.AddProgram(programName))
            .ThrowsAsync(new Exception("failed to add program"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _studyProgramService.AddProgram(programName));
        Assert.Equal("failed to add program", exception.Message);
    }

    #endregion
}
