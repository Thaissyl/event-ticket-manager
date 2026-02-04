# Project Overview - Product Development Requirements

**Last Updated:** 2026-02-04
**Version:** 1.0.0
**Status:** Active Development - Phase 01

## Executive Summary

Event Ticket Manager is a full-stack, multi-organizer event ticketing platform supporting ticket tier management, shopping cart with time-based reservations, VietQR payment integration via SePay, QR code-based check-in, and comprehensive analytics dashboards. Built with Next.js 15, ASP.NET Core 8, PostgreSQL 16, and containerized via Docker.

## Project Metadata

| Property | Value |
|----------|-------|
| Project Name | Event Ticket Manager |
| Code Name | ETM |
| Repository | D:\event-ticket-manager |
| Architecture | Monorepo (src/frontend, src/backend) |
| License | TBD |
| Team Size | 1-3 developers |
| Estimated Timeline | 11 phases / 80 hours |

## Stakeholders

### Primary Stakeholders
- **Event Organizers**: Create and manage events, configure ticket tiers, track sales analytics
- **Attendees**: Browse events, purchase tickets, receive QR codes, manage orders
- **System Administrators**: Platform management, user moderation, financial oversight

### Secondary Stakeholders
- **Payment Provider**: SePay (Vietnamese VietQR bank transfer service)
- **Development Team**: Maintainers and contributors
- **Infrastructure Team**: DevOps, deployment, monitoring

## Business Goals

### Primary Objectives
1. **Multi-organizer Platform**: Enable multiple event organizers to independently create and manage events
2. **Seamless Ticketing**: Provide frictionless ticket purchase experience with inventory protection
3. **Local Payment Integration**: Support Vietnamese market via SePay VietQR
4. **Operational Efficiency**: QR-based check-in for fast event entry
5. **Data-Driven Decisions**: Real-time analytics for organizers

### Success Metrics
- Event creation time < 5 minutes
- Ticket purchase completion rate > 85%
- Payment processing success rate > 95%
- Check-in processing time < 3 seconds per attendee
- Platform uptime > 99.5%

## Scope

### In Scope - Core Features

#### User Management
- Email/password authentication via ASP.NET Identity
- Role-based access control (Admin, Organizer, Attendee)
- User profile management
- OAuth integration (Google, Facebook) - future

#### Event Management
- Event creation with rich text descriptions
- Venue information and location
- Event dates with timezone support
- Event visibility (published/draft)
- Event cover images
- Multi-tier ticketing (VIP, General, Early Bird)

#### Ticket Sales
- Shopping cart with time-based reservations (10 minutes)
- Real-time inventory tracking with optimistic locking
- Promo code support (percentage/fixed discount)
- Per-tier quantity limits
- Order history and management

#### Payment Processing
- SePay VietQR bank transfer integration
- Webhook-based payment confirmation
- Transaction logging and reconciliation
- Manual refund workflow (SePay limitation)

#### Ticketing System
- QR code generation per ticket
- Email delivery of tickets
- Digital ticket wallet view
- QR scanner interface for organizers
- Check-in status tracking

#### Analytics Dashboard
- Sales trends (daily, weekly, monthly)
- Revenue by ticket tier
- Ticket sales funnel metrics
- Attendee demographics (basic)
- Real-time sales monitoring

### Out of Scope - v1.0

- Recurring events
- Seat selection/mapping
- Affiliate/referral programs
- Multi-currency support
- Mobile native apps
- Advanced CRM features
- Email marketing automation
- Third-party integrations (Stripe, PayPal)

## Functional Requirements

### FR-1: User Authentication
- **Priority**: P0 (Critical)
- **Description**: Users can register, login, and manage accounts
- **Acceptance Criteria**:
  - Email verification for new accounts
  - Password reset via email
  - Session management with JWT tokens
  - Role assignment (Admin assigns Organizer role)

### FR-2: Event Creation
- **Priority**: P1 (High)
- **Description**: Organizers can create and publish events
- **Acceptance Criteria**:
  - Required fields: title, description, venue, start/end dates
  - Support for markdown formatting in descriptions
  - Draft/published state management
  - Event cover image upload (max 5MB)
  - Timezone selection

### FR-3: Ticket Tier Configuration
- **Priority**: P1 (High)
- **Description**: Organizers define multiple ticket tiers per event
- **Acceptance Criteria**:
  - Tier fields: name, description, price, quantity, sale start/end
  - Minimum 1 tier, maximum 10 tiers per event
  - Inventory tracking per tier
  - Tier visibility control

### FR-4: Shopping Cart
- **Priority**: P1 (High)
- **Description**: Attendees add tickets to cart with temporary reservation
- **Acceptance Criteria**:
  - Cart items reserved for 10 minutes
  - Expired reservations auto-release inventory
  - Cart persistence across sessions (logged-in users)
  - Quantity validation against available inventory

### FR-5: Payment Processing
- **Priority**: P0 (Critical)
- **Description**: Secure payment via SePay VietQR
- **Acceptance Criteria**:
  - Generate unique payment QR code per order
  - Display bank transfer instructions
  - Webhook confirmation within 2 minutes
  - Idempotent webhook handling
  - Transaction log retention (7 years)

### FR-6: Ticket Delivery
- **Priority**: P1 (High)
- **Description**: Generate and deliver QR tickets post-payment
- **Acceptance Criteria**:
  - Unique QR code per ticket (UUID-based)
  - Email delivery within 5 minutes of payment
  - PDF ticket attachment
  - Ticket list accessible in user dashboard

### FR-7: Check-In System
- **Priority**: P1 (High)
- **Description**: QR scanning for event entry validation
- **Acceptance Criteria**:
  - Mobile-friendly scanner interface
  - Instant validation feedback (valid/invalid/already used)
  - Offline mode support (future)
  - Check-in timestamp recording

### FR-8: Analytics Dashboard
- **Priority**: P2 (Medium)
- **Description**: Sales and attendee insights for organizers
- **Acceptance Criteria**:
  - Real-time sales count and revenue
  - Chart visualizations (line, bar, pie)
  - Date range filtering
  - Export to CSV

### FR-9: Promo Code System
- **Priority**: P2 (Medium)
- **Description**: Discount codes for marketing campaigns
- **Acceptance Criteria**:
  - Percentage or fixed amount discounts
  - Usage limit per code
  - Valid date range
  - One-time use per user (optional)
  - Code validation at checkout

### FR-10: Admin Panel
- **Priority**: P2 (Medium)
- **Description**: Platform management for administrators
- **Acceptance Criteria**:
  - User role management
  - Event moderation (approve/reject/unpublish)
  - Financial dashboard (platform revenue)
  - Activity logs

## Non-Functional Requirements

### NFR-1: Performance
- Page load time < 2 seconds (p95)
- API response time < 500ms (p95)
- Database query optimization via indexes
- Frontend code splitting and lazy loading
- CDN for static assets (future)

### NFR-2: Scalability
- Support 1000 concurrent users (v1.0 target)
- Horizontal scaling via stateless backend
- Database connection pooling
- Redis caching for sessions (future)
- Load balancer support (future)

### NFR-3: Security
- HTTPS enforcement in production
- Environment-configurable CORS origins
- Security headers (X-Frame-Options, CSP, etc.)
- SQL injection prevention via parameterized queries
- XSS protection via output encoding
- Rate limiting on authentication endpoints
- API key authentication for webhooks
- Sensitive data encryption at rest (future)

### NFR-4: Reliability
- 99.5% uptime SLA
- Automated database backups (daily)
- Error logging with stack traces
- Health check endpoints
- Graceful degradation on payment failures

### NFR-5: Maintainability
- Clean Architecture separation (Core, Infrastructure, API)
- Type-safe API contracts via NSwag
- Comprehensive unit test coverage (>70%)
- Integration tests for critical flows
- Code documentation and inline comments
- Consistent code formatting (linting)

### NFR-6: Usability
- Mobile-responsive design (Tailwind CSS)
- Accessible UI (WCAG 2.1 Level AA)
- Intuitive navigation
- Contextual help text
- Error messages with actionable guidance

### NFR-7: Compatibility
- Browser support: Chrome, Firefox, Safari, Edge (latest 2 versions)
- Node.js 20+ for frontend
- .NET 8 for backend
- PostgreSQL 16
- Docker containerization

## Technical Constraints

### Technology Stack (Fixed)
- **Frontend**: Next.js 15 (App Router), React 19, TypeScript, shadcn/ui, Tailwind CSS 4
- **Backend**: ASP.NET Core 8 Minimal APIs, C#
- **Database**: PostgreSQL 16, Entity Framework Core 8
- **Authentication**: ASP.NET Identity + NextAuth.js
- **Payments**: SePay API (REST)
- **Containerization**: Docker, Docker Compose
- **Type Safety**: NSwag for C# to TypeScript generation

### Infrastructure Constraints
- Monorepo structure (single repository)
- Development: Docker Compose with hot reload
- Production: TBD (Azure, AWS, or DigitalOcean)
- Database migrations via EF Core
- Environment variable-based configuration

### Third-Party Dependencies
- **SePay**: Vietnamese payment gateway, manual refunds required
- **Email Service**: SMTP (SendGrid or similar) - future
- **Image Storage**: Local filesystem (v1.0), S3-compatible storage (future)

## Dependencies

### External Dependencies
- SePay API availability (99% uptime expected)
- Email delivery service (future)
- DNS and domain registration (future)

### Internal Dependencies
- Phase 01 (Setup) blocks all phases
- Phase 02 (Database) blocks 03-11
- Phase 04 (Auth) blocks 05-10
- Phase 05 (Events) blocks 06-08
- Phase 06 (Cart) blocks 07 (Payment)

## Risks and Mitigation

| Risk | Probability | Impact | Mitigation Strategy |
|------|-------------|--------|---------------------|
| SePay API changes | Medium | High | Version pinning, webhook validation, fallback manual verification |
| Race conditions on inventory | Medium | High | Optimistic locking, cart reservations, transactional updates |
| Payment webhook failures | Low | Critical | Retry mechanism, idempotency keys, manual reconciliation dashboard |
| Data loss | Low | Critical | Automated backups, point-in-time recovery, replication (future) |
| Performance degradation | Medium | Medium | Caching, query optimization, horizontal scaling readiness |
| Security breach | Low | Critical | Security headers, input validation, dependency updates, penetration testing |

## Compliance and Legal

### Data Privacy
- GDPR compliance considerations (if EU users) - future
- User data retention policy (account deletion = anonymize orders)
- Cookie consent (future)

### Financial
- Transaction logging for audit trail (7-year retention)
- Financial reporting capabilities
- Tax calculation (VAT/GST) - out of scope for v1.0

### Accessibility
- WCAG 2.1 Level AA target
- Keyboard navigation support
- Screen reader compatibility

## Deployment Strategy

### Phases
1. **Development** (Current): Docker Compose with hot reload
2. **Staging**: Pre-production testing environment (future)
3. **Production**: Cloud deployment with CI/CD (future)

### Rollout Plan
- Internal testing (Phase 11)
- Closed beta with 1-2 organizers
- Public launch

## Success Criteria - v1.0 Launch

### Technical
- [ ] All 11 phases completed
- [ ] Test coverage > 70%
- [ ] Zero critical security vulnerabilities
- [ ] API documentation complete (Swagger)
- [ ] Deployment runbook documented

### Business
- [ ] 5+ events created by 3+ organizers
- [ ] 100+ tickets sold
- [ ] Payment success rate > 90%
- [ ] Positive user feedback (NPS > 40)

## Future Enhancements (Roadmap)

### v1.1 (Q2 2026)
- OAuth social login (Google, Facebook)
- Email notifications (order confirmation, event reminders)
- Advanced analytics (conversion funnels, cohort analysis)

### v1.2 (Q3 2026)
- Recurring events
- Seat selection for venues
- Mobile app (React Native)

### v2.0 (Q4 2026)
- Multi-currency support
- Stripe/PayPal integration
- Affiliate/referral programs
- Advanced CRM and email marketing

## Glossary

- **Ticket Tier**: Pricing category for event tickets (e.g., VIP, General Admission)
- **Cart Reservation**: Temporary inventory hold preventing overselling
- **Optimistic Locking**: Concurrency control via version/timestamp comparison
- **VietQR**: Vietnamese standardized QR payment format
- **SePay**: Third-party payment gateway for Vietnamese bank transfers
- **NSwag**: OpenAPI toolchain for generating TypeScript from C# contracts
- **Clean Architecture**: Layered design (Core, Infrastructure, API) for testability

## References

- [Main Plan](../plans/260202-1213-event-ticket-manager/plan.md)
- [Architecture Research](../plans/reports/researcher-260202-1222-nextjs-dotnet-architecture.md)
- [Ticketing Features Research](../plans/reports/researcher-260202-1222-event-ticketing-features.md)
- [SePay Integration Research](../plans/reports/researcher-260202-1222-sepay-integration.md)
- [Phase 01 Code Review](../plans/reports/code-reviewer-260203-2046-phase01-setup.md)
