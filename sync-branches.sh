#!/bin/bash

# Script to sync main branch with MAIN branch and delete MAIN
# This script uses the GitHub REST API to update branch references

OWNER="nullisnull100-debug"
REPO="University-VR"
MAIN_BRANCH_SHA="44537ce87711ecca3e68447cebf6b2929e4babc2"

echo "=== Branch Sync Script ==="
echo "Repository: $OWNER/$REPO"
echo "Target SHA: $MAIN_BRANCH_SHA"
echo ""

# Check if GITHUB_TOKEN is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "ERROR: GITHUB_TOKEN environment variable is not set"
    echo "Please set it with: export GITHUB_TOKEN=your_token_here"
    exit 1
fi

echo "Step 1: Updating main branch to point to MAIN branch commit..."
response=$(curl -s -X PATCH \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  "https://api.github.com/repos/$OWNER/$REPO/git/refs/heads/main" \
  -d "{\"sha\":\"$MAIN_BRANCH_SHA\", \"force\": true}")

if echo "$response" | grep -q "\"ref\""; then
    echo "✓ Successfully updated main branch"
else
    echo "✗ Failed to update main branch"
    echo "Response: $response"
    exit 1
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
