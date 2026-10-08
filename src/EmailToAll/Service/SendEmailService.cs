using EmailToAll.Data;
using EmailToAll.Models;
using EmailToAll.Payload;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EmailToAll.Service
{
    /// <summary>
    /// Sends one message from the signed-in user's own mailbox to many recipients
    /// and records every attempt in the history table.
    /// </summary>
    public class SendEmailService
    {
        public const int MaxRecipients = 500;
        public const int MaxAttachments = 10;
        public const long MaxAttachmentBytes = 25 * 1024 * 1024;
        private const int MaxSubjectLength = 500;

        private static readonly Regex AddressSeparator = new(@"[,;\s]+", RegexOptions.Compiled);

        private readonly IUnitOfWork _unitOfWork;
        private readonly SmtpProviderService _smtpProviders;
        private readonly EncryptionService _encryptionService;
        private readonly ILogger<SendEmailService> _logger;

        public SendEmailService(
            IUnitOfWork unitOfWork,
            SmtpProviderService smtpProviders,
            EncryptionService encryptionService,
            ILogger<SendEmailService> logger)
        {
            _unitOfWork = unitOfWork;
            _smtpProviders = smtpProviders;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<SendEmailResponse> SendAsync(int userId, SendEmailPayload payload)
        {
            if (payload == null)
            {
                throw new Exception("Input Field Null");
            }

            var setup = await _unitOfWork.EmailSetup.GetByUserIdAsync(userId);
            if (setup == null)
            {
                throw new Exception("Email setup is missing. Add your email and password in Email Setup first.");
            }

            // Validation errors are returned to the user and are not recorded in the history.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var invalid = new List<string>();
            var to = ParseRecipients(payload.To, seen, invalid);
            var cc = ParseRecipients(payload.Cc, seen, invalid);
            var bcc = ParseRecipients(payload.Bcc, seen, invalid);

            if (invalid.Count > 0)
            {
                throw new Exception($"Invalid email address: {string.Join(", ", invalid.Take(5))}{(invalid.Count > 5 ? "..." : string.Empty)}");
            }

            var recipientCount = to.Count + cc.Count + bcc.Count;
            if (recipientCount == 0)
            {
                throw new Exception("Add at least one recipient.");
            }

            if (recipientCount > MaxRecipients)
            {
                throw new Exception($"A single email can have at most {MaxRecipients} recipients.");
            }

            var subject = (payload.Subject ?? string.Empty).Trim();
            if (subject.Length == 0)
            {
                throw new Exception("Subject is required.");
            }

            if (subject.Length > MaxSubjectLength)
            {
                throw new Exception($"Subject must be {MaxSubjectLength} characters or fewer.");
            }

            var files = payload.Attachments ?? new List<IFormFile>();
            if (files.Count > MaxAttachments)
            {
                throw new Exception($"You can attach at most {MaxAttachments} files.");
            }

            if (files.Sum(f => f.Length) > MaxAttachmentBytes)
            {
                throw new Exception($"Attachments are limited to {MaxAttachmentBytes / (1024 * 1024)} MB in total.");
            }

            var attachmentNames = files.Select(f => Path.GetFileName(f.FileName)).ToList();
            var now = DateTime.UtcNow;

            var history = new EmailHistory
            {
                UserId = userId,
                FromEmail = setup.Email,
                ToEmails = EmailJson.Serialize(to),
                CcEmails = EmailJson.Serialize(cc),
                BccEmails = EmailJson.Serialize(bcc),
                Subject = subject,
                Body = payload.Body ?? string.Empty,
                IsHtml = payload.IsHtml,
                AttachmentNames = EmailJson.Serialize(attachmentNames),
                RecipientCount = recipientCount,
                Status = EmailStatus.Sent,
                SentAt = now,
                CreateBy = userId,
                CreateDate = now,
                UpdateBy = userId,
                UpdateDate = now,
                IsActive = true
            };

            var streams = new List<Stream>();
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress((string?)null, setup.Email));
                message.To.AddRange(to.Select(a => MailboxAddress.Parse(a)));
                message.Cc.AddRange(cc.Select(a => MailboxAddress.Parse(a)));
                message.Bcc.AddRange(bcc.Select(a => MailboxAddress.Parse(a)));
                message.Subject = subject;

                var builder = new BodyBuilder();
                if (payload.IsHtml)
                {
                    builder.HtmlBody = payload.Body ?? string.Empty;
                }
                else
                {
                    builder.TextBody = payload.Body ?? string.Empty;
                }

                foreach (var file in files)
                {
                    var stream = file.OpenReadStream();
                    streams.Add(stream);

                    var name = Path.GetFileName(file.FileName);
                    if (ContentType.TryParse(file.ContentType, out var contentType))
                    {
                        builder.Attachments.Add(name, stream, contentType);
                    }
                    else
                    {
                        builder.Attachments.Add(name, stream);
                    }
                }

                message.Body = builder.ToMessageBody();

                string password;
                try
                {
                    password = _encryptionService.Decrypt(setup.Password);
                }
                catch (Exception ex) when (ex is CryptographicException or FormatException)
                {
                    throw new InvalidOperationException("Your saved email password can no longer be read. Please save Email Setup again.");
                }

                using var smtp = await _smtpProviders.ConnectAsync(setup.Email, password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending email for user {UserId} failed.", userId);

                var reason = ex switch
                {
                    InvalidOperationException => ex.Message,
                    SmtpCommandException => $"The mail server rejected the message: {ex.Message}",
                    _ => $"Failed to send the email: {ex.Message}"
                };

                history.Status = EmailStatus.Failed;
                history.ErrorMessage = reason.Length > 1000 ? reason[..1000] : reason;
                await _unitOfWork.EmailHistory.CreateAsync(history);

                throw new Exception(reason);
            }
            finally
            {
                foreach (var stream in streams)
                {
                    stream.Dispose();
                }
            }

            await _unitOfWork.EmailHistory.CreateAsync(history);

            return new SendEmailResponse
            {
                Id = history.Id,
                RecipientCount = recipientCount,
                SentAt = history.SentAt
            };
        }

        /// <summary>
        /// Splits every value on commas, semicolons and whitespace, validates each address and
        /// drops duplicates (also across To, Cc and Bcc).
        /// </summary>
        private static List<string> ParseRecipients(IEnumerable<string>? values, HashSet<string> seen, List<string> invalid)
        {
            var result = new List<string>();
            if (values == null)
            {
                return result;
            }

            foreach (var value in values)
            {
                foreach (var token in AddressSeparator.Split(value ?? string.Empty))
                {
                    var candidate = token.Trim('<', '>', '"', '\'');
                    if (candidate.Length == 0)
                    {
                        continue;
                    }

                    if (!candidate.TryParseEmail(out var address))
                    {
                        invalid.Add(candidate);
                        continue;
                    }

                    if (seen.Add(address))
                    {
                        result.Add(address);
                    }
                }
            }

            return result;
        }
    }
}
