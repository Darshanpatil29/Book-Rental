# BOOKIFY - Comprehensive Implementation Plan & Requirements

**Project Type:** Apartment Rental Booking System  
**Technology Stack:** .NET 8, ASP.NET Core, PostgreSQL, Keycloak, Redis  
**Architecture:** Clean Architecture with CQRS and Domain-Driven Design  
**Date:** April 2026  

---

## 1. EXECUTIVE SUMMARY

Bookify is a **production-ready apartment rental booking platform** demonstrating enterprise-level .NET development practices. It implements a robust, scalable architecture suitable for a SaaS booking platform with multi-tenant capabilities.

**Key Highlights:**
- ✅ Rich domain modeling with value objects and domain events
- ✅ CQRS pattern for optimized read/write operations
- ✅ Microservices-ready architecture
- ✅ Enterprise-grade security (Keycloak OAuth/OpenID Connect)
- ✅ Comprehensive API versioning strategy
- ✅ Distributed caching and background job processing
- ✅ Full test coverage with integration testing

---

## 2. PROJECT OVERVIEW

### 2.1 Business Objectives

| Objective | Description |
|-----------|-------------|
| **User Onboarding** | Easy registration and profile management with secure authentication |
| **Apartment Discovery** | Searchable apartment catalog with filtering by availability dates |
| **Booking Management** | Reserve apartments with dynamic pricing, confirmation workflows |
| **Review System** | Transparent ratings and reviews for apartments and experiences |
| **Payment & Pricing** | Transparent, itemized pricing with automatic calculations |
| **Report Generation** | Analytics for administrators (future phase) |

### 2.2 Key Stakeholders

- **Platform Administrators**: Manage apartments, reviews, disputes
- **Apartment Owners**: List properties, manage availability, view bookings
- **Guests/Renters**: Search, book, review apartments
- **System Maintainers**: Monitor infrastructure, security, performance

### 2.3 Success Criteria

- [ ] All 12 API endpoints operational and tested
- [ ] Sub-100ms response time for search queries (with caching)
- [ ] 99.9% uptime SLA compliance
- [ ] Zero data loss with outbox pattern
- [ ] All OWASP Top 10 vulnerabilities addressed
- [ ] Full API documentation (OpenAPI/Swagger)

---

## 3. FUNCTIONAL REQUIREMENTS

### 3.1 User Management

**REQ-001: User Registration**
- Users register with email and personal details (first name, last name)
- System validates email uniqueness
- User created in Keycloak (external identity provider)
- Confirmation email sent (future: email verification)
- **Acceptance Criteria:**
  - Duplicate emails rejected with HTTP 400
  - User searchable immediately after creation
  - Identity linked between Keycloak and database

**REQ-002: User Authentication**
- JWT-based stateless authentication
- OAuth 2.0 / OpenID Connect with Keycloak
- Access tokens valid for 15 minutes (configurable)
- Refresh tokens for session extension
- **Acceptance Criteria:**
  - Invalid credentials return 401 Unauthorized
  - Expired tokens return 401 with refresh opportunity
  - All endpoints (except register/login) require valid JWT

**REQ-003: Authority & Permissions**
- Role-based access control (RBAC)
- Permission-level granularity (UsersRead, UsersWrite, etc.)
- Roles: Admin, Owner, Guest
- **Acceptance Criteria:**
  - Non-admin users cannot access admin endpoints
  - Guests cannot modify bookings
  - Owners can only manage their own apartments

### 3.2 Apartment Management

**REQ-004: Apartment Registration**
- Owners register apartments with:
  - Name, description, address details
  - Price per night (with currency support)
  - Cleaning fee amount
  - Amenities (multiple selection: WiFi, parking, AC, garden view, mountain view)
  - Photos (future phase)
- **Acceptance Criteria:**
  - All required fields validated
  - Invalid prices rejected (negative, zero)
  - Amenities stored as bitmask for efficient querying

**REQ-005: Apartment Search**
- Search by date range (start date, end date)
- Return only available apartments (no booking conflicts)
- Filter results by:
  - Price range (min/max)
  - Amenities (multi-select)
  - Location (city, state)
- Results paginated (20 results per page)
- **Acceptance Criteria:**
  - Results exclude reserved apartments in date range
  - Search results cached in Redis for 1 hour
  - Response time < 100ms (with cache)
  - Pagination metadata included (total count, page size)

**REQ-006: Apartment Details**
- View complete apartment information
- Display reviews and ratings
- Show owner information
- Availability calendar (next 90 days)
- **Acceptance Criteria:**
  - Read operations use Dapper (optimized SQL)
  - Owner anonymity for privacy (show only first name)

### 3.3 Booking Management

**REQ-007: Booking Reservation**
- Guest reserves apartment for date range
- System validates:
  - Date range overlap detection (no double-booking)
  - Minimum stay requirements (future: configurable)
  - Check-in/check-out time validity
- Booking created with initial status: **Reserved**
- Price calculated with all components:
  - Base price (nights × nightly rate)
  - Cleaning fee (one-time)
  - Amenity surcharge (5% for views, 1% for AC/parking)
- **Acceptance Criteria:**
  - Overlapping dates rejected with HTTP 409 Conflict
  - Price calculation accurate to 2 decimal places
  - Booking confirmation email sent to guest
  - Owner receives notification of new reservation

**REQ-008: Booking Confirmation**
- Owner confirms or rejects reservation (within 48 hours)
- Confirmed: Apartment becomes unavailable for that period
- Rejected: Returns to available state
- Domain events trigger downstream processes
- **Acceptance Criteria:**
  - Only apartment owner can confirm/reject
  - Status transitions: Reserved → Confirmed/Rejected
  - No double confirmation possible
  - Guest notified by email of decision

**REQ-009: Booking Completion**
- System marks booking as completed after checkout date
- Triggered by background job (Quartz scheduler)
- Enables guest to leave review
- **Acceptance Criteria:**
  - Automatic status update after checkout
  - Revenue recognized only for completed bookings
  - Review system enabled only for completed bookings

**REQ-010: Booking History**
- Guest views all their bookings (past and future)
- Bookings displayable with full details:
  - Apartment info, dates, pricing breakdown
  - Current status
  - Owner contact information (if confirmed)
- **Acceptance Criteria:**
  - Resource-based authorization (users can only view own bookings)
  - Sorted by date (newest first)
  - Includes canceled/rejected bookings

### 3.4 Review System

**REQ-011: Review Creation**
- Only guests with completed bookings can review
- Review includes:
  - Rating (1-5 stars, required)
  - Text comment (optional, max 500 characters)
- Review belongs to booking, apartment, and user
- **Acceptance Criteria:**
  - Reviews restricted to completed bookings
  - Rating validation (1-5 range)
  - Duplicate reviews prevented (one per booking)
  - Domain event raised (`ReviewCreatedDomainEvent`)

**REQ-012: Review Visibility**
- Reviews displayed on apartment detail page
- Show reviewer name, date, rating, comment
- Calculate apartment average rating from all reviews
- **Acceptance Criteria:**
  - Reviews sorted by date (newest first)
  - Anonymous reviews option (future phase)
  - Moderation workflow for offensive content (future)

### 3.5 API Versioning

**REQ-013: Multiple API Versions**
- Support v1 and v2 endpoints simultaneously
- v1: Legacy endpoints for backward compatibility
- v2: Enhanced endpoints with new features
- Version parameter in URL path: `/api/v1/...`, `/api/v2/...`
- **Acceptance Criteria:**
  - Both versions functional and independent
  - Documentation for both versions in Swagger
  - Deprecation path clear for v1 sunset

---

## 4. NON-FUNCTIONAL REQUIREMENTS

### 4.1 Performance

| Requirement | Target | Rationale |
|-------------|--------|-----------|
| **Search Response Time** | < 100ms | Must handle peak traffic |
| **API Latency (p95)** | < 200ms | User experience critical |
| **Database Query Time** | < 50ms | Optimize N+1 queries |
| **Cache Hit Ratio** | > 80% | Reduce database load |
| **Concurrent Users** | 10,000+ | Scalability requirement |
| **Data Transfer Rate** | > 1GB/sec | Network throughput |

**Implementation Approach:**
- Redis caching for search results (1-hour TTL)
- Database query optimization with Dapper for reads
- EF Core for optimized writes with change tracking
- Connection pooling in PostgreSQL
- CDN placement for static content

### 4.2 Security

| Requirement | Implementation |
|-------------|-----------------|
| **Authentication** | Keycloak OAuth 2.0 + JWT Bearer Tokens |
| **Authorization** | RBAC (Role-Based) + Permission-level checks |
| **Data Encryption** | TLS 1.3 for transport, AES-256 for sensitive fields |
| **OWASP Compliance** | A01:2021 Injection, A07:2021 CSRF Prevention, etc. |
| **Secrets Management** | Azure Key Vault / Configuration encryption |
| **Rate Limiting** | API rate limiting (100 req/min per user) |
| **SQL Injection** | Parameterized queries (EF Core + Dapper) |
| **CORS Policy** | Restrictive cross-origin policy |
| **Input Validation** | FluentValidation on all inputs |
| **Audit Logging** | All sensitive operations logged with Serilog |

### 4.3 Reliability & Availability

| Requirement | Target |
|-------------|--------|
| **Uptime SLA** | 99.9% (8.76 hours downtime/year) |
| **MTTR (Mean Time To Recover)** | < 2 hours |
| **RTO (Recovery Time Objective)** | 1 hour |
| **RPO (Recovery Point Objective)** | 15 minutes |
| **Error Rate** | < 0.1% (1 per 1000 requests) |

**Implementation Approach:**
- Database replication (Primary + Standby)
- Automated backup every 6 hours
- Health checks on critical endpoints
- Circuit breakers for external service calls (Keycloak)
- Graceful degradation when cache unavailable

### 4.4 Scalability

- **Horizontal Scaling:** Stateless API design allows multiple instances
- **Database Scaling:** Read replicas for reporting, write master for transactional
- **Cache Scaling:** Redis cluster support for distributed caching
- **Job Scaling:** Quartz.NET distributed scheduler
- **Message Queue:** Future: RabbitMQ/Kafka for event distribution

### 4.5 Data Integrity

- **ACID Transactions:** Database transactions ensure consistency
- **Optimistic Concurrency:** `xmin` row version prevents lost updates
- **Outbox Pattern:** Guarantees delivery of domain events
- **Idempotency:** Commands are idempotent (safe to retry)
- **Data Validation:** Domain entity constraints enforced

### 4.6 Maintainability

- **Layered Architecture:** Clear separation of concerns
- **Dependency Injection:** Loosely coupled dependencies
- **Unit Testing:** > 80% code coverage target
- **Integration Testing:** Testcontainers for realistic scenarios
- **Documentation:** OpenAPI/Swagger + inline code comments
- **Code Standards:** EditorConfig + StyleCop analyzers

### 4.7 Compliance & Legal

- **GDPR Compliance:**
  - User data retention policies
  - Right to be forgotten (data deletion)
  - Data export functionality
- **PCI DSS:** Payment processing compliance (if handling cards directly)
- **Privacy Policy:** Clear terms around data usage
- **Terms of Service:** Booking cancellation policies, liability limits

---

## 5. IMPLEMENTATION PLAN & PHASES

### 5.1 Phase 0: Foundation Setup (Weeks 1-2)

**Deliverables:**
- ✅ Development environment setup (Visual Studio, Docker)
- ✅ Git repository initialized with branching strategy
- ✅ Database schema created (PostgreSQL)
- ✅ Keycloak realm configured with client credentials
- ✅ Docker Compose stack with all services
- ✅ CI/CD pipeline basics (GitHub Actions)

**Activities:**
1. Initialize .NET 8 solution with 4-layer structure:
   - `Bookify.API` (Presentation)
   - `Bookify.Application` (Use cases)
   - `Bookify.Domain` (Core business logic)
   - `Bookify.Infrastructure` (Technical implementation)

2. Configure PostgreSQL:
   - Create database `bookify`
   - Create tables: apartments, users, bookings, reviews
   - Setup constraints and indexes

3. Keycloak Setup:
   - Create realm `bookify`
   - Configure OAuth clients: `bookify-client`, `bookify-admin`
   - Setup user roles: Admin, Owner, Guest

4. Docker Compose:
   ```yaml
   services:
     api: (port 5000)
     postgres: (port 5432)
     keycloak: (port 8080)
     redis: (port 6379)
   ```

**Success Criteria:**
- All services start without errors
- Database migrations apply cleanly
- Keycloak accessible and clients configured

---

### 5.2 Phase 1: Core User & Auth (Weeks 3-4)

**Deliverables:**
- ✅ User registration endpoint
- ✅ User login endpoint with JWT tokens
- ✅ User profile endpoint
- ✅ Role-based authorization
- ✅ API versioning infrastructure (v1, v2)
- ✅ Swagger documentation

**User Stories:**

**US-001: User Registration**
```
AS A guest
I WANT TO register for an account
SO THAT I can search and book apartments

Acceptance Criteria:
- POST /api/v1/users/register with email, firstName, lastName
- Email validation and uniqueness check
- User created in Keycloak
- User persisted in database
- Success response: 201 Created with user ID
- Error handling: 400 Bad Request for validation failures
```

**US-002: User Login**
```
AS A registered user
I WANT TO log in with my credentials
SO THAT I can access the booking system

Acceptance Criteria:
- POST /api/v1/users/login with email, password
- JWT access token returned
- Token includes user ID, roles, email
- Refresh token mechanism in place
- Tokens expire appropriately
```

**US-003: View User Profile**
```
AS A logged-in user
I WANT TO view my profile
SO THAT I can manage my account

Acceptance Criteria:
- GET /api/v1/users/me returns current user
- Only authenticated users can access
- Shows: ID, firstName, lastName, email, roles
- v2 endpoint includes additional metadata
```

**Technical Tasks:**
- [ ] Implement `User` entity with validation
- [ ] Create `RegisterUserCommand` + handler
- [ ] Create `LoginCommand` + handler
- [ ] Configure FluentValidation rules
- [ ] Setup JWT middleware and Keycloak integration
- [ ] Implement `IAuthenticationService` interface
- [ ] Write unit tests for auth logic
- [ ] Document endpoints in Swagger

**Definition of Done:**
- Code reviewed and approved
- Unit tests passing (>80% coverage)
- Integration tests with test database
- Swagger documentation updated
- No uncommitted breaking changes

---

### 5.3 Phase 2: Apartment Catalog (Weeks 5-6)

**Deliverables:**
- ✅ Apartment management (CRUD)
- ✅ Apartment search with date filtering
- ✅ Amenities system
- ✅ Redis caching for search results
- ✅ Pagination support

**User Stories:**

**US-004: List Available Apartments**
```
AS A guest
I WANT TO search for available apartments
SO THAT I can find a place to stay

Acceptance Criteria:
- GET /api/v1/apartments?startDate=2026-04-25&endDate=2026-04-30
- Returns apartments not booked during dates
- Pagination: 20 results per page
- Response includes: name, description, price, amenities, rating
- Results cached in Redis for 1 hour
- Response time < 100ms
```

**US-005: View Apartment Details**
```
AS A guest
I WANT TO view apartment details
SO THAT I can make an informed decision

Acceptance Criteria:
- GET /api/v1/apartments/{id}
- Shows full details: address, photos, amenities, pricing
- Displays reviews (average rating, individual reviews)
- Shows availability for next 90 days
- Optimized with Dapper for read performance
```

**US-006: Manage Apartments (Owner)**
```
AS AN apartment owner
I WANT TO list and manage my apartments
SO THAT guests can find and book them

Acceptance Criteria:
- POST /api/v1/apartments creates new apartment
- PUT /api/v1/apartments/{id} updates details
- Only owner can modify their apartments
- DELETE endpoint removes apartment (if no active bookings)
- Validation: required fields, positive prices
```

**Technical Tasks:**
- [ ] Implement `Apartment` aggregate root
- [ ] Create value objects: `Address`, `Money`, `Amenity`
- [ ] Implement `SearchApartmentsQuery` + handler
- [ ] Setup Redis caching via `QueryCachingBehavior`
- [ ] Create `CreateApartmentCommand` + handler
- [ ] Implement Dapper read models for optimized queries
- [ ] Add pagination support to queries
- [ ] Setup `PricingService` for calculations
- [ ] Write comprehensive integration tests

**Dependencies:**
- Must complete Phase 1 (authentication, authorization)
- Keycloak roles (Owner role required)

---

### 5.4 Phase 3: Booking System (Weeks 7-8)

**Deliverables:**
- ✅ Booking reservation logic
- ✅ Booking confirmation workflow
- ✅ Complex pricing calculation
- ✅ Domain events (BookingReservedEvent, BookingConfirmedEvent)
- ✅ Outbox pattern for reliable event delivery

**User Stories:**

**US-007: Reserve an Apartment**
```
AS A guest
I WANT TO reserve an apartment
SO THAT I can secure a booking

Acceptance Criteria:
- POST /api/v1/bookings with apartmentId, startDate, endDate
- System validates no overlapping bookings
- Price calculated: (nights × nightly) + cleaning + surcharges
- Booking status: Reserved
- Confirmation email sent to guest
- Owner notified of new reservation
- Cannot book if apartment already reserved
```

**US-008: Confirm Booking (Owner)**
```
AS AN apartment owner
I WANT TO confirm guest reservations
SO THAT the booking becomes official

Acceptance Criteria:
- POST /api/v1/bookings/{id}/confirm (owner only)
- Status transitions: Reserved → Confirmed
- Dates locked (unavailable for other guests)
- Guest receives confirmation email
- Owner receives confirmation
- Cannot confirm if dates changed
```

**US-009: Reject Booking (Owner)**
```
AS AN apartment owner
I WANT TO reject unsuitable bookings
SO THAT I maintain control over my apartment

Acceptance Criteria:
- POST /api/v1/bookings/{id}/reject (owner only)
- Status: Rejected, apartment returns to available
- Guest receives rejection notification
- Can optionally provide reason for rejection
- Reason stored for analytics
```

**Technical Tasks:**
- [ ] Implement `Booking` aggregate root with state machine
- [ ] Create value objects: `DateRange`, `BookingStatus`
- [ ] Implement `ReserveBookingCommand` + handler
- [ ] Implement `ConfirmBookingCommand` + handler
- [ ] Implement `RejectBookingCommand` + handler
- [ ] Setup domain events:
  - `BookingReservedDomainEvent`
  - `BookingConfirmedDomainEvent`
  - `BookingRejectedDomainEvent`
- [ ] Implement outbox pattern:
  - `OutboxMessage` table
  - `OutboxProcessor` background job (Quartz)
- [ ] Create email notification service (`IEmailService`)
- [ ] Implement `PricingService` with complex calculations
- [ ] Add optimistic concurrency control (`xmin`)
- [ ] Write comprehensive tests for state machine

**Complex Pricing Logic:**
```
TotalPrice = 
  (NightlyRate × NumberOfNights) +    // Base price
  CleaningFee +                         // One-time fee
  (Base × AmenityPercentage)           // 5% for views, 1% for AC/Parking
```

---

### 5.5 Phase 4: Reviews & Ratings (Weeks 9)

**Deliverables:**
- ✅ Review creation (only for completed bookings)
- ✅ Review display and rating aggregation
- ✅ Review listing on apartment pages

**User Stories:**

**US-010: Leave a Review**
```
AS A guest with a completed booking
I WANT TO leave a review
SO THAT I can share my experience

Acceptance Criteria:
- POST /api/v1/reviews with bookingId, rating, comment
- Rating required (1-5 stars)
- Comment optional (max 500 chars)
- Only available after booking completion
- One review per booking
- Domain event: ReviewCreatedDomainEvent
```

**US-011: View Reviews**
```
AS A guest browsing apartments
I WANT TO see reviews
SO THAT I can make informed decisions

Acceptance Criteria:
- GET /api/v1/apartments/{id} includes reviews
- Shows average rating (1-5 stars)
- Lists individual reviews (newest first)
- Includes reviewer name and review date
- Pagination for many reviews (10 per page)
```

**Technical Tasks:**
- [ ] Implement `Review` entity
- [ ] Create `CreateReviewCommand` + handler
- [ ] Implement validation: booking must be completed
- [ ] Create domain event: `ReviewCreatedDomainEvent`
- [ ] Add review aggregation to apartment details
- [ ] Setup rating calculation (average)
- [ ] Add pagination for reviews
- [ ] Unit and integration tests

---

### 5.6 Phase 5: Testing & Quality (Weeks 10-11)

**Deliverables:**
- ✅ Unit test suite (>80% coverage)
- ✅ Integration tests (all major flows)
- ✅ Architecture tests (layer boundaries)
- ✅ Performance benchmarks
- ✅ Security testing (OWASP)

**Testing Strategy:**

**Unit Tests** (XUnit + NSubstitute)
- Domain entity validation
- Value object operations
- Service logic (PricingService, etc.)
- Command/Query handlers (with mocked dependencies)
- **Target: 80%+ coverage**

**Integration Tests** (Testcontainers)
- Full database transactions
- Keycloak integration
- Redis caching
- Real EF Core DbContext
- Real repository implementations

**Architecture Tests** (NetArchTest)
- API layer doesn't directly reference Infrastructure
- Domain layer is dependency-free
- Application layer only depends on Domain
- No circular dependencies

**Performance Tests**
- Search query < 100ms (cached)
- API response < 200ms (p95)
- Database queries < 50ms
- Concurrent user load testing (10,000 concurrent)

**Security Tests**
- SQL injection attempts blocked
- CORS policy enforced
- OWASP Top 10 validation
- Authorization checks functional
- Rate limiting works

**Technical Tasks:**
- [ ] Write unit tests for all domain entities
- [ ] Write integration tests for critical paths:
  - User registration → Login → Booking flow
  - Apartment search → Book → Confirm flow
  - Review creation flow
- [ ] Architecture validation tests
- [ ] Performance benchmarking with "ApacheBench"
- [ ] OWASP security scanning
- [ ] Code coverage reporting

---

### 5.7 Phase 6: Deployment & Documentation (Weeks 12)

**Deliverables:**
- ✅ Production-ready Docker images
- ✅ Kubernetes manifests
- ✅ CI/CD pipeline (GitHub Actions)
- ✅ API documentation (OpenAPI/Swagger)
- ✅ Deployment guide
- ✅ Run books and troubleshooting

**Deployment Tasks:**
- [ ] Dockerfile optimization (multi-stage build)
- [ ] Docker Compose for local development
- [ ] Kubernetes manifests:
  - Namespace, Service, Deployment
  - ConfigMaps for settings
  - Secrets for sensitive data
  - Ingress rules
- [ ] GitHub Actions CI:
  - Build on push to main/develop
  - Run tests automatically
  - Build Docker image
  - Push to registry
  - Deploy to staging/production
- [ ] Environment management:
  - Development configuration
  - Staging configuration
  - Production configuration (secrets)
- [ ] Database migration strategy
- [ ] Monitoring & alerting setup

**Documentation:**
- [ ] API documentation generator (Swagger)
- [ ] Architecture decision records (ADRs)
- [ ] Deployment procedures
- [ ] Troubleshooting guide
- [ ] Developer onboarding guide
- [ ] Operations run book

---

## 6. ARCHITECTURE OVERVIEW

### 6.1 Layered Architecture Diagram

```
┌─────────────────────────────────────────────┐
│    Bookify.API (Presentation Layer)          │
│  Controllers | Endpoints | Middleware        │
└──────────────────┬──────────────────────────┘
         ▼
┌─────────────────────────────────────────────┐
│  Bookify.Application (Application Layer)     │
│  Commands | Queries | Handlers | Behaviors  │
│  DTOs | Validators | Mappers                │
└──────────────────┬──────────────────────────┘
         ▼
┌─────────────────────────────────────────────┐
│    Bookify.Domain (Domain Layer)             │
│  Entities | Value Objects | Domain Events   │
│  Repository Interfaces | Domain Services    │
└──────────────────┬──────────────────────────┘
         ▼
┌─────────────────────────────────────────────┐
│   Bookify.Infrastructure (Infrastructure)    │
│  EF Core DbContext | Repositories           │
│  Keycloak Client | Redis Client             │
│  Email Service | Quartz Scheduler           │
└─────────────────────────────────────────────┘
```

### 6.2 CQRS Pattern

**Command Flow (State-Changing):**
```
Request → API Controller 
       → ReserveBookingCommand (MediatR)
       → ValidationBehavior (FluentValidation)
       → LoggingBehavior (Serilog)
       → ReserveBookingCommandHandler
       → ApartmentRepository.GetById()
       → BookingRepository.Add()
       → DbContext.SaveChanges() (EF Core)
       → Domain Events published
       → Outbox messages created
       → Response (201 Created)
```

**Query Flow (Read Operations):**
```
Request → API Controller
       → SearchApartmentsQuery (MediatR)
       → QueryCachingBehavior checks Redis
       → (Cache Hit) → Return cached results
       → (Cache Miss) → SearchApartmentsQueryHandler
       → Dapper ExecuteQuery(optimized SQL)
       → Cache results in Redis (1 hour TTL)
       → Response (200 OK)
```

### 6.3 Authentication & Authorization Flow

```
1. Registration Request
   ├─ POST /api/v1/users/register
   ├─ RegisterUserCommand → Handler
   ├─ AuthenticationService.Register()
   ├─ Keycloak Admin API call (client credentials)
   ├─ User created in Keycloak realm
   ├─ Identity ID returned
   └─ User persisted in PostgreSQL

2. Login Flow
   ├─ POST /api/v1/users/login
   ├─ LoginCommand → Handler
   ├─ JwtService.GetToken()
   ├─ Keycloak token endpoint request
   ├─ JWT access token issued by Keycloak
   └─ Token returned to client

3. Authenticated Request
   ├─ Request with "Authorization: Bearer <JWT>"
   ├─ JwtBearerMiddleware validates token
   ├─ Keycloak public key validates signature
   ├─ User identity populated in HttpContext
   ├─ Authorization handler checks permissions
   └─ Request proceeds if authorized

4. Permission Check
   ├─ [HasPermission(Permissions.BookingsCreate)]
   ├─ PermissionAuthorizationHandler
   ├─ Query user permissions from DB
   ├─ Check against permission requirement
   ├─ (Authorized) → Proceed
   └─ (Denied) → 403 Forbidden
```

### 6.4 Event Processing (Outbox Pattern)

```
1. Booking Reserved
   ├─ ReserveBookingCommandHandler executes
   ├─ Booking aggregate created
   ├─ booking.Reserve() applies domain event
   ├─ BookingReservedDomainEvent added to events collection
   └─ DbContext.SaveChanges()
       ├─ Booking saved to 'bookings' table
       └─ OutboxMessage saved to 'outbox_messages' table
          (JSON: {Type: "BookingReserved", Content: {...}})

2. Background Job (Quartz)
   ├─ OutboxProcessor runs every 5 seconds
   ├─ Query outbox_messages WHERE processed_on_utc IS NULL
   ├─ For each message:
   │  ├─ Deserialize JSON to domain event
   │  ├─ Get handler from dependency container
   │  ├─ Handler.Handle(event)
   │  │  ├─ EmailService.SendBookingConfirmationEmail()
   │  │  ├─ NotificationService.NotifyOwner()
   │  │  └─ AnalyticsService.RecordBookingEvent()
   │  └─ Update message: processed_on_utc = now
   └─ Repeat

3. Error Handling
   ├─ If processing fails, exception caught
   ├─ Message remains unprocesed
   ├─ Retry on next job execution
   ├─ After 5 failed attempts, moved to dead-letter
   └─ Alert sent to operations team
```

### 6.5 Database Schema (Simplified)

```sql
-- Users (Authentication & Authorization)
CREATE TABLE users (
    id UUID PRIMARY KEY,
    first_name VARCHAR(200) NOT NULL,
    last_name VARCHAR(200) NOT NULL,
    email VARCHAR(400) NOT NULL UNIQUE,
    identity_id TEXT NOT NULL,  -- Keycloak ID
    created_on_utc TIMESTAMP NOT NULL,
    xmin INT8 NOT NULL          -- Optimistic concurrency
);

-- Apartments (Rental Properties)
CREATE TABLE apartments (
    id UUID PRIMARY KEY,
    owner_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(200) NOT NULL,
    description VARCHAR(2000),
    address_country VARCHAR(100),
    address_state VARCHAR(100),
    address_city VARCHAR(100),
    address_zip_code VARCHAR(20),
    address_street VARCHAR(200),
    price_amount DECIMAL(10,2),
    price_currency VARCHAR(3),
    cleaning_fee_amount DECIMAL(10,2),
    cleaning_fee_currency VARCHAR(3),
    amenities INT4,          -- Bitmask: WiFi=1, Parking=2, AC=4, Views=8, etc.
    last_booked_on_utc TIMESTAMP,
    created_on_utc TIMESTAMP,
    xmin INT8
);

-- Bookings (Reservations)
CREATE TABLE bookings (
    id UUID PRIMARY KEY,
    apartment_id UUID NOT NULL REFERENCES apartments(id),
    user_id UUID NOT NULL REFERENCES users(id),
    duration_start DATE NOT NULL,
    duration_end DATE NOT NULL,
    price_for_period_amount DECIMAL(10,2),
    price_for_period_currency VARCHAR(3),
    cleaning_fee_amount DECIMAL(10,2),
    cleaning_fee_currency VARCHAR(3),
    amenities_upcharge_amount DECIMAL(10,2),
    amenities_upcharge_currency VARCHAR(3),
    total_price_amount DECIMAL(10,2),
    total_price_currency VARCHAR(3),
    status INT (0=Reserved, 1=Confirmed, 2=Rejected, 3=Completed, 4=Cancelled),
    created_on_utc TIMESTAMP,
    confirmed_on_utc TIMESTAMP,
    rejected_on_utc TIMESTAMP,
    completed_on_utc TIMESTAMP,
    cancelled_on_utc TIMESTAMP,
    xmin INT8
);

-- Reviews (Ratings & Comments)
CREATE TABLE reviews (
    id UUID PRIMARY KEY,
    apartment_id UUID NOT NULL REFERENCES apartments(id),
    booking_id UUID NOT NULL REFERENCES bookings(id),
    user_id UUID NOT NULL REFERENCES users(id),
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment VARCHAR(500),
    created_on_utc TIMESTAMP
);

-- Domain Events (Outbox Pattern)
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY,
    type VARCHAR(500) NOT NULL,     -- Event type name
    content TEXT NOT NULL,           -- JSON payload
    created_on_utc TIMESTAMP NOT NULL,
    processed_on_utc TIMESTAMP       -- NULL = unprocessed
);

-- Authorization (Roles & Permissions)
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE  -- Admin, Owner, Guest
);

CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE  -- UsersRead, BookingsWrite, etc.
);

CREATE TABLE role_permissions (
    role_id UUID REFERENCES roles(id),
    permission_id UUID REFERENCES permissions(id),
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE user_roles (
    user_id UUID REFERENCES users(id),
    role_id UUID REFERENCES roles(id),
    PRIMARY KEY (user_id, role_id)
);
```

---

## 7. TECHNOLOGY STACK DETAILS

### 7.1 Backend (API Server)

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Runtime** | .NET 8 (LTS) | Latest stable, long-term support |
| **Framework** | ASP.NET Core 8 | High performance, cloud-native |
| **Language** | C# 12 | Modern features, strong typing |

### 7.2 Database & Persistence

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Primary DB** | PostgreSQL 15+ | ACID compliance, JSON support, extensions |
| **ORM (Write)** | Entity Framework Core 8 | Automatic migrations, change tracking |
| **ORM (Read)** | Dapper | Lightweight, performance optimized |
| **Cache** | Redis (Standalone/Cluster) | Sub-ms latency, eviction policies |
| **Migrations** | EF Core Migrations | Version control, rollback support |

### 7.3 Architecture & Patterns

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **CQRS** | MediatR 12+ | Separates reads from writes |
| **Validation** | FluentValidation | Fluent API, reusable rules |
| **Error Handling** | FluentResults | Result-based, no exceptions |
| **Mapping** | AutoMapper | DTO transformation, convention-based |
| **Testing** | xUnit | Modern, attribute-based |
| **Mocking** | NSubstitute | Intuitive, record/replay |
| **Assertions** | FluentAssertions | Readable test assertions |

### 7.4 Authentication & Authorization

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Identity Provider** | Keycloak | Open-source, OAuth 2.0/OIDC standard |
| **Authentication** | JWT Bearer | Stateless, scalable |
| **Authorization** | Custom handlers | Permission/role-based policies |
| **Admin API** | Keycloak.NET Client | Programmatic realm management |

### 7.5 Infrastructure & DevOps

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Containerization** | Docker | Consistent environments |
| **Orchestration** | Kubernetes | Scalability, self-healing |
| **CI/CD** | GitHub Actions | GitHub-native, free for public repos |
| **Monitoring** | Serilog + Seq | Structured logging, visualization |
| **Health Checks** | Built-in Middleware | Liveness/readiness probes |
| **Background Jobs** | Quartz.NET | Distributed scheduler, retry logic |

---

## 8. RISK ASSESSMENT & MITIGATION

### 8.1 High-Risk Items

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **Double-booking Race Condition** | High | Critical | Database-level unique constraint on (apartment_id, date). Optimistic locking with `xmin`. |
| **Keycloak Integration Failure** | Medium | High | Circuit breaker pattern. Fallback to cached user data. Health checks on startup. |
| **Data Loss in Event Processing** | Low | Critical | Outbox pattern with database transactions. Idempotent handlers. Dead-letter queue for failures. |
| **Performance Degradation** | Medium | High | Redis caching. Query optimization with Dapper. Load testing before production. Connection pooling. |
| **Unauthorized Data Access** | Medium | Critical | Authorization handlers on every endpoint. Resource-based auth for user data. Audit logging. |
| **Email Service Outage** | Medium | Medium | Async processing via outbox. Retry policy. Fallback notification channels. |

### 8.2 Medium-Risk Items

| Risk | Mitigation |
|------|-----------|
| **Incorrect Price Calculation** | PricingService unit tests. Booking verification before confirmation. |
| **Network Latency** | Caching layer. API response compression. GraphQL batching (future). |
| **Concurrency Issues** | Distributed locks for critical sections. Optimistic concurrency on aggregates. |
| **Database Migration Failures** | Test migrations in staging. Rollback strategy. Backward-compatible migrations. |
| **API Versioning Complexity** | Clear deprecation timeline. Gradual v1 sunset. Migration guides. |

---

## 9. DEPENDENCY & INTEGRATION POINTS

### 9.1 External Services

| Service | Purpose | Protocol | Failover |
|---------|---------|----------|----------|
| **Keycloak** | Identity/Auth | OpenID Connect | Circuit breaker, cached tokens |
| **PostgreSQL** | Transactions | PostgreSQL Protocol | Connection pooling, replicas |
| **Redis** | Caching | Redis Protocol | Graceful degradation, fallback to DB |
| **SMTP Server** | Email | SMTP/TLS | Queue async, retry policy |
| **Third-party APIs** | Future integration | REST | Circuit breaker, timeout handling |

### 9.2 Internal Service Communication

```
Bookify.API
├─ → Bookify.Application (commands/queries)
├─ → Bookify.Domain (entities, business logic)
└─ → Bookify.Infrastructure
     ├─ → PostgreSQL (data persistence)
     ├─ → Keycloak (authentication)
     ├─ → Redis (caching)
     └─ → SMTP (email notifications)
```

---

## 10. RESOURCE REQUIREMENTS

### 10.1 Development Team

| Role | Count | Responsibilities |
|------|-------|------------------|
| **Tech Lead / Architect** | 1 | Design, code reviews, decisions |
| **Senior Backend Developer** | 1 | Core domain logic, EF Core, database |
| **Mid-level Backend Developer** | 2 | Feature implementation, API endpoints |
| **QA Engineer** | 1 | Test automation, quality assurance |
| **DevOps Engineer** | 1 | Infrastructure, CI/CD, monitoring |
| **Product Manager** | 1 | Requirements, prioritization, stakeholder management |

**Total: 7 FTE for 12-week project**

### 10.2 Infrastructure Required

**Development Environment:**
- 4 vCPU, 8GB RAM workstations
- Visual Studio Professional / JetBrains Rider
- Docker Desktop for local containerization
- Git client and IDE extensions

**Staging Environment:**
- 2 Kubernetes nodes (2 vCPU, 4GB RAM each)
- PostgreSQL managed service (or containerized)
- Redis managed service (or containerized)
- Keycloak instance

**Production Environment (Initial):**
- 3 Kubernetes nodes (4 vCPU, 8GB RAM each)
- PostgreSQL managed service with replication
- Redis managed service with clustering
- Keycloak in HA configuration
- Load balancer (LB)
- Backup and disaster recovery

### 10.3 Licenses & Tools

| Tool | License | Cost | Notes |
|------|---------|------|-------|
| Visual Studio Professional | Paid | $550/year/user | Or JetBrains for alternatives |
| GitHub Enterprise (if needed) | Paid | $231/user/month | Public repos free |
| Azure DevOps (if on Azure) | Variable | Pay-as-you-go | Alternative to GitHub |
| PostgreSQL | Open Source | Free | Community supported |
| Keycloak | Open Source | Free | Community supported |
| Redis | Open Source | Free | Community supported, or managed services |
| Docker Hub | Free/Paid | 5-25/month | Private registries |
| New Relic / DataDog (monitoring) | Paid | 500-2000/month | Production monitoring required |

---

## 11. TIMELINE & MILESTONES

### 11.1 Project Schedule (12 Weeks)

```
Week 1-2   │ Foundation & Setup
           ├─ [✓] Development environment
           ├─ [✓] Git + CI/CD pipeline basics
           ├─ [✓] Database schema
           ├─ [✓] Keycloak realm
           └─ [✓] Docker Compose

Week 3-4   │ User Authentication
           ├─ [✓] User registration
           ├─ [✓] User login
           ├─ [✓] JWT token handling
           ├─ [✓] API versioning v1/v2
           └─ [✓] Swagger documentation

Week 5-6   │ Apartment Catalog
           ├─ [✓] Apartment CRUD operations
           ├─ [✓] Apartment search with filtering
           ├─ [✓] Amenities system
           ├─ [✓] Redis caching
           └─ [✓] Pagination support

Week 7-8   │ Booking System
           ├─ [✓] Booking reservation logic
           ├─ [✓] Booking confirmation workflow
           ├─ [✓] Pricing calculations
           ├─ [✓] Domain events & outbox
           └─ [✓] Email notifications

Week 9     │ Reviews & Ratings
           ├─ [✓] Review creation
           ├─ [✓] Review display
           └─ [✓] Rating aggregation

Week 10-11 │ Testing & Quality
           ├─ [✓] Unit test suite (>80% coverage)
           ├─ [✓] Integration tests
           ├─ [✓] Architecture validation
           ├─ [✓] Performance benchmarks
           └─ [✓] Security testing

Week 12    │ Deployment & Documentation
           ├─ [✓] Docker images optimization
           ├─ [✓] Kubernetes manifests
           ├─ [✓] CI/CD pipeline completion
           ├─ [✓] API documentation
           ├─ [✓] Deployment guides
           └─ [✓] Operations run books
```

### 11.2 Major Milestones

| Milestone | Target Date | Deliverables |
|-----------|------------|--------------|
| **M1: Development Ready** | End Week 2 | All dev environments functional, code repo setup |
| **M2: Auth Complete** | End Week 4 | Registration, login, API versioning working |
| **M3: MVP Feature Set** | End Week 8 | Apartments searchable, bookings functional |
| **M4: Feature Complete** | End Week 9 | All 12 requirements coded |
| **M5: Quality Gate** | End Week 11 | Tests passing, security scan clean |
| **M6: Production Ready** | End Week 12 | Deployed, monitored, documented |

### 11.3 Critical Path

```
Foundation Setup (Week 1-2)
        ↓
Authentication & Authorization (Week 3-4) ← CRITICAL
        ↓
Apartment Catalog (Week 5-6) ← CRITICAL
        ↓
Booking System (Week 7-8) ← CRITICAL (complex business logic)
        ↓
Reviews (Week 9)
        ↓
Testing & QA (Week 10-11) ← CRITICAL (quality gate)
        ↓
Deployment (Week 12)
```

---

## 12. SUCCESS METRICS & KPIs

### 12.1 Technical KPIs

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Response Time (p95)** | < 200ms | APM monitoring |
| **Error Rate** | < 0.1% | Application logs |
| **Uptime** | 99.9% | Health checks |
| **Database Query Time (p95)** | < 50ms | Query profiling |
| **Cache Hit Ratio** | > 80% | Redis metrics |
| **Test Coverage** | > 80% | Code coverage tools |
| **Build Success Rate** | 100% | CI/CD logs |

### 12.2 Business KPIs

| KPI | Target | Measurement |
|-----|--------|------------|
| **User Registration Rate** | 100+ per week | Analytics tracking |
| **Booking Conversion Rate** | > 5% | Platform metrics |
| **Average Review Rating** | > 4.0 stars | Database aggregation |
| **Owner Confirmation Rate** | > 90% | Booking status analytics |
| **System SLA Compliance** | 99.9% | Monitoring dashboard |
| **Time-to-Market** | 12 weeks | Project schedule |

---

## 13. NEXT STEPS & QUICK START

### 13.1 Immediate Actions (Week 1)

1. **Setup Development Environment:**
   ```bash
   # Clone repository
   git clone https://github.com/Tmaturano/Bookify.git
   cd Bookify

   # Install Docker Desktop
   # Install Visual Studio 2022 Community / JetBrains Rider

   # Start infrastructure
   docker-compose up -d

   # Restore NuGet packages
   dotnet restore

   # Run migrations
   dotnet ef database update
   ```

2. **Verify Deployment:**
   - [ ] API running on http://localhost:5000
   - [ ] Swagger accessible at http://localhost:5000/swagger
   - [ ] PostgreSQL connected
   - [ ] Keycloak running on http://localhost:8080

3. **Create First User:**
   - Register via `/api/v1/users/register`
   - Login to get JWT token
   - Test authenticated endpoint `GET /api/v1/users/me`

### 13.2 Key Files to Review

```
Bookify.API/
├─ Program.cs                    // Dependency injection
├─ Controllers/UsersController.cs // Example endpoints
└─ Middleware/                   // Custom middleware

Bookify.Application/
├─ Commands/RegisterUserCommand.cs          // Example command
├─ Queries/SearchApartmentsQuery.cs         // Example query
└─ Behaviors/ValidationBehavior.cs          // Pipeline behavior

Bookify.Domain/
├─ Entities/Booking.cs           // State machine example
└─ ValueObjects/Money.cs         // Value object pattern

Bookify.Infrastructure/
├─ Persistence/ApplicationDbContext.cs
└─ Authentication/JwtService.cs
```

### 13.3 Recommended Reading

1. **Architecture:**
   - Clean Architecture by Robert C. Martin
   - Domain-Driven Design by Eric Evans
   - CQRS by Martin Fowler

2. **Technology:**
   - Entity Framework Core documentation (microsoft.com)
   - MediatR GitHub wiki
   - Keycloak Administration Guide

3. **Code Quality:**
   - Code Complete by Steve McConnell
   - Pragmatic Programmer (pragmatic-architect course)
   - xUnit best practices

---

## 14. APPENDIX: API ENDPOINTS REFERENCE

### User Management
```
POST   /api/v1/users/register               # Register new user
POST   /api/v1/users/login                  # Login and get JWT
GET    /api/v1/users/me                     # Get current user profile
GET    /api/v2/users/me                     # v2 with extended info
```

### Apartment Management
```
GET    /api/v1/apartments                   # Search available apartments
GET    /api/v1/apartments/{id}              # Get apartment details
POST   /api/v1/apartments                   # Create apartment (owner)
PUT    /api/v1/apartments/{id}              # Update apartment (owner)
DELETE /api/v1/apartments/{id}              # Delete apartment (owner)
```

### Booking Management
```
POST   /api/v1/bookings                     # Reserve apartment
GET    /api/v1/bookings/{id}                # Get booking details
POST   /api/v1/bookings/{id}/confirm        # Confirm booking (owner)
POST   /api/v1/bookings/{id}/reject         # Reject booking (owner)
POST   /api/v1/bookings/{id}/cancel         # Cancel booking
GET    /api/v1/bookings                     # List user's bookings
```

### Review Management
```
POST   /api/v1/reviews                      # Create review
GET    /api/v1/apartments/{id}/reviews      # Get apartment reviews
GET    /api/v1/reviews/{id}                 # Get specific review
```

### System Health
```
GET    /health                              # Health check endpoint
```

---

## 15. CONCLUSION

**Bookify is a strategic learning project that demonstrates:**

✅ Enterprise clean architecture with clear separation of concerns  
✅ CQRS pattern for optimized read/write operations  
✅ Rich domain modeling with business logic encapsulation  
✅ Production-ready authentication & authorization  
✅ Reliable event processing with the outbox pattern  
✅ Modern .NET best practices and patterns  
✅ Comprehensive testing strategy  
✅ Cloud-native deployment readiness  

**This implementation plan provides:**

✅ Clear phases with achievable milestones  
✅ Detailed requirements for all features  
✅ Technical specifications for implementation  
✅ Risk assessment and mitigation strategies  
✅ Resource allocation recommendations  
✅ Success metrics and KPIs  
✅ Quick-start guide for development team  

**Estimated Effort:** 12 weeks with 7-person team  
**Technology Stack:** .NET 8, PostgreSQL, Keycloak, Redis, Kubernetes  
**Production Readiness:** Complete after Phase 6  

---

**Document prepared by:** Software Architecture & Project Management  
**Date:** April 19, 2026  
**Version:** 1.0 - Initial Release
