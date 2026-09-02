# Interactions Module API Test Cases

## Module Overview
- **Base Paths**: `/api/interactions`, `/api/qa/ask`
- **Authentication**: Public

---

## Checklist Summary

- [ ] **GET /api/interactions**
  - [ ] `TC-INT-HLTH-001`: Health check endpoint returns 200 OK

- [ ] **POST /api/qa/ask** (AI Travel Assistant)
  - [ ] `TC-INT-QA-001`: Ask question with valid history and currency returns 200 OK with answer
  - [ ] `TC-INT-QA-002`: Ask question with empty question string returns 400 Bad Request
  - [ ] `TC-INT-QA-003`: Ask question with malformed conversation history returns 400 Bad Request

---

## Detailed Test Case Specifications

### TC-INT-QA-001: Query AI travel assistant
- **Endpoint**: `POST /api/qa/ask`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "question": "What baggage allowance is included for economy class on Vietnam Airlines?",
    "history": [
      {
        "role": "user",
        "content": "Hi, I have a flight tomorrow."
      }
    ],
    "currency": "USD"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Body contains non-empty `answer` string.
