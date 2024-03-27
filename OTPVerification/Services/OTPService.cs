using OTPVerification.Models;
using OTPVerification.Services.Interface;
using OTPVerification.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace OTPVerification.Services
{
    public class OTPService : IOTPService
    {
        private readonly IEmailService _emailService;

        public OTPService(IEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task<VerifyOTPViewModel> VerifyOTP(MailRequest request)
        {
            try
            {
                string otp = GenerateOTP();
                string token = CreateToken(request.ToEmail, otp);
                byte[] key = GenerateKey();
                byte[] iv = GenerateIV();

                // Encrypt the token
                string encryptedToken = Encrypt(token, key, iv);
                string encryptedKey = Convert.ToBase64String(key);
                string encryptedIV = Convert.ToBase64String(iv);

                request.OTP = otp;
                request.Token = encryptedToken;

                await _emailService.SendEmailAsync(request);

                var verifyResult = new VerifyOTPViewModel
                {
                    EncryptedToken = encryptedToken,
                    EncryptedKey = encryptedKey,
                    EncryptedIV = encryptedIV,
                    ToEmail = request.ToEmail
                };

                return verifyResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while verifying OTP.", ex);
            }
        }

        public async Task<VerifyOTPViewModel> ValidateOTP(UserOTPModel model)
        {
            try
            {
                string decryptedToken = Decrypt(model.Token, Convert.FromBase64String(model.Key), Convert.FromBase64String(model.IV));

                string[] parts = decryptedToken.Split('|');
                string email = parts[0];
                string otp = parts[1];
                DateTime expiryTime = DateTime.Parse(parts[2]);

                return new VerifyOTPViewModel
                {
                    ToEmail = email,
                    OTP = otp,
                    ExpiryTime = expiryTime
                };

            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while validating OTP.", ex);
            }
        }
        public async Task<object> ResendOTP(string email)
        {
            try
            {
                // Generate a new OTP
                string newOtp = GenerateOTP();

                // Create a new token with the updated OTP
                string newToken = CreateToken(email, newOtp);

                byte[] newKey = GenerateKey();
                byte[] newIV = GenerateIV();

                // Encrypt the token
                string encryptedToken = Encrypt(newToken, newKey, newIV);
                string encryptedKey = Convert.ToBase64String(newKey);
                string encryptedIV = Convert.ToBase64String(newIV);

                // Send the new OTP via email
                var model = new MailRequest { ToEmail = email, OTP = newOtp, Token = encryptedToken };
                await _emailService.SendEmailAsync(model);

                return new
                {
                    Success = true,
                    OTP = newOtp,
                    Token = encryptedToken,
                    Key = encryptedKey,
                    IV = encryptedIV,
                    ToEmail = email
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while resending OTP.", ex);
            }
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        private string CreateToken(string email, string otp)
        {
            DateTime expiryTime = DateTime.Now.AddMinutes(2);
            return $"{email}|{otp}|{expiryTime:o}";
        }

        private string Encrypt(string token, byte[] key, byte[] iv)
        {
            // Encrypt token using a symmetric encryption algorithm
            byte[] data = Encoding.UTF8.GetBytes(token);
            byte[] encryptedData;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
            }

            return Convert.ToBase64String(encryptedData);
        }

        private byte[] GenerateKey()
        {
            // Generate a random encryption key
            byte[] key = new byte[32];  // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        private byte[] GenerateIV()
        {
            // Generate a random initialization vector (IV)
            byte[] iv = new byte[16]; // 128 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        private string Decrypt(string encryptedToken, byte[] key, byte[] iv)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedToken);
            byte[] decryptedData;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            }

            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
