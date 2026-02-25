namespace EventTickets.Core.Services;

public interface IVietQrService
{
    /// <summary>
    /// Generates a VietQR code URL for the given order
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentCode">Unique payment code for order matching</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>VietQR image URL and payment details</returns>
    Task<VietQrResponse> GenerateQrCodeAsync(decimal amount, string paymentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique payment code for order matching
    /// </summary>
    /// <returns>Unique payment code (format: ETM-{timestamp}-{random})</returns>
    string GeneratePaymentCode();
}

public record VietQrResponse(
    string QrCodeUrl,
    string PaymentCode,
    string BankCode,
    string AccountNumber,
    string AccountName,
    decimal Amount
);
