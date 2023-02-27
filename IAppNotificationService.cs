using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mavericks.Catalogue.Client.Services
{
    public interface IAppNotificationService
    {
        Task Success(string message);
        Task Success(IEnumerable<string> messages);
        Task SuccessWithTitle(string message, string? title = null);
        Task SuccessWithTitle(IEnumerable<string> messages, string? title = null);
        Task Error(string message);
        Task Error(IEnumerable<string> messages);
        Task Error(Exception exception);
        Task ErrorWithTitle(string message, string? title = null);
        Task ErrorWithTitle(IEnumerable<string> messages, string? title = null);
        Task ErrorWithTitle(Exception exception, string? title = null);
    }
}