using EmailToAll.Data;
using EmailToAll.Models;
using EmailToAll.Payload;

namespace EmailToAll.Service
{
    public class EmailSetupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SmtpProviderService _smtpProviders;
        private readonly EncryptionService _encryptionService;

        public EmailSetupService(IUnitOfWork unitOfWork, SmtpProviderService smtpProviders, EncryptionService encryptionService)
        {
            _unitOfWork = unitOfWork;
            _smtpProviders = smtpProviders;
            _encryptionService = encryptionService;
        }

        public async Task<EmailSetupResponse> GetAsync(int userId)
        {
            var setup = await _unitOfWork.EmailSetup.GetByUserIdAsync(userId);
            return ToResponse(setup);
        }

        /// <summary>
        /// Verifies the credentials against the provider's SMTP server and only then stores them.
        /// </summary>
        public async Task<EmailSetupResponse> SaveAsync(int userId, EmailSetupPayload payload)
        {
            if (payload == null)
            {
                throw new Exception("Input Field Null");
            }

            if (!payload.Email.TryParseEmail(out var email))
            {
                throw new Exception("Please enter a valid email address.");
            }

            email = email.NormalizeEmail();

            if (string.IsNullOrEmpty(payload.Password))
            {
                throw new Exception("Password is required.");
            }

            if (_smtpProviders.Resolve(email) == null)
            {
                throw new Exception(
                    $"This email provider is not supported yet. Supported providers: {string.Join(", ", _smtpProviders.ProviderNames)}.");
            }

            using (var smtp = await _smtpProviders.ConnectAsync(email, payload.Password))
            {
                await smtp.DisconnectAsync(true);
            }

            var now = DateTime.UtcNow;
            var setup = await _unitOfWork.EmailSetup.GetByUserIdAsync(userId);

            if (setup == null)
            {
                setup = new EmailSetup
                {
                    UserId = userId,
                    Email = email,
                    Password = _encryptionService.Encrypt(payload.Password),
                    CreateBy = userId,
                    CreateDate = now,
                    UpdateBy = userId,
                    UpdateDate = now,
                    IsActive = true
                };
                await _unitOfWork.EmailSetup.CreateAsync(setup);
            }
            else
            {
                setup.Email = email;
                setup.Password = _encryptionService.Encrypt(payload.Password);
                setup.UpdateBy = userId;
                setup.UpdateDate = now;
                await _unitOfWork.EmailSetup.UpdateAsync(setup);
            }

            return ToResponse(setup);
        }

        private EmailSetupResponse ToResponse(EmailSetup? setup)
        {
            return new EmailSetupResponse
            {
                Configured = setup != null,
                Email = setup?.Email,
                Provider = setup == null ? null : _smtpProviders.Resolve(setup.Email)?.Name,
                UpdatedAt = setup == null ? null : DateTime.SpecifyKind(setup.UpdateDate, DateTimeKind.Utc),
                SupportedProviders = _smtpProviders.ProviderNames
            };
        }
    }
}
