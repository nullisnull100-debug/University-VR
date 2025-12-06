# Branch Sync Instructions

## Summary
The `main` branch (default) needs to be synced with the `MAIN` branch, and then the `MAIN` branch should be deleted.

## Current Status
- **Default branch**: `main` (lowercase)
- **Branch to sync from**: `MAIN` (uppercase)
- **Commits ahead**: MAIN has 34 commits ahead of main
- **MAIN branch HEAD SHA**: `44537ce87711ecca3e68447cebf6b2929e4babc2`

## Changes in MAIN Branch
The MAIN branch includes:
1. Removed root-level .cs files (Scripts-classroom.cs, Whiteboardinteractable.cs, avatarsimple.cs, demoui.cs)
2. Added UPDATE_PLAN.md documentation
3. Added unity/README.md for plug-and-play setup
4. Added unity/Scripts/UniversityVR.asmdef assembly definition
5. Fixed deprecated crypto API (RNGCryptoServiceProvider -> RandomNumberGenerator.Create())
6. Added material cleanup to prevent memory leaks in NetworkPlayer.cs
7. Added platform checks for clipboard operations in InstructorAdminPanel.cs

## Automated Script Available
An automated script `sync-branches.sh` has been created that uses the GitHub REST API to perform the sync and delete operations. See the "Using the Automated Script" section below.

## Steps to Complete (Requires Repository Admin Access)

### Option 1: Use GitHub REST API (Automated Script)
Run the provided script:
```bash
# Set your GitHub personal access token
export GITHUB_TOKEN="your_github_token_here"

# Run the sync script
./sync-branches.sh
```

The script will:
1. Update the `main` branch reference to point to `44537ce87711ecca3e68447cebf6b2929e4babc2` (MAIN branch HEAD)
2. Delete the `MAIN` branch

### Option 2: Force Push (Manual - Clean History)
```bash
# Checkout main branch
git checkout main

# Reset main to point to MAIN
git reset --hard origin/MAIN

# Force push to update remote main
git push origin main --force
```

### Option 3: Merge (Manual - Preserves History)
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

## Using the Automated Script

### Prerequisites
1. Repository admin access or permissions to:
   - Force push to the `main` branch
   - Delete remote branches
2. GitHub Personal Access Token with `repo` scope

### Steps
1. Create a Personal Access Token:
   - Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Click "Generate new token" (classic)
   - Select scope: `repo` (Full control of private repositories)
   - Copy the generated token

2. Run the script:
```bash
# Navigate to your local repository directory
cd /path/to/University-VR
export GITHUB_TOKEN="your_token_here"
./sync-branches.sh
```

## Verification
After completing these steps, verify:
1. `git fetch origin`
2. `git log origin/main --oneline -5` shows commits from MAIN branch
3. `git branch -r | grep MAIN` returns nothing (branch deleted)
4. The GitHub repository branches page shows only `main`
