# VR Classroom Service — Overview

Goal:
Build a service that lets universities offer online students the option to attend classes in immersive VR (and sensible 2D fallbacks). Students can join instances (classrooms), interact with teachers and peers, manipulate interactive objects, and record/review sessions. The platform must integrate with campus identity/LMS, be secure and private (FERPA/GDPR-aware), and scale across many simultaneous classes.

What I prepared:
- Architecture and dataflow recommendations
- Tech stack options (client/server/infra)
- MVP plan with features, milestones and test metrics
- API spec sketch for room/session lifecycle
- Security & compliance checklist
- Unity integration guidance and sample code pointers
- Cost & staffing rough estimate

How to use:
1. Pick device coverage (PC + Mobile WebXR + Oculus/Meta Quest is common).
2. Decide on an initial networking stack (Photon, Unity Relay + Netcode, Mirror + dedicated servers).
3. Build an MVP classroom supporting 8–25 concurrent users, voice, shared object interaction, and LMS SSO.
4. Run a 1-course pilot, collect telemetry, then iterate.

Next artifacts:
- ARCHITECTURE.md
- TECH_STACK.md
- MVP_PLAN.md
- API_SPEC.yaml
- UNITY_SAMPLE.md
- SECURITY_COMPLIANCE.md
```
