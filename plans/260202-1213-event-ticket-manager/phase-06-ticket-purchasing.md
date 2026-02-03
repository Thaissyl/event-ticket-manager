# Phase 06: Ticket Purchasing

## Context Links
- [Main Plan](plan.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)

## Overview
- **Priority:** P1 (Core feature)
- **Status:** pending
- **Effort:** 10h
- **Description:** Shopping cart with reservations, order creation, promo codes

## Key Insights
- Cart reservations prevent overselling (15-min timeout)
- Optimistic locking on inventory updates
- Guest checkout supported (no account required)
- Promo codes with percentage/fixed discounts
- Background job cleans expired reservations

## Requirements

### Functional
- F1: Add tickets to cart with quantity
- F2: Cart persists across sessions (cookie-based for guests)
- F3: 15-minute reservation timeout
- F4: Apply promo codes
- F5: Guest checkout with email
- F6: Order confirmation with ticket details

### Non-Functional
- NF1: Inventory accuracy (no overselling)
- NF2: Cart operations <100ms
- NF3: Reservation cleanup every minute

## Architecture

### Cart Reservation Flow

```
┌──────────┐     ┌───────────┐     ┌─────────────┐
│  User    │     │  Cart     │     │ TicketTier  │
│          │     │  Session  │     │ Inventory   │
└────┬─────┘     └─────┬─────┘     └──────┬──────┘
     │                 │                   │
     │ Add to Cart     │                   │
     │────────────────>│                   │
     │                 │                   │
     │                 │ Check Availability│
     │                 │──────────────────>│
     │                 │                   │
     │                 │ Reserve (Optimistic Lock)
     │                 │──────────────────>│
     │                 │                   │
     │                 │ Create CartReservation
     │                 │ (expires in 15 min)
     │                 │                   │
     │  Cart Updated   │                   │
     │<────────────────│                   │
     │                 │                   │
     │                 │ ... 15 min pass...│
     │                 │                   │
     │                 │ Background Job    │
     │                 │ Cleanup Expired   │
     │                 │──────────────────>│
     │                 │                   │
     │                 │ Release Reserved  │
     │                 │ Qty back to pool  │
```

### Inventory Calculation

```sql
-- Available tickets = Total - Sold - Reserved
available = quantity_total - quantity_sold - (
  SELECT COALESCE(SUM(quantity), 0)
  FROM cart_reservations
  WHERE ticket_tier_id = ? AND expires_at > NOW()
)
```

### Optimistic Locking Pattern

```csharp
public async Task<bool> ReserveTickets(Guid tierId, int qty)
{
    var tier = await _db.TicketTiers.FindAsync(tierId);
    var available = tier.QuantityTotal - tier.QuantitySold - tier.QuantityReserved;

    if (qty > available) return false;

    tier.QuantityReserved += qty;

    try {
        await _db.SaveChangesAsync(); // RowVersion check
        return true;
    } catch (DbUpdateConcurrencyException) {
        // Retry or return false
        return false;
    }
}
```

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/CartEndpoints.cs`
- `src/backend/EventTickets.API/Endpoints/OrderEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Cart/AddToCartRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Cart/CartResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Cart/CartItemResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Orders/CreateOrderRequest.cs`
- `src/backend/EventTickets.Core/DTOs/Orders/OrderResponse.cs`
- `src/backend/EventTickets.Core/DTOs/PromoCodes/ApplyPromoRequest.cs`
- `src/backend/EventTickets.Core/Services/ICartService.cs`
- `src/backend/EventTickets.Core/Services/IOrderService.cs`
- `src/backend/EventTickets.Core/Services/IPromoCodeService.cs`
- `src/backend/EventTickets.Infrastructure/Services/CartService.cs`
- `src/backend/EventTickets.Infrastructure/Services/OrderService.cs`
- `src/backend/EventTickets.Infrastructure/Services/PromoCodeService.cs`
- `src/backend/EventTickets.Infrastructure/BackgroundServices/CartCleanupService.cs`

### Create (Frontend)
- `src/frontend/app/(public)/cart/page.tsx`
- `src/frontend/app/(public)/checkout/page.tsx`
- `src/frontend/app/(public)/orders/[id]/page.tsx` - Order confirmation
- `src/frontend/components/cart/cart-drawer.tsx`
- `src/frontend/components/cart/cart-item.tsx`
- `src/frontend/components/cart/cart-summary.tsx`
- `src/frontend/components/checkout/checkout-form.tsx`
- `src/frontend/components/checkout/promo-code-input.tsx`
- `src/frontend/components/checkout/order-summary.tsx`
- `src/frontend/lib/cart-context.tsx` - Cart state management
- `src/frontend/hooks/use-cart.ts`

## Implementation Steps

### 1. Cart Service Implementation (2.5h)
- [ ] Create `ICartService` interface
- [ ] Implement cart operations with session tracking
- [ ] Implement reservation creation with optimistic locking
- [ ] Add inventory availability checks
- [ ] Handle concurrent access scenarios
- [ ] Create cart cleanup background service

### 2. Order Service Implementation (2h)
- [ ] Create `IOrderService` interface
- [ ] Convert cart to order on checkout
- [ ] Generate unique payment code for SePay
- [ ] Create tickets for order items
- [ ] Calculate totals with promo discounts
- [ ] Handle guest checkout

### 3. Promo Code Service (1h)
- [ ] Create `IPromoCodeService` interface
- [ ] Validate promo code eligibility
- [ ] Calculate discount amount
- [ ] Track promo code usage

### 4. Cart API Endpoints (1.5h)
- [ ] `GET /api/cart` - Get current cart
- [ ] `POST /api/cart/items` - Add item
- [ ] `PUT /api/cart/items/{id}` - Update quantity
- [ ] `DELETE /api/cart/items/{id}` - Remove item
- [ ] `POST /api/cart/promo` - Apply promo code
- [ ] `DELETE /api/cart` - Clear cart

### 5. Order API Endpoints (1h)
- [ ] `POST /api/orders` - Create order from cart
- [ ] `GET /api/orders` - User's orders
- [ ] `GET /api/orders/{id}` - Order details

### 6. Frontend Cart UI (2h)
- [ ] Create cart context with localStorage sync
- [ ] Create cart drawer component
- [ ] Add to cart from event details
- [ ] Show cart item count in header
- [ ] Handle reservation timer display
- [ ] Create checkout page
- [ ] Add promo code input
- [ ] Create order confirmation page

## Todo List
- [ ] Implement CartService with reservations
- [ ] Implement OrderService
- [ ] Implement PromoCodeService
- [ ] Create cart cleanup background job
- [ ] Create cart API endpoints
- [ ] Create order API endpoints
- [ ] Build cart drawer component
- [ ] Build checkout page
- [ ] Add promo code functionality
- [ ] Create order confirmation page
- [ ] Test race conditions

## Success Criteria
- [ ] Can add tickets to cart
- [ ] Reservations expire after 15 minutes
- [ ] Cannot oversell tickets
- [ ] Promo codes apply correctly
- [ ] Guest checkout works
- [ ] Order created with pending payment

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Race condition oversell | Medium | High | Optimistic locking + retries |
| Cart session loss | Low | Medium | Persist to DB for logged users |
| Cleanup job failure | Low | Medium | Monitor, manual recovery |

## Security Considerations
- Validate quantities (positive, within limits)
- Rate limit cart operations
- Validate promo codes on server
- Sanitize guest email input
- Prevent cart tampering (signed session)

## Next Steps
After completion, proceed to [Phase 07: SePay Payment](phase-07-sepay-payment.md)
