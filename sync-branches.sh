#!/bin/bash

# Script to sync main branch with MAIN branch and delete MAIN
# This script uses the GitHub REST API to update branch references
#
# Usage:
#   ./sync-branches.sh
#
# Environment variables (optional):
#   GITHUB_TOKEN  - Required: GitHub Personal Access Token with 'repo' scope
#   REPO_OWNER    - Optional: Repository owner (default: nullisnull100-debug)
#   REPO_NAME     - Optional: Repository name (default: University-VR)
#   TARGET_SHA    - Optional: Target commit SHA (default: 44537ce87711ecca3e68447cebf6b2929e4babc2)
#
# Example:
#   export GITHUB_TOKEN="ghp_xxxxxxxxxxxx"
#   ./sync-branches.sh

# Configuration - can be overridden via environment variables
OWNER="${REPO_OWNER:-nullisnull100-debug}"
REPO="${REPO_NAME:-University-VR}"
MAIN_BRANCH_SHA="${TARGET_SHA:-44537ce87711ecca3e68447cebf6b2929e4babc2}"

echo "=== Branch Sync Script ==="
echo "Repository: $OWNER/$REPO"
echo "Target SHA: $MAIN_BRANCH_SHA"
echo ""

# Check if GITHUB_TOKEN is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "ERROR: GITHUB_TOKEN environment variable is not set"
    echo ""
    echo "Usage:"
    echo "  export GITHUB_TOKEN='your_token_here'"
    echo "  ./sync-branches.sh"
    echo ""
    echo "To create a token, visit: https://github.com/settings/tokens"
    echo "Required scope: repo (Full control of private repositories)"
    exit 1
fi

echo "Step 1: Updating main branch to point to MAIN branch commit..."
update_response=$(curl -s -w "\n%{http_code}" -X PATCH \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  "https://api.github.com/repos/$OWNER/$REPO/git/refs/heads/main" \
  -d "{\"sha\":\"$MAIN_BRANCH_SHA\", \"force\": true}")

http_code=$(echo "$update_response" | tail -n1)
response_body=$(echo "$update_response" | sed '$d')

if [ "$http_code" = "200" ]; then
    # Verify the response contains the expected ref
    if echo "$response_body" | grep -q "\"ref\".*\"refs/heads/main\""; then
        echo "✓ Successfully updated main branch (HTTP 200)"
    else
        echo "⚠ Unexpected response format (HTTP 200 but missing ref)"
        echo "Response: $response_body"
        exit 1
    fi
elif [ "$http_code" = "422" ]; then
    echo "✗ Failed to update main branch - Validation failed (HTTP 422)"
    echo "Response: $response_body"
    exit 1
else
    echo "✗ Failed to update main branch (HTTP $http_code)"
    echo "Response: $response_body"
    exit 1
fi

# Verification step: Confirm the update was successful
echo ""
echo "Verifying main branch update..."
verify_response=$(curl -s -w "\n%{http_code}" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  "https://api.github.com/repos/$OWNER/$REPO/git/refs/heads/main")

verify_http_code=$(echo "$verify_response" | tail -n1)
verify_body=$(echo "$verify_response" | sed '$d')

if [ "$verify_http_code" = "200" ]; then
    current_sha=$(echo "$verify_body" | grep -o '"sha"[[:space:]]*:[[:space:]]*"[^"]*"' | head -1 | cut -d'"' -f4)
    if [ "$current_sha" = "$MAIN_BRANCH_SHA" ]; then
        echo "✓ Verification successful: main branch now points to $current_sha"
    else
        echo "✗ Verification failed: main branch points to $current_sha instead of $MAIN_BRANCH_SHA"
        exit 1
    fi
else
    echo "⚠ Could not verify update (HTTP $verify_http_code)"
fi

echo ""
echo "Step 2: Deleting MAIN branch..."
delete_response=$(curl -s -w "\n%{http_code}" -X DELETE \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  "https://api.github.com/repos/$OWNER/$REPO/git/refs/heads/MAIN")

http_code=$(echo "$delete_response" | tail -n1)
response_body=$(echo "$delete_response" | sed '$d')

if [ "$http_code" = "204" ]; then
    echo "✓ Successfully deleted MAIN branch (HTTP 204 - No Content)"
elif [ "$http_code" = "404" ]; then
    echo "⚠ MAIN branch was already deleted or doesn't exist (HTTP 404)"
else
    echo "✗ Failed to delete MAIN branch (HTTP $http_code)"
    echo "Response: $response_body"
    exit 1
fi

echo ""
echo "=== Branch Sync Complete ==="
echo "✓ main branch now points to: $MAIN_BRANCH_SHA"
echo "✓ MAIN branch has been deleted"
echo ""
echo "Verification commands:"
echo "  git fetch origin"
echo "  git log origin/main --oneline -5"
echo "  git branch -r | grep MAIN  # Should return nothing"
