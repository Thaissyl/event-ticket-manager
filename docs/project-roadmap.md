# Project Roadmap

**Last Updated:** 2026-02-04
**Version:** 1.0.0
**Project Status:** Active Development - Phase 01

## Overview

Event Ticket Manager development is organized into 11 phases spanning approximately 80 hours. This roadmap tracks progress, milestones, and upcoming features.

## Development Phases

### Phase 01: Project Setup ✅ (Needs Verification)

**Status**: Complete with pending verification
**Effort**: 4h (actual: 3.5h)
**Priority**: P0 (Critical)
**Completion**: 87.5%

**Objectives**:
- Initialize monorepo structure
- Setup Next.js 15 frontend with shadcn/ui
- Setup ASP.NET Core 8 backend with Clean Architecture
- Configure Docker Compose for development and production
- Implement security best practices

**Completed**:
- [x] Monorepo structure (src/frontend, src/backend)
- [x] Next.js 15 with App Router
- [x] shadcn/ui components (Button, Card, Input, Label)
- [x] ASP.NET Core 8 with Minimal APIs
- [x] Clean Architecture layers (API, Core, Infrastructure)
- [x] Docker Compose with health checks
- [x] Environment variable configuration (.env.example)
- [x] CORS with environment-configurable origins
- [x] Security headers (CSP, X-Frame-Options, X-XSS-Protection, etc.)
- [x] HTTPS redirection for production
- [x] .dockerignore files
- [x] Standalone Next.js output

**Pending**:
- [ ] Verify Docker services start successfully
- [ ] Test hot reload functionality

**Blockers**: None

**Next Steps**: Docker verification, then proceed to Phase 02

---

### Phase 02: Database Schema

**Status**: Pending
**Effort**: 6h
**Priority**: P1 (High)
**Completion**: 0%
**Blocked By**: Phase 01 verification

**Objectives**:
- Design PostgreSQL database schema
- Implement EF Core DbContext and configurations
- Create initial migrations
- Seed development data

**Key Deliverables**:
- [ ] EF Core packages installed (Npgsql.EntityFrameworkCore.PostgreSQL)
- [ ] Entity classes (User, Event, TicketTier, Order, Ticket, Payment)
- [ ] DbContext with fluent API configurations
- [ ] Optimistic locking via RowVersion
- [ ] Initial migration (CreateDatabase)
- [ ] Seed data for testing
- [ ] Database indexes on common queries

**Success Criteria**:
- Migration applies successfully
- Database schema matches ERD in phase plan
- Seed data populates correctly

**Dependencies**: None (blocks all feature phases)

---

### Phase 03: Backend API Structure

**Status**: Pending
**Effort**: 8h
**Priority**: P1 (High)
**Completion**: 0%
**Blocked By**: Phase 02

**Objectives**:
- Implement repository pattern
- Create service layer for business logic
- Define API endpoints (Minimal APIs)
- Configure NSwag for TypeScript generation
- Implement error handling middleware

**Key Deliverables**:
- [ ] Repository interfaces and implementations
- [ ] Service interfaces and implementations
- [ ] CRUD endpoints for Events, TicketTiers, Orders
- [ ] DTOs for request/response models
- [ ] NSwag configuration (tools/nswag.json)
- [ ] TypeScript type generation working
- [ ] Global exception handling middleware
- [ ] API versioning (/api/v1/...)

**Success Criteria**:
- Swagger UI shows all endpoints
- TypeScript types generate without errors
- API returns proper status codes and error messages

**Dependencies**: Phase 02 (database schema required)

---

### Phase 04: Authentication

**Status**: Pending
**Effort**: 8h
**Priority**: P0 (Critical)
**Completion**: 0%
**Blocked By**: Phase 03

**Objectives**:
- Implement ASP.NET Identity
- Create JWT token generation
- Add authentication middleware
- Implement role-based authorization
- Create login/register endpoints

**Key Deliverables**:
- [ ] ASP.NET Identity configuration
- [ ] ApplicationUser entity (extends IdentityUser)
- [ ] JWT token generation service
- [ ] Login endpoint (POST /api/v1/auth/login)
- [ ] Register endpoint (POST /api/v1/auth/register)
- [ ] Refresh token endpoint (POST /api/v1/auth/refresh)
- [ ] Role-based authorization ([Authorize(Roles = "Organizer")])
- [ ] Password reset flow (future)

**Success Criteria**:
- User can register and receive JWT token
- User can login and access protected endpoints
- Organizer role can create events
- Admin role can manage users

**Dependencies**: Phase 03 (API structure required)

---

### Phase 05: Event Management

**Status**: Pending
**Effort**: 10h
**Priority**: P1 (High)
**Completion**: 0%
**Blocked By**: Phase 04

**Objectives**:
- Implement event CRUD operations
- Add ticket tier management
- Create organizer dashboard UI
- Implement event publishing workflow

**Key Deliverables**:
- [ ] Create Event endpoint (POST /api/v1/events)
- [ ] Update Event endpoint (PUT /api/v1/events/{id})
- [ ] Delete Event endpoint (DELETE /api/v1/events/{id})
- [ ] List Events endpoint (GET /api/v1/events)
- [ ] Event details endpoint (GET /api/v1/events/{id})
- [ ] Ticket tier CRUD endpoints
- [ ] Event form UI (Next.js)
- [ ] Organizer dashboard page
- [ ] Event list page (public)
- [ ] Event details page (public)

**Success Criteria**:
- Organizer can create, edit, delete events
- Events display with ticket tiers
- Public can browse published events
- Draft events not visible to public

**Dependencies**: Phase 04 (authentication required for organizer actions)

---

### Phase 06: Ticket Purchasing

**Status**: Pending
**Effort**: 10h
**Priority**: P1 (High)
**Completion**: 0%
**Blocked By**: Phase 05

**Objectives**:
- Implement shopping cart with reservations
- Add inventory management with optimistic locking
- Create checkout flow
- Implement promo code validation

**Key Deliverables**:
- [ ] Add to cart endpoint (POST /api/v1/cart/items)
- [ ] Remove from cart endpoint (DELETE /api/v1/cart/items/{id})
- [ ] View cart endpoint (GET /api/v1/cart)
- [ ] Cart reservation logic (10-minute expiry)
- [ ] Optimistic locking on TicketTiers (RowVersion)
- [ ] Create order endpoint (POST /api/v1/orders)
- [ ] Promo code validation
- [ ] Cart UI component
- [ ] Checkout page
- [ ] Order confirmation page

**Success Criteria**:
- User can add tickets to cart
- Cart reservations prevent overselling
- Expired reservations release inventory
- Promo codes apply discounts correctly
- Optimistic lock prevents race conditions

**Dependencies**: Phase 05 (events and tiers required)

---

### Phase 07: SePay Payment

**Status**: Pending
**Effort**: 8h
**Priority**: P0 (Critical)
**Completion**: 0%
**Blocked By**: Phase 06

**Objectives**:
- Integrate SePay VietQR API
- Implement webhook handling
- Create payment confirmation flow
- Add transaction logging

**Key Deliverables**:
- [ ] SePay service implementation
- [ ] Generate payment QR endpoint (POST /api/v1/payments)
- [ ] Webhook endpoint (POST /api/webhooks/sepay)
- [ ] Webhook signature validation
- [ ] Payment status update logic
- [ ] Transaction logging table (SePayWebhooks)
- [ ] Payment confirmation page (QR display)
- [ ] Order status polling UI
- [ ] Email notification (ticket delivery) - future

**Success Criteria**:
- QR code generates for order
- Webhook updates order status to Confirmed
- Idempotent webhook handling (no duplicate confirmations)
- Payment failures logged for manual review

**Dependencies**: Phase 06 (order creation required)

---

### Phase 08: QR Tickets & Check-In

**Status**: Pending
**Effort**: 6h
**Priority**: P1 (High)
**Completion**: 0%
**Blocked By**: Phase 07

**Objectives**:
- Generate QR codes for tickets
- Implement ticket verification API
- Create check-in scanner UI
- Add check-in tracking

**Key Deliverables**:
- [ ] QR code generation service
- [ ] Ticket entity with QR code field
- [ ] Verify ticket endpoint (GET /api/v1/tickets/{qrCode}/verify)
- [ ] Check-in endpoint (POST /api/v1/tickets/{qrCode}/checkin)
- [ ] My tickets page (user dashboard)
- [ ] QR scanner page (organizer)
- [ ] Check-in status tracking
- [ ] Check-in analytics (attendee count)

**Success Criteria**:
- Each ticket has unique QR code
- QR scanner validates tickets instantly
- Duplicate scan detection (already checked in)
- Organizer can view check-in stats

**Dependencies**: Phase 07 (payment confirmation creates tickets)

---

### Phase 09: Analytics Dashboard

**Status**: Pending
**Effort**: 8h
**Priority**: P2 (Medium)
**Completion**: 0%
**Blocked By**: Phase 08

**Objectives**:
- Create sales analytics endpoints
- Implement revenue tracking
- Build organizer analytics dashboard
- Add chart visualizations

**Key Deliverables**:
- [ ] Sales summary endpoint (GET /api/v1/analytics/sales)
- [ ] Revenue by tier endpoint
- [ ] Sales trends endpoint (daily, weekly, monthly)
- [ ] Attendee demographics endpoint
- [ ] Analytics dashboard page (organizer)
- [ ] Chart components (Recharts or similar)
- [ ] Date range filtering
- [ ] Export to CSV functionality

**Success Criteria**:
- Organizer can view real-time sales count
- Revenue breakdown by ticket tier
- Sales trend charts display correctly
- Data exports match UI values

**Dependencies**: Phase 08 (ticket sales data required)

---

### Phase 10: Admin Panel

**Status**: Pending
**Effort**: 6h
**Priority**: P2 (Medium)
**Completion**: 0%
**Blocked By**: Phase 09

**Objectives**:
- Create admin user management
- Implement event moderation
- Add financial dashboard
- Create activity logs

**Key Deliverables**:
- [ ] Admin endpoints (user CRUD, role assignment)
- [ ] Event moderation endpoints (approve, reject, unpublish)
- [ ] Financial dashboard endpoint (platform revenue)
- [ ] Activity log endpoint
- [ ] Admin panel UI
- [ ] User management page
- [ ] Event moderation page
- [ ] Financial reports page

**Success Criteria**:
- Admin can assign/revoke Organizer role
- Admin can unpublish inappropriate events
- Financial dashboard shows accurate totals
- Activity logs track critical actions

**Dependencies**: Phase 09 (analytics for financial dashboard)

---

### Phase 11: Testing & Deployment

**Status**: Pending
**Effort**: 6h
**Priority**: P0 (Critical)
**Completion**: 0%
**Blocked By**: Phase 10

**Objectives**:
- Write unit and integration tests
- Implement E2E tests
- Create deployment documentation
- Conduct security audit

**Key Deliverables**:
- [ ] Backend unit tests (xUnit)
- [ ] Frontend unit tests (Jest + React Testing Library)
- [ ] Integration tests (checkout flow, payment flow)
- [ ] E2E tests (Playwright or Cypress)
- [ ] Test coverage report (target: 70%+)
- [ ] Production deployment checklist
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Security audit (dependency scanning, OWASP)
- [ ] Performance testing (load testing)

**Success Criteria**:
- Test coverage > 70%
- All critical flows covered by E2E tests
- Zero critical security vulnerabilities
- Deployment runbook complete

**Dependencies**: Phase 10 (all features complete)

---

## Milestones

### Milestone 1: Foundation Complete
**Target Date**: TBD
**Status**: 87.5% Complete

**Includes**:
- Phase 01: Project Setup ✅ (needs verification)
- Phase 02: Database Schema
- Phase 03: Backend API Structure

**Deliverables**:
- Functional REST API with type-safe frontend
- Database schema with migrations
- Development environment ready

---

### Milestone 2: Core Features Complete
**Target Date**: TBD
**Status**: 0% Complete

**Includes**:
- Phase 04: Authentication
- Phase 05: Event Management
- Phase 06: Ticket Purchasing
- Phase 07: SePay Payment

**Deliverables**:
- Users can register and login
- Organizers can create events
- Attendees can purchase tickets
- Payments processed via SePay

---

### Milestone 3: Full Platform Ready
**Target Date**: TBD
**Status**: 0% Complete

**Includes**:
- Phase 08: QR Tickets & Check-In
- Phase 09: Analytics Dashboard
- Phase 10: Admin Panel

**Deliverables**:
- Complete ticketing workflow (purchase to check-in)
- Analytics for organizers
- Admin moderation tools

---

### Milestone 4: Production Launch
**Target Date**: TBD
**Status**: 0% Complete

**Includes**:
- Phase 11: Testing & Deployment

**Deliverables**:
- Comprehensive test suite
- Production deployment
- CI/CD pipeline
- Security audit complete

---

## Feature Backlog (Future Versions)

### v1.1 (Planned - Q2 2026)

**Authentication Enhancements**:
- [ ] OAuth social login (Google, Facebook)
- [ ] Two-factor authentication (2FA)
- [ ] Email verification

**Notifications**:
- [ ] Email notifications (order confirmation, ticket delivery)
- [ ] Event reminder emails
- [ ] SMS notifications (via Twilio)

**Analytics Enhancements**:
- [ ] Conversion funnel analysis
- [ ] Cohort analysis
- [ ] Advanced demographics

**Payment**:
- [ ] Stripe integration (international payments)
- [ ] Refund automation (manual SePay refunds tracked in system)

---

### v1.2 (Planned - Q3 2026)

**Event Features**:
- [ ] Recurring events
- [ ] Seat selection and mapping
- [ ] Multi-day events with session scheduling
- [ ] Event categories and tags

**Ticketing**:
- [ ] Group tickets (family packs)
- [ ] VIP packages (bundled perks)
- [ ] Early bird pricing automation

**Mobile**:
- [ ] React Native mobile app
- [ ] Offline check-in mode (sync when online)
- [ ] Mobile wallet integration (Apple Wallet, Google Pay)

---

### v2.0 (Planned - Q4 2026)

**Platform Expansion**:
- [ ] Multi-currency support
- [ ] Multi-language support (i18n)
- [ ] Marketplace (ticket resale with fee)

**Marketing Tools**:
- [ ] Affiliate and referral programs
- [ ] Email marketing campaigns
- [ ] Social media integrations

**CRM**:
- [ ] Customer profiles and history
- [ ] Segmentation and targeted campaigns
- [ ] Loyalty programs

**Advanced Features**:
- [ ] Live streaming integration
- [ ] Virtual event support
- [ ] On-demand content delivery

---

## Timeline Visualization

```
Phase 01 ████████████████░░ 87.5%
Phase 02 ░░░░░░░░░░░░░░░░░░  0%
Phase 03 ░░░░░░░░░░░░░░░░░░  0%
Phase 04 ░░░░░░░░░░░░░░░░░░  0%
Phase 05 ░░░░░░░░░░░░░░░░░░  0%
Phase 06 ░░░░░░░░░░░░░░░░░░  0%
Phase 07 ░░░░░░░░░░░░░░░░░░  0%
Phase 08 ░░░░░░░░░░░░░░░░░░  0%
Phase 09 ░░░░░░░░░░░░░░░░░░  0%
Phase 10 ░░░░░░░░░░░░░░░░░░  0%
Phase 11 ░░░░░░░░░░░░░░░░░░  0%

Overall Progress: 8% (7h / 80h)
```

---

## Progress Tracking

### Completed (7/80 hours)
- ✅ Monorepo structure
- ✅ Next.js 15 frontend
- ✅ ASP.NET Core 8 backend
- ✅ Docker Compose setup
- ✅ Security headers
- ✅ CORS configuration
- ✅ Environment variables

### In Progress (0.5/80 hours)
- ⚠️ Docker verification (Phase 01)

### Next Up (6/80 hours)
- 📋 Database schema (Phase 02)

---

## Dependencies Graph

```
Phase 01 (Setup)
    ↓
Phase 02 (Database)
    ↓
Phase 03 (API Structure)
    ↓
Phase 04 (Auth) ──────┐
    ↓                 │
Phase 05 (Events)     │
    ↓                 │
Phase 06 (Cart)       │
    ↓                 │
Phase 07 (Payment)    │
    ↓                 │
Phase 08 (QR)         │
    ↓                 │
Phase 09 (Analytics)  │
    ↓                 │
Phase 10 (Admin) ←────┘
    ↓
Phase 11 (Testing)
```

---

## Risk Register

| Risk | Phase | Probability | Impact | Mitigation |
|------|-------|-------------|--------|------------|
| SePay API changes | 07 | Medium | High | Version pinning, webhook validation |
| Optimistic lock failures | 06 | Medium | High | Retry logic, user-friendly error messages |
| Database performance | 02-11 | Low | Medium | Indexes, query optimization, connection pooling |
| Security vulnerabilities | 04, 11 | Low | Critical | Regular dependency updates, security audit |
| Scope creep | All | Medium | Medium | Strict adherence to phase plans, defer to future versions |

---

## Success Metrics - v1.0

### Technical Metrics
- [ ] Test coverage > 70%
- [ ] Page load time < 2s (p95)
- [ ] API response time < 500ms (p95)
- [ ] Zero critical security vulnerabilities
- [ ] Uptime > 99.5%

### Business Metrics
- [ ] 5+ events created by 3+ organizers
- [ ] 100+ tickets sold
- [ ] Payment success rate > 90%
- [ ] Positive user feedback (NPS > 40)

---

## Change Log

### 2026-02-04
- Roadmap created
- Phase 01 status updated to 87.5% (needs verification)
- Security fixes completed (CORS, headers, HTTPS redirection)

---

## References

- [Main Implementation Plan](../plans/260202-1213-event-ticket-manager/plan.md)
- [Project Overview PDR](./project-overview-pdr.md)
- [System Architecture](./system-architecture.md)
- [Phase 01 Code Review](../plans/reports/code-reviewer-260203-2046-phase01-setup.md)
