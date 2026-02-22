# Phase 02: Database Schema - Summary

**Status:** ⏳ PENDING
**Effort:** 6h
**Priority:** P1 (Critical - blocks all features)

---

## What Is Planned

### Entity Design
- [ ] Create entity classes in `EventTickets.Core/Entities/`
- [ ] Create enum types in `EventTickets.Core/Enums/`
- [ ] Add navigation properties for relationships
- [ ] Add data annotations and Fluent API configurations

### Core Entities

#### Users (ASP.NET Identity)
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public UserRole Role { get; set; }  // Admin, Organizer, Attendee
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Events
- Id, OrganizerId, Title, Description
- Venue info (Name, Address, City)
- Start/End DateTime
- Status (Draft, Published, Cancelled, Completed)
- ImageUrl, TotalCapacity
- **RowVersion** (optimistic locking)

#### Ticket Tiers
- EventId, Name, Description, Price
- QuantityTotal, QuantitySold, QuantityReserved
- SaleStart/End DateTime
- **RowVersion** (optimistic locking)

#### Orders
- UserId (nullable for guests), GuestEmail, GuestName
- TotalAmount, Status
- PaymentCode (SePay matching)
- CreatedAt, UpdatedAt

#### Tickets
- OrderId, TicketTierId
- QrCode, QrCodeSignature (HMAC)
- AttendeeName, AttendeeEmail
- Status (Valid, Used, Cancelled, Refunded)
- CheckedInAt

#### Cart Reservations
- SessionId, TicketTierId, Quantity
- ExpiresAt (15-minute timeout)
- CreatedAt

#### Payments
- OrderId, SePayTransactionId
- Amount, Status, ReferenceCode
- PaidAt, CreatedAt

#### SePay Webhooks
- SePayTransactionId, Payload (JSON)
- Processed, ProcessingError
- CreatedAt

#### Promo Codes
- EventId (nullable for global), Code
- DiscountType (Percentage, FixedAmount), DiscountValue
- MaxUses, CurrentUses
- ValidFrom, ValidUntil

### Database Configuration
- [ ] Create `ApplicationDbContext` with Identity
- [ ] Configure entity relationships with Fluent API
- [ ] Add indexes for common queries
- [ ] Setup cascade delete rules
- [ ] Configure RowVersion for optimistic locking

### Migrations
- [ ] Add EF Core packages
- [ ] Create initial migration
- [ ] Apply migration to PostgreSQL
- [ ] Seed admin user

---

## Implementation Steps

### 1. Create Entity Classes (1.5h)
- ApplicationUser, Event, TicketTier, Order, Ticket
- CartReservation, Payment, SePayWebhook, PromoCode
- Enums: UserRole, EventStatus, OrderStatus, TicketStatus, PaymentStatus, DiscountType

### 2. Configure DbContext (1.5h)
- ApplicationDbContext extending IdentityDbContext
- Fluent API for relationships
- Indexes on: Events.OrganizerId, Events.Status, Tickets.QrCode
- Cascade delete configuration

### 3. Create Entity Configurations (1.5h)
- Separate configuration classes per entity
- String lengths, required fields
- Unique constraints (email, promo code, qr_code)
- Decimal precision for money

### 4. Create and Run Migrations (1h)
- `dotnet ef migrations add InitialCreate`
- Review generated migration
- `dotnet ef database update`

### 5. Seed Initial Data (0.5h)
- Admin user seeder
- Sample organizer/event (dev only)

---

## Success Criteria

- [ ] All entities created with correct relationships
- [ ] Migration applies cleanly to PostgreSQL
- [ ] Indexes exist on queried columns
- [ ] RowVersion configured for inventory tables
- [ ] Admin user seeded successfully

---

## Database Schema ERD

```
┌─────────────┐       ┌─────────────┐       ┌──────────────┐
│   Users     │───1:N─│   Events    │───1:N─│ TicketTiers  │
└─────────────┘       └─────────────┘       └──────────────┘
       │                                            │
       │              ┌─────────────┐               │
       └───1:N───────│   Orders    │───────────────┘
                      └─────────────┘        │
                             │               │
                      ┌──────┴──────┐        │
                      ▼             ▼        ▼
               ┌──────────┐  ┌────────────────┐
               │ Payments │  │    Tickets     │
               └──────────┘  └────────────────┘
                      ▲
                      │
               ┌──────────────┐
               │SePayWebhooks │
               └──────────────┘

┌─────────────┐       ┌─────────────┐
│ PromoCodes  │───1:N─│   Events    │
└─────────────┘       └─────────────┘

┌─────────────────┐
│ CartReservations│
└─────────────────┘
```

---

## Next Phase

After completion, proceed to **[Phase 03: Backend API Structure](phase-03-summary.md)**
