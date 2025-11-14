```markdown
# Unity Integration Notes & Sample pseudo-code

Recommendations:
- Use Unity 2020+ LTS.
- For networking:
  - Quick MVP: Photon Fusion (hosted, easy) or Unity Netcode + Relay.
  - If you prefer full control: Mirror/ENet on dedicated servers.
- Use authoritative server pattern:
  - Server owns object state, clients send input/intent.
  - Server validates and sends snapshots (delta-compressed).

Avatar & Interaction:
- Simple avatars with head + hands transforms (2–3 networked transforms).
- Interactable objects: server-side physics for authoritative behavior, clients predict interactions (client-side prediction + server reconciliation).

Voice:
- Use WebRTC if clients include browsers; use provider SDK (Agora/Vivox) for cross-device voice and recording.

Example: High-level Unity client flow (pseudo-code)
```csharp
// Pseudo-code (not full compile-ready)
class VRClient : MonoBehaviour {
    string apiToken;
    void Start() {
        apiToken = AuthenticateWithCampusSSO();
        var joinInfo = RequestMatchmakerJoin(sessionId, apiToken);
        ConnectToGameServer(joinInfo);
        InitVoice(joinInfo.turnServers);
    }

    void FixedUpdate() {
        // collect input (position/orientation, button events)
        var input = CollectInput();
        SendInputToServer(input); // via reliable or unreliable channel depending on event
        ReceiveSnapshots(); // apply authoritative states with interpolation
    }

    void OnInteract(GameObject obj, InteractionEvent e) {
        // send intention to server
        SendEventToServer(new { type="interact", target= obj.id, action=e.type });
    }
}
```

Server-side tips:
- Snapshot frequency: 10–20 Hz for object state; higher for small fast objects if needed.
- Use delta compression and client-side interpolation/extrapolation to hide jitter.
- Keep messages minimal: ObjectId + transform + event id + small state payload.

Testing:
- Use local dev tools to simulate packet loss and latency.
- Stress-test with headless Unity instances (or server builds) and bots to validate scaling.

Assets:
- Provide a small teacher mode UI: pointer, remote whiteboard, spawn quizzes/interactive widgets.
```
