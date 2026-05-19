namespace Bookify.Application.Features.Authentication.DTOs;

/// <summary>
/// Request model for user registration with email and password
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// User's email address (unique identifier for login)
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password (will be hashed with BCrypt)
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public required string LastName { get; set; }
}

/// <summary>
/// Request model for user login with credentials
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password (will be verified against stored hash)
    /// </summary>
    public required string Password { get; set; }
}

/// <summary>
/// Response model for successful authentication (registration or login)
/// Contains access and refresh tokens
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// JWT access token for API requests
    /// Header: Authorization: Bearer {accessToken}
    /// Lifetime: 15 minutes (configurable)
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Refresh token for obtaining a new access token without re-authenticating
    /// Stored client-side (typically in httpOnly cookie)
    /// Lifetime: 7 days (configurable)
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Access token expiration time (Unix seconds)
    /// Allows client to preemptively refresh before expiry
    /// </summary>
    public long ExpiresIn { get; set; }

    /// <summary>
    /// Token type (always "Bearer" for JWT)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// User information from newly authenticated account
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// Request model for refreshing an expired access token
/// </summary>
public sealed class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token obtained from previous login/registration
    /// </summary>
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Request model for logout (invalidates tokens)
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// Refresh token to invalidate
    /// Ensures only this specific login session is terminated
    /// </summary>
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Request model for password change (authenticated users only)
/// </summary>
public sealed class ChangePasswordRequest
{
    /// <summary>
    /// User's current password (for verification)
    /// </summary>
    public required string CurrentPassword { get; set; }

    /// <summary>
    /// User's new password
    /// </summary>
    public required string NewPassword { get; set; }

    /// <summary>
    /// New password confirmation (must match NewPassword)
    /// </summary>
    public required string ConfirmPassword { get; set; }
}

/// <summary>
/// Request model for Google OAuth sign-in
/// </summary>
public sealed class GoogleSignInRequest
{
    /// <summary>
    /// Google ID Token obtained from Google Sign-In client library
    /// Contains signed user information from Google
    /// </summary>
    public required string IdToken { get; set; }
}

/// <summary>
/// Request model for linking Google account to existing email-based account
/// </summary>
public sealed class LinkGoogleAccountRequest
{
    /// <summary>
    /// Google ID Token to link to user's account
    /// </summary>
    public required string IdToken { get; set; }
}

/// <summary>
/// User information included in authentication responses
/// Contains basic profile data without sensitive information
/// </summary>
public sealed class UserInfo
{
    /// <summary>
    /// User's unique identifier (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// List of role names assigned to user
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether user's email is verified (optional)
    /// </summary>
    public bool EmailVerified { get; set; }
}

/// <summary>
/// Error response for authentication failures
/// </summary>
public sealed class AuthErrorResponse
{
    /// <summary>
    /// Error code (e.g., "INVALID_CREDENTIALS", "TOKEN_EXPIRED")
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Additional details (field-level errors for validation)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}
