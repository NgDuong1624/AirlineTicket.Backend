using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Notifications.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    public EmailSender(ILogger<EmailSender> logger) => _logger = logger;
    public Task<(bool Success, string? Error)> SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending Email to {To}: {Subject}", to, subject);
        return Task.FromResult((true, (string?)null));
    }
}

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;
    public SmsSender(ILogger<SmsSender> logger) => _logger = logger;
    public Task<(bool Success, string? Error)> SendAsync(string phone, string message)
    {
        _logger.LogInformation("Sending SMS to {Phone}: {Message}", phone, message);
        return Task.FromResult((true, (string?)null));
    }
}

public class PushSender
{
    private readonly ILogger<PushSender> _logger;
    public PushSender(ILogger<PushSender> logger) => _logger = logger;
    public Task<(bool Success, string? Error)> SendAsync(string recipient, string message)
    {
        _logger.LogInformation("Sending Push Notification to {Recipient}: {Message}", recipient, message);
        return Task.FromResult((true, (string?)null));
    }
}

public class NotificationSender : INotificationSender
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly PushSender _pushSender;

    public NotificationSender(IEmailSender emailSender, ISmsSender smsSender, PushSender pushSender)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
    }

    public async Task<(bool Success, string? Error)> SendAsync(Notification notification)
    {
        return notification.Type switch
        {
            0 => await _emailSender.SendAsync(notification.Recipient, notification.Subject ?? "Notification", notification.Content),
            1 => await _smsSender.SendAsync(notification.Recipient, notification.Content),
            2 => await _pushSender.SendAsync(notification.Recipient, notification.Content),
            _ => (false, "Unsupported notification type")
        };
    }
}
