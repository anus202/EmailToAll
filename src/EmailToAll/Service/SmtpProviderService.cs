using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Sockets;

namespace EmailToAll.Service
{
    public class SmtpProviderOptions
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Domains { get; set; } = new();
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
    }

    /// <summary>
    /// Maps a user's email domain to an SMTP server. The list comes from the <c>SmtpProviders</c>
    /// configuration section, so users can never point the server at an arbitrary host.
    /// </summary>
    public class SmtpProviderService
    {
        private readonly List<SmtpProviderOptions> _providers;
        private readonly ILogger<SmtpProviderService> _logger;

        public SmtpProviderService(IConfiguration configuration, ILogger<SmtpProviderService> logger)
        {
            _providers = configuration.GetSection("SmtpProviders").Get<List<SmtpProviderOptions>>() ?? new();
            _logger = logger;
        }

        public List<string> ProviderNames => _providers.Select(p => p.Name).ToList();

        public SmtpProviderOptions? Resolve(string email)
        {
            var at = email.LastIndexOf('@');
            if (at < 0)
            {
                return null;
            }

            var domain = email[(at + 1)..];
            return _providers.FirstOrDefault(p => p.Domains.Contains(domain, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>Opens an authenticated SMTP connection. The caller disposes the client.</summary>
        public async Task<SmtpClient> ConnectAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var provider = Resolve(email) ?? throw new InvalidOperationException(
                $"This email provider is not supported. Supported providers: {string.Join(", ", ProviderNames)}.");

            var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    provider.Host,
                    provider.Port,
                    provider.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                    cancellationToken);

                await smtp.AuthenticateAsync(email, password, cancellationToken);
                return smtp;
            }
            catch (Exception ex)
            {
                smtp.Dispose();
                _logger.LogWarning(ex, "SMTP connection to {Host} failed for {Email}.", provider.Host, email);

                if (ex is AuthenticationException)
                {
                    throw new InvalidOperationException(
                        $"{provider.Name} rejected the email or password. Use an app password instead of your normal password if two-step verification is on.");
                }

                if (ex is SmtpCommandException or SmtpProtocolException or IOException or SocketException)
                {
                    throw new InvalidOperationException(
                        $"Could not connect to the {provider.Name} mail server: {ex.Message}");
                }

                throw;
            }
        }
    }
}
