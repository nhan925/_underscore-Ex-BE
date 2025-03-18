using Microsoft.AspNetCore.Mvc;
using Serilog;
using student_management_api.Contracts.IServices;
using student_management_api.Models.Authentication;
using student_management_api.Services;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/auth")]

public class AuthController : Controller
{
    private readonly IJwtService _jwtService;

    public AuthController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var token = await _jwtService.AuthenticateUser(request);
            if (token == null)
            {
                return Unauthorized(new { message = "invalid request" });
            }

            return Ok(new { access_token = token });
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error($"Action: Login, Message: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
    }
}
