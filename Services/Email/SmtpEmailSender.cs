using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace JobMatch.Services.Email
{
    public class SmtpEmailSender : IAppEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) { _config = config; }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _config["Email:Smtp:Host"];
            var portStr = _config["Email:Smtp:Port"];
            var user = _config["Email:Smtp:Username"];
            var pass = _config["Email:Smtp:Password"];
            var from = _config["Email:From"] ?? user;
            int port = 587;
            int.TryParse(portStr, out port);

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
                return; 

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true
            };

            if (!string.IsNullOrEmpty(user))
            {
                client.Credentials = new NetworkCredential(user, pass);
            }

            var msg = new MailMessage(from, toEmail, subject, htmlBody) { IsBodyHtml = true };
            await client.SendMailAsync(msg);
        }
    }
}
