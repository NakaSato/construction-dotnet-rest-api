#!/bin/bash

# =================================================================================================
# Delete First 5 Projects Script (Safe Testing)
# =================================================================================================
# This script deletes only the FIRST 5 projects from the Solar Projects API
# Use this for testing the deletion functionality safely
# =================================================================================================

# Configuration
API_BASE_URL="${1:-http://localhost:5001}"
API_ENDPOINT="$API_BASE_URL/api/v1/projects"
LOGIN_ENDPOINT="$API_BASE_URL/api/v1/auth/login"
MAX_DELETIONS=5

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

echo "=============================================="
echo -e "${YELLOW}🧪 Safe Delete Test (First $MAX_DELETIONS Projects)${NC}"
echo "=============================================="
echo -e "${CYAN}Target API: $API_BASE_URL${NC}"
echo ""

# Step 1: Login and get JWT token
echo "🔐 Authenticating..."
login_response=$(curl -s -X POST "$LOGIN_ENDPOINT" \
    -H "Content-Type: application/json" \
    -d '{"username": "admin@example.com", "password": "Admin123!"}')

JWT_TOKEN=$(echo "$login_response" | jq -r '.data.token // .token // .accessToken // empty')

if [[ -z "$JWT_TOKEN" || "$JWT_TOKEN" == "null" ]]; then
    echo -e "${RED}❌ Failed to authenticate${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Authentication successful${NC}"

# Step 2: Fetch all projects
echo "📋 Fetching projects..."
projects_response=$(curl -s -X GET "$API_ENDPOINT" \
    -H "Authorization: Bearer $JWT_TOKEN")

# Extract project IDs (try different response structures)
project_ids=()
if echo "$projects_response" | jq -e '.data.items' >/dev/null 2>&1; then
    project_ids=($(echo "$projects_response" | jq -r '.data.items[].projectId // .data.items[].id // empty'))
elif echo "$projects_response" | jq -e '.data.projects' >/dev/null 2>&1; then
    project_ids=($(echo "$projects_response" | jq -r '.data.projects[].projectId // .data.projects[].id // empty'))
elif echo "$projects_response" | jq -e '.data' >/dev/null 2>&1; then
    project_ids=($(echo "$projects_response" | jq -r '.data[].projectId // .data[].id // empty'))
elif echo "$projects_response" | jq -e '.[0].projectId' >/dev/null 2>&1; then
    project_ids=($(echo "$projects_response" | jq -r '.[].projectId // .[].id // empty'))
fi

total_projects=${#project_ids[@]}

if [[ $total_projects -eq 0 ]]; then
    echo -e "${YELLOW}ℹ️  No projects found${NC}"
    exit 0
fi

# Limit to first N projects for safety
projects_to_delete=("${project_ids[@]:0:$MAX_DELETIONS}")
delete_count=${#projects_to_delete[@]}

echo -e "${CYAN}Total projects found: $total_projects${NC}"
echo -e "${YELLOW}Will delete first $delete_count projects (safe test)${NC}"
echo ""

# Show which projects will be deleted
echo "Projects to be deleted:"
for i in "${!projects_to_delete[@]}"; do
    project_id="${projects_to_delete[$i]}"
    echo -e "  $((i+1)). Project ID: $project_id"
done

echo ""
read -p "Continue with deletion of these $delete_count projects? (y/N): " confirm

if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
    echo -e "${YELLOW}❌ Operation cancelled${NC}"
    exit 0
fi

# Step 3: Delete projects
echo ""
echo "🗑️  Starting deletion..."
echo "=============================================="

deleted_count=0
failed_count=0

for project_id in "${projects_to_delete[@]}"; do
    if [[ -z "$project_id" || "$project_id" == "null" ]]; then
        continue
    fi
    
    echo -n "Deleting project ID $project_id... "
    
    delete_response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X DELETE "$API_ENDPOINT/$project_id" \
        -H "Authorization: Bearer $JWT_TOKEN")
    
    http_status=$(echo "$delete_response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    
    if [[ "$http_status" -eq 200 || "$http_status" -eq 204 || "$http_status" -eq 404 ]]; then
        echo -e "${GREEN}✅ Success${NC}"
        ((deleted_count++))
    else
        echo -e "${RED}❌ Failed (HTTP $http_status)${NC}"
        ((failed_count++))
    fi
    
    sleep 0.1
done

# Summary
echo ""
echo "=============================================="
echo -e "${BOLD}📊 Test Deletion Summary${NC}"
echo "=============================================="
echo -e "${GREEN}✅ Successfully deleted: $deleted_count projects${NC}"
echo -e "${RED}❌ Failed to delete: $failed_count projects${NC}"
echo ""
echo -e "${BLUE}Test completed. Use delete-all-projects.sh for full deletion.${NC}"
