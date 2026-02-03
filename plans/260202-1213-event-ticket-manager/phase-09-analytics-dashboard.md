# Phase 09: Analytics Dashboard

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)

## Overview
- **Priority:** P2 (Value-add for organizers)
- **Status:** pending
- **Effort:** 8h
- **Description:** Sales analytics, attendance tracking, data export

## Key Insights
- Organizers need sales insights per event
- Time-series data for trend analysis
- Real-time updates during sales periods
- CSV export for external analysis
- Caching for expensive queries

## Requirements

### Functional
- F1: Revenue and ticket sales by event
- F2: Sales by tier breakdown
- F3: Daily/weekly sales trends
- F4: Check-in progress on event day
- F5: Promo code effectiveness
- F6: CSV export of attendees

### Non-Functional
- NF1: Dashboard loads <2s
- NF2: Charts render smoothly
- NF3: Data updates every minute

## Architecture

### Analytics Data Model

```sql
-- Precomputed daily sales (materialized or table)
CREATE TABLE daily_sales (
  event_id UUID,
  date DATE,
  tier_id UUID,
  tickets_sold INT,
  revenue DECIMAL(12,2),
  PRIMARY KEY (event_id, date, tier_id)
);

-- Or compute on-the-fly with query:
SELECT
  DATE(o.created_at) as date,
  tt.id as tier_id,
  tt.name as tier_name,
  COUNT(t.id) as tickets_sold,
  SUM(tt.price) as revenue
FROM tickets t
JOIN ticket_tiers tt ON t.ticket_tier_id = tt.id
JOIN orders o ON t.order_id = o.id
WHERE tt.event_id = @eventId
  AND o.status = 'Paid'
GROUP BY DATE(o.created_at), tt.id, tt.name
ORDER BY date;
```

### Dashboard Components

```
┌─────────────────────────────────────────────────────────┐
│  Event: Summer Music Festival 2026                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐    │
│  │ Revenue │  │ Tickets │  │ Orders  │  │Check-in │    │
│  │ $25,000 │  │   500   │  │   320   │  │  85%    │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘    │
│                                                         │
├─────────────────────────────────────────────────────────┤
│  Sales Over Time                    │ Sales by Tier     │
│  ┌────────────────────────────┐    │ ┌──────────────┐  │
│  │         Line Chart          │    │ │  Pie Chart   │  │
│  │    /\      /\               │    │ │              │  │
│  │   /  \    /  \     /\       │    │ │  VIP: 10%    │  │
│  │  /    \  /    \   /  \      │    │ │  GA: 90%     │  │
│  │ /      \/      \_/    \     │    │ │              │  │
│  └────────────────────────────┘    │ └──────────────┘  │
│                                     │                   │
├─────────────────────────────────────────────────────────┤
│  Recent Orders                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Order      │ Customer      │ Amount  │ Status   │   │
│  ├─────────────────────────────────────────────────┤   │
│  │ ETM-001    │ john@...      │ $150    │ Paid     │   │
│  │ ETM-002    │ jane@...      │ $75     │ Paid     │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  [Export Attendees CSV]  [Export Sales Report]          │
└─────────────────────────────────────────────────────────┘
```

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/AnalyticsEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Analytics/EventSummaryResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Analytics/SalesTrendResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Analytics/TierBreakdownResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Analytics/PromoCodeStatsResponse.cs`
- `src/backend/EventTickets.Core/Services/IAnalyticsService.cs`
- `src/backend/EventTickets.Infrastructure/Services/AnalyticsService.cs`
- `src/backend/EventTickets.Infrastructure/Services/ExportService.cs`

### Create (Frontend)
- `src/frontend/app/(dashboard)/dashboard/events/[id]/analytics/page.tsx`
- `src/frontend/components/analytics/stat-card.tsx`
- `src/frontend/components/analytics/sales-chart.tsx`
- `src/frontend/components/analytics/tier-pie-chart.tsx`
- `src/frontend/components/analytics/checkin-progress.tsx`
- `src/frontend/components/analytics/recent-orders-table.tsx`
- `src/frontend/components/analytics/promo-stats.tsx`
- `src/frontend/components/analytics/export-buttons.tsx`
- `src/frontend/hooks/use-analytics.ts`

## Implementation Steps

### 1. Analytics Service (2h)
- [ ] Create `IAnalyticsService` interface
- [ ] Implement event summary calculation
- [ ] Implement daily sales aggregation
- [ ] Implement tier breakdown
- [ ] Add promo code stats
- [ ] Add caching layer (Redis or memory)

### 2. Analytics Endpoints (1.5h)
- [ ] `GET /api/analytics/events/{id}/summary` - Key metrics
- [ ] `GET /api/analytics/events/{id}/sales-trend` - Daily sales
- [ ] `GET /api/analytics/events/{id}/tier-breakdown` - By tier
- [ ] `GET /api/analytics/events/{id}/promo-stats` - Promo codes
- [ ] `GET /api/analytics/events/{id}/recent-orders` - Latest orders
- [ ] Add organizer authorization

### 3. Export Service (1h)
- [ ] Create `IExportService` interface
- [ ] Implement CSV attendee list export
- [ ] Implement sales report export
- [ ] Add GDPR-compliant data handling
- [ ] Stream large exports

### 4. Export Endpoints (0.5h)
- [ ] `GET /api/analytics/events/{id}/export/attendees`
- [ ] `GET /api/analytics/events/{id}/export/sales`
- [ ] Return CSV with proper headers

### 5. Dashboard UI (3h)
- [ ] Create stat card components
- [ ] Integrate Recharts for line charts
- [ ] Create pie chart for tier breakdown
- [ ] Build recent orders table
- [ ] Add date range selector
- [ ] Implement auto-refresh
- [ ] Add export buttons with download

## API Response Examples

```json
// GET /api/analytics/events/{id}/summary
{
  "totalRevenue": 25000.00,
  "ticketsSold": 500,
  "ordersCount": 320,
  "checkedInCount": 425,
  "checkedInPercentage": 85.0,
  "averageOrderValue": 78.13
}

// GET /api/analytics/events/{id}/sales-trend?days=30
{
  "data": [
    { "date": "2026-02-01", "ticketsSold": 45, "revenue": 2250.00 },
    { "date": "2026-02-02", "ticketsSold": 62, "revenue": 3100.00 }
  ]
}

// GET /api/analytics/events/{id}/tier-breakdown
{
  "tiers": [
    { "id": "uuid", "name": "VIP", "sold": 50, "total": 100, "revenue": 5000.00 },
    { "id": "uuid", "name": "GA", "sold": 450, "total": 900, "revenue": 20000.00 }
  ]
}
```

## Todo List
- [ ] Implement AnalyticsService
- [ ] Create analytics endpoints
- [ ] Implement export service
- [ ] Create export endpoints
- [ ] Build stat cards
- [ ] Create sales trend chart
- [ ] Create tier breakdown chart
- [ ] Build recent orders table
- [ ] Add export functionality
- [ ] Test with sample data

## Success Criteria
- [ ] Dashboard shows accurate metrics
- [ ] Charts render correctly
- [ ] Data refreshes automatically
- [ ] CSV exports download correctly
- [ ] Performance within limits
- [ ] Only event owners see data

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Slow queries | Medium | Medium | Indexes, caching |
| Large exports timeout | Low | Medium | Streaming, pagination |
| Chart rendering issues | Low | Low | Test various data sizes |

## Security Considerations
- Authorize analytics to event owners only
- Sanitize exported data
- Rate limit export endpoints
- Log data access for audit
- Mask sensitive attendee data in logs

## Next Steps
After completion, proceed to [Phase 10: Admin Panel](phase-10-admin-panel.md)
