using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Services;
using EventTickets.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class AdminService(
    ApplicationDbContext _context,
    UserManager<ApplicationUser> _userManager) : IAdminService
{
    public async Task<PlatformStatsResponse> GetPlatformStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _userManager.Users.CountAsync(ct);
        var totalEvents = await _context.Events.CountAsync(ct);
        var publishedEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Published)
            .CountAsync(ct);
        var totalOrders = await _context.Orders.CountAsync(ct);
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => o.TotalAmount, ct);
        var activeEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Published && e.EndDateTime > DateTime.UtcNow)
            .CountAsync(ct);

        return new PlatformStatsResponse(
            totalUsers,
            totalEvents,
            publishedEvents,
            totalOrders,
            totalRevenue,
            activeEvents
        );
    }

    public async Task<PaginatedResponse<UserListItem>> ListUsersAsync(
        string? search = null,
        UserRole? role = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(search) ||
                                    u.FullName.ToLower().Contains(search));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItem(
                u.Id,
                u.Email ?? "",
                u.FullName,
                u.Role,
                u.CreatedAt,
                u.Orders.Count
            ))
            .ToListAsync(ct);

        return new PaginatedResponse<UserListItem>(
            users,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<PaginatedResponse<AdminEventListItem>> ListAllEventsAsync(
        string? search = null,
        EventStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Events
            .Include(e => e.Organizer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(e => e.Title != null && e.Title.ToLower().Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new AdminEventListItem(
                e.Id,
                e.Title ?? "Untitled",
                e.Organizer.Email ?? "",
                e.Status,
                e.StartDateTime,
                e.TicketTiers.Sum(tt => tt.QuantitySold),
                e.TicketTiers.Sum(tt => tt.QuantitySold * tt.Price)
            ))
            .ToListAsync(ct);

        return new PaginatedResponse<AdminEventListItem>(
            events,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }

    public async Task<bool> UpdateEventStatusAsync(Guid eventId, EventStatus newStatus, string? reason = null, CancellationToken ct = default)
    {
        var @event = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (@event == null) return false;

        @event.Status = newStatus;

        // TODO: Log audit entry with reason
        // TODO: Notify organizer of status change

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PaginatedResponse<TransactionListItem>> ListTransactionsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Orders
            .Include(o => o.Tickets)
            .AsQueryable();

        var totalCount = await query.CountAsync(ct);

        var transactions = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new TransactionListItem(
                o.PaymentCode,
                o.UserId != null ? "User" : o.GuestEmail,
                o.TotalAmount,
                o.Status.ToString(),
                o.CreatedAt
            ))
            .ToListAsync(ct);

        return new PaginatedResponse<TransactionListItem>(
            transactions,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }
}
