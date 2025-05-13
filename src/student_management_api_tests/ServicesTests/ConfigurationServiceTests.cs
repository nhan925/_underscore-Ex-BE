using Moq;
using Xunit;
using student_management_api.Services;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using student_management_api.Models.DTO;
using System;

namespace student_management_api_tests.ServicesTests;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _mockConfigRepo;
    private readonly Mock<IStudentStatusRepository> _mockStatusRepo;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        _mockConfigRepo = new Mock<IConfigurationRepository>();
        _mockStatusRepo = new Mock<IStudentStatusRepository>();
        _service = new ConfigurationService(_mockConfigRepo.Object, _mockStatusRepo.Object);
    }

    #region CheckEmailDomain Tests
    [Fact]
    public async Task CheckEmailDomain_NullDomainsConfig_ReturnsFalse()
    {
        _mockConfigRepo.Setup(repo => repo.GetEmailDomainsConfig())
            .ReturnsAsync((Configuration<List<string>>?)null); // Explicitly mark the type as nullable

        var result = await _service.CheckEmailDomain("test@example.com");

        Assert.False(result);
    }

    [Fact]
    public async Task CheckEmailDomain_ValidDomain_ReturnsTrue()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "example.com" },
            IsActive = true
        };
        _mockConfigRepo.Setup(repo => repo.GetEmailDomainsConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckEmailDomain("user@example.com");

        Assert.True(result);
    }

    [Fact]
    public async Task CheckEmailDomain_InactiveConfig_ReturnsTrue()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "example.com" },
            IsActive = false
        };
        _mockConfigRepo.Setup(repo => repo.GetEmailDomainsConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckEmailDomain("user@other.com");

        Assert.True(result);
    }

    [Fact]
    public async Task CheckEmailDomain_InvalidDomain_ReturnsFalse()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "example.com" },
            IsActive = true
        };
        _mockConfigRepo.Setup(repo => repo.GetEmailDomainsConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckEmailDomain("user@other.com");

        Assert.False(result);
    }
    #endregion

    #region CheckPhoneNumber Tests
    [Fact]
    public async Task CheckPhoneNumber_InvalidFormat_ReturnsFalse()
    {
        var result = await _service.CheckPhoneNumber("123456");

        Assert.False(result);
    }

    [Fact]
    public async Task CheckPhoneNumber_ValidNumber_ReturnsTrue()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "US" },
            IsActive = true
        };
        _mockConfigRepo.Setup(repo => repo.GetPhoneNumberCountryConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckPhoneNumber("+14155552671");

        Assert.True(result);
    }

    [Fact]
    public async Task CheckPhoneNumber_NullConfig_ReturnsFalse()
    {
        _mockConfigRepo.Setup(repo => repo.GetPhoneNumberCountryConfig())
            .ReturnsAsync((Configuration<List<string>>?)null);

        var result = await _service.CheckPhoneNumber("+14155552671");

        Assert.False(result);
    }

    [Fact]
    public async Task CheckPhoneNumber_InvalidForCountry_ReturnsFalse()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "US" },
            IsActive = true
        };
        _mockConfigRepo.Setup(repo => repo.GetPhoneNumberCountryConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckPhoneNumber("+919876543210"); // Invalid for US

        Assert.False(result);
    }

    [Fact]
    public async Task CheckPhoneNumber_InactiveConfig_ReturnsTrue()
    {
        var config = new Configuration<List<string>>
        {
            Value = new List<string> { "US" },
            IsActive = false
        };
        _mockConfigRepo.Setup(repo => repo.GetPhoneNumberCountryConfig())
            .ReturnsAsync(config);

        var result = await _service.CheckPhoneNumber("+919876543210");

        Assert.True(result);
    }
    #endregion

    #region GetEmailDomainsConfig Tests
    [Fact]
    public async Task GetEmailDomainsConfig_ValidCall_ReturnsConfig()
    {
        var expectedConfig = new Configuration<List<string>> { Value = new List<string> { "example.com" } };
        _mockConfigRepo.Setup(repo => repo.GetEmailDomainsConfig())
            .ReturnsAsync(expectedConfig);

        var result = await _service.GetEmailDomainsConfig();

        Assert.Equal(expectedConfig, result);
    }
    #endregion

    #region GetPhoneNumberCountryConfig Tests
    [Fact]
    public async Task GetPhoneNumberCountryConfig_ValidCall_ReturnsConfig()
    {
        var expectedConfig = new Configuration<List<string>> { Value = new List<string> { "US", "IN" } };
        _mockConfigRepo.Setup(repo => repo.GetPhoneNumberCountryConfig())
            .ReturnsAsync(expectedConfig);

        var result = await _service.GetPhoneNumberCountryConfig();

        Assert.Equal(expectedConfig, result);
    }
    #endregion

    #region GetStudentStatusConfig Tests
    [Fact]
    public async Task GetStudentStatusConfig_ConfigNotFound_ReturnsNull()
    {
        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync((Configuration<Dictionary<int, List<int>>>?)null);

        var result = await _service.GetStudentStatusConfig();

        Assert.Null(result);
    }
    #endregion

    #region TurnAllRulesOnOrOff Tests
    [Fact]
    public async Task TurnAllRulesOnOrOff_EnableRules_ReturnsUpdatedCount()
    {
        _mockConfigRepo.Setup(repo => repo.TurnAllRulesOnOrOff(true))
            .ReturnsAsync(1);

        var result = await _service.TurnAllRulesOnOrOff(true);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task TurnAllRulesOnOrOff_NoRulesUpdated_ReturnsZero()
    {
        _mockConfigRepo.Setup(repo => repo.TurnAllRulesOnOrOff(false))
            .ReturnsAsync(0);

        var result = await _service.TurnAllRulesOnOrOff(false);

        Assert.Equal(0, result);
    }
    #endregion

    #region UpdateEmailDomainsConfig Tests
    [Fact]
    public async Task UpdateEmailDomainsConfig_ValidConfig_ReturnsUpdatedCount()
    {
        var config = new Configuration<List<string>> { Value = new List<string> { "example.com" } };
        _mockConfigRepo.Setup(repo => repo.UpdateConfig(config))
            .ReturnsAsync(1);

        var result = await _service.UpdateEmailDomainsConfig(config);

        Assert.Equal(1, result);
    }
    #endregion

    #region UpdatePhoneNumberCountryConfig Tests
    [Fact]
    public async Task UpdatePhoneNumberCountryConfig_ValidConfig_ReturnsUpdatedCount()
    {
        var config = new Configuration<List<string>> { Value = new List<string> { "US", "IN" } };
        _mockConfigRepo.Setup(repo => repo.UpdateConfig(config))
            .ReturnsAsync(1);

        var result = await _service.UpdatePhoneNumberCountryConfig(config);

        Assert.Equal(1, result);
    }
    #endregion

    #region UpdateStudentStatusConfig Tests
    [Fact]
    public async Task UpdateStudentStatusConfig_AllStatusesExist_UpdatesConfigSuccessfully()
    {
        // Arrange
        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Id = 1,
            Type = "StudentStatusConfig",
            Value = new Dictionary<int, List<int>>
            {
                { 1, new List<int> { 2, 3 } },
                { 2, new List<int> { 3 } }
            },
            IsActive = true
        };

        // We extract 2 and 3 as they appear in the values
        var expectedStatusIds = new List<int> { 2, 3 };

        _mockStatusRepo.Setup(repo => repo.ReferenceStudentStatuses(It.Is<List<int>>(ids =>
            ids.Count == expectedStatusIds.Count &&
            ids.Contains(2) &&
            ids.Contains(3))))
            .ReturnsAsync(expectedStatusIds.Count);

        _mockConfigRepo.Setup(repo => repo.UpdateConfig(config))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateStudentStatusConfig(config);

        // Assert
        Assert.Equal(1, result);
        _mockStatusRepo.Verify(repo => repo.ReferenceStudentStatuses(It.IsAny<List<int>>()), Times.Once);
        _mockConfigRepo.Verify(repo => repo.UpdateConfig(config), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentStatusConfig_SomeStatusesDoNotExist_ThrowsException()
    {
        // Arrange
        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Id = 1,
            Type = "StudentStatusConfig",
            Value = new Dictionary<int, List<int>>
            {
                { 1, new List<int> { 2, 3, 4 } }
            },
            IsActive = true
        };

        // Return fewer statuses than requested to simulate missing statuses
        _mockStatusRepo.Setup(repo => repo.ReferenceStudentStatuses(It.IsAny<List<int>>()))
            .ReturnsAsync(2); // Only 2 statuses found, but we requested 3

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.UpdateStudentStatusConfig(config));
        Assert.Equal("One or more statuses are not found", exception.Message);
        _mockConfigRepo.Verify(repo => repo.UpdateConfig(config), Times.Never);
    }

    [Fact]
    public async Task UpdateStudentStatusConfig_EmptyConfig_HandlesSuccessfully()
    {
        // Arrange
        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Id = 1,
            Type = "StudentStatusConfig",
            Value = new Dictionary<int, List<int>>(),
            IsActive = true
        };

        // Empty list of status IDs because Value is empty
        _mockStatusRepo.Setup(repo => repo.ReferenceStudentStatuses(It.Is<List<int>>(ids => ids.Count == 0)))
            .ReturnsAsync(0);

        _mockConfigRepo.Setup(repo => repo.UpdateConfig(config))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateStudentStatusConfig(config);

        // Assert
        Assert.Equal(1, result);
        _mockStatusRepo.Verify(repo => repo.ReferenceStudentStatuses(It.Is<List<int>>(ids => ids.Count == 0)), Times.Once);
        _mockConfigRepo.Verify(repo => repo.UpdateConfig(config), Times.Once);
    }
    #endregion

    #region GetNextStatuses Tests
    [Fact]
    public async Task GetNextStatuses_ConfigIsInactive_ReturnsAllStatuses()
    {
        // Arrange
        var allStatuses = new List<StudentStatus>
        {
            new StudentStatus { Id = 1, Name = "Pending" },
            new StudentStatus { Id = 2, Name = "Active" },
            new StudentStatus { Id = 3, Name = "Completed" }
        };

        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Value = new Dictionary<int, List<int>> { { 1, new List<int> { 2 } } },
            IsActive = false
        };

        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync(config);
        _mockStatusRepo.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(allStatuses);

        // Act
        var result = await _service.GetNextStatuses(1);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, s => s.Id == 1);
        Assert.Contains(result, s => s.Id == 2);
        Assert.Contains(result, s => s.Id == 3);
    }

    [Fact]
    public async Task GetNextStatuses_ConfigIsNull_ReturnsOnlyCurrentStatus()
    {
        // Arrange
        var allStatuses = new List<StudentStatus>
        {
            new StudentStatus { Id = 1, Name = "Pending" },
            new StudentStatus { Id = 2, Name = "Active" },
            new StudentStatus { Id = 3, Name = "Completed" }
        };

        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync((Configuration<Dictionary<int, List<int>>>?)null);
        _mockStatusRepo.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(allStatuses);

        // Act
        var result = await _service.GetNextStatuses(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetNextStatuses_ConfigIsEmptyAndActive_ReturnsOnlyCurrentStatus()
    {
        // Arrange
        var allStatuses = new List<StudentStatus>
        {
            new StudentStatus { Id = 1, Name = "Pending" },
            new StudentStatus { Id = 2, Name = "Active" },
            new StudentStatus { Id = 3, Name = "Completed" }
        };

        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Value = new Dictionary<int, List<int>>(),
            IsActive = true
        };

        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync(config);
        _mockStatusRepo.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(allStatuses);

        // Act
        var result = await _service.GetNextStatuses(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetNextStatuses_StatusIdNotInConfig_ReturnsOnlyCurrentStatus()
    {
        // Arrange
        var allStatuses = new List<StudentStatus>
        {
            new StudentStatus { Id = 1, Name = "Pending" },
            new StudentStatus { Id = 2, Name = "Active" },
            new StudentStatus { Id = 3, Name = "Completed" }
        };

        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Value = new Dictionary<int, List<int>> { { 2, new List<int> { 3 } } },
            IsActive = true
        };

        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync(config);
        _mockStatusRepo.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(allStatuses);

        // Act
        var result = await _service.GetNextStatuses(1); // Status ID 1 not in config

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetNextStatuses_StatusIdInConfig_ReturnsCurrentAndNextStatuses()
    {
        // Arrange
        var allStatuses = new List<StudentStatus>
        {
            new StudentStatus { Id = 1, Name = "Pending" },
            new StudentStatus { Id = 2, Name = "Active" },
            new StudentStatus { Id = 3, Name = "Completed" },
            new StudentStatus { Id = 4, Name = "Cancelled" }
        };

        var config = new Configuration<Dictionary<int, List<int>>>
        {
            Value = new Dictionary<int, List<int>> { { 1, new List<int> { 2, 3 } } },
            IsActive = true
        };

        _mockConfigRepo.Setup(repo => repo.GetStudentStatusConfig())
            .ReturnsAsync(config);
        _mockStatusRepo.Setup(repo => repo.GetAllStudentStatuses())
            .ReturnsAsync(allStatuses);

        // Act
        var result = await _service.GetNextStatuses(1);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, s => s.Id == 1); // Current status always included
        Assert.Contains(result, s => s.Id == 2); // Next status from config
        Assert.Contains(result, s => s.Id == 3); // Next status from config
        Assert.DoesNotContain(result, s => s.Id == 4); // Not a next status in config
    }
    #endregion
}
