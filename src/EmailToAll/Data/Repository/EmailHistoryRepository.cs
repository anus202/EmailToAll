using EmailToAll.Data.Interface;
using EmailToAll.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailToAll.Data.Repository
{
    public class EmailHistoryRepository : IEmailHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<EmailHistory> ForUser(int userId)
        {
            return _context.EmailHistories
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDelete);
        }

        public async Task CreateAsync(EmailHistory emailHistory)
        {
            await _context.EmailHistories.AddAsync(emailHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailHistory?> GetByIdAsync(int userId, int id)
        {
            return await ForUser(userId).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(List<EmailHistory> Items, int Total)> GetPagedAsync(int userId, string? search, int page, int pageSize)
        {
            var query = ForUser(userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.Subject.Contains(term) ||
                    x.ToEmails.Contains(term) ||
                    x.CcEmails.Contains(term) ||
                    x.BccEmails.Contains(term));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.SentAt)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<EmailHistory>> GetRecentAsync(int userId, int count)
        {
            return await ForUser(userId)
                .OrderByDescending(x => x.SentAt)
                .ThenByDescending(x => x.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<EmailHistoryStats> GetStatsAsync(int userId)
        {
            var since = DateTime.UtcNow.AddHours(-24);
            var sent = ForUser(userId).Where(x => x.Status == EmailStatus.Sent);

            return new EmailHistoryStats
            {
                TotalSent = await sent.CountAsync(),
                TotalFailed = await ForUser(userId).CountAsync(x => x.Status == EmailStatus.Failed),
                TotalRecipients = await sent.SumAsync(x => (int?)x.RecipientCount) ?? 0,
                SentLast24Hours = await sent.CountAsync(x => x.SentAt >= since)
            };
        }
    }
}
