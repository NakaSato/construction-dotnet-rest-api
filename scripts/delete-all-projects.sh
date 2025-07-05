#!/bin/bash

# =================================================================================================
# Delete All Projects Script
# =================================================================================================
# This script deletes ALL projects from the Solar Projects API
# WARNING: This action is IRREVERSIBLE - use with extreme caution!
# =================================================================================================

# Configuration
API_BASE_URL="${1:-http://localhost:5001}"
API_ENDPOINT="$API_BASE_URL/api/v1/projects"
LOGIN_ENDPOINT="$API_BASE_URL/api/v1/auth/login"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# Function to display usage
show_usage() {
    echo "Usage: $0 [API_BASE_URL] [--confirm]"
    echo ""
    echo "Options:"
    echo "  API_BASE_URL  Base URL of the API (default: http://localhost:5001)"
    echo "  --confirm     Skip confirmation prompt (DANGEROUS!)"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Delete from localhost with confirmation"
    echo "  $0 --confirm                         # Delete from localhost without confirmation"
    echo "  $0 http://api.example.com --confirm  # Delete from custom API without confirmation"
}

# Parse arguments
SKIP_CONFIRMATION=false
FINAL_API_BASE_URL="$API_BASE_URL"

while [[ $# -gt 0 ]]; do
    case $1 in
        --confirm)
            SKIP_CONFIRMATION=true
            shift
            ;;
        --help|-h)
            show_usage
            exit 0
            ;;
        http*)
            FINAL_API_BASE_URL="$1"
            shift
            ;;
        *)
            # If it's not a flag and doesn't start with http, it might be a localhost URL
            if [[ "$1" != --* ]]; then
                FINAL_API_BASE_URL="$1"
            fi
            shift
            ;;
    esac
done

# Update endpoints with final API URL
API_ENDPOINT="$FINAL_API_BASE_URL/api/v1/projects"
LOGIN_ENDPOINT="$FINAL_API_BASE_URL/api/v1/auth/login"

echo "=============================================="
echo -e "${RED}${BOLD}⚠️  DANGER: DELETE ALL PROJECTS${NC}"
echo "=============================================="
echo -e "${YELLOW}Target API: $FINAL_API_BASE_URL${NC}"
echo -e "${CYAN}Skip confirmation: $SKIP_CONFIRMATION${NC}"
echo ""

# Safety confirmation
if [[ "$SKIP_CONFIRMATION" != "true" ]]; then
    echo -e "${RED}${BOLD}WARNING: This will DELETE ALL PROJECTS permanently!${NC}"
    echo -e "${RED}This action CANNOT be undone!${NC}"
    echo ""
    read -p "Are you absolutely sure you want to continue? (type 'DELETE ALL' to confirm): " confirmation
    
    if [[ "$confirmation" != "DELETE ALL" ]]; then
        echo -e "${YELLOW}❌ Operation cancelled by user${NC}"
        exit 0
    fi
    
    echo ""
    echo -e "${YELLOW}⚠️  Last chance to abort!${NC}"
    read -p "Type 'YES DELETE EVERYTHING' to proceed: " final_confirmation
    
    if [[ "$final_confirmation" != "YES DELETE EVERYTHING" ]]; then
        echo -e "${YELLOW}❌ Operation cancelled by user${NC}"
        exit 0
    fi
fi

echo ""
echo -e "${BLUE}🚀 Starting deletion process...${NC}"

# Step 1: Login and get JWT token
echo "🔐 Authenticating..."
login_response=$(curl -s -X POST "$LOGIN_ENDPOINT" \
    -H "Content-Type: application/json" \
    -d '{"username": "admin@example.com", "password": "Admin123!"}')

# Try different token paths in case API structure varies
JWT_TOKEN=$(echo "$login_response" | jq -r '.data.token // .token // .accessToken // empty')

if [[ -z "$JWT_TOKEN" || "$JWT_TOKEN" == "null" ]]; then
    echo -e "${RED}❌ Failed to authenticate. Login response:${NC}"
    echo "$login_response" | jq . 2>/dev/null || echo "$login_response"
    exit 1
fi

echo -e "${GREEN}✅ Authentication successful${NC}"

# Step 2: Fetch all projects
echo "📋 Fetching all projects..."
projects_response=$(curl -s -X GET "$API_ENDPOINT" \
    -H "Authorization: Bearer $JWT_TOKEN")

# Handle different response structures
project_ids=()

# Try to extract project IDs from different possible response structures
if echo "$projects_response" | jq -e '.data.items' >/dev/null 2>&1; then
    # Structure: { data: { items: [...] } }
    project_ids=($(echo "$projects_response" | jq -r '.data.items[].projectId // .data.items[].id // empty'))
elif echo "$projects_response" | jq -e '.data.projects' >/dev/null 2>&1; then
    # Structure: { data: { projects: [...] } }
    project_ids=($(echo "$projects_response" | jq -r '.data.projects[].projectId // .data.projects[].id // empty'))
elif echo "$projects_response" | jq -e '.data' >/dev/null 2>&1; then
    # Structure: { data: [...] }
    project_ids=($(echo "$projects_response" | jq -r '.data[].projectId // .data[].id // empty'))
elif echo "$projects_response" | jq -e '.[0].projectId' >/dev/null 2>&1; then
    # Structure: [...]
    project_ids=($(echo "$projects_response" | jq -r '.[].projectId // .[].id // empty'))
else
    echo -e "${RED}❌ Could not parse projects response. Response structure:${NC}"
    echo "$projects_response" | jq . 2>/dev/null || echo "$projects_response"
    exit 1
fi

project_count=${#project_ids[@]}

if [[ $project_count -eq 0 ]]; then
    echo -e "${YELLOW}ℹ️  No projects found to delete${NC}"
    exit 0
fi

echo -e "${CYAN}Found $project_count projects to delete${NC}"

# Step 3: Delete each project
echo ""
echo "🗑️  Starting deletion process..."
echo "=============================================="

deleted_count=0
failed_count=0
failed_ids=()

for project_id in "${project_ids[@]}"; do
    if [[ -z "$project_id" || "$project_id" == "null" ]]; then
        continue
    fi
    
    echo -n "Deleting project ID $project_id... "
    
    delete_response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X DELETE "$API_ENDPOINT/$project_id" \
        -H "Authorization: Bearer $JWT_TOKEN")
    
    # Extract HTTP status
    http_status=$(echo "$delete_response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    response_body=$(echo "$delete_response" | sed 's/HTTP_STATUS:[0-9]*$//')
    
    if [[ "$http_status" -eq 200 || "$http_status" -eq 204 || "$http_status" -eq 404 ]]; then
        echo -e "${GREEN}✅ Success${NC}"
        ((deleted_count++))
    else
        echo -e "${RED}❌ Failed (HTTP $http_status)${NC}"
        ((failed_count++))
        failed_ids+=("$project_id")
        
        # Log the error response
        if [[ -n "$response_body" ]]; then
            echo "   Error: $response_body"
        fi
    fi
    
    # Small delay to avoid overwhelming the API
    sleep 0.1
done

# Step 4: Summary
echo ""
echo "=============================================="
echo -e "${BOLD}📊 Deletion Summary${NC}"
echo "=============================================="
echo -e "${GREEN}✅ Successfully deleted: $deleted_count projects${NC}"

if [[ $failed_count -gt 0 ]]; then
    echo -e "${RED}❌ Failed to delete: $failed_count projects${NC}"
    echo -e "${RED}Failed project IDs: ${failed_ids[*]}${NC}"
fi

echo ""

# Step 5: Verify deletion
echo "🔍 Verifying deletion..."
verification_response=$(curl -s -X GET "$API_ENDPOINT" \
    -H "Authorization: Bearer $JWT_TOKEN")

remaining_count=0
if echo "$verification_response" | jq -e '.data.totalCount' >/dev/null 2>&1; then
    remaining_count=$(echo "$verification_response" | jq -r '.data.totalCount // 0')
elif echo "$verification_response" | jq -e '.data.items' >/dev/null 2>&1; then
    remaining_count=$(echo "$verification_response" | jq -r '.data.items | length')
elif echo "$verification_response" | jq -e '.data' >/dev/null 2>&1; then
    remaining_count=$(echo "$verification_response" | jq -r '.data | length')
elif echo "$verification_response" | jq -e 'length' >/dev/null 2>&1; then
    remaining_count=$(echo "$verification_response" | jq -r 'length')
fi

echo -e "${CYAN}Projects remaining in system: $remaining_count${NC}"

if [[ $remaining_count -eq 0 ]]; then
    echo -e "${GREEN}${BOLD}🎉 All projects successfully deleted!${NC}"
else
    echo -e "${YELLOW}⚠️  Some projects may still remain in the system${NC}"
fi

echo ""
echo -e "${BLUE}Deletion process completed.${NC}"
