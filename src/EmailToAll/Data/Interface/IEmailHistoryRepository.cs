using EmailToAll.Models;

namespace EmailToAll.Data.Interface
{
    public class EmailHistoryStats
    {
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalRecipients { get; set; }
        public int SentLast24Hours { get; set; }
    }

    public interface IEmailHistoryRepository
    {
        Task CreateAsync(EmailHistory emailHistory);
        Task<EmailHistory?> GetByIdAsync(int userId, int id);
        Task<(List<EmailHistory> Items, int Total)> GetPagedAsync(int userId, string? search, int page, int pageSize);
        Task<List<EmailHistory>> GetRecentAsync(int userId, int count);
        Task<EmailHistoryStats> GetStatsAsync(int userId);
    }
}
