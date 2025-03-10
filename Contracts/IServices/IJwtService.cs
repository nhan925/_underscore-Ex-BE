using student_management_api.Models.Authentication;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface IJwtService
{
    Task<string?> AuthenticateUser(AuthRequest request);

    string GenerateJwtToken(User user);
}
