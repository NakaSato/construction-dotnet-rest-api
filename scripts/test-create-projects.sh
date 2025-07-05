#!/bin/bash

# =================================================================================================
# Test Projects Creation Script (First 3 Projects Only)
# =================================================================================================
# This script creates a test subset of projects for validation
# =================================================================================================

set -e

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

# Check if projects.json exists
if [[ ! -f "$SCRIPT_DIR/projects.json" ]]; then
    echo "❌ projects.json not found in script directory"
    exit 1
fi

# Create test file with first 3 projects
echo "🔄 Creating test file with first 3 projects..."
jq '.[0:3]' "$SCRIPT_DIR/projects.json" > "$SCRIPT_DIR/test-projects.json"

echo "📋 Test projects to be created:"
jq -r '.[] | "  - \(.projectName) (ID: \(.projectId))"' "$SCRIPT_DIR/test-projects.json"

echo ""
echo "🚀 Starting test creation..."

# Run the main script with test file
"$SCRIPT_DIR/create-projects-from-json.sh" "$SCRIPT_DIR/test-projects.json"

# Clean up test file
rm -f "$SCRIPT_DIR/test-projects.json"

echo ""
echo "✅ Test completed!"
