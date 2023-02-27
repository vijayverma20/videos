using Microsoft.Extensions.Logging;
using RSP.Dashboard.Services.Interfaces.Auth;
using RSP.Dashboard.Services.Interfaces.Shared;
using RSP.Dashboard.Services.Models;
using System.Text.Json.Serialization;
using System.Text;
using Mavericks.Infrastructure.WebClient.Authentication;
using Mavericks.Infrastructure.WebClient.Authentication.OAuth;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace RSP.Dashboard.Services.Services.Auth
{
    public class AuthService : IAuthService, IAppInitializer
    {
        private readonly ILocalStorageService localStorageService;
        private readonly IOAuthTokenProvider tokenProvider;
        private readonly AppSettings appSettings;
        private readonly ILogger<IAuthService> logger;

        public AuthService(ILocalStorageService localStorageService, IOAuthTokenProvider tokenProvider, AppSettings appSettings, ILogger<IAuthService> logger)
        {
            this.localStorageService = localStorageService;
            this.tokenProvider = tokenProvider;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public event Action OnLogin;
        public event Action OnLogout;

        public bool IsLoggedIn { get; private set; }
        public bool IsProcessing { get; private set; }
        public User User { get; private set; }

        public async Task Login(string username, string password)
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
            user.Sid = username;

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

        public async Task Initialize()
        {
            IsProcessing = true;
            try
            {
                var user = localStorageService.Get<User>(LocalStorageKey.User);
                var tokenData = localStorageService.Get(LocalStorageKey.TokenData);

                if (user is not null && tokenData is not null)
                {
                    tokenProvider.HandleGetTokenFailure = HandleGetTokenFailure;

                    tokenProvider.With(new AuthenticationRequest()
                    {
                        ClientId = appSettings.GatekeeperClientId,
                        ClientSecret = appSettings.GatekeeperClientSecret
                    });

                    ((IPersistedTokenProvider)tokenProvider).RestoreData(tokenData);

                    var newToken = await tokenProvider.GetToken();

                    if (newToken != null)
                    {
                        if (!IsValid(newToken))
                        {
                            Logout();
                        }

                        tokenData = ((IPersistedTokenProvider)tokenProvider).GetSerializedData();
                        var userData = ParseUserFromJwt(newToken);
                        userData.Sid = user.Sid;

                        IsLoggedIn = true;
                        user = userData;

                        localStorageService.Set(LocalStorageKey.User, user);
                        localStorageService.Set(LocalStorageKey.TokenData, tokenData);
                    }
                }
            }
            finally
            {
                IsProcessing = false;
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

        private bool IsValid(string token)
        {
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityToken(token);
            }
            catch (Exception)
            {
                return false;
            }

            return jwtSecurityToken.ValidTo > DateTime.UtcNow;
        }

        private sealed class JsonWebTokenPayload
        {
            [JsonPropertyName("mail")]
            public string Email { get; set; }
            [JsonPropertyName("name")]
            public string Username { get; set; }
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }
        }
    }
}
