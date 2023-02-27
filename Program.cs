using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Mavericks.Catalogue.Client.Providers.Links;
using Mavericks.Catalogue.Client.Providers.ProductCatalogue.PremiumCalculation;
using Mavericks.Catalogue.Client.Services;
using Mavericks.ProductCatalogue.Extensions.PremiumCalculation.Providers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mavericks.Catalogue.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var appSettings = builder.Configuration.Get<AppSettings>();
            builder.Services.AddSingleton(appSettings);

            // Register components
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Register services
            builder.Services
                .AddBlazorise(options =>
                {
                    options.Immediate = true;
                    options.Debounce = true;
                    options.DebounceInterval = 300;
                })
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            builder.Services
                .AddLogging()
                .AddTransient<ILogger>(sp => sp.GetRequiredService<ILogger<App>>())
                .AddLocalization()
                .AddTransient<IAppNotificationService, AppNotificationService>()
                .AddTransient<IClipboardService, ClipboardService>()
                .AddSingleton<ILoaderService, LoaderService>()
                .AddSingleton<ILocalizerService, LocalizerService>()
                .AddSingleton<ILocalStorageService, LocalStorageService>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IRulemanLinkProvider, RulemanLinkProvider>()
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(appSettings.ApiUrl) });

            // Authentication services
            builder.Services.AddGatekeeperTokenProvider(
                options => options.UseUrl(appSettings.GatekeeperUrl),
                tokenProviderLifetime: ServiceLifetime.Singleton);

            // Catalogue services
            builder.Services
                .AddCatalogueConditionEngine();

            // Product catalogue services
            builder.Services
                .AddProductCatalogueCore(cacheStoreLifetime: ServiceLifetime.Singleton)
                .AddProductCatalogueWebClient(options => options.UseUrl(appSettings.ApiUrl).UseTokenInterceptor())
                .AddProductCatalogueConditionEngine()
                .AddProductCataloguePremiumCalculator(options => options.UseUrl(appSettings.PremiumCalculationApiUrl));
            builder.Services
                .AddTransient<IBaseAmountProvider, BaseAmountProvider>();

            // Service catalogue services
            builder.Services
                .AddServiceCatalogueCore()
                .AddServiceCatalogueWebClient(options => options.UseUrl(appSettings.ApiUrl).UseTokenInterceptor());

            // Plugin services
            builder.Services
                .AddPluginsWebClient(options => options.Url = appSettings.PartnerFdsTakenApiUrl);

            await builder.Build().InitializeApp().RunAsync();
        }

        private static WebAssemblyHost InitializeApp(this WebAssemblyHost host)
        {
            var appInitializers = new[]
            {
                (IAppInitializer)host.Services.GetRequiredService<ILocalizerService>(),
                (IAppInitializer)host.Services.GetRequiredService<IUserService>()
            };

            foreach (var appInitializer in appInitializers)
            {
                appInitializer.Initialize();
            }
            return host;
        }
    }
}