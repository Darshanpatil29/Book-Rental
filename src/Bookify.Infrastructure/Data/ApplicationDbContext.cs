using Microsoft.EntityFrameworkCore;
using Bookify.Domain.Entities;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Data;

/// <summary>
/// Entity Framework Core Database Context for Bookify.
/// Manages all entities and their relationships with the PostgreSQL database.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
/// </remarks>
/// <param name="options">The DbContext options.</param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    #region DbSets - Domain Entities

    /// <summary>
    /// Gets or sets the Users table.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Apartments table.
    /// </summary>
    public DbSet<Apartment> Apartments { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Bookings table.
    /// </summary>
    public DbSet<Booking> Bookings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Reviews table.
    /// </summary>
    public DbSet<Review> Reviews { get; set; } = null!;

    #endregion

    #region DbSets - Access Control Entities

    /// <summary>
    /// Gets or sets the Roles table.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Permissions table.
    /// </summary>
    public DbSet<Permission> Permissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UserRoles junction table (many-to-many mapping).
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the RolePermissions junction table (many-to-many mapping).
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    #endregion

    /// <summary>
    /// Configures the model when EF Core builds the model for this context.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurations();

        // Apply global configurations
        ApplyGlobalConfigurations(modelBuilder);
    }

    /// <summary>
    /// Applies global query filters and configurations.
    /// </summary>
    private void ApplyGlobalConfigurations(ModelBuilder modelBuilder)
    {
        // Configure decimal precision globally for Money types
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal)))
        {
            property.SetPrecision(10);
            property.SetScale(2);
        }
    }

    /// <summary>
    /// Saves all changes to the database,  updating audit fields automatically.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves all changes asynchronously, updating audit fields automatically.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit fields (CreatedOnUtc, ModifiedOnUtc) for all tracked entities.
    /// Note: These fields are marked with [DatabaseGenerated(Identity)] and have default
    /// CURRENT_TIMESTAMP constraints at the database level, so EF Core manages them automatically.
    /// This method is retained for future use if custom audit logic is needed.
    /// </summary>
    private void UpdateAuditFields()
    {
        // Audit fields are automatically managed by database constraints and EF Core's
        // DatabaseGenerated(Identity) attributes. No manual updates needed here.
    }
}
