# Phase 03: Backend API Structure - Summary

**Status:** ⏳ PENDING
**Effort:** 8h
**Priority:** P1 (Critical - frontend depends on API)

---

## What Is Planned

### API Architecture
- [ ] Setup Minimal APIs with Carter library
- [ ] Implement global exception handling middleware
- [ ] Add rate limiting middleware
- [ ] Define repository interfaces and implementations
- [ ] Create endpoint groups for all resources
- [ ] Configure OpenAPI/Swagger documentation
- [ ] Setup NSwag for TypeScript client generation

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

### API Endpoints Structure

```
/api
├── /auth
│   ├── POST /register
│   ├── POST /login
│   ├── POST /refresh
│   └── GET  /me
├── /events
│   ├── GET  /                   # List (public, paginated)
│   ├── GET  /{id}              # Details
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
│   ├── POST /sepay/webhook     # SePay callback
│   └── GET  /{orderId}/status  # Payment status
├── /checkin
│   ├── POST /                   # Check in ticket
│   └── GET  /events/{id}/stats  # Check-in stats
├── /analytics
│   ├── GET  /events/{id}/summary
│   ├── GET  /events/{id}/sales-trend
│   └── GET  /events/{id}/export/*
└── /admin
    ├── GET  /stats             # Platform overview
    ├── GET  /users             # List users
    └── PUT  /users/{id}/role   # Change role
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

---

## Implementation Steps

### 1. Configure API Project (1.5h)
- [ ] Add NuGet packages: Carter, MediatR, FluentValidation
- [ ] Configure DI in Program.cs
- [ ] Setup Swagger middleware
- [ ] Configure JSON serialization

### 2. Create Middleware (1.5h)
- [ ] ExceptionHandlingMiddleware
- [ ] RateLimitingMiddleware
- [ ] Request logging middleware

### 3. Define Repository Interfaces (1h)
- [ ] IRepository<T> generic interface
- [ ] IEventRepository, IOrderRepository, etc.
- [ ] Query specifications pattern
- [ ] Pagination support

### 4. Implement Base Repositories (1.5h)
- [ ] BaseRepository<T> with CRUD
- [ ] Specific repository implementations
- [ ] Optimistic locking support
- [ ] Register in DI container

### 5. Create Endpoint Groups (2h)
- [ ] AuthEndpoints, EventEndpoints, CartEndpoints
- [ ] OrderEndpoints, PaymentEndpoints, CheckinEndpoints
- [ ] AnalyticsEndpoints, AdminEndpoints
- [ ] Request/response DTOs
- [ ] FluentValidation validators

### 6. Configure Type Generation (0.5h)
- [ ] Setup NSwag OpenAPI generation
- [ ] Test TypeScript client generation
- [ ] Add npm script in frontend

---

## Success Criteria

- [ ] All endpoints return correct HTTP status codes
- [ ] Validation errors return 400 with field details
- [ ] Swagger UI accessible at /swagger
- [ ] TypeScript types generate correctly
- [ ] Rate limiting blocks excessive requests

---

## Next Phase

After completion, proceed to **[Phase 04: Authentication](phase-04-summary.md)**
