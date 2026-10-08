using EmailToAll.Data.Interface;
using EmailToAll.Data.Repository;

namespace EmailToAll.Data
{
    public interface IUnitOfWork
    {
        ISignupRepository Signup {  get; }
        IOtpVerificationRepository Otp { get; }



        Task SaveAsync();
    }
}
