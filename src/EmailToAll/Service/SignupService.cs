using EmailToAll.Data;
using EmailToAll.Data.Interface;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class SignupService
    {
        private const int MinPasswordLength = 8;

        private readonly IUnitOfWork _unitofWork;
        private readonly EmailService _emailService;
        private readonly JwtService _jwtService;
        private readonly EncryptionService _encryptionService;
        private readonly ILogger<SignupService> _logger;

        public SignupService(
            IUnitOfWork unitOfWork,
            EmailService emailService,
            JwtService jwtService,
            EncryptionService encryptionService,
            ILogger<SignupService> logger)
        {
            _unitofWork = unitOfWork;
            _emailService = emailService;
            _jwtService = jwtService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<object> Signup(Signup signup)
        {
            if (signup == null)
            {
                throw new Exception("Input Field Null");
            }

            if (!signup.Email.TryParseEmail(out var email))
            {
                throw new Exception("Please enter a valid email address.");
            }

            email = email.NormalizeEmail();

            if (string.IsNullOrEmpty(signup.Password) || signup.Password.Length < MinPasswordLength)
            {
                throw new Exception($"Password must be at least {MinPasswordLength} characters.");
            }

            if (signup.Password != signup.ConfirmPass)
            {
                throw new Exception("Password and Confirm Password do not match.");
            }

            if (await _unitofWork.Signup.GetByEmailAsync(email) != null)
            {
                throw new Exception("An account with this email already exists.");
            }

            var now = DateTime.UtcNow;
            var user = new Signup
            {
                Email = email,
                Password = _encryptionService.Encrypt(signup.Password),
                CreateDate = now,
                UpdateDate = now,
                IsActive = true
            };
            await _unitofWork.Signup.AddAsync(user);

            var otpCode = OtpCode.Generate();
            var otpPayload = new OtpVerification
            {
                Email = email,
                OtpCode = otpCode,
                CreatedAt = now,
                ExpiryTime = now.AddMinutes(OtpCode.ValidMinutes),
                IsUsed = false
            };

            await _unitofWork.Otp.CreateAsync(otpPayload);
            await _unitofWork.SaveAsync();

            try
            {
                await SendOtpEmailAsync(email, otpCode);
            }
            catch (Exception ex)
            {
                // Do not leave an unverifiable account behind, so the user can simply try again.
                _logger.LogError(ex, "Could not send the verification email to {Email}.", email);
                await _unitofWork.Signup.DeleteAsync(user.Id);
                throw new Exception("We could not send the verification email. Check the address and try again.");
            }

            return new
            {
                Message = "Signup added successfully. OTP sent to email.",
            };
        }

        public async Task<string> OptVerification(SignupTokenValidate payload)
        {
            if (payload == null)
            {
                throw new Exception("Input Field Null");
            }

            var otp = await _unitofWork.Otp.GetValidAsync(payload.Email.NormalizeEmail(), payload.Token?.Trim() ?? string.Empty);
            if (otp == null)
            {
                throw new Exception("Invalid or expired OTP");
            }

            otp.IsUsed = true;
            await _unitofWork.Otp.UpdateAsync(otp);
            await _unitofWork.SaveAsync();

            return "OTP Verified Successfully";
        }

        public async Task<string> ResendOtp(ResendOtp payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new Exception("Email is required");
            }

            var email = payload.Email.NormalizeEmail();
            if (await _unitofWork.Signup.GetByEmailAsync(email) == null)
            {
                throw new Exception("Invalid email");
            }

            var pending = (await _unitofWork.Otp.GetAllAsync()).Where(x => x.Email == email && !x.IsUsed).ToList();
            foreach (var old in pending)
            {
                old.IsUsed = true;
                await _unitofWork.Otp.UpdateAsync(old);
            }

            var otpCode = OtpCode.Generate();
            var otpPayload = new OtpVerification
            {
                Email = email,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(OtpCode.ValidMinutes),
                IsUsed = false
            };

            await _unitofWork.Otp.CreateAsync(otpPayload);

            try
            {
                await SendOtpEmailAsync(email, otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not resend the verification email to {Email}.", email);
                throw new Exception("We could not send the verification email. Please try again later.");
            }

            return "OTP Resent Successfully";
        }

        private Task SendOtpEmailAsync(string email, string otpCode)
        {
            string emailBody = $@"
                    <h3>Welcome to EmailToAll, {email}!</h3>
                    <p>Your verification code is <b>{otpCode}</b></p>
                    <p>This code will expire in {OtpCode.ValidMinutes} minutes.</p>";

            return _emailService.SendEmailAsync(email, "Signup Email Verification OTP", emailBody);
        }
    }
}
