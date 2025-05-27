namespace student_management_api.Localization.AiTranslation;

public interface IExternalTranslationService
{
    Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);
}
