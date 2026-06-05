using System.Net;
using System.Net.Mail;

namespace Wellora.Services
{
    public interface IEmailService
    {
        void SendEmail(string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string subject, string body)
        {
            var smtpSection = _config.GetSection("Smtp");
            var smtpClient = new SmtpClient(smtpSection["Host"])
            {
                Port = int.Parse(smtpSection["Port"]),
                Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
                EnableSsl = bool.Parse(smtpSection["EnableSsl"])
            };

            smtpClient.Send(
                smtpSection["From"],
                smtpSection["To"],
                subject,
                body
            );
        }
    }
}