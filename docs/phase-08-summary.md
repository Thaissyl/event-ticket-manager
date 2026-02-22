# Phase 08: QR Tickets & Check-in - Summary

**Status:** ⏳ PENDING
**Effort:** 6h
**Priority:** P2 (Important for event day)

---

## What Is Planned

### QR Ticket Features
- [ ] Generate unique QR code per ticket
- [ ] HMAC signature prevents forgery
- [ ] PDF ticket with event details and QR
- [ ] Email tickets to attendees
- [ ] Mobile check-in scanner
- [ ] Prevent duplicate check-ins
- [ ] Show attendee info on successful scan
- [ ] Real-time check-in statistics

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
     │ POST /checkin   │                   │
     │────────────────>│ Validate Signature│
     │                 │ Find Ticket       │
     │                 │──────────────────>│
     │                 │ Check Status      │
     │                 │ - valid?          │
     │                 │ - not used?       │
     │                 │ - event matches?  │
     │                 │ Update checked_in │
     │                 │──────────────────>│
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

---

## API Endpoints

```
POST /api/checkin
Request: { qrCode: "ticket:uuid:v1:signature" }
Response: {
  success: true,
  attendeeName: "John Doe",
  ticketTier: "VIP",
  checkedInAt: "2026-03-15T19:05:00Z"
}

GET /api/events/{id}/checkin-stats
Response: {
  totalSold: 500,
  checkedIn: 423,
  percentage: 84.6,
  byTier: [
    { name: "VIP", sold: 50, checkedIn: 48 },
    { name: "GA", sold: 450, checkedIn: 375 }
  ]
}

GET /api/tickets/{id}/pdf
Response: PDF file download
```

---

## Frontend Components

### Organizer Pages
- `/dashboard/events/[id]/checkin` - Scanner page

### Components
- `QrScanner` - Camera-based QR scanner
- `CheckinResult` - Success/failure display
- `CheckinStats` - Real-time statistics
- `TicketDisplay` - Ticket view for attendees

### Public Pages
- `/tickets/[id]` - View/download ticket

---

## Implementation Steps

### 1. QR Code Service (1h)
- [ ] IQrCodeService interface
- [ ] QR code string generation
- [ ] HMAC signature creation
- [ ] Signature validation
- [ ] Use QRCoder library for image generation

### 2. Check-in Service (1.5h)
- [ ] ICheckinService interface
- [ ] Parse and validate QR code
- [ ] Check ticket status and event
- [ ] Prevent duplicate check-ins
- [ ] Update checked_in_at timestamp
- [ ] Return attendee details

### 3. Check-in Endpoint (0.5h)
- [ ] POST /api/checkin
- [ ] GET /api/events/{id}/checkins
- [ ] GET /api/events/{id}/checkin-stats
- [ ] Add organizer authorization

### 4. PDF Ticket Service (1.5h)
- [ ] ITicketPdfService interface
- [ ] Use QuestPDF for generation
- [ ] Include event details and QR code
- [ ] Add download endpoint
- [ ] Cache generated PDFs

### 5. Scanner UI (1.5h)
- [ ] Scanner page for organizers
- [ ] Use html5-qrcode or react-qr-reader
- [ ] Show scan result with visual feedback
- [ ] Display attendee name and ticket type
- [ ] Show check-in count and stats
- [ ] Handle errors (invalid, already used)

---

## Success Criteria

- [ ] QR codes generate correctly
- [ ] Check-in validates and records
- [ ] Duplicate scans rejected
- [ ] PDF tickets download correctly
- [ ] Scanner works on mobile
- [ ] Stats update in real-time

---

## Security Considerations

- HMAC signature prevents ticket forgery
- Rate limit check-in endpoint
- Log all check-in attempts
- Only event organizers can scan
- Validate event ID matches ticket

---

## Offline Mode (Future Enhancement)

1. Pre-download ticket list before event
2. Store locally with IndexedDB
3. Validate offline using cached data
4. Queue check-ins for sync
5. Sync when connectivity restored

---

## Next Phase

After completion, proceed to **[Phase 09: Analytics Dashboard](phase-09-summary.md)**
