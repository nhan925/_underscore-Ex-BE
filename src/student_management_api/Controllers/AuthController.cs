using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.Authentication;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        using (_logger.BeginScope("Login attempt for {Username}", request.Username))
        {
            _logger.LogInformation("Received login request");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login request data for {Username}", request.Username);
                    return BadRequest(ModelState);
                }

                var token = await _jwtService.AuthenticateUser(request);

                if (token == null)
                {
                    _logger.LogWarning("Login failed for {Username}: Invalid credentials", request.Username);
                    return Unauthorized(new { message = "Invalid request" });
                }

                _logger.LogInformation("Login successful for {Username}", request.Username);
                return Ok(new { access_token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for {Username}: {Message}", request.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
