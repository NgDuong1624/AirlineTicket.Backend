# Support Chat Module

## Overview
Real-time customer ↔ staff chat via SignalR. Supports auto-assignment, load-balanced routing, and message persistence.

## SignalR Hub
- **Endpoint**: `/hubs/support`
- **Auth**: None (customer join), none (staff register — currently open for prototyping; secure with JWT in production)

## Hubs
### SupportChatHub
#### Client → Server Methods
| Method | Args | Description |
|--------|------|-------------|
| `CustomerJoinChat` | `airlineId: Guid`, `customerName: string` | Customer enters queue for a specific airline |
| `StaffRegister` | `airlineId: Guid`, `staffName: string` | Staff member goes online for their airline |
| `SendMessageToAirline` | `message: string` | Customer sends message to assigned staff |
| `SendMessageToCustomer` | `customerConnectionId: string`, `message: string` | Staff replies to a specific customer |

#### Server → Client Events
| Event | Args | Description |
|-------|------|-------------|
| `AgentAssigned` | `staffName: string` | Notifies customer of staff assignment |
| `ReceiveMessage` | `connectionId, role, name, message` | Incoming message delivery |
| `SystemMessage` | `message: string` | System-level notifications (queue, errors) |
| `NewCustomerChat` | `connectionId, customerName` | Notifies staff of new customer |
| `CustomerDisconnected` | `connectionId, customerName` | Notifies staff when customer leaves |

## Load Balancing
Staff members are automatically assigned to customers based on **fewest active chats** for the best distribution.

## Persistence
Chat messages are persisted in the `ChatMessages` table in the database using EF Core (`InteractionDbContext`). This ensures that chat history survives page reloads and can be audited.

## Database Schema
- **Table**: `dbo.ChatMessages`
- **Columns**: `Id`, `AirlineId`, `SenderRole`, `SenderName`, `CustomerConnectionId`, `StaffConnectionId`, `Content`, `SentAt`
