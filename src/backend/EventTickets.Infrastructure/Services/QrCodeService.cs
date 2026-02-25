using System.Security.Cryptography;
using System.Text;
using EventTickets.Core.Services;
using Microsoft.Extensions.Options;
using QRCoder;

namespace EventTickets.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    private readonly QrCodeOptions _options;

    public QrCodeService(IOptions<QrCodeOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateQrCode(Guid ticketId)
    {
        var signature = GenerateSignature(ticketId);
        return $"ticket:{ticketId}:v1:{signature}";
    }

    public bool ValidateQrCode(string qrCode, out Guid ticketId)
    {
        ticketId = Guid.Empty;

        if (string.IsNullOrEmpty(qrCode))
            return false;

        var parts = qrCode.Split(':');
        if (parts.Length != 4 || parts[0] != "ticket" || parts[2] != "v1")
            return false;

        if (!Guid.TryParse(parts[1], out ticketId))
            return false;

        var providedSignature = parts[3];
        var expectedSignature = GenerateSignature(ticketId);

        return providedSignature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> GenerateQrImageAsync(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        return Task.FromResult(Convert.ToBase64String(qrCodeBytes));
    }

    private string GenerateSignature(Guid ticketId)
    {
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);
        var data = Encoding.UTF8.GetBytes(ticketId.ToString("N"));

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);

        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
}

public class QrCodeOptions
{
    public const string SectionName = "QrCode";
    public string SecretKey { get; set; } = "your-secret-key-change-in-production-min-32-chars";
}
