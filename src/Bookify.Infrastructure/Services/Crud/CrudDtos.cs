using Bookify.Shared.Enums;

namespace Bookify.Infrastructure.Services.Crud;

public sealed record AddressRequest(string Country, string State, string City, string ZipCode, string Street);

public sealed record MoneyRequest(decimal Amount, string Currency);

public sealed record DateRangeRequest(DateOnly StartDate, DateOnly EndDate);

public sealed record UserRequest(
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive);

public sealed record RoleRequest(string Name, string Description, bool IsActive);

public sealed record PermissionRequest(
    string Feature,
    string Action,
    string Name,
    string Description,
    int SortOrder,
    bool IsActive);

public sealed record ApartmentRequest(
    Guid OwnerId,
    string Title,
    string Description,
    AddressRequest Address,
    MoneyRequest PricePerNight,
    AmenityType Amenities,
    int MaxGuests,
    bool IsActive);

public sealed record BookingRequest(
    Guid ApartmentId,
    Guid UserId,
    DateRangeRequest Duration,
    BookingStatus Status,
    MoneyRequest BasePrice,
    MoneyRequest TotalPrice,
    MoneyRequest AmenitySurcharge,
    MoneyRequest? CleaningFee,
    MoneyRequest? ServiceFee,
    bool IsPaid,
    bool IsCancelled,
    DateTime? PaidOnUtc,
    DateTime? ConfirmedOnUtc,
    DateTime? CancelledOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? RejectedOnUtc,
    MoneyRequest? RefundAmount);

public sealed record ReviewRequest(
    Guid BookingId,
    Guid UserId,
    Guid ApartmentId,
    int Rating,
    string Comment,
    bool IsApproved);

public sealed record UserRoleRequest(Guid UserId, Guid RoleId, DateTime? RemovedOnUtc);

public sealed record RolePermissionRequest(Guid RoleId, Guid PermissionId);