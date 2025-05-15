using Microsoft.IdentityModel.Tokens;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Models.Authentication;
using student_management_api.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace student_management_api.Services;

public class JwtService: IJwtService
{
    private readonly IUserRepository _userRepository;
    private readonly string _secret;
    private readonly int _expirationHours;

    public JwtService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new Exception("JWT_SECRET is missing");
        _expirationHours = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS") ?? "24");
    }

    public async Task<string?> AuthenticateUser(AuthRequest request)
    {
        var user = await _userRepository.GetUser(request.Username!);
        
        if (user == null)
        {
            throw new UnauthorizedAccessException("user not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            throw new UnauthorizedAccessException("invalid password");
        }

        return GenerateJwtToken(user);
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
