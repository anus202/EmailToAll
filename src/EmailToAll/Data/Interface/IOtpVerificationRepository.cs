using EmailToAll.Models;

namespace EmailToAll.Data.Interface
{
    public interface IOtpVerificationRepository
    {
        Task <IEnumerable<OtpVerification>> GetAllAsync();
        Task <OtpVerification> GetByIdAsync(int id);
        Task CreateAsync(OtpVerification otpVerification);
        Task UpdateAsync(OtpVerification otpVerification);

        /// <summary>Returns the unused, unexpired OTP matching the email and code, or null.</summary>
        Task<OtpVerification?> GetValidAsync(string email, string code);
    }
}
