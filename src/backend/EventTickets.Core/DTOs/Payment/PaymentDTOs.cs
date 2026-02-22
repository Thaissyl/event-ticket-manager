namespace EventTickets.Core.DTOs;

public record SePayWebhookRequest(
    string Gateway,
    string TransactionCode,
    decimal Amount,
    string? Content,
    DateTime TransactionDate,
    string? CustomerPhone
);

public record WebhookResponse(
    bool Success,
    string Message
);
