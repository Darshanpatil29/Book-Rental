namespace Bookify.Infrastructure.Configuration;

/// <summary>
/// Configuration options for authentication (JWT + Google OAuth)
/// Binds to appsettings.json "Authentication" section
/// </summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// JWT Bearer token configuration
    /// </summary>
    public required JwtOptions Jwt { get; set; }

    /// <summary>
    /// Google OAuth 2.0 configuration
    /// </summary>
    public required GoogleOptions Google { get; set; }

    /// <summary>
    /// Token blacklist (revocation) configuration
    /// </summary>
    public required TokenBlacklistOptions TokenBlacklist { get; set; }
}

/// <summary>
/// JWT Bearer token settings
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Secret key for signing JWT tokens (minimum 32 characters for HMAC256)
    /// NEVER commit this to version control - use environment variables
    /// </summary>
    public required string Secret { get; set; }

    /// <summary>
    /// Access token lifetime in minutes (e.g., 15 for 15 minutes)
    /// Typical: 15 minutes
    /// </summary>
    public int AccessTokenExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days (e.g., 7 for 7 days)
    /// Typical: 7 days
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>
    /// Token issuer claim (e.g., "https://bookify.com")
    /// Validates that token was issued by this service
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Token audience claim (e.g., "bookify-api")
    /// Validates that token is intended for this service
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Clock skew tolerance in seconds for validation (e.g., 0 for strict)
    /// Accounts for minor time differences between servers
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 0;

    /// <summary>
    /// Whether to validate token expiration
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer claim
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate audience claim
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}

/// <summary>
/// Google OAuth 2.0 configuration
/// Obtained from Google Cloud Console: https://console.cloud.google.com/
/// </summary>
public sealed class GoogleOptions
{
    /// <summary>
    /// OAuth 2.0 Client ID from Google Cloud Console
    /// Format: xxxxxxxxxxxxxx.apps.googleusercontent.com
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// OAuth 2.0 Client Secret from Google Cloud Console
    /// NEVER commit this to version control - use environment variables
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Redirect URI for OAuth callback (must match Google Console configuration)
    /// Format: https://yourdomain/api/v1/auth/google/callback
    /// </summary>
    public required string RedirectUri { get; set; }

    /// <summary>
    /// Whether Google authentication is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// List of scopes to request from Google (e.g., "openid email profile")
    /// </summary>
    public string Scopes { get; set; } = "openid email profile";
}

/// <summary>
/// Token blacklist (revocation) configuration
/// Used to invalidate tokens on logout
/// </summary>
public sealed class TokenBlacklistOptions
{
    /// <summary>
    /// Whether token blacklist is enabled (requires Redis)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis key prefix for blacklisted tokens (e.g., "token:blacklist:")
    /// Full key format: "{prefix}{token_jti}"
    /// </summary>
    public string RedisKeyPrefix { get; set; } = "token:blacklist:";

    /// <summary>
    /// Redis database number (typically 0 for primary, 1-15 for alternatives)
    /// </summary>
    public int RedisDatabase { get; set; } = 0;

    /// <summary>
    /// Connection string for Redis (if different from main cache)
    /// If empty, uses default Redis connection from main configuration
    /// </summary>
    public string? RedisConnectionString { get; set; }
}
