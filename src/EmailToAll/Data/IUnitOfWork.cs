using EmailToAll.Data.Interface;
using EmailToAll.Data.Repository;

namespace EmailToAll.Data
{
    public interface IUnitOfWork
    {
        ISignupRepository Signup {  get; }
        IOtpVerificationRepository Otp { get; }
        IEmailSetupRepository EmailSetup { get; }
        IEmailHistoryRepository EmailHistory { get; }

        Task SaveAsync();
    }
}
