using EventTickets.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventTickets.Infrastructure.BackgroundServices;

public class CartCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CartCleanupService> _logger;

    public CartCleanupService(
        IServiceProvider serviceProvider,
        ILogger<CartCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart Cleanup Service is starting");

        // Run every minute
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupExpiredReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired cart reservations");
            }
        }

        _logger.LogInformation("Cart Cleanup Service is stopping");
    }

    private async Task CleanupExpiredReservationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var cartReservationRepository = scope.ServiceProvider.GetRequiredService<ICartReservationRepository>();
        var ticketTierRepository = scope.ServiceProvider.GetRequiredService<ITicketTierRepository>();

        var expiredReservations = await cartReservationRepository.GetExpiredAsync(ct);
        var count = 0;

        foreach (var reservation in expiredReservations)
        {
            // Update ticket tier reserved quantity
            var tier = await ticketTierRepository.GetWithTicketsAsync(reservation.TicketTierId, ct);
            if (tier != null && tier.QuantityReserved > 0)
            {
                tier.QuantityReserved = Math.Max(0, tier.QuantityReserved - reservation.Quantity);
                await ticketTierRepository.UpdateAsync(tier, ct);
            }

            await cartReservationRepository.DeleteAsync(reservation, ct);
            count++;
        }

        await cartReservationRepository.SaveChangesAsync(ct);

        if (count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired cart reservations", count);
        }
    }
}
