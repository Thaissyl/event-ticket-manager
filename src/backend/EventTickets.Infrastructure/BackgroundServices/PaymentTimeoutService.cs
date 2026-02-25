using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventTickets.Infrastructure.BackgroundServices;

public class PaymentTimeoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentTimeoutService> _logger;
    private const int PaymentTimeoutMinutes = 30;

    public PaymentTimeoutService(
        IServiceProvider serviceProvider,
        ILogger<PaymentTimeoutService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Timeout Service is starting");

        // Run every minute
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CancelExpiredPaymentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling expired payments");
            }
        }

        _logger.LogInformation("Payment Timeout Service is stopping");
    }

    private async Task CancelExpiredPaymentsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var cancelledCount = await paymentService.CancelExpiredPaymentsAsync(PaymentTimeoutMinutes, ct);

        if (cancelledCount > 0)
        {
            _logger.LogInformation("Cancelled {Count} expired payments", cancelledCount);
        }
    }
}
