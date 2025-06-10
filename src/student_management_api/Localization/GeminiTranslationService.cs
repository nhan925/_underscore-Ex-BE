using student_management_api.Controllers;
using System.Text;
using System.Text.Json;

namespace student_management_api.Localization;

public class GeminiTranslationService : IExternalTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _apiKey;
    private readonly ILogger<GeminiTranslationService> _logger;

    public GeminiTranslationService(ILogger<GeminiTranslationService> logger, string apiKey, string model = "gemma-3-27b-it")
    {
        _httpClient = new HttpClient();
        _model = model;
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        if (sourceLanguage == targetLanguage)
            return text;

        var languageMap = new Dictionary<string, string>
        {
            { "en", "English" },
            { "vi", "Vietnamese" }
        };

        var prompt = $"You are a professional translator. Translate clearly and accurately from {languageMap[sourceLanguage]} to {languageMap[targetLanguage]}. Only return the translated text, no explanation.\n\n{languageMap[sourceLanguage]}: {text}\n\n{languageMap[targetLanguage]}:";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}",
            content);

        // Fallback to default language if the API key is not set or the model is not available
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation request failed for text: {Text}", text);
            return text; // Return original text if request fails
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        var translatedText = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return translatedText?.Trim('"').TrimEnd('.').Trim('”').Trim() ?? "";
    }
}

