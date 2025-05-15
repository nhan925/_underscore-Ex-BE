using Xunit;
using student_management_api.Services;
using student_management_api.Models.Configuration;
using System.Collections.Generic;

namespace student_management_api_tests.ServicesTests;

public class CountryPhoneCodeServiceTests
{
    private readonly CountryPhoneCodeService _service;

    public CountryPhoneCodeServiceTests()
    {
        _service = new CountryPhoneCodeService();
    }

    #region GetAllCountriesPhoneCode Tests
    [Fact]
    public void GetAllCountriesPhoneCode_ValidCall_ReturnsNonEmptyList()
    {
        var result = _service.GetAllCountriesPhoneCode();

        Assert.NotNull(result);
        Assert.IsType<List<CountryPhoneCode>>(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAllCountriesPhoneCode_ValidCall_ContainsMajorCountries()
    {
        var result = _service.GetAllCountriesPhoneCode();

        // Check if common countries are present
        Assert.Contains(result, c => c.Code == "US");
        Assert.Contains(result, c => c.Code == "GB");
        Assert.Contains(result, c => c.Code == "CA");
    }

    [Fact]
    public void GetAllCountriesPhoneCode_ValidCall_HasCorrectFormat()
    {
        var result = _service.GetAllCountriesPhoneCode();

        // Check if all entries have the required format
        foreach (var country in result)
        {
            Assert.NotNull(country.Name);
            Assert.NotEmpty(country.Name);
            Assert.NotNull(country.Code);
            Assert.NotEmpty(country.Code);
            Assert.NotNull(country.CallingCode);
            Assert.NotEmpty(country.CallingCode);
            Assert.StartsWith("+", country.CallingCode);
        }
    }
    #endregion
}
