# Bookify - Enterprise Apartment Rental Booking Platform

An enterprise-grade .NET 10 apartment rental booking system built with **Clean Architecture**, **CQRS**, and **Domain-Driven Design**.

## 🚀 Quick Start

### Prerequisites
- **.NET 10 SDK** (verify: `dotnet --version`)
- **Docker Desktop** (PostgreSQL 15, Redis 7)
- **Git**

### Installation & Setup

```bash
# 1. Clone and navigate to project
git clone <repo-url>
cd Bookify-Personal

# 2. Start Docker infrastructure
docker-compose up -d

# 3. Restore and build solution
dotnet restore
dotnet build Bookify.sln

# 4. Run all tests
dotnet test Bookify.sln

# 5. Start API with watch mode
dotnet watch run --project src/Bookify.API/Bookify.API.csproj
```

**API Endpoints:**
- 🌐 **API**: https://localhost:5000
- 📚 **Swagger**: https://localhost:5000/swagger
- ❤️ **Health**: https://localhost:5000/health

---

## 📋 Implementation Plan Summary

### **Project Overview**
Bookify is a production-ready, multi-tenant apartment rental booking platform demonstrating enterprise-level .NET development. It implements CQRS for optimized operations, rich domain modeling with value objects, and comprehensive security using Google OAuth 2.0 and JWT.

### **Phase Breakdown (12 Weeks)**

| Phase | Duration | Focus | Status |
|-------|----------|-------|--------|
| **Phase 0** | Weeks 1-2 | Foundation Setup | ✅ Complete |
| **Phase 1** | Weeks 3-4 | User & Auth (Registration, Login) | ✅ Complete |
| **Phase 2** | Weeks 5-6 | Apartment Catalog (CRUD, Search, Filtering) | 🔄 In Progress |
| **Phase 3** | Weeks 7-8 | Booking Management (Reservation, Confirmation) | ⏳ Planned |
| **Phase 4** | Weeks 9-10 | Review System & Analytics | ⏳ Planned |
| **Phase 5** | Weeks 11-12 | Performance, Security Hardening, Deployment | ⏳ Planned |

---

## 🎯 Key Functional Features

### **1. User Management (REQ-001 to REQ-003)**
- ✅ User registration with email validation
- ✅ JWT-based authentication via Google OAuth 2.0
- ✅ Role-Based Access Control (RBAC): Admin, Owner, Guest

### **2. Apartment Management (REQ-004 to REQ-006)**
- 🔄 Apartment registration with metadata (name, price, amenities, address)
- 🔄 Advanced search with date availability, price range, amenity filters
- 🔄 Pagination & Redis caching (1-hour TTL, >80% hit ratio target)

### **3. Booking Management (REQ-007 to REQ-010)**
- ⏳ Booking reservation with overlap detection
- ⏳ Dynamic pricing (base + cleaning fee + surcharges)
- ⏳ Owner confirmation/rejection workflow
- ⏳ Booking history with resource-based authorization

### **4. Review System (REQ-011 to REQ-012)**
- ⏳ Reviews restricted to completed bookings only
- ⏳ 5-star ratings with optional comments (max 500 chars)
- ⏳ Apartment average rating calculation
- ⏳ Domain events for audit trail

### **5. API Versioning (REQ-013)**
- ✅ Dual version support: `/api/v1/` & `/api/v2/`
- ✅ Full Swagger/OpenAPI documentation

---

## 🏗️ Architecture & Technical Stack

### **Layered Architecture**
```
API Layer (Controllers, Middleware)
    ↓
Application Layer (CQRS Handlers, Validators, DTOs)
    ↓
Domain Layer (Entities, Value Objects, Domain Events)
    ↓
Infrastructure Layer (EF Core, Dapper, Redis, Google OAuth 2.0)
```

### **Technology Stack**

| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 10.0 | Runtime |
| **PostgreSQL** | 15 | Primary database |
| **Redis** | 7.0 | Caching layer |
| **Google OAuth 2.0** | Managed | Authentication (OAuth 2.0/OpenID Connect) |
| **MediatR** | 11.1.0 | CQRS command/query bus |
| **Entity Framework Core** | 8.0.0 | ORM for writes |
| **Dapper** | 2.1.0 | Optimized queries for reads |
| **FluentValidation** | 11.7.1 | Input validation |
| **AutoMapper** | 12.0.1 | DTO mapping |
| **Quartz** | 3.7.0 | Background jobs |
| **Serilog** | 3.1.1 | Structured logging |
| **xUnit** | 2.6.2 | Unit testing |
| **NSubstitute** | 5.1.0 | Mocking |
| **FluentAssertions** | 6.12.0 | Test assertions |

---

## 📊 Non-Functional Requirements (NFRs)

### **Performance Targets**
- Search response time: **< 100ms** (with Redis cache)
- API latency (p95): **< 200ms**
- Cache hit ratio: **> 80%**
- Concurrent users: **10,000+**

### **Security**
- ✅ OAuth 2.0 + JWT Bearer authentication
- ✅ RBAC with permission-level granularity
- ✅ TLS 1.3 transport encryption
- ✅ Parameterized queries (SQL injection prevention)
- ✅ Input validation on all endpoints
- ✅ OWASP Top 10 compliance
- ✅ Rate limiting: 100 req/min per user

### **Reliability & Availability**
- **SLA Target**: 99.9% uptime
- **MTTR**: < 2 hours
- **Backup**: Every 6 hours
- **Health Checks**: All critical endpoints monitored

### **Data Integrity**
- ACID transactions for consistency
- Optimistic concurrency control
- **Outbox Pattern** for guaranteed domain event delivery
- Idempotent command handlers

---

## 📁 Project Structure

```
Bookify.sln
├── src/
│   ├── Bookify.API/                 # Controllers, middleware, entry point
│   ├── Bookify.Application/         # CQRS handlers, validators, DTOs
│   ├── Bookify.Domain/              # Entities, value objects, domain events
│   ├── Bookify.Infrastructure/      # EF Core, Dapper, Redis, Google OAuth 2.0
│   └── Bookify.Shared/              # Constants, enums, exceptions
├── tests/
│   ├── Bookify.UnitTests/           # Domain & application logic tests
│   ├── Bookify.IntegrationTests/    # Database & service integration tests
│   └── Bookify.ArchitectureTests/   # Layer boundaries, namespace validation
├── docs/
│   ├── BOOKIFY_IMPLEMENTATION_PLAN.md  # Detailed specs & requirements
│   ├── CONCEPTS_AND_PATTERNS.md        # Architecture deep-dive
│   └── scripts.md                      # Useful SQL & deployment scripts
└── docker-compose.yml               # Local development stack
```

---

## 🛠️ Build & Test Commands

```bash
# Build solution (no warnings allowed)
dotnet build Bookify.sln

# Run all tests
dotnet test Bookify.sln --verbosity normal

# Run specific test project
dotnet test tests/Bookify.UnitTests/Bookify.UnitTests.csproj

# Watch mode (auto-recompile)
dotnet watch run --project src/Bookify.API/Bookify.API.csproj

# Publish for production
dotnet publish Bookify.sln -c Release
```

---

## 📖 API Endpoints Overview

### **Health & Documentation**
```
GET  /health              # Health check
GET  /swagger             # OpenAPI/Swagger UI
```

### **User Management**
```
POST  /api/v1/users/register     # Register new user
POST  /api/v1/users/login        # Login with email/password
GET   /api/v1/users/me           # Get current user profile
GET   /api/v2/users/me           # Get extended user info (v2)
```

### **Apartment Catalog** (In Progress)
```
GET   /api/v1/apartments                  # Search with filters & pagination
GET   /api/v1/apartments/{id}             # Get apartment details
POST  /api/v1/apartments                  # Create apartment (owner auth)
PUT   /api/v1/apartments/{id}             # Update apartment (owner only)
DELETE /api/v1/apartments/{id}            # Delete apartment (owner only)
```

### **Bookings** (Planned)
```
POST  /api/v1/bookings                    # Create booking
GET   /api/v1/bookings/{id}               # Get booking details
GET   /api/v1/bookings/user/me            # List user's bookings
PATCH /api/v1/bookings/{id}/confirm       # Owner confirms booking
PATCH /api/v1/bookings/{id}/reject        # Owner rejects booking
```

### **Reviews** (Planned)
```
POST  /api/v1/reviews                     # Create review
GET   /api/v1/apartments/{id}/reviews     # Get apartment reviews
```

---

## 🧪 Testing Strategy

### **Unit Tests** (`Bookify.UnitTests/`)
- Domain logic, service calculations, validators
- Target: > 80% coverage

### **Integration Tests** (`Bookify.IntegrationTests/`)
- Database operations, Google OAuth auth, Redis caching
- Uses Testcontainers for realistic scenarios

### **Architecture Tests** (`Bookify.ArchitectureTests/`)
- Layer boundary enforcement
- Namespace conventions validation
- No circular dependencies

---

## 🔐 Sensitive Areas (Require Explicit Permission)

Before modifying:
- Database schema & migrations
- Authentication & authorization logic
- Domain entities & aggregate roots
- API contracts (request/response DTOs)
- Pricing calculations
- Outbox pattern & event processing
- Payment-related code

---

## 📚 Key Reference Files

- **[BOOKIFY_IMPLEMENTATION_PLAN.md](docs/BOOKIFY_IMPLEMENTATION_PLAN.md)** - Complete requirements & specifications
- **[CONCEPTS_AND_PATTERNS.md](docs/CONCEPTS_AND_PATTERNS.md)** - Architecture patterns deep-dive
- **[scripts.md](docs/scripts.md)** - Useful SQL and deployment scripts
- **[Program.cs](src/Bookify.API/Program.cs)** - DI configuration & middleware setup
- **[Entity.cs](src/Bookify.Domain/Entity.cs)** - Base entity class with domain events

---

## 🚢 Deployment & Infrastructure

### **Docker Services**
- **PostgreSQL 15** (port 5432): `bookify-postgres`
- **Redis 7** (port 6379): `bookify-redis`
- **Google OAuth 2.0**: external identity provider configured via environment variables

### **Configuration**
- Development: `appsettings.Development.json`
- Production: `appsettings.Production.json`
- Environment-specific secrets via Azure Key Vault (production)

### **CI/CD Pipeline**
- GitHub Actions for automated build/test on PR
- Deployment to Azure Container Instances or App Service
- Full test suite must pass before merge

---

## 📝 Code Standards

- **Language**: C# 12 (nullable enabled)
- **Naming**: PascalCase (types/methods), camelCase (variables), UPPER_CASE (constants)
- **Interfaces**: Prefix with `I` (e.g., `IRepository`)
- **Indentation**: 4 spaces (enforced via .editorconfig)
- **Analyzers**: EnforceCodeStyleInBuild enabled
- **Coverage Target**: > 80% unit/integration test coverage

---

## 🐛 Troubleshooting

### **Docker Services Not Starting**
```bash
# Check service health
docker ps

# View logs
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f postgres
```

### **Build Failures**
```bash
# Clean cache
dotnet clean && dotnet restore

# Parallel build
dotnet build -m
```

### **Database Issues**
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

---

## 📧 Support & Contact

For questions about architecture, requirements, or implementation details, refer to the detailed documentation in the `docs/` folder or consult the copilot-instructions.md file.

---

**Last Updated**: May 17, 2026  
**Project Status**: Phase 2 (Apartment Catalog) in progress
