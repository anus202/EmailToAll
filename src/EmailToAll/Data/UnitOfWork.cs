using EmailToAll.Data.Interface;
using EmailToAll.Models;

namespace EmailToAll.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
       public ISignupRepository Signup { get; }
        public IOtpVerificationRepository Otp { get; }
        public UnitOfWork(ApplicationDbContext dbContext , ISignupRepository signupRepo, IOtpVerificationRepository otpRepo)
        {
            _context = dbContext;
            Signup = signupRepo;
            Otp = otpRepo;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
