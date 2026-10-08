using EmailToAll.Data;

namespace EmailToAll.Service
{
    public class OtpExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OtpExpiryService> _logger;

        public OtpExpiryService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OtpExpiryService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OTP Expiry Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var unitOfWork = scope.ServiceProvider
                        .GetRequiredService<IUnitOfWork>();

                    var otps = await unitOfWork.Otp.GetAllAsync();

                    if (otps == null || !otps.Any(x => !x.IsUsed))
                    {
                        _logger.LogDebug(
                            "No unused OTPs found.");
                    }
                    else
                    {
                        var expiredOtps = otps
                            .Where(x =>
                                !x.IsUsed &&
                                x.ExpiryTime <= DateTime.UtcNow)
                            .ToList();

                        foreach (var otp in expiredOtps)
                        {
                            otp.IsUsed = true;

                            await unitOfWork.Otp.UpdateAsync(otp);

                            _logger.LogInformation(
                                "OTP for email {Email} expired and marked as used.",
                                otp.Email);
                        }

                        if (expiredOtps.Any())
                        {
                            _logger.LogInformation(
                                "{Count} expired OTP(s) marked as used.",
                                expiredOtps.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while processing expired OTPs.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
            _logger.LogInformation("OTP Expiry Service stopped.");
        }
    }
}