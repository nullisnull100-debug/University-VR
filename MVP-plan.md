# VR Classroom MVP Plan

## Demo Checklist

### Phase 1: Core Demo (Complete)
- [x] Basic classroom layout script (ClassroomBootstrapper)
- [x] Player controller with WASD movement
- [x] First-person camera perspective
- [x] Third-person camera perspective
- [x] Camera switching (V key)
- [x] Interactive whiteboard (draw with mouse)
- [x] Onboarding menu (name, age, school, major)
- [x] Main menu system
- [x] Gameplay HUD (prompts, camera mode display)
- [x] Interactive desks (sit feature)
- [x] Interactive podium (presentation mode)
- [x] Unity setup documentation

### Phase 2: Multiplayer (Current Focus)
- [x] Network manager (Photon PUN2)
- [x] Remote player connection & sync
- [x] Network player prefab with position sync
- [x] Voice chat integration (Photon Voice)
- [x] Push-to-talk and voice-activated modes
- [x] Mute/unmute controls
- [x] Spatial audio support
- [x] Synchronized whiteboard (NetworkedWhiteboard)
- [x] Network lobby UI
- [x] Room creation and joining

### Phase 3: Enhanced Demo
- [ ] VR headset support (XR Interaction Toolkit)
- [ ] Hand tracking / controller input
- [ ] Multiple classroom layouts
- [ ] Student avatar customization
- [ ] Instructor laser pointer
- [ ] User presence indicators

### Phase 4: LMS Integration
- [ ] SSO authentication
- [ ] Course roster sync
- [ ] Session recording
- [ ] Attendance tracking

## Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look |
| Shift | Run |
| Space | Jump |
| V | Toggle 1st/3rd person |
| E | Interact |
| C | Clear whiteboard |
| Escape | Menu |
| M | Toggle mute (voice) |
| T | Push-to-talk |
| PageUp/Down | Adjust volume |

## Getting Started

See `unity/SETUP_GUIDE.md` for Unity import and networking setup instructions.
