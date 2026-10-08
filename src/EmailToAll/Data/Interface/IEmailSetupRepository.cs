using EmailToAll.Models;

namespace EmailToAll.Data.Interface
{
    public interface IEmailSetupRepository
    {
        Task<EmailSetup?> GetByUserIdAsync(int userId);
        Task CreateAsync(EmailSetup emailSetup);
        Task UpdateAsync(EmailSetup emailSetup);
    }
}
