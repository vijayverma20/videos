using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace SEB.OutpaymentSystem.Client
{
    public partial class App
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Inject]
        NotificationService NotificationService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            base.OnInitialized();
            if (!NavigationManager.BaseUri.Contains("localhost"))
            {
                await RegisterForUpdateAvailableNotification();
            }
        }

        private async Task RegisterForUpdateAvailableNotification()
        {
            await jsRuntime.InvokeAsync<object>(
                identifier: "registerForUpdateAvailableNotification",
                DotNetObjectReference.Create(this),
                nameof(OnUpdateAvailable));
        }

        [JSInvokable(nameof(OnUpdateAvailable))]
        public Task OnUpdateAvailable()
        {
            NotificationService.Notify(
                new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Update available",
                    Detail = "A new version of the application is available. Click here to reload.",
                    Click = async (not) => await jsRuntime.InvokeVoidAsync("reloadApp"),
                    CloseOnClick = false,
                    Duration = 60 * 60 * 1000
                });

            return Task.CompletedTask;
        }
    }
}
