using Blazorise;
using Mavericks.Infrastructure.WebClient;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mavericks.Catalogue.Client.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        private readonly INotificationService notificationService;
        private readonly ILocalizerService localizerService;

        public AppNotificationService(INotificationService notificationService, ILocalizerService localizerService)
        {
            this.notificationService = notificationService;
            this.localizerService = localizerService;
        }

        public Task Success(string message)
            => CreateNotification(notificationService.Success, message);

        public Task Success(IEnumerable<string> messages)
            => CreateNotification(notificationService.Success, messages);

        public Task SuccessWithTitle(string message, string? title = null)
            => CreateNotification(notificationService.Success, message, title ?? localizerService["Success"]);

        public Task SuccessWithTitle(IEnumerable<string> messages, string? title = null)
            => CreateNotification(notificationService.Success, messages, title ?? localizerService["Success"]);

        public Task Error(string message)
            => CreateNotification(notificationService.Error, message);

        public Task Error(IEnumerable<string> messages)
            => CreateNotification(notificationService.Error, messages);

        public Task Error(Exception exception)
            => CreateErrorNotification(exception);

        public Task ErrorWithTitle(string message, string? title = null)
            => CreateNotification(notificationService.Error, message, title ?? localizerService["Error"]);

        public Task ErrorWithTitle(IEnumerable<string> messages, string? title = null)
            => CreateNotification(notificationService.Error, messages, title ?? localizerService["Error"]);

        public Task ErrorWithTitle(Exception exception, string? title = null)
            => CreateErrorNotification(exception, title ?? localizerService["Error"]);

        private Task CreateNotification(Func<string, string?, Action<NotificationOptions>?, Task> method, string message, string? title = null)
        {
            return method.Invoke(message, title, null);
        }

        private Task CreateNotification(Func<MarkupString, string?, Action<NotificationOptions>?, Task> method, IEnumerable<string> messages, string? title = null)
        {
            var message = new MarkupString("<ul>" + string.Join("", messages.Select(m => $"<li>{m}</li>")) + "</ul>");
            return method.Invoke(message, title, null);
        }

        private Task CreateErrorNotification(Exception exception, string? title = null)
        {
            if (exception is ProblemDetailsException problemDetailsException)
            {
                if (problemDetailsException.Problem.Errors.Any())
                {
                    var errorList = problemDetailsException.Problem.Errors.SelectMany(error => error.Value.Select(val => string.IsNullOrEmpty(error.Key) ? $"<li>{val}</li>" : $"<li>{error.Key}: {val}</li>"));
                    return notificationService.Error(new MarkupString("<ul>" + string.Join("", errorList) + "</ul>"), problemDetailsException.Problem.Title);
                }
                return notificationService.Error(problemDetailsException.Message, problemDetailsException.Problem.Title ?? title);
            }

            var message = $"{exception.GetType().Name}: {exception.Message}";
            return notificationService.Error(message, title);
        }
    }
}
