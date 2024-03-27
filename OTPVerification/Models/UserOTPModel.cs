namespace OTPVerification.Models
{
    public class UserOTPModel
    {
        public string? OTP { get; set; }
        public string? Token { get; set; }
        public string? Key { get; set; }
        public string? IV { get; set; }
        public string? ToEmail { get; set; }

    }
}
