using EmailToAll.Data.Interface;
using EmailToAll.Models;

namespace EmailToAll.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ISignupRepository Signup { get; }
        public IOtpVerificationRepository Otp { get; }
        public IEmailSetupRepository EmailSetup { get; }
        public IEmailHistoryRepository EmailHistory { get; }

        public UnitOfWork(
            ApplicationDbContext dbContext,
            ISignupRepository signupRepo,
            IOtpVerificationRepository otpRepo,
            IEmailSetupRepository emailSetupRepo,
            IEmailHistoryRepository emailHistoryRepo)
        {
            _context = dbContext;
            Signup = signupRepo;
            Otp = otpRepo;
            EmailSetup = emailSetupRepo;
            EmailHistory = emailHistoryRepo;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
