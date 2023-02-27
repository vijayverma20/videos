using Mavericks.Catalogue.Client.Models;

namespace Mavericks.Catalogue.Client.Services
{
    public interface ILocalStorageService
    {
        string? Get(LocalStorageKey key);
        T? Get<T>(LocalStorageKey key);
        void Set(LocalStorageKey key, object? value);
    }
}