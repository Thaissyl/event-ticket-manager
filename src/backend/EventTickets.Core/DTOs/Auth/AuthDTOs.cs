using EventTickets.Core.Enums;

namespace EventTickets.Core.DTOs;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName, UserRole Role = UserRole.Attendee);
public record RefreshTokenRequest(string Token, string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
