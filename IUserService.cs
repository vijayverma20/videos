using Mavericks.Catalogue.Client.Models;
using System;
using System.Threading.Tasks;

namespace Mavericks.Catalogue.Client.Services
{
    public interface IUserService
    {
        event Action? OnLogin;
        event Action? OnLogout;

        bool IsLoggedIn { get; }
        User? User { get; }
        Task Login(string? username, string? password);
        void Logout();
    }
}