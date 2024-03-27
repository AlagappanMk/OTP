namespace OTPVerification.Models
{
    public class MailRequest
    {
        public string? ToEmail { get; set; }
        public string? OTP { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string? Token { get; set; }
    }
}
