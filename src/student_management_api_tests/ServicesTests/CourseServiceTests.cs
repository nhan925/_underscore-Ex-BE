using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Exceptions;
using student_management_api.Models.DTO;
using student_management_api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace student_management_api_tests.ServicesTests;

public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly CourseService _courseService;

    public CourseServiceTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _courseService = new CourseService(_mockCourseRepository.Object);
    }

    #region GetAllCourses Tests
    [Fact]
    public async Task GetAllCourses_ReturnsAllCourses()
    {
        // Arrange
        var expectedCourses = new List<Course>
        {
            new Course
            {
                Id = "CS101",
                Name = "Introduction to Programming",
                Credits = 4,
                FacultyId = 1,
                Description = "An introductory course to programming",
                CreatedAt = DateTime.Now,
                IsActive = true
            },
            new Course
            {
                Id = "CS102",
                Name = "Data Structures",
                Credits = 4,
                FacultyId = 1,
                Description = "A course on data structures",
                CreatedAt = DateTime.Now,
                IsActive = true
            }
        };

        _mockCourseRepository
            .Setup(repo => repo.GetAllCourses())
            .ReturnsAsync(expectedCourses);

        // Act
        var result = await _courseService.GetAllCourses();

        // Assert
        Assert.Equal(expectedCourses, result);
        Assert.Equal(2, result.Count);
        Assert.Equal("CS101", result[0].Id);
        Assert.Equal("CS102", result[1].Id);
    }

    [Fact]
    public async Task GetAllCourses_NoCoursesExist_ReturnsEmptyList()
    {
        // Arrange
        _mockCourseRepository
            .Setup(repo => repo.GetAllCourses())
            .ReturnsAsync((List<Course>)null);

        // Act
        var result = await _courseService.GetAllCourses();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    #endregion

    #region GetCourseById Tests
    [Fact]
    public async Task GetCourseById_ValidId_ReturnsCourse()
    {
        // Arrange
        string courseId = "CS101";
        var expectedCourse = new Course
        {
            Id = courseId,
            Name = "Introduction to Programming",
            Credits = 4,
            FacultyId = 1,
            Description = "An introductory course to programming",
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        _mockCourseRepository
            .Setup(repo => repo.GetCourseById(courseId))
            .ReturnsAsync(expectedCourse);

        // Act
        var result = await _courseService.GetCourseById(courseId);

        // Assert
        Assert.Equal(expectedCourse, result);
        Assert.Equal(courseId, result.Id);
        Assert.Equal("Introduction to Programming", result.Name);
        Assert.Equal(4, result.Credits);
    }

    [Fact]
    public async Task GetCourseById_InvalidId_ReturnsEmptyCourse()
    {
        // Arrange
        string courseId = "INVALID";

        _mockCourseRepository
            .Setup(repo => repo.GetCourseById(courseId))
            .ReturnsAsync((Course)null);

        // Act
        var result = await _courseService.GetCourseById(courseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(default, result.Id);
        Assert.Equal(default, result.Name);
    }
    #endregion

    #region UpdateCourseById Tests
    [Fact]
    public async Task UpdateCourseById_ValidCourse_ReturnsUpdatedRowCount()
    {
        // Arrange
        var course = new Course
        {
            Id = "CS101",
            Name = "Updated Programming Course",
            Credits = 5,
            FacultyId = 1,
            Description = "An updated course",
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        int expectedRowsAffected = 1;

        _mockCourseRepository
            .Setup(repo => repo.UpdateCourseById(course))
            .ReturnsAsync(expectedRowsAffected);

        // Act
        var result = await _courseService.UpdateCourseById(course);

        // Assert
        Assert.Equal(expectedRowsAffected, result);
        _mockCourseRepository.Verify(
            repo => repo.UpdateCourseById(course),
            Times.Once()
        );
    }
    #endregion

    #region AddCourse Tests
    [Fact]
    public async Task AddCourse_ValidCourse_ReturnsAddedRowCount()
    {
        // Arrange
        var course = new Course
        {
            Id = "CS103",
            Name = "Algorithms",
            Credits = 4,
            FacultyId = 1,
            Description = "A course on algorithms",
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        int expectedRowsAffected = 1;

        _mockCourseRepository
            .Setup(repo => repo.AddCourse(course))
            .ReturnsAsync(expectedRowsAffected);

        // Act
        var result = await _courseService.AddCourse(course);

        // Assert
        Assert.Equal(expectedRowsAffected, result);
        _mockCourseRepository.Verify(
            repo => repo.AddCourse(course),
            Times.Once()
        );
    }
    #endregion

    #region DeleteCourseById Tests
    [Fact]
    public async Task DeleteCourseById_ValidRequest_CallsRepository()
    {
        // Arrange
        string courseId = "CS101";
        var course = new Course
        {
            Id = courseId,
            Name = "Introduction to Programming",
            Credits = 4,
            FacultyId = 1,
            Description = "An introductory course to programming",
            CreatedAt = DateTime.UtcNow.AddMinutes(-20), // Created 20 minutes ago
            IsActive = true
        };

        _mockCourseRepository
            .Setup(repo => repo.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mockCourseRepository
            .Setup(repo => repo.DeleteCourseById(courseId))
            .ReturnsAsync(courseId);

        // Act
        var result = await _courseService.DeleteCourseById(courseId);

        // Assert
        Assert.Equal(courseId, result);
        _mockCourseRepository.Verify(
            repo => repo.DeleteCourseById(courseId),
            Times.Once()
        );
    }

    [Fact]
    public async Task DeleteCourseById_InactiveCourse_ThrowsException()
    {
        // Arrange
        string courseId = "CS101";
        var course = new Course
        {
            Id = courseId,
            Name = "Introduction to Programming",
            Credits = 4,
            FacultyId = 1,
            Description = "An introductory course to programming",
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            IsActive = false // Course already deleted
        };

        _mockCourseRepository
            .Setup(repo => repo.GetCourseById(courseId))
            .ReturnsAsync(course);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _courseService.DeleteCourseById(courseId));

        Assert.Equal("The course has been deleted, you cannot delete it again", exception.Message);
        _mockCourseRepository.Verify(
            repo => repo.DeleteCourseById(courseId),
            Times.Never()
        );
    }

    [Fact]
    public async Task DeleteCourseById_CourseOlderThan30Minutes_ThrowsException()
    {
        // Arrange
        string courseId = "CS101";
        var course = new Course
        {
            Id = courseId,
            Name = "Introduction to Programming",
            Credits = 4,
            FacultyId = 1,
            Description = "An introductory course to programming",
            CreatedAt = DateTime.UtcNow.AddMinutes(-40), // Created 40 minutes ago
            IsActive = true
        };

        _mockCourseRepository
            .Setup(repo => repo.GetCourseById(courseId))
            .ReturnsAsync(course);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _courseService.DeleteCourseById(courseId));

        Assert.Equal("Exceeded 30 minutes, cannot delete", exception.Message);
        _mockCourseRepository.Verify(
            repo => repo.DeleteCourseById(courseId),
            Times.Never()
        );
    }
    #endregion

    #region CheckStudentExistFromCourse Tests
    [Fact]
    public async Task CheckStudentExistFromCourse_StudentsExist_ReturnsTrue()
    {
        // Arrange
        string courseId = "CS101";

        _mockCourseRepository
            .Setup(repo => repo.CheckStudentExistFromCourse(courseId))
            .ReturnsAsync(true);

        // Act
        var result = await _courseService.CheckStudentExistFromCourse(courseId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckStudentExistFromCourse_NoStudentsExist_ReturnsFalse()
    {
        // Arrange
        string courseId = "CS101";

        _mockCourseRepository
            .Setup(repo => repo.CheckStudentExistFromCourse(courseId))
            .ReturnsAsync(false);

        // Act
        var result = await _courseService.CheckStudentExistFromCourse(courseId);

        // Assert
        Assert.False(result);
    }
    #endregion
}