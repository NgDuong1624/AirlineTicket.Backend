# Interactions Module

## Overview
The **Interactions Module** handles customer interactions, reviews, and an AI-powered travel assistant. It provides automated AI customer support for flight bookings, route inquiries, luggage policies, and pricing via a retrieval and generation pipeline.

---

## How It Works (For End Users & Clients)
1. **AI Travel Assistant**: Users can ask natural language questions regarding flights, baggage allowances, booking steps, and travel policies (`POST /api/v1/qa/ask`).
2. **Context & Conversation History**: The client can send preceding chat messages (`History`) along with user preferences (e.g., preferred `Currency`) to provide conversational context to the underlying AI model.
3. **Graceful Degradation**: The endpoint handles rate limits and API quota limits cleanly with HTTP 429 (quota exceeded) and HTTP 503 (service unavailable) responses.
4. **Reviews**: Supports customer reviews for flights and airlines with star ratings (1–5) and verified purchase flags.

---

## Domain Entities & Data Model

### Review
Represents a customer review for an airline or flight.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid): FK to the user who submitted the review.
- `AirlineId` (Guid?): FK to the airline being reviewed.
- `FlightId` (Guid?): FK to the flight being reviewed.
- `Rating` (int): Rating from 1 to 5.
- `Comment` (string?): Review comment text.
- `IsVerifiedPurchase` (bool): Whether the reviewer completed a confirmed booking.
- `IsHidden` (bool): Whether the review is hidden by moderators.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### ChatMessage (AI Assistant Context & Support History)
Represents a message in the chat conversation history.
- `Id` (Guid): Unique identifier.
- `AirlineId` (Guid?): Airline context for the conversation.
- `SenderRole` (string): Role of sender (`"Customer"`, `"Staff"`, or `"Assistant"`).
- `SenderName` (string): Display name of sender.
- `CustomerConnectionId` (string?): SignalR connection ID of customer.
- `StaffConnectionId` (string?): SignalR connection ID of staff.
- `Content` (string): Message text.
- `SentAt` (DateTime): UTC timestamp of message dispatch.

---

## API Reference

### Endpoints

| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/v1/interactions` | None | Module status probe for interactions service. | None | `InteractionsStatusDto` |
| **POST** | `/api/v1/qa/ask` | None | Send question to AI Travel Assistant with optional chat history and currency. | `AskQuestionRequest { question: string, history?: ChatMessageDto[], currency?: string }` | `AirTicketAnswerResponse { answer: string }` |

---

## AI Service Configuration
The AI Travel Assistant connects via configured `AiServiceOptions` and `ModelStoreOptions`, orchestrating LLM queries and tool functions to inspect flights, fares, and system information.
