using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using student_management_api.Contracts.IServices;
using student_management_api.Models.Configuration;
using student_management_api.Models.DTO;

namespace student_management_api.Controllers;

[ApiController]
[Route("api/config")]
[Authorize]
public class ConfigurationController : Controller
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    [HttpGet("{type}")]
    public async Task<IActionResult> GetConfig(string type)
    {
        using (_logger.BeginScope("GetConfig request"))
        {
            _logger.LogInformation($"Fetching config of type {type}");

            if (type == "email")
            {
                var config = await _configurationService.GetEmailDomainsConfig();
                _logger.LogInformation("Successfully retrieved email domains config");
                return Ok(config);
            }
            else if (type == "phone-number")
            {
                var config = await _configurationService.GetPhoneNumberCountryConfig();
                _logger.LogInformation("Successfully retrieved phone number countries config");
                return Ok(config);
            }
            else if (type == "student-status")
            {
                var config = await _configurationService.GetStudentStatusConfig();
                _logger.LogInformation("Successfully retrieved student status rules config");
                return Ok(config);
            }
            else
            {
                _logger.LogWarning("Invalid config type");
                return BadRequest(new { message = "Invalid config type" });
            }
        }
    }

    [HttpGet("check/{type}/{value}")]
    public async Task<IActionResult> CheckConfig(string type, string value) 
    {
        using (_logger.BeginScope("CheckConfig request"))
        {
            _logger.LogInformation($"Checking config of type {type} with value {value}");

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Value is empty");
                return BadRequest(new { message = "Value is empty" });
            }

            if (type == "email")
            {
                var result = await _configurationService.CheckEmailDomain(value);
                _logger.LogInformation($"Email domain check result: {result}");
                return Ok(new { result = result });
            }
            else if (type == "phone-number")
            {
                var result = await _configurationService.CheckPhoneNumber(value);
                _logger.LogInformation($"Phone number check result: {result}");
                return Ok(new { result = result });
            }
            else
            {
                _logger.LogWarning("Invalid config type");
                return BadRequest(new { message = "Invalid config type" });
            }
        }
    }

    [HttpGet("next-statuses/{statusId}")]
    public async Task<IActionResult> GetNextStatuses(int statusId)
    {
        using (_logger.BeginScope("GetNextStatuses request"))
        {
            _logger.LogInformation($"Fetching next statuses for status {statusId}");
            var nextStatuses = await _configurationService.GetNextStatuses(statusId);

            _logger.LogInformation("Successfully retrieved next statuses");
            return Ok(nextStatuses);
        }
    }

    [HttpPost("update/email")]
    public async Task<IActionResult> UpdateEmailDomainsConfig([FromBody] Configuration<List<string>> config)
    {
        using (_logger.BeginScope("UpdateEmailDomainsConfig request"))
        {
            _logger.LogInformation("Updating email domains config");
            var updatedCount = await _configurationService.UpdateEmailDomainsConfig(config);

            if (updatedCount > 0)
            {
                _logger.LogInformation("Email domains config updated successfully");
                return Ok(new { message = "Email domains config updated successfully" });
            }
            else
            {
                _logger.LogWarning("No changes applied");
                return NotFound(new { message = "No changes applied" });
            }
        }
    }

    [HttpPost("update/phone-number")]
    public async Task<IActionResult> UpdatePhoneNumberCountryConfig([FromBody] Configuration<List<string>> config)
    {
        using (_logger.BeginScope("UpdatePhoneNumberCountryConfig request"))
        {
            _logger.LogInformation("Updating phone number country config");
            var updatedCount = await _configurationService.UpdatePhoneNumberCountryConfig(config);

            if (updatedCount > 0)
            {
                _logger.LogInformation("Phone number country config updated successfully");
                return Ok(new { message = "Phone number country config updated successfully" });
            }
            else
            {
                _logger.LogWarning("No changes applied");
                return NotFound(new { message = "No changes applied" });
            }
        }
    }

    [HttpPost("update/student-status")]
    public async Task<IActionResult> UpdateStudentStatusConfig([FromBody] Configuration<Dictionary<int, List<int>>> config)
    {
        using (_logger.BeginScope("UpdateStudentStatusConfig request"))
        {
            _logger.LogInformation("Updating student status rules config");
            var updatedCount = await _configurationService.UpdateStudentStatusConfig(config);

            if (updatedCount > 0)
            {
                _logger.LogInformation("Student status rules config updated successfully");
                return Ok(new { message = "Student status rules config updated successfully" });
            }
            else
            {
                _logger.LogWarning("No changes applied");
                return NotFound(new { message = "No changes applied" });
            }
        }
    }

    [HttpPost("is-active/{isActive}")]
    public async Task<IActionResult> TurnAllRulesOnOrOff(bool isActive)
    {
        using (_logger.BeginScope("TurnAllRulesOnOrOff request"))
        {
            _logger.LogInformation($"Turning all rules {(isActive ? "on" : "off")}");
            var updatedCount = await _configurationService.TurnAllRulesOnOrOff(isActive);

            if (updatedCount > 0)
            {
                _logger.LogInformation("All rules turned {Status}", isActive ? "on" : "off");
                return Ok(new { Message = $"All rules turned {(isActive ? "on" : "off")}." });
            }
            else
            {
                _logger.LogWarning("No rules found or no changes applied");
                return NotFound(new { Message = "No rules found or no changes applied." });
            }
        }
    }
}
