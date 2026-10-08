using EmailToAll.Data;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class EmailHistoryService
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;
        private const int RecentCount = 5;

        private readonly IUnitOfWork _unitOfWork;

        public EmailHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmailHistoryPage> ListAsync(int userId, string? search, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var (items, total) = await _unitOfWork.EmailHistory.GetPagedAsync(userId, search, page, pageSize);

            return new EmailHistoryPage
            {
                Items = items.Select(x => ToItem(x, includeBody: false)).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<EmailHistoryItem> GetAsync(int userId, int id)
        {
            var history = await _unitOfWork.EmailHistory.GetByIdAsync(userId, id);
            if (history == null)
            {
                throw new Exception("Email not found.");
            }

            return ToItem(history, includeBody: true);
        }

        public async Task<DashboardResponse> DashboardAsync(int userId)
        {
            var setup = await _unitOfWork.EmailSetup.GetByUserIdAsync(userId);
            var stats = await _unitOfWork.EmailHistory.GetStatsAsync(userId);
            var recent = await _unitOfWork.EmailHistory.GetRecentAsync(userId, RecentCount);

            return new DashboardResponse
            {
                EmailConfigured = setup != null,
                ConfiguredEmail = setup?.Email,
                TotalSent = stats.TotalSent,
                TotalFailed = stats.TotalFailed,
                TotalRecipients = stats.TotalRecipients,
                SentLast24Hours = stats.SentLast24Hours,
                Recent = recent.Select(x => ToItem(x, includeBody: false)).ToList()
            };
        }

        private static EmailHistoryItem ToItem(EmailHistory x, bool includeBody)
        {
            return new EmailHistoryItem
            {
                Id = x.Id,
                FromEmail = x.FromEmail,
                To = EmailJson.Deserialize(x.ToEmails),
                Cc = EmailJson.Deserialize(x.CcEmails),
                Bcc = EmailJson.Deserialize(x.BccEmails),
                Subject = x.Subject,
                IsHtml = x.IsHtml,
                Attachments = EmailJson.Deserialize(x.AttachmentNames),
                RecipientCount = x.RecipientCount,
                Status = x.Status,
                ErrorMessage = x.ErrorMessage,
                // SentAt is stored in UTC; mark it so JSON consumers parse it as UTC.
                SentAt = DateTime.SpecifyKind(x.SentAt, DateTimeKind.Utc),
                Body = includeBody ? x.Body : null
            };
        }
    }
}
