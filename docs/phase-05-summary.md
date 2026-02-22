# Phase 05: Event Management - Summary

**Status:** ⏳ PENDING
**Effort:** 10h
**Priority:** P1 (Core feature)

---

## What Is Planned

### Event Management Features
- [ ] Full event CRUD operations (organizer only)
- [ ] Multiple ticket tiers per event
- [ ] Event status lifecycle management
- [ ] Image upload for event banners
- [ ] Event search and filtering
- [ ] Category/tag system
- [ ] Organizer dashboard

### Event State Machine

```
    ┌─────────┐
    │  Draft  │
    └────┬────┘
         │ publish()
         ▼
    ┌─────────┐
    │Published│
    └────┬────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌─────────┐ ┌──────────┐
│Cancelled│ │Completed │
└─────────┘ └──────────┘
```

---

## API Endpoints

```
GET    /api/events              # List (public, paginated)
GET    /api/events/{id}         # Details (public)
POST   /api/events              # Create (organizer)
PUT    /api/events/{id}         # Update (owner)
DELETE /api/events/{id}         # Delete (owner, draft only)
POST   /api/events/{id}/publish # Publish (owner)
POST   /api/events/{id}/cancel  # Cancel (owner)

GET    /api/events/{eventId}/tiers
POST   /api/events/{eventId}/tiers
PUT    /api/events/{eventId}/tiers/{tierId}
DELETE /api/events/{eventId}/tiers/{tierId}

POST   /api/events/{id}/image   # Upload image
```

### Request/Response Examples

```csharp
// Create Event
public record CreateEventRequest(
    string Title,
    string Description,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int TotalCapacity,
    List<CreateTicketTierRequest> Tiers
);

// Create Ticket Tier
public record CreateTicketTierRequest(
    string Name,
    string Description,
    decimal Price,
    int QuantityTotal,
    DateTime SaleStartDateTime,
    DateTime SaleEndDateTime
);

// Event Response
public record EventResponse(
    Guid Id,
    string Title,
    string Description,
    string VenueName,
    string VenueCity,
    DateTime StartDateTime,
    string Status,
    string ImageUrl,
    int TicketsAvailable,
    List<TicketTierResponse> Tiers
);
```

---

## Frontend Pages

### Public Pages
- `/events` - Event listing with search/filters
- `/events/[id]` - Event details with ticket tiers

### Organizer Dashboard
- `/dashboard/events` - My events list
- `/dashboard/events/new` - Create event (multi-step form)
- `/dashboard/events/[id]/edit` - Edit event
- `/dashboard/events/[id]/tiers` - Manage ticket tiers

---

## Implementation Steps

### 1. Backend Event Service (2h)
- [ ] IEventService interface
- [ ] EventService with CRUD operations
- [ ] Authorization checks (owner only)
- [ ] Event status transitions
- [ ] Tier overlap validation

### 2. Backend Event Endpoints (2h)
- [ ] Event CRUD endpoints
- [ ] Tier management endpoints
- [ ] Image upload with file validation
- [ ] Request validation
- [ ] Pagination and filtering

### 3. Frontend Event Listing (2h)
- [ ] Event card component
- [ ] Event list with grid layout
- [ ] Search input and filters
- [ ] Pagination
- [ ] Loading skeletons

### 4. Frontend Event Details (1.5h)
- [ ] Event details page
- [ ] Ticket tiers display
- [ ] "Get Tickets" button
- [ ] Map embed for venue
- [ ] Share buttons

### 5. Frontend Event Management (2.5h)
- [ ] Multi-step event form
- [ ] Ticket tier management UI
- [ ] Image upload with preview
- [ ] Form validation
- [ ] Organizer dashboard layout
- [ ] Event status badges

---

## Success Criteria

- [ ] Organizer can create event with tiers
- [ ] Event appears in public listing
- [ ] Event details show correct availability
- [ ] Image upload works and displays
- [ ] Search and filters work correctly
- [ ] Only owner can edit their events

---

## Security Considerations

- Validate image file types (jpg, png, webp only)
- Limit image size (max 5MB)
- Authorize all organizer operations
- Sanitize event description HTML
- Rate limit event creation

---

## Next Phase

After completion, proceed to **[Phase 06: Ticket Purchasing](phase-06-summary.md)**
