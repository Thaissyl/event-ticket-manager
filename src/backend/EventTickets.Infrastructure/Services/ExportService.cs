using System.Text;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Services;
using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Infrastructure.Services;

public class ExportService(ApplicationDbContext _context) : IExportService
{
    public async Task<byte[]> ExportAttendeesAsync(Guid eventId, CancellationToken ct = default)
    {
        var tickets = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.TicketTier)
            .Include(t => t.Order)
            .Where(t => t.TicketTier.EventId == eventId)
            .Where(t => t.Order != null && t.Order.Status == OrderStatus.Paid)
            .OrderBy(t => t.TicketTier.Name)
            .ThenBy(t => t.AttendeeName)
            .Select(t => new AttendeeExportItem(
                t.Order!.PaymentCode,
                t.AttendeeName,
                MaskEmail(t.AttendeeEmail),
                t.TicketTier.Name ?? "Unnamed",
                t.Status.ToString(),
                t.CheckedInAt
            ))
            .ToListAsync(ct);

        return GenerateCsv(tickets, [
            "Order Code", "Attendee Name", "Email", "Ticket Tier", "Status", "Checked In At"
        ], [
            x => x.OrderCode,
            x => x.AttendeeName,
            x => x.AttendeeEmail,
            x => x.TierName,
            x => x.Status,
            x => x.CheckedInAt?.ToString("yyyy-MM-dd HH:mm") ?? ""
        ]);
    }

    public async Task<byte[]> ExportSalesReportAsync(Guid eventId, CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Tickets.Any(t => t.TicketTier.EventId == eventId))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new SalesReportExportItem(
                o.PaymentCode,
                o.CreatedAt,
                MaskEmail(o.UserId != null ? "***user***" : o.GuestEmail),
                o.Tickets.Count,
                o.TotalAmount,
                o.Status.ToString()
            ))
            .ToListAsync(ct);

        return GenerateCsv(orders, [
            "Order Code", "Order Date", "Customer Email", "Ticket Count", "Total Amount", "Status"
        ], [
            x => x.OrderCode,
            x => x.OrderDate.ToString("yyyy-MM-dd HH:mm"),
            x => x.CustomerEmail,
            x => x.TicketCount.ToString(),
            x => x.TotalAmount.ToString("F2"),
            x => x.Status
        ]);
    }

    private static byte[] GenerateCsv<T>(
        IEnumerable<T> data,
        IEnumerable<string> headers,
        IEnumerable<Func<T, string>> selectors)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",", headers));

        // Rows
        foreach (var item in data)
        {
            var values = selectors.Select(s => EscapeCsvField(s(item)));
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsvField(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || email.Length < 5)
            return "***@***";

        var parts = email.Split('@');
        if (parts.Length != 2)
            return "***@***";

        var username = parts[0];
        var domain = parts[1];

        var maskedUsername = username.Length > 2
            ? $"{username[..2]}{new string('*', username.Length - 2)}"
            : new string('*', username.Length);

        var domainParts = domain.Split('.');
        var maskedDomain = domainParts.Length > 1
            ? $"{new string('*', domainParts[0].Length)}.{domainParts[1]}"
            : new string('*', domain.Length);

        return $"{maskedUsername}@{maskedDomain}";
    }
}
