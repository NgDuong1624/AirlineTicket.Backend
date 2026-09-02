# Notifications Module API Test Cases

## Module Overview
- **Base Paths**: `/api/notifications`, `/api/admin/notifications/templates`
- **Authentication**: Authenticated User, AdminOnly

---

## Checklist Summary

- [ ] **GET /api/notifications**
  - [ ] `TC-NTF-GET-001`: Authenticated user gets paginated notifications returns 200 OK
  - [ ] `TC-NTF-GET-002`: Unauthenticated request returns 401 Unauthorized

- [ ] **GET /api/notifications/unread-count**
  - [ ] `TC-NTF-CNT-001`: Authenticated user gets unread count returns integer number
  - [ ] `TC-NTF-CNT-002`: Unauthenticated request returns 401 Unauthorized

- [ ] **PUT /api/notifications/{id}/read**
  - [ ] `TC-NTF-READ-001`: Mark specific notification as read returns 200/204
  - [ ] `TC-NTF-READ-002`: Mark non-existent notification ID returns 404 Not Found
  - [ ] `TC-NTF-READ-003`: Mark another user's notification returns 403/404

- [ ] **PUT /api/notifications/read-all**
  - [ ] `TC-NTF-ALL-001`: Mark all notifications as read for current user returns 200/204

- [ ] **DELETE /api/notifications/{id}`**
  - [ ] `TC-NTF-DEL-001`: Soft delete notification returns 200/204
  - [ ] `TC-NTF-DEL-002`: Delete non-existent notification returns 404 Not Found

- [ ] **Admin Notification Templates Management**
  - [ ] `TC-NTF-TMPL-001`: Admin lists notification templates `GET /api/admin/notifications/templates` returns 200 OK
  - [ ] `TC-NTF-TMPL-002`: Admin gets template by ID `GET /api/admin/notifications/templates/{id}` returns 200 OK
  - [ ] `TC-NTF-TMPL-003`: Admin updates notification template `PUT /api/admin/notifications/templates/{id}` returns 200 OK
  - [ ] `TC-NTF-TMPL-004`: Non-admin access to template endpoints returns 403 Forbidden

---

## Detailed Test Case Specifications

### TC-NTF-READ-001: Mark notification as read
- **Endpoint**: `PUT /api/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6/read`
- **Auth**: Customer/Staff JWT (owner of notification)
- **Assertions**:
  - HTTP Status: `200 OK` or `204 No Content`
  - Subsequent `GET /api/notifications/unread-count` reflects decremented count.
