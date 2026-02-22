# Phase 09: Analytics Dashboard - Summary

**Status:** ⏳ PENDING
**Effort:** 8h
**Priority:** P2 (Value-add for organizers)

---

## What Is Planned

### Analytics Features
- [ ] Revenue and ticket sales by event
- [ ] Sales by tier breakdown
- [ ] Daily/weekly sales trends (time-series)
- [ ] Check-in progress on event day
- [ ] Promo code effectiveness
- [ ] CSV export for attendees and sales reports
- [ ] Real-time data updates

### Dashboard Layout

```
┌─────────────────────────────────────────────────────────┐
│  Event: Summer Music Festival 2026                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐    │
│  │ Revenue │  │ Tickets │  │ Orders  │ │Check-in │    │
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

---

## API Endpoints

```
GET /api/analytics/events/{id}/summary
Response: {
  totalRevenue: 25000.00,
  ticketsSold: 500,
  ordersCount: 320,
  checkedInCount: 425,
  checkedInPercentage: 85.0,
  averageOrderValue: 78.13
}

GET /api/analytics/events/{id}/sales-trend?days=30
Response: {
  data: [
    { date: "2026-02-01", ticketsSold: 45, revenue: 2250.00 },
    { date: "2026-02-02", ticketsSold: 62, revenue: 3100.00 }
  ]
}

GET /api/analytics/events/{id}/tier-breakdown
Response: {
  tiers: [
    { id: "uuid", name: "VIP", sold: 50, total: 100, revenue: 5000.00 },
    { id: "uuid", name: "GA", sold: 450, total: 900, revenue: 20000.00 }
  ]
}

GET /api/analytics/events/{id}/promo-stats
Response: {
  codes: [
    { code: "EARLY20", discount: 4000.00, uses: 25 },
    { code: "VIP10", discount: 1500.00, uses: 10 }
  ]
}

GET /api/analytics/events/{id}/recent-orders
Response: {
  orders: [...]
}

GET /api/analytics/events/{id}/export/attendees
Response: CSV file download

GET /api/analytics/events/{id}/export/sales
Response: CSV file download
```

---

## Frontend Components

### Pages
- `/dashboard/events/[id]/analytics` - Analytics dashboard

### Components
- `StatCard` - Metric display cards
- `SalesChart` - Line chart (Recharts)
- `TierPieChart` - Tier breakdown
- `CheckinProgress` - Check-in percentage
- `RecentOrdersTable` - Latest orders
- `PromoStats` - Promo code effectiveness
- `ExportButtons` - CSV export
- `DateRangeSelector` - Filter by date

### Hooks
- `useAnalytics` - Fetch analytics data

---

## Implementation Steps

### 1. Analytics Service (2h)
- [ ] IAnalyticsService interface
- [ ] Event summary calculation
- [ ] Daily sales aggregation
- [ ] Tier breakdown
- [ ] Promo code stats
- [ ] Caching layer (Redis or memory)

### 2. Analytics Endpoints (1.5h)
- [ ] GET /api/analytics/events/{id}/summary
- [ ] GET /api/analytics/events/{id}/sales-trend
- [ ] GET /api/analytics/events/{id}/tier-breakdown
- [ ] GET /api/analytics/events/{id}/promo-stats
- [ ] GET /api/analytics/events/{id}/recent-orders
- [ ] Add organizer authorization

### 3. Export Service (1h)
- [ ] IExportService interface
- [ ] CSV attendee list export
- [ ] Sales report export
- [ ] GDPR-compliant data handling
- [ ] Stream large exports

### 4. Export Endpoints (0.5h)
- [ ] GET /api/analytics/events/{id}/export/attendees
- [ ] GET /api/analytics/events/{id}/export/sales
- [ ] Return CSV with proper headers

### 5. Dashboard UI (3h)
- [ ] Stat card components
- [ ] Integrate Recharts for line charts
- [ ] Create pie chart for tier breakdown
- [ ] Build recent orders table
- [ ] Add date range selector
- [ ] Implement auto-refresh
- [ ] Add export buttons with download

---

## Success Criteria

- [ ] Dashboard shows accurate metrics
- [ ] Charts render correctly
- [ ] Data refreshes automatically
- [ ] CSV exports download correctly
- [ ] Performance within limits (<2s load)
- [ ] Only event owners see data

---

## Security Considerations

- Authorize analytics to event owners only
- Sanitize exported data
- Rate limit export endpoints
- Log data access for audit
- Mask sensitive attendee data in logs

---

## Next Phase

After completion, proceed to **[Phase 10: Admin Panel](phase-10-summary.md)**
