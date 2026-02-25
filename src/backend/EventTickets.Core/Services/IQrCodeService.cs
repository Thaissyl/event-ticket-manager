namespace EventTickets.Core.Services;

public interface IQrCodeService
{
    /// <summary>
    /// Generates a QR code string for a ticket with HMAC signature
    /// Format: ticket:{uuid}:v1:{signature}
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <returns>QR code string with signature</returns>
    string GenerateQrCode(Guid ticketId);

    /// <summary>
    /// Validates a QR code string and extracts ticket ID
    /// </summary>
    /// <param name="qrCode">QR code string to validate</param>
    /// <param name="ticketId">Extracted ticket ID if valid</param>
    /// <returns>True if signature is valid</returns>
    bool ValidateQrCode(string qrCode, out Guid ticketId);

    /// <summary>
    /// Generates QR code image as base64 string for display
    /// </summary>
    /// <param name="content">Content to encode in QR</param>
    /// <returns>Base64 encoded PNG image</returns>
    Task<string> GenerateQrImageAsync(string content);
}
