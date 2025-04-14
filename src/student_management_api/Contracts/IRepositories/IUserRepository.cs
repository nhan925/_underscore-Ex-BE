using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IUserRepository
{
    Task<User?> GetUser(string username);
}
