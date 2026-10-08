using EmailToAll.Data;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class LoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EncryptionService _encryptionService;
        private readonly EmailService _emailservice;
        private readonly JwtService _jwtService;

        public LoginService(IUnitOfWork unitOfWork, EncryptionService encryptionService, EmailService emailService, JwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _emailservice = emailService;
            _jwtService = jwtService;
        }

        public async Task<object> Login(LoginPayload payload)
        {
            try
            {
                if (payload == null || string.IsNullOrEmpty(payload.Email) || string.IsNullOrEmpty(payload.Password))
                {
                    return "Email and password are required";
                }
                var user = (await _unitOfWork.Signup.GetAlllAsync());

                if (user == null || !user.Any())
                {
                    return "Invalid email or password";
                }

                var encPassword = _encryptionService.Encrypt(payload.Password);
                var res = user.FirstOrDefault(x => x.Email == payload.Email && x.Password == encPassword
                );
                var token = await _jwtService.GenerateToken(2, payload.Email);
                if (res == null)
                {
                    return "Invalid email or password";
                }
                return new
                {
                    messeg = "Signup added successfully. OTP sent to email.",
                    token = token,
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<string> ForgetPassword(ForgetPass payload)
        {
            try
            {
                if (payload == null || string.IsNullOrEmpty(payload.Email))
                {
                    return "Email is required";
                }
                var user = (await _unitOfWork.Signup.GetAlllAsync());
                if (user == null || !user.Any())
                {
                    return "Invalid email";
                }
                var res = user.FirstOrDefault(x => x.Email == payload.Email);
                if (res == null)
                {
                    return "Invalid email";
                }

                var genratetoken = new Random().Next(100000, 999999).ToString();
                OtpVerification otpPayload = new OtpVerification
                {

                    Email = payload.Email,
                    OtpCode = genratetoken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _unitOfWork.Otp.CreateAsync(otpPayload);
                await _unitOfWork.SaveAsync();
                string emailBody = $@"
                    <h3>Hello {payload.Email},</h3>
                    <p>Your password reset code is <b>{genratetoken}</b></p>
                    <p>This code will expire in 5 minutes.</p>";

                await _emailservice.SendEmailAsync(payload.Email, "Password Reset OTP", emailBody);
                return "Password reset link sent to email";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> OptVerification(LoginTokenValidate payload)
        {
            try
            {
                var res = string.Empty;
                var getOtp = await _unitOfWork.Otp.GetAllAsync();

                var ChkOtp = getOtp.FirstOrDefault(x => x.Email == payload.Email && x.OtpCode == payload.Token && x.IsUsed == false);
                if (ChkOtp != null)
                {
                    ChkOtp.IsUsed = true;
                    await _unitOfWork.Otp.UpdateAsync(ChkOtp);
                    await _unitOfWork.SaveAsync();
                }

                return res = ChkOtp != null ? "OTP Verified Successfully" : "Invalid OTP";
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> PasswordReset(LoginPayload payload)
        {
            try
            {
                var res = string.Empty;
                var getOtp = await _unitOfWork.Otp.GetAllAsync();

                var getuser = (await _unitOfWork.Signup.GetAlllAsync()).FirstOrDefault(x => x.Email == payload.Email);
                getuser.Password = _encryptionService.Encrypt(payload.Password);
                await _unitOfWork.Signup.UpdateAsync(getuser);
                await _unitOfWork.SaveAsync();

                return "Password Updated Successfully";

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
