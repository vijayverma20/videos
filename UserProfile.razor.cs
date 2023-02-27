using Microsoft.AspNetCore.Components;
using RSP.Dashboard.Services.Interfaces.Auth;
using RSP.Dashboard.Services.Interfaces.Shared;

namespace RSP.Dashboard.Components.Account
{
    public partial class UserProfile : IDisposable
    {
        [Inject]
        public ILocalizerService LocalizerService { get; set; }

        [Inject]
        public IAuthService UserService { get; set; }

        private bool userDropdownOpen;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            UserService.OnLogin += StateHasChanged;
            UserService.OnLogout += StateHasChanged;
        }

        public void Dispose()
        {
            UserService.OnLogin -= StateHasChanged;
            UserService.OnLogout -= StateHasChanged;
        }

        private void ToggleUserDropdown()
        {
            userDropdownOpen = !userDropdownOpen;
        }

        private string GetUserFallbackImageText()
        {
            var username = GetUserName();
            if (username?.Length > 0)
            {
                return username.Substring(0, 1);
            }

            return "A";
        }

        private string GetUserName()
        {
            if (UserService.IsLoggedIn)
            {
                return UserService.User?.Name ?? LocalizerService["Username.User"];
            }

            return LocalizerService["Username.Anonymous"];
        }

        private string GetUserEmail()
        {
            if (UserService.IsLoggedIn)
            {
                return UserService.User?.Email;
            }

            return null;
        }
    }
}
