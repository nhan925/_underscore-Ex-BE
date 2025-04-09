using student_management_api.Models.Configuration;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IRepositories;

public interface IConfigurationRepository
{
    Task<Configuration<List<string>>> GetEmailDomainsConfig();
    Task<Configuration<List<string>>> GetPhoneNumberCountryConfig();
    Task<Configuration<Dictionary<int, List<int>>>> GetStudentStatusConfig();

    Task<int> UpdateConfig<T>(Configuration<T> config);

    Task<int> TurnAllRulesOnOrOff(bool isOn);
}
