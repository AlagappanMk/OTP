using OTPVerification.Models;
using OTPVerification.ViewModels;

namespace OTPVerification.Services.Interface
{
    public interface IOTPService
    {
        Task<VerifyOTPViewModel> VerifyOTP(MailRequest request);
        Task<VerifyOTPViewModel> ValidateOTP(UserOTPModel model);
        Task<object> ResendOTP(string email);
    }
}
