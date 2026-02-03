# Phase 07: SePay Payment Integration

## Context Links
- [Main Plan](plan.md)
- [SePay Integration Report](../reports/researcher-260202-1222-sepay-integration.md)

## Overview
- **Priority:** P1 (Core feature)
- **Status:** pending
- **Effort:** 8h
- **Description:** VietQR payment with SePay webhook processing

## Key Insights
- VietQR generates bank transfer QR code
- SePay detects payment via bank API
- Webhook notifies of successful payment
- Payment code embedded in transfer description
- Manual refunds via bank portal (no API)

## Requirements

### Functional
- F1: Generate VietQR code with order amount
- F2: Payment code embedded for matching
- F3: Webhook receives payment confirmation
- F4: Order marked paid, tickets activated
- F5: Payment timeout (30 minutes)
- F6: Fallback polling for missed webhooks

### Non-Functional
- NF1: Webhook response <2s
- NF2: Idempotent payment processing
- NF3: Audit trail for all transactions

## Architecture

### Payment Flow

```
┌──────────┐    ┌───────────┐    ┌─────────┐    ┌───────────┐
│  User    │    │  Next.js  │    │  API    │    │  SePay    │
└────┬─────┘    └─────┬─────┘    └────┬────┘    └─────┬─────┘
     │                │               │               │
     │ Checkout       │               │               │
     │───────────────>│               │               │
     │                │ Create Order  │               │
     │                │──────────────>│               │
     │                │               │               │
     │                │ Order + Code  │               │
     │                │<──────────────│               │
     │                │               │               │
     │  VietQR Code   │               │               │
     │<───────────────│               │               │
     │                │               │               │
     │ Scan & Pay     │               │               │
     │─────────────────────────────────────────────────>
     │ (Banking App)  │               │               │
     │                │               │               │
     │                │               │   Webhook     │
     │                │               │<──────────────│
     │                │               │               │
     │                │               │ Process &     │
     │                │               │ Update Order  │
     │                │               │               │
     │                │ Order Paid    │               │
     │<───────────────│<──────────────│               │
     │ (via polling   │               │               │
     │  or redirect)  │               │               │
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
  "code": "ORD-ABC123",
  "content": "Thanh toan ORD-ABC123",
  "transferType": "in",
  "transferAmount": 500000,
  "referenceCode": "MBVCB.3278907687"
}
```

## Related Code Files

### Create (Backend)
- `src/backend/EventTickets.API/Endpoints/PaymentEndpoints.cs`
- `src/backend/EventTickets.Core/DTOs/Payments/VietQrResponse.cs`
- `src/backend/EventTickets.Core/DTOs/Payments/SePayWebhookPayload.cs`
- `src/backend/EventTickets.Core/DTOs/Payments/PaymentStatusResponse.cs`
- `src/backend/EventTickets.Core/Services/IPaymentService.cs`
- `src/backend/EventTickets.Core/Services/IVietQrService.cs`
- `src/backend/EventTickets.Infrastructure/Services/PaymentService.cs`
- `src/backend/EventTickets.Infrastructure/Services/VietQrService.cs`
- `src/backend/EventTickets.Infrastructure/Services/SePayWebhookService.cs`
- `src/backend/EventTickets.Infrastructure/BackgroundServices/PaymentTimeoutService.cs`
- `src/backend/EventTickets.Infrastructure/BackgroundServices/PaymentReconciliationService.cs`

### Create (Frontend)
- `src/frontend/app/(public)/payment/[orderId]/page.tsx`
- `src/frontend/components/payment/vietqr-display.tsx`
- `src/frontend/components/payment/payment-status.tsx`
- `src/frontend/components/payment/payment-timer.tsx`
- `src/frontend/hooks/use-payment-status.ts`

## Implementation Steps

### 1. VietQR Service (1.5h)
- [ ] Create `IVietQrService` interface
- [ ] Generate VietQR URL with parameters
- [ ] Configure bank account details from settings
- [ ] Generate unique payment codes (format: `ETM-{timestamp}-{random}`)

### 2. Payment Service (2h)
- [ ] Create `IPaymentService` interface
- [ ] Create payment record for order
- [ ] Process webhook payload
- [ ] Implement idempotency (check transaction ID)
- [ ] Update order and ticket status
- [ ] Send confirmation email (integrate later)

### 3. Webhook Endpoint (1.5h)
- [ ] Create `POST /api/payments/sepay/webhook`
- [ ] Validate request (API key header)
- [ ] Parse webhook payload
- [ ] Log to SePayWebhooks table
- [ ] Process asynchronously via queue
- [ ] Return success response quickly

### 4. Background Services (1.5h)
- [ ] Create `PaymentTimeoutService`
  - Runs every minute
  - Cancels orders pending >30 minutes
  - Releases ticket reservations
- [ ] Create `PaymentReconciliationService`
  - Runs every 15 minutes
  - Polls SePay API for missed transactions
  - Matches by payment code

### 5. Payment UI (1.5h)
- [ ] Create payment page with VietQR display
- [ ] Show countdown timer (30 min)
- [ ] Poll for payment status
- [ ] Show success/failure states
- [ ] Redirect to order confirmation

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

## Todo List
- [ ] Implement VietQrService
- [ ] Implement PaymentService
- [ ] Create webhook endpoint
- [ ] Add idempotency checks
- [ ] Create payment timeout job
- [ ] Create reconciliation job
- [ ] Build payment UI
- [ ] Add status polling
- [ ] Test with SePay sandbox
- [ ] Add webhook logging

## Success Criteria
- [ ] VietQR code displays with correct amount
- [ ] Webhook processes payment correctly
- [ ] Order marked as paid
- [ ] Tickets status changed to valid
- [ ] Duplicate webhooks handled
- [ ] Timeout cancels unpaid orders

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Webhook delivery failure | Medium | High | Reconciliation job |
| Duplicate payments | Low | Medium | Idempotency by transaction ID |
| Amount mismatch | Low | High | Validate amount matches order |
| SePay downtime | Low | High | Show manual payment option |

## Security Considerations
- Validate webhook API key
- Log all webhook payloads (redact sensitive)
- Verify payment amount matches order
- Use HTTPS for all API calls
- Store API tokens in environment variables
- Rate limit webhook endpoint

## Refund Process (Manual)
1. Customer requests refund via support
2. Verify order and payment status
3. Process refund via bank portal
4. Update order status to refunded
5. Update ticket status to refunded
6. Notify customer

## Next Steps
After completion, proceed to [Phase 08: QR Tickets & Check-in](phase-08-qr-tickets-checkin.md)
