using Dapper;
using student_management_api.Contracts;
using student_management_api.Models.DTO;
using System.Data;

namespace student_management_api.Repositories;

public class UserRepository: IUserRepository
{
    private readonly IDbConnection _db;
    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<User?> GetUser(string username)
    {
        string sql = "SELECT * FROM users WHERE username=@Username";
        return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }
}
