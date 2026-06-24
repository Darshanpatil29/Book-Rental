# BOOKIFY - Updated Implementation Plan (May 19, 2026)

**Architecture Update**: Replacing Keycloak with custom ASP.NET Core Authentication Schemes (JWT Bearer + Google OAuth)

---

## **PHASE 0: Infrastructure & Authentication Setup** 

### **✅ Phase 0A: Core Infrastructure (COMPLETED)**

**Deliverables:**
- ✅ DbContextOptions configuration class
- ✅ ServiceCollectionExtensions DI registration
- ✅ ApplicationDbContext with Fluent API mappings
- ✅ EntityConfigurations (8 entity configurations)
- ✅ appsettings configuration files (Development + Production)
- ✅ Program.cs integration with InfrastructureConfiguration
- ✅ EF Core migrations & database schema
- ✅ PostgreSQL database setup (running in Docker)

**Status:** Complete ✅

---

### **⏳ Phase 0B: Authentication Setup (JWT Bearer + Google OAuth)**

**Duration:** 6-7 hours | **Priority:** HIGH | **Blocks:** All other phases

#### **0B1: Configuration & Setup (1 hour)**

**Deliverables:**
- [ ] `AuthenticationOptions.cs` - Centralized auth configuration
  - JWT settings (secret, issuer, audience, expiry)
  - Google OAuth settings (client ID, client secret, redirect URI)
  - Token expiry configuration
  - Redis token blacklist settings

- [ ] `appsettings.json` updates
  ```json
  {
    "Authentication": {
      "Jwt": {
        "Secret": "your-min-32-char-secret-key",
        "AccessTokenExpiryMinutes": 15,
        "RefreshTokenExpiryDays": 7,
        "Issuer": "https://bookify.com",
        "Audience": "bookify-api"
      },
      "Google": {
        "ClientId": "xxxx.apps.googleusercontent.com",
        "ClientSecret": "your-secret",
        "RedirectUri": "https://localhost:5000/api/v1/auth/google/callback"
      },
      "TokenBlacklist": {
        "Enabled": true,
        "RedisKeyPrefix": "token:blacklist:"
      }
    }
  }
  ```

- [ ] Install required NuGet packages
  ```bash
  dotnet add package BCrypt.Net-Next
  dotnet add package Google.Apis.Auth
  ```

**Location:** `src/Bookify.Infrastructure/Configuration/`

---

#### **0B2: JWT Token Provider (1.5 hours)**

**Deliverables:**
- [ ] `JwtTokenProvider.cs` - Token generation & validation
  - `GenerateAccessToken(User, roles, permissions)` → JWT string
  - `GenerateRefreshToken()` → Secure random token
  - `ValidateAccessToken(token)` → ClaimsPrincipal
  - `ValidateRefreshToken(token)` → Validation result
  - Build claims from user roles & permissions

- [ ] `TokenBlacklistService.cs` - Redis-based token revocation
  - `AddToBlacklist(token, expiryTime)` → Store in Redis
  - `IsBlacklisted(token)` → Check if revoked
  - Auto-cleanup with Redis TTL

**Location:** `src/Bookify.Infrastructure/Services/Authentication/`

**Key Methods:**
```csharp
public class JwtTokenProvider
{
    public string GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    public string GenerateRefreshToken();
    public ClaimsPrincipal ValidateAccessToken(string token);
    public (bool IsValid, string? Error) ValidateRefreshToken(string token);
}

public class TokenBlacklistService
{
    public Task AddToBlacklist(string token, TimeSpan expiry);
    public Task<bool> IsBlacklisted(string token);
}
```

---

#### **0B3: Authentication Services (2 hours)**

**Deliverables:**
- [ ] `AuthService.cs` - Email/Password authentication
  - `RegisterAsync(email, password, firstName, lastName)` → User + JWT
  - `LoginAsync(email, password)` → JWT + RefreshToken
  - `RefreshTokenAsync(refreshToken)` → New AccessToken
  - `LogoutAsync(userId, refreshToken)` → Invalidate tokens
  - `ChangePasswordAsync(userId, currentPassword, newPassword)`

- [ ] `GoogleAuthService.cs` - Google OAuth 2.0 authentication
  - `AuthenticateWithGoogleAsync(idToken)` → User + JWT
  - `LinkGoogleAccountAsync(userId, idToken)` → Link Google to existing account
  - `ValidateGoogleTokenAsync(idToken)` → User info from Google

**Location:** `src/Bookify.Infrastructure/Services/Authentication/`

**Key Methods:**
```csharp
public class AuthService
{
    public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    public Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    public Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
    public Task<Result> LogoutAsync(Guid userId, string refreshToken);
}

public class GoogleAuthService
{
    public Task<Result<User>> AuthenticateWithGoogleAsync(string idToken);
    public Task<Result> LinkGoogleAccountAsync(Guid userId, string idToken);
}
```

---

#### **0B4: Authentication Scheme Registration (1 hour)**

**Deliverables:**
- [ ] Update `ServiceCollectionExtensions.cs` → `AddAuthentication()` method
  - Register JwtBearerScheme handler
  - Register GoogleOAuth handler
  - Configure token validation parameters
  - Set default authentication scheme

**Code:**
```csharp
public static IServiceCollection AddAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var authOptions = configuration
        .GetSection("Authentication")
        .Get<AuthenticationOptions>();
    
    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => { /* Configure JWT */ })
        .AddGoogle(options => { /* Configure Google */ });
    
    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("OwnerOrAdmin", policy => policy.RequireRole("Owner", "Admin"));
        options.AddPolicy("ApartmentsRead", policy => policy.RequireClaim("permission", "apartments:read"));
        // ... more policies
    });
}
```

**Location:** `src/Bookify.Infrastructure/Extensions/`

---

### **⏳ Phase 0C: Authorization Setup (1.5 hours)**

**Deliverables:**
- [ ] `RbacService.cs` - Role & Permission checks
  - `HasRole(principal, roleNames)` → Check user role
  - `HasPermission(principal, permissionKey)` → Check permission
  - `IsResourceOwner(userId, resourceOwnerId)` → Ownership check
  - `GetUserPermissions(userId)` → Load all permissions (cached in Redis)

- [ ] `RequirePermissionAttribute.cs` - Custom authorization attribute
  - `[RequirePermission("apartments:create")]`
  - Validates permission claim in JWT

- [ ] Authorization Policies (in ServiceCollectionExtensions)
  ```csharp
  options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
  options.AddPolicy("ApartmentsCreate", policy => policy.RequireClaim("permission", "apartments:create"));
  options.AddPolicy("BookingsManage", policy => policy.RequireClaim("permission", "bookings:manage"));
  ```

**Location:** `src/Bookify.Infrastructure/Services/AccessControl/`

---

### **⏳ Phase 0D: API Endpoints (2 hours)**

**Deliverables:**
- [ ] `AuthController.cs` - Authentication endpoints
  ```
  POST   /api/v1/auth/register              # Email registration
  POST   /api/v1/auth/login                 # Email login
  POST   /api/v1/auth/google/signin         # Google OAuth signin
  POST   /api/v1/auth/refresh               # Refresh access token
  POST   /api/v1/auth/logout                # Logout & blacklist
  GET    /api/v1/auth/me                    # Get current user
  POST   /api/v1/auth/change-password       # Change password
  ```

- [ ] `IdentityController.cs` - User profile endpoints
  ```
  GET    /api/v1/identity/profile           # Get user profile
  PUT    /api/v1/identity/profile           # Update profile
  POST   /api/v1/identity/link-google       # Link Google account
  ```

**Location:** `src/Bookify.API/Controllers/`

---

## **PHASE 1: User Management**

**Duration:** 3-4 hours | **Priority:** HIGH | **Depends on:** Phase 0B-0D

### **1A: User Repository (1 hour)**

**Deliverables:**
- [ ] `UserRepository.cs` implementing `IUserRepository`
  - `GetByEmailAsync(email)`
  - `GetByGoogleIdAsync(googleId)`
  - `CreateAsync(user)`
  - `UpdateAsync(user)`
  - `DeleteAsync(userId)`

**Location:** `src/Bookify.Infrastructure/Repositories/`

---

### **1B: User CQRS Handlers (2 hours)**

**Deliverables:**
- [ ] Commands
  - `UpdateProfileCommand` + Handler
  - `ChangePasswordCommand` + Handler
  - `LinkGoogleAccountCommand` + Handler

- [ ] Queries
  - `GetUserProfileQuery` + Handler
  - `GetUserByEmailQuery` + Handler

**Location:** `src/Bookify.Application/Features/Users/`

---

### **1C: Role & Permission Setup (1 hour)**

**Deliverables:**
- [ ] Seed data migration
  - Roles: Admin, Owner, Guest
  - Permissions: apartments:*, bookings:*, reviews:*, users:*
  - RolePermissions: Map permissions to roles

**Location:** `src/Bookify.Infrastructure/Data/Migrations/`

---

## **PHASE 2: Apartment Management**

**Duration:** 4-5 hours | **Priority:** HIGH | **Depends on:** Phase 1

### **2A: Apartment Repository (1 hour)**

**Deliverables:**
- [ ] `ApartmentRepository.cs` implementing `IApartmentRepository`
  - CRUD operations
  - Search with filters (amenities, price range, availability)
  - Pagination support

---

### **2B: Apartment CQRS Handlers (2 hours)**

**Deliverables:**
- [ ] Commands
  - `CreateApartmentCommand` + Handler
  - `UpdateApartmentCommand` + Handler
  - `DeleteApartmentCommand` + Handler

- [ ] Queries
  - `SearchApartmentsQuery` + Handler (with Redis caching)
  - `GetApartmentDetailsQuery` + Handler

---

### **2C: API Endpoints (2 hours)**

**Deliverables:**
- [ ] `ApartmentsController.cs`
  ```
  GET    /api/v1/apartments                 # Search (cached)
  GET    /api/v1/apartments/{id}            # Details
  POST   /api/v1/apartments                 # Create (Owner+)
  PUT    /api/v1/apartments/{id}            # Update (Owner only)
  DELETE /api/v1/apartments/{id}            # Delete (Owner+Admin)
  ```

---

## **PHASE 3: Booking Management**

**Duration:** 5-6 hours | **Priority:** HIGH | **Depends on:** Phase 2

### **3A: Booking Repository + CQRS (2.5 hours)**

**Deliverables:**
- [ ] `BookingRepository.cs`
- [ ] Booking Commands (Create, Confirm, Cancel, Complete, Reject)
- [ ] Booking Queries (GetBooking, GetUserBookings, GetApartmentBookings)

---

### **3B: Booking Business Logic (2 hours)**

**Deliverables:**
- [ ] Overlap detection (ensure no date conflicts)
- [ ] Dynamic pricing calculation
- [ ] Status transition validation
- [ ] Domain events (BookingCreated, BookingConfirmed, etc.)

---

### **3C: API Endpoints (1.5 hours)**

**Deliverables:**
- [ ] `BookingsController.cs` with full CRUD + status operations

---

## **PHASE 4: Reviews & Ratings**

**Duration:** 2-3 hours | **Priority:** MEDIUM

### **Deliverables:**
- [ ] Review Repository + CQRS handlers
- [ ] Review endpoints
- [ ] Rating aggregation & caching

---

## **PHASE 5: Advanced Features**

**Duration:** 3-4 hours | **Priority:** LOW

### **Deliverables:**
- [ ] Payment integration (Stripe/PayPal)
- [ ] Email notifications
- [ ] Background jobs (reminders, cleanup)
- [ ] Admin dashboard endpoints
- [ ] Analytics & reporting

---

## **TIMELINE OVERVIEW**

| Phase | Tasks | Hours | Status |
|-------|-------|-------|--------|
| **0A** | Infrastructure | 7 | ✅ Complete |
| **0B** | JWT + Google Auth | 6-7 | ⏳ Next |
| **0C** | Authorization & RBAC | 1.5 | ⏳ Next |
| **0D** | Auth Endpoints | 2 | ⏳ Next |
| **1** | User Management | 3-4 | Blocked |
| **2** | Apartment CRUD | 4-5 | Blocked |
| **3** | Booking System | 5-6 | Blocked |
| **4** | Reviews | 2-3 | Blocked |
| **5** | Advanced Features | 3-4 | Blocked |
| **TOTAL** | All Phases | 33-39 | - |

---

## **DEPENDENCIES & BLOCKERS**

```
Phase 0A ✅
    ↓
Phase 0B (JWT + Google Auth)
    ↓
Phase 0C (Authorization)
    ↓
Phase 0D (Auth Endpoints)
    ↓
Phase 1 (User Management)
    ├─→ Phase 2 (Apartments)
    │       ├─→ Phase 3 (Bookings)
    │       └─→ Phase 4 (Reviews)
    │
    └─→ Phase 5 (Advanced)
```

---

## **KEY ARCHITECTURE DECISIONS**

| Decision | Approach | Rationale |
|----------|----------|-----------|
| **Auth Method** | JWT Bearer (custom) | Stateless, scalable, mobile-friendly |
| **OAuth Provider** | Google only | Simplifies setup, no Keycloak overhead |
| **Token Strategy** | Access + Refresh tokens | Security best practice, logout support |
| **Token Storage** | JWT (client) + Refresh (Redis) | Stateless + revocation capability |
| **Authorization** | ASP.NET Core Schemes + Policies | Framework-native, automatic validation |
| **Password Hashing** | BCrypt | Industry standard, salted |
| **Permission Caching** | Redis (1-hour TTL) | Performance optimization |
| **Database** | PostgreSQL 15 (already setup) | Reliability, ACID compliance |

---

## **DATABASE UPDATES FOR AUTHENTICATION**

### **New/Modified Tables:**

**Users (already exists, modifications):**
- Add: `GoogleId` (nullable, unique)
- Add: `AuthenticationMethod` enum (EmailPassword=1, Google=2, Both=3)
- Modify: `PasswordHash` (nullable - null if Google-only)

**RefreshTokens (new):**
```sql
CREATE TABLE "RefreshTokens" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id"),
    "Token" VARCHAR(500) NOT NULL UNIQUE,
    "ExpiryDate" TIMESTAMP NOT NULL,
    "IsRevoked" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**TokenBlacklist (handled by Redis, no table needed)**

---

## **TESTING STRATEGY**

### **Unit Tests:**
- AuthService methods
- JwtTokenProvider validation
- RbacService permission checks
- Repository queries

### **Integration Tests:**
- Full auth flow (register → login → access protected endpoint)
- Google OAuth flow
- Token refresh flow
- Logout & token blacklist

### **API Tests:**
- All endpoints with various roles
- Authorization failures
- Invalid token handling

---

## **DEPLOYMENT CONSIDERATIONS**

**Environment Variables (Production):**
```
Authentication__Jwt__Secret=<generated-secret-min-32-chars>
Authentication__Jwt__Issuer=https://bookify.com
Authentication__Google__ClientId=<from-google-console>
Authentication__Google__ClientSecret=<from-google-console>
```

**Secrets Management:**
- Use Azure Key Vault or similar
- Never commit secrets to git
- Rotate tokens regularly

---

## **NEXT IMMEDIATE ACTION**

**Start Phase 0B:** Authentication Setup
1. Create `AuthenticationOptions.cs`
2. Create `JwtTokenProvider.cs` + `TokenBlacklistService.cs`
3. Create `AuthService.cs` + `GoogleAuthService.cs`
4. Update `ServiceCollectionExtensions.cs` with scheme registration
5. Create `AuthController.cs` with endpoints

**Estimated Time:** 6-7 hours

Ready to begin? 🚀
