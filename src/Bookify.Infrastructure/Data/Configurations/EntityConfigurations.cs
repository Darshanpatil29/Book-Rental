using Microsoft.EntityFrameworkCore;
using Bookify.Domain.Entities;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Data;

/// <summary>
/// Centralizes all entity configuration logic for the Bookify database model.
/// Each method configures a specific entity or entity relationship.
/// </summary>
internal static class EntityConfigurations
{
    /// <summary>
    /// Applies all entity configurations to the model builder.
    /// </summary>
    public static void ApplyConfigurations(this ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
        ConfigureApartment(modelBuilder);
        ConfigureBooking(modelBuilder);
        ConfigureReview(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureRolePermission(modelBuilder);
    }

    /// <summary>
    /// Configures the User entity and its relationships.
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Relationships
            entity.HasMany(e => e.OwnedApartments)
                .WithOne(a => a.Owner)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("Users");
        });
    }

    /// <summary>
    /// Configures the Apartment entity with owned value objects and relationships.
    /// </summary>
    private static void ConfigureApartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OwnerId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.OwnsOne(e => e.Address, navigationBuilder =>
            {
                navigationBuilder.Property(a => a.Street)
                    .HasColumnName("Address_Street")
                    .HasMaxLength(200);

                navigationBuilder.Property(a => a.City)
                    .HasColumnName("Address_City")
                    .HasMaxLength(100);

                navigationBuilder.Property(a => a.State)
                    .HasColumnName("Address_State")
                    .HasMaxLength(50);

                navigationBuilder.Property(a => a.ZipCode)
                    .HasColumnName("Address_ZipCode")
                    .HasMaxLength(20);

                navigationBuilder.Property(a => a.Country)
                    .HasColumnName("Address_Country")
                    .HasMaxLength(100);
            });

            entity.OwnsOne(e => e.PricePerNight, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("PricePerNight_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("PricePerNight_Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            entity.Property(e => e.Amenities)
                .IsRequired();

            entity.Property(e => e.MaxGuests)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Relationships
            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.Apartment)
                .HasForeignKey(b => b.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Reviews)
                .WithOne(r => r.Apartment)
                .HasForeignKey(r => r.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.OwnerId)
                .HasDatabaseName("IX_Apartments_OwnerId");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Apartments_IsActive");

            // Table configuration
            entity.ToTable("Apartments");
        });
    }

    /// <summary>
    /// Configures the Booking entity with owned value objects (Money, DateRange) and relationships.
    /// </summary>
    private static void ConfigureBooking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApartmentId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.OwnsOne(e => e.Duration, navigationBuilder =>
            {
                navigationBuilder.Property(dr => dr.StartDate)
                    .HasColumnName("Duration_StartDate");

                navigationBuilder.Property(dr => dr.EndDate)
                    .HasColumnName("Duration_EndDate");
            });

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.OwnsOne(e => e.BasePrice, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("BasePrice_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("BasePrice_Currency")
                    .HasMaxLength(3);
            });

            entity.OwnsOne(e => e.TotalPrice, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("TotalPrice_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("TotalPrice_Currency")
                    .HasMaxLength(3);
            });

            entity.OwnsOne(e => e.AmenitySurcharge, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("AmenitySurcharge_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("AmenitySurcharge_Currency")
                    .HasMaxLength(3);
            });

            entity.OwnsOne(e => e.CleaningFee, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("CleaningFee_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("CleaningFee_Currency")
                    .HasMaxLength(3);
            });

            entity.OwnsOne(e => e.ServiceFee, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("ServiceFee_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("ServiceFee_Currency")
                    .HasMaxLength(3);
            });

            entity.Property(e => e.IsPaid)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.IsCancelled)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.PaidOnUtc);
            entity.Property(e => e.ConfirmedOnUtc);
            entity.Property(e => e.CancelledOnUtc);
            entity.Property(e => e.CompletedOnUtc);
            entity.Property(e => e.RejectedOnUtc);

            entity.OwnsOne(e => e.RefundAmount, navigationBuilder =>
            {
                navigationBuilder.Property(m => m.Amount)
                    .HasColumnName("RefundAmount_Amount")
                    .HasPrecision(10, 2);

                navigationBuilder.Property(m => m.Currency)
                    .HasColumnName("RefundAmount_Currency")
                    .HasMaxLength(3);
            });

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.ApartmentId)
                .HasDatabaseName("IX_Bookings_ApartmentId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Bookings_UserId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Bookings_Status");

            // Composite index for date range queries
            entity.HasIndex(e => new { e.ApartmentId, e.Status })
                .HasDatabaseName("IX_Bookings_ApartmentId_Status");

            // Table configuration
            entity.ToTable("Bookings");
        });
    }

    /// <summary>
    /// Configures the Review entity and its relationships.
    /// </summary>
    private static void ConfigureReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BookingId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.ApartmentId)
                .IsRequired();

            entity.Property(e => e.Rating)
                .IsRequired();

            entity.Property(e => e.Comment)
                .HasMaxLength(1000);

            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Relationships
            entity.HasOne(e => e.Booking)
                .WithMany()
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.ApartmentId)
                .HasDatabaseName("IX_Reviews_ApartmentId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Reviews_UserId");

            entity.HasIndex(e => e.IsApproved)
                .HasDatabaseName("IX_Reviews_IsApproved");

            // Table configuration
            entity.ToTable("Reviews");
        });
    }

    /// <summary>
    /// Configures the Role entity and its relationships with permissions and users.
    /// </summary>
    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name");

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Relationships
            entity.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("Roles");
        });
    }

    /// <summary>
    /// Configures the Permission entity and its relationships.
    /// </summary>
    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Key)
                .IsUnique()
                .HasDatabaseName("IX_Permissions_Key");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Feature)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ModifiedOnUtc)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.Feature)
                .HasDatabaseName("IX_Permissions_Feature");

            entity.HasIndex(e => new { e.Feature, e.Action })
                .HasDatabaseName("IX_Permissions_Feature_Action");

            // Relationships
            entity.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("Permissions");
        });
    }

    /// <summary>
    /// Configures the UserRole junction entity for the many-to-many relationship between users and roles.
    /// </summary>
    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.AssignedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RemovedOnUtc);

            // Indexes
            entity.HasIndex(e => new { e.UserId, e.RoleId })
                .HasDatabaseName("IX_UserRoles_UserId_RoleId");

            entity.HasIndex(e => e.RoleId)
                .HasDatabaseName("IX_UserRoles_RoleId");

            // Table configuration
            entity.ToTable("UserRoles");
        });
    }

    /// <summary>
    /// Configures the RolePermission junction entity for the many-to-many relationship between roles and permissions.
    /// </summary>
    private static void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.CreatedOnUtc)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.PermissionId)
                .HasDatabaseName("IX_RolePermissions_PermissionId");

            // Table configuration
            entity.ToTable("RolePermissions");
        });
    }
}
