# Phase 03: Backend API Structure

## Context Links
- [Main Plan](plan.md)
- [Architecture Report](../reports/researcher-260202-1222-nextjs-dotnet-architecture.md)

## Overview
- **Priority:** P1 (Critical - frontend depends on API)
- **Status:** completed
- **Effort:** 8h
- **Description:** Setup ASP.NET Core 8 Minimal APIs with clean architecture

## Key Insights
- Minimal APIs for performance and simplicity
- Carter library for organized endpoint grouping
- MediatR for CQRS pattern
- FluentValidation for request validation
- Global exception handling

## Requirements

### Functional
- F1: RESTful API endpoints for all resources
- F2: Request validation with meaningful errors
- F3: OpenAPI/Swagger documentation
- F4: TypeScript client generation

### Non-Functional
- NF1: Response time <100ms for simple queries
- NF2: Consistent error response format
- NF3: Rate limiting (100 req/min per IP)

## Architecture

### Clean Architecture Layers

```
┌───────────────────────────────────────────────┐
│              API Layer (Endpoints)             │
│  - Minimal APIs with Carter                    │
│  - Request/Response DTOs                       │
│  - Validation (FluentValidation)               │
└─────────────────────┬─────────────────────────┘
                      │
┌─────────────────────▼─────────────────────────┐
│            Application Layer                   │
│  - MediatR Commands/Queries                    │
│  - Handlers                                    │
│  - Business Logic                              │
└─────────────────────┬─────────────────────────┘
                      │
┌─────────────────────▼─────────────────────────┐
│              Core Layer                        │
│  - Entities                                    │
│  - Interfaces                                  │
│  - Domain Services                             │
└─────────────────────┬─────────────────────────┘
                      │
┌─────────────────────▼─────────────────────────┐
│          Infrastructure Layer                  │
│  - EF Core Repositories                        │
│  - External Services                           │
│  - Database Context                            │
└───────────────────────────────────────────────┘
```

### API Endpoint Groups

```
/api
├── /auth
│   ├── POST /register
│   ├── POST /login
│   ├── POST /refresh
│   └── GET  /me
├── /events
│   ├── GET  /                   # List events (public)
│   ├── GET  /{id}              # Event details
│   ├── POST /                   # Create (organizer)
│   ├── PUT  /{id}              # Update (owner)
│   └── DELETE /{id}            # Delete (owner)
├── /events/{eventId}/tiers
│   ├── GET  /                   # List tiers
│   ├── POST /                   # Create tier
│   ├── PUT  /{tierId}          # Update tier
│   └── DELETE /{tierId}        # Delete tier
├── /cart
│   ├── GET  /                   # Get cart
│   ├── POST /items             # Add to cart
│   ├── PUT  /items/{id}        # Update quantity
│   └── DELETE /items/{id}      # Remove item
├── /orders
│   ├── GET  /                   # User's orders
│   ├── GET  /{id}              # Order details
│   └── POST /                   # Create from cart
├── /payments
│   └── POST /sepay/webhook     # SePay callback
├── /checkin
│   └── POST /                   # Check in ticket
├── /analytics
│   ├── GET  /events/{id}/sales # Sales data
│   └── GET  /events/{id}/checkins # Check-in data
└── /admin
    ├── GET  /users             # List users
    ├── GET  /events            # All events
    └── GET  /stats             # Platform stats
```

### Standard Response Format

```csharp
// Success
public record ApiResponse<T>(T Data, string? Message = null);

// Error
public record ApiError(string Code, string Message, Dictionary<string, string[]>? Errors = null);

// Paginated
public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
```

## Related Code Files

### Create
- `src/backend/EventTickets.API/Program.cs` (update)
- `src/backend/EventTickets.API/Endpoints/AuthEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/EventEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/TicketTierEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/CartEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/OrderEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/PaymentEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/CheckinEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/AnalyticsEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/AdminEndpoints.cs`
- `src/backend/EventTickets.API/Middleware/ExceptionHandlingMiddleware.cs`
- `src/backend/EventTickets.API/Middleware/RateLimitingMiddleware.cs`
- `src/backend/EventTickets.Core/Interfaces/IRepository.cs`
- `src/backend/EventTickets.Core/Interfaces/IEventRepository.cs`
- `src/backend/EventTickets.Core/Interfaces/IOrderRepository.cs`
- `src/backend/EventTickets.Infrastructure/Repositories/BaseRepository.cs`
- `src/backend/EventTickets.Infrastructure/Repositories/EventRepository.cs`
- `src/backend/EventTickets.Infrastructure/Repositories/OrderRepository.cs`

## Implementation Steps

### 1. Configure API Project (1.5h)
- [x] Add NuGet packages: Carter, MediatR, FluentValidation, NSwag
- [x] Configure dependency injection in `Program.cs`
- [x] Setup OpenAPI/Swagger middleware
- [x] Add CORS configuration for frontend
- [x] Configure JSON serialization options

### 2. Create Middleware (1.5h)
- [x] Implement `ExceptionHandlingMiddleware` for global error handling
- [x] Implement `RateLimitingMiddleware` using `System.Threading.RateLimiting`
- [x] Add request logging middleware
- [x] Configure middleware pipeline order

### 3. Define Repository Interfaces (1h)
- [x] Create `IRepository<T>` generic interface
- [x] Create specific repository interfaces
- [x] Define query specifications pattern
- [x] Add pagination support to interfaces

### 4. Implement Base Repositories (1.5h)
- [x] Create `BaseRepository<T>` with common CRUD
- [x] Implement specific repositories
- [x] Add optimistic locking support
- [x] Register repositories in DI container

### 5. Create Endpoint Groups (2h)
- [x] Create Carter modules for each endpoint group
- [x] Define request/response DTOs
- [x] Add FluentValidation validators
- [x] Wire up to repository layer (placeholder logic)
- [x] Add authorization attributes

### 6. Configure Type Generation (0.5h)
- [x] Setup NSwag for OpenAPI generation
- [x] Test TypeScript client generation
- [x] Add npm script in frontend

## Todo List
- [x] Add required NuGet packages
- [x] Configure Program.cs with all middleware
- [x] Create exception handling middleware
- [x] Create rate limiting middleware
- [x] Define repository interfaces
- [x] Implement base repository
- [x] Create all endpoint modules
- [x] Setup request validation
- [x] Configure OpenAPI/Swagger
- [x] Test TypeScript generation

## Success Criteria
- [x] All endpoints return correct HTTP status codes
- [x] Validation errors return 400 with field details
- [x] Swagger UI accessible at /swagger
- [x] TypeScript types generate correctly
- [x] Rate limiting blocks excessive requests

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Circular dependency | Medium | Medium | Strict layer separation |
| N+1 queries | High | Medium | Use Include() and projection |
| Over-engineering | Medium | Low | Start simple, refactor later |

## Security Considerations
- Validate all input with FluentValidation
- Rate limit to prevent abuse
- Log security-relevant events
- Sanitize error messages (no stack traces in prod)

## Next Steps
After completion, proceed to [Phase 04: Authentication](phase-04-authentication.md)
