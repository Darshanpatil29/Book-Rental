using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Bookify.Infrastructure.Configuration;

namespace Bookify.Infrastructure.Services.Authentication;

/// <summary>
/// Manages token blacklisting (revocation) using Redis
/// When a token is blacklisted, it cannot be used for authentication
/// Useful for implementing logout functionality without maintaining a list of valid sessions
/// </summary>
public sealed class TokenBlacklistService
{
    private readonly AuthenticationOptions _authOptions;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<TokenBlacklistService> _logger;

    public TokenBlacklistService(
        IOptions<AuthenticationOptions> authOptions,
        IConnectionMultiplexer redis,
        ILogger<TokenBlacklistService> logger)
    {
        _authOptions = authOptions.Value;
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Adds a token to the blacklist, preventing its use for future authentication attempts
    /// Uses Redis with automatic expiration (TTL) matching the token's remaining lifetime
    /// </summary>
    /// <param name="token">JWT token string to blacklist</param>
    /// <param name="expiryTime">Token expiration timestamp (Unix seconds)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was added; false if token blacklist is disabled or Redis error</returns>
    public async Task<bool> AddToBlacklistAsync(
        string token,
        long expiryTime,
        CancellationToken cancellationToken = default)
    {
        if (!_authOptions.TokenBlacklist.Enabled)
        {
            _logger.LogDebug("Token blacklist is disabled; token not added");
            return true;
        }

        try
        {
            var db = _redis.GetDatabase(_authOptions.TokenBlacklist.RedisDatabase);

            // Extract JTI claim if present (unique token identifier)
            var jti = ExtractJtiFromToken(token) ?? token.Substring(0, Math.Min(50, token.Length));

            var blacklistKey = $"{_authOptions.TokenBlacklist.RedisKeyPrefix}{jti}";

            // Calculate TTL: token expiration time - now (in seconds)
            var ttl = expiryTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (ttl <= 0)
            {
                // Token already expired, no need to blacklist
                _logger.LogDebug("Token already expired; not adding to blacklist");
                return true;
            }

            // Store in Redis with automatic expiration matching token expiry
            var result = await db.StringSetAsync(
                blacklistKey,
                "revoked",
                TimeSpan.FromSeconds(ttl),
                When.NotExists);

            if (result)
            {
                _logger.LogInformation(
                    "Token added to blacklist (expires in {Ttl} seconds)",
                    ttl);
            }
            else
            {
                _logger.LogWarning("Token was already in blacklist");
            }

            return true;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(
                ex,
                "Redis connection error while blacklisting token; token revocation may fail");
            // In production, consider whether to fail the logout or succeed anyway
            // For now, we'll log but continue (token will eventually expire)
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while adding token to blacklist");
            return false;
        }
    }

    /// <summary>
    /// Checks if a token is blacklisted (revoked)
    /// Returns false if token blacklist is disabled
    /// </summary>
    /// <param name="token">JWT token string to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is blacklisted; false if valid or blacklist disabled</returns>
    public async Task<bool> IsBlacklistedAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (!_authOptions.TokenBlacklist.Enabled)
        {
            _logger.LogDebug("Token blacklist is disabled; returning false");
            return false;
        }

        try
        {
            var db = _redis.GetDatabase(_authOptions.TokenBlacklist.RedisDatabase);

            // Extract JTI claim if present
            var jti = ExtractJtiFromToken(token) ?? token.Substring(0, Math.Min(50, token.Length));

            var blacklistKey = $"{_authOptions.TokenBlacklist.RedisKeyPrefix}{jti}";

            var exists = await db.KeyExistsAsync(blacklistKey);

            if (exists)
            {
                _logger.LogWarning("Blacklisted token attempted to be used");
            }

            return exists;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(
                ex,
                "Redis connection error while checking token blacklist; defaulting to allow token");
            // In production, decide: fail-open (allow token) or fail-closed (deny token)
            // Currently: fail-open (more user-friendly)
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while checking token blacklist");
            return false;
        }
    }

    /// <summary>
    /// Removes a token from the blacklist (useful for testing or token recovery)
    /// </summary>
    /// <param name="token">JWT token string to remove from blacklist</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token was removed or wasn't blacklisted</returns>
    public async Task<bool> RemoveFromBlacklistAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (!_authOptions.TokenBlacklist.Enabled)
        {
            return true;
        }

        try
        {
            var db = _redis.GetDatabase(_authOptions.TokenBlacklist.RedisDatabase);

            var jti = ExtractJtiFromToken(token) ?? token.Substring(0, Math.Min(50, token.Length));
            var blacklistKey = $"{_authOptions.TokenBlacklist.RedisKeyPrefix}{jti}";

            var deleted = await db.KeyDeleteAsync(blacklistKey);

            _logger.LogInformation("Token removed from blacklist (existed: {Existed})", deleted);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error removing token from blacklist");
            return false;
        }
    }

    /// <summary>
    /// Clears all blacklisted tokens from Redis
    /// WARNING: Use with caution in production; affects all logged-out users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
    public async Task<long> ClearAllBlacklistedTokensAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_authOptions.TokenBlacklist.Enabled)
        {
            return 0;
        }

        try
        {
            var db = _redis.GetDatabase(_authOptions.TokenBlacklist.RedisDatabase);
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            var pattern = $"{_authOptions.TokenBlacklist.RedisKeyPrefix}*";
            var keys = server.Keys(
                database: _authOptions.TokenBlacklist.RedisDatabase,
                pattern: pattern);

            var deletedCount = 0L;

            foreach (var key in keys)
            {
                if (await db.KeyDeleteAsync(key))
                {
                    deletedCount++;
                }
            }

            _logger.LogWarning("Cleared {Count} blacklisted tokens", deletedCount);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error clearing blacklisted tokens");
            return 0;
        }
    }

    /// <summary>
    /// Gets statistics about the token blacklist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of currently blacklisted tokens</returns>
    public async Task<long> GetBlacklistedTokenCountAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_authOptions.TokenBlacklist.Enabled)
        {
            return 0;
        }

        try
        {
            var db = _redis.GetDatabase(_authOptions.TokenBlacklist.RedisDatabase);
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            var pattern = $"{_authOptions.TokenBlacklist.RedisKeyPrefix}*";
            var keys = server.Keys(
                database: _authOptions.TokenBlacklist.RedisDatabase,
                pattern: pattern);

            return keys.LongCount();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting blacklisted token count");
            return 0;
        }
    }

    /// <summary>
    /// Extracts the JTI (JWT ID) claim from a token for use as unique identifier
    /// JTI is optional in JWT but recommended for token tracking
    /// Falls back to token hash if JTI not present
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>JTI claim value, or null if not found</returns>
    private static string? ExtractJtiFromToken(string token)
    {
        try
        {
            // Simple extraction without validation - just to get JTI for blacklist key
            // Format: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            // Decode payload (second part)
            var payload = parts[1];

            // Add padding if necessary
            var paddedPayload = payload.Length % 4 == 0
                ? payload
                : payload + new string('=', 4 - payload.Length % 4);

            var decodedBytes = Convert.FromBase64String(paddedPayload);
            var json = System.Text.Encoding.UTF8.GetString(decodedBytes);

            // Extract JTI using simple string search (avoid JSON parsing dependency)
            var jtiStart = json.IndexOf("\"jti\":\"", StringComparison.Ordinal);
            if (jtiStart == -1)
            {
                return null;
            }

            jtiStart += 7; // Length of "\"jti\":\""
            var jtiEnd = json.IndexOf("\"", jtiStart, StringComparison.Ordinal);

            if (jtiEnd == -1)
            {
                return null;
            }

            return json.Substring(jtiStart, jtiEnd - jtiStart);
        }
        catch
        {
            return null;
        }
    }
}
