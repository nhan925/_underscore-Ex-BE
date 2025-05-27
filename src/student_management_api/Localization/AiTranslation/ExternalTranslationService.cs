using student_management_api.Localization.AiTranslation;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ExternalTranslationService : IExternalTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public ExternalTranslationService(string baseUrl, string model)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _model = model;
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        if (sourceLanguage == targetLanguage)
        {
            return text; // No translation needed if both languages are the same
        }

        var languageMap = new Dictionary<string, string>
        {
            { "en", "English" },
            { "vi", "Vietnamese" }
        };

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional translator. Translate clearly and accurately between Vietnamese and English. Always provide ONLY the translated text without quotation marks, periods, or any other punctuation unless it's part of the original text." },
                new { role = "user", content = $"Translate the following {languageMap[sourceLanguage]} text to {languageMap[targetLanguage]}:\r\n\r\n{languageMap[sourceLanguage]}: \"{text}\"\r\n\r\n{languageMap[targetLanguage]}:" }
            },
            temperature = 0.0,
            top_p = 0.9,
            top_k = 40,
            frequency_penalty = 1.1,
            presence_penalty = 1.1,
            max_tokens = -1,
            stream = false
        };

        string json = JsonSerializer.Serialize(requestBody);

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        // Deserialize the response to extract the translated text
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        // The translated text is usually at choices[0].message.content
        var translatedText = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        return translatedText.Trim('"').TrimEnd('.').Trim('”').Trim(' ');
    }
}
