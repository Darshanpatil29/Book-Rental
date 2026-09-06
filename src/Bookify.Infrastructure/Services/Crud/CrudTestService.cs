using System.Reflection;
using Bookify.Domain.Common;
using Bookify.Domain.Entities;
using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Data;
using Bookify.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class CrudTestService : ICrudTestService
{
    private readonly ApplicationDbContext _db;

    public CrudTestService(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : DataEntity
        => _db.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : DataEntity
        => _db.Set<T>().AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<T> CreateAsync<T>(object request, CancellationToken cancellationToken = default) where T : DataEntity
    {
        var entity = CreateInstance<T>();
        ApplyValues(entity, request, isCreate: true);
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<T?> UpdateAsync<T>(Guid id, object request, CancellationToken cancellationToken = default) where T : DataEntity
    {
        var entity = await _db.Set<T>().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
            return null;

        ApplyValues(entity, request, isCreate: false);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : DataEntity
    {
        var entity = await _db.Set<T>().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
            return false;

        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<UserRole>> GetAllUserRolesAsync(CancellationToken cancellationToken = default)
        => _db.UserRoles.AsNoTracking().Include(item => item.User).Include(item => item.Role).ToListAsync(cancellationToken);

    public Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        => _db.UserRoles.Include(item => item.User).Include(item => item.Role).FirstOrDefaultAsync(item => item.UserId == userId && item.RoleId == roleId, cancellationToken);

    public async Task<UserRole> CreateUserRoleAsync(UserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = UserRole.Create(request.UserId, request.RoleId);
        if (request.RemovedOnUtc.HasValue)
            SetProperty(entity, nameof(UserRole.RemovedOnUtc), request.RemovedOnUtc);

        _db.UserRoles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<UserRole?> UpdateUserRoleAsync(Guid userId, Guid roleId, UserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetUserRoleAsync(userId, roleId, cancellationToken);
        if (entity is null)
            return null;

        SetProperty(entity, nameof(UserRole.RemovedOnUtc), request.RemovedOnUtc);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var entity = await GetUserRoleAsync(userId, roleId, cancellationToken);
        if (entity is null)
            return false;

        _db.UserRoles.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<List<RolePermission>> GetAllRolePermissionsAsync(CancellationToken cancellationToken = default)
        => _db.RolePermissions.AsNoTracking().Include(item => item.Role).Include(item => item.Permission).ToListAsync(cancellationToken);

    public Task<RolePermission?> GetRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        => _db.RolePermissions.Include(item => item.Role).Include(item => item.Permission).FirstOrDefaultAsync(item => item.RoleId == roleId && item.PermissionId == permissionId, cancellationToken);

    public async Task<RolePermission> CreateRolePermissionAsync(RolePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = RolePermission.Create(request.RoleId, request.PermissionId);
        _db.RolePermissions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<RolePermission?> UpdateRolePermissionAsync(Guid roleId, Guid permissionId, RolePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetRolePermissionAsync(roleId, permissionId, cancellationToken);
        if (entity is null)
            return null;

        if (entity.RoleId == request.RoleId && entity.PermissionId == request.PermissionId)
            return entity;

        _db.RolePermissions.Remove(entity);
        var replacement = RolePermission.Create(request.RoleId, request.PermissionId);
        _db.RolePermissions.Add(replacement);
        await _db.SaveChangesAsync(cancellationToken);
        return replacement;
    }

    public async Task<bool> DeleteRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var entity = await GetRolePermissionAsync(roleId, permissionId, cancellationToken);
        if (entity is null)
            return false;

        _db.RolePermissions.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static T CreateInstance<T>() where T : DataEntity
        => (T)(Activator.CreateInstance(typeof(T), nonPublic: true)
            ?? throw new InvalidOperationException($"Unable to create instance of {typeof(T).Name}."));

    private static void ApplyValues<TEntity>(TEntity entity, object request, bool isCreate)
        where TEntity : DataEntity
    {
        foreach (var requestProperty in request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var entityProperty = typeof(TEntity).GetProperty(requestProperty.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var setter = entityProperty?.GetSetMethod(true);
            if (entityProperty is null || setter is null)
                continue;

            var requestValue = requestProperty.GetValue(request);
            if (!TryConvertValue(entityProperty.PropertyType, requestValue, out var convertedValue))
                continue;

            setter.Invoke(entity, new[] { convertedValue });
        }

        if (isCreate)
        {
            SetProperty(entity, nameof(DataEntity.Id), Guid.NewGuid());
            SetProperty(entity, nameof(DataEntity.CreatedOnUtc), DateTime.UtcNow);
        }

        SetProperty(entity, nameof(DataEntity.ModifiedOnUtc), DateTime.UtcNow);
    }

    private static void SetProperty<TValue>(object target, string propertyName, TValue? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var setter = property?.GetSetMethod(true);
        if (property is null || setter is null)
            return;

        setter.Invoke(target, new object?[] { value });
    }

    private static bool TryConvertValue(Type targetType, object? value, out object? convertedValue)
    {
        convertedValue = null;

        if (value is null)
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;

        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (actualType.IsAssignableFrom(value.GetType()))
        {
            convertedValue = value;
            return true;
        }

        if (actualType.IsEnum)
        {
            convertedValue = value is string enumText
                ? Enum.Parse(actualType, enumText, ignoreCase: true)
                : Enum.ToObject(actualType, value);
            return true;
        }

        if (actualType == typeof(Guid))
        {
            convertedValue = value is Guid guid ? guid : Guid.Parse(value.ToString() ?? string.Empty);
            return true;
        }

        if (actualType == typeof(DateOnly))
        {
            convertedValue = value is DateOnly dateOnly ? dateOnly : DateOnly.Parse(value.ToString() ?? string.Empty);
            return true;
        }

        if (actualType == typeof(DateTime))
        {
            convertedValue = value is DateTime dateTime ? dateTime : DateTime.Parse(value.ToString() ?? string.Empty);
            return true;
        }

        if (actualType == typeof(Money))
        {
            convertedValue = CreateMoney(value);
            return true;
        }

        if (actualType == typeof(Address))
        {
            convertedValue = CreateAddress(value);
            return true;
        }

        if (actualType == typeof(DateRange))
        {
            convertedValue = CreateDateRange(value);
            return true;
        }

        return false;
    }

    private static Money CreateMoney(object value)
    {
        var type = value.GetType();
        var amount = (decimal)(type.GetProperty(nameof(MoneyRequest.Amount))?.GetValue(value) ?? 0m);
        var currency = type.GetProperty(nameof(MoneyRequest.Currency))?.GetValue(value)?.ToString() ?? string.Empty;
        return new Money(amount, currency);
    }

    private static Address CreateAddress(object value)
    {
        var type = value.GetType();
        return new Address(
            type.GetProperty(nameof(AddressRequest.Country))?.GetValue(value)?.ToString() ?? string.Empty,
            type.GetProperty(nameof(AddressRequest.State))?.GetValue(value)?.ToString() ?? string.Empty,
            type.GetProperty(nameof(AddressRequest.City))?.GetValue(value)?.ToString() ?? string.Empty,
            type.GetProperty(nameof(AddressRequest.ZipCode))?.GetValue(value)?.ToString() ?? string.Empty,
            type.GetProperty(nameof(AddressRequest.Street))?.GetValue(value)?.ToString() ?? string.Empty);
    }

    private static DateRange CreateDateRange(object value)
    {
        var type = value.GetType();
        return new DateRange(
            (DateOnly)(type.GetProperty(nameof(DateRangeRequest.StartDate))?.GetValue(value) ?? default(DateOnly)),
            (DateOnly)(type.GetProperty(nameof(DateRangeRequest.EndDate))?.GetValue(value) ?? default(DateOnly)));
    }
}