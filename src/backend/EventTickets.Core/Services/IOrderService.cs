using EventTickets.Core.DTOs;

namespace EventTickets.Core.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderFromCartAsync(string sessionId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderResponse?> GetOrderAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken ct = default);
    Task<OrderResponse?> GetOrderByPaymentCodeAsync(string paymentCode, CancellationToken ct = default);
}
