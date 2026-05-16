# Bookify - Cloud Agent Instructions

**Bookify** is an enterprise .NET 10 apartment rental booking platform using Clean Architecture (CQRS + DDD). Current phase: Week 5-6 (Apartment Catalog).

## Quick Start Commands

**ALWAYS run these in order after cloning:**
```bash
# 1. Start Docker infrastructure (PostgreSQL 15, Redis 7, Keycloak 23)
docker-compose up -d

# 2. Restore and build solution
dotnet restore
dotnet build Bookify.sln

# 3. Run all tests (xUnit + NSubstitute + FluentAssertions)
dotnet test Bookify.sln

# 4. Run API with watch mode (auto-recompile on changes)
dotnet watch run --project src/Bookify.API/Bookify.API.csproj
```

**API runs on**: https://localhost:5000 | **Swagger**: https://localhost:5000/swagger | **Health**: https://localhost:5000/health

## Critical Prerequisites

- **.NET 10 SDK** (verify: `dotnet --version`)
- **Docker Desktop** with Docker Compose (for PostgreSQL 15, Redis 7, Keycloak 23)
- Git

**Docker must be running** before building/testing. If you see connection errors, verify:
```bash
docker ps  # Should list bookify-postgres, bookify-redis, bookify-keycloak
docker-compose logs  # Check service health
```

## Project Structure & Key Files

**Solution**: `Bookify.sln` (4 source + 3 test projects)

| Layer | Path | Purpose |
|-------|------|---------|
| **API** | `src/Bookify.API/` | Controllers, middleware, Program.cs entry point |
| **Application** | `src/Bookify.Application/` | CQRS handlers, commands, queries, DTOs, validators |
| **Domain** | `src/Bookify.Domain/` | Entities, value objects, domain events, Entity.cs base |
| **Infrastructure** | `src/Bookify.Infrastructure/` | EF Core, Dapper, Redis, Keycloak, external services |

**Tests** (in `tests/`):
- `Bookify.UnitTests/` – Domain/service logic
- `Bookify.IntegrationTests/` – Database + external services  
- `Bookify.ArchitectureTests/` – Layer boundaries, namespaces

**Config files**:
- `Directory.Build.props` – Global: net10.0, nullable, code analyzers enabled
- `.editorconfig` – Code style: 4-space indent, PascalCase, interface prefix 'I'
- `docker-compose.yml` – Local dev: PostgreSQL (5432), Redis (6379), Keycloak (8080)

## Build & Test Validation

**Build** (no errors/warnings):
```bash
dotnet build Bookify.sln
```

**Test** (all tests must pass):
```bash
dotnet test Bookify.sln --verbosity normal
```

Expected: 3 test projects run, no failures. Test timeout: ~30 seconds.

**Code Quality**:
- `.editorconfig` enforced (4-space indent, PascalCase types/methods, interface 'I' prefix)
- Analyzers enabled in build: `EnforceCodeStyleInBuild=true`
- Unused parameters trigger warnings: `CodeQualityUnusedParameters=all:warning`

## Dependencies & Versions (Locked)

| Component | Version | Purpose |
|-----------|---------|---------|
| MediatR | 11.1.0 | CQRS command/query pipeline |
| AutoMapper | 12.0.1 | DTO mapping |
| FluentValidation | 11.7.1 | Input validation |
| Entity Framework Core | 8.0.0 | ORM (PostgreSQL) |
| Dapper | 2.1.0 | Optimized queries |
| Npgsql.EFCore.PostgreSQL | 8.0.0 | PostgreSQL provider |
| StackExchange.Redis | 2.6.122 | Redis caching |
| Serilog | 3.1.1 | Logging |
| Quartz | 3.7.0 | Background jobs |
| xUnit | 2.6.2 | Unit testing |
| NSubstitute | 5.1.0 | Mocking |
| FluentAssertions | 6.12.0 | Assertions |

## Architecture Patterns (Enforce Strictly)

### CQRS + MediatR
- **Commands** (state-changing): `Features/[Feature]/Commands/Create[Feature]Command.cs`
- **Queries** (read-only): `Features/[Feature]/Queries/Get[Feature]Query.cs`
- Handlers must be in same folder with `Handler` suffix
- All public commands/queries need validators with `Validator` suffix
- Pipeline: MediatR → Validation → Logging → Handler

### Domain-Driven Design
- **Entities**: Inherit from `Entity` (in `Domain/Entity.cs`), have `Guid Id`
- **Value Objects**: Immutable, no identity, no inheritance
- **Domain Events**: Derived from event base class, raised in aggregates
- **Repositories**: Abstract interface in Domain, implementation in Infrastructure
- **No dependencies** from Domain/Application → Infrastructure/API

### Clean Architecture Dependency Rule
```
API → Application → Domain
Infrastructure → Application → Domain
(No backward dependencies allowed)
```

## API Endpoints & Testing

### Health & Swagger
```
GET  /health              # Health check
GET  /swagger             # API documentation
```

### User Management (Phase 1 ✅)
```
POST  /api/v1/users/register    # Register with Keycloak
POST  /api/v1/users/login       # Login returns JWT
GET   /api/v1/users/me          # Requires auth bearer token
GET   /api/v2/users/me          # Extended info (v2)
```

### Apartment Catalog (Phase 2 🔄)
```
GET   /api/v1/apartments                # Search + filters + pagination
GET   /api/v1/apartments/{id}           # Get details
POST  /api/v1/apartments                # Create (auth required)
PUT   /api/v1/apartments/{id}           # Update (owner only)
DELETE /api/v1/apartments/{id}          # Delete (owner only)
```

Test with JWT Bearer token in Authorization header: `Authorization: Bearer {token}`

## Database & Caching

**PostgreSQL 15** (bookify-postgres:5432):
- User: `bookify` | Password: `secure_password`
- Database: `bookify`
- Migrations via EF Core: `dotnet ef database update`

**Redis 7** (bookify-redis:6379):
- Used for apartment search caching (1-hour TTL)
- Target hit ratio: >80%

**Keycloak 23** (bookify-keycloak:8080):
- Admin: `admin:admin`
- OpenID Connect provider for JWT tokens
- Required for user authentication flows

## Code Standards & Validation

**Naming**:
- PascalCase: `Booking`, `IApartmentRepository`, `GetApartmentById()`
- camelCase: parameters, local variables (`apartmentId`, `userEmail`)
- UPPER_CASE_WITH_UNDERSCORE: constants (`MAX_BOOKING_DAYS = 365`)
- Interface prefix: `I` (e.g., `IRepository`, `IUnitOfWork`)

**File Organization** (strict):
```
[Layer]/[Feature]/
├── Commands/
│   ├── CreateXxxCommand.cs
│   ├── CreateXxxCommandHandler.cs
│   └── CreateXxxCommandValidator.cs
├── Queries/
│   ├── GetXxxQuery.cs
│   └── GetXxxQueryHandler.cs
└── DTOs/
    ├── CreateXxxRequest.cs
    └── XxxResponse.cs
```

## Before Making Any Changes

**IMPORTANT: Sensitive Areas Require Explicit Permission**

Before modifying any of the following areas, **ALWAYS ask the user for explicit permission and wait for approval**:

- **Database schema** (entities, migrations, relationships)
- **Authentication & Authorization** (Keycloak integration, JWT handling, permissions)
- **Domain entities & aggregate roots** (business logic, invariants, domain events)
- **API contracts** (request/response DTOs, endpoint signatures)
- **Critical dependencies** (MediatR, EF Core, Keycloak client versions)
- **Pricing calculations** (financial logic must be verified)
- **Outbox pattern / Event processing** (reliability-critical)
- **Payment-related code** (any transaction, booking confirmation logic)

**When Uncertain, Refer to the Plan:**
Always prefer guidance from `docs/BOOKIFY_IMPLEMENTATION_PLAN.md` before implementing. This document contains:
- REQ-001 through REQ-012: Detailed acceptance criteria
- Phase-by-phase breakdown (Weeks 1-12)
- Technical specifications and constraints
- Risk mitigation strategies
- Database schema definition

## Make Changes Safely

1. **Before editing**: `dotnet build` should pass with no warnings
2. **After editing**: Always run `dotnet test Bookify.sln` (must pass)
3. **Verify code style**: Build enforces .editorconfig rules (`EnforceCodeStyleInBuild=true`)
4. **Check dependencies**: Do not upgrade package versions without justification
5. **Domain events**: Raise in aggregate root, process via outbox pattern (Quartz job)
6. **Database changes**: Create EF Core migration: `dotnet ef migrations add [Description] -p src/Bookify.Infrastructure -s src/Bookify.API`
7. **Sensitive changes**: Ask user for explicit permission before modifying sensitive areas (see section above)

## Known Configurations & Workarounds

**Docker containers must be healthy**:
- PostgreSQL: `pg_isready` check, waits 10s between retries
- Redis: `redis-cli ping` check
- Keycloak: HTTP health endpoint check
- Wait 15-30 seconds after `docker-compose up` before building/testing

**Build hints**:
- Enable NuGet restore cache if available
- Parallel build: `dotnet build -m` (uses all CPU cores)
- Clean cache if build fails: `dotnet clean && dotnet restore`

**Tests**:
- Integration tests require Docker services running
- Unit tests run in isolation (fast, <5 seconds)
- Target coverage: >80% across domain/application layers

## Common Troubleshooting

**Docker containers must be healthy**:
- PostgreSQL: `pg_isready` check, waits 10s between retries
- Redis: `redis-cli ping` check
- Keycloak: HTTP health endpoint check
- Wait 15-30 seconds after `docker-compose up` before building/testing

**Port Conflicts**:
```bash
# If ports already in use, check:
netstat -ano | findstr :5000   # API port
netstat -ano | findstr :5432   # PostgreSQL
netstat -ano | findstr :6379   # Redis
netstat -ano | findstr :8080   # Keycloak
```

**Database Issues**:
```bash
# Reset database
dotnet ef database drop
dotnet ef database update

# View migrations
dotnet ef migrations list
```

**Docker Stack Issues**:
```bash
# Restart services
docker-compose down
docker-compose up -d

# View logs
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f keycloak
```

## Important Reference Files

- **docs/BOOKIFY_IMPLEMENTATION_PLAN.md** - Complete project specification
- **docs/CONCEPTS_AND_PATTERNS.md** - Architecture deep-dive
- **docs/scripts.md** - Useful SQL and deployment scripts
- **src/Bookify.API/Program.cs** - Dependency injection & middleware setup
- **src/Bookify.Domain/Entity.cs** - Base entity class

---

**Last Updated**: May 16, 2026 | **Trust these instructions and only search if information is incomplete or in error.**
