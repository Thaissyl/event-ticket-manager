using EventTickets.Core.Entities;
using EventTickets.Core.Enums;

namespace EventTickets.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithTicketsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Order?> GetByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
}
