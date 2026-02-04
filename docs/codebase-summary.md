# Codebase Summary

**Generated:** 2026-02-04
**Version:** 1.0.0
**Total Files:** 117 (excluding binary)
**Total Tokens:** 72,613
**Total Chars:** 271,791

## Project Structure

```
event-ticket-manager/
├── src/
│   ├── backend/              # ASP.NET Core 8 API
│   │   ├── EventTickets.API/          # HTTP presentation layer
│   │   ├── EventTickets.Core/         # Domain logic
│   │   ├── EventTickets.Infrastructure/  # Data access
│   │   └── EventTickets.sln           # Solution file
│   └── frontend/             # Next.js 15 frontend
│       ├── src/
│       │   ├── app/          # App Router pages
│       │   ├── components/   # React components
│       │   └── lib/          # Utilities
│       ├── Dockerfile
│       └── package.json
├── plans/                    # Implementation plans
│   ├── 260202-1213-event-ticket-manager/  # Main plan directory
│   │   ├── plan.md           # Master plan overview
│   │   ├── phase-01-project-setup.md
│   │   ├── phase-02-database-schema.md
│   │   └── ... (11 phases total)
│   └── reports/              # Research and review reports
├── tools/
│   └── nswag.json            # TypeScript type generation config
├── docs/                     # Documentation
├── docker-compose.yml        # Production deployment
├── docker-compose.dev.yml    # Development environment
└── .env.example              # Environment variable template
```

## Technology Stack

### Backend

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Framework | ASP.NET Core | 8.0 | Web API framework |
| Language | C# | 12 | Statically typed language |
| API Style | Minimal APIs | Built-in | Lightweight endpoints |
| ORM | Entity Framework Core | 8.0 | Database abstraction |
| Database | PostgreSQL | 16 | Relational database |
| Auth | ASP.NET Identity | 8.0 | User management |
| OpenAPI | Swashbuckle | 6.6.2 | API documentation |

**Projects**:
- **EventTickets.API**: Entry point, endpoints, middleware, DTOs
- **EventTickets.Core**: Domain entities, interfaces, business logic
- **EventTickets.Infrastructure**: EF Core, repositories, external services

### Frontend

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Framework | Next.js | 16.1.6 | React SSR/SSG framework |
| UI Library | React | 19.2.3 | Component-based UI |
| Language | TypeScript | 5.x | Type safety |
| Styling | Tailwind CSS | 4.x | Utility-first CSS |
| Components | shadcn/ui | Latest | Pre-built components |
| Icons | lucide-react | 0.563.0 | Icon library |

**Structure**:
- **src/app**: App Router pages (layout.tsx, page.tsx)
- **src/components/ui**: shadcn/ui components (button, card, input, label)
- **src/lib**: Utility functions (utils.ts)

## Key Files Overview

### Backend Files

#### Program.cs (EventTickets.API)
**Location**: `src/backend/EventTickets.API/Program.cs`
**Lines**: 85
**Purpose**: Application entry point, DI configuration, middleware pipeline

**Key Features**:
- Swagger/OpenAPI documentation with API key auth
- Environment-configurable CORS origins (`AllowedOrigins` env var)
- Security headers middleware (CSP, X-Frame-Options, X-XSS-Protection)
- HTTPS redirection in production
- Health check endpoint: `GET /health`
- API info endpoint: `GET /api/v1/info`

**Configuration**:
```csharp
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")
    ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(','))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

#### EventTickets.API.csproj
**Location**: `src/backend/EventTickets.API/EventTickets.API.csproj`
**Target Framework**: .NET 8.0
**Dependencies**:
- `Microsoft.AspNetCore.OpenApi` (8.0.22)
- `Swashbuckle.AspNetCore` (6.6.2)
- Project references: Core, Infrastructure

#### EventTickets.Core.csproj
**Location**: `src/backend/EventTickets.Core/EventTickets.Core.csproj`
**Target Framework**: .NET 8.0
**Dependencies**: None (pure domain logic)

#### EventTickets.Infrastructure.csproj
**Location**: `src/backend/EventTickets.Infrastructure/EventTickets.Infrastructure.csproj`
**Target Framework**: .NET 8.0
**Dependencies**: Project reference to Core

#### Class1.cs (Placeholder)
**Location**: `src/backend/EventTickets.Core/Class1.cs`, `EventTickets.Infrastructure/Class1.cs`
**Status**: Auto-generated placeholder, to be replaced with actual implementations

### Frontend Files

#### package.json
**Location**: `src/frontend/package.json`
**Scripts**:
- `dev`: Start development server (`next dev`)
- `build`: Production build (`next build`)
- `start`: Start production server (`next start`)
- `lint`: Run ESLint
- `generate-types`: Run NSwag type generation from backend OpenAPI spec

**Dependencies**:
- `next`: 16.1.6
- `react`: 19.2.3
- `react-dom`: 19.2.3
- `class-variance-authority`: 0.7.1 (for shadcn/ui)
- `clsx`: 2.1.1 (class utility)
- `lucide-react`: 0.563.0 (icons)
- `tailwind-merge`: 3.4.0 (Tailwind class merging)

**Dev Dependencies**:
- `@tailwindcss/postcss`: 4.x
- `typescript`: 5.x
- `eslint`: 9.x
- `tailwindcss`: 4.x

#### next.config.ts
**Location**: `src/frontend/next.config.ts`
**Key Settings**:
- `output: 'standalone'`: Self-contained build for Docker deployment
- `NEXT_PUBLIC_API_URL`: Environment variable for backend API URL (default: `http://localhost:5000`)

#### layout.tsx (Root Layout)
**Location**: `src/frontend/src/app/layout.tsx`
**Purpose**: Root HTML structure, fonts, metadata

**Features**:
- Geist Sans and Geist Mono fonts
- Global CSS import
- Metadata placeholder (to be customized)

#### page.tsx (Home Page)
**Location**: `src/frontend/src/app/page.tsx`
**Purpose**: Landing page with event browsing and organizer dashboard buttons

**Components Used**:
- `Button` (shadcn/ui)
- `Card`, `CardHeader`, `CardTitle`, `CardDescription`, `CardContent` (shadcn/ui)

**Design**:
- Gradient background (zinc-50 to zinc-100, dark mode: zinc-900 to black)
- Centered card layout
- Placeholder buttons for "Browse Events" and "Organizer Dashboard"

#### shadcn/ui Components
**Location**: `src/frontend/src/components/ui/`
**Files**:
- `button.tsx`: Button component with variants (default, destructive, outline, secondary, ghost, link)
- `card.tsx`: Card container components (Card, CardHeader, CardFooter, CardTitle, CardDescription, CardContent)
- `input.tsx`: Input field component
- `label.tsx`: Label component for form fields

**Note**: These are pre-built components from shadcn/ui. DO NOT modify directly; extend via composition.

#### lib/utils.ts
**Location**: `src/frontend/src/lib/utils.ts`
**Purpose**: Utility function for merging Tailwind classes

```typescript
import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

### Configuration Files

#### .env.example
**Location**: `.env.example`
**Purpose**: Environment variable template (never commit actual `.env`)

**Sections**:
- **Database**: PostgreSQL connection (host, port, database, user, password)
- **Backend**: ASP.NET Core environment, URLs, CORS origins, JWT config
- **Frontend**: Next.js API URL, NextAuth config
- **SePay**: Payment API token, account number, webhook secret

#### docker-compose.yml (Production)
**Location**: `docker-compose.yml`
**Services**:
- `postgres`: PostgreSQL 16 Alpine, port 5432, health checks
- `backend`: ASP.NET Core, port 5000, depends on postgres
- `frontend`: Next.js, port 3000, depends on backend

**Features**:
- Environment variable substitution (e.g., `${POSTGRES_DB:-event_tickets}`)
- Named volumes for data persistence
- Health checks for service orchestration

#### docker-compose.dev.yml (Development)
**Location**: `docker-compose.dev.yml`
**Services**: Same as production, with additions:
- **backend**: Volume mounts for hot reload (`dotnet watch`)
- **frontend**: Volume mounts for hot reload (`npm run dev`), `WATCHPACK_POLLING=true`

**Differences**:
- Hardcoded development credentials (postgres/postgres)
- Source code mounted as volumes
- Separate volume for postgres data (`postgres_dev_data`)

#### Dockerfiles

**Backend Dockerfile** (`src/backend/Dockerfile`):
- Multi-stage build (restore, build, publish, runtime)
- Final image: `mcr.microsoft.com/dotnet/aspnet:8.0`

**Backend Dockerfile.dev** (`src/backend/Dockerfile.dev`):
- Development image with SDK for `dotnet watch`

**Frontend Dockerfile** (`src/frontend/Dockerfile`):
- Multi-stage build (dependencies, builder, runtime)
- Standalone output copied to runtime image
- Runs on port 3000 with `node server.js`

**Frontend Dockerfile.dev** (`src/frontend/Dockerfile.dev`):
- Development image with hot reload

#### .dockerignore
**Backend** (`src/backend/.dockerignore`):
- Excludes `bin/`, `obj/`, `node_modules/`, `.git/`, etc.

**Frontend** (`src/frontend/.dockerignore`):
- Excludes `.next/`, `node_modules/`, `.git/`, etc.

### Planning and Reports

#### plan.md
**Location**: `plans/260202-1213-event-ticket-manager/plan.md`
**Purpose**: Master implementation plan overview

**Content**:
- Project overview and tech stack
- 11 phases with status, effort, and file links
- Dependencies between phases
- Critical decisions (REST over GraphQL, optimistic locking, NSwag)

**Phases**:
1. Project Setup (4h) - **needs-verification**
2. Database Schema (6h) - pending
3. Backend API Structure (8h) - pending
4. Authentication (8h) - pending
5. Event Management (10h) - pending
6. Ticket Purchasing (10h) - pending
7. SePay Payment (8h) - pending
8. QR Tickets & Check-in (6h) - pending
9. Analytics Dashboard (8h) - pending
10. Admin Panel (6h) - pending
11. Testing & Deployment (6h) - pending

#### Phase Files
**Location**: `plans/260202-1213-event-ticket-manager/phase-*.md`
**Structure** (consistent across all phases):
- Context links
- Overview (priority, status, effort, description)
- Key insights
- Requirements (functional and non-functional)
- Architecture (diagrams, entity structures)
- Related code files
- Implementation steps
- Todo list
- Success criteria
- Risk assessment
- Security considerations
- Next steps

**Example: phase-01-project-setup.md**:
- Status: **needs-verification** (Phase 01 complete, but security fixes needed)
- Security fixes identified by code review:
  - CORS origins now environment-configurable ✓
  - Security headers added ✓
  - Standalone output configured ✓
  - .dockerignore files created ✓
  - HTTPS redirection for production ✓

#### Research Reports
**Location**: `plans/reports/researcher-*.md`

**researcher-260202-1222-nextjs-dotnet-architecture.md**:
- Best practices for Next.js + ASP.NET Core integration
- Clean Architecture patterns
- Type generation strategies (NSwag recommended)

**researcher-260202-1222-event-ticketing-features.md**:
- Ticketing platform feature comparison
- Ticket tier strategies
- Cart reservation mechanisms
- QR code generation best practices

**researcher-260202-1222-sepay-integration.md**:
- SePay API overview
- VietQR payment flow
- Webhook implementation guide
- Manual refund process (no API)

#### Code Review Report
**Location**: `plans/reports/code-reviewer-260203-2046-phase01-setup.md`
**Score**: 6.5/10
**Status**: Security issues identified, fixes implemented

**Key Findings**:
- ✓ CORS origins now configurable
- ✓ Security headers added
- ✓ Standalone Next.js output
- ✓ .dockerignore files created
- ✓ HTTPS redirection middleware
- ⚠️ Docker verification pending

### Tools

#### nswag.json
**Location**: `tools/nswag.json`
**Purpose**: Configuration for generating TypeScript types from C# OpenAPI spec

**Planned Flow**:
1. Backend generates OpenAPI spec at `/swagger/v1/swagger.json`
2. NSwag reads spec and generates TypeScript client
3. Output: `src/frontend/src/types/api.ts` and `src/frontend/src/lib/api-client.ts`

## Dependencies

### Backend NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.OpenApi | 8.0.22 | OpenAPI spec generation |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger UI |

**Planned Additions**:
- `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core provider)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (auth)
- `System.IdentityModel.Tokens.Jwt` (JWT generation)

### Frontend NPM Packages

**Production Dependencies** (12 total):
- Core: `next`, `react`, `react-dom`
- UI: `class-variance-authority`, `clsx`, `lucide-react`, `radix-ui`, `tailwind-merge`

**Dev Dependencies** (8 total):
- Build: `@tailwindcss/postcss`, `tailwindcss`, `tw-animate-css`
- TypeScript: `typescript`, `@types/node`, `@types/react`, `@types/react-dom`
- Linting: `eslint`, `eslint-config-next`

## Current Implementation Status

### Completed (Phase 01)

- [x] Monorepo structure
- [x] Next.js 15 frontend with shadcn/ui
- [x] ASP.NET Core 8 backend with Clean Architecture
- [x] Docker Compose for development and production
- [x] Environment variable configuration
- [x] CORS with environment-configurable origins
- [x] Security headers middleware
- [x] HTTPS redirection for production
- [x] Health check and API info endpoints
- [x] .dockerignore files for both frontend and backend
- [x] Standalone Next.js output for Docker

### Pending (Phases 02-11)

- [ ] Database schema with EF Core
- [ ] NSwag type generation pipeline
- [ ] Authentication (ASP.NET Identity + JWT)
- [ ] Event and ticket tier management
- [ ] Shopping cart with reservations
- [ ] SePay payment integration
- [ ] QR code generation and check-in
- [ ] Analytics dashboard
- [ ] Admin panel
- [ ] Comprehensive testing

## Key Design Decisions

### 1. REST over GraphQL
**Rationale**: Simpler for flat event/ticket models, better HTTP caching, less overhead

### 2. Optimistic Locking + Cart Reservations
**Rationale**: Prevents overselling via database-level version control and temporary inventory holds

### 3. NSwag for Type Generation
**Rationale**: C# as single source of truth, eliminates type drift between frontend and backend

### 4. Clean Architecture
**Rationale**: Testability, framework independence, clear separation of concerns

### 5. Standalone Next.js Output
**Rationale**: Self-contained deployment for Docker, no external dependencies

### 6. Manual Refunds (SePay)
**Rationale**: SePay lacks refund API, refunds handled via bank portal

## Development Workflow

### Local Setup
1. Copy `.env.example` to `.env` and configure values
2. Run `docker-compose -f docker-compose.dev.yml up`
3. Access frontend: `http://localhost:3000`
4. Access backend: `http://localhost:5000`
5. Access Swagger: `http://localhost:5000/swagger`

### Hot Reload
- **Backend**: `dotnet watch` automatically recompiles on file changes
- **Frontend**: Next.js dev server watches `src/` directory

### Type Generation (Planned)
```bash
cd src/frontend
npm run generate-types
```
Generates TypeScript types from backend OpenAPI spec.

## Security Considerations

### Implemented
- Environment-configurable CORS origins
- Security headers (CSP, X-Frame-Options, X-XSS-Protection)
- HTTPS redirection in production
- API key authentication for Swagger
- .dockerignore to prevent sensitive file inclusion

### Planned
- JWT token authentication
- Rate limiting on auth endpoints
- Webhook signature validation (SePay)
- Input validation via DataAnnotations
- SQL injection prevention via EF Core parameterized queries

## Performance Considerations

### Current
- Next.js automatic code splitting
- Standalone output reduces deployment size
- PostgreSQL connection pooling (EF Core default)

### Planned
- Database indexes on common queries
- Redis caching for session data
- CDN for static assets
- Horizontal scaling support (stateless backend)

## Known Limitations

1. **SePay Refunds**: Manual process, no API
2. **Offline Check-In**: Not supported in v1.0
3. **Image Storage**: Local filesystem (not scalable)
4. **Session Management**: Client-side JWT (no server-side revocation in v1.0)

## Next Steps

1. **Fix Phase 01 Issues**: Verify Docker services start successfully
2. **Phase 02**: Implement database schema with EF Core
3. **NSwag Setup**: Configure type generation pipeline
4. **Authentication**: ASP.NET Identity + JWT implementation

## Glossary

- **Clean Architecture**: Layered design (Core, Infrastructure, API) for testability
- **Minimal APIs**: ASP.NET Core lightweight endpoint style
- **App Router**: Next.js 13+ file-based routing system
- **shadcn/ui**: Pre-built accessible components using Radix UI + Tailwind
- **Optimistic Locking**: Concurrency control via version/timestamp
- **NSwag**: OpenAPI toolchain for generating TypeScript from C#
- **SePay**: Vietnamese payment gateway for VietQR bank transfers

## File Metrics

**Top 5 Files by Token Count**:
1. `src/backend/EventTickets.API/obj/project.assets.json` (7,141 tokens, 9.8%)
2. `plans/reports/code-reviewer-260203-2046-phase01-setup.md` (3,308 tokens, 4.6%)
3. `plans/260202-1213-event-ticket-manager/phase-02-database-schema.md` (2,414 tokens, 3.3%)
4. `plans/260202-1213-event-ticket-manager/phase-11-testing-deployment.md` (2,410 tokens, 3.3%)
5. `plans/reports/researcher-260202-1222-sepay-integration.md` (2,404 tokens, 3.3%)

## References

- [Project Overview PDR](./project-overview-pdr.md)
- [System Architecture](./system-architecture.md)
- [Code Standards](./code-standards.md)
- [Main Implementation Plan](../plans/260202-1213-event-ticket-manager/plan.md)
