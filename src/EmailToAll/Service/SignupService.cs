using EmailToAll.Data;
using EmailToAll.Data.Interface;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class SignupService
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly EmailService _emailService;
        private readonly JwtService _jwtService;
        private readonly EncryptionService _encryptionService;

        public SignupService(IUnitOfWork unitOfWork, EmailService emailService, JwtService jwtService, EncryptionService encryptionService)
        {
            _unitofWork = unitOfWork;
            _emailService = emailService;
            _jwtService = jwtService;
            _encryptionService = encryptionService;
        }
        public async Task<object> Signup(Signup signup)
        {
            try
            {
                if (signup == null)
                {
                    throw new Exception("Input Field Null");
                }
                if (signup.Password != signup.ConfirmPass)
                {
                    throw new Exception("Password and Confirm Password do not match.");
                }

                signup.Password = _encryptionService.Encrypt(signup.Password);
                await _unitofWork.Signup.AddAsync(signup);

                string otpCode = new Random().Next(100000, 999999).ToString();

                OtpVerification otpPayload = new OtpVerification
                {

                    Email = signup.Email,
                    OtpCode = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _unitofWork.Otp.CreateAsync(otpPayload);
                await _unitofWork.SaveAsync();

                string emailBody = $@"
                    <h3>Welcome to EmailToAll, {signup.Email}!</h3>
                    <p>Your verification code is <b>{otpCode}</b></p>
                    <p>This code will expire in 5 minutes.</p>";

                await _emailService.SendEmailAsync(signup.Email, "Signup Email Verification OTP", emailBody);
                return new
                {
                    Message = "Signup added successfully. OTP sent to email.",
                };

            }
            catch (Exception ex)
            {
                return "Failed to add signup: " + ex.Message;
            }
        }
        public async Task<string> OptVerification(SignupTokenValidate payload)
        {
            try
            {
                var res = string.Empty;
                var getOtp = await _unitofWork.Otp.GetAllAsync();

                var ChkOtp = getOtp.FirstOrDefault(x => x.Email == payload.Email && x.OtpCode == payload.Token && x.IsUsed == false);
                if (ChkOtp != null)
                {
                    ChkOtp.IsUsed = true;
                    await _unitofWork.Otp.UpdateAsync(ChkOtp);
                    await _unitofWork.SaveAsync();
                }

                return res = ChkOtp != null ? "OTP Verified Successfully" : "Invalid OTP";
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> ResendOtp(ResendOtp payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
                {
                    return "Email is required";
                }

                var getOtp = await _unitofWork.Otp.GetAllAsync();

                var exp = getOtp.FirstOrDefault(x => x.Email == payload.Email && x.IsUsed == false);

                if (exp != null)
                {
                    exp.IsUsed = true;

                    await _unitofWork.Otp.UpdateAsync(exp);
                }

                string otpCode = Random.Shared
                    .Next(100000, 1000000)
                    .ToString();

                var otpPayload = new OtpVerification
                {
                    Email = payload.Email,
                    OtpCode = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _unitofWork.Otp.CreateAsync(otpPayload);

                string emailBody = $@"
                    <h3>Welcome to EmailToAll, {payload.Email}!</h3>
                    <p>Your verification code is <b>{otpCode}</b></p>
                    <p>This code will expire in 5 minutes.</p>";

                await _emailService.SendEmailAsync(payload.Email, "Signup Email Verification OTP", emailBody);

                return "OTP Resent Successfully";
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
