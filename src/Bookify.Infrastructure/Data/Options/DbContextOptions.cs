namespace Bookify.Infrastructure.Data;

/// <summary>
/// Configuration options for the Bookify Database Context.
/// </summary>
public class DbContextOptions
{
    public const string SectionName = "Database";
    /// <summary>
    /// Gets or sets the database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to enable sensitive data logging (for debugging only).
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed error logging.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to track query performance.
    /// </summary>
    public bool TrackQueryPerformance { get; set; } = false;

    /// <summary>
    /// Gets or sets the command timeout in seconds (30 = use default).
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether lazy loading proxies are enabled.
    /// </summary>
    public bool UseLazyLoadingProxies { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum pool size for connection pooling.
    /// </summary>
    public int MaxPoolSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the environment name (Development, Staging, Production).
    /// </summary>
    public string? EnvironmentName { get; set; }
}
