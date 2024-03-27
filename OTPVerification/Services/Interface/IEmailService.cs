using OTPVerification.Models;

namespace OTPVerification.Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailRequest mailrequest);
    }
}
