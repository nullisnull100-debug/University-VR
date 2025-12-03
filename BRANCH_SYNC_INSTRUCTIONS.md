# Branch Sync Instructions

## Summary
The `main` branch (default) needs to be synced with the `MAIN` branch, and then the `MAIN` branch should be deleted.

## Current Status
- **Default branch**: `main` (lowercase)
- **Branch to sync from**: `MAIN` (uppercase)
- **Commits ahead**: MAIN has 34 commits ahead of main

## Changes in MAIN Branch
The MAIN branch includes:
1. Removed root-level .cs files (Scripts-classroom.cs, Whiteboardinteractable.cs, avatarsimple.cs, demoui.cs)
2. Added UPDATE_PLAN.md documentation
3. Added unity/README.md for plug-and-play setup
4. Added unity/Scripts/UniversityVR.asmdef assembly definition
5. Fixed deprecated crypto API (RNGCryptoServiceProvider -> RandomNumberGenerator.Create())
6. Added material cleanup to prevent memory leaks in NetworkPlayer.cs
7. Added platform checks for clipboard operations in InstructorAdminPanel.cs

## Steps to Complete (Requires Repository Admin Access)

### Option 1: Force Push (Recommended for Clean History)
```bash
# Checkout main branch
git checkout main

# Reset main to point to MAIN
git reset --hard origin/MAIN

# Force push to update remote main
git push origin main --force
```

### Option 2: Merge (Preserves History)
The merge has already been prepared locally:
```bash
# Checkout main branch
git checkout main

# Merge MAIN into main (already done locally, commit: 00af13a)
git merge origin/MAIN --allow-unrelated-histories

# Resolve conflicts (already resolved - accepted MAIN's version for better code quality)
# - unity/Scripts/Instructor/InstructorAdminPanel.cs
# - unity/Scripts/Instructor/InstructorVerification.cs
# - unity/Scripts/Networking/NetworkPlayer.cs

# Push the merge commit
git push origin main
```

### Delete MAIN Branch (After Syncing)
Once the main branch has been synced:
```bash
# Delete the remote MAIN branch
git push origin --delete MAIN
```

## Why This Can't Be Done Automatically
This task requires:
1. Force push capabilities to the default branch (main)
2. Permission to delete remote branches
3. These operations are restricted for security reasons and require repository admin access

## Verification
After completing these steps, verify:
1. `git log origin/main` shows all commits from MAIN
2. `git branch -r` does not list `origin/MAIN`
3. The GitHub repository only has one primary branch: `main`
