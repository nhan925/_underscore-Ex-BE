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

public class YearAndSemesterServiceTests
{
    private readonly Mock<IYearAndSemesterRepository> _mockYearAndSemesterRepository;
    private readonly YearAndSemesterService _yearAndSemesterService;

    public YearAndSemesterServiceTests()
    {
        _mockYearAndSemesterRepository = new Mock<IYearAndSemesterRepository>();
        _yearAndSemesterService = new YearAndSemesterService(_mockYearAndSemesterRepository.Object);
    }

    #region GetAllYears Tests

    [Fact]
    public async Task GetAllYears_WhenYearsExist_ReturnsYears()
    {
        // Arrange
        var expectedYears = new List<Year>
            {
                new Year { Id = 1, Name = "2021" },
                new Year { Id = 2, Name = "2022" }
            };
        _mockYearAndSemesterRepository.Setup(repo => repo.GetAllYears())
            .ReturnsAsync(expectedYears);

        // Act
        var result = await _yearAndSemesterService.GetAllYears();

        // Assert
        Assert.Equal(expectedYears.Count, result.Count);
        Assert.Equal(expectedYears[0].Id, result[0].Id);
        Assert.Equal(expectedYears[0].Name, result[0].Name);
        Assert.Equal(expectedYears[1].Id, result[1].Id);
        Assert.Equal(expectedYears[1].Name, result[1].Name);
    }

    [Fact]
    public async Task GetAllYears_WhenNoYearsExist_ReturnsEmptyList()
    {
        // Arrange
        _mockYearAndSemesterRepository.Setup(repo => repo.GetAllYears())
            .ReturnsAsync(new List<Year>());

        // Act
        var result = await _yearAndSemesterService.GetAllYears();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetSemestersByYear Tests

    [Fact]
    public async Task GetSemestersByYear_WhenSemestersExist_ReturnsSemesters()
    {
        // Arrange
        var yearId = 2022;
        var expectedSemesters = new List<Semester>
    {
        new Semester { Id = 1, SemesterNum = 1, YearId = yearId, StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 5, 1) },
        new Semester { Id = 2, SemesterNum = 2, YearId = yearId, StartDate = new DateTime(2022, 9, 1), EndDate = new DateTime(2022, 12, 31) }
    };
        _mockYearAndSemesterRepository.Setup(repo => repo.GetSemestersByYear(yearId))
            .ReturnsAsync(expectedSemesters);

        // Act
        var result = await _yearAndSemesterService.GetSemestersByYear(yearId);

        // Assert
        Assert.Equal(expectedSemesters.Count, result.Count);

        // Assert all properties for the first semester
        Assert.Equal(expectedSemesters[0].Id, result[0].Id);
        Assert.Equal(expectedSemesters[0].SemesterNum, result[0].SemesterNum);
        Assert.Equal(expectedSemesters[0].YearId, result[0].YearId);
        Assert.Equal(expectedSemesters[0].StartDate, result[0].StartDate);
        Assert.Equal(expectedSemesters[0].EndDate, result[0].EndDate);

        // Assert all properties for the second semester
        Assert.Equal(expectedSemesters[1].Id, result[1].Id);
        Assert.Equal(expectedSemesters[1].SemesterNum, result[1].SemesterNum);
        Assert.Equal(expectedSemesters[1].YearId, result[1].YearId);
        Assert.Equal(expectedSemesters[1].StartDate, result[1].StartDate);
        Assert.Equal(expectedSemesters[1].EndDate, result[1].EndDate);
    }

    [Fact]
    public async Task GetSemestersByYear_WhenNoSemestersExist_ReturnsEmptyList()
    {
        // Arrange
        var yearId = 2022;
        _mockYearAndSemesterRepository.Setup(repo => repo.GetSemestersByYear(yearId))
            .ReturnsAsync(new List<Semester>());

        // Act
        var result = await _yearAndSemesterService.GetSemestersByYear(yearId);

        // Assert
        Assert.Empty(result);
    }

    #endregion
}
