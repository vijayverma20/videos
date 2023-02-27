using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using RSP.Dashboard.Services.Interfaces.Auth;
using System.Reflection;
using System.Net;

namespace RSP.Dashboard.Components.Routing
{
    public class AppRouteView : RouteView
    {
        [Inject]
        public IAuthService UserService { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {
            var authorizeAttribute = RouteData.PageType.GetCustomAttribute<AuthorizeAttribute>();
            if (authorizeAttribute is not null && !UserService.IsLoggedIn && !UserService.IsProcessing)
            {
                var returnUrl = WebUtility.UrlEncode(new Uri(Navigation.Uri).PathAndQuery);
                Navigation.NavigateTo($"account/login?returnUrl={returnUrl}");
                return;
            }

            base.Render(builder);
        }
    }
}
