using Fintex.Investments.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Notifications
{
    /// <summary>
    /// SMTP sender for ZeptoMail-backed notification emails.
    /// </summary>
    public class ZeptoMailNotificationEmailSender : INotificationEmailSender
    {
        private readonly NotificationEmailOptions _options;

        public ZeptoMailNotificationEmailSender(IOptions<NotificationEmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string recipientName, string recipientEmail, string subject, string htmlBody)
        {
            if (!_options.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                throw new InvalidOperationException("Recipient email is required for notification delivery.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(new MailboxAddress(recipientName ?? "Trader", recipientEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using (var client = new SmtpClient())
            {
                client.SslProtocols = SslProtocols.Tls12;

                var socketOptions = _options.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : _options.UseStartTls
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.Auto;

                await client.ConnectAsync(_options.Host, _options.Port, socketOptions);
                await client.AuthenticateAsync(_options.Username, _options.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
