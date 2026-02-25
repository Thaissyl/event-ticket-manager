namespace EventTickets.Core.Services;

public interface ITicketPdfService
{
    /// <summary>
    /// Generates a PDF ticket for a given ticket ID
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateTicketPdfAsync(Guid ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates QR code image as byte array
    /// </summary>
    /// <param name="qrCode">QR code string</param>
    /// <returns>PNG image bytes</returns>
    Task<byte[]> GenerateQrImageAsync(string qrCode);
}
