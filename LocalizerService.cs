using Microsoft.Extensions.Localization;
using RSP.Dashboard.Services.Interfaces.Shared;
using RSP.Dashboard.Services.Models;
using System.Globalization;
using System.Text;

namespace RSP.Dashboard.Services.Services.Shared
{
    public class LocalizerService : ILocalizerService, IAppInitializer
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISettingsService settingsService;
        private readonly IDictionary<string, string> displayTexts = new Dictionary<string, string>();

        public LocalizerService(IStringLocalizer<Resource> stringLocalizer, ISettingsService settingsService)
        {
            this.stringLocalizer = stringLocalizer;
            this.settingsService = settingsService;
        }

        public event Action OnLanguageChange;

        public string this[string key] => GetValue(key);

        public string GetLanguage()
        {
            return settingsService.Language;
        }

        public void SetLanguage(string languageCode)
        {
            SetCulture(languageCode);
            settingsService.Language = languageCode;
        }

        public async Task Initialize()
        {
            SetCulture(settingsService.Language);
            await Task.CompletedTask;
        }

        private void SetCulture(string languageCode)
        {
            if (CultureInfo.DefaultThreadCurrentCulture?.Name != languageCode)
            {
                var culture = new CultureInfo(languageCode);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                OnLanguageChange?.Invoke();
            }
        }

        private string GetValue(string key)
        {
            var localizedString = stringLocalizer[key];
            if (!localizedString.ResourceNotFound)
            {
                return localizedString.Value;
            }
            return GetDisplayText(key);
        }

        private string GetDisplayText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (!displayTexts.ContainsKey(text))
            {
                displayTexts[text] = InsertSpaces(text);
            }

            return displayTexts[text];
        }

        private static string InsertSpaces(string text)
        {
            if (text.Contains('.'))
            {
                var textParts = text.Split('.');
                text = textParts[^1];
            }
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                stringBuilder.Append(text[i]);

                int nextChar = i + 1;
                if (nextChar < text.Length && char.IsUpper(text[nextChar]) && !char.IsUpper(text[i]))
                {
                    stringBuilder.Append(' ');
                }
            }

            return stringBuilder.ToString();
        }
    }
}
