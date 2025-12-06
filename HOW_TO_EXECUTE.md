# 🚀 How to Execute the Branch Sync

## Where to Run This

You need to run this on **your local computer** in a **terminal/command prompt**, NOT on GitHub's website.

## Step-by-Step Instructions

### Step 1: Open a Terminal

**On Windows:**
- Press `Win + R`, type `cmd` or `powershell`, press Enter
- Or search for "Terminal" or "Command Prompt" in Start Menu

**On Mac:**
- Press `Cmd + Space`, type "Terminal", press Enter
- Or go to Applications → Utilities → Terminal

**On Linux:**
- Press `Ctrl + Alt + T`
- Or search for "Terminal" in your applications

### Step 2: Clone or Navigate to the Repository

If you haven't cloned the repository yet:
```bash
# Clone the repository
git clone https://github.com/nullisnull100-debug/University-VR.git

# Navigate into it
cd University-VR
```

If you already have it cloned:
```bash
# Navigate to your local copy
cd /path/to/University-VR
```

### Step 3: Make Sure You're on the Right Branch

```bash
# Fetch the latest changes
git fetch origin

# Checkout the PR branch
git checkout copilot/delete-non-default-repo-branch

# Pull latest changes
git pull
```

### Step 4: Create a GitHub Personal Access Token

1. Go to: https://github.com/settings/tokens
2. Click **"Generate new token"** → **"Generate new token (classic)"**
3. Give it a name (e.g., "Branch Sync Script")
4. Select scope: **☑ repo** (Full control of private repositories)
5. Click **"Generate token"** at the bottom
6. **IMPORTANT:** Copy the token immediately (it won't be shown again)
   - It will look like: `ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxx`

### Step 5: Run the Script

In your terminal, run these commands:

```bash
# Set your GitHub token (replace with your actual token)
export GITHUB_TOKEN="ghp_paste_your_token_here"

# Make the script executable (if needed)
chmod +x sync-branches.sh

# Run the script
./sync-branches.sh
```

**On Windows (PowerShell):**
```powershell
# Set your GitHub token
$env:GITHUB_TOKEN = "ghp_paste_your_token_here"

# Run the script using bash (if you have Git Bash installed)
bash sync-branches.sh

# Or if you only have PowerShell, you'll need to install Git Bash first
```

### Step 6: Verify the Results

After the script completes successfully, verify:

```bash
# Fetch the latest changes
git fetch origin

# Check that main has the new commits
git log origin/main --oneline -10

# Verify MAIN branch is deleted (should return nothing)
git branch -r | grep MAIN
```

## Expected Output

When you run `./sync-branches.sh`, you should see:

```
=== Branch Sync Script ===
Repository: nullisnull100-debug/University-VR
Target SHA: 44537ce87711ecca3e68447cebf6b2929e4babc2

Step 1: Updating main branch to point to MAIN branch commit...
✓ Successfully updated main branch (HTTP 200)

Verifying main branch update...
✓ Verification successful: main branch now points to 44537ce87711ecca3e68447cebf6b2929e4babc2

Step 2: Deleting MAIN branch...
✓ Successfully deleted MAIN branch (HTTP 204 - No Content)

=== Branch Sync Complete ===
✓ main branch now points to: 44537ce87711ecca3e68447cebf6b2929e4babc2
✓ MAIN branch has been deleted
```

## Troubleshooting

### "command not found: ./sync-branches.sh"
- Make sure you're in the right directory: `pwd` should show the University-VR path
- Make sure the script is executable: `chmod +x sync-branches.sh`

### "GITHUB_TOKEN environment variable is not set"
- You need to export the token first: `export GITHUB_TOKEN="your_token"`
- Make sure you copied the token correctly from GitHub

### "Permission denied" or "403 Forbidden"
- Your token needs the `repo` scope
- Make sure you have admin/write access to the repository
- Check if your token has expired

### "Branch not found" or "404"
- The MAIN branch might have already been deleted
- Run `git ls-remote --heads origin` to see available branches

## Alternative: GitHub Web Interface

If you're not comfortable with the terminal, you can also do this via GitHub's web interface:

1. Go to your repository on GitHub: https://github.com/nullisnull100-debug/University-VR
2. Click on **"Settings"** tab
3. Click on **"Branches"** in the left sidebar
4. This requires repository admin access to force-push branches

However, the script is the **recommended and easier way** to do this.

## Summary

**WHERE:** Your local computer's terminal/command prompt  
**WHAT:** Run `./sync-branches.sh` after setting `GITHUB_TOKEN`  
**WHY:** To sync the main branch with MAIN and delete the MAIN branch  

The script cannot run on GitHub itself - it needs your local terminal where you have your GitHub access token.
