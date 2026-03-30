namespace Fintex.Web.Host.Notifications
{
    /// <summary>
    /// SMTP settings for transactional notification emails.
    /// </summary>
    public class NotificationEmailOptions
    {
        public bool Enabled { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public bool UseStartTls { get; set; }

        public bool UseSsl { get; set; }
    }
}
