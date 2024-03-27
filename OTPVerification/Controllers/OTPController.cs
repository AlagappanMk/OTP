using Microsoft.AspNetCore.Mvc;
using OTPVerification.Models;
using OTPVerification.Services.Interface;
using OTPVerification.ViewModels;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

namespace OTPVerification.Controllers
{
    public class OTPController : Controller
    {
        private readonly IOTPService _otpService;
        public OTPController(IOTPService otpService)
        {
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult SendOTP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(MailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            try
            {
                var verifyResult = await _otpService.VerifyOTP(request);

                return View(verifyResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(UserOTPModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var verificationResult = await _otpService.ValidateOTP(model);

                if (DateTime.Now > verificationResult.ExpiryTime)
                {
                    return View("VerifyOTP", new VerifyOTPViewModel
                    {
                        EncryptedToken = model.Token,
                        EncryptedKey = model.Key,
                        EncryptedIV = model.IV,
                        Message = "OTP has expired.",
                        MessageClass = "invalid-message",
                        ToEmail = verificationResult.ToEmail
                    });
                }
                if (verificationResult.OTP == model.OTP)
                {
                    return RedirectToAction("Welcome");
                }
                else
                {
                    return View("VerifyOTP", new VerifyOTPViewModel
                    {
                        EncryptedToken = model.Token,
                        EncryptedKey = model.Key,
                        EncryptedIV = model.IV,
                        Message = "Invalid OTP.",
                        MessageClass = "invalid-message",
                        ToEmail = verificationResult.ToEmail
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendOTP(UserOTPModel userOtpModel)
        {
            try
            {
                var resendResult = await _otpService.ResendOTP(userOtpModel.ToEmail);

                return Json(resendResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public IActionResult Welcome()
        {
            return View();
        }
    }
}

