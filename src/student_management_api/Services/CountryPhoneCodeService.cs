using PhoneNumbers;
using student_management_api.Contracts.IServices;
using student_management_api.Models.Configuration;
using System.Globalization;

namespace student_management_api.Services;

public class CountryPhoneCodeService : ICountryPhoneCodeService
{
    public List<CountryPhoneCode> GetAllCountriesPhoneCode()
    {
        var phoneUtil = PhoneNumberUtil.GetInstance();
        var regionCodes = phoneUtil.GetSupportedRegions();
        var result = new List<CountryPhoneCode>();

        foreach (var region in regionCodes)
        {
            try
            {
                var regionInfo = new RegionInfo(region); // Might throw exception for invalid regions

                var countryPhoneCode = new CountryPhoneCode
                {
                    Name = regionInfo.EnglishName, // Get country name
                    Code = region,
                    CallingCode = $"+{phoneUtil.GetCountryCodeForRegion(region)}"
                };

                result.Add(countryPhoneCode);
            }
            catch (ArgumentException)
            {
                // Skip invalid region codes that .NET does not support
                continue;
            }
        }

        return result;
    }

}
