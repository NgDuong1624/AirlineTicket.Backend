# Backend Payment Processing Architecture & Flow

## 1. High-Level Lifecycle

The backend manages multi-gateway payment processing (`Stripe`, `PayPal`, `VNPay`, `MoMo`) via asynchronous webhooks, transactional outbox pattern, and idempotent state machines.

```
[Client App] ── 1. POST /api/payments/checkout ──► [AirlineTicket.Api]
                                                         │
                                               2. Create Payment (Pending)
                                               3. Generate Gateway Session / URL
                                                         │
                                                         ▼
[Payment Gateway] ◄── 4. Redirect / Embedded Auth ───────┘
       │
       ├──────────────────────────────┐
       │ (Success / Cancel Return)    │ (Server-to-Server IPN/Webhook)
       ▼                              ▼
[Client Return URL]          [POST /api/payments/webhooks/{provider}]
                                      │
                             1. Signature & Timestamp Validation
                             2. Save to bookings.webhook_events
                             3. Emit to bookings.outbox_messages
                             4. Fast Ack (200 OK < 200ms)
                                      │
                                      ▼
                        [PaymentOutboxProcessor Worker]
                                      │
                             1. Deduplicate & Replay Check
                             2. Transition Payment & Booking Status
                             3. Issue Tickets & Emit Domain Events
                             4. Push via SignalR BookingHub
```

---

## 2. Supported Currencies & Formatting Rules

Currencies map across the system's 6 supported locales:

| Currency | Code | Decimals | Gateway Smallest Unit Multiplier | Associated Locale |
| :--- | :--- | :--- | :--- | :--- |
| US Dollar | `USD` | 2 | `100` (cents) | `en-US` |
| Vietnamese Dong | `VND` | 0 | `1` (dong) | `vi-VN` |
| Chinese Yuan | `CNY` | 2 | `100` (fen) | `zh-CN` |
| Japanese Yen | `JPY` | 0 | `1` (yen) | `ja-JP` |
| South Korean Won | `KRW` | 0 | `1` (won) | `ko-KR` |
| Euro | `EUR` | 2 | `100` (cents) | `fr-FR` |

**Domain Value Object Rule (`Money`)**:
- Zero-decimal currencies (`VND`, `JPY`, `KRW`) reject fractional inputs (`amount % 1 == 0`).
- Conversion to gateway units (`ToGatewayUnits`) applies currency-specific unit scaling.

---

## 3. Core Domain Entities & State Machine

### A. `Payment` Entity (`bookings.payments`)
Tracks transaction progress with optimistic locking (`ConcurrencyVersion`):

- **Fields**:
  - `Id`: `Guid` (Primary Key).
  - `BookingId`: `Guid` (Foreign Key to `bookings.bookings`).
  - `Provider`: `PaymentProvider` (`Stripe`, `PayPal`, `VNPay`, `MoMo`).
  - `ProviderTransactionId`: `string` (Gateway payment intent/order/transaction ID).
  - `Amount`: `decimal`.
  - `Currency`: `Currency` (`USD`, `VND`, `CNY`, `JPY`, `KRW`, `EUR`).
  - `Status`: `PaymentTransactionStatus`.
  - `ProviderTimestamp`: `DateTime?` (Gateway event timestamp to drop out-of-order stale webhooks).
  - `ConcurrencyVersion`: `int`.
  - `RawResponse`: `string?` (Gateway audit payload).
  - `FailureReason`: `string?`.
  - `CreatedAt`: `DateTime`.
  - `UpdatedAt`: `DateTime?`.

### B. State Transition Rules
```
                ┌───────────────────────────────────────┐
                │                                       │
                ▼                                       │
            [Pending]                                   │
             │     │                                    │
             │     ▼                                    │
             │  [Processing]                            │
             │     │                                    │
             ├─────┼───────────────┬─────────────────┐  │
             ▼     ▼               ▼                 ▼  │
        [Succeeded]            [Failed]         [Cancelled]
         │        │
         ▼        ▼
  [RefundPending] [Refunded]
         │            ▲
         └────────────┘
```

- `Failed`, `Cancelled`, and `Refunded` are terminal states.
- Re-entrant transition to the identical state is a safe no-op.
- Out-of-order events with `providerTimestamp < Payment.ProviderTimestamp` are ignored.

---

## 4. Webhook Processing & Security

### A. Fast Acknowledgment & Ingestion Pipeline
1. **Raw Body Buffering**: `HttpRequest.EnableBuffering()` preserves raw UTF-8 bytes for HMAC validation.
2. **Replay Defense**: Enforces maximum 5-minute clock drift on event timestamp headers.
3. **Idempotency Gate**: Unique constraint on `(Provider, ProviderEventId)` in `bookings.webhook_events`. Duplicates return `200 OK` immediately.
4. **Outbox Emission**: Inserts `ProcessPaymentCallbackCommand` into `bookings.outbox_messages` in the same database transaction.

### B. Gateway Signature Verification Strategies

| Provider | Signature Verification Method | Payload Identifiers |
| :--- | :--- | :--- |
| **Stripe** | `Stripe-Signature` header (HMAC-SHA256 with timestamp) | `PaymentIntent` / `CheckoutSession` ID |
| **PayPal** | `PAYPAL-TRANSMISSION-SIG` + Cert URL + SHA256 payload digest | `capture_id` / `order_id` |
| **VNPay** | HMAC-SHA512 checksum of sorted query parameters (`vnp_SecureHash`) | `vnp_TxnRef`, `vnp_TransactionNo` |
| **MoMo** | HMAC-SHA256 signature of canonical key-value formatted string | `orderId`, `transId` |

---

## 5. Background Jobs & Reconciliation

- **`PaymentOutboxProcessor`**: Polls and dispatches pending callback commands from `bookings.outbox_messages` with Polly retry policies.
- **`PaymentReconciliationJob`**: Runs daily at 02:00 UTC. Queries transactions stranded in `Pending` or `Processing` for > 1 hour and calls gateway query APIs (`QueryTransactionStatusAsync`) to synchronize terminal states.
