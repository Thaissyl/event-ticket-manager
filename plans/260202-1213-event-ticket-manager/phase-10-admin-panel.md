# Phase 10: Admin Panel

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)

## Overview
- **Priority:** P2 (Platform management)
- **Status:** pending
- **Effort:** 6h
- **Description:** Platform admin for user, event, and transaction management

## Key Insights
- Admin role has full platform access
- Overview of platform metrics
- User management (roles, status)
- Event moderation capabilities
- Transaction monitoring

## Requirements

### Functional
- F1: Platform-wide statistics dashboard
- F2: User listing and role management
- F3: Event listing with moderation controls
- F4: Transaction/payment monitoring
- F5: Organizer verification/approval

### Non-Functional
- NF1: Admin pages load <2s
- NF2: Pagination for large lists
- NF3: Search across entities

## Architecture

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

### Admin Dashboard Layout

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

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/AdminEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Admin/PlatformStatsResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Admin/UserListResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Admin/UpdateUserRoleRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Admin/EventModerationRequest.cs`
- `src/backend/EventTickets.Core/Services/IAdminService.cs`
- `src/backend/EventTickets.Infrastructure/Services/AdminService.cs`

### Create (Frontend)
- `src/frontend/app/admin/layout.tsx`
- `src/frontend/app/admin/page.tsx` - Dashboard
- `src/frontend/app/admin/users/page.tsx`
- `src/frontend/app/admin/users/[id]/page.tsx`
- `src/frontend/app/admin/events/page.tsx`
- `src/frontend/app/admin/events/[id]/page.tsx`
- `src/frontend/app/admin/transactions/page.tsx`
- `src/frontend/app/admin/organizers/page.tsx`
- `src/frontend/components/admin/admin-sidebar.tsx`
- `src/frontend/components/admin/platform-stats.tsx`
- `src/frontend/components/admin/user-table.tsx`
- `src/frontend/components/admin/event-table.tsx`
- `src/frontend/components/admin/transaction-table.tsx`
- `src/frontend/components/admin/user-role-dropdown.tsx`

## Implementation Steps

### 1. Admin Service (1.5h)
- [ ] Create `IAdminService` interface
- [ ] Implement platform statistics
- [ ] Implement user listing with search
- [ ] Implement event listing with filters
- [ ] Implement transaction listing
- [ ] Add role update functionality

### 2. Admin Endpoints (1.5h)
- [ ] `GET /api/admin/stats` - Platform overview
- [ ] `GET /api/admin/users` - List users (paginated)
- [ ] `PUT /api/admin/users/{id}/role` - Change role
- [ ] `PUT /api/admin/users/{id}/status` - Enable/disable
- [ ] `GET /api/admin/events` - All events
- [ ] `PUT /api/admin/events/{id}/status` - Moderate
- [ ] `GET /api/admin/transactions` - Payments
- [ ] Add Admin role authorization

### 3. Admin Layout (0.5h)
- [ ] Create admin layout with sidebar
- [ ] Add navigation links
- [ ] Add role-based access check
- [ ] Redirect non-admins

### 4. Dashboard Page (1h)
- [ ] Create platform stats cards
- [ ] Add recent activity feed
- [ ] Add pending actions list
- [ ] Add quick action links

### 5. User Management UI (1h)
- [ ] Create user table with search
- [ ] Add role change dropdown
- [ ] Add status toggle
- [ ] Show user details page
- [ ] Add pagination

### 6. Event & Transaction Views (0.5h)
- [ ] Create event listing table
- [ ] Add moderation actions
- [ ] Create transaction listing
- [ ] Add filters and search

## API Endpoints

```
// Platform Statistics
GET /api/admin/stats
Response: {
  "users": { "total": 1234, "weeklyGrowth": 12 },
  "events": { "total": 456, "active": 89 },
  "orders": { "total": 5678, "weeklyCount": 123 },
  "revenue": { "total": 89012.50, "weeklyRevenue": 4500.00 }
}

// User Management
GET /api/admin/users?search=john&role=Organizer&page=1
Response: {
  "items": [
    { "id": "uuid", "email": "john@example.com", "role": "Organizer", "status": "Active" }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20
}

PUT /api/admin/users/{id}/role
{ "role": "Organizer" }

// Event Moderation
PUT /api/admin/events/{id}/status
{ "status": "Suspended", "reason": "Policy violation" }
```

## Todo List
- [ ] Implement AdminService
- [ ] Create admin endpoints
- [ ] Add admin authorization
- [ ] Create admin layout
- [ ] Build dashboard page
- [ ] Create user management UI
- [ ] Create event management UI
- [ ] Create transaction view
- [ ] Test all admin functions

## Success Criteria
- [ ] Platform stats display correctly
- [ ] User roles can be changed
- [ ] Events can be moderated
- [ ] Transactions visible
- [ ] Only admins can access
- [ ] Search and pagination work

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Unauthorized access | Low | Critical | Strict role checks |
| Data modification errors | Low | High | Audit logging |
| Performance with large data | Medium | Medium | Pagination, indexes |

## Security Considerations
- Require Admin role for all endpoints
- Log all admin actions with user ID
- Rate limit admin endpoints
- Two-factor auth for admin accounts (future)
- Session timeout for admin pages
- IP restriction option (future)

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

## Next Steps
After completion, proceed to [Phase 11: Testing & Deployment](phase-11-testing-deployment.md)
