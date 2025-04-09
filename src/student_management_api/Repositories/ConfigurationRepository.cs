using student_management_api.Contracts.IRepositories;
using student_management_api.Models.Configuration;
using System.Data;
using Dapper;

namespace student_management_api.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly IDbConnection _db;

    public ConfigurationRepository(IDbConnection db)
    {
        _db = db;
    }

    private async Task<Configuration<T>> GetConfig<T>(string type)
    {
        string query = "SELECT * FROM configurations WHERE type = @Type";
        var result = await _db.QueryFirstOrDefaultAsync<Configuration<T>>(query, new { Type = type });

        return result;
    }

    public async Task<Configuration<List<string>>> GetEmailDomainsConfig()
    {
        return await GetConfig<List<string>>("email_domains");
    }

    public async Task<Configuration<List<string>>> GetPhoneNumberCountryConfig()
    {
        return await GetConfig<List<string>>("phone_countries");
    }

    public async Task<Configuration<Dictionary<int, List<int>>>> GetStudentStatusConfig()
    {
        return await GetConfig<Dictionary<int, List<int>>>("student_status_rules");
    }

    public async Task<int> TurnAllRulesOnOrOff(bool isOn)
    {
        string query = "UPDATE configurations SET is_active = @IsOn";
        return await _db.ExecuteAsync(query, new { IsOn = isOn });
    }

    public async Task<int> UpdateConfig<T>(Configuration<T> config)
    {
        string query = "UPDATE configurations SET value = @Value, is_active = @IsActive WHERE type = @Type";
        return await _db.ExecuteAsync(query, config);
    }
}
