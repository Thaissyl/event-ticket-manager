# Phase 07: SePay Payment Integration - Summary

**Status:** ⏳ PENDING
**Effort:** 8h
**Priority:** P1 (Core feature)

---

## What Is Planned

### VietQR Payment Features
- [ ] Generate VietQR code with order amount
- [ ] Payment code embedded in transfer description
- [ ] Webhook receives payment confirmation
- [ ] Order marked paid, tickets activated
- [ ] Payment timeout (30 minutes)
- [ ] Fallback polling for missed webhooks
- [ ] Idempotent payment processing

### Payment Flow

```
┌──────────┐    ┌───────────┐    ┌─────────┐    ┌───────────┐
│  User    │    │  Next.js  │    │  API    │    │  SePay    │
└────┬─────┘    └─────┬─────┘    └────┬────┘    └─────┬─────┘
     │                │               │               │
     │ Checkout       │               │               │
     │──────────────>│               │               │
     │                │ Create Order  │               │
     │                │──────────────>│               │
     │                │ Order + Code  │               │
     │                │<──────────────│               │
     │  VietQR Code   │               │               │
     │<───────────────│               │               │
     │ Scan & Pay     │               │               │
     │───────────────────────────────────────────────>│
     │                │               │   Webhook     │
     │                │               │<──────────────│
     │                │               │ Process &     │
     │                │               │ Update Order  │
     │ Order Paid     │               │               │
     │<───────────────│<──────────────│               │
```

### VietQR Code Format

```
https://img.vietqr.io/image/{bank_code}-{account_number}-compact.png
  ?amount={order_amount}
  &addInfo={payment_code}
  &accountName={merchant_name}
```

### Webhook Payload

```json
{
  "id": 92704,
  "gateway": "Vietcombank",
  "transactionDate": "2023-03-25 14:02:37",
  "accountNumber": "0123499999",
  "code": "ETM-260202-ABC123",
  "content": "Thanh toan ETM-260202-ABC123",
  "transferType": "in",
  "transferAmount": 500000,
  "referenceCode": "MBVCB.3278907687"
}
```

---

## Configuration

```json
// appsettings.json
{
  "SePay": {
    "ApiToken": "xxx",
    "WebhookApiKey": "xxx",
    "BankCode": "VCB",
    "AccountNumber": "0123456789",
    "AccountName": "EVENT TICKETS CO LTD",
    "BaseUrl": "https://my.sepay.vn/userapi/"
  },
  "VietQR": {
    "BaseUrl": "https://img.vietqr.io/image/"
  }
}
```

---

## API Endpoints

```
GET  /api/payments/{orderId}/vietqr    # Generate VietQR code
GET  /api/payments/{orderId}/status    # Check payment status
POST /api/payments/sepay/webhook       # SePay callback
```

### Response Examples

```csharp
// VietQR Response
public record VietQrResponse(
    string QrCodeUrl,
    string PaymentCode,
    decimal Amount,
    DateTime ExpiresAt
);

// Payment Status
public record PaymentStatusResponse(
    Guid OrderId,
    string Status,      // Pending, Paid, Cancelled, Expired
    decimal Amount,
    DateTime? PaidAt
);
```

---

## Frontend Components

### Pages
- `/payment/[orderId]` - Payment page with VietQR display

### Components
- `VietQrDisplay` - QR code with instructions
- `PaymentStatus` - Success/failure states
- `PaymentTimer` - 30-minute countdown
- `PaymentInstructions` - Bank transfer steps

### Hooks
- `usePaymentStatus` - Poll for payment updates

---

## Implementation Steps

### 1. VietQR Service (1.5h)
- [ ] IVietQrService interface
- [ ] Generate VietQR URL with parameters
- [ ] Configure bank account details
- [ ] Generate unique payment codes (ETM-{timestamp}-{random})

### 2. Payment Service (2h)
- [ ] IPaymentService interface
- [ ] Create payment record for order
- [ ] Process webhook payload
- [ ] Idempotency check (transaction ID)
- [ ] Update order and ticket status
- [ ] Send confirmation email

### 3. Webhook Endpoint (1.5h)
- [ ] POST /api/payments/sepay/webhook
- [ ] Validate API key header
- [ ] Parse webhook payload
- [ ] Log to SePayWebhooks table
- [ ] Process asynchronously via queue
- [ ] Return success quickly

### 4. Background Services (1.5h)
- [ ] PaymentTimeoutService (every minute)
    - Cancels orders pending >30 minutes
    - Releases ticket reservations
- [ ] PaymentReconciliationService (every 15 min)
    - Polls SePay API for missed transactions
    - Matches by payment code

### 5. Payment UI (1.5h)
- [ ] Payment page with VietQR display
- [ ] Countdown timer (30 min)
- [ ] Payment status polling
- [ ] Success/failure states
- [ ] Redirect to order confirmation

---

## Success Criteria

- [ ] VietQR code displays with correct amount
- [ ] Webhook processes payment correctly
- [ ] Order marked as paid
- [ ] Tickets status changed to valid
- [ ] Duplicate webhooks handled
- [ ] Timeout cancels unpaid orders

---

## Security Considerations

- Validate webhook API key
- Log all webhook payloads (redact sensitive)
- Verify payment amount matches order
- Use HTTPS for all API calls
- Store API tokens in environment variables
- Rate limit webhook endpoint

---

## Refund Process (Manual)

1. Customer requests refund via support
2. Verify order and payment status
3. Process refund via bank portal
4. Update order status to refunded
5. Update ticket status to refunded
6. Notify customer

---

## Next Phase

After completion, proceed to **[Phase 08: QR Tickets & Check-in](phase-08-summary.md)**
