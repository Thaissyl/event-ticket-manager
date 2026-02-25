using EventTickets.Core.DTOs;
using EventTickets.Core.Enums;

namespace EventTickets.Core.Services;

public interface IAdminService
{
    Task<PlatformStatsResponse> GetPlatformStatsAsync(CancellationToken ct = default);
    Task<PaginatedResponse<UserListItem>> ListUsersAsync(string? search = null, UserRole? role = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole, CancellationToken ct = default);
    Task<PaginatedResponse<AdminEventListItem>> ListAllEventsAsync(string? search = null, EventStatus? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<bool> UpdateEventStatusAsync(Guid eventId, EventStatus newStatus, string? reason = null, CancellationToken ct = default);
    Task<PaginatedResponse<TransactionListItem>> ListTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record UserListItem(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    DateTime CreatedAt,
    int OrderCount
);

public record AdminEventListItem(
    Guid Id,
    string Title,
    string OrganizerEmail,
    EventStatus Status,
    DateTime StartDateTime,
    int TicketsSold,
    decimal Revenue
);

public record TransactionListItem(
    string OrderCode,
    string CustomerEmail,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);
