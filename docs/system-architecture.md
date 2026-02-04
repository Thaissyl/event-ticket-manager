# System Architecture

**Last Updated:** 2026-02-04
**Version:** 1.0.0

## Overview

Event Ticket Manager follows a **Clean Architecture** pattern with clear separation between frontend (Next.js), backend (ASP.NET Core), and database (PostgreSQL). The system is containerized via Docker for consistent development and production environments.

## Architecture Principles

1. **Separation of Concerns**: Clear boundaries between presentation, business logic, and data layers
2. **Dependency Inversion**: Core domain logic independent of frameworks and infrastructure
3. **API-First Design**: Backend exposes OpenAPI-documented REST endpoints
4. **Type Safety**: C# as source of truth for TypeScript via NSwag generation
5. **Stateless Backend**: Horizontal scaling support, session state externalized (future Redis)

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (Browser)                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Next.js 15 Frontend (React 19)              │   │
│  │  - App Router (file-based routing)                  │   │
│  │  - shadcn/ui components (Radix + Tailwind)          │   │
│  │  - TypeScript (generated from backend)              │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTPS (REST API)
                        ▼
┌─────────────────────────────────────────────────────────────┐
│          ASP.NET Core 8 Backend (Minimal APIs)              │
│  ┌───────────────────────────────────────────────────┐     │
│  │              EventTickets.API                     │     │
│  │  - Endpoints (REST routes)                        │     │
│  │  - Middleware (auth, CORS, security headers)      │     │
│  │  - Swagger/OpenAPI docs                           │     │
│  └───────────────────┬───────────────────────────────┘     │
│                      │                                      │
│  ┌───────────────────▼───────────────────────────────┐     │
│  │           EventTickets.Core (Domain)              │     │
│  │  - Entities (POCOs)                               │     │
│  │  - Interfaces (repositories, services)            │     │
│  │  - Business logic                                 │     │
│  └───────────────────┬───────────────────────────────┘     │
│                      │                                      │
│  ┌───────────────────▼───────────────────────────────┐     │
│  │    EventTickets.Infrastructure (Data Access)      │     │
│  │  - DbContext (EF Core)                            │     │
│  │  - Repositories (data operations)                 │     │
│  │  - External services (SePay, email)               │     │
│  └───────────────────┬───────────────────────────────┘     │
└────────────────────────┬────────────────────────────────────┘
                         │ EF Core (ORM)
                         ▼
┌─────────────────────────────────────────────────────────────┐
│               PostgreSQL 16 Database                        │
│  - Relational schema with indexes                           │
│  - Optimistic locking (RowVersion)                          │
│  - ACID transactions                                        │
└─────────────────────────────────────────────────────────────┘

External:
┌──────────────┐
│   SePay API  │ (Webhook callbacks for payment confirmation)
└──────────────┘
```

## Clean Architecture Layers

### Layer 1: EventTickets.Core (Domain Layer)

**Purpose**: Framework-agnostic business logic and domain models

**Responsibilities**:
- Define entities (Event, Ticket, User, Order, etc.)
- Declare repository and service interfaces
- Business rules and validation logic
- Domain events (future)

**Dependencies**: None (pure C#)

**Key Principles**:
- No framework dependencies (no EF Core, ASP.NET references)
- POCOs (Plain Old C# Objects)
- Interface-based contracts for infrastructure

**Example Structure**:
```
EventTickets.Core/
├── Entities/
│   ├── ApplicationUser.cs
│   ├── Event.cs
│   ├── TicketTier.cs
│   ├── Order.cs
│   ├── Ticket.cs
│   ├── Payment.cs
│   └── PromoCode.cs
├── Interfaces/
│   ├── IEventRepository.cs
│   ├── IOrderRepository.cs
│   ├── IPaymentService.cs
│   └── IQrCodeGenerator.cs
├── Services/
│   └── InventoryService.cs (business logic)
└── Enums/
    ├── UserRole.cs
    ├── OrderStatus.cs
    └── PaymentStatus.cs
```

### Layer 2: EventTickets.Infrastructure (Data + External Services)

**Purpose**: Implementation of interfaces defined in Core layer

**Responsibilities**:
- EF Core DbContext configuration
- Repository implementations
- Database migrations
- External service integrations (SePay, email)
- File storage operations

**Dependencies**: EventTickets.Core, EF Core, third-party SDKs

**Key Principles**:
- Implements interfaces from Core
- No direct usage from API (accessed via DI)
- Encapsulates data access details

**Example Structure**:
```
EventTickets.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── EventConfiguration.cs (EF fluent API)
│   │   └── OrderConfiguration.cs
│   └── Migrations/ (auto-generated)
├── Repositories/
│   ├── EventRepository.cs
│   └── OrderRepository.cs
├── Services/
│   ├── SePayService.cs
│   ├── EmailService.cs
│   └── QrCodeGenerator.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs (DI registration)
```

### Layer 3: EventTickets.API (Presentation Layer)

**Purpose**: HTTP interface exposing REST endpoints

**Responsibilities**:
- Minimal API endpoint definitions
- Request/response DTOs
- Authentication/authorization middleware
- CORS configuration
- Swagger/OpenAPI documentation
- Security headers

**Dependencies**: EventTickets.Core, EventTickets.Infrastructure

**Key Principles**:
- Thin layer, delegates to Core services
- DTOs separate from domain entities
- OpenAPI documentation via attributes

**Example Structure**:
```
EventTickets.API/
├── Endpoints/
│   ├── EventEndpoints.cs
│   ├── TicketEndpoints.cs
│   ├── OrderEndpoints.cs
│   ├── PaymentEndpoints.cs
│   └── AuthEndpoints.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── DTOs/
│   ├── Requests/
│   │   ├── CreateEventRequest.cs
│   │   └── CreateOrderRequest.cs
│   └── Responses/
│       ├── EventResponse.cs
│       └── OrderResponse.cs
├── Program.cs (entry point, DI, middleware pipeline)
└── appsettings.json
```

## Data Flow Examples

### Event Creation Flow

```
1. User submits form → Next.js client validates
2. POST /api/v1/events → EventEndpoints.cs
3. Map DTO → Event entity
4. eventRepository.CreateAsync(event)
5. DbContext.SaveChangesAsync() → PostgreSQL INSERT
6. Return EventResponse DTO
7. Frontend updates UI
```

### Ticket Purchase Flow (Complex)

```
1. User adds tickets → POST /api/v1/cart/items
2. CartService.AddItem() → Create CartReservation (temp inventory hold)
3. User proceeds → POST /api/v1/orders
4. OrderService.CreateOrder():
   - Validate cart reservations not expired
   - Check inventory via optimistic locking (RowVersion)
   - Create Order entity (status: PendingPayment)
   - Generate Payment entity with unique ID
5. SePayService.GeneratePaymentQR(paymentId)
6. Return QR code + instructions to frontend
7. User scans QR, completes bank transfer
8. SePay webhook → POST /api/webhooks/sepay
9. PaymentService.ConfirmPayment():
   - Validate webhook signature
   - Update Payment status → Completed
   - Update Order status → Confirmed
   - Generate Ticket entities with QR codes
10. EmailService.SendTickets() (future)
11. Frontend polls order status, shows tickets
```

## Technology Stack Details

### Frontend Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Framework | Next.js | 16.1.6 | React SSR/SSG framework with App Router |
| UI Library | React | 19.2.3 | Component-based UI |
| Language | TypeScript | 5.x | Type safety |
| Styling | Tailwind CSS | 4.x | Utility-first CSS |
| Components | shadcn/ui | Latest | Pre-built accessible components (Radix) |
| Icons | lucide-react | 0.563.0 | Icon library |
| State | React Context | Built-in | Global state (auth, cart) |
| Forms | React Hook Form | TBD | Form validation |
| HTTP Client | fetch API | Native | API requests |
| Build Output | Standalone | Next.js | Self-contained deployment |

**Key Configuration**:
- **next.config.ts**: `output: 'standalone'` for Docker
- **App Router**: File-based routing in `src/app/`
- **TypeScript**: Generated types from backend via NSwag

### Backend Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Framework | ASP.NET Core | 8.0 | Web API framework |
| Language | C# | 12 | Statically typed language |
| API Style | Minimal APIs | Built-in | Lightweight endpoint definitions |
| ORM | EF Core | 8.0 | Database abstraction |
| Database | PostgreSQL | 16 | Relational database |
| Auth | ASP.NET Identity | 8.0 | User management, roles |
| Tokens | JWT | Built-in | Stateless authentication |
| OpenAPI | Swashbuckle | 6.6.2 | API documentation |
| Type Gen | NSwag | TBD | C# to TypeScript |

**Key Configuration**:
- **Program.cs**: Middleware pipeline, DI, endpoints
- **Clean Architecture**: 3-layer separation
- **Connection Pooling**: Default EF Core behavior

### Database Schema

**Key Tables**:
- `AspNetUsers`: Extended with ApplicationUser (ASP.NET Identity)
- `Events`: Event metadata, organizer relation
- `TicketTiers`: Pricing tiers per event
- `Orders`: Customer purchases
- `Tickets`: Individual QR-coded tickets
- `Payments`: SePay transaction tracking
- `CartReservations`: Temporary inventory holds
- `PromoCodes`: Discount codes
- `SePayWebhooks`: Webhook event log

**Indexes**:
- `Events.OrganizerId, Events.StartDate`
- `Orders.UserId, Orders.CreatedAt`
- `Tickets.QrCode` (unique)
- `CartReservations.ExpiresAt` (for cleanup job)

**Optimistic Locking**:
- `TicketTiers.RowVersion` (timestamp) prevents overselling

## Security Architecture

### Authentication Flow

```
1. User login → POST /api/auth/login (email, password)
2. ASP.NET Identity validates credentials
3. Generate JWT token (claims: userId, role, email)
4. Return token + refresh token to client
5. Client stores in httpOnly cookie or localStorage
6. Subsequent requests include Authorization: Bearer <token>
7. JWT middleware validates signature, extracts claims
8. Endpoint checks [Authorize] attribute or role
```

### Security Measures (Implemented - Phase 01)

- **HTTPS Redirection**: Enforced in production
- **CORS Configuration**: Environment-configurable origins (`AllowedOrigins`)
- **Security Headers**:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `X-XSS-Protection: 1; mode=block`
  - `Referrer-Policy: no-referrer`
  - `Content-Security-Policy: default-src 'self'...`
- **API Key Auth**: Swagger UI protected
- **Input Validation**: DTO validation via DataAnnotations
- **SQL Injection Prevention**: Parameterized queries via EF Core

### Security Measures (Planned)

- Rate limiting on auth endpoints
- Request size limits
- Webhook signature validation (SePay)
- Environment variable encryption
- Dependency vulnerability scanning

## API Design

### REST Conventions

- **Versioning**: URL-based (`/api/v1/...`)
- **HTTP Methods**: GET (read), POST (create), PUT (update), DELETE (remove)
- **Status Codes**:
  - 200 OK (success)
  - 201 Created (resource created)
  - 400 Bad Request (validation error)
  - 401 Unauthorized (missing/invalid token)
  - 403 Forbidden (insufficient permissions)
  - 404 Not Found
  - 409 Conflict (optimistic lock failure)
  - 500 Internal Server Error
- **Response Format**: JSON
- **Pagination**: Query params `?page=1&pageSize=20`
- **Filtering**: Query params `?status=published&startDate=2026-01-01`

### Example Endpoints (Planned)

```
GET    /health                          # System health check
GET    /api/v1/info                     # API version info

POST   /api/v1/auth/register            # User registration
POST   /api/v1/auth/login               # User login
POST   /api/v1/auth/refresh             # Refresh JWT

GET    /api/v1/events                   # List events (public)
GET    /api/v1/events/{id}              # Event details
POST   /api/v1/events                   # Create event (organizer)
PUT    /api/v1/events/{id}              # Update event (organizer)
DELETE /api/v1/events/{id}              # Delete event (organizer)

GET    /api/v1/events/{id}/tiers        # List ticket tiers
POST   /api/v1/events/{id}/tiers        # Add tier (organizer)

POST   /api/v1/cart/items               # Add to cart
DELETE /api/v1/cart/items/{id}          # Remove from cart
GET    /api/v1/cart                     # View cart

POST   /api/v1/orders                   # Create order (from cart)
GET    /api/v1/orders/{id}              # Order details
GET    /api/v1/orders                   # User's orders

POST   /api/webhooks/sepay              # SePay payment webhook

GET    /api/v1/tickets/{qrCode}/verify  # Check-in verification
POST   /api/v1/tickets/{qrCode}/checkin # Mark checked in

GET    /api/v1/analytics/sales          # Sales dashboard (organizer)
```

### OpenAPI Documentation

- **Swagger UI**: Available at `/swagger` (dev only)
- **JSON Spec**: Available at `/swagger/v1/swagger.json`
- **NSwag Integration**: Generates TypeScript client from spec

## Deployment Architecture

### Development Environment (Docker Compose)

```yaml
# docker-compose.dev.yml
services:
  postgres:       # Port 5432
  backend:        # Port 5000, dotnet watch (hot reload)
  frontend:       # Port 3000, npm run dev (hot reload)
```

**Features**:
- Volume mounts for source code
- Hot reload for both frontend and backend
- Hardcoded development credentials (not in production)
- No HTTPS (localhost only)

### Production Environment (Planned)

**Infrastructure**:
- Cloud provider: Azure, AWS, or DigitalOcean
- Load balancer with SSL termination
- Multiple backend instances (horizontal scaling)
- Managed PostgreSQL database
- Redis for session storage
- Object storage (S3-compatible) for images

**CI/CD**:
- GitHub Actions (planned)
- Automated tests before deploy
- Docker image build and push
- Rolling deployment with health checks

## Scalability Considerations

### Current Architecture (Supports ~1000 concurrent users)

- Stateless backend (JWT tokens, no server-side sessions)
- Connection pooling via EF Core
- Database indexes on common queries

### Future Enhancements

- **Caching**: Redis for frequently accessed data (event listings)
- **CDN**: Static assets (images, JS bundles)
- **Database Read Replicas**: Separate read traffic
- **Queue System**: Background jobs (email sending, analytics aggregation)
- **Microservices**: Split payment/ticketing into separate services (if needed)

## Monitoring and Observability (Planned)

- **Health Checks**: `/health` endpoint for uptime monitoring
- **Logging**: Structured logs via Serilog, sent to centralized system
- **Metrics**: Request count, latency, error rate (Prometheus/Grafana)
- **Alerts**: Critical errors, payment failures, high latency

## Data Backup and Recovery

- **Automated Backups**: Daily PostgreSQL dumps
- **Retention**: 30 days
- **Point-in-Time Recovery**: Transaction log archival (future)
- **Disaster Recovery**: Cross-region replication (future)

## Performance Targets

| Metric | Target | Current Status |
|--------|--------|----------------|
| Page Load (p95) | < 2s | TBD (not measured) |
| API Response (p95) | < 500ms | TBD (not measured) |
| Database Query (p95) | < 100ms | TBD (not measured) |
| Concurrent Users | 1000 | Not tested |
| Payment Confirmation | < 2min | Dependent on SePay |
| Ticket Generation | < 5min | Not implemented |

## Technology Decisions

### Why Next.js 15?

- **App Router**: Modern file-based routing with React Server Components
- **TypeScript Native**: Built-in support
- **Performance**: Automatic code splitting, image optimization
- **SEO**: Server-side rendering for public event pages
- **Developer Experience**: Hot reload, error overlays

### Why ASP.NET Core 8 Minimal APIs?

- **Performance**: High throughput, low latency
- **Type Safety**: C# strong typing
- **Ecosystem**: Mature libraries (EF Core, Identity)
- **Minimal APIs**: Less boilerplate than controllers
- **NSwag**: Seamless TypeScript generation

### Why PostgreSQL 16?

- **Open Source**: No licensing costs
- **ACID Compliance**: Strong data consistency
- **Performance**: Excellent for read-heavy workloads
- **JSON Support**: Flexible for future schema changes
- **Tooling**: Mature ecosystem (pgAdmin, monitoring)

### Why Clean Architecture?

- **Testability**: Core logic isolated from frameworks
- **Flexibility**: Swap infrastructure without changing business logic
- **Clarity**: Clear separation of concerns
- **Maintainability**: Easier to onboard new developers

## Integration Points

### SePay Integration

**Communication**: REST API + Webhooks
**Flow**:
1. Backend calls SePay API to generate payment QR
2. User completes bank transfer
3. SePay sends webhook to `/api/webhooks/sepay`
4. Backend validates signature, updates order

**Error Handling**:
- Webhook retry mechanism (SePay retries up to 3 times)
- Idempotency via unique payment ID
- Manual reconciliation dashboard for failed webhooks

### Email Service (Future)

**Communication**: SMTP or REST API (SendGrid/Mailgun)
**Triggers**:
- Order confirmation
- Ticket delivery
- Event reminders
- Password reset

## Constraints and Limitations

### Known Limitations

- **SePay Refunds**: Manual process via bank portal (no API)
- **Offline Check-In**: Not supported in v1.0 (requires network)
- **Image Storage**: Local filesystem (not scalable, no CDN)
- **Session Management**: Client-side JWT (no server-side revocation)

### Technical Debt (Accepted for v1.0)

- No caching layer (Redis planned for v1.1)
- Basic analytics (advanced features in v1.2)
- No email service integration (v1.1)
- Manual deployment (CI/CD in v1.1)

## References

- [Project Overview PDR](./project-overview-pdr.md)
- [Code Standards](./code-standards.md)
- [Deployment Guide](./deployment-guide.md)
- [Architecture Research Report](../plans/reports/researcher-260202-1222-nextjs-dotnet-architecture.md)
