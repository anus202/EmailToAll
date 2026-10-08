using System.ComponentModel.DataAnnotations;

namespace EmailToAll.Models
{
    public static class EmailStatus
    {
        public const string Sent = "Sent";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// One row per send attempt. Recipient and attachment lists are stored as JSON arrays.
    /// </summary>
    public class EmailHistory : CommonField
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string ToEmails { get; set; } = "[]";
        public string CcEmails { get; set; } = "[]";
        public string BccEmails { get; set; } = "[]";
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public string AttachmentNames { get; set; } = "[]";
        public int RecipientCount { get; set; }
        public string Status { get; set; } = EmailStatus.Sent;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
