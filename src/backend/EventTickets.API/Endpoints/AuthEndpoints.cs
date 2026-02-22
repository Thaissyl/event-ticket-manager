namespace EventTickets.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST /api/auth/register - Register new user
        group.MapPost("/register", () =>
        {
            // TODO: Implement registration logic in Phase 04
            return Results.Ok(new { message = "Registration endpoint - to be implemented in Phase 04" });
        })
        .WithName("Register")
        .WithSummary("Register a new user");

        // POST /api/auth/login - Login
        group.MapPost("/login", () =>
        {
            // TODO: Implement login logic in Phase 04
            return Results.Ok(new { message = "Login endpoint - to be implemented in Phase 04" });
        })
        .WithName("Login")
        .WithSummary("Login user");

        // GET /api/auth/me - Get current user
        group.MapGet("/me", () =>
        {
            // TODO: Implement current user logic in Phase 04
            return Results.Ok(new { message = "Current user endpoint - to be implemented in Phase 04" });
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current user")
        .RequireAuthorization();
    }
}
