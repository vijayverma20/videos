using System;

namespace Mavericks.Catalogue.Client.Services
{
    public interface ILocalizerService
    {
        event Action? OnLanguageChange;
        string this[string key] { get; }
        string GetLanguage();
        void SetLanguage(string languageCode);
    }
}
