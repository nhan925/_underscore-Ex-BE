using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.DTO;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/phone-code")]
[Authorize]
public class CountryPhoneCodeController : ControllerBase
{
    private readonly ICountryPhoneCodeService _countryPhoneCodeService;
    private readonly ILogger<CountryPhoneCodeController> _logger;

    public CountryPhoneCodeController(ICountryPhoneCodeService countryPhoneCodeService, ILogger<CountryPhoneCodeController> logger)
    {
        _countryPhoneCodeService = countryPhoneCodeService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all country phone codes",
        Description = "Fetches a list of all country phone codes."
    )]
    public IActionResult GetCountryPhoneCode()
    {
        using (_logger.BeginScope("GetCountryPhoneCode request"))
        {
            _logger.LogInformation("Fetching all country phone codes");
            var countryPhoneCodes = _countryPhoneCodeService.GetAllCountriesPhoneCode();

            _logger.LogInformation("Successfully retrieved {Count} country phone codes", countryPhoneCodes.Count);
            return Ok(countryPhoneCodes);
        }
    }

}
