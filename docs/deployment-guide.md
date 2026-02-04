# Deployment Guide

**Last Updated:** 2026-02-04
**Version:** 1.0.0

## Overview

This guide covers local development setup, Docker deployment, environment configuration, and production deployment strategies for Event Ticket Manager.

## Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| Docker | 24.0+ | Containerization |
| Docker Compose | 2.20+ | Multi-container orchestration |
| Node.js | 20 LTS | Frontend development (optional) |
| .NET SDK | 8.0 | Backend development (optional) |
| Git | Latest | Version control |

**Note**: Node.js and .NET SDK only required for development outside Docker.

### System Requirements

**Development**:
- CPU: 2 cores minimum, 4 cores recommended
- RAM: 4GB minimum, 8GB recommended
- Storage: 10GB free space

**Production** (planned):
- CPU: 4 cores minimum
- RAM: 8GB minimum, 16GB recommended
- Storage: 50GB+ (depends on database size)

## Local Development Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd event-ticket-manager
```

### 2. Environment Configuration

Copy environment template:
```bash
cp .env.example .env
```

Edit `.env` with your configuration:
```env
# Database
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=event_tickets
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password_here

# Backend
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
AllowedOrigins=http://localhost:3000,http://localhost:3001
JWT_SECRET=your_jwt_secret_here_min_32_chars_use_secure_generator
JWT_ISSUER=EventTickets
JWT_AUDIENCE=EventTickets

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your_nextauth_secret_here_use_secure_generator

# SePay (Payment)
SEPAY_API_TOKEN=your_sepay_api_token
SEPAY_ACCOUNT_NUMBER=your_bank_account_number
SEPAY_WEBHOOK_SECRET=your_webhook_secret
```

**Security Notes**:
- Generate strong secrets using `openssl rand -base64 32`
- Never commit `.env` to version control
- Use different secrets for development and production

### 3. Start Development Environment

Using Docker Compose (recommended):
```bash
docker-compose -f docker-compose.dev.yml up
```

**Services Started**:
- PostgreSQL: `localhost:5432`
- Backend API: `http://localhost:5000`
- Frontend: `http://localhost:3000`

**Hot Reload Enabled**:
- Backend: `dotnet watch` auto-recompiles on file changes
- Frontend: Next.js dev server watches `src/` directory

### 4. Verify Installation

**Check Health**:
```bash
curl http://localhost:5000/health
# Expected: {"status":"healthy","timestamp":"2026-02-04T..."}
```

**Check API Info**:
```bash
curl http://localhost:5000/api/v1/info
# Expected: {"name":"Event Ticket Manager API","version":"1.0.0","environment":"Development"}
```

**Access Swagger UI**:
Open `http://localhost:5000/swagger` in browser (development only)

**Access Frontend**:
Open `http://localhost:3000` in browser

### 5. Stop Services

```bash
docker-compose -f docker-compose.dev.yml down
```

Retain database data:
```bash
docker-compose -f docker-compose.dev.yml down
```

Remove database volumes (reset):
```bash
docker-compose -f docker-compose.dev.yml down -v
```

## Development Without Docker

### Backend

**Prerequisites**: .NET 8 SDK installed

**Steps**:
```bash
cd src/backend

# Restore dependencies
dotnet restore

# Run API project
cd EventTickets.API
dotnet run
```

**Access**: `http://localhost:5000` (or port specified in `launchSettings.json`)

### Frontend

**Prerequisites**: Node.js 20+ installed

**Steps**:
```bash
cd src/frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

**Access**: `http://localhost:3000`

### Database

**Run PostgreSQL via Docker**:
```bash
docker run -d \
  --name etm-postgres \
  -e POSTGRES_DB=event_tickets \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine
```

**Connect**:
- Host: localhost
- Port: 5432
- Database: event_tickets
- User: postgres
- Password: postgres (dev only)

## Docker Deployment

### Production Build

**Build Images**:
```bash
docker-compose build
```

**Start Services**:
```bash
docker-compose up -d
```

**View Logs**:
```bash
docker-compose logs -f
```

**Stop Services**:
```bash
docker-compose down
```

### Docker Compose Configuration

#### docker-compose.yml (Production)

**Features**:
- Environment variable substitution
- Health checks for postgres
- Service dependencies
- Named volumes for data persistence

**Customization**:
Edit environment variables in `.env` or override in compose file.

#### docker-compose.dev.yml (Development)

**Additional Features**:
- Volume mounts for source code (hot reload)
- Hardcoded dev credentials (DO NOT use in production)
- `dotnet watch` and `npm run dev` commands
- Separate volume for dev database

## Database Management

### Migrations (Future - EF Core)

**Create Migration**:
```bash
cd src/backend/EventTickets.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../EventTickets.API
```

**Apply Migration**:
```bash
dotnet ef database update --startup-project ../EventTickets.API
```

**View Migrations**:
```bash
dotnet ef migrations list --startup-project ../EventTickets.API
```

**Rollback Migration**:
```bash
dotnet ef database update PreviousMigrationName --startup-project ../EventTickets.API
```

### Backup and Restore

**Backup**:
```bash
docker exec etm-postgres pg_dump -U postgres event_tickets > backup.sql
```

**Restore**:
```bash
docker exec -i etm-postgres psql -U postgres event_tickets < backup.sql
```

**Automated Backups** (cron example):
```bash
# Add to crontab (daily backup at 2 AM)
0 2 * * * docker exec etm-postgres pg_dump -U postgres event_tickets | gzip > /backups/event_tickets_$(date +\%Y\%m\%d).sql.gz
```

## Environment Variables

### Backend Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Yes | Development | Environment (Development, Staging, Production) |
| `ASPNETCORE_URLS` | Yes | http://+:5000 | Listening URLs |
| `AllowedOrigins` | Yes | http://localhost:3000 | CORS origins (comma-separated) |
| `ConnectionStrings__DefaultConnection` | Yes | - | PostgreSQL connection string |
| `JWT_SECRET` | Yes | - | JWT signing key (min 32 chars) |
| `JWT_ISSUER` | Yes | EventTickets | JWT issuer |
| `JWT_AUDIENCE` | Yes | EventTickets | JWT audience |
| `SEPAY_API_TOKEN` | Yes | - | SePay API authentication token |
| `SEPAY_ACCOUNT_NUMBER` | Yes | - | Bank account number for payments |
| `SEPAY_WEBHOOK_SECRET` | Yes | - | Webhook signature validation key |

### Frontend Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `NEXT_PUBLIC_API_URL` | Yes | http://localhost:5000 | Backend API base URL |
| `NEXTAUTH_URL` | Yes | http://localhost:3000 | Frontend base URL (for NextAuth) |
| `NEXTAUTH_SECRET` | Yes | - | NextAuth session encryption key |

### Database Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `POSTGRES_HOST` | Yes | localhost | Database host |
| `POSTGRES_PORT` | Yes | 5432 | Database port |
| `POSTGRES_DB` | Yes | event_tickets | Database name |
| `POSTGRES_USER` | Yes | postgres | Database user |
| `POSTGRES_PASSWORD` | Yes | - | Database password |

## Type Generation (NSwag)

**Purpose**: Generate TypeScript types from backend OpenAPI spec

**Configuration**: `tools/nswag.json` (to be created in Phase 03)

**Run Generation**:
```bash
cd src/frontend
npm run generate-types
```

**Output**:
- `src/types/api.ts`: TypeScript type definitions
- `src/lib/api-client.ts`: Typed HTTP client (future)

**Integration**: Run before frontend build to ensure types are up-to-date

## Production Deployment (Planned)

### Cloud Platform Options

**Option 1: Azure**
- Azure Container Apps (backend + frontend)
- Azure Database for PostgreSQL
- Azure Key Vault (secrets)
- Azure CDN (static assets)

**Option 2: AWS**
- ECS Fargate (backend + frontend containers)
- RDS PostgreSQL
- Secrets Manager
- CloudFront (CDN)

**Option 3: DigitalOcean**
- App Platform (Docker deployment)
- Managed PostgreSQL
- Spaces (object storage)

### Deployment Checklist

- [ ] Generate strong production secrets
- [ ] Configure environment variables
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Remove Swagger UI (production)
- [ ] Enable HTTPS redirection
- [ ] Configure SSL certificate
- [ ] Set CORS origins to production domains
- [ ] Configure database connection pooling
- [ ] Set up automated backups
- [ ] Configure monitoring and logging
- [ ] Set up health check endpoints for load balancer
- [ ] Test payment webhook integration
- [ ] Configure email service (SendGrid/Mailgun)
- [ ] Set up CDN for static assets (future)

### Docker Production Best Practices

**Multi-Stage Builds**:
- Build layer (dependencies + compilation)
- Runtime layer (minimal image with only runtime dependencies)

**Security**:
- Run containers as non-root user
- Use secrets management (Docker secrets, cloud KMS)
- Scan images for vulnerabilities (`docker scan`)
- Keep base images updated

**Performance**:
- Use `.dockerignore` to reduce build context
- Enable BuildKit for faster builds
- Use layer caching effectively
- Minimize image size (Alpine base images)

### CI/CD Pipeline (Planned - GitHub Actions)

**Workflow Example**:
```yaml
# .github/workflows/deploy.yml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run backend tests
        run: dotnet test src/backend
      - name: Run frontend tests
        run: cd src/frontend && npm test

  build:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker images
        run: docker-compose build
      - name: Push to registry
        run: |
          echo ${{ secrets.REGISTRY_PASSWORD }} | docker login -u ${{ secrets.REGISTRY_USERNAME }} --password-stdin
          docker-compose push

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to production
        run: |
          # Deploy to cloud platform (Azure, AWS, DO)
```

## Monitoring and Logging (Planned)

### Health Checks

**Endpoint**: `GET /health`

**Response**:
```json
{
  "status": "healthy",
  "timestamp": "2026-02-04T10:30:00Z"
}
```

**Use Cases**:
- Load balancer health probes
- Uptime monitoring (UptimeRobot, Pingdom)
- Kubernetes readiness/liveness probes

### Logging

**Backend** (Serilog - planned):
- Structured JSON logs
- Log levels: Debug, Information, Warning, Error, Critical
- Output: Console (Docker logs), File, Cloud service

**Frontend**:
- Client errors logged to backend API
- Performance metrics (Web Vitals)

**Centralized Logging** (future):
- Azure Application Insights
- AWS CloudWatch
- Datadog
- Grafana Loki

### Metrics (Future)

**Backend**:
- Request count and latency
- Error rate
- Database query performance
- Payment success rate

**Frontend**:
- Page load time (LCP, FID, CLS)
- API call latency
- JavaScript errors

**Tools**:
- Prometheus + Grafana
- Application Insights
- New Relic

## Troubleshooting

### Common Issues

**Issue: Docker containers fail to start**

**Check**:
```bash
docker-compose logs
```

**Solutions**:
- Verify `.env` file exists and is correctly formatted
- Check port conflicts (5000, 3000, 5432)
- Ensure Docker has sufficient resources (CPU, RAM)

**Issue: Backend cannot connect to database**

**Check**:
```bash
docker-compose ps
# Ensure postgres container is "healthy"
```

**Solutions**:
- Verify `ConnectionStrings__DefaultConnection` in `.env`
- Check postgres health: `docker exec etm-postgres pg_isready -U postgres`
- Review postgres logs: `docker-compose logs postgres`

**Issue: Frontend cannot reach backend API**

**Check**: Browser console for CORS errors

**Solutions**:
- Verify `AllowedOrigins` includes frontend URL
- Check `NEXT_PUBLIC_API_URL` matches backend URL
- Ensure backend is running and accessible

**Issue: Hot reload not working**

**Solutions**:
- **Backend**: Verify volume mounts in `docker-compose.dev.yml`
- **Frontend**: Check `WATCHPACK_POLLING=true` env var (for Docker on Windows/Mac)
- Restart containers: `docker-compose -f docker-compose.dev.yml restart`

**Issue: Database connection pool exhausted**

**Solutions**:
- Increase connection pool size in connection string
- Check for connection leaks (ensure `using` statements in C#)
- Scale backend horizontally

## Performance Optimization

### Backend

**Database**:
- Add indexes on frequently queried columns (OrganizerId, StartDate)
- Use `.Include()` for eager loading to avoid N+1 queries
- Implement pagination for large result sets

**Caching** (future):
- Redis for session data and frequently accessed data
- Response caching middleware for read-heavy endpoints

**Connection Pooling**:
EF Core uses pooling by default. Customize in connection string:
```
Host=postgres;Database=event_tickets;Username=postgres;Password=***;Minimum Pool Size=5;Maximum Pool Size=20;
```

### Frontend

**Code Splitting**:
- Next.js automatically splits routes
- Use dynamic imports for heavy components:
  ```typescript
  const HeavyComponent = dynamic(() => import('./heavy-component'));
  ```

**Image Optimization**:
- Use Next.js `<Image>` component for automatic optimization
- Serve WebP format with fallback

**Bundle Size**:
- Analyze with `npm run build` (Next.js shows bundle sizes)
- Remove unused dependencies
- Use tree-shaking friendly imports

## Security Considerations

### Production Hardening

**Backend**:
- [ ] Disable Swagger UI in production
- [ ] Enable HTTPS redirection
- [ ] Configure rate limiting on auth endpoints
- [ ] Implement request size limits
- [ ] Use strong JWT secrets (min 32 chars)
- [ ] Enable security headers (already implemented in Phase 01)

**Database**:
- [ ] Use strong passwords
- [ ] Disable remote access (internal network only)
- [ ] Enable SSL for connections (future)
- [ ] Regular backups with encryption

**Frontend**:
- [ ] Implement CSP headers
- [ ] Sanitize user inputs
- [ ] Use httpOnly cookies for sensitive tokens
- [ ] Enable SRI for CDN scripts (future)

**Secrets Management**:
- Use cloud provider's secret manager (Azure Key Vault, AWS Secrets Manager)
- Never log secrets
- Rotate secrets regularly

## Rollback Procedure

**Application Rollback**:
```bash
# Revert to previous Docker images
docker-compose down
git checkout <previous-commit>
docker-compose build
docker-compose up -d
```

**Database Rollback**:
```bash
# Restore from backup
docker exec -i etm-postgres psql -U postgres event_tickets < backup_20260203.sql

# Or rollback EF Core migration
dotnet ef database update PreviousMigrationName --startup-project src/backend/EventTickets.API
```

## References

- [Docker Documentation](https://docs.docker.com/)
- [Next.js Deployment](https://nextjs.org/docs/deployment)
- [ASP.NET Core Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [System Architecture](./system-architecture.md)
- [Code Standards](./code-standards.md)
