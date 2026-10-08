namespace EmailToAll.Payload
{
    public class EmailSetupPayload
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EmailSetupResponse
    {
        public bool Configured { get; set; }
        public string? Email { get; set; }
        public string? Provider { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> SupportedProviders { get; set; } = new();
    }

    /// <summary>
    /// Bound from multipart/form-data. <c>To</c>, <c>Cc</c> and <c>Bcc</c> may be repeated fields;
    /// each value may itself hold several addresses separated by commas, semicolons or whitespace.
    /// </summary>
    public class SendEmailPayload
    {
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public List<IFormFile> Attachments { get; set; } = new();
    }

    public class SendEmailResponse
    {
        public int Id { get; set; }
        public int RecipientCount { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class EmailHistoryItem
    {
        public int Id { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public List<string> Attachments { get; set; } = new();
        public int RecipientCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }

        /// <summary>Only filled in by the detail endpoint.</summary>
        public string? Body { get; set; }
    }

    public class EmailHistoryPage
    {
        public List<EmailHistoryItem> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class DashboardResponse
    {
        public bool EmailConfigured { get; set; }
        public string? ConfiguredEmail { get; set; }
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalRecipients { get; set; }
        public int SentLast24Hours { get; set; }
        public List<EmailHistoryItem> Recent { get; set; } = new();
    }
}
