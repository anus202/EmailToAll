using EmailToAll.Data.Interface;
using EmailToAll.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailToAll.Data.Repository
{
    public class OtpVerificationRepository : IOtpVerificationRepository
    {
        private readonly ApplicationDbContext _context;

        public OtpVerificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(OtpVerification otpVerification)
        {
            try
            {
                await _context.OtpVerifications.AddAsync(otpVerification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<OtpVerification>> GetAllAsync()
        {
            try
            {
                return  await _context.OtpVerifications.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<OtpVerification> GetByIdAsync(int id)
        {
            try
            {
                return await _context.OtpVerifications.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateAsync(OtpVerification otpVerification)
        {
            try
            {
                _context.OtpVerifications.Update(otpVerification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
