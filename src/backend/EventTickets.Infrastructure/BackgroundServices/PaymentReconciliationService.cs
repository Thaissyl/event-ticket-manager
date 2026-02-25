using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventTickets.Infrastructure.BackgroundServices;

public class PaymentReconciliationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentReconciliationService> _logger;

    public PaymentReconciliationService(
        IServiceProvider serviceProvider,
        ILogger<PaymentReconciliationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Reconciliation Service is starting");

        // Run every 15 minutes
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReconcilePaymentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling payments");
            }
        }

        _logger.LogInformation("Payment Reconciliation Service is stopping");
    }

    private async Task ReconcilePaymentsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var reconciledCount = await paymentService.ReconcilePaymentsAsync(ct);

        if (reconciledCount > 0)
        {
            _logger.LogInformation("Reconciled {Count} payments", reconciledCount);
        }
    }
}
