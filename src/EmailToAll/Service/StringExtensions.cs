using MimeKit;
using System.Security.Claims;
using System.Text.Json;

namespace EmailToAll.Service
{
    public static class StringExtensions
    {
        /// <summary>Trims and lower-cases an email address so lookups are case-insensitive.</summary>
        public static string NormalizeEmail(this string? value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Accepts only bare addresses such as <c>user@example.com</c> (no display names).
        /// </summary>
        public static bool TryParseEmail(this string? value, out string email)
        {
            email = string.Empty;

            var candidate = value?.Trim();
            if (string.IsNullOrEmpty(candidate) || candidate.Length > 256)
            {
                return false;
            }

            if (!MailboxAddress.TryParse(candidate, out var mailbox) ||
                !string.Equals(mailbox.Address, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var at = mailbox.Address.LastIndexOf('@');
            if (at < 1 || !mailbox.Address[(at + 1)..].Contains('.'))
            {
                return false;
            }

            email = mailbox.Address;
            return true;
        }
    }

    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Reads the signed-in user's id from the JWT.</summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var id))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }

            return id;
        }
    }

    /// <summary>Recipient and attachment lists are persisted as JSON arrays.</summary>
    public static class EmailJson
    {
        public static string Serialize(IEnumerable<string> values) => JsonSerializer.Serialize(values);

        public static List<string> Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
    }
}
