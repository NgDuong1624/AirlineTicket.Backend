# Backend Implementation Plan: Multi-Gateway Payment Integration

## Overview
Implement multi-provider payment processing (`Stripe`, `PayPal`, `VNPay`, `MoMo`) in `AirlineTicket.Modules.Bookings` with asynchronous webhook callback lifecycle, transactional outbox pattern, idempotent event handling, and automated reconciliation.

- **Schema**: PostgreSQL `bookings` schema (`bookings.payments`, `bookings.webhook_events`, `bookings.outbox_messages`).
- **Framework**: .NET 10 (Clean Architecture + CQRS via MediatR).

---

## Phase 1: Domain Layer (`AirlineTicket.Modules.Bookings.Domain`)

- [ ] **Value Objects & Enums**:
  - [ ] `Currency`: `USD`, `VND` (`src/Modules/v1/Bookings/Domain/Enums/Currency.cs`).
  - [ ] `Money`: Record struct validating decimal rules (`VND`: 0 decimals, `USD`: 2 decimals) (`src/Modules/v1/Bookings/Domain/ValueObjects/Money.cs`).
  - [ ] `PaymentProvider`: `Stripe`, `PayPal`, `VNPay`, `MoMo` (`src/Modules/v1/Bookings/Domain/Enums/PaymentProvider.cs`).
  - [ ] `PaymentTransactionStatus`: `Pending`, `Processing`, `Succeeded`, `Failed`, `Cancelled`, `RefundPending`, `Refunded` (`src/Modules/v1/Bookings/Domain/Enums/PaymentTransactionStatus.cs`).
  - [ ] `WebhookEventStatus`: `Received`, `Processed`, `Failed` (`src/Modules/v1/Bookings/Domain/Enums/WebhookEventStatus.cs`).
- [ ] **Entities & Domain Logic**:
  - [ ] `Payment`: Aggregate root tracking transaction state, provider metadata, raw responses, and optimistic locking concurrency (`src/Modules/v1/Bookings/Domain/Entities/Payment.cs`).
  - [ ] `WebhookEvent`: Entity tracking ingested webhook events for replay prevention and idempotency (`src/Modules/v1/Bookings/Domain/Entities/WebhookEvent.cs`).
  - [ ] State Machine transitions with guard conditions preventing invalid lifecycle jumps.
- [ ] **Domain Events**:
  - [ ] `PaymentInitiatedEvent`, `PaymentSucceededEvent`, `PaymentFailedEvent`, `PaymentRefundedEvent` (`src/Modules/v1/Bookings/Domain/Events/`).

---

## Phase 2: Persistence & Infrastructure (`AirlineTicket.Modules.Bookings.Infrastructure`)

- [ ] **EF Core Configurations**:
  - [ ] `PaymentConfiguration`: Map to table `bookings.payments` with indexes on `(BookingId)`, `(Provider, ProviderTransactionId)` (`src/Modules/v1/Bookings/Infrastructure/Data/Configurations/PaymentConfiguration.cs`).
  - [ ] `WebhookEventConfiguration`: Map to table `bookings.webhook_events` with unique constraint on `(Provider, ProviderEventId)` (`src/Modules/v1/Bookings/Infrastructure/Data/Configurations/WebhookEventConfiguration.cs`).
- [ ] **Database Migrations**:
  - [ ] Add EF Core migration for payment tables in `bookings` schema.
  - [ ] Update seed data / integration test database setup scripts.
- [ ] **Repositories**:
  - [ ] `IPaymentRepository` & `PaymentRepository` implementation.
  - [ ] `IWebhookEventRepository` & `WebhookEventRepository` implementation.

---

## Phase 3: Gateway Integration & Strategy Pattern (`AirlineTicket.Modules.Bookings.Infrastructure.Gateways`)

- [ ] **Gateway Abstraction**:
  - [ ] `IPaymentGateway` contract (`CreatePaymentUrlAsync`, `ProcessWebhookAsync`, `RefundPaymentAsync`, `QueryTransactionStatusAsync`).
  - [ ] `IPaymentGatewayFactory` strategy resolver.
- [ ] **Gateway Implementations**:
  - [ ] `StripePaymentGateway`:
    - [ ] Stripe.net SDK integration.
    - [ ] Create PaymentIntent / Checkout Session (supports embedded elements & hosted checkout).
    - [ ] Webhook signature verification (`Stripe-Signature`).
  - [ ] `PayPalPaymentGateway`:
    - [ ] PayPal REST v2 API integration (Orders v2 SDK / HttpClient).
    - [ ] Order creation & capture.
    - [ ] Webhook verification with PayPal Transmission signature headers.
  - [ ] `VNPayPaymentGateway`:
    - [ ] URL generation with HMAC-SHA512 checksum sorting keys alphabetically.
    - [ ] IPN / return callback checksum validation.
    - [ ] QueryDR transaction query API.
  - [ ] `MoMoPaymentGateway`:
    - [ ] AIO redirect / capture URL creation with HMAC-SHA256 signature.
    - [ ] IPN webhook signature verification and raw JSON parsing.
    - [ ] Transaction status query API.
- [ ] **Options & Configurations**:
  - [ ] Register `PaymentOptions` bound to `src/appsettings.shared.json` & `src/appsettings.shared.Development.json`.

---

## Phase 4: Application Layer (CQRS & Commands) (`AirlineTicket.Modules.Bookings.Application`)

- [ ] **Commands**:
  - [ ] `CreateCheckoutSessionCommand` / Handler: Initiates payment record (`Pending`), invokes gateway, returns checkout URL / client secret (`src/Modules/v1/Bookings/Application/Payments/Commands/CreateCheckoutSession/`).
  - [ ] `IngestWebhookCommand` / Handler: Fast-acknowledges webhook, verifies signature, stores raw payload in `webhook_events`, emits Outbox message (`src/Modules/v1/Bookings/Application/Payments/Commands/IngestWebhook/`).
  - [ ] `ProcessPaymentCallbackCommand` / Handler: Outbox consumer handler, updates `Payment` & `Booking` status, emits `BookingConfirmedEvent`, triggers ticket issuance.
  - [ ] `RefundPaymentCommand` / Handler: Processes gateway refund, updates transaction status to `Refunded`.
- [ ] **Queries**:
  - [ ] `GetPaymentStatusQuery` / Handler: Retrieves current payment transaction status for polling client.

---

## Phase 5: API Endpoints & Security (`AirlineTicket.Modules.Bookings.Api`)

- [ ] **Middleware & Filters**:
  - [ ] Body buffering filter (`HttpRequest.EnableBuffering()`) to allow signature verification on raw payload.
  - [ ] Clock-skew / replay attack filter (5-minute timestamp tolerance).
- [ ] **Endpoints**:
  - [ ] `POST /api/payments/checkout`: Client initiates checkout session.
  - [ ] `POST /api/payments/webhooks/stripe`: Stripe webhook ingestion endpoint.
  - [ ] `POST /api/payments/webhooks/paypal`: PayPal webhook ingestion endpoint.
  - [ ] `POST /api/payments/webhooks/vnpay`: VNPay IPN webhook ingestion endpoint.
  - [ ] `POST /api/payments/webhooks/momo`: MoMo IPN webhook ingestion endpoint.
  - [ ] `GET /api/payments/{bookingId}/status`: Client status polling endpoint.

---

## Phase 6: Background Processing & Reconciliation (`AirlineTicket.Worker`)

- [ ] **Transactional Outbox Worker**:
  - [ ] `PaymentOutboxProcessor`: Polls unprocessed payment outbox messages, executes `ProcessPaymentCallbackCommand` with retry policy.
- [ ] **Reconciliation Job**:
  - [ ] `PaymentReconciliationJob`: Periodic background job (daily 02:00 UTC) querying unresolved `Pending` / `Processing` payments (> 1 hour old) against provider query APIs.

---

## Phase 7: Real-Time SignalR Hub (`AirlineTicket.SignalR`)

- [ ] Update `BookingHub` to broadcast `PaymentCompleted` / `BookingConfirmed` events directly to connected booking client.

---

## Phase 8: Testing & Verification

- [ ] **Unit Tests** (`tests/UnitTest/Modules/v1/Bookings/`):
  - [ ] Money & Currency value object rules.
  - [ ] Payment entity state transition unit tests.
  - [ ] Gateway checksum/signature calculation tests (VNPay HMAC-SHA512, MoMo HMAC-SHA256, Stripe, PayPal).
- [ ] **Integration Tests** (`tests/IntegrationTest/Modules/v1/Bookings.Api.IntegrationTests/Endpoints/Payments/`):
  - [ ] `CreateCheckoutSessionTests.cs` (`POST /api/payments/checkout`).
  - [ ] `StripeWebhookTests.cs`, `PayPalWebhookTests.cs`, `VNPayWebhookTests.cs`, `MoMoWebhookTests.cs`.
  - [ ] `GetPaymentStatusTests.cs` (`GET /api/payments/{bookingId}/status`).
- [ ] **Idempotency & Replay Verification**:
  - [ ] Duplicate webhook payload returns `200 OK` without double-crediting.
  - [ ] Expired webhook payloads rejected.
