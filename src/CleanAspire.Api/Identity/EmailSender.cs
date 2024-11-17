// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace CleanAspire.Api.Identity;

public class EmailSender : IEmailSender
{

    private readonly SmtpClientOptions _smtpOptions;
    public EmailSender(IConfiguration configuration)
    {
        _smtpOptions = configuration.GetSection("SmtpClientOptions").Get<SmtpClientOptions>();
    }
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("", _smtpOptions.DefaultFromEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        if (_smtpOptions.UsePickupDirectory && !string.IsNullOrEmpty(_smtpOptions.MailPickupDirectory))
        {
            smtpClient.LocalDomain = "localhost";
            await smtpClient.SendAsync(message);
        }
        else
        {
            await smtpClient.ConnectAsync(_smtpOptions.Server, _smtpOptions.Port, _smtpOptions.UseSsl);

            if (_smtpOptions.RequiresAuthentication)
            {
                await smtpClient.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Password);
            }

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
    }
}

public class SmtpClientOptions
{
    public const string Key = nameof(SmtpClientOptions);
    public string Server { get; set; }
    public int Port { get; set; } = 25;
    public string User { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; } = false;
    public bool RequiresAuthentication { get; set; } = true;
    public string PreferredEncoding { get; set; }
    public bool UsePickupDirectory { get; set; } = false;
    public string MailPickupDirectory { get; set; }
    public object SocketOptions { get; set; }
    public string DefaultFromEmail { get; set; }
}
