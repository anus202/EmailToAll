using EmailToAll.Data;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class LoginService
    {
        private const int MinPasswordLength = 8;

        private readonly IUnitOfWork _unitOfWork;
        private readonly EncryptionService _encryptionService;
        private readonly EmailService _emailservice;
        private readonly JwtService _jwtService;
        private readonly ILogger<LoginService> _logger;

        public LoginService(
            IUnitOfWork unitOfWork,
            EncryptionService encryptionService,
            EmailService emailService,
            JwtService jwtService,
            ILogger<LoginService> logger)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _emailservice = emailService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<object> Login(LoginPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrEmpty(payload.Password))
            {
                throw new Exception("Email and password are required");
            }

            var user = await _unitOfWork.Signup.GetByEmailAsync(payload.Email.NormalizeEmail());
            if (user == null || user.Password != _encryptionService.Encrypt(payload.Password))
            {
                throw new Exception("Invalid email or password");
            }

            var token = await _jwtService.GenerateToken(user.Id, user.Email);
            return new
            {
                message = "Login successful",
                token = token,
                email = user.Email
            };
        }

        public async Task<string> ForgetPassword(ForgetPass payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new Exception("Email is required");
            }

            var email = payload.Email.NormalizeEmail();
            var user = await _unitOfWork.Signup.GetByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("Invalid email");
            }

            var code = OtpCode.Generate();
            var otpPayload = new OtpVerification
            {
                Email = email,
                OtpCode = code,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(OtpCode.ValidMinutes),
                IsUsed = false
            };

            await _unitOfWork.Otp.CreateAsync(otpPayload);
            await _unitOfWork.SaveAsync();

            string emailBody = $@"
                    <h3>Hello {email},</h3>
                    <p>Your password reset code is <b>{code}</b></p>
                    <p>This code will expire in {OtpCode.ValidMinutes} minutes.</p>";

            try
            {
                await _emailservice.SendEmailAsync(email, "Password Reset OTP", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send the password reset email to {Email}.", email);
                throw new Exception("We could not send the email. Please try again later.");
            }

            return "Password reset code sent to email";
        }

        /// <summary>
        /// Checks the code without consuming it; <see cref="PasswordReset"/> consumes it.
        /// </summary>
        public async Task<string> OptVerification(LoginTokenValidate payload)
        {
            if (payload == null)
            {
                throw new Exception("Input Field Null");
            }

            var otp = await _unitOfWork.Otp.GetValidAsync(payload.Email.NormalizeEmail(), payload.Token?.Trim() ?? string.Empty);
            if (otp == null)
            {
                throw new Exception("Invalid or expired OTP");
            }

            return "OTP Verified Successfully";
        }

        public async Task<string> PasswordReset(PasswordResetPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Token))
            {
                throw new Exception("Email and OTP are required");
            }

            if (string.IsNullOrEmpty(payload.Password) || payload.Password.Length < MinPasswordLength)
            {
                throw new Exception($"Password must be at least {MinPasswordLength} characters.");
            }

            var email = payload.Email.NormalizeEmail();
            var otp = await _unitOfWork.Otp.GetValidAsync(email, payload.Token.Trim());
            if (otp == null)
            {
                throw new Exception("Invalid or expired OTP");
            }

            var user = await _unitOfWork.Signup.GetByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("Invalid email");
            }

            // Burn the code first so it can never be replayed.
            otp.IsUsed = true;
            await _unitOfWork.Otp.UpdateAsync(otp);

            user.Password = _encryptionService.Encrypt(payload.Password);
            user.UpdateDate = DateTime.UtcNow;
            await _unitOfWork.Signup.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return "Password Updated Successfully";
        }
    }
}
