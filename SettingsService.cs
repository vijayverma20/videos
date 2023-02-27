using RSP.Dashboard.Services.Interfaces.Shared;
using RSP.Dashboard.Services.Models;

namespace RSP.Dashboard.Services.Services.Shared
{
    public class SettingsService : ISettingsService
    {
        private readonly ILocalStorageService localStorageService;
        private readonly IDictionary<string, LocalStorageKey> localStorageKeys;
        private readonly IDictionary<string, object> settings;

        public SettingsService(ILocalStorageService localStorageService, AppSettings appSettings)
        {
            this.localStorageService = localStorageService;
            localStorageKeys = new Dictionary<string, LocalStorageKey>()
            {
                { nameof(CollapseNavBar), LocalStorageKey.CollapseNavBar },
                { nameof(PageSize), LocalStorageKey.PageSize },
                { nameof(Language), LocalStorageKey.PreferredLanguage }
            };
            settings = new Dictionary<string, object>()
            {
                { nameof(CollapseNavBar), localStorageService.Get<bool?>(LocalStorageKey.CollapseNavBar) ?? false },
                { nameof(PageSize), localStorageService.Get<int?>(LocalStorageKey.PageSize) ?? appSettings.DefaultPageSize },
                { nameof(Language), localStorageService.Get(LocalStorageKey.PreferredLanguage) ?? appSettings.DefaultLanguage }
            };
        }

        public event Action<string> OnChange;
        public bool CollapseNavBar
        {
            get { return GetValue<bool>(nameof(CollapseNavBar)); }
            set { SetValue(nameof(CollapseNavBar), value); }
        }

        public int PageSize
        {
            get { return GetValue<int>(nameof(PageSize)); }
            set { SetValue(nameof(PageSize), value); }
        }

        public string Language
        {
            get { return GetValue<string>(nameof(Language)); }
            set { SetValue(nameof(Language), value); }
        }

        private T GetValue<T>(string propertyName)
        {
            return (T)settings[propertyName];
        }

        private void SetValue(string propertyName, object value)
        {
            if (settings[propertyName] != value)
            {
                settings[propertyName] = value;
                localStorageService.Set(localStorageKeys[propertyName], value);
                OnChange?.Invoke(propertyName);
            }
        }
    }
}
