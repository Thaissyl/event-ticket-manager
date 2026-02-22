# Phase 06: Ticket Purchasing - Summary

**Status:** ⏳ PENDING
**Effort:** 10h
**Priority:** P1 (Core feature)

---

## What Is Planned

### Shopping Cart Features
- [ ] Add tickets to cart with quantity selection
- [ ] Cart persists across sessions (cookie-based for guests)
- [ ] 15-minute reservation timeout
- [ ] Apply promo codes
- [ ] Guest checkout with email
- [ ] Order confirmation with ticket details
- [ ] Background cleanup of expired reservations

### Cart Reservation Flow

```
┌──────────┐     ┌───────────┐     ┌─────────────┐
│  User    │     │  Cart     │     │ TicketTier  │
│          │     │  Session  │     │ Inventory   │
└────┬─────┘     └─────┬─────┘     └──────┬──────┘
     │                 │                   │
     │ Add to Cart     │                   │
     │────────────────>│                   │
     │                 │ Check Availability│
     │                 │──────────────────>│
     │                 │ Reserve (Optimistic Lock)
     │                 │──────────────────>│
     │                 │ Create CartReservation
     │                 │ (expires in 15 min)
     │ Cart Updated    │                   │
     │<────────────────│                   │
     │                 │ ... 15 min pass...│
     │                 │ Background Job    │
     │                 │ Cleanup Expired   │
     │                 │──────────────────>│
     │                 │ Release Reserved  │
     │                 │ Qty back to pool  │
```

### Inventory Calculation

```sql
-- Available = Total - Sold - Reserved
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
        return false; // Retry or fail
    }
}
```

---

## API Endpoints

```
GET    /api/cart                   # Get current cart
POST   /api/cart/items             # Add item
PUT    /api/cart/items/{id}        # Update quantity
DELETE /api/cart/items/{id}        # Remove item
POST   /api/cart/promo             # Apply promo code
DELETE /api/cart                   # Clear cart

POST   /api/orders                 # Create from cart
GET    /api/orders                 # User's orders
GET    /api/orders/{id}            # Order details
```

### Request/Response Examples

```csharp
// Add to Cart
public record AddToCartRequest(
    Guid TicketTierId,
    int Quantity
);

// Apply Promo
public record ApplyPromoRequest(
    string Code
);

// Create Order
public record CreateOrderRequest(
    string GuestEmail,
    string GuestName,
    string? PromoCode
);

// Cart Response
public record CartResponse(
    List<CartItemResponse> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    string? PromoCode,
    string? PromoError
);

// Order Response
public record OrderResponse(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    string PaymentCode,
    DateTime CreatedAt,
    List<TicketResponse> Tickets
);
```

---

## Frontend Components

### Pages
- `/cart` - Shopping cart page
- `/checkout` - Checkout form
- `/orders/[id]` - Order confirmation

### Components
- `CartDrawer` - Slide-out cart
- `CartItem` - Individual cart item
- `CartSummary` - Totals and checkout button
- `CheckoutForm` - Guest details form
- `PromoCodeInput` - Promo code input
- `OrderSummary` - Order review

### State Management
- `CartContext` - Cart state with localStorage sync
- `useCart` hook - Cart operations

---

## Implementation Steps

### 1. Cart Service Implementation (2.5h)
- [ ] ICartService interface
- [ ] Cart operations with session tracking
- [ ] Reservation creation with optimistic locking
- [ ] Inventory availability checks
- [ ] Concurrent access handling
- [ ] Cart cleanup background service

### 2. Order Service Implementation (2h)
- [ ] IOrderService interface
- [ ] Convert cart to order
- [ ] Generate unique payment code (format: ETM-{timestamp}-{random})
- [ ] Create tickets for order items
- [ ] Calculate totals with promo discounts
- [ ] Handle guest checkout

### 3. Promo Code Service (1h)
- [ ] IPromoCodeService interface
- [ ] Validate promo eligibility
- [ ] Calculate discount amount
- [ ] Track promo usage

### 4. Cart API Endpoints (1.5h)
- [ ] GET /api/cart
- [ ] POST /api/cart/items
- [ ] PUT /api/cart/items/{id}
- [ ] DELETE /api/cart/items/{id}
- [ ] POST /api/cart/promo
- [ ] DELETE /api/cart

### 5. Order API Endpoints (1h)
- [ ] POST /api/orders
- [ ] GET /api/orders
- [ ] GET /api/orders/{id}

### 6. Frontend Cart UI (2h)
- [ ] Cart context with localStorage sync
- [ ] Cart drawer component
- [ ] Add to cart from event details
- [ ] Cart item count in header
- [ ] Reservation timer display
- [ ] Checkout page
- [ ] Promo code input
- [ ] Order confirmation page

---

## Success Criteria

- [ ] Can add tickets to cart
- [ ] Reservations expire after 15 minutes
- [ ] Cannot oversell tickets
- [ ] Promo codes apply correctly
- [ ] Guest checkout works
- [ ] Order created with pending payment

---

## Security Considerations

- Validate quantities (positive, within limits)
- Rate limit cart operations
- Validate promo codes on server
- Sanitize guest email input
- Prevent cart tampering (signed session)

---

## Next Phase

After completion, proceed to **[Phase 07: SePay Payment](phase-07-summary.md)**
