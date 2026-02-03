# Phase 02: Database Schema

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)
- [SePay Report](../reports/researcher-260202-1222-sepay-integration.md)

## Overview
- **Priority:** P1 (Critical - blocks all features)
- **Status:** pending
- **Effort:** 6h
- **Description:** Design and implement PostgreSQL schema with EF Core migrations

## Key Insights
- Use UUIDs for primary keys (better for distributed systems)
- Optimistic locking via `RowVersion` for inventory updates
- Cart reservations table for temporary ticket holds
- Separate payment tracking for SePay transactions

## Requirements

### Functional
- F1: User accounts with roles (Admin, Organizer, Attendee)
- F2: Events with multiple ticket tiers
- F3: Orders and tickets with QR codes
- F4: Cart reservations with expiry
- F5: Promo codes with usage tracking
- F6: SePay webhook/transaction logging

### Non-Functional
- NF1: Indexes for common queries
- NF2: Cascading deletes where appropriate
- NF3: Audit timestamps on all tables

## Architecture

### Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────┐       ┌──────────────┐
│   Users     │───1:N─│   Events    │───1:N─│ TicketTiers  │
└─────────────┘       └─────────────┘       └──────────────┘
       │                                            │
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
```

### Core Entities

```csharp
// Users - ASP.NET Identity extended
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public UserRole Role { get; set; }  // Admin, Organizer, Attendee
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Events
public class Event
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string VenueName { get; set; }
    public string VenueAddress { get; set; }
    public string VenueCity { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public EventStatus Status { get; set; }  // Draft, Published, Cancelled, Completed
    public string ImageUrl { get; set; }
    public int TotalCapacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public uint RowVersion { get; set; }  // Optimistic locking
}

// Ticket Tiers
public class TicketTier
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int QuantityTotal { get; set; }
    public int QuantitySold { get; set; }
    public int QuantityReserved { get; set; }
    public DateTime SaleStartDateTime { get; set; }
    public DateTime SaleEndDateTime { get; set; }
    public uint RowVersion { get; set; }
}

// Orders
public class Order
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }  // Nullable for guest checkout
    public string GuestEmail { get; set; }
    public string GuestName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }  // Pending, Paid, Cancelled, Refunded
    public string PaymentCode { get; set; }  // For SePay matching
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Tickets
public class Ticket
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTierId { get; set; }
    public string QrCode { get; set; }  // UUID-based unique code
    public string QrCodeSignature { get; set; }  // HMAC for validation
    public string AttendeeName { get; set; }
    public string AttendeeEmail { get; set; }
    public TicketStatus Status { get; set; }  // Valid, Used, Cancelled, Refunded
    public DateTime? CheckedInAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Cart Reservations (temporary holds)
public class CartReservation
{
    public Guid Id { get; set; }
    public string SessionId { get; set; }  // Browser session or user ID
    public Guid TicketTierId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Payments
public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public long? SePayTransactionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }  // Pending, Completed, Failed
    public string ReferenceCode { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// SePay Webhook Logs
public class SePayWebhook
{
    public Guid Id { get; set; }
    public long SePayTransactionId { get; set; }
    public string Payload { get; set; }  // JSON
    public bool Processed { get; set; }
    public string ProcessingError { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Promo Codes
public class PromoCode
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }  // Null for global codes
    public string Code { get; set; }
    public DiscountType DiscountType { get; set; }  // Percentage, FixedAmount
    public decimal DiscountValue { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}
```

## Related Code Files

### Create
- `src/backend/EventTickets.Core/Entities/ApplicationUser.cs`
- `src/backend/EventTickets.Core/Entities/Event.cs`
- `src/backend/EventTickets.Core/Entities/TicketTier.cs`
- `src/backend/EventTickets.Core/Entities/Order.cs`
- `src/backend/EventTickets.Core/Entities/Ticket.cs`
- `src/backend/EventTickets.Core/Entities/CartReservation.cs`
- `src/backend/EventTickets.Core/Entities/Payment.cs`
- `src/backend/EventTickets.Core/Entities/SePayWebhook.cs`
- `src/backend/EventTickets.Core/Entities/PromoCode.cs`
- `src/backend/EventTickets.Core/Enums/UserRole.cs`
- `src/backend/EventTickets.Core/Enums/EventStatus.cs`
- `src/backend/EventTickets.Core/Enums/OrderStatus.cs`
- `src/backend/EventTickets.Core/Enums/TicketStatus.cs`
- `src/backend/EventTickets.Core/Enums/PaymentStatus.cs`
- `src/backend/EventTickets.Core/Enums/DiscountType.cs`
- `src/backend/EventTickets.Infrastructure/Data/ApplicationDbContext.cs`
- `src/backend/EventTickets.Infrastructure/Data/Configurations/*.cs`

## Implementation Steps

### 1. Create Entity Classes (1.5h)
- [ ] Create all entity classes in `EventTickets.Core/Entities`
- [ ] Create enum types in `EventTickets.Core/Enums`
- [ ] Add navigation properties for relationships
- [ ] Add data annotations where needed

### 2. Configure DbContext (1.5h)
- [ ] Create `ApplicationDbContext` extending `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- [ ] Configure entity relationships with Fluent API
- [ ] Add indexes for common query patterns
- [ ] Configure cascade delete rules
- [ ] Setup `RowVersion` for optimistic locking

### 3. Create Entity Configurations (1.5h)
- [ ] Create separate configuration classes per entity
- [ ] Configure string lengths and required fields
- [ ] Setup unique constraints (email, promo code, qr_code)
- [ ] Configure decimal precision for money fields

### 4. Create and Run Migrations (1h)
- [ ] Add EF Core packages to Infrastructure project
- [ ] Configure connection string in `appsettings.json`
- [ ] Create initial migration: `dotnet ef migrations add InitialCreate`
- [ ] Review generated migration for correctness
- [ ] Apply migration: `dotnet ef database update`

### 5. Seed Initial Data (0.5h)
- [ ] Create admin user seeder
- [ ] Create sample organizer and event (dev only)
- [ ] Add seed data migration

## Todo List
- [ ] Create all entity classes
- [ ] Create enum types
- [ ] Configure ApplicationDbContext
- [ ] Create entity configuration classes
- [ ] Add indexes for performance
- [ ] Generate EF Core migration
- [ ] Apply migration to PostgreSQL
- [ ] Seed admin user

## Success Criteria
- [ ] All entities created with correct relationships
- [ ] Migration applies cleanly to PostgreSQL
- [ ] Indexes exist on frequently queried columns
- [ ] RowVersion configured for inventory tables
- [ ] Admin user seeded successfully

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Schema changes later | High | Medium | Use migrations for all changes |
| Performance issues | Medium | High | Review query plans, add indexes |
| Data integrity issues | Low | High | Foreign keys, constraints |

## Security Considerations
- Password hashes via ASP.NET Identity (bcrypt)
- Never store plaintext sensitive data
- Mask QR code signatures in logs

## Next Steps
After completion, proceed to [Phase 03: Backend API Structure](phase-03-backend-api-structure.md)
