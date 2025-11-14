# Suggested Tech Stack

Clients:
- Primary: Unity 2020+ (good device ecosystem: Quest, PCVR, AR).
- Alternative: WebXR (for browser access), Unreal for high-fidelity visuals.
- Voice: WebRTC or managed (Agora, Vivox).

Networking / Multiplayer:
- Small-scale / Quick MVP:
  - Photon Realtime / Fusion — quick to integrate, managed servers.
  - Mirror + Hosted servers (cheaper, open-source).
  - Unity Netcode + Unity Relay/Multiplay (native stack if using Unity).
- Medium/Scale:
  - Dedicated authoritative servers on AWS GameLift / Google Game Servers / Multiplay.
  - Use ENet (reliable UDP) or WebRTC DataChannels for cross-platform.
- Real-time events and CRDTs:
  - Consider operational transforms or lock / authoritative patterns for shared object edits.
  - For complex object editing, persist events to EventStore or Kafka for replay.

Backend:
- Language/Framework: Node.js (fast prototyping), Go (low-latency services), or C# (if deep Unity backend integration desired).
- API: REST for control plane; WebSocket/WebRTC for realtime.
- DB: PostgreSQL for relational data (rosters, courses), Redis for presence/fast state, S3 for recordings & assets.
- Auth: OIDC/SAML connectors, LTI 1.3 for LMS integration.

Infrastructure:
- K8s (GKE/EKS/AKS) or Fargate/ECS for API & matchmaker.
- Game server fleet via GKE or managed providers (GameLift/Multiplay).
- CDN for asset distribution (CloudFront/Akamai).
- Monitoring: Prometheus + Grafana; logs in ELK or Datadog.
- CI/CD: GitHub Actions for client & server pipelines; automated deploys to staging and canary to production.

Third-party services:
- SSO/LTI: OAuth2/OIDC + LTI library (IMS Global).
- TURN Provider: Twilio, Xirsys, or self-hosted coturn.
- Voice & Recording: Agora / Twilio Programmable Video / Vivox for SFU; or self-hosted Janus or Jitsi for smaller scale.

Developer tooling:
- Local dev: local relay/mock server and sample Unity scenes.
- Telemetry: student action events, object interactions, join/leave, latency stats.
```
