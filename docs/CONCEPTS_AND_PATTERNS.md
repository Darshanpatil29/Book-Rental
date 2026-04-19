# Bookify: Essential Concepts & Patterns Guide

A comprehensive reference for understanding the technologies and patterns used in the Bookify apartment rental booking system.

---

## Table of Contents

1. [CQRS Pattern](#cqrs-pattern)
2. [MediatR](#mediatr)
3. [Keycloak Authentication](#keycloak-authentication)
4. [Redis Caching](#redis-caching)
5. [Docker Containerization](#docker-containerization)
6. [Kubernetes Orchestration](#kubernetes-orchestration)
7. [CI/CD Pipeline](#cicd-pipeline)
8. [Integration Overview](#integration-overview)

---

## 1. CQRS Pattern

### What is CQRS?

**CQRS = Command Query Responsibility Segregation**

CQRS is an architectural pattern that separates read operations (queries) from write operations (commands) into completely different models and data flows. Instead of using a single data model for all operations, you maintain separate models optimized for each purpose.

### Traditional Approach vs CQRS

**Traditional (CRUD):**
```
Request → Single Model → Database
         (Read/Write)
         
User creates booking → Same model handles both read and write
```

**CQRS Approach:**
```
Write Side                          Read Side
Command (Write) →                   Query (Read) →
ReserveBooking                      SearchApartments
   ↓                                   ↓
Write Model                         Read Model
(Normalized)                        (Denormalized)
   ↓                                   ↓
PostgreSQL                          Redis Cache
(Transaction)                       (Optimized)
```

### CQRS in Bookify

**Commands (State-Changing Operations):**
```csharp
// Write operations
public class ReserveBookingCommand : IRequest<Result<BookingReservedResponse>>
{
    public Guid ApartmentId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public Guid UserId { get; init; }
}

// Handler processes the command
public class ReserveBookingCommandHandler : IRequestHandler<ReserveBookingCommand, Result<BookingReservedResponse>>
{
    public async Task<Result<BookingReservedResponse>> Handle(...)
    {
        // Business logic: validate dates, check availability, calculate price
        // Save to database
        // Publish domain events
    }
}
```

**Queries (Read Operations):**
```csharp
// Read operations (optimized for speed)
public class SearchApartmentsQuery : IRequest<Result<List<ApartmentDto>>>
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
}

// Handler optimized for read performance
public class SearchApartmentsQueryHandler : IRequestHandler<SearchApartmentsQuery, Result<List<ApartmentDto>>>
{
    public async Task<Result<List<ApartmentDto>>> Handle(...)
    {
        // Check Redis cache first (< 1ms)
        // If miss, query database with optimized SQL (Dapper)
        // Cache result
    }
}
```

### Benefits of CQRS

| Benefit | How Bookify Uses It |
|---------|-------------------|
| **Performance** | Read queries optimized with Dapper, writes with EF Core |
| **Scalability** | Read replicas can handle search traffic independently |
| **Clarity** | Clear intent: command = write, query = read |
| **Flexibility** | Different databases for reads vs writes (SQL vs Redis) |
| **Event Sourcing** | Easy to integrate with domain events and the outbox pattern |

### CQRS Flow in Bookify

```
1. Reserve Booking (COMMAND)
   POST /api/v1/bookings
   ├─ API receives ReserveBookingCommand
   ├─ MediatR pipeline: Validation → Logging → Handler
   ├─ Handler validates dates, checks availability
   ├─ Creates Booking aggregate, saves to PostgreSQL
   ├─ Raises BookingReservedDomainEvent
   └─ Outbox captures event for async processing

2. Search Apartments (QUERY)
   GET /api/v1/apartments?startDate=2026-04-25&endDate=2026-04-30
   ├─ API receives SearchApartmentsQuery
   ├─ MediatR pipeline: Cache check → Logging → Handler
   ├─ Handler checks Redis (1 hour TTL)
   ├─ (Cache hit) → Return immediately (< 1ms)
   ├─ (Cache miss) → Query PostgreSQL with Dapper
   ├─ Cache result in Redis
   └─ Return response (< 100ms)
```

---

## 2. MediatR

### What is MediatR?

**MediatR** is a .NET library that implements the mediator pattern, enabling request-response messaging without direct dependencies between components. It acts as a "message bus" for commands and queries in your application.

### Problem MediatR Solves

**Without MediatR (Tightly Coupled):**
```csharp
public class BookingsController
{
    private readonly ApartmentRepository _apartmentRepo;
    private readonly BookingRepository _bookingRepo;
    private readonly PricingService _pricingService;
    private readonly EmailService _emailService;
    
    // Every dependency injected, hard to test, tight coupling
    public BookingsController(ApartmentRepository apartmentRepo, ...) { }
}
```

**With MediatR (Loosely Coupled):**
```csharp
public class BookingsController
{
    private readonly IMediator _mediator;
    
    // Only depends on one abstraction
    public BookingsController(IMediator mediator) 
    {
        _mediator = mediator;
    }
    
    public async Task<IActionResult> Reserve(ReserveBookingRequest request)
    {
        var command = new ReserveBookingCommand { ... };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Created(...) : BadRequest(...);
    }
}
```

### Core Concepts: Commands, Queries, Handlers

```
Request (Command/Query)
        ↓
    MediatR
        ↓
    Pipeline Behaviors
    (Validation, Logging, etc.)
        ↓
    Handler
    (Business Logic)
        ↓
    Response
```

**Three Types of Messages:**

1. **Commands** (IRequest<TResponse>)
   - Modify state
   - Return a response
   - Only one handler per command

2. **Queries** (IRequest<TResponse>)
   - Read-only operations
   - Return data
   - Only one handler per query

3. **Notifications** (INotification)
   - Fire-and-forget
   - Multiple handlers can process
   - Don't return values

### MediatR Pipeline in Bookify

```csharp
// Setup in Startup
services.AddMediatR(config => 
{
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Pipeline Behaviors (middleware for each command/query)
public class ValidationBehavior<in TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, NextDelegate<TResponse> next, ...)
    {
        if (_validators.Any())
        {
            var validationFailures = _validators
                .SelectMany(v => v.Validate(request).Errors)
                .ToList();
                
            if (validationFailures.Any())
                throw new ValidationException(validationFailures);
        }
        
        return await next();  // Continue to next behavior or handler
    }
}

public class LoggingBehavior<in TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, NextDelegate<TResponse> next, ...)
    {
        _logger.LogInformation("Executing {@Request}", request);
        var response = await next();
        _logger.LogInformation("Executed {@Request} → {@Response}", request, response);
        return response;
    }
}
```

### Complete Example: Reserve Booking

```csharp
// 1. Define the Command
public class ReserveBookingCommand : IRequest<Result<BookingReservedResponse>>
{
    public Guid ApartmentId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

// 2. Define the Validation
public class ReserveBookingCommandValidator : AbstractValidator<ReserveBookingCommand>
{
    public ReserveBookingCommandValidator()
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty().GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

// 3. Define the Handler
public class ReserveBookingCommandHandler : IRequestHandler<ReserveBookingCommand, Result<BookingReservedResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IPricingService _pricingService;
    
    public async Task<Result<BookingReservedResponse>> Handle(ReserveBookingCommand request, CancellationToken ct)
    {
        // 1. Get apartment
        var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId);
        if (apartment is null)
            return Result.Failure<BookingReservedResponse>("Apartment not found");
        
        // 2. Check availability
        var hasConflict = await _bookingRepository.HasConflictAsync(
            request.ApartmentId, 
            request.StartDate, 
            request.EndDate);
        if (hasConflict)
            return Result.Failure<BookingReservedResponse>("Dates not available");
        
        // 3. Calculate price
        var pricing = _pricingService.CalculateTotalPrice(
            apartment, 
            request.StartDate, 
            request.EndDate);
        
        // 4. Create booking
        var booking = Booking.Reserve(
            request.ApartmentId, 
            request.StartDate, 
            request.EndDate, 
            pricing);
        
        // 5. Save to database
        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 6. Return success
        return Result.Success(new BookingReservedResponse 
        { 
            BookingId = booking.Id, 
            TotalPrice = pricing.TotalPrice 
        });
    }
}

// 4. Use in Controller
[HttpPost("bookings")]
public async Task<IActionResult> ReserveBooking(ReserveBookingRequest request)
{
    var command = new ReserveBookingCommand 
    { 
        ApartmentId = request.ApartmentId,
        StartDate = request.StartDate,
        EndDate = request.EndDate 
    };
    
    var result = await _mediator.Send(command);
    
    return result.IsSuccess 
        ? Created($"/api/v1/bookings/{result.Value.BookingId}", result.Value)
        : BadRequest(result.Error);
}
```

### Benefits of MediatR in Bookify

- **Decoupling:** Controllers don't know about specific services
- **Testability:** Easy to mock handlers, test in isolation
- **Consistency:** All commands/queries follow same pattern
- **Cross-cutting Concerns:** Pipeline behaviors handle validation, logging, caching
- **Maintainability:** Clear request → handler → response flow

---

## 3. Keycloak Authentication

### What is Keycloak?

**Keycloak** is an open-source identity and access management (IAM) solution that handles user authentication, authorization, and account management. It implements OAuth 2.0 and OpenID Connect standards.

### Why Keycloak?

| Benefit | How Bookify Uses It |
|---------|-------------------|
| **External Auth** | Don't store passwords; Keycloak handles securely |
| **OAuth 2.0/OIDC** | Industry standard, secure token-based auth |
| **Roles & Permissions** | Built-in RBAC (Admin, Owner, Guest) |
| **User Management** | Self-service registration, password reset |
| **Multi-tenant Ready** | Realm isolation for future expansion |
| **Open Source** | Free, no vendor lock-in |

### Keycloak Architecture

```
┌─────────────────────────────────────────────┐
│           Bookify Web/Mobile App            │
└─────────────────────────────────────────────┘
        ↓ (Login Request)        ↑ (JWT Token)
        
┌─────────────────────────────────────────────┐
│         Keycloak Server (Port 8080)         │
│ ┌──────────────────────────────────────┐   │
│ │       Bookify Realm                  │   │
│ ├──────────────────────────────────────┤   │
│ │ • bookify-client (Web app)           │   │
│ │ • bookify-admin (Admin client)       │   │
│ ├──────────────────────────────────────┤   │
│ │ Roles: Admin, Owner, Guest           │   │
│ ├──────────────────────────────────────┤   │
│ │ User Directory                       │   │
│ └──────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

### Authentication Flow in Bookify

#### 1. User Registration

```
Client Request: POST /api/v1/users/register
├─ { email, firstName, lastName, password }
│
API Handler (RegisterUserCommandHandler)
├─ Validate email format and uniqueness
├─ Call AuthenticationService.Register()
│  └─ Call Keycloak Admin API
│     └─ POST /admin/realms/bookify/users
│        ├─ Create user in Keycloak
│        ├─ Set password
│        └─ Return Keycloak User ID
├─ Save User to PostgreSQL
│  ├─ id (UUID)
│  ├─ firstName, lastName, email
│  ├─ identityId (Keycloak ID)
│  └─ createdOn
└─ Return 201 Created (User ID)
```

#### 2. User Login

```
Client Request: POST /api/v1/users/login
├─ { email, password }
│
Keycloak Token Endpoint
├─ POST http://keycloak:8080/realms/bookify/protocol/openid-connect/token
├─ Client credentials (bookify-client)
├─ User credentials (email, password)
│
Keycloak Issues JWT Token
├─ Header: { "alg": "RS256", "typ": "JWT", "kid": "..." }
├─ Payload: {
│    "sub": "user-id",
│    "email": "user@example.com",
│    "email_verified": true,
│    "name": "John Doe",
│    "roles": ["guest", "owner"],
│    "iat": 1682006400,              // Issued at
│    "exp": 1682006400 + 900,        // Expires in 15 mins
│    "aud": "bookify-client"
│  }
└─ Signature: HMAC-SHA256(header.payload, secret)

Return JWT to Client
└─ Store in: localStorage, sessionStorage, or HttpOnly Cookie
```

#### 3. Authenticated Request

```
Client Request with JWT:
GET /api/v1/users/me
Authorization: Bearer eyJhbGciOiJSUzI1NiIs...

API Middleware (JwtBearerAuthentication)
├─ Extract token from Authorization header
├─ Get Keycloak public key
├─ Validate signature (RS256 with public key)
├─ Check expiration (exp claim)
├─ If valid → Parse claims
└─ Populate HttpContext.User

Example Claims:
├─ sub = "550e8400-e29b-41d4-a716-446655440000" (User ID)
├─ email = "john@example.com"
├─ name = "John Doe"
└─ roles = ["guest", "owner"]

Handler processes request
├─ Access User ID: User.FindFirst(ClaimTypes.NameIdentifier)?.Value
├─ Check claims for authorization
└─ Return data
```

### Keycloak Setup for Bookify

**Realm Configuration:**
```yaml
Realm Name: bookify
Enabled: true

Clients:
  - bookify-client
    Enabled: true
    Client Type: Web
    Standard Flow: Enabled
    Authentication Flow: Standard Flow
    Root URL: http://localhost:3000
    
  - bookify-admin
    Enabled: true
    Client Type: Confidential
    Service Account Enabled: true

Roles:
  - admin (Realm role)
  - owner (Realm role)
  - guest (Realm role)

Password Policy:
  - Min Length: 8
  - Uppercase: 1
  - Lowercase: 1
  - Numbers: 1
  - Special Chars: 1
```

### Integration with .NET

```csharp
// Startup Configuration
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://keycloak:8080/realms/bookify";
        options.Audience = "bookify-client";
        options.RequireHttpsMetadata = false;  // Dev only!
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

// Authorization Policies
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("admin"));
    
    options.AddPolicy("RequireOwner", policy =>
        policy.RequireRole("owner"));
});

// Use in Controllers
[Authorize(Policy = "RequireAdmin")]
[HttpPost("apartments/{id}/approve")]
public async Task<IActionResult> ApproveApartment(Guid id)
{
    // Only admin can approve
}
```

### Key Keycloak Endpoints

| Endpoint | Purpose |
|----------|---------|
| `POST /realms/bookify/protocol/openid-connect/token` | Get access token |
| `POST /realms/bookify/protocol/openid-connect/logout` | Revoke token |
| `GET /realms/bookify/protocol/openid-connect/certs` | Get public keys |
| `GET /realms/bookify/protocol/openid-connect/userinfo` | Get user info |
| `POST /admin/realms/bookify/users` | Create user (admin) |
| `GET /admin/realms/bookify/users` | List users (admin) |

---

## 4. Redis Caching

### What is Redis?

**Redis** is an in-memory data structure store that functions as a cache, message broker, and session store. It offers sub-millisecond latency and high throughput.

### Why Redis for Bookify?

**Problem:** 
- Search query on 100,000+ apartments with date filtering = slow
- Database query takes 50-100ms
- Many users search for same date ranges

**Solution with Redis:**
- Cache popular search results
- Return from memory in < 1ms
- Reduce database load by 80%+

### Data Structures in Redis

```
Redis Data Types Used in Bookify:

1. STRING - Simple key-value
   KEY: "apartment:123:details"
   VALUE: {"id": 123, "name": "Cozy Studio", ...}

2. HASH - Object with fields
   KEY: "apartment:123"
   FIELDS: { name, description, price, owner_id, ... }

3. SET - Unique collection
   KEY: "apartment:123:booked_dates:2026-04"
   VALUES: { "2026-04-20", "2026-04-21", "2026-04-22", ... }

4. SORTED SET - Ranked collection
   KEY: "apartments:by_rating"
   VALUES: { (4.5, "apartment:123"), (4.2, "apartment:456"), ... }

5. LIST - Ordered collection
   KEY: "search_queries:recent"
   VALUES: [...recent searches...]
```

### Caching Strategy in Bookify

```
Search Request
│
├─ Generate cache key
│  key = "search:apartments:2026-04-25:2026-04-30:minPrice=100:maxPrice=500"
│
├─ Check Redis (< 1ms)
│  redis.Get(key)
│
├─ Cache HIT
│  └─ Deserialize JSON → Return (< 1ms)
│
├─ Cache MISS
│  ├─ Query PostgreSQL (30-50ms)
│  │  SELECT * FROM apartments WHERE...
│  ├─ Serialize JSON
│  ├─ Store in Redis with TTL (1 hour)
│  │  redis.SetEx(key, 3600, jsonData)
│  └─ Return response (< 100ms total)
│
└─ Total Response Time: HIT < 1ms, MISS < 100ms
```

### Redis Methods in .NET

```csharp
using StackExchange.Redis;

public class CacheService
{
    private readonly IDatabase _db;
    
    public CacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }
    
    // SET - Store with TTL (Time To Live)
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }
    
    // GET - Retrieve value
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNull ? null : JsonSerializer.Deserialize<T>(value.ToString());
    }
    
    // DELETE - Remove key
    public async Task DeleteAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
    
    // INVALIDATE - Remove pattern (e.g., all apartment searches)
    public async Task InvalidatePatternAsync(string pattern)
    {
        var keys = _db.GetServer(_db.Multiplexer.GetEndPoints().First())
            .Keys(pattern: pattern);
        await _db.KeyDeleteAsync(keys.ToArray());
    }
}
```

### MediatR Pipeline Behavior for Caching

```csharp
public class QueryCachingBehavior<in TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableQuery
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request, 
        NextDelegate<TResponse> next, 
        IPipelineBehaviorContext context)
    {
        // Check if query should be cached
        if (!request.UseCache)
            return await next();
        
        // Generate cache key
        var cacheKey = request.CacheKey;
        
        // Try to get from cache
        var cachedResult = await _cacheService.GetAsync<TResponse>(cacheKey);
        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
            return cachedResult;
        }
        
        // Cache miss - execute handler
        _logger.LogInformation("Cache MISS for {CacheKey}", cacheKey);
        var result = await next();
        
        // Store in cache
        await _cacheService.SetAsync(cacheKey, result, request.CacheDuration);
        
        return result;
    }
}

// Query interface for caching
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
    bool UseCache { get; }
}

// Query implementation
public class SearchApartmentsQuery : IRequest<Result<List<ApartmentDto>>>, ICacheableQuery
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    
    public string CacheKey =>
        $"search:apartments:{StartDate:yyyy-MM-dd}:{EndDate:yyyy-MM-dd}:" +
        $"minPrice={MinPrice}:maxPrice={MaxPrice}";
    
    public TimeSpan CacheDuration => TimeSpan.FromHours(1);
    public bool UseCache => true;
}
```

### Cache Invalidation Strategies

```csharp
public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICacheService _cacheService;
    
    public async Task<Result> Handle(ConfirmBookingCommand request, CancellationToken ct)
    {
        // Confirm the booking
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        booking.Confirm();
        await _bookingRepository.UpdateAsync(booking);
        
        // INVALIDATE CACHE - dates now unavailable
        // Remove search results that include this apartment/date combo
        var cachePattern = $"search:apartments:*:*:{request.ApartmentId}:*";
        await _cacheService.InvalidatePatternAsync(cachePattern);
        
        // Also invalidate apartment details cache
        await _cacheService.DeleteAsync($"apartment:{request.ApartmentId}:details");
        
        return Result.Success();
    }
}
```

### Redis in Docker

```yaml
# docker-compose.yml
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    volumes:
      - redis_data:/data

# Startup code
var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
services.AddSingleton(multiplexer);
services.AddScoped<ICacheService, RedisService>();
```

---

## 5. Docker Containerization

### What is Docker?

**Docker** is a containerization platform that packages applications with all dependencies (libraries, runtime, config) into an isolated "container" that runs consistently everywhere.

### Problem Docker Solves

**Without Docker:**
```
Developer Machine
├─ .NET 8
├─ PostgreSQL 15
├─ Redis 7
├─ Java 21 (for Keycloak)
└─ Works on my machine!

CI/CD Server
├─ .NET 7 (different version!)
├─ PostgreSQL 13
├─ Redis 6
└─ Build fails, dependency conflicts
```

**With Docker:**
```
Dockerfile (Recipe)
├─ FROM mcr.microsoft.com/dotnet/sdk:8
├─ COPY source files
├─ RUN dotnet restore
├─ RUN dotnet build
├─ ENTRYPOINT ["dotnet", "Bookify.API.dll"]
└─ Builds same image everywhere
```

### Docker Core Concepts

```
┌─────────────────────┐
│     Dockerfile      │  (Recipe/Instructions)
│  FROM ...           │
│  RUN ...            │
│  COPY ...           │
└──────────┬──────────┘
           ↓
    ┌─────────────┐
    │ Docker Image│  (Executable snapshot)
    │ bookify:1.0 │  (Read-only)
    └──────┬──────┘
           ↓
    ┌──────────────┐
    │  Container   │  (Running instance)
    │ bookify:prod │  (Writable layer)
    └──────────────┘
```

### Dockerfile for Bookify

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8 AS build

WORKDIR /src

# Copy solution and project files
COPY ["Bookify.sln", "./"]
COPY ["src/Bookify.API/Bookify.API.csproj", "src/Bookify.API/"]
COPY ["src/Bookify.Application/Bookify.Application.csproj", "src/Bookify.Application/"]
COPY ["src/Bookify.Domain/Bookify.Domain.csproj", "src/Bookify.Domain/"]
COPY ["src/Bookify.Infrastructure/Bookify.Infrastructure.csproj", "src/Bookify.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Bookify.sln"

# Copy all source code
COPY . .

# Build application
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8

WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Security: Run as non-root
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet /app/HealthChecks.dll

# Start application
ENTRYPOINT ["dotnet", "Bookify.API.dll"]
```

### Multi-stage Build Benefits

```
Stage 1: Build
├─ Download SDK (500MB)
├─ Restore NuGet packages
├─ Compile code
└─ Output: build artifacts

Stage 2: Publish
├─ Publish for release
└─ Output: deployment files

Stage 3: Runtime
├─ Start with clean runtime image (200MB)
├─ Copy only publish output
└─ Result: Slim final image (450MB)

WITHOUT multi-stage: 700MB image
WITH multi-stage: 450MB image
```

### Docker Compose - Local Development

```yaml
# docker-compose.yml
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: bookify
      POSTGRES_USER: bookify_user
      POSTGRES_PASSWORD: bookify_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U bookify_user"]
      interval: 5s
      timeout: 5s
      retries: 5

  # Redis Cache
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5

  # Keycloak Identity Provider
  keycloak:
    image: keycloak/keycloak:latest
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: keycloak_user
      KC_DB_PASSWORD: keycloak_password
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
    command:
      - start-dev

  # Bookify API
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    environment:
      ConnectionStrings__DefaultConnection: "Server=postgres;Port=5432;Database=bookify;User Id=bookify_user;Password=bookify_password;"
      Redis__ConnectionString: "redis:6379"
      Keycloak__Authority: "http://keycloak:8080/realms/bookify"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      keycloak:
        condition: service_started

volumes:
  postgres_data:
  redis_data:
```

### Common Docker Commands

```bash
# Build image
docker build -t bookify:1.0 .

# Run container
docker run -p 5000:5000 bookify:1.0

# View running containers
docker ps

# View logs
docker logs container_id

# Start/Stop
docker start container_id
docker stop container_id

# Docker Compose (all services)
docker-compose up -d      # Start in background
docker-compose logs -f    # View logs
docker-compose down       # Stop all services
```

---

## 6. Kubernetes Orchestration

### What is Kubernetes?

**Kubernetes** (K8s) is a container orchestration platform that automates deployment, scaling, and management of containerized applications.

### Problem Kubernetes Solves

**Manual Management:**
```
10 API instances + 5 Database replicas + Caching layer + Load Balancer
├─ Server crashes → Manual restart
├─ High traffic → Manual scale up
├─ New version → Manual rolling update
├─ Failed pod → Manual replacement
└─ Disaster recovery → Manual failover
```

**Kubernetes Automation:**
```
Define desired state
├─ "Run 3 instances of Bookify API"
├─ "Restart if any fails"
├─ "Scale to 10 if CPU > 80%"
└─ "Update to new version smoothly"

Kubernetes ensures actual state matches desired state
```

### Kubernetes Architecture

```
┌─────────────────────────── Kubernetes Cluster ───────────────────────────┐
│                                                                            │
│  ┌──────────────────────── Master (Control Plane) ───────────────────┐   │
│  │                                                                    │   │
│  │  API Server      Scheduler      Controller Manager              │   │
│  │  (Endpoint)      (Pod placement) (Self-healing)                │   │
│  └────────────────────────────────────────────────────────────────┘   │
│                                                                            │
│  ┌──────────── Worker Node 1 ─────────┐  ┌──────────── Worker Node 2 ──┐ │
│  │                                     │  │                             │ │
│  │  ┌── Pod ──────────────────────┐  │  │  ┌── Pod ──────────────────┐ │ │
│  │  │ Bookify API Container       │  │  │  │ Bookify API Container  │ │ │
│  │  │ Port 5000                   │  │  │  │ Port 5000              │ │ │
│  │  └─────────────────────────────┘  │  │  └────────────────────────┘ │ │
│  │                                     │  │                             │ │
│  │  ┌── Pod ──────────────────────┐  │  │  ┌── Pod ──────────────────┐ │ │
│  │  │ Postgres Instance           │  │  │  │ Redis Instance         │ │ │
│  │  └─────────────────────────────┘  │  │  └────────────────────────┘ │ │
│  │                                     │  │                             │ │
│  └────────────────────────────────────┘  └─────────────────────────────┘ │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### Key Kubernetes Objects for Bookify

#### 1. Deployment (Manage API Replicas)

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bookify-api
  namespace: default
spec:
  replicas: 3  # Always run 3 instances
  selector:
    matchLabels:
      app: bookify-api
  
  template:
    metadata:
      labels:
        app: bookify-api
    
    spec:
      containers:
      - name: api
        image: bookify:1.0
        ports:
        - containerPort: 5000
        
        # Environment variables
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: bookify-secrets
              key: db-connection-string
        
        # Resource limits
        resources:
          requests:
            cpu: "250m"
            memory: "256Mi"
          limits:
            cpu: "500m"
            memory: "512Mi"
        
        # Health checks
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 10
        
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
```

#### 2. Service (Expose API)

```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: bookify-api-service
spec:
  type: LoadBalancer  # Expose to internet
  ports:
  - port: 80          # External port
    targetPort: 5000  # Container port
    protocol: TCP
  
  selector:
    app: bookify-api
```

#### 3. ConfigMap (Configuration)

```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: bookify-config
data:
  LOG_LEVEL: "Information"
  CACHE_TTL: "3600"
  KEYCLOAK_REALM: "bookify"
```

#### 4. Secret (Sensitive Data)

```yaml
# k8s/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: bookify-secrets
type: Opaque
data:
  db-connection-string: U2VydmVyPXBvc3RncmVzOy4uLiA=  # base64 encoded
  db-password: Ym9va2lmeV9wYXNzd29yZA==
  keycloak-secret: bXlzZWNyZXR0b2tlbg==
```

#### 5. Ingress (URL Routing)

```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: bookify-ingress
spec:
  rules:
  - host: api.bookify.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: bookify-api-service
            port:
              number: 80
```

### Kubernetes Deployment Process

```
1. Build Image
   docker build -t bookify:2.0 .
   docker push registry.azurecr.io/bookify:2.0

2. Update Deployment
   kubectl set image deployment/bookify-api \
     api=registry.azurecr.io/bookify:2.0

3. Kubernetes Rolling Update
   ├─ Start new pod with v2.0
   ├─ Wait for health checks to pass
   ├─ Shift traffic to v2.0
   ├─ Stop old pod with v1.0
   ├─ Repeat for all 3 replicas
   └─ Zero downtime!

4. Verify Deployment
   kubectl get deployment bookify-api
   kubectl get pods
   kubectl logs -f deployment/bookify-api
```

### Kubernetes Scaling

```
# Automatic Scaling based on CPU
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bookify-api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bookify-api
  
  minReplicas: 3
  maxReplicas: 10
  
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 80  # Scale up if CPU > 80%
  
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 85  # Scale up if memory > 85%

# Result:
  1000 users    → 3 instances
  5000 users    → 5 instances
  10000 users   → 10 instances (max)
```

---

## 7. CI/CD Pipeline

### What is CI/CD?

**CI (Continuous Integration):** Automatically test code after every push  
**CD (Continuous Deployment):** Automatically deploy to production after tests pass

### CI/CD Pipeline Benefits

```
Without CI/CD:
1. Developer codes locally
2. Commits to Git
3. Manual testing by QA (2 days)
4. Manual deployment to staging (30 mins)
5. Manual deployment to production (1 hour)
Total: 3 days to production!

With CI/CD:
1. Developer codes locally
2. Push to Git
3-8. Automatic: Build → Test → Security Scan → Deploy to Staging → Integration Tests → Deploy to Production
Total: 5 minutes!
```

### GitHub Actions Pipeline for Bookify

```yaml
# .github/workflows/deploy.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  # Job 1: Build and Test
  build-and-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: password
          POSTGRES_DB: bookify_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
    
    steps:
    # Step 1: Checkout code
    - uses: actions/checkout@v3
    
    # Step 2: Setup .NET
    - uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    # Step 3: Restore dependencies
    - name: Restore dependencies
      run: dotnet restore
    
    # Step 4: Build
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    # Step 5: Run Unit Tests
    - name: Run Unit Tests
      run: dotnet test tests/Bookify.UnitTests --configuration Release --no-build
    
    # Step 6: Run Integration Tests
    - name: Run Integration Tests
      run: dotnet test tests/Bookify.IntegrationTests --configuration Release --no-build
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost;Port=5432;Database=bookify_test;User Id=postgres;Password=password;"
    
    # Step 7: Code Coverage
    - name: Generate Coverage Report
      run: dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
    
    # Step 8: Print Coverage
    - name: Comment Coverage
      uses: romeovs/lcov-reporter-action@v0.3.1
      with:
        lcov-file: ./coverage.lcov

  # Job 2: Security Scanning
  security-scan:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        path: './'
        format: 'JSON'
    
    - name: Upload Results
      uses: github/codeql-action/upload-sarif@v2
      if: always()

  # Job 3: Build Docker Image
  build-docker:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2
    
    - name: Build Docker Image
      run: |
        docker build \
          -t bookify:${{ github.sha }} \
          -t bookify:latest \
          .
    
    - name: Login to Docker Registry
      run: |
        echo "${{ secrets.DOCKER_PASSWORD }}" | \
        docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
    
    - name: Push to Registry
      run: |
        docker push bookify:${{ github.sha }}
        docker push bookify:latest

  # Job 4: Deploy to Production
  deploy-prod:
    runs-on: ubuntu-latest
    needs: build-docker
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/bookify-api \
          api=bookify:${{ github.sha }} \
          --namespace=production
      env:
        KUBECONFIG: ${{ secrets.KUBECONFIG }}
    
    - name: Verify Deployment
      run: |
        kubectl rollout status deployment/bookify-api \
          --namespace=production \
          --timeout=5m
```

### Pipeline Flow Visualization

```
Developer Push to Main
        ↓
├─ Build & Test (parallel)
│  ├─ Restore packages
│  ├─ Build solution
│  ├─ Unit tests (5 mins)
│  └─ Integration tests (10 mins)
│
├─ Security Scan (parallel)
│  ├─ OWASP dependency check
│  └─ Code vulnerability scan
│
├─ Build Docker Image (depends on Build & Test)
│  ├─ Multi-stage build
│  └─ Push to registry
│
└─ Deploy to Production (depends on Docker)
   ├─ Update Kubernetes deployment
   ├─ Rolling update starts
   ├─ Health checks pass
   └─ Live in production!

Total time: ~20 minutes (tests run in parallel)
```

### Deployment Strategies

#### Blue-Green Deployment
```
Blue (v1.0 - Current)
├─ 3 replicas running
├─ Receiving all traffic
└─ Stable in production

Green (v2.0 - New)
├─ 3 replicas running
├─ Running tests
└─ Not receiving traffic

Validation passes:
├─ Switch traffic to Green
├─ Blue becomes standby
└─ Instant rollback if needed
```

#### Canary Deployment
```
v1.0 - 95% of traffic (2.85 out of 3 replicas)
v2.0 - 5% of traffic (0.15 of 1 replica)

Monitor for 1 hour:
├─ Error rate increase?
├─ Response time increase?
└─ Resource usage increase?

If good:
├─ Increase to 50/50
├─ Monitor 1 hour
├─ Increase to 100 v2.0
└─ Remove v1.0

If issues:
├─ Rollback to 100% v1.0
├─ Investigate v2.0
└─ Fix and retry
```

---

## 8. Integration Overview

### How All Pieces Work Together

```
┌─────────────────────────────────────────────────────────────┐
│                    User's Web Browser                       │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           │ HTTP Request + JWT Token
                           ↓
┌──────────────────────────────────────────────────────────────┐
│              CI/CD Pipeline (GitHub Actions)                 │
│  • Auto-builds on push                                       │
│  • Runs tests                                                │
│  • Builds Docker image                                       │
│  • Deploys to Kubernetes                                     │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ↓
┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┐
│         Kubernetes Cluster (Production)                      │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            Load Balancer / Ingress                   │   │
│  │         (Distributes traffic)                        │   │
│  └──────────────┬─────────────────────────────────────┘    │
│                 │                                            │
│  ┌──────────────┴─────────────────────────────────────┐     │
│  │                                                     │     │
│  │  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐│     │
│  │  │  Pod 1      │  │  Pod 2      │  │  Pod 3       ││     │
│  │  │ Bookify API │  │ Bookify API │  │ Bookify API  ││     │
│  │  │ Container   │  │ Container   │  │ Container    ││     │
│  │  │Port 5000    │  │Port 5000    │  │Port 5000     ││     │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬───────┘│     │
│  │         │                 │                 │         │     │
│  │         └─────────────────┼─────────────────┘         │     │
│  │                           │                           │     │
│  └───────────────────────────┼───────────────────────────┘     │
│                              │                                  │
└──────────────────────────────┼──────────────────────────────────┘
                               │
                 ┌─────────────┼─────────────┐
                 │             │             │
                 ↓             ↓             ↓
         ┌─────────────┐ ┌──────────┐ ┌─────────────┐
         │ PostgreSQL  │ │  Redis   │ │  Keycloak   │
         │  Database   │ │  Cache   │ │  Auth       │
         │             │ │          │ │             │
         │ • Users     │ │ • Search │ │ • Tokens    │
         │ • Bookings  │ │ • Query  │ │ • Users     │
         │ • Reviews   │ │ • Results│ │ • Roles     │
         └─────────────┘ └──────────┘ └─────────────┘
```

### Request Flow with All Concepts

#### Example: Make a Booking

```
1. FRONTEND
   User clicks "Book Apartment"
   ├─ Has JWT token from Keycloak login
   └─ POST /api/v1/bookings
      {
        "apartmentId": "123",
        "startDate": "2026-05-01",
        "endDate": "2026-05-05"
      }

2. KUBERNETES
   Request hits load balancer
   ├─ Distributed to one of 3 API pods
   └─ Container starts processing

3. .NET API (MediatR Command)
   ├─ API receives POST request
   ├─ Validates JWT token (Keycloak public key)
   │  └─ Extracts user ID from claims
   ├─ Creates ReserveBookingCommand
   │  └─ MediatR pipeline triggers
   ├─ ValidationBehavior (FluentValidation)
   ├─ LoggingBehavior (Serilog)
   └─ ReserveBookingCommandHandler
      │
      ├─ Fetch apartment from PostgreSQL
      │  └─ Check owner, price, amenities
      │
      ├─ Check for booking conflicts
      │  └─ Query bookings table
      │
      ├─ Calculate total price
      │  └─ (nights × nightly) + cleaning + amenities
      │
      ├─ Create Booking aggregate
      │  └─ booking.Reserve()
      │
      ├─ Persist to PostgreSQL (EF Core)
      │  └─ ACID transaction
      │
      ├─ Raise BookingReservedDomainEvent
      │  └─ Add to outbox table
      │  
      └─ Return 201 Created
         {
           "bookingId": "abc123",
           "totalPrice": 450.00
         }

4. BACKGROUND PROCESSING (Quartz Job)
   ├─ OutboxProcessor runs every 5 seconds
   ├─ Finds unprocessed outbox messages
   ├─ Deserializes BookingReservedDomainEvent
   ├─ Publishes to event handlers
   │  ├─ EmailService
   │  │  ├─ Send confirmation to guest
   │  │  └─ Send notification to owner
   │  ├─ NotificationService
   │  │  └─ Push notification
   │  └─ AnalyticsService
   │     └─ Record booking event
   └─ Mark message as processed

5. CACHING (Redis) - INVALIDATION
   ├─ Booking confirmed changes dates
   ├─ Search caches must be invalidated
   └─ Pattern: "search:apartments:*:*:123:*"
      └─ Next search will query DB and recache

6. RESPONSE
   Browser receives 201 Created
   ├─ User sees "Booking confirmed!"
   ├─ Confirmation email arrives (async)
   └─ Owner receives notification
```

#### Example: Search Apartments

```
1. FRONTEND
   User filters apartments for May 1-5
   ├─ Has valid JWT token
   └─ GET /api/v1/apartments?startDate=2026-05-01&endDate=2026-05-05

2. KUBERNETES
   Request hits load balancer
   └─ Distributed to one of 3 API pods

3. .NET API (MediatR Query with Caching)
   ├─ API validates JWT
   ├─ Creates SearchApartmentsQuery
   └─ MediatR pipeline triggers
      │
      ├─ ValidationBehavior
      ├─ LoggingBehavior
      ├─ QueryCachingBehavior (CACHE CHECK)
      │  │
      │  ├─ Generate cache key:
      │  │  "search:apartments:2026-05-01:2026-05-05"
      │  │
      │  ├─ Check Redis
      │  │  ├─ Cache HIT → Return cached JSON (< 1ms)
      │  │  └─ Cache MISS → Continue to handler
      │  │
      │  └─ SearchApartmentsQueryHandler
      │     ├─ Query PostgreSQL with Dapper
      │     │  SQL:
      │     │  SELECT apartments.* FROM apartments
      │     │  LEFT JOIN bookings ON ...
      │     │  WHERE apartments.deletedOn IS NULL
      │     │  AND NOT EXISTS (
      │     │    SELECT 1 FROM bookings
      │     │    WHERE bookings.apartment_id = apartments.id
      │     │    AND bookings.status IN ('Reserved', 'Confirmed')
      │     │    AND bookings.duration_end >= '2026-05-01'
      │     │    AND bookings.duration_start < '2026-05-05'
      │     │  )
      │     │  LIMIT 20
      │     │
      │     ├─ Result: 5 apartments match
      │     │  ├─ Studio (150€/night)
      │     │  ├─ 2-Bedroom (250€/night)
      │     │  ├─ Penthouse (500€/night)
      │     │  └─ ...
      │     │
      │     ├─ Cache result (TTL: 1 hour)
      │     │  redis.SetEx("search:...", 3600, json)
      │     │
      │     └─ Return response (< 100ms)

4. RESPONSE
   Browser receives 200 OK
   [
     {
       "id": "apt1",
       "name": "Cozy Studio",
       "price": 150.00,
       "amenities": ["WiFi", "AC"],
       "rating": 4.8
     },
     ...
   ]
```

---

## Key Takeaways

| Concept | Purpose | In Bookify |
|---------|---------|-----------|
| **CQRS** | Separate reads from writes | Commands handle bookings, Queries handle searches |
| **MediatR** | Decouple controllers from services | All requests go through MediatR pipeline |
| **Keycloak** | Secure authentication | OAuth 2.0 tokens for API access |
| **Redis** | Fast caching | Cache apartment searches (1-hour TTL) |
| **Docker** | Consistent containers | Same image runs everywhere (dev, staging, prod) |
| **Kubernetes** | Orchestrate containers | Auto-scale, self-heal, rolling updates |
| **CI/CD** | Automate testing & deployment | Github Actions: push → test → build → deploy |

All concepts work together to create a **scalable, maintainable, production-ready** booking platform.

---

**This document was created as a learning reference for the Bookify project.**  
**Last Updated:** April 19, 2026
