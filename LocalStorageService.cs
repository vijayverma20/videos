using Microsoft.JSInterop;
using RSP.Dashboard.Services.Interfaces.Shared;
using RSP.Dashboard.Services.Models;
using System.Text.Json;

namespace RSP.Dashboard.Services.Services.Shared
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSInProcessRuntime jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = (IJSInProcessRuntime)jsRuntime;
        }

        public string Get(LocalStorageKey key)
        {
            return Get<string>(key);
        }

        public T Get<T>(LocalStorageKey key)
        {
            var value = jsRuntime.Invoke<string>("globalThis.localStorage.getItem", key.ToString());
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value);
        }

        public void Set(LocalStorageKey key, object value)
        {
            if (value is null)
            {
                jsRuntime.InvokeVoid("globalThis.localStorage.removeItem", key.ToString());
            }
            else
            {
                jsRuntime.InvokeVoid("globalThis.localStorage.setItem", key.ToString(), JsonSerializer.Serialize(value));
            }
        }
    }
}
