using EventTickets.Core.Services;
using Microsoft.Extensions.Options;

namespace EventTickets.Infrastructure.Services;

public class VietQrService : IVietQrService
{
    private readonly SePayOptions _options;

    public VietQrService(IOptions<SePayOptions> options)
    {
        _options = options.Value;
    }

    public Task<VietQrResponse> GenerateQrCodeAsync(decimal amount, string paymentCode, CancellationToken cancellationToken = default)
    {
        var qrCodeUrl = $"https://img.vietqr.io/image/{_options.BankCode}-{_options.AccountNumber}-compact.png" +
                        $"?amount={amount}" +
                        $"&addInfo={paymentCode}" +
                        $"&accountName={Uri.EscapeDataString(_options.AccountName)}";

        var response = new VietQrResponse(
            qrCodeUrl,
            paymentCode,
            _options.BankCode,
            _options.AccountNumber,
            _options.AccountName,
            amount
        );

        return Task.FromResult(response);
    }

    public string GeneratePaymentCode()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var suffix = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return $"ETM-{timestamp}-{suffix}";
    }
}

public class SePayOptions
{
    public const string SectionName = "SePay";
    public string ApiToken { get; set; } = string.Empty;
    public string WebhookApiKey { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://my.sepay.vn/userapi/";
}
