namespace EventTickets.Core.DTOs;

public record CheckinRequest(
    string QrCode
);

public record CheckinResponse(
    bool Success,
    string Message,
    string? AttendeeName,
    string? EventName,
    DateTime? CheckedInAt
);
