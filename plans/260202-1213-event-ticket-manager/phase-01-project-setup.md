# Phase 01: Project Setup

## Context Links
- [Main Plan](plan.md)
- [Architecture Report](../reports/researcher-260202-1222-nextjs-dotnet-architecture.md)

## Overview
- **Priority:** P1 (Critical - blocks all other phases)
- **Status:** pending
- **Effort:** 4h
- **Description:** Initialize monorepo structure, configure Docker, setup development environment

## Key Insights
- Monorepo with `src/frontend` and `src/backend` directories
- Docker Compose for local dev (Next.js + ASP.NET Core + PostgreSQL)
- NSwag for C# to TypeScript type generation
- Hot reloading for both frontend and backend

## Requirements

### Functional
- F1: Monorepo structure with clear separation
- F2: Docker Compose for local development
- F3: Environment variable configuration
- F4: Type generation pipeline

### Non-Functional
- NF1: Hot reload in development
- NF2: Consistent environment across machines
- NF3: Fast startup time (<30s)

## Architecture

```
event-ticket-manager/
├── src/
│   ├── frontend/                 # Next.js 14 App Router
│   │   ├── app/
│   │   │   ├── (auth)/          # Auth pages
│   │   │   ├── (dashboard)/     # Organizer dashboard
│   │   │   ├── (public)/        # Public event pages
│   │   │   ├── admin/           # Admin panel
│   │   │   └── api/             # API routes
│   │   ├── components/
│   │   │   ├── ui/              # shadcn/ui components
│   │   │   └── shared/          # Shared components
│   │   ├── lib/
│   │   │   ├── api-client.ts    # Generated API client
│   │   │   ├── auth.ts          # NextAuth config
│   │   │   └── utils.ts
│   │   ├── types/               # Generated from backend
│   │   ├── next.config.js
│   │   ├── tailwind.config.js
│   │   └── package.json
│   └── backend/
│       ├── EventTickets.API/     # Web API project
│       │   ├── Endpoints/        # Minimal API endpoints
│       │   ├── Middleware/
│       │   └── Program.cs
│       ├── EventTickets.Core/    # Domain layer
│       │   ├── Entities/
│       │   ├── Interfaces/
│       │   └── Services/
│       ├── EventTickets.Infrastructure/  # Data access
│       │   ├── Data/
│       │   ├── Repositories/
│       │   └── Services/
│       └── EventTickets.sln
├── tools/
│   └── nswag.json               # Type generation config
├── docker-compose.yml
├── docker-compose.dev.yml
├── .env.example
├── .gitignore
└── README.md
```

## Related Code Files

### Create
- `src/frontend/package.json`
- `src/frontend/next.config.js`
- `src/frontend/tailwind.config.js`
- `src/frontend/tsconfig.json`
- `src/frontend/app/layout.tsx`
- `src/frontend/app/page.tsx`
- `src/backend/EventTickets.sln`
- `src/backend/EventTickets.API/Program.cs`
- `src/backend/EventTickets.API/EventTickets.API.csproj`
- `src/backend/EventTickets.Core/EventTickets.Core.csproj`
- `src/backend/EventTickets.Infrastructure/EventTickets.Infrastructure.csproj`
- `docker-compose.yml`
- `docker-compose.dev.yml`
- `.env.example`
- `tools/nswag.json`

## Implementation Steps

### 1. Initialize Monorepo Structure (1h)
- [ ] Create directory structure as per architecture
- [ ] Initialize git repository
- [ ] Create root `.gitignore` with Node, .NET, IDE patterns
- [ ] Create `.env.example` with all required variables

### 2. Setup Next.js Frontend (1h)
- [ ] Initialize Next.js 14 with TypeScript: `npx create-next-app@latest`
- [ ] Configure App Router structure
- [ ] Install and configure Tailwind CSS
- [ ] Install shadcn/ui: `npx shadcn@latest init`
- [ ] Create basic layout and page components
- [ ] Configure environment variables for API URL

### 3. Setup ASP.NET Core Backend (1h)
- [ ] Create solution: `dotnet new sln -n EventTickets`
- [ ] Create API project: `dotnet new webapi -n EventTickets.API --use-minimal-apis`
- [ ] Create Core library: `dotnet new classlib -n EventTickets.Core`
- [ ] Create Infrastructure library: `dotnet new classlib -n EventTickets.Infrastructure`
- [ ] Add project references
- [ ] Configure NSwag for OpenAPI generation
- [ ] Setup basic health check endpoint

### 4. Configure Docker (1h)
- [ ] Create `Dockerfile` for frontend (multi-stage build)
- [ ] Create `Dockerfile` for backend
- [ ] Create `docker-compose.yml` for production
- [ ] Create `docker-compose.dev.yml` with:
  - PostgreSQL 16
  - Next.js with volume mounts (hot reload)
  - ASP.NET Core with volume mounts (dotnet watch)
- [ ] Test `docker-compose up` starts all services
- [ ] Verify services can communicate

### 5. Setup Type Generation Pipeline
- [ ] Configure NSwag in `tools/nswag.json`
- [ ] Add npm script `generate-types` in frontend
- [ ] Test type generation produces valid TypeScript
- [ ] Add pre-build hook to regenerate types

## Todo List
- [ ] Create monorepo directory structure
- [ ] Initialize Next.js 14 project
- [ ] Initialize ASP.NET Core solution
- [ ] Configure Tailwind + shadcn/ui
- [ ] Setup Docker Compose
- [ ] Configure NSwag type generation
- [ ] Create environment variable templates
- [ ] Test full development workflow

## Success Criteria
- [ ] `docker-compose -f docker-compose.dev.yml up` starts all services
- [ ] Frontend accessible at http://localhost:3000
- [ ] Backend accessible at http://localhost:5000
- [ ] PostgreSQL accessible at localhost:5432
- [ ] Hot reload works for both frontend and backend
- [ ] Type generation produces valid TypeScript client

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Docker network issues | Medium | High | Use explicit network config |
| NSwag version conflicts | Low | Medium | Pin specific versions |
| Hot reload failures | Medium | Low | Fallback to manual restart |

## Security Considerations
- Never commit `.env` files (use `.env.example`)
- Use Docker secrets for production credentials
- Ensure PostgreSQL password is strong

## Next Steps
After completion, proceed to [Phase 02: Database Schema](phase-02-database-schema.md)
