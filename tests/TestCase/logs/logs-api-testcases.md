# Logs Module API Test Cases

## Module Overview
- **Base Paths**: `/api/admin/logs`, `/api/partner/logs`
- **Authentication**: AdminOnly, PartnerOnly

---

## Checklist Summary

- [ ] **GET /api/admin/logs**
  - [ ] `TC-LOG-ADM-001`: Admin gets paginated system logs returns 200 OK
  - [ ] `TC-LOG-ADM-002`: Filter by `level` (e.g., `Error`, `Critical`) returns matched logs
  - [ ] `TC-LOG-ADM-003`: Filter by `search` text returns logs containing keyword
  - [ ] `TC-LOG-ADM-004`: Filter by `airlineId="system"` returns system-only logs
  - [ ] `TC-LOG-ADM-005`: Non-admin access returns 403 Forbidden
  - [ ] `TC-LOG-ADM-006`: Unauthenticated access returns 401 Unauthorized

- [ ] **GET /api/partner/logs**
  - [ ] `TC-LOG-PART-001`: Partner gets paginated logs scoped to partner's airline returns 200 OK
  - [ ] `TC-LOG-PART-002`: Partner cannot access logs of another airline
  - [ ] `TC-LOG-PART-003`: Non-partner access returns 403 Forbidden

---

## Detailed Test Case Specifications

### TC-LOG-ADM-002: Admin filter logs by level
- **Endpoint**: `GET /api/admin/logs?level=Error&pageIndex=1&pageSize=10`
- **Auth**: Admin JWT
- **Assertions**:
  - HTTP Status: `200 OK`
  - All returned items have `level == "Error"`.
  - Paged result contains pagination metadata (`pageNumber`, `pageSize`, `totalCount`).
