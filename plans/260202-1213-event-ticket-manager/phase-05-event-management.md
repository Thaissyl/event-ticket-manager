# Phase 05: Event Management

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)

## Overview
- **Priority:** P1 (Core feature)
- **Status:** pending
- **Effort:** 10h
- **Description:** Full event CRUD with ticket tiers, organizer dashboard

## Key Insights
- Events have multiple ticket tiers (GA, VIP, Early Bird)
- Organizers can only manage their own events
- Events have status lifecycle: Draft -> Published -> Completed/Cancelled
- Image uploads for event banners
- Category/tag filtering for discovery

## Requirements

### Functional
- F1: Create/edit/delete events (organizer only)
- F2: Multiple ticket tiers per event
- F3: Event status management
- F4: Event image upload
- F5: Event search and filtering
- F6: Event categories/tags
- F7: Organizer dashboard with event list

### Non-Functional
- NF1: Event listing <200ms response
- NF2: Image optimization (resize, compress)
- NF3: Pagination for event lists

## Architecture

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

### API Endpoints

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

### DTOs

```csharp
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

public record CreateTicketTierRequest(
    string Name,
    string Description,
    decimal Price,
    int QuantityTotal,
    DateTime SaleStartDateTime,
    DateTime SaleEndDateTime
);

public record EventResponse(
    Guid Id,
    string Title,
    string Description,
    string VenueName,
    string VenueAddress,
    string VenueCity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string Status,
    string ImageUrl,
    int TotalCapacity,
    int TicketsAvailable,
    OrganizerInfo Organizer,
    List<TicketTierResponse> Tiers
);

public record EventListItem(
    Guid Id,
    string Title,
    string VenueCity,
    DateTime StartDateTime,
    string ImageUrl,
    decimal MinPrice,
    int TicketsAvailable
);
```

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/EventEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/TicketTierEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Events/CreateEventRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Events/UpdateEventRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Events/EventResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Events/EventListItem.cs`
- `src/backend/EventTickets.Core/DTOs/TicketTiers/CreateTicketTierRequest.cs`
- `src/backend/EventTickets.Core/DTOs/TicketTiers/TicketTierResponse.cs`
- `src/backend/EventTickets.Core/Services/IEventService.cs`
- `src/backend/EventTickets.Infrastructure/Services/EventService.cs`
- `src/backend/EventTickets.Infrastructure/Services/ImageService.cs`

### Create (Frontend)
- `src/frontend/app/(public)/events/page.tsx` - Event listing
- `src/frontend/app/(public)/events/[id]/page.tsx` - Event details
- `src/frontend/app/(dashboard)/dashboard/page.tsx` - Organizer home
- `src/frontend/app/(dashboard)/dashboard/events/page.tsx` - My events
- `src/frontend/app/(dashboard)/dashboard/events/new/page.tsx` - Create event
- `src/frontend/app/(dashboard)/dashboard/events/[id]/edit/page.tsx` - Edit event
- `src/frontend/components/events/event-card.tsx`
- `src/frontend/components/events/event-list.tsx`
- `src/frontend/components/events/event-form.tsx`
- `src/frontend/components/events/ticket-tier-form.tsx`
- `src/frontend/components/events/event-filters.tsx`

## Implementation Steps

### 1. Backend Event Service (2h)
- [ ] Create `IEventService` interface
- [ ] Implement `EventService` with CRUD operations
- [ ] Add authorization checks (owner only)
- [ ] Implement event status transitions
- [ ] Add validation for tier overlaps

### 2. Backend Event Endpoints (2h)
- [ ] Implement all event CRUD endpoints
- [ ] Implement tier management endpoints
- [ ] Add image upload endpoint with file validation
- [ ] Add request validation
- [ ] Add pagination and filtering

### 3. Frontend Event Listing (2h)
- [ ] Create event card component
- [ ] Create event list with grid layout
- [ ] Add search input and filters
- [ ] Implement pagination
- [ ] Add loading skeletons

### 4. Frontend Event Details (1.5h)
- [ ] Create event details page
- [ ] Display ticket tiers with availability
- [ ] Add "Get Tickets" button
- [ ] Show event location with map embed
- [ ] Add share buttons

### 5. Frontend Event Management (2.5h)
- [ ] Create multi-step event form
- [ ] Add ticket tier management UI
- [ ] Implement image upload with preview
- [ ] Add form validation
- [ ] Create organizer dashboard layout
- [ ] Add event status badges

## Todo List
- [ ] Implement EventService
- [ ] Create event CRUD endpoints
- [ ] Add tier management endpoints
- [ ] Implement image upload
- [ ] Create event listing page
- [ ] Create event details page
- [ ] Build organizer dashboard
- [ ] Create event form
- [ ] Add tier management UI
- [ ] Test complete event flow

## Success Criteria
- [ ] Organizer can create event with tiers
- [ ] Event appears in public listing
- [ ] Event details show correct availability
- [ ] Image upload works and displays
- [ ] Search and filters work correctly
- [ ] Only owner can edit their events

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Image storage costs | Medium | Medium | Compress images, set limits |
| Slow queries | Medium | Medium | Add indexes, caching |
| Tier pricing errors | Low | High | Validation, audit logs |

## Security Considerations
- Validate image file types (allow only jpg, png, webp)
- Limit image size (max 5MB)
- Authorize all organizer operations
- Sanitize event description HTML
- Rate limit event creation

## Next Steps
After completion, proceed to [Phase 06: Ticket Purchasing](phase-06-ticket-purchasing.md)
