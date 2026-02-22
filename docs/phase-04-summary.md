# Phase 04: Authentication - Summary

**Status:** ⏳ PENDING
**Effort:** 8h
**Priority:** P1 (Critical - all protected routes depend on auth)

---

## What Is Planned

### Hybrid Authentication
- [ ] Configure ASP.NET Identity in backend
- [ ] Implement JWT token generation/validation
- [ ] Configure NextAuth in frontend
- [ ] Email/password registration and login
- [ ] Google OAuth integration
- [ ] Role-based access control (Admin, Organizer, Attendee)
- [ ] Password reset flow
- [ ] Token refresh mechanism

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
       │                        │  6. Forward + JWT      │
       │                        │───────────────────────>│
       │                        │  7. Validate JWT       │
       │                        │  8. Response           │
       │                        │<───────────────────────│
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

### User Roles

| Role | Permissions |
|------|-------------|
| Admin | Full platform access, user management |
| Organizer | Create/manage own events, view analytics |
| Attendee | Browse events, purchase tickets, view orders |

---

## Implementation Steps

### 1. Configure ASP.NET Identity (2h)
- [ ] Add Identity packages to Infrastructure
- [ ] Configure ApplicationUser with Identity
- [ ] Setup Identity in Program.cs
- [ ] Configure password requirements
- [ ] Add role seeding (Admin, Organizer, Attendee)

### 2. Implement JWT Service (1.5h)
- [ ] Create IJwtService interface
- [ ] Token generation with claims
- [ ] Token validation
- [ ] JWT settings in appsettings.json
- [ ] Refresh token logic

### 3. Create Auth Endpoints (1.5h)
- [ ] POST /api/auth/register
- [ ] POST /api/auth/login
- [ ] POST /api/auth/refresh
- [ ] GET /api/auth/me
- [ ] POST /api/auth/forgot-password
- [ ] POST /api/auth/reset-password

### 4. Configure NextAuth (1.5h)
- [ ] Install next-auth
- [ ] Create auth options with Credentials provider
- [ ] Add Google OAuth provider
- [ ] Configure JWT callbacks
- [ ] Setup session provider
- [ ] Add auth middleware for protected routes

### 5. Create Auth UI Components (1.5h)
- [ ] Login form (shadcn/ui)
- [ ] Registration form
- [ ] Forgot password page
- [ ] Form validation (react-hook-form + zod)
- [ ] OAuth buttons

---

## API Endpoints

```
POST /api/auth/register
Request: { email, password, fullName }
Response: { user, token }

POST /api/auth/login
Request: { email, password }
Response: { user, token, refreshToken }

POST /api/auth/refresh
Request: { refreshToken }
Response: { token, refreshToken }

GET /api/auth/me
Headers: Authorization: Bearer {token}
Response: { user }

POST /api/auth/forgot-password
Request: { email }
Response: { message }

POST /api/auth/reset-password
Request: { token, newPassword }
Response: { message }
```

---

## Success Criteria

- [ ] User can register with email/password
- [ ] User can login and receive JWT
- [ ] Protected routes redirect to login
- [ ] JWT validated on backend API calls
- [ ] Roles correctly assigned and checked
- [ ] Token refresh works before expiry

---

## Security Considerations

- HttpOnly cookies for tokens
- CSRF protection
- Rate limit auth endpoints (5/min)
- Log failed login attempts
- Hash passwords with bcrypt
- Validate JWT signature and expiry
- Revoke tokens on password change

---

## Next Phase

After completion, proceed to **[Phase 05: Event Management](phase-05-summary.md)**
