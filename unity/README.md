# Unity VR Classroom Scripts

This directory contains plug-and-play Unity scripts for the VR Classroom project.

## Quick Setup

### Option 1: Direct Copy (Recommended)
1. Copy the entire `Scripts/` folder to your Unity project's `Assets/` directory
2. The folder structure will be preserved automatically
3. Unity will detect and compile all scripts

### Option 2: Drag and Drop
1. In Unity, open your project
2. Drag the `Scripts/` folder from your file system into the Unity Project window under `Assets/`
3. Unity will import all scripts with the correct structure

## Folder Structure

```
Scripts/
├── UniversityVR.asmdef          # Assembly definition for proper compilation
├── Classroom/                    # Classroom setup and management
├── Instructor/                   # Instructor-specific features (admin, attendance, etc.)
├── Interactables/               # Interactive objects (whiteboard, desks, podium)
├── Networking/                   # Multiplayer networking (Photon PUN2)
├── Player/                       # Player controller, camera, avatar
└── UI/                          # User interface components
```

## Dependencies

The scripts require these Unity packages (install via Package Manager):

### Core Packages
- TextMeshPro (for UI text)
- Unity UI (built-in)

### For Networking Features (Optional)
- Photon PUN2 (Multiplayer networking)
- Photon Voice (Voice chat)

To install Photon:
1. Download from Unity Asset Store or Photon website
2. Import the PUN2 package
3. Follow Photon setup wizard

## After Import

1. Unity will automatically compile all scripts
2. Check the Console for any missing dependencies
3. Install required packages if prompted
4. See `SETUP_GUIDE.md` for detailed scene setup instructions

## Notes

- All scripts use proper namespaces for organization
- The assembly definition ensures clean compilation
- Scripts are organized by feature for easy navigation
- Compatible with Unity 2020.3 LTS and newer
