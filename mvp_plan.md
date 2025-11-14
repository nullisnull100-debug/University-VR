# MVP Plan (8–12 weeks)

MVP objectives:
- Support scheduled classes with 8–25 concurrent students.
- Authoritative server per class instance.
- 3D classroom with a few interactable objects (pointer, whiteboard, quiz object).
- Voice chat & minimal moderation controls.
- LMS SSO (basic) + roster mapping.
- Recording of session events and voice.

Milestones:

Week 0–2: Kickoff & Prototype
- Choose stacks and device targets (e.g., Unity + WebXR fallback).
- Build a simple Unity scene: avatars, basic movement, gaze pointing.
- Implement matchmaker stub and local authoritative server prototype.
Deliverable: Local demo with 4 clients on same LAN.

Week 3–5: Core real-time & Server
- Implement authoritative server for object sync (ENet or Realtime SDK).
- Add object interaction API: pick-up/drop, property sync.
- Integrate WebRTC or voice provider.
- Basic security: token-based room join with short TTL.
Deliverable: Hosted dev server supporting 8 concurrent users.

Week 6–8: LMS & Auth + Recording
- Implement SSO / LTI integration with sample Canvas/Moodle.
- Implement session recording (event log + audio capture).
- Instructor moderation tools (mute/kick, grant presenter).
Deliverable: Pilot-ready demo with one partner course.

Week 9–12: Pilot & Iterate
- Run 1-course pilot with 10–25 students.
- Collect metrics: join success, median latency, packet loss, CPU & bandwidth.
- Iterate UX and fix blocking issues.
Deliverable: Pilot report and next-phase roadmap.

Success metrics for MVP:
- Join success rate > 98% for enrolled students.
- Median RTT < 100 ms for object updates within region.
- Voice quality MOS > 3.5 for 95% of participants.
- Instructor satisfaction score >= 4/5 in pilot survey.

Team & Roles:
- 1 Product Manager
- 2 Unity/Client Engineers
- 1 Backend Engineer (matchmaker/game servers)
- 1 DevOps (K8s, autoscaling, networking)
- 1 QA/Researcher (pilot orchestration & analytics)
- Optional: 1 UX/Instructional Designer (for pedagogy)

Pilot checklist:
- Consent forms and privacy disclosures (FERPA)
- Device access plan for students without VR headsets
- On-call support during pilot sessions
```
