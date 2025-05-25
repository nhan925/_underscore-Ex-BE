using PhoneNumbers;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Exceptions;
using student_management_api.Models.Configuration;
using student_management_api.Models.DTO;

namespace student_management_api.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IStudentStatusRepository _studentStatusRepository;

    public ConfigurationService(IConfigurationRepository configurationRepository, IStudentStatusRepository studentStatusRepository)
    {
        _configurationRepository = configurationRepository;
        _studentStatusRepository = studentStatusRepository;
    }

    public async Task<bool> CheckEmailDomain(string email)
    {
        var domainsConfig = await GetEmailDomainsConfig();

        if (domainsConfig == null || (domainsConfig.Value!.Count == 0 && domainsConfig.IsActive))
        {
            return false; // No valid domains to check
        }

        if (!domainsConfig.IsActive)
        {
            return true; 
        }

        return domainsConfig.Value.Contains(email.Split('@')[1]);
    }

    public async Task<bool> CheckPhoneNumber(string phoneNumber)
    {
        if (phoneNumber[0] != '+' && phoneNumber[0] != '0')
        {
            return false; // Invalid number format
        }

        var countriesConfig = await GetPhoneNumberCountryConfig(); // Get allowed country codes

        if (countriesConfig == null || (countriesConfig.Value!.Count == 0 && countriesConfig.IsActive))
        {
            return false;
        }

        if (!countriesConfig.IsActive)
        {
            return true;
        }

        var phoneUtil = PhoneNumberUtil.GetInstance();

        foreach (var countryCode in countriesConfig.Value)
        {
            try
            {
                string normalizedNumber = NormalizePhoneNumber(phoneNumber, countryCode);
                var parsedNumber = phoneUtil.Parse(normalizedNumber, countryCode);

                // Ensure the parsed number belongs to the expected country
                string parsedRegion = phoneUtil.GetRegionCodeForNumber(parsedNumber);
                if (parsedRegion == countryCode && phoneUtil.IsValidNumber(parsedNumber))
                {
                    return true; // Valid number in allowed country
                }
            }
            catch (NumberParseException)
            {
                continue; // Ignore parsing errors and check next country
            }
        }

        return false; // No valid match
    }

    // Normalize phone number to E.164 format
    private string NormalizePhoneNumber(string phoneNumber, string countryCode)
    {
        phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

        var phoneUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var parsedNumber = phoneUtil.Parse(phoneNumber, countryCode);
            return phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164); // Standardize format
        }
        catch (NumberParseException)
        {
            return phoneNumber; // Return as-is if parsing fails
        }
    }

    public async Task<Configuration<List<string>>> GetEmailDomainsConfig()
    {
        return await _configurationRepository.GetEmailDomainsConfig();
    }

    public async Task<List<StudentStatus>> GetNextStatuses(int statusId)
    {
        var studentStatusesConfig = await GetStudentStatusConfig();
        var allStatuses = await _studentStatusRepository.GetAllStudentStatuses();

        if (studentStatusesConfig == null || (studentStatusesConfig.Value!.Count == 0 && studentStatusesConfig.IsActive))
        {
            return allStatuses.Where(stt => stt.Id == statusId).ToList(); // No valid statuses to check
        }

        if (!studentStatusesConfig.IsActive)
        {
            return allStatuses;
        }

        if (!studentStatusesConfig.Value.ContainsKey(statusId))
        {
            return allStatuses.Where(stt => stt.Id == statusId).ToList(); // No valid statuses to check
        }

        var nextStatuses = allStatuses.Where(s => studentStatusesConfig.Value[statusId].Contains(s.Id) || s.Id == statusId).ToList();

        return nextStatuses;
    }

    public async Task<Configuration<List<string>>> GetPhoneNumberCountryConfig()
    {
        return await _configurationRepository.GetPhoneNumberCountryConfig();
    }

    public async Task<Configuration<Dictionary<int, List<int>>>> GetStudentStatusConfig()
    {
        return await _configurationRepository.GetStudentStatusConfig();
    }

    public async Task<int> TurnAllRulesOnOrOff(bool isOn)
    {
        return await _configurationRepository.TurnAllRulesOnOrOff(isOn);
    }

    public async Task<int> UpdateEmailDomainsConfig(Configuration<List<string>> config)
    {
        return await _configurationRepository.UpdateConfig(config);
    }

    public async Task<int> UpdatePhoneNumberCountryConfig(Configuration<List<string>> config)
    {
        return await _configurationRepository.UpdateConfig(config);
    }

    public async Task<int> UpdateStudentStatusConfig(Configuration<Dictionary<int, List<int>>> config)
    {
        var studentStatusesIds = config.Value!
            .SelectMany(kvp => kvp.Value)
            .Distinct()
            .ToList();
        var referenceStatusesCount = await _studentStatusRepository.ReferenceStudentStatuses(studentStatusesIds);

        if (referenceStatusesCount != studentStatusesIds.Count)
        {
            throw new NotFoundException("One or more statuses are not found");
        }

        var result = await _configurationRepository.UpdateConfig(config);
        return result;
    }
}
