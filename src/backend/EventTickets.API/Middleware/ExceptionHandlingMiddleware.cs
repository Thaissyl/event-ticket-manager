using System.Net;
using System.Text.Json;

namespace EventTickets.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, message) = GetErrorDetails(exception);

        var response = new
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = title,
            Status = statusCode,
            Detail = message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Title, string Message) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", "You do not have permission to access this resource."),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not Found", exception.Message),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
            TimeoutException => ((int)HttpStatusCode.RequestTimeout, "Request Timeout", "The request timed out."),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An error occurred while processing your request.")
        };
    }
}
