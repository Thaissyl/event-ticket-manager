# SePay Payment Integration Research Report

**Date:** 2026-02-02
**Context:** Vietnamese payment gateway integration for event ticket management
**Focus:** SePay + VietQR bank transfer flow

---

## 1. SePay API Overview

### Base Configuration
- **Base URL:** `https://my.sepay.vn/userapi/`
- **Authentication:** API Token (Bearer token)
- **Rate Limit:** 2 calls/second
- **Rate Limit Response:** HTTP 429 with `x-sepay-userapi-retry-after` header (seconds to wait)
- **Sandbox:** `my.dev.sepay.vn` for testing without real transactions

### Key Endpoints

**Transaction API:**
- `GET /transactions/list` - Query transactions with filters
- `GET /transactions/details/{id}` - Single transaction details
- `GET /transactions/count` - Count transactions

**Bank Account API:**
- `GET /bankaccounts/list` - List linked bank accounts
- `GET /bankaccounts/details/{id}` - Account details with balance
- `GET /bankaccounts/count` - Count accounts

**Query Parameters:**
- `account_number` - Filter by bank account
- `transaction_date_min/max` - Date range (yyyy-mm-dd format)
- `since_id` - Pagination from specific ID
- `limit` - Max 5000 for transactions, default 100 for accounts
- `reference_number` - Filter by bank reference
- `amount_in/amount_out` - Filter by amount

---

## 2. VietQR Bank Transfer Flow

### Standard Flow
1. **Generate VietQR Code**
   - Service: VietQR.io Quicklink API
   - Format: `https://img.vietqr.io/image/{bank_code}-{account_number}-{template}.jpg`
   - Templates: compact, print, qr_only
   - Supports: 30+ Vietnamese banks (VCB, TPBank, MBBank, VietinBank, BIDV, ACB, OCB, etc.)

2. **Customer Scans QR**
   - Opens banking app via deeplink
   - Pre-filled: account number, amount, payment code
   - Customer confirms payment

3. **Bank Processes Transfer**
   - Real-time transfer via Open Banking APIs
   - Generates unique reference number

4. **SePay Detects Transaction**
   - Auto-detects via bank API integration
   - Extracts payment code from transaction content
   - Triggers webhook notification

### VietQR Integration Options
- **Public Quicklink** (Free): Static QR generation, 99.5% SLA
- **Registered** (Free): Custom QR design, 99.9% SLA
- **Verified** (Contact): Custom domain, payment automation, 99.95% SLA
- **payOS Service:** Auto-verification via Open Banking APIs

---

## 3. Webhook Handling for Payment Confirmations

### Configuration
**Setup via SePay Dashboard:**
- Name: Custom identifier
- Event Triggers: money_in, money_out, or both
- Conditions:
  - Filter by specific bank account(s)
  - Filter by virtual accounts (optional)
  - Skip if no payment code detected (optional)
- Target URL: Your endpoint
- Auth: OAuth2, API Key, or None
- Content-Type: `application/json`, `multipart/form-data`, or `application/x-www-form-urlencoded`

### Payload Structure
```json
{
    "id": 92704,
    "gateway": "Vietcombank",
    "transactionDate": "2023-03-25 14:02:37",
    "accountNumber": "0123499999",
    "code": null,
    "content": "chuyen tien mua iphone",
    "transferType": "in",
    "transferAmount": 2277000,
    "accumulated": 19077000,
    "subAccount": null,
    "referenceCode": "MBVCB.3278907687",
    "description": ""
}
```

**Critical Fields:**
- `id` - Unique transaction identifier (use for deduplication)
- `transferType` - "in" (incoming) or "out" (outgoing)
- `code` - Auto-detected payment code
- `referenceCode` - Bank's SMS reference code
- `transferAmount` - Transaction amount

### Response Requirements
**OAuth 2.0:** HTTP 201 + `{"success": true}`
**API Key/No Auth:** HTTP 200-201 + `{"success": true}`

### Retry Mechanism
- **Triggers:** Network failure or configured status codes
- **Schedule:** Fibonacci intervals (minutes) - 7 retries over 5 hours
- **Timeouts:** Connection 5s, Response 8s
- **Important:** NO auto-retry if connection succeeds but response fails
- **Manual Retry:** Available via dashboard (transaction details or webhook logs)

### Deduplication Strategy
**CRITICAL:** Implement idempotency using:
- Primary: `id` field (unique per transaction)
- Fallback: Composite key of `referenceCode` + `transferType` + `transferAmount`

---

## 4. Security Best Practices

### Authentication
- **API Token:** Store in environment variables, never commit to code
- **Webhook Auth:** Use API Key header `Authorization: Apikey YOUR_KEY`
- **OAuth2:** For advanced integrations requiring user authorization

### Data Protection
- **HTTPS Only:** All API calls and webhook endpoints must use TLS
- **Token Rotation:** Regenerate API tokens periodically
- **IP Whitelisting:** Configure allowed IPs in SePay dashboard (if available)
- **Webhook Verification:** Validate incoming webhook IPs against SePay's ranges

### Transaction Security
- **Idempotency:** Prevent duplicate payment processing via transaction ID
- **Amount Validation:** Always verify `transferAmount` matches order total
- **Payment Code Matching:** Validate `code` field matches order reference
- **Timeout Handling:** Set payment expiry (recommended: 15-30 minutes for QR codes)
- **Balance Verification:** Use `/bankaccounts/details` to verify funds received

### Error Handling
- **Rate Limiting:** Implement exponential backoff when receiving 429
- **Webhook Failures:** Log all failed webhooks for manual review
- **Transaction Reconciliation:** Daily batch check via `/transactions/list` API
- **Monitoring:** Track webhook delivery success rate

### Privacy
- **PCI Compliance:** SePay handles bank details, merchant never stores card/account data
- **Customer Data:** Only transaction metadata (amounts, codes) transmitted
- **Logging:** Sanitize logs to remove sensitive content from transaction descriptions

---

## 5. Error Handling & Refund Flows

### Common Error Scenarios

**API Errors:**
- **429 Too Many Requests:** Back off using `x-sepay-userapi-retry-after` header value
- **401 Unauthorized:** Token expired or invalid - regenerate API token
- **404 Not Found:** Invalid endpoint or resource ID
- **500 Server Error:** SePay service issue - retry with exponential backoff

**Webhook Failures:**
- **Connection Timeout:** Check firewall/network configs
- **Validation Failure:** Log payload, verify signature/auth
- **Processing Error:** Return success to SePay, process async via queue

**Payment Issues:**
- **Underpayment:** Customer pays less than required amount
- **Overpayment:** Customer pays more than required
- **Wrong Account:** Payment to incorrect merchant account
- **Missing Code:** Transaction content lacks payment reference
- **Duplicate Payment:** Same customer pays twice

### Refund Flow

**SePay Does NOT Provide Automated Refund API**

**Manual Refund Process:**
1. **Identify Refund Request:** Customer support ticket or cancellation
2. **Verify Original Transaction:** Query via `/transactions/details/{id}`
3. **Validate Refund Eligibility:**
   - Transaction exists and completed
   - Amount matches refund request
   - Within refund policy window
4. **Manual Bank Transfer:**
   - Use merchant's bank portal
   - Transfer to customer's account (from transaction content or customer record)
   - Reference original `referenceCode` in transfer description
5. **Record Refund:**
   - Create refund record in database
   - Link to original transaction ID
   - Update order status
6. **Notify Customer:** Send refund confirmation with timeline

### Partial Refund Handling
- Calculate refund amount (original - amount_to_refund)
- Follow same manual process
- Track multiple partial refunds per transaction

### Dispute Resolution
- **Data Export:** Use `/transactions/list` with date range for evidence
- **Transaction Proof:** Screenshot from SePay dashboard
- **Reference Code:** Use `referenceCode` for bank-level inquiries

### Reconciliation Strategy
**Daily Batch:**
```
GET /transactions/list?transaction_date_min=YYYY-MM-DD 00:00:00&transaction_date_max=YYYY-MM-DD 23:59:59
```
- Compare with internal order records
- Flag mismatches (missing, duplicate, amount discrepancies)
- Generate reconciliation report

**Monthly Audit:**
- Use `/bankaccounts/details` to verify final balance
- Cross-check with accounting records

---

## Integration Architecture Recommendations

### Payment Flow
1. **Order Creation:** Generate unique payment code (e.g., ORDER123456)
2. **QR Generation:** Create VietQR with code embedded
3. **Display QR:** Show to customer with 15-minute expiry
4. **Webhook Reception:** Receive transaction notification
5. **Validation:** Verify code, amount, account
6. **Order Fulfillment:** Mark paid, send tickets
7. **Fallback Check:** Cron job polls `/transactions/list` for missed webhooks

### Database Schema Considerations
```sql
-- Store webhook payloads for audit
CREATE TABLE sepay_webhooks (
  id SERIAL PRIMARY KEY,
  sepay_transaction_id BIGINT UNIQUE,
  payload JSONB,
  processed BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP
);

-- Link to orders
CREATE TABLE order_payments (
  id SERIAL PRIMARY KEY,
  order_id INT REFERENCES orders(id),
  sepay_transaction_id BIGINT,
  amount DECIMAL(12,2),
  status VARCHAR(50),
  reference_code VARCHAR(255)
);
```

### Tech Stack Suggestions
- **Queue System:** Use Redis/Bull for async webhook processing
- **Retry Library:** Implement exponential backoff for API calls
- **Monitoring:** Sentry/LogRocket for error tracking
- **Cron Jobs:** Node-cron for reconciliation tasks

---

## Unresolved Questions

1. **SePay IP Whitelist:** Does SePay provide official webhook source IP ranges for firewall rules?
2. **Webhook Signature:** Is there HMAC/signature verification available for webhook payloads (not documented)?
3. **Virtual Accounts:** How to implement VA-based payment routing for multi-event scenarios?
4. **Transaction Limit:** What's the maximum transaction amount supported?
5. **Refund API ETA:** Is there a roadmap for automated refund/reversal endpoints?
6. **Real-time Balance:** Can we subscribe to real-time balance updates instead of polling?
7. **Multi-currency:** Does SePay support foreign currency transactions (USD, EUR)?

---

## References

- SePay Main: https://sepay.vn
- Developer Docs: https://developer.sepay.vn
- API Docs: https://docs.sepay.vn
- VietQR: https://vietqr.io
- Sandbox: https://my.dev.sepay.vn
