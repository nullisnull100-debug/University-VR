# Architecture — High Level

Goals:
- Low-latency real-time interactions for up to ~50 concurrent users per classroom instance initially.
- Authoritative server model for object state to avoid client cheating and ensure consistency.
- Flexible: allow both fully immersive VR clients and 2D browser/WebRTC clients.

Components:
1. Client (Unity, Unreal, or WebXR)
   - VR device support (Meta Quest, Pico), desktop VR, and WebXR fallback.
   - Handles rendering, input, local predict & reconciliation, and voice capture.
   - Connects to presence/matchmaker and then the authoritative classroom server.

2. Matchmaker / Lobby Service (stateless)
   - Authenticates user (SAML/OAuth2 via campus SSO or LTI).
   - Finds or provisions a classroom instance (dedicated server or shared hosted instance).
   - Returns connection metadata (server IP/port, TURN info, join token).

3. Authoritative Classroom Server (game server)
   - Runs authoritative object simulation and physics for interactive objects.
   - Syncs state via a real-time protocol (UDP-based: ENet, or WebRTC DataChannel, or TCP for smaller scale).
   - Maintains authoritative positions, interactions, and permissions.
   - Emits event stream for recording and auditing.

4. Real-time Voice/Video
   - WebRTC for voice (peer-assisted or SFU), or a managed service (e.g., Vivox, Agora).
   - Typically separate from object-sync channel (different QoS & protocols).

5. Persistence & Services
   - Session storage (Postgres / DynamoDB) for session metadata, roster, recordings.
   - Object/content store (S3/Blob).
   - Recording service: ingest server events + mixed audio/video → stored classroom replay.
   - Analytics/telemetry (Prometheus, Grafana, ELK/Datadog).

6. LMS & Identity Integration
   - LTI and/or OAuth2 connector to Canvas/Moodle/Blackboard for roster, course mapping.
   - SSO via SAML/OIDC; map campus roles to in-room roles (student/TA/instructor/observer).

7. Orchestration & Scaling
   - Kubernetes/ECS to host matchmaker, API, and auxiliary services.
   - Game server fleet on GKE/EKS/ECS or hosted game server providers (e.g., Unity Multiplay, PlayFab, GameLift).
   - Autoscaling based on concurrent classroom instances and CPU/network.

Dataflow (join flow):
- Client -> Auth (SSO) -> Matchmaker -> Provision/find server -> Client connects to classroom server (UDP/DataChannel) and voice SFU -> Classroom server authoritatively syncs world and interactions.

Key design decisions to make early:
- Authoritative server per classroom vs. P2P: choose server authority for consistency and moderation.
- Networking layer: WebRTC DataChannel vs. UDP-based library (ENet) or managed relays.
- Hosted server provider (Unity Multiplay / AWS GameLift / self-managed K8s): choose based on budget and control.

Operational concerns:
- TURN/STUN for NAT traversal (if using WebRTC).
- Logging/retention policies (FERPA).
- Moderation tools: instructor mute/kick, presence logs, replay audit.
```
