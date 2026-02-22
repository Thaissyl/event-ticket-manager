namespace EventTickets.Core.DTOs;

public record ApiResponse<T>(T Data, string? Message = null);

public record ApiError(string Code, string Message, Dictionary<string, string[]>? Errors = null);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
