using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Configuration;

namespace Bookify.Infrastructure.Services.Authentication;

/// <summary>
/// Generates and validates JWT Bearer tokens for authentication
/// Handles both access tokens (short-lived, claims-rich) and refresh tokens (long-lived, simple)
/// </summary>
public sealed class JwtTokenProvider
{
    private readonly AuthenticationOptions _authOptions;
    private readonly ILogger<JwtTokenProvider> _logger;

    public JwtTokenProvider(
        IOptions<AuthenticationOptions> authOptions,
        ILogger<JwtTokenProvider> logger)
    {
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generates a JWT access token containing user claims, roles, and permissions
    /// Token is signed with HS256 (HMAC-SHA256) using the configured secret
    /// </summary>
    /// <param name="user">User entity to generate token for</param>
    /// <param name="roles">List of role names assigned to user (e.g., ["Admin", "Owner"])</param>
    /// <param name="permissions">List of permission keys (e.g., ["apartments:read", "bookings:create"])</param>
    /// <returns>JWT token string ready for use in Authorization: Bearer header</returns>
    /// <exception cref="InvalidOperationException">Thrown if JWT secret is invalid</exception>
    public string GenerateAccessToken(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        try
        {
            var key = GetSigningKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = BuildAccessTokenClaims(user, roles, permissions);

            var token = new JwtSecurityToken(
                issuer: _authOptions.Jwt.Issuer,
                audience: _authOptions.Jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authOptions.Jwt.AccessTokenExpiryMinutes),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation(
                "Access token generated for user {UserId} with {RoleCount} roles and {PermissionCount} permissions",
                user.Id,
                roles.Count,
                permissions.Count);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to generate access token for user {UserId}",
                user.Id);
            throw new InvalidOperationException("Failed to generate access token", ex);
        }
    }

    /// <summary>
    /// Generates a refresh token - a secure random string used to obtain a new access token
    /// Refresh tokens are long-lived and stored server-side (in RefreshTokens table or Redis)
    /// Uses cryptographically secure random bytes (RNG)
    /// </summary>
    /// <returns>Base64-encoded refresh token (32 bytes = 256 bits)</returns>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32]; // 32 bytes = 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var refreshToken = Convert.ToBase64String(randomBytes);

        _logger.LogDebug("Refresh token generated successfully");

        return refreshToken;
    }

    /// <summary>
    /// Validates an access token and returns claims principal
    /// Checks signature, expiration, issuer, and audience
    /// </summary>
    /// <param name="token">JWT token string to validate</param>
    /// <returns>ClaimsPrincipal if valid; null if invalid or expired</returns>
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var key = GetSigningKey();

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _authOptions.Jwt.ValidateIssuerSigningKey,
                IssuerSigningKey = key,

                ValidateIssuer = _authOptions.Jwt.ValidateIssuer,
                ValidIssuer = _authOptions.Jwt.Issuer,

                ValidateAudience = _authOptions.Jwt.ValidateAudience,
                ValidAudience = _authOptions.Jwt.Audience,

                ValidateLifetime = _authOptions.Jwt.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_authOptions.Jwt.ClockSkewSeconds),

                RequireExpirationTime = true
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            _logger.LogDebug("Access token validated successfully");

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(
                ex,
                "Access token validation failed: token expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning(
                ex,
                "Access token validation failed: invalid signature");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(
                ex,
                "Access token validation failed: invalid token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error validating access token");
            return null;
        }
    }

    /// <summary>
    /// Extracts user ID (sub claim) from a validated claims principal
    /// Returns null if claim is missing or invalid
    /// </summary>
    /// <param name="principal">ClaimsPrincipal from validated token</param>
    /// <returns>User ID as GUID, or null if not found</returns>
    public Guid? GetUserIdFromClaims(ClaimsPrincipal principal)
    {
        var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub");

        if (subClaim?.Value is null)
        {
            _logger.LogWarning("User ID claim (sub) not found in token");
            return null;
        }

        if (Guid.TryParse(subClaim.Value, out var userId))
        {
            return userId;
        }

        _logger.LogWarning("Invalid user ID format in token: {Value}", subClaim.Value);
        return null;
    }

    /// <summary>
    /// Extracts email from a validated claims principal
    /// </summary>
    /// <param name="principal">ClaimsPrincipal from validated token</param>
    /// <returns>Email, or null if not found</returns>
    public string? GetEmailFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Extracts role names from a validated claims principal
    /// Multiple roles are stored as separate role claims
    /// </summary>
    /// <param name="principal">ClaimsPrincipal from validated token</param>
    /// <returns>List of role names</returns>
    public IReadOnlyList<string> GetRolesFromClaims(ClaimsPrincipal principal)
    {
        return principal
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Extracts permission keys from a validated claims principal
    /// Permissions are stored as custom "permission" claims
    /// </summary>
    /// <param name="principal">ClaimsPrincipal from validated token</param>
    /// <returns>List of permission keys</returns>
    public IReadOnlyList<string> GetPermissionsFromClaims(ClaimsPrincipal principal)
    {
        return principal
            .FindAll("permission")
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Calculates token expiration timestamp from access token
    /// Useful for setting response headers or cache duration
    /// </summary>
    /// <param name="principal">ClaimsPrincipal from validated token</param>
    /// <returns>Expiration timestamp as Unix seconds, or null if not found</returns>
    public long? GetExpirationFromClaims(ClaimsPrincipal principal)
    {
        var expClaim = principal.FindFirst("exp");

        if (expClaim?.Value is null || !long.TryParse(expClaim.Value, out var expUnix))
        {
            return null;
        }

        return expUnix;
    }

    /// <summary>
    /// Builds the claims collection for an access token
    /// Includes: subject, email, name, roles, permissions, and standard JWT claims
    /// </summary>
    /// <remarks>
    /// Standard JWT claims (iss, aud, exp, iat, nbf) are added automatically by JwtSecurityToken
    /// This method adds additional claims for authorization
    /// </remarks>
    private List<Claim> BuildAccessTokenClaims(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        var claims = new List<Claim>
        {
            // Subject claim - uniquely identifies the user
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),

            // Email claim
            new(ClaimTypes.Email, user.Email),

            // Name claims
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
        };

        // Add each role as a separate role claim
        // This allows [Authorize(Roles = "Admin,Owner")] to work
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add each permission as a custom claim
        // This allows custom authorization policies like [RequirePermission("apartments:read")]
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        return claims;
    }

    /// <summary>
    /// Gets the symmetric security key from the configured JWT secret
    /// The secret must be at least 32 characters for HMAC256 (256 bits)
    /// </summary>
    /// <returns>SymmetricSecurityKey for token signing/validation</returns>
    /// <exception cref="InvalidOperationException">Thrown if secret is invalid</exception>
    private SymmetricSecurityKey GetSigningKey()
    {
        if (string.IsNullOrEmpty(_authOptions.Jwt.Secret))
        {
            throw new InvalidOperationException("JWT secret is not configured");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_authOptions.Jwt.Secret);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                $"JWT secret must be at least 32 characters (256 bits). Current length: {keyBytes.Length} bytes");
        }

        return new SymmetricSecurityKey(keyBytes);
    }
}
