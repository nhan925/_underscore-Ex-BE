using student_management_api.Models.Configuration;
using student_management_api.Models.DTO;

namespace student_management_api.Contracts.IServices;

public interface IConfigurationService
{
    Task<Configuration<List<string>>> GetEmailDomainsConfig();
    Task<bool> CheckEmailDomain(string email);
    Task<int> UpdateEmailDomainsConfig(Configuration<List<string>> config);

    Task<Configuration<List<string>>> GetPhoneNumberCountryConfig();
    Task<bool> CheckPhoneNumber(string phoneNumber);
    Task<int> UpdatePhoneNumberCountryConfig(Configuration<List<string>> config);

    Task<Configuration<Dictionary<int, List<int>>>> GetStudentStatusConfig();
    Task<List<StudentStatus>> GetNextStatuses(int statusId);
    Task<int> UpdateStudentStatusConfig(Configuration<Dictionary<int, List<int>>> config);

    Task<int> TurnAllRulesOnOrOff(bool isOn);
}
