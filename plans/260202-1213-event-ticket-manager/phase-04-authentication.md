# Phase 04: Authentication

## Context Links
- [Main Plan](plan.md)
- [Architecture Report](../reports/researcher-260202-1222-nextjs-dotnet-architecture.md)

## Overview
- **Priority:** P1 (Critical - all protected routes depend on auth)
- **Status:** completed
- **Effort:** 8h
- **Description:** Implement hybrid auth with NextAuth (frontend) + ASP.NET Identity (backend)

## Key Insights
- NextAuth handles OAuth providers and session management
- ASP.NET Identity stores users and manages roles
- JWT tokens passed from frontend to backend
- Backend validates JWT and extracts claims
- Roles: Admin, Organizer, Attendee

## Requirements

### Functional
- F1: Email/password registration and login
- F2: Google OAuth login
- F3: Role-based access control
- F4: JWT token refresh
- F5: Password reset flow

### Non-Functional
- NF1: Tokens expire in 1 hour, refresh in 7 days
- NF2: Passwords hashed with bcrypt
- NF3: Rate limit login attempts (5/min)

## Architecture

### Authentication Flow

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Browser    │         │   Next.js    │         │  ASP.NET     │
│              │         │   (NextAuth) │         │  Core API    │
└──────┬───────┘         └──────┬───────┘         └──────┬───────┘
       │                        │                        │
       │  1. Login Request      │                        │
       │───────────────────────>│                        │
       │                        │                        │
       │                        │  2. Validate Creds     │
       │                        │───────────────────────>│
       │                        │                        │
       │                        │  3. User + Roles       │
       │                        │<───────────────────────│
       │                        │                        │
       │  4. JWT + Session      │                        │
       │<───────────────────────│                        │
       │                        │                        │
       │  5. API Request + JWT  │                        │
       │───────────────────────>│                        │
       │                        │                        │
       │                        │  6. Forward + JWT      │
       │                        │───────────────────────>│
       │                        │                        │
       │                        │  7. Validate JWT       │
       │                        │  8. Response           │
       │                        │<───────────────────────│
       │                        │                        │
       │  9. Data               │                        │
       │<───────────────────────│                        │
```

### JWT Token Structure

```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Organizer",
  "iat": 1706832000,
  "exp": 1706835600
}
```

## Related Code Files

### Create (Frontend)
- `src/frontend/lib/auth.ts` - NextAuth configuration
- `src/frontend/lib/auth-options.ts` - Provider configs
- `src/frontend/app/api/auth/[...nextauth]/route.ts` - NextAuth route
- `src/frontend/app/(auth)/login/page.tsx`
- `src/frontend/app/(auth)/register/page.tsx`
- `src/frontend/app/(auth)/forgot-password/page.tsx`
- `src/frontend/components/auth/login-form.tsx`
- `src/frontend/components/auth/register-form.tsx`
- `src/frontend/middleware.ts` - Route protection

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/AuthEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Auth/LoginRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Auth/RegisterRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Auth/AuthResponse.cs`
- `src/backend/EventTickets.Core/Services/IAuthService.cs`
- `src/backend/EventTickets.Infrastructure/Services/AuthService.cs`
- `src/backend/EventTickets.Infrastructure/Services/JwtService.cs`

## Implementation Steps

### 1. Configure ASP.NET Identity (2h)
- [x] Add Identity packages to Infrastructure project
- [x] Configure `ApplicationUser` with Identity
- [x] Setup Identity in `Program.cs`
- [x] Configure password requirements
- [x] Add role seeding (Admin, Organizer, Attendee)

### 2. Implement JWT Service (1.5h)
- [x] Create `IJwtService` interface
- [x] Implement token generation with claims
- [x] Implement token validation
- [x] Configure JWT settings in `appsettings.json`
- [x] Add refresh token logic

### 3. Create Auth Endpoints (1.5h)
- [x] `POST /api/auth/register` - Create account
- [x] `POST /api/auth/login` - Authenticate
- [x] `POST /api/auth/refresh` - Refresh token
- [x] `GET /api/auth/me` - Current user info
- [x] `POST /api/auth/forgot-password` - Request reset
- [x] `POST /api/auth/reset-password` - Complete reset
- [x] Add request validation

### 4. Configure NextAuth (1.5h)
- [x] Install NextAuth: `npm install next-auth`
- [x] Create auth options with Credentials provider
- [x] Add Google OAuth provider (UI only)
- [x] Configure JWT callbacks
- [x] Setup session provider in layout
- [x] Add auth middleware for protected routes

### 5. Create Auth UI Components (1.5h)
- [x] Create login form with Tailwind CSS
- [x] Create registration form
- [x] Create forgot password page (placeholder)
- [x] Add form validation
- [x] Handle loading and error states
- [x] Add OAuth buttons (UI only)

## Todo List
- [x] Configure ASP.NET Identity
- [x] Implement JWT generation/validation
- [x] Create auth API endpoints
- [x] Add role seeding
- [x] Configure NextAuth providers
- [x] Create login/register pages
- [x] Add route protection middleware
- [x] Test complete auth flow

## Success Criteria
- [x] User can register with email/password
- [x] User can login and receive JWT
- [x] Protected routes redirect to login
- [x] JWT validated on backend API calls
- [x] Roles correctly assigned and checked
- [x] Token refresh works before expiry

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Token sync issues | Medium | High | Clear error handling |
| OAuth config errors | Medium | Medium | Thorough testing |
| Session hijacking | Low | High | HttpOnly cookies, HTTPS |

## Security Considerations
- Use HttpOnly cookies for tokens
- Implement CSRF protection
- Rate limit auth endpoints
- Log failed login attempts
- Hash passwords with bcrypt (Identity default)
- Validate JWT signature and expiry
- Revoke tokens on password change

## Next Steps
After completion, proceed to [Phase 05: Event Management](phase-05-event-management.md)
