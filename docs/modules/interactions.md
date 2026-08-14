# Interactions Module

## Overview
The **Interactions Module** handles customer feedback and provides an AI-powered travel assistant. It allows users to submit reviews for airlines and flights, and enables real-time Q&A with an AI travel assistant to help with flight-related queries.

---

## How It Works (For End Users & Clients)
1. **Reviews**: Customers can submit reviews for airlines and flights they have experienced. Reviews include a rating (1-5 stars) and a comment. The system tracks if the purchase was verified.
2. **AI Travel Assistant**: Users can ask questions about flight tickets, travel requirements, or general travel advice. The system uses an AI service to process these questions and provide intelligent answers. The chat history can be included to provide context for the AI.

---

## Domain Entities & Data Model

### Review
Represents a customer review for an airline or flight.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid): FK to the user who submitted the review.
- `AirlineId` (Guid?): FK to the airline being reviewed.
- `FlightId` (Guid?): FK to the flight being reviewed.
- `Rating` (int): Rating from 1 to 5.
- `Comment` (string?): Review comment.
- `IsVerifiedPurchase` (bool): Whether the purchase was verified.
- `IsHidden` (bool): Whether the review is hidden.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### ChatMessage (AI Assistant Context)
Represents a message in the AI chat history.
- `Id` (Guid): Unique identifier.
- `AirlineId` (Guid): The airline context for the chat.
- `SenderRole` (string): Role of the sender ("Customer" or "Staff").
- `SenderName` (string): Name of the sender.
- `CustomerConnectionId` (string?): SignalR connection ID of the customer.
- `StaffConnectionId` (string?): SignalR connection ID of the staff.
- `Content` (string): The message content.
- `SentAt` (DateTime): UTC timestamp of when the message was sent.

---

## API Reference

### Endpoints

| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/interactions` | None | Health check endpoint for the interactions module. | None | `unknown` (Status 200 OK) |
| **POST** | `/api/qa/ask` | None | Send a question to the AI Travel Assistant. | `{ question: string, history: Array<{ role: string, content: string }>, currency: string }` | `AirTicketAnswerResponse { answer: string, ... }` |

---

## AI Service Configuration
The AI Travel Assistant is configured via `AiServiceOptions` and `ModelStoreOptions`, which define the AI model, temperature, and other parameters for the AI service. These settings are typically managed in the application configuration.
