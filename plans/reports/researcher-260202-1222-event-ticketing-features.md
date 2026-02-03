# Event Ticketing Platform Research Report

**Date:** 2026-02-02
**Focus:** Core features, database design, inventory management, QR codes, analytics

---

## 1. Core Features (Eventbrite/Ticketmaster Patterns)

### Event Management
- **Event CRUD**: Create/edit/delete events with rich details (title, description, venue, dates, images)
- **Multi-session support**: Recurring events, multiple dates/times
- **Ticket tiers**: General admission, VIP, early bird, group discounts
- **Capacity management**: Total capacity, per-tier limits
- **Venue integration**: Location data, seating charts (optional advanced feature)
- **Event categories/tags**: Filtering and discovery

### Ticket Purchasing
- **Shopping cart**: Hold tickets temporarily (5-15 min timeout)
- **Pricing strategies**: Dynamic pricing, promo codes, bulk discounts
- **Payment integration**: Stripe/PayPal/credit cards
- **Guest checkout**: No account required for basic purchases
- **Order confirmation**: Email with ticket details and QR codes
- **PDF ticket generation**: Downloadable tickets with QR codes

### User Management
- **Roles**: Admin, Organizer, Attendee
- **Authentication**: Email/password, OAuth (Google, Facebook)
- **User profiles**: Purchase history, saved payment methods
- **Organizer dashboard**: Manage events, view sales, export data
- **Admin panel**: Platform oversight, analytics, user management

### Discovery & Search
- **Search/filter**: By date, location, category, price range
- **Featured events**: Promoted listings
- **Recommendations**: Based on user history/preferences

---

## 2. Database Schema Design

### Core Tables

**users**
```
id (PK, UUID)
email (unique, indexed)
password_hash
full_name
role (enum: admin, organizer, attendee)
created_at, updated_at
```

**events**
```
id (PK, UUID)
organizer_id (FK -> users.id)
title, description
venue_name, venue_address, venue_city, venue_country
start_datetime, end_datetime
status (enum: draft, published, cancelled, completed)
image_url
total_capacity
created_at, updated_at
```

**ticket_tiers**
```
id (PK, UUID)
event_id (FK -> events.id, cascading delete)
name (e.g., "General Admission", "VIP")
description
price (decimal)
quantity_total
quantity_sold (calculated or cached)
sale_start_datetime, sale_end_datetime
```

**orders**
```
id (PK, UUID)
user_id (FK -> users.id, nullable for guest checkout)
guest_email (for guests)
total_amount (decimal)
payment_status (enum: pending, completed, failed, refunded)
payment_intent_id (Stripe reference)
created_at, updated_at
```

**tickets**
```
id (PK, UUID)
order_id (FK -> orders.id)
ticket_tier_id (FK -> ticket_tiers.id)
qr_code (unique, indexed - UUID or hash)
attendee_name, attendee_email
checked_in_at (nullable timestamp)
status (enum: valid, used, cancelled, refunded)
created_at
```

**promo_codes**
```
id (PK, UUID)
event_id (FK -> events.id, nullable for global codes)
code (unique, indexed)
discount_type (enum: percentage, fixed_amount)
discount_value (decimal)
max_uses, current_uses
valid_from, valid_until
```

### Indexes
- `events(start_datetime, status)` - event listings
- `tickets(qr_code)` - fast check-in lookups
- `orders(user_id, created_at)` - purchase history
- `ticket_tiers(event_id, sale_end_datetime)` - availability checks

---

## 3. Ticket Inventory Management & Race Conditions

### Critical Challenge
Multiple users attempting to purchase last tickets simultaneously.

### Solutions

**A. Pessimistic Locking (Database-level)**
```sql
BEGIN TRANSACTION;
SELECT quantity_sold FROM ticket_tiers
WHERE id = ? FOR UPDATE;  -- Row-level lock

UPDATE ticket_tiers
SET quantity_sold = quantity_sold + ?
WHERE id = ? AND (quantity_sold + ?) <= quantity_total;

COMMIT;
```
- Pros: Guarantees consistency
- Cons: Lower throughput under high contention

**B. Optimistic Locking (Version-based)**
```sql
UPDATE ticket_tiers
SET quantity_sold = quantity_sold + ?, version = version + 1
WHERE id = ? AND version = ? AND (quantity_sold + ?) <= quantity_total;
-- Check affected rows, retry if 0
```
- Pros: Better performance
- Cons: Requires retry logic

**C. Shopping Cart Reservations**
- Reserve tickets for 5-15 minutes during checkout
- Create `cart_reservations` table with expiration timestamps
- Background job releases expired reservations
- Deduct from `quantity_available = quantity_total - quantity_sold - reserved_count`

**D. Queue System (High-demand events)**
- Implement waiting room (e.g., AWS SQS, Redis queue)
- Token-based access to purchase flow
- Prevents thundering herd on ticket release

**Recommended Approach:** Combination of B + C
- Optimistic locking for inventory updates
- Cart reservations prevent overselling
- Cleanup job runs every minute

---

## 4. QR Code Generation & Check-in System

### QR Code Strategy

**Content Format**
```
ticket:{ticket_uuid}:v1:{checksum}
```
- UUID: Unique ticket identifier
- Version: Future schema changes
- Checksum: Prevent tampering (HMAC-SHA256 with secret key)

**Generation**
- On order completion, generate QR per ticket
- Store as SVG/PNG or generate on-demand from UUID
- Libraries: `qrcode` (Node.js), `python-qrcode` (Python)

**Security**
- Never embed sensitive data (names, emails) in QR
- QR only contains ticket ID + signature
- Validate signature on scan to prevent forgery

### Check-in Flow

**Mobile Scanner App (Organizer)**
1. Scan QR code
2. Parse ticket UUID
3. API call: `POST /api/checkin` with UUID
4. Backend validates:
   - Ticket exists
   - Belongs to this event
   - Not already checked in (`checked_in_at IS NULL`)
   - Order payment completed
5. Update `tickets.checked_in_at = NOW()`
6. Return success + attendee name

**Offline Mode (Advanced)**
- Pre-download valid ticket list before event
- Store locally, sync check-ins when online
- Conflict resolution on sync

**Fraud Prevention**
- Rate limiting on check-in API
- Flag duplicate scan attempts (someone photographed another's ticket)
- Real-time alerts for anomalies

---

## 5. Analytics & Reporting Features

### Organizer Analytics

**Sales Metrics**
- Total revenue, tickets sold (by tier, over time)
- Conversion funnel: Views → Carts → Purchases
- Average order value, revenue per ticket tier
- Promo code effectiveness

**Attendance Tracking**
- Check-in rate (checked_in vs total sold)
- Peak check-in times (for staffing)
- No-show rate

**Geographic Data**
- Attendee locations (by city/country from user profiles)
- Popular venues/regions

**Time-series Charts**
- Daily/weekly ticket sales trends
- Sales velocity (tickets/hour during launch)

### Platform-level Analytics (Admin)

**Business Metrics**
- GMV (Gross Merchandise Value)
- Platform fees collected
- Active organizers, events created
- User growth (signups, retention)

**Event Performance**
- Top-grossing events
- Fastest-selling events
- Category popularity

**Data Export**
- CSV export of attendee lists (GDPR-compliant)
- Financial reports for accounting
- Custom date ranges

### Technical Implementation

**Tools**
- Aggregation queries with `GROUP BY`, window functions
- Materialized views for expensive calculations
- Time-series DB (TimescaleDB) or caching layer (Redis) for real-time dashboards
- Charting libraries: Chart.js, Recharts, D3.js

**Example Query (Sales by Tier)**
```sql
SELECT
  tt.name AS tier_name,
  COUNT(t.id) AS tickets_sold,
  SUM(tt.price) AS revenue
FROM tickets t
JOIN ticket_tiers tt ON t.ticket_tier_id = tt.id
WHERE tt.event_id = ?
GROUP BY tt.id, tt.name;
```

---

## Key Architectural Patterns

### Event-Driven Architecture
- Use events (RabbitMQ/Kafka) for async tasks: email sending, QR generation, analytics updates
- Decouple order processing from ticket generation

### CQRS (Command Query Responsibility Segregation)
- Separate write models (order creation) from read models (analytics dashboards)
- Read replicas for reporting queries

### API Design
- RESTful endpoints: `GET /api/events`, `POST /api/orders`
- Idempotency keys for order creation (prevent double-charges on retries)
- Pagination for event listings
- Rate limiting to prevent abuse

### Scalability Considerations
- CDN for event images, static assets
- Database read replicas for analytics
- Horizontal scaling for API servers
- Redis caching for hot events

---

## Unresolved Questions

1. **Payment processor choice**: Stripe vs PayPal vs multi-provider support?
2. **Refund policy automation**: Partial refunds, cancellation windows?
3. **Mobile app requirements**: Native iOS/Android or PWA?
4. **Internationalization**: Multi-currency, timezone handling, localization?
5. **Accessibility standards**: WCAG compliance level (A, AA, AAA)?
6. **Seat selection feature**: General admission only or reserved seating?
7. **Waitlist functionality**: Auto-notify when tickets available?
8. **Social features**: Share events, invite friends, group purchases?
