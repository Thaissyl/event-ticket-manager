using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventTickets.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitCounter> _counters;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
        _counters = new ConcurrentDictionary<string, RateLimitCounter>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_counters.TryGetValue(ipAddress, out var counter))
        {
            if (counter.Count >= _options.RequestLimit)
            {
                var timeUntilReset = counter.WindowStart.Add(_options.Window) - DateTime.UtcNow;
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", ipAddress);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = "Too Many Requests",
                    Status = 429,
                    Detail = $"Rate limit exceeded. Try again in {(int)timeUntilReset.TotalSeconds} seconds.",
                    Instance = context.Request.Path
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
                return;
            }

            counter.Count++;
        }
        else
        {
            _counters.TryAdd(ipAddress, new RateLimitCounter
            {
                Count = 1,
                WindowStart = DateTime.UtcNow
            });
        }

        await _next(context);
    }
}

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}

public class RateLimitCounter
{
    public int Count { get; set; }
    public DateTime WindowStart { get; set; }
}
