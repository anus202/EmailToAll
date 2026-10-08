using EmailToAll.Data.Interface;
using EmailToAll.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailToAll.Data.Repository
{
    public class SignupRepository : ISignupRepository
    {
        private readonly ApplicationDbContext _context;
        public SignupRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Signup signup)
        {
            try
            {
                var res = await _context.Signups.AddAsync(signup);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var res = await _context.Signups.FindAsync(id);
                _context.Remove(res);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Signup>> GetAlllAsync()
        {
            try
            {
               return await _context.Signups.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Signup?> GetByEmailAsync(string email)
        {
            return await _context.Signups.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<Signup> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Signups.FirstOrDefaultAsync(s =>s.Id == id).ConfigureAwait(false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateAsync(Signup signup)
        {
            try
            {
                _context.Signups.Update(signup);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
