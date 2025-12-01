# Unity Demo Setup Guide

This guide explains how to import the VR Classroom demo into Unity and get it running.

## Prerequisites

- Unity 2020.3 LTS or later (2021 LTS or 2022 LTS recommended)
- Basic Unity knowledge

## Quick Start

### 1. Create a New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "3D" or "3D (URP)" template
4. Name your project (e.g., "VRClassroomDemo")
5. Click "Create Project"

### 2. Import the Scripts

Copy all files from the `unity/Scripts/` folder to your Unity project's `Assets/Scripts/` folder:

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── CameraController.cs
│   ├── UI/
│   │   ├── OnboardingMenu.cs
│   │   ├── MainMenuController.cs
│   │   └── GameplayHUD.cs
│   ├── Classroom/
│   │   └── ClassroomSceneManager.cs
│   ├── Interactables/
│   │   ├── InteractableBase.cs
│   │   ├── WhiteboardInteractable.cs
│   │   ├── InteractableDesk.cs
│   │   └── InteractivePodium.cs
│   ├── avatarsimple.cs
│   └── Scripts-classroom.cs
```

### 3. Create the Demo Scene

#### Scene Hierarchy Structure

Create the following GameObject hierarchy:

```
DemoClassroom (Scene)
├── --- MANAGERS ---
│   ├── ClassroomSceneManager (empty GameObject)
│   │   └── ClassroomSceneManager.cs component
│   └── EventSystem (create via UI > Event System)
│
├── --- ENVIRONMENT ---
│   ├── Floor (3D Object > Plane, scale 10x10)
│   ├── Walls (optional, for realism)
│   ├── Lighting
│   │   ├── Directional Light
│   │   └── Point Lights (optional)
│   └── ClassroomRoot (empty, for ClassroomBootstrapper)
│       └── ClassroomBootstrapper.cs component
│
├── --- UI ---
│   └── Canvas (create via UI > Canvas)
│       ├── OnboardingPanel
│       │   ├── NameInputField
│       │   ├── AgeInputField
│       │   ├── SchoolInputField
│       │   ├── MajorInputField
│       │   ├── RoleDropdown
│       │   ├── SubmitButton
│       │   ├── SkipButton
│       │   └── ErrorText
│       ├── MainMenuPanel
│       │   ├── WelcomeText
│       │   ├── EnterClassroomButton
│       │   ├── SettingsButton
│       │   ├── EditProfileButton
│       │   └── QuitButton
│       └── GameplayHUD
│           ├── InteractionPrompt
│           ├── CameraModeText
│           ├── Crosshair
│           └── ControlsHint
│
├── --- PLAYER (spawned at runtime) ---
│
└── --- SPAWN POINTS ---
    ├── StudentSpawnPoint (empty, position at back of room)
    └── InstructorSpawnPoint (empty, position at podium)
```

### 4. Create Basic Prefabs

#### Desk Prefab
1. Create a Cube (3D Object > Cube)
2. Scale to (1, 0.7, 0.6) for a desk shape
3. Add Box Collider (should be added automatically)
4. Create a prefab by dragging to Project window
5. Add `InteractableDesk.cs` component

#### Podium Prefab
1. Create a Cube scaled to (0.8, 1.2, 0.5)
2. Add Box Collider
3. Create prefab
4. Add `InteractivePodium.cs` component

#### Whiteboard Prefab
1. Create a Cube scaled to (4, 2.5, 0.1)
2. Add Box Collider
3. Create prefab
4. Add `WhiteboardInteractable.cs` component

### 5. Configure Components

#### ClassroomSceneManager Setup
1. Select the ClassroomSceneManager GameObject
2. Assign references:
   - Player Spawn Point: StudentSpawnPoint transform
   - Show Onboarding On Start: ✓ (checked)
   - Auto Spawn Player: ✓ (checked)

#### ClassroomBootstrapper Setup
1. Assign prefabs:
   - Desk Prefab: your desk prefab
   - Podium Prefab: your podium prefab
   - Whiteboard Prefab: your whiteboard prefab
2. Configure layout:
   - Rows: 3
   - Cols: 5
   - Desk Spacing: (1.6, 0, 1.4)

#### OnboardingMenu Setup
1. Add `OnboardingMenu.cs` to Canvas
2. Wire up all InputFields, Buttons, and Text references
3. Set Minimum Age: 13

#### MainMenuController Setup
1. Add to Canvas
2. Wire up button references
3. Connect to ClassroomSceneManager

### 6. Test the Scene

1. Press Play in Unity Editor
2. The onboarding menu should appear
3. Fill in your information and click Submit
4. You should spawn in the classroom
5. Use WASD to move, mouse to look
6. Press V to switch between first/third person
7. Press E to interact with objects
8. Press Escape to pause

## Controls Summary

| Key | Action |
|-----|--------|
| W/A/S/D | Move |
| Mouse | Look around |
| Shift | Run |
| Space | Jump |
| V | Toggle camera (1st/3rd person) |
| E | Interact |
| C | Clear whiteboard |
| Escape | Pause menu |
| R | Reload scene (demo mode) |

## Script Descriptions

### Core Scripts

| Script | Description |
|--------|-------------|
| `PlayerController.cs` | Handles player movement, mouse look, and camera perspective switching |
| `CameraController.cs` | Alternative camera controller with more features (orbit, collision) |
| `ClassroomSceneManager.cs` | Manages scene initialization, player spawning, and scene state |
| `ClassroomBootstrapper.cs` | Procedurally generates classroom layout (desks, podium, whiteboard) |

### UI Scripts

| Script | Description |
|--------|-------------|
| `OnboardingMenu.cs` | Collects user info (name, age, school, major, role) at startup |
| `MainMenuController.cs` | Main menu with settings and profile management |
| `GameplayHUD.cs` | In-game HUD showing prompts, camera mode, and controls |
| `DemoUIController.cs` | Demo-specific UI for investors (tour, view switching) |

### Interactable Scripts

| Script | Description |
|--------|-------------|
| `InteractableBase.cs` | Base class for all interactive objects |
| `WhiteboardInteractable.cs` | Drawable whiteboard using mouse/pointer |
| `InteractableDesk.cs` | Desk that player can sit at |
| `InteractivePodium.cs` | Instructor podium with presentation controls |

### Legacy Scripts

| Script | Description |
|--------|-------------|
| `AvatarSimple.cs` | Simple WASD+mouse controller (use PlayerController instead) |

## Troubleshooting

### "Script not found" errors
- Make sure all scripts are in Assets/Scripts
- Check for compile errors in Console

### Player doesn't spawn
- Check ClassroomSceneManager has showOnboardingOnStart or autoSpawnPlayer enabled
- Check Console for errors

### Can't interact with objects
- Make sure objects have Colliders
- Check InteractableBase.isInteractable is true
- Verify you're close enough (interactionDistance)

### Camera not working
- Check PlayerController has playerCamera assigned
- Verify Camera.main exists in scene

## Next Steps

1. **Add VR Support**: Integrate XR Interaction Toolkit for VR headsets
2. **Networking**: Add Photon Fusion or Unity Netcode for multiplayer
3. **Voice Chat**: Integrate Agora or Vivox for spatial audio
4. **Content**: Add presentation slides, quizzes, and more interactive objects

## File Locations

```
Repository Root/
├── unity/
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── UI/
│   │   ├── Classroom/
│   │   └── Interactables/
│   ├── Scenes/
│   ├── Prefabs/
│   └── Materials/
├── docs/
├── ARCHITECTURE.md
├── TECH_STACK.md
└── README.md
```
