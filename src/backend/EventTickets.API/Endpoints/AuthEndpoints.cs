using EventTickets.Core.DTOs;
using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventTickets.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST /api/auth/register - Register new user
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IJwtService jwtService,
            CancellationToken ct) =>
        {
            // Validate email uniqueness
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return Results.BadRequest(new ApiError("EMAIL_EXISTS", "Email already registered"));

            // Create new user
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(new ApiError("REGISTRATION_FAILED",
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            // Add to role
            await userManager.AddToRoleAsync(user, request.Role.ToString());

            // Generate tokens
            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            return Results.Ok(new AuthResponse(
                token,
                refreshToken,
                DateTime.UtcNow.AddMinutes(60),
                new UserResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.Role)
            ));
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithOpenApi();

        // POST /api/auth/login - Login
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IJwtService jwtService,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Results.UnprocessableEntity(new ApiError("INVALID_CREDENTIALS", "Invalid email or password"));

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                return Results.UnprocessableEntity(new ApiError("INVALID_CREDENTIALS", "Invalid email or password"));

            if (result.IsLockedOut)
                return Results.UnprocessableEntity(new ApiError("ACCOUNT_LOCKED", "Account is locked out"));

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            return Results.Ok(new AuthResponse(
                token,
                refreshToken,
                DateTime.UtcNow.AddMinutes(60),
                new UserResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.Role)
            ));
        })
        .WithName("Login")
        .WithSummary("Login user")
        .WithOpenApi();

        // POST /api/auth/refresh - Refresh token
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IJwtService jwtService,
            CancellationToken ct) =>
        {
            if (!jwtService.ValidateToken(request.Token))
                return Results.Unauthorized();

            var userId = jwtService.GetUserIdFromToken(request.Token);
            if (userId is null)
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            if (user is null)
                return Results.Unauthorized();

            var newToken = jwtService.GenerateToken(user);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            return Results.Ok(new AuthResponse(
                newToken,
                newRefreshToken,
                DateTime.UtcNow.AddMinutes(60),
                new UserResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.Role)
            ));
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh JWT token")
        .WithOpenApi();

        // GET /api/auth/me - Get current user
        group.MapGet("/me", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager,
            CancellationToken ct) =>
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return Results.NotFound(new ApiError("USER_NOT_FOUND", "User not found"));

            return Results.Ok(new UserResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.Role));
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user")
        .RequireAuthorization();

        // POST /api/auth/forgot-password - Request password reset
        group.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            [FromServices] UserManager<ApplicationUser> userManager,
            CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                // Don't reveal if email exists
                return Results.Ok(new { message = "If email exists, password reset link sent" });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: Send email with reset token
            // For now, return token in response (development only)
            return Results.Ok(new { message = "Password reset token generated", token });
        })
        .WithName("ForgotPassword")
        .WithSummary("Request password reset")
        .WithOpenApi();

        // POST /api/auth/reset-password - Reset password
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            [FromServices] UserManager<ApplicationUser> userManager,
            CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Results.BadRequest(new ApiError("INVALID_REQUEST", "Invalid reset request"));

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
                return Results.BadRequest(new ApiError("RESET_FAILED",
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Results.Ok(new { message = "Password reset successfully" });
        })
        .WithName("ResetPassword")
        .WithSummary("Reset password with token")
        .WithOpenApi();
    }
}
