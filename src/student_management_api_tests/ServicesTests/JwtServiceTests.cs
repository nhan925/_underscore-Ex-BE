using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Moq;
using student_management_api.Contracts.IRepositories;
using student_management_api.Localization;
using student_management_api.Models.Authentication;
using student_management_api.Models.DTO;
using student_management_api.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace student_management_api_tests.ServicesTests;

public class JwtServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly JwtService _jwtService;
    private readonly Mock<IStringLocalizer<Messages>> _mockLocalizer;

    // Testing values
    private const string TestSecret = "testsecretkeywithenoughlengthforhmacsha256signature";
    private const int TestExpirationHours = 24;

    public JwtServiceTests()
    {
        // Set environment variables for testing
        Environment.SetEnvironmentVariable("JWT_SECRET", TestSecret);
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_HOURS", TestExpirationHours.ToString());

        _mockUserRepository = new Mock<IUserRepository>();
        _mockLocalizer = new Mock<IStringLocalizer<Messages>>();
        _jwtService = new JwtService(_mockUserRepository.Object, _mockLocalizer.Object);
    }

    #region AuthenticateUser Tests
    [Fact]
    public async Task AuthenticateUser_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var authRequest = new AuthRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("password123") // Hashed password
        };

        _mockUserRepository
            .Setup(repo => repo.GetUser(authRequest.Username))
            .ReturnsAsync(user);

        // Act
        var token = await _jwtService.AuthenticateUser(authRequest);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Verify token can be decoded
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify claims
        Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Username, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
    }

    [Fact]
    public async Task AuthenticateUser_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var authRequest = new AuthRequest
        {
            Username = "nonexistent",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(repo => repo.GetUser(authRequest.Username))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _jwtService.AuthenticateUser(authRequest));
    }

    [Fact]
    public async Task AuthenticateUser_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var authRequest = new AuthRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("password123") // Correct hashed password
        };

        _mockUserRepository
            .Setup(repo => repo.GetUser(authRequest.Username))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _jwtService.AuthenticateUser(authRequest));
    }
    #endregion

    #region GenerateJwtToken Tests
    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser"
        };

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validate token format
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(token));

        // Decode and verify claims
        var jwtToken = tokenHandler.ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Username, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);

        // Verify expiration
        var expectedExpiryTime = DateTime.UtcNow.AddHours(TestExpirationHours);
        // Allow a small time difference due to execution time
        Assert.True(Math.Abs((expectedExpiryTime - jwtToken.ValidTo).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateJwtToken_ValidateTokenSignature()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser"
        };

        // Act
        var token = _jwtService.GenerateJwtToken(user);

        // Assert - Try to validate the token with our testing key
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // This will throw if signature validation fails
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.NotNull(principal);
        Assert.IsType<JwtSecurityToken>(validatedToken);
    }
    #endregion

    // Clean up environment variables after tests
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_HOURS", null);
    }
}