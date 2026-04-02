using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace Fintex.Investments.Notifications
{
    internal static class NotificationEmailTemplateBuilder
    {
        public static string Build(NotificationItem notification, string intro = null)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var title = Encode(notification.Title);
            var message = Encode(notification.Message);
            var symbol = Encode(notification.Symbol);
            var provider = Encode(notification.Provider.ToString());
            var typeLabel = Encode(GetTypeLabel(notification.Type));
            var severityLabel = Encode(notification.Severity.ToString());
            var severityColor = GetSeverityColor(notification.Severity);
            var verdict = notification.Verdict?.ToString();
            var occurredAt = FormatOccurredAt(notification.OccurredAt);
            var introCopy = Encode(
                string.IsNullOrWhiteSpace(intro)
                    ? "Fintex triggered this alert based on your current notification preferences."
                    : intro);

            var details = new StringBuilder();
            AppendDetail(details, "Type", typeLabel);
            AppendDetail(details, "Severity", severityLabel);
            AppendDetail(details, "Symbol", symbol);
            AppendDetail(details, "Provider", provider);

            if (notification.ReferencePrice.HasValue)
            {
                AppendDetail(details, "Reference price", FormatPrice(notification.ReferencePrice.Value));
            }

            if (notification.TargetPrice.HasValue)
            {
                AppendDetail(details, "Target price", FormatPrice(notification.TargetPrice.Value));
            }

            if (!string.IsNullOrWhiteSpace(verdict))
            {
                AppendDetail(details, "Verdict", Encode(verdict));
            }

            if (notification.ConfidenceScore.HasValue)
            {
                AppendDetail(details, "Confidence", $"{notification.ConfidenceScore.Value.ToString("0.0", CultureInfo.InvariantCulture)}%");
            }

            AppendDetail(details, "Occurred", occurredAt);

            return $@"<!doctype html>
<html lang=""en"">
  <body style=""margin:0;padding:0;background:#050907;font-family:Segoe UI,Arial,sans-serif;color:#eaf7ee;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#050907;padding:24px 12px;"">
      <tr>
        <td align=""center"">
          <table role=""presentation"" width=""640"" cellspacing=""0"" cellpadding=""0"" style=""width:640px;max-width:100%;background:#0b140f;border:1px solid #1a3022;border-radius:14px;overflow:hidden;"">
            <tr>
              <td style=""padding:18px 20px;background:#0f1e15;border-bottom:1px solid #1f3a2a;"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                  <tr>
                    <td style=""font-size:14px;font-weight:700;color:#9bf2b1;letter-spacing:0.04em;text-transform:uppercase;"">Fintex Alerts</td>
                    <td align=""right"">
                      <span style=""display:inline-block;padding:6px 10px;border-radius:999px;background:{severityColor};color:#ffffff;font-size:12px;font-weight:700;"">{severityLabel}</span>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>

            <tr>
              <td style=""padding:22px 20px 6px 20px;"">
                <h1 style=""margin:0 0 8px 0;font-size:22px;line-height:1.3;color:#ecfff2;"">{title}</h1>
                <p style=""margin:0;color:#b9d4c2;font-size:14px;line-height:1.7;"">{message}</p>
              </td>
            </tr>

            <tr>
              <td style=""padding:12px 20px 0 20px;"">
                <p style=""margin:0;color:#93b7a0;font-size:13px;line-height:1.7;"">{introCopy}</p>
              </td>
            </tr>

            <tr>
              <td style=""padding:16px 20px 6px 20px;"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;background:#0f1a14;border:1px solid #1c3024;border-radius:10px;overflow:hidden;"">
                  {details}
                </table>
              </td>
            </tr>

            <tr>
              <td style=""padding:14px 20px 20px 20px;"">
                <p style=""margin:0;color:#9dbca9;font-size:12px;line-height:1.7;"">
                  You are receiving this because alert email notifications are enabled in your Fintex account.
                </p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }

        private static void AppendDetail(StringBuilder builder, string label, string value)
        {
            builder.Append($@"
<tr>
  <td style=""padding:10px 12px;border-bottom:1px solid #1b2c22;color:#99b6a5;font-size:12px;text-transform:uppercase;letter-spacing:0.04em;width:170px;"">{label}</td>
  <td style=""padding:10px 12px;border-bottom:1px solid #1b2c22;color:#e7f6ed;font-size:13px;"">{value}</td>
</tr>");
        }

        private static string FormatOccurredAt(DateTime occurredAt)
        {
            var utcTime = occurredAt.Kind == DateTimeKind.Utc
                ? occurredAt
                : DateTime.SpecifyKind(occurredAt, DateTimeKind.Utc);

            return utcTime.ToString("dddd, MMMM d, yyyy 'at' hh:mm tt 'UTC'", CultureInfo.InvariantCulture);
        }

        private static string FormatPrice(decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string GetTypeLabel(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PriceTarget:
                    return "Price Target";
                case NotificationType.TradeAutomation:
                    return "Automation Rule";
                case NotificationType.TradeFill:
                    return "Trade Fill";
                case NotificationType.GoalAutomation:
                    return "Goal Automation";
                default:
                    return "Trade Opportunity";
            }
        }

        private static string GetSeverityColor(NotificationSeverity severity)
        {
            switch (severity)
            {
                case NotificationSeverity.Success:
                    return "#2a8f46";
                case NotificationSeverity.Warning:
                    return "#b88a14";
                case NotificationSeverity.Danger:
                    return "#bf3a3a";
                default:
                    return "#2d6f46";
            }
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
