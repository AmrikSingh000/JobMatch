using System.Threading.Tasks;

namespace JobMatch.Services.Email
{
    public interface IAppEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
