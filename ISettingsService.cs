using System;

namespace Mavericks.Catalogue.Client.Services
{
    public interface ISettingsService
    {
        event Action<string>? OnChange;
        bool CollapseNavBar { get; set; }
        int PageSize { get; set; }
        string Language { get; set; }
    }
}