using Mavericks.Catalogue.Client.Models;
using Mavericks.Infrastructure.WebClient.Authentication;
using Mavericks.Infrastructure.WebClient.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mavericks.Catalogue.Client.Services
{
    public class UserService : IUserService, IAppInitializer
    {
        private readonly ILocalStorageService localStorageService;
        private readonly IOAuthTokenProvider tokenProvider;
        private readonly AppSettings appSettings;
        private readonly ILogger<IUserService> logger;

        public UserService(ILocalStorageService localStorageService, IOAuthTokenProvider tokenProvider, AppSettings appSettings, ILogger<IUserService> logger)
        {
            this.localStorageService = localStorageService;
            this.tokenProvider = tokenProvider;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public event Action? OnLogin;
        public event Action? OnLogout;

        public bool IsLoggedIn { get; private set; }
        public User? User { get; private set; }

        public async Task Login(string? username, string? password)
        {
            tokenProvider.HandleGetTokenFailure = null;
            var token = await tokenProvider.With(new AuthenticationRequest()
            {
                ClientId = appSettings.GatekeeperClientId,
                ClientSecret = appSettings.GatekeeperClientSecret,
                UserName = username,
                Password = password
            }).GetToken();

            if (token is null)
            {
                throw new InvalidOperationException("Invalid username or password.");
            }

            var user = ParseUserFromJwt(token);
            var tokenData = ((IPersistedTokenProvider)tokenProvider).GetSerializedData();

            IsLoggedIn = true;
            User = user;

            localStorageService.Set(LocalStorageKey.User, user);
            localStorageService.Set(LocalStorageKey.TokenData, tokenData);

            OnLogin?.Invoke();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            User = null;

            localStorageService.Set(LocalStorageKey.User, null);
            localStorageService.Set(LocalStorageKey.TokenData, null);
            ((IPersistedTokenProvider)tokenProvider).RestoreData(null);

            OnLogout?.Invoke();
        }

        public void Initialize()
        {
            var user = localStorageService.Get<User>(LocalStorageKey.User);
            var tokenData = localStorageService.Get(LocalStorageKey.TokenData);
            if (user is not null && tokenData is not null)
            {
                IsLoggedIn = true;
                User = user;
                tokenProvider.With(new AuthenticationRequest()
                {
                    ClientId = appSettings.GatekeeperClientId,
                    ClientSecret = appSettings.GatekeeperClientSecret
                });
                tokenProvider.HandleGetTokenFailure = HandleGetTokenFailure;
                ((IPersistedTokenProvider)tokenProvider).RestoreData(tokenData);
            }
        }

        private void HandleGetTokenFailure(Exception exception)
        {
            logger.LogError(exception, "Failed to get token.");
            Logout();
        }

        private static User ParseUserFromJwt(string token)
        {
            var tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
            {
                throw new InvalidOperationException("Invalid JWT token.");
            }

            var jwtPayloadBase64 = tokenParts[1];
            switch (jwtPayloadBase64.Length % 4)
            {
                case 1: jwtPayloadBase64 += "==="; break;
                case 2: jwtPayloadBase64 += "=="; break;
                case 3: jwtPayloadBase64 += "="; break;
            }

            var userInfo = JsonSerializer.Deserialize<JsonWebTokenPayload>(Encoding.UTF8.GetString(Convert.FromBase64String(jwtPayloadBase64)));
            if (userInfo is null)
            {
                throw new InvalidOperationException("JWT token payload is empty.");
            }

            return new User()
            {
                Name = userInfo.DisplayName,
                Email = userInfo.Email
            };
        }

        private sealed class JsonWebTokenPayload
        {
            [JsonPropertyName("mail")]
            public string? Email { get; set; }
            [JsonPropertyName("name")]
            public string? Username { get; set; }
            [JsonPropertyName("displayName")]
            public string? DisplayName { get; set; }
        }
    }
}
