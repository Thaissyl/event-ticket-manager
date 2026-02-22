# Phase 10: Admin Panel - Summary

**Status:** ⏳ PENDING
**Effort:** 6h
**Priority:** P2 (Platform management)

---

## What Is Planned

### Admin Panel Features
- [ ] Platform-wide statistics dashboard
- [ ] User management (roles, status)
- [ ] Event listing with moderation controls
- [ ] Transaction/payment monitoring
- [ ] Organizer verification/approval
- [ ] Audit logging for admin actions

### Admin Routes

```
/admin
├── /                    # Dashboard overview
├── /users              # User management
│   └── /[id]           # User details
├── /events             # All events
│   └── /[id]           # Event details + moderation
├── /transactions       # Payment transactions
├── /organizers         # Organizer applications
└── /settings           # Platform settings
```

### Dashboard Layout

```
┌──────────────────────────────────────────────────────────┐
│  Admin Dashboard                                          │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  Platform Overview                                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐     │
│  │  Users   │ │  Events  │ │  Orders  │ │ Revenue  │     │
│  │  1,234   │ │   456    │ │  5,678   │ │ $89,012  │     │
│  │ +12/week │ │ +5/week  │ │+123/week │ │+$4.5k/wk │     │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘     │
│                                                           │
├──────────────────────────────────────────────────────────┤
│  Recent Activity                │  Pending Actions        │
│  ┌───────────────────────┐     │ ┌───────────────────┐   │
│  │ User john@ registered │     │ │ 3 organizer apps  │   │
│  │ Event "..." published │     │ │ 2 flagged events  │   │
│  │ Order ETM-... paid    │     │ │ 1 refund request  │   │
│  └───────────────────────┘     │ └───────────────────┘   │
│                                 │                         │
├──────────────────────────────────────────────────────────┤
│  Quick Links                                              │
│  [Manage Users] [Review Events] [View Transactions]      │
└──────────────────────────────────────────────────────────┘
```

---

## API Endpoints

```
// Platform Statistics
GET /api/admin/stats
Response: {
  users: { total: 1234, weeklyGrowth: 12 },
  events: { total: 456, active: 89 },
  orders: { total: 5678, weeklyCount: 123 },
  revenue: { total: 89012.50, weeklyRevenue: 4500.00 }
}

// User Management
GET /api/admin/users?search=john&role=Organizer&page=1
Response: {
  items: [
    { id: "uuid", email: "john@example.com", role: "Organizer", status: "Active" }
  ],
  totalCount: 45,
  page: 1,
  pageSize: 20
}

PUT /api/admin/users/{id}/role
{ role: "Organizer" }

PUT /api/admin/users/{id}/status
{ status: "Suspended" }

// Event Moderation
GET /api/admin/events?status=Flagged&page=1

PUT /api/admin/events/{id}/status
{ status: "Suspended", reason: "Policy violation" }

// Transactions
GET /api/admin/transactions?dateFrom=2026-01-01&page=1
```

---

## Frontend Components

### Pages
- `/admin` - Dashboard overview
- `/admin/users` - User listing
- `/admin/users/[id]` - User details
- `/admin/events` - Event moderation
- `/admin/events/[id]` - Event details
- `/admin/transactions` - Transaction list

### Components
- `AdminSidebar` - Navigation
- `PlatformStats` - Overview cards
- `UserTable` - User listing with search
- `EventTable` - Event listing with filters
- `TransactionTable` - Payment listing
- `UserRoleDropdown` - Role change
- `StatusToggle` - Enable/disable users

---

## Implementation Steps

### 1. Admin Service (1.5h)
- [ ] IAdminService interface
- [ ] Platform statistics calculation
- [ ] User listing with search
- [ ] Event listing with filters
- [ ] Transaction listing
- [ ] Role update functionality

### 2. Admin Endpoints (1.5h)
- [ ] GET /api/admin/stats
- [ ] GET/PUT /api/admin/users
- [ ] PUT /api/admin/users/{id}/role
- [ ] PUT /api/admin/users/{id}/status
- [ ] GET/PUT /api/admin/events
- [ ] GET /api/admin/transactions
- [ ] Add Admin role authorization

### 3. Admin Layout (0.5h)
- [ ] Admin layout with sidebar
- [ ] Navigation links
- [ ] Role-based access check
- [ ] Redirect non-admins

### 4. Dashboard Page (1h)
- [ ] Platform stats cards
- [ ] Recent activity feed
- [ ] Pending actions list
- [ ] Quick action links

### 5. User Management UI (1h)
- [ ] User table with search
- [ ] Role change dropdown
- [ ] Status toggle
- [ ] User details page
- [ ] Pagination

### 6. Event & Transaction Views (0.5h)
- [ ] Event listing table
- [ ] Moderation actions
- [ ] Transaction listing
- [ ] Filters and search

---

## Success Criteria

- [ ] Platform stats display correctly
- [ ] User roles can be changed
- [ ] Events can be moderated
- [ ] Transactions visible
- [ ] Only admins can access
- [ ] Search and pagination work

---

## Security Considerations

- Require Admin role for all endpoints
- Log all admin actions with user ID
- Rate limit admin endpoints
- Two-factor auth for admin accounts (future)
- Session timeout for admin pages
- IP restriction option (future)

---

## Audit Logging

```csharp
public record AdminAuditLog(
    Guid Id,
    Guid AdminUserId,
    string Action,        // "ChangeRole", "SuspendEvent", etc.
    string TargetType,    // "User", "Event", etc.
    Guid TargetId,
    string Details,       // JSON of changes
    DateTime CreatedAt
);
```

---

## Next Phase

After completion, proceed to **[Phase 11: Testing & Deployment](phase-11-summary.md)**
