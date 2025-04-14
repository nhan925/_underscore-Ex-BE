using student_management_api.Models.Configuration;

namespace student_management_api.Contracts.IServices;

public interface ICountryPhoneCodeService
{
    List<CountryPhoneCode> GetAllCountriesPhoneCode();
}
