using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Models.Authentication;
using student_management_api.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger, IStringLocalizer<Messages> localizer)
    {
        _jwtService = jwtService;
        _logger = logger;
        _localizer = localizer; 
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login for user",
        Description = "Login endpoint for users to authenticate and receive a JWT token."
    )]
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
                    return BadRequest(new ErrorResponse<ModelStateDictionary>(status: 400, message: _localizer["invalid_input"], details: ModelState));
                }

                var token = await _jwtService.AuthenticateUser(request);

                if (token == null)
                {
                    _logger.LogWarning("Login failed for {Username}: Invalid credentials", request.Username);
                    return Unauthorized(new ErrorResponse<string>(status: 401, message: _localizer["invalid_credentials"]));
                }

                _logger.LogInformation("Login successful for {Username}", request.Username);
                return Ok(new { access_token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for {Username}: {Message}", request.Username, ex.Message);
                return Unauthorized(new ErrorResponse<string>(status: 401, message: _localizer["login_failed"], details: ex.Message));
            }
        }
    }
}
