using Dapper;
using student_management_api.Contracts.IRepositories;
using student_management_api.Models.DTO;
using System.Data;
using System.Globalization;

namespace student_management_api.Repositories;

public class LecturersRepository : ILecturersRepository
{
    private readonly IDbConnection _db;
    private readonly string _cultureSuffix;

    public LecturersRepository(IDbConnection db)
    {
        _db = db;
        _cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "" : $"_{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}";
    }

    public async Task<List<Lecturer>> GetAllLecturers()
    {
        string sql = $"SELECT id, full_name, date_of_birth, gender{_cultureSuffix} AS gender, email, phone_number FROM lecturers";
        var lecturers = await _db.QueryAsync<Lecturer>(sql);
        
        return lecturers.ToList();
    }
}
