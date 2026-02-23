using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;

namespace EventTickets.Core.Services;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(string sessionId, CancellationToken ct = default);
    Task<CartItemResponse?> AddItemAsync(string sessionId, Guid ticketTierId, int quantity, CancellationToken ct = default);
    Task<CartItemResponse?> UpdateItemAsync(string sessionId, Guid ticketTierId, int quantity, CancellationToken ct = default);
    Task<bool> RemoveItemAsync(string sessionId, Guid ticketTierId, CancellationToken ct = default);
    Task<bool> ClearCartAsync(string sessionId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetCartItemsForOrderAsync(string sessionId, CancellationToken ct = default);
    Task<bool> ReleaseCartReservationsAsync(string sessionId, CancellationToken ct = default);
}
