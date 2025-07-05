#!/bin/bash

# =================================================================================================
# Verify Projects Script
# =================================================================================================
# This script checks what projects exist in the API
# =================================================================================================

# Configuration
API_BASE_URL="${1:-http://localhost:5001}"
API_ENDPOINT="$API_BASE_URL/api/v1/projects"
LOGIN_ENDPOINT="$API_BASE_URL/api/v1/auth/login"

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "=============================================="
echo "🔍 Projects Verification Script"
echo "=============================================="

# Login and get token
echo "🔐 Logging in..."
login_response=$(curl -s -X POST "$LOGIN_ENDPOINT" \
    -H "Content-Type: application/json" \
    -d '{"username": "admin@example.com", "password": "Admin123!"}')

JWT_TOKEN=$(echo "$login_response" | jq -r '.data.token // empty')

if [[ -z "$JWT_TOKEN" || "$JWT_TOKEN" == "null" ]]; then
    echo "❌ Failed to get JWT token"
    exit 1
fi

echo "✅ Successfully authenticated"

# Get all projects
echo ""
echo "📋 Fetching all projects..."
response=$(curl -s -X GET "$API_ENDPOINT" \
    -H "Authorization: Bearer $JWT_TOKEN")

# Parse response
total_count=$(echo "$response" | jq -r '.data.totalCount // 0')
projects=$(echo "$response" | jq -r '.data.projects // []')

echo -e "${GREEN}Found $total_count projects:${NC}"
echo ""

if [[ "$total_count" -gt 0 ]]; then
    echo "$projects" | jq -r '.[] | "  📄 \(.projectName) (ID: \(.id))"'
    
    echo ""
    echo "📊 Project Summary:"
    echo "$projects" | jq -r 'group_by(.status) | .[] | "  \(.[0].status): \(. | length) projects"'
    
    echo ""
    echo "🏗️ Total Capacity:"
    total_capacity=$(echo "$projects" | jq -r '[.[].totalCapacityKw // 0] | add')
    echo "  Total: ${total_capacity} kW"
    
    echo ""
    echo "⚡ Equipment Summary:"
    echo "$projects" | jq -r '[.[].equipmentDetails] | {
        "125kW Inverters": ([.[].inverter125kw // 0] | add),
        "80kW Inverters": ([.[].inverter80kw // 0] | add),
        "60kW Inverters": ([.[].inverter60kw // 0] | add),
        "40kW Inverters": ([.[].inverter40kw // 0] | add)
    } | to_entries | .[] | "  \(.key): \(.value)"'
else
    echo "  No projects found"
fi

echo ""
echo "=============================================="
