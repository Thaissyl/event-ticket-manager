namespace EventTickets.Core.Entities;

public class SePayWebhook
{
    public Guid Id { get; set; }
    public long SePayTransactionId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public bool Processed { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime CreatedAt { get; set; }
}
