# Phase 08: QR Tickets & Check-in

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)

## Overview
- **Priority:** P2 (Important for event day)
- **Status:** pending
- **Effort:** 6h
- **Description:** QR code generation, PDF tickets, check-in scanner

## Key Insights
- QR code contains ticket UUID + HMAC signature
- Signature prevents forgery
- Check-in validates ticket status and event
- Scanner works on mobile (organizer app or web)
- Offline mode for venues with poor connectivity

## Requirements

### Functional
- F1: Generate unique QR code per ticket
- F2: PDF ticket with event details and QR
- F3: Email tickets to attendee
- F4: Check-in via QR scan
- F5: Prevent duplicate check-ins
- F6: Show attendee info on successful scan

### Non-Functional
- NF1: QR scan response <500ms
- NF2: PDF generation <2s
- NF3: Works on mobile browsers

## Architecture

### QR Code Structure

```
Format: ticket:{uuid}:v1:{signature}
Example: ticket:a1b2c3d4-e5f6-7890-abcd-ef1234567890:v1:abc123def456

Signature = HMAC-SHA256(ticket_uuid, SECRET_KEY).substring(0, 12)
```

### Check-in Flow

```
┌──────────┐     ┌───────────┐     ┌──────────────┐
│  Scanner │     │    API    │     │   Database   │
│  (Phone) │     │           │     │              │
└────┬─────┘     └─────┬─────┘     └──────┬───────┘
     │                 │                   │
     │ Scan QR         │                   │
     │ Parse Code      │                   │
     │                 │                   │
     │ POST /checkin   │                   │
     │────────────────>│                   │
     │                 │                   │
     │                 │ Validate Signature│
     │                 │                   │
     │                 │ Find Ticket       │
     │                 │──────────────────>│
     │                 │                   │
     │                 │ Check Status      │
     │                 │ - valid?          │
     │                 │ - not used?       │
     │                 │ - event matches?  │
     │                 │                   │
     │                 │ Update checked_in │
     │                 │──────────────────>│
     │                 │                   │
     │ Success +       │                   │
     │ Attendee Name   │                   │
     │<────────────────│                   │
```

### PDF Ticket Layout

```
┌─────────────────────────────────────────┐
│  [Event Banner Image]                   │
├─────────────────────────────────────────┤
│  Event: Summer Music Festival 2026      │
│  Date: March 15, 2026 7:00 PM           │
│  Venue: Hanoi Opera House               │
├─────────────────────────────────────────┤
│  Ticket: VIP Access                     │
│  Attendee: John Doe                     │
│  Order: ETM-260202-ABC123               │
├─────────────────────────────────────────┤
│                                         │
│         ┌───────────────┐               │
│         │   [QR CODE]   │               │
│         │               │               │
│         └───────────────┘               │
│                                         │
│  Ticket ID: a1b2c3d4                    │
├─────────────────────────────────────────┤
│  Terms & Conditions...                  │
└─────────────────────────────────────────┘
```

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/CheckinEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/TicketEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Checkin/CheckinRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Checkin/CheckinResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Tickets/TicketPdfRequest.cs`
- `src/backend/EventTickets.Core/Services/IQrCodeService.cs`
- `src/backend/EventTickets.Core/Services/ICheckinService.cs`
- `src/backend/EventTickets.Core/Services/ITicketPdfService.cs`
- `src/backend/EventTickets.Infrastructure/Services/QrCodeService.cs`
- `src/backend/EventTickets.Infrastructure/Services/CheckinService.cs`
- `src/backend/EventTickets.Infrastructure/Services/TicketPdfService.cs`

### Create (Frontend)
- `src/frontend/app/(dashboard)/dashboard/events/[id]/checkin/page.tsx`
- `src/frontend/app/(public)/tickets/[id]/page.tsx` - View ticket
- `src/frontend/components/tickets/ticket-display.tsx`
- `src/frontend/components/tickets/qr-code-display.tsx`
- `src/frontend/components/checkin/qr-scanner.tsx`
- `src/frontend/components/checkin/checkin-result.tsx`
- `src/frontend/components/checkin/checkin-stats.tsx`
- `src/frontend/hooks/use-qr-scanner.ts`

## Implementation Steps

### 1. QR Code Service (1h)
- [ ] Create `IQrCodeService` interface
- [ ] Implement QR code string generation
- [ ] Implement HMAC signature creation
- [ ] Implement signature validation
- [ ] Use QRCoder library for image generation

### 2. Check-in Service (1.5h)
- [ ] Create `ICheckinService` interface
- [ ] Parse and validate QR code
- [ ] Check ticket status and event
- [ ] Prevent duplicate check-ins
- [ ] Update `checked_in_at` timestamp
- [ ] Return attendee details

### 3. Check-in Endpoint (0.5h)
- [ ] `POST /api/checkin` - Process check-in
- [ ] `GET /api/events/{id}/checkins` - List check-ins
- [ ] `GET /api/events/{id}/checkin-stats` - Stats
- [ ] Add organizer authorization

### 4. PDF Ticket Service (1.5h)
- [ ] Create `ITicketPdfService` interface
- [ ] Use QuestPDF or iText for generation
- [ ] Include event details and QR code
- [ ] Add download endpoint
- [ ] Cache generated PDFs

### 5. Scanner UI (1.5h)
- [ ] Create scanner page for organizers
- [ ] Use html5-qrcode or react-qr-reader
- [ ] Show scan result with visual feedback
- [ ] Display attendee name and ticket type
- [ ] Show check-in count and stats
- [ ] Handle errors (invalid, already used)

## API Endpoints

```
POST /api/checkin
{
  "qrCode": "ticket:uuid:v1:signature"
}
Response: {
  "success": true,
  "attendeeName": "John Doe",
  "ticketTier": "VIP",
  "checkedInAt": "2026-03-15T19:05:00Z"
}

GET /api/events/{id}/checkin-stats
Response: {
  "totalSold": 500,
  "checkedIn": 423,
  "percentage": 84.6,
  "byTier": [
    { "name": "VIP", "sold": 50, "checkedIn": 48 },
    { "name": "GA", "sold": 450, "checkedIn": 375 }
  ]
}

GET /api/tickets/{id}/pdf
Response: PDF file download
```

## Todo List
- [ ] Implement QR code generation
- [ ] Implement signature validation
- [ ] Create check-in service
- [ ] Create check-in endpoint
- [ ] Implement PDF generation
- [ ] Build scanner UI
- [ ] Add check-in stats
- [ ] Test on mobile devices
- [ ] Handle edge cases

## Success Criteria
- [ ] QR codes generate correctly
- [ ] Check-in validates and records
- [ ] Duplicate scans rejected
- [ ] PDF tickets download correctly
- [ ] Scanner works on mobile
- [ ] Stats update in real-time

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Scanner compatibility | Medium | Medium | Test multiple devices |
| QR forgery | Low | High | HMAC signature |
| Slow check-in line | Medium | Medium | Optimize response time |

## Security Considerations
- HMAC signature prevents ticket forgery
- Rate limit check-in endpoint
- Log all check-in attempts
- Only event organizers can scan
- Validate event ID matches ticket

## Offline Mode (Future Enhancement)
1. Pre-download ticket list before event
2. Store locally with IndexedDB
3. Validate offline using cached data
4. Queue check-ins for sync
5. Sync when connectivity restored

## Next Steps
After completion, proceed to [Phase 09: Analytics Dashboard](phase-09-analytics-dashboard.md)
