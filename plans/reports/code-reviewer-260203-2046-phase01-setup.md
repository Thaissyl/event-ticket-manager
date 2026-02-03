# Code Review: Phase 01 - Project Setup

**Reviewer:** code-reviewer
**Date:** 2026-02-03
**Scope:** Event Ticket Manager Phase 01 Project Setup
**Score:** 6.5/10

---

## Scope

**Files Reviewed:**
- `src/frontend/src/app/page.tsx` (31 lines)
- `src/backend/EventTickets.API/Program.cs` (56 lines)
- `docker-compose.yml` (48 lines)
- `docker-compose.dev.yml` (63 lines)
- `.env.example` (24 lines)
- `tools/nswag.json` (50 lines)
- `src/backend/Dockerfile` (22 lines)
- `src/backend/Dockerfile.dev` (16 lines)
- `src/frontend/Dockerfile` (31 lines)
- `src/frontend/Dockerfile.dev` (11 lines)
- `src/frontend/next.config.ts` (8 lines)

**Lines Analyzed:** ~360 lines
**Review Focus:** Initial project setup, security, Docker config, architecture adherence
**Updated Plans:** `plans/260202-1213-event-ticket-manager/phase-01-project-setup.md` (status update required)

---

## Overall Assessment

Phase 01 setup establishes basic infrastructure but contains several **CRITICAL security issues** and missing production configurations. Code compiles successfully (backend: 0 errors, frontend: builds cleanly), but Docker environment not verified running. Structure follows Clean Architecture principles. Several YAGNI/KISS violations and incomplete configurations.

**Build Status:**
- Backend: ✅ Builds (0 warnings, 0 errors)
- Frontend: ✅ Builds (TypeScript check passed)
- Docker: ⚠️ Not verified running (API version error)

---

## Critical Issues

### 1. **HARDCODED CREDENTIALS IN PRODUCTION CONFIG** 🔴
**File:** `docker-compose.yml` (lines 6-8)
```yaml
POSTGRES_DB: ${POSTGRES_DB:-event_tickets}
POSTGRES_USER: ${POSTGRES_USER:-postgres}
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
```
**Impact:** Production deployment will use default password "postgres" if env vars not set
**Fix:** Remove fallback defaults in production compose file
```yaml
# docker-compose.yml - NO DEFAULTS
POSTGRES_DB: ${POSTGRES_DB}
POSTGRES_USER: ${POSTGRES_USER}
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
```

### 2. **CORS ALLOWS CREDENTIALS WITHOUT ORIGIN VALIDATION** 🔴
**File:** `src/backend/EventTickets.API/Program.cs` (lines 18-24)
```csharp
policy.WithOrigins("http://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```
**Impact:** Hardcoded localhost origin blocks production usage; no environment-based configuration
**Fix:** Use environment variable for allowed origins
```csharp
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
policy.WithOrigins(allowedOrigins)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

### 3. **MISSING HTTPS ENFORCEMENT** 🔴
**File:** `src/backend/EventTickets.API/Program.cs`
**Impact:** Backend runs HTTP-only, no HTTPS redirect middleware configured
**Fix:** Add HTTPS redirection for production
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

### 4. **EXPOSED DATABASE PORT IN PRODUCTION** 🔴
**File:** `docker-compose.yml` (lines 9-10)
```yaml
ports:
  - "5432:5432"
```
**Impact:** PostgreSQL accessible from host in production (attack surface)
**Fix:** Remove port mapping in production compose; only expose in dev
```yaml
# Production: Remove ports section
# Dev only: docker-compose.dev.yml keeps ports
```

---

## High Priority Findings

### 5. **NO SWAGGER AUTHENTICATION IN PRODUCTION**
**File:** `Program.cs` (lines 30-34)
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```
**Issue:** Swagger disabled in production (good), but no basic auth if enabled
**Recommendation:** Add authentication guard if ever exposed
```csharp
app.UseSwagger();
app.UseSwaggerUI(c => c.DocumentTitle = "ETM API");
app.MapSwagger().RequireAuthorization("AdminOnly");
```

### 6. **MISSING HEALTHCHECK IN PRODUCTION DOCKERFILE**
**File:** `src/backend/Dockerfile`
**Issue:** No HEALTHCHECK instruction for container orchestration
**Fix:** Add health endpoint check
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1
```

### 7. **FRONTEND DOCKERFILE MISSING NEXT.JS STANDALONE CONFIG**
**File:** `src/frontend/Dockerfile` (line 22)
```dockerfile
COPY --from=builder /app/.next/standalone ./
```
**Issue:** Requires `output: 'standalone'` in `next.config.ts` (missing)
**Fix:** Update `next.config.ts`
```typescript
const nextConfig: NextConfig = {
  output: 'standalone', // Required for Docker standalone build
};
```

### 8. **NO RATE LIMITING OR REQUEST SIZE LIMITS**
**File:** `Program.cs`
**Issue:** No protection against DoS attacks
**Recommendation:** Add Kestrel limits
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    options.Limits.MaxConcurrentConnections = 100;
});
```

### 9. **ENVIRONMENT VARIABLES EXPOSED IN FRONTEND**
**File:** `docker-compose.yml` (line 40)
```yaml
NEXT_PUBLIC_API_URL=http://localhost:5000
```
**Issue:** Hardcoded localhost URL won't work in production
**Fix:** Use runtime environment detection or build-time args
```yaml
NEXT_PUBLIC_API_URL=${API_URL:-http://localhost:5000}
```

### 10. **NO LOGGING CONFIGURATION**
**Files:** All backend code
**Issue:** No structured logging, log levels, or log sinks configured
**Recommendation:** Add Serilog or built-in logging config in `appsettings.json`

---

## Medium Priority Improvements

### 11. **Docker Volume Permissions Issues (Windows)**
**File:** `docker-compose.dev.yml` (lines 30-37)
**Issue:** Volume mounts may have permission issues on Windows
**Recommendation:** Document volume mount caveats in README; consider named volumes

### 12. **Missing .dockerignore Files**
**Location:** `src/backend/`, `src/frontend/`
**Issue:** Build context includes `bin/`, `obj/`, `node_modules/` unnecessarily
**Fix:** Create `.dockerignore`
```
# Backend
bin/
obj/
*.user
.vs/

# Frontend
node_modules/
.next/
.env*.local
```

### 13. **NSwag Config Missing Output Validation**
**File:** `tools/nswag.json` (line 46)
```json
"output": "src/frontend/src/lib/api-client.ts"
```
**Issue:** No validation that output directory exists before generation
**Recommendation:** Add mkdir command to generation script

### 14. **No Security Headers Middleware**
**File:** `Program.cs`
**Issue:** Missing CSP, X-Frame-Options, HSTS headers
**Recommendation:** Add security headers middleware
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});
```

### 15. **Frontend API Client Has Credentials Flag**
**File:** `tools/nswag.json` (line 19)
```json
"withCredentials": true
```
**Issue:** Requires CORS credentials; increases complexity
**Recommendation:** Use JWT in Authorization header instead of cookies

### 16. **Postgres Connection String In Plaintext**
**File:** `docker-compose.dev.yml` (line 27)
**Issue:** Password visible in docker inspect
**Recommendation:** Use Docker secrets in production

### 17. **No Database Migration Strategy**
**Files:** Backend projects
**Issue:** No Entity Framework migrations configured yet
**Recommendation:** Will be addressed in Phase 02 (acceptable for setup phase)

---

## Low Priority Suggestions

### 18. **Frontend Page is Static Placeholder**
**File:** `src/frontend/src/app/page.tsx`
**Issue:** Buttons non-functional, hardcoded text
**Status:** Acceptable for Phase 01; features come later

### 19. **API Version Endpoint Returns Hardcoded Version**
**File:** `Program.cs` (line 47)
```csharp
version = "1.0.0"
```
**Suggestion:** Read from assembly version or appsettings
```csharp
version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
```

### 20. **Container Names May Conflict**
**Files:** Both docker-compose files
**Issue:** `etm-*` names may conflict if running multiple environments
**Recommendation:** Use compose project name prefix instead

### 21. **No Graceful Shutdown Handling**
**File:** `Program.cs`
**Recommendation:** Add shutdown hooks for cleanup tasks (low priority for now)

---

## Positive Observations

✅ **Clean Architecture Separation:** Core/Infrastructure/API layers properly structured
✅ **Multi-stage Docker Builds:** Frontend uses proper optimization (deps → builder → runner)
✅ **TypeScript Strict Mode:** Frontend configured correctly
✅ **Health Check Endpoint:** `/health` implemented for monitoring
✅ **Development Hot Reload:** Both frontend and backend support live reloading
✅ **Environment Variable Template:** `.env.example` provides good documentation
✅ **No TODO/FIXME Comments:** Code is clean, no placeholder comments
✅ **Successful Builds:** Both projects compile without errors
✅ **OpenAPI Documentation:** Swagger/NSwag configured for API contracts

---

## Recommended Actions

### Immediate (Before Phase 02)
1. **FIX:** Remove password defaults from `docker-compose.yml`
2. **FIX:** Add `output: 'standalone'` to `next.config.ts`
3. **FIX:** Create `.dockerignore` files for both frontend and backend
4. **FIX:** Make CORS origins environment-configurable
5. **FIX:** Remove database port exposure in production compose
6. **ADD:** HTTPS redirection middleware for production
7. **ADD:** Security headers middleware
8. **TEST:** Verify `docker-compose up` works (currently failing)

### Before Production Deployment
9. **ADD:** Rate limiting and request size limits
10. **ADD:** Structured logging configuration
11. **ADD:** HEALTHCHECK to production Dockerfiles
12. **CONFIGURE:** Docker secrets for sensitive data
13. **REVIEW:** Switch from cookie-based auth to JWT tokens
14. **DOCUMENT:** Volume mount permissions for Windows in README

### Next Phase Dependencies
15. **PLAN:** Entity Framework migrations (Phase 02)
16. **PLAN:** Authentication middleware (Phase 04)
17. **PLAN:** Admin authorization policies (Phase 10)

---

## Metrics

- **Type Coverage:** N/A (no domain logic yet)
- **Test Coverage:** 0% (no tests in Phase 01)
- **Linting Issues:** 0 (builds clean)
- **Security Vulnerabilities:** 4 Critical, 5 High
- **YAGNI Violations:** 2 (NSwag withCredentials, unused nswag options)
- **Missing Configs:** 8 (HTTPS, rate limiting, logging, .dockerignore, etc.)

---

## Task Completeness Status

### Phase 01 Plan Tasks (from `phase-01-project-setup.md`)

#### ✅ Completed
- [x] Create monorepo directory structure
- [x] Initialize Next.js 14 project
- [x] Initialize ASP.NET Core solution
- [x] Configure Tailwind + shadcn/ui
- [x] Setup Docker Compose
- [x] Configure NSwag type generation
- [x] Create environment variable templates

#### ⚠️ Partially Complete
- [ ] **Test full development workflow** - Docker compose not verified running
- [ ] **Frontend accessible at http://localhost:3000** - Not verified
- [ ] **Backend accessible at http://localhost:5000** - Not verified
- [ ] **PostgreSQL accessible at localhost:5432** - Not verified
- [ ] **Hot reload works** - Not tested
- [ ] **Type generation produces valid TypeScript client** - Not tested (no API endpoints yet)

#### ❌ Missing
- [ ] Security hardening (HTTPS, headers, rate limiting)
- [ ] Production configuration cleanup (remove defaults)
- [ ] `.dockerignore` files
- [ ] Docker health checks in production Dockerfiles
- [ ] Logging configuration

### Success Criteria Assessment
**Status:** 5/6 criteria need verification
**Blocker:** Docker environment verification failed (API version error)
**Next Step:** Fix Docker daemon connection, then verify all services start

---

## Plan File Update Required

**File:** `D:\event-ticket-manager\plans\260202-1213-event-ticket-manager\phase-01-project-setup.md`

**Proposed Status Change:**
```markdown
## Overview
- **Priority:** P1 (Critical - blocks all other phases)
- **Status:** in-progress → needs-verification ⚠️
- **Effort:** 4h (3.5h completed, 0.5h security fixes needed)
```

**Add Security Fixes Section:**
```markdown
## Security Fixes Required (Pre-Phase 02)
- [ ] Remove password defaults from production compose
- [ ] Make CORS origins environment-configurable
- [ ] Add output: 'standalone' to next.config.ts
- [ ] Create .dockerignore files
- [ ] Remove database port exposure in production
- [ ] Add HTTPS redirection for production
- [ ] Add security headers middleware
- [ ] Verify Docker services start successfully
```

---

## Unresolved Questions

1. **Why is Docker daemon connection failing?** (500 Internal Server Error on API route)
   - Is Docker Desktop running?
   - Is Docker Compose version compatible?
   - Are named pipes accessible?

2. **What is the production deployment target?** (Cloud platform, VPS, on-premise?)
   - Affects HTTPS termination strategy
   - Affects secret management approach
   - Affects container orchestration choice

3. **Is cookie-based auth required?** (NSwag withCredentials: true suggests it)
   - If yes, need SameSite/Secure cookie configuration
   - If no, can simplify to JWT-only approach

4. **What is the logging/monitoring strategy?**
   - Application Insights, ELK, CloudWatch?
   - Affects appsettings.json configuration

5. **Are there Windows-specific deployment considerations?**
   - Docker volume permissions on Windows
   - Path separators in configs

---

**Review completed. Proceed with security fixes before Phase 02.**
