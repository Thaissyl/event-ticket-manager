using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ICartReservationRepository _cartReservationRepository;
    private readonly ITicketTierRepository _ticketTierRepository;

    public CartService(
        ICartReservationRepository cartReservationRepository,
        ITicketTierRepository ticketTierRepository)
    {
        _cartReservationRepository = cartReservationRepository;
        _ticketTierRepository = ticketTierRepository;
    }

    public async Task<CartResponse> GetCartAsync(string sessionId, CancellationToken ct = default)
    {
        var reservations = await _cartReservationRepository.GetBySessionAsync(sessionId, ct);

        var items = reservations.Select(r => new CartItemResponse(
            r.TicketTierId,
            r.TicketTier.Name,
            r.TicketTier.Price,
            r.Quantity,
            r.TicketTier.Price * r.Quantity
        ));

        var totalItems = items.Sum(i => i.Quantity);
        var totalAmount = items.Sum(i => i.Subtotal);

        return new CartResponse(items, totalItems, totalAmount);
    }

    public async Task<CartItemResponse?> AddItemAsync(string sessionId, Guid ticketTierId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0)
            return null;

        var tier = await _ticketTierRepository.GetWithTicketsAsync(ticketTierId, ct);
        if (tier == null)
            return null;

        // Check if sale is active
        var now = DateTime.UtcNow;
        if (now < tier.SaleStartDateTime || now > tier.SaleEndDateTime)
            throw new InvalidOperationException("Ticket sale is not active");

        // Calculate available quantity
        var reservedQuantity = await _cartReservationRepository.GetTotalReservedForTierAsync(ticketTierId, ct);
        var availableQuantity = tier.QuantityTotal - tier.QuantitySold - reservedQuantity;

        if (quantity > availableQuantity)
            throw new InvalidOperationException($"Only {availableQuantity} tickets available");

        // Check if item already exists in cart
        var existingReservation = await _cartReservationRepository.GetBySessionAndTierAsync(sessionId, ticketTierId, ct);

        // Use optimistic locking to reserve tickets
        var retries = 3;
        while (retries > 0)
        {
            try
            {
                // Reload tier to get latest RowVersion
                tier = await _ticketTierRepository.GetWithTicketsAsync(ticketTierId, ct);

                // Recalculate availability with latest data
                reservedQuantity = await _cartReservationRepository.GetTotalReservedForTierAsync(ticketTierId, ct);
                availableQuantity = tier.QuantityTotal - tier.QuantitySold - reservedQuantity;

                var newQuantity = existingReservation?.Quantity ?? 0;
                newQuantity += quantity;

                if (newQuantity > availableQuantity)
                    throw new InvalidOperationException($"Only {availableQuantity} tickets available");

                if (existingReservation != null)
                {
                    // Update existing reservation
                    existingReservation.Quantity = newQuantity;
                    existingReservation.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
                    await _cartReservationRepository.UpdateAsync(existingReservation, ct);
                }
                else
                {
                    // Create new reservation
                    var reservation = new CartReservation
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionId,
                        TicketTierId = ticketTierId,
                        Quantity = quantity,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _cartReservationRepository.AddAsync(reservation, ct);
                }

                // Update tier's reserved quantity
                tier.QuantityReserved = (int)reservedQuantity + (existingReservation?.Quantity ?? 0) + quantity;
                await _ticketTierRepository.UpdateAsync(tier, ct);

                await _cartReservationRepository.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateConcurrencyException)
            {
                retries--;
                if (retries == 0)
                    throw new InvalidOperationException("Could not reserve tickets due to high demand. Please try again.");
                await Task.Delay(100, ct);
            }
        }

        // Return updated cart item
        var updatedReservation = await _cartReservationRepository.GetBySessionAndTierAsync(sessionId, ticketTierId, ct);
        if (updatedReservation == null)
            return null;

        return new CartItemResponse(
            updatedReservation.TicketTierId,
            updatedReservation.TicketTier.Name,
            updatedReservation.TicketTier.Price,
            updatedReservation.Quantity,
            updatedReservation.TicketTier.Price * updatedReservation.Quantity
        );
    }

    public async Task<CartItemResponse?> UpdateItemAsync(string sessionId, Guid ticketTierId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 0)
            return null;

        var existingReservation = await _cartReservationRepository.GetBySessionAndTierAsync(sessionId, ticketTierId, ct);
        if (existingReservation == null)
            return null;

        if (quantity == 0)
        {
            await RemoveItemAsync(sessionId, ticketTierId, ct);
            return null;
        }

        var tier = await _ticketTierRepository.GetWithTicketsAsync(ticketTierId, ct);

        // Calculate available quantity
        var reservedQuantity = await _cartReservationRepository.GetTotalReservedForTierAsync(ticketTierId, ct);
        var availableQuantity = tier.QuantityTotal - tier.QuantitySold - reservedQuantity + existingReservation.Quantity;

        if (quantity > availableQuantity)
            throw new InvalidOperationException($"Only {availableQuantity} tickets available");

        // Use optimistic locking
        var retries = 3;
        while (retries > 0)
        {
            try
            {
                tier = await _ticketTierRepository.GetWithTicketsAsync(ticketTierId, ct);
                existingReservation.Quantity = quantity;
                existingReservation.ExpiresAt = DateTime.UtcNow.AddMinutes(15);

                await _cartReservationRepository.UpdateAsync(existingReservation, ct);
                await _cartReservationRepository.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateConcurrencyException)
            {
                retries--;
                if (retries == 0)
                    throw new InvalidOperationException("Could not update cart due to high demand. Please try again.");
                await Task.Delay(100, ct);
            }
        }

        return new CartItemResponse(
            ticketTierId,
            tier.Name,
            tier.Price,
            quantity,
            tier.Price * quantity
        );
    }

    public async Task<bool> RemoveItemAsync(string sessionId, Guid ticketTierId, CancellationToken ct = default)
    {
        var reservation = await _cartReservationRepository.GetBySessionAndTierAsync(sessionId, ticketTierId, ct);
        if (reservation == null)
            return false;

        await _cartReservationRepository.DeleteAsync(reservation, ct);
        await _cartReservationRepository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> ClearCartAsync(string sessionId, CancellationToken ct = default)
    {
        var reservations = await _cartReservationRepository.GetBySessionAsync(sessionId, ct);
        if (!reservations.Any())
            return true;

        foreach (var reservation in reservations)
        {
            await _cartReservationRepository.DeleteAsync(reservation, ct);
        }

        await _cartReservationRepository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Dictionary<Guid, int>> GetCartItemsForOrderAsync(string sessionId, CancellationToken ct = default)
    {
        var reservations = await _cartReservationRepository.GetBySessionAsync(sessionId, ct);
        return reservations.ToDictionary(r => r.TicketTierId, r => r.Quantity);
    }

    public async Task<bool> ReleaseCartReservationsAsync(string sessionId, CancellationToken ct = default)
    {
        return await ClearCartAsync(sessionId, ct);
    }
}
