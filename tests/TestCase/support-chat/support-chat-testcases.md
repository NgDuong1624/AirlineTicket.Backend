# Support Chat Module SignalR Hub Test Cases

## Module Overview
- **Hub URL**: `/hubs/support`
- **Protocol**: SignalR WebSocket
- **Client Methods**: `CustomerJoinChat`, `StaffRegister`, `SendMessageToAirline`, `SendMessageToCustomer`
- **Server Events**: `SystemMessage`, `AgentAssigned`, `ReceiveMessage`, `NewCustomerChat`, `CustomerDisconnected`

---

## Checklist Summary

- [ ] **Customer Connection & Queue**
  - [ ] `TC-CHAT-CST-001`: Customer connects and joins chat via `CustomerJoinChat(airlineId, customerName)` receiving `SystemMessage`
  - [ ] `TC-CHAT-CST-002`: Customer joined when staff online triggers `AgentAssigned(staffName)` and `NewCustomerChat` to staff
  - [ ] `TC-CHAT-CST-003`: Customer joined with no staff online receives "No staff online" `SystemMessage`

- [ ] **Staff Registration & Load Balancing**
  - [ ] `TC-CHAT-STF-001`: Staff goes online via `StaffRegister(airlineId, staffName)`
  - [ ] `TC-CHAT-STF-002`: Auto-assignment dispatches customer to staff member with fewest active chats
  - [ ] `TC-CHAT-STF-003`: Staff registering automatically claims queued waiting customers

- [ ] **Bi-directional Messaging & Persistence**
  - [ ] `TC-CHAT-MSG-001`: Customer sends message via `SendMessageToAirline` -> delivered to staff and persisted in `ChatMessages` table
  - [ ] `TC-CHAT-MSG-002`: Staff replies via `SendMessageToCustomer` -> delivered to customer and persisted in `ChatMessages` table

- [ ] **Disconnection & Reassignment**
  - [ ] `TC-CHAT-DSC-001`: Customer disconnect triggers `CustomerDisconnected` event to assigned staff
  - [ ] `TC-CHAT-DSC-002`: Staff disconnect sends `SystemMessage` and reassigns customers to next available staff

---

## Detailed Test Case Specifications

### TC-CHAT-MSG-001: Customer message routing and persistence
- **Hub**: `/hubs/support`
- **Preconditions**: Customer `C1` and Staff `S1` connected for Airline `A1` and paired.
- **Invocation**:
  - Customer calls `SendMessageToAirline("Where is my terminal gate?")`.
- **Assertions**:
  - Staff receives `ReceiveMessage` event with payload `{ senderRole: "Customer", message: "Where is my terminal gate?" }`.
  - Database table `ChatMessages` has new row with `SenderRole = "Customer"`, matching `AirlineId`, and `Content = "Where is my terminal gate?"`.
