namespace EventTickets.Core.Services;

public interface IExportService
{
    Task<byte[]> ExportAttendeesAsync(Guid eventId, CancellationToken ct = default);
    Task<byte[]> ExportSalesReportAsync(Guid eventId, CancellationToken ct = default);
}

public record AttendeeExportItem(
    string OrderCode,
    string AttendeeName,
    string AttendeeEmail,
    string TierName,
    string Status,
    DateTime? CheckedInAt
);

public record SalesReportExportItem(
    string OrderCode,
    DateTime OrderDate,
    string CustomerEmail,
    int TicketCount,
    decimal TotalAmount,
    string Status
);
