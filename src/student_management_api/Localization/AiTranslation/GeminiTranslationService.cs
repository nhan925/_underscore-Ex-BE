using System.Text;
using System.Text.Json;

namespace student_management_api.Localization.AiTranslation;

public class GeminiTranslationService : IExternalTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _apiKey;

    public GeminiTranslationService(string apiKey, string model = "gemma-3-27b-it")
    {
        _httpClient = new HttpClient();
        _model = model;
        _apiKey = apiKey;
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

        response.EnsureSuccessStatusCode();
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

