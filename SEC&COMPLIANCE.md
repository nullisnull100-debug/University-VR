# Security, Privacy & Compliance

Legal / Privacy:
- FERPA: keep student education records private; define retention & access policies.
- GDPR (if EU students): handle consent, right to be forgotten; document data flows and location.
- Recordings: require explicit consent for audio/video recording; instructors can enable/disable per session.

Authentication & Authorization:
- Use campus SAML/OIDC wherever possible; fallback to institutional OAuth.
- Short-lived join tokens signed by the matchmaker (JWT with session_id, user_id, role).
- Role mapping: student / TA / instructor / auditor — enforce permissions server-side.

Data protection:
- All control-plane and media transport must be TLS/DTLS encrypted.
- Use SRTP for voice; encrypt recordings at rest (S3 SSE or equivalent).
- Minimize PII stored — only necessary metadata (user id mapping to campus ID).

Logging & Access:
- Audit logs for moderation actions and joins/leaves.
- Role-based access control for replay downloads and admin tools.
- Log retention policy: keep minimum required for support and compliance.

Operational Security:
- Pen-test networking endpoints and matchmaker APIs.
- Harden game servers (limit ports, patch regularly).
- DDoS protection for matchmaker and signaling endpoints (Cloudflare / AWS Shield).

Student safety & moderation:
- Instructor controls for muting, temporary eject, or placing students in a lobby.
- Provide a direct reporting tool in the client.
- Content moderation for shared artifacts (uploaded media) with scanning and approval workflow.

Backup & Disaster Recovery:
- Back up metadata DB daily; recordings to multi-region storage if required.
- Define RTO/RPO for classroom sessions and provide a documentation page for instructors on recovery.
```
