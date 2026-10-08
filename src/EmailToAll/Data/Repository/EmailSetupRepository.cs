using EmailToAll.Data.Interface;
using EmailToAll.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailToAll.Data.Repository
{
    public class EmailSetupRepository : IEmailSetupRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailSetupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailSetup?> GetByUserIdAsync(int userId)
        {
            return await _context.EmailSetups
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDelete);
        }

        public async Task CreateAsync(EmailSetup emailSetup)
        {
            await _context.EmailSetups.AddAsync(emailSetup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmailSetup emailSetup)
        {
            _context.EmailSetups.Update(emailSetup);
            await _context.SaveChangesAsync();
        }
    }
}
