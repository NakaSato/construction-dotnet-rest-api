#!/bin/bash

# Comprehensive script to test ALL Project API endpoints
# Solar Project Management API - Complete Test Suite

echo "🌞 Solar Project API Comprehensive Test Suite"
echo "=============================================="

# API Configuration
API_BASE_URL="http://localhost:5002"
BASE_ENDPOINT="/api/v1/projects"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# Global variables
TOKEN=""
PROJECT_ID=""
MANAGER_ID=""

# Function to print section headers
print_section() {
    echo ""
    echo -e "${BOLD}${BLUE}═══════════════════════════════════════${NC}"
    echo -e "${BOLD}${BLUE}🔧 $1${NC}"
    echo -e "${BOLD}${BLUE}═══════════════════════════════════════${NC}"
}

# Function to print test results
print_result() {
    local status=$1
    local endpoint=$2
    local description=$3
    
    case $status in
        2*)
            echo -e "${GREEN}✅ SUCCESS${NC} - ${BOLD}$endpoint${NC} - $description"
            ;;
        4*)
            echo -e "${YELLOW}⚠️  CLIENT ERROR${NC} - ${BOLD}$endpoint${NC} - $description (Status: $status)"
            ;;
        5*)
            echo -e "${RED}❌ SERVER ERROR${NC} - ${BOLD}$endpoint${NC} - $description (Status: $status)"
            ;;
        *)
            echo -e "${PURPLE}❓ UNKNOWN${NC} - ${BOLD}$endpoint${NC} - $description (Status: $status)"
            ;;
    esac
}

# Function to make API call and extract status
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo -e "${CYAN}🔄 Testing: ${BOLD}$method $endpoint${NC} - $description"
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}\n" -X $method "${API_BASE_URL}$endpoint" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $TOKEN" \
            -d "$data")
    else
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}\n" -X $method "${API_BASE_URL}$endpoint" \
            -H "Authorization: Bearer $TOKEN")
    fi
    
    http_status=$(echo "$response" | grep "HTTP_STATUS:" | cut -d: -f2)
    response_body=$(echo "$response" | sed '/HTTP_STATUS:/d')
    
    print_result "$http_status" "$method $endpoint" "$description"
    
    # Show response body for successful operations or detailed errors
    if [[ $http_status =~ ^2 ]] || [[ $http_status =~ ^4 ]]; then
        if command -v jq &> /dev/null; then
            echo "$response_body" | jq '.' 2>/dev/null | head -10
        else
            echo "$response_body" | head -5
        fi
        echo ""
    fi
    
    echo "$http_status:$response_body"
}

# Authentication
print_section "AUTHENTICATION"

echo -e "${CYAN}🔐 Authenticating with test_admin...${NC}"

login_response=$(curl -s -X POST "${API_BASE_URL}/api/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "test_admin",
        "password": "Admin123!"
    }')

TOKEN=$(echo $login_response | jq -r '.data.token // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo -e "${RED}❌ Authentication failed${NC}"
    echo "Response: $login_response"
    exit 1
fi

echo -e "${GREEN}✅ Authentication successful${NC}"
echo -e "${CYAN}Token: ${TOKEN:0:20}...${NC}"

# Get Manager ID
echo -e "${CYAN}🔍 Finding project manager...${NC}"

users_response=$(curl -s -H "Authorization: Bearer $TOKEN" "${API_BASE_URL}/api/v1/users?pageSize=50")
MANAGER_ID=$(echo $users_response | jq -r '.data.items[]? | select(.roleName == "Manager" and .isActive == true) | .userId' 2>/dev/null | head -1)

if [ -z "$MANAGER_ID" ] || [ "$MANAGER_ID" = "null" ]; then
    MANAGER_ID=$(echo $users_response | jq -r '.data.items[]? | select(.roleName == "Admin" and .isActive == true) | .userId' 2>/dev/null | head -1)
fi

echo -e "${GREEN}✅ Project manager found: $MANAGER_ID${NC}"

# Test 1: GET All Projects
print_section "TEST 1: GET ALL PROJECTS"

result=$(api_call "GET" "$BASE_ENDPOINT" "" "Get all projects (default pagination)")
result=$(api_call "GET" "$BASE_ENDPOINT?pageSize=5" "" "Get projects with custom page size")
result=$(api_call "GET" "$BASE_ENDPOINT?pageNumber=1&pageSize=3" "" "Get projects with pagination")
result=$(api_call "GET" "$BASE_ENDPOINT?status=Active" "" "Filter projects by status")

# Test 2: CREATE Project
print_section "TEST 2: CREATE PROJECT"

timestamp=$(date +%Y%m%d_%H%M%S)
create_data="{
    \"projectName\": \"API Test Solar Project - $timestamp\",
    \"address\": \"123 Test Solar Street, API City, State 12345\",
    \"clientInfo\": \"API Test Corp - Contact: John Doe (555-123-4567)\",
    \"startDate\": \"2025-07-01T00:00:00Z\",
    \"estimatedEndDate\": \"2025-12-31T00:00:00Z\",
    \"projectManagerId\": \"$MANAGER_ID\",
    \"team\": \"E1\",
    \"connectionType\": \"HV\",
    \"connectionNotes\": \"High voltage connection for industrial installation\",
    \"totalCapacityKw\": 250.5,
    \"pvModuleCount\": 500,
    \"equipmentDetails\": {
        \"inverter125kw\": 2,
        \"inverter80kw\": 1,
        \"inverter60kw\": 0,
        \"inverter40kw\": 1
    },
    \"ftsValue\": 8,
    \"revenueValue\": 3,
    \"pqmValue\": 2,
    \"locationCoordinates\": {
        \"latitude\": 13.7563,
        \"longitude\": 100.5018
    }
}"

result=$(api_call "POST" "$BASE_ENDPOINT" "$create_data" "Create new solar project")

# Extract project ID from response
if [[ $result =~ ^2 ]]; then
    response_body=$(echo "$result" | cut -d: -f2-)
    PROJECT_ID=$(echo "$response_body" | jq -r '.data.projectId // empty' 2>/dev/null)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        echo -e "${GREEN}✅ Project created with ID: $PROJECT_ID${NC}"
    fi
fi

# Test 3: GET Specific Project
print_section "TEST 3: GET SPECIFIC PROJECT"

if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
    result=$(api_call "GET" "$BASE_ENDPOINT/$PROJECT_ID" "" "Get project by ID")
else
    echo -e "${YELLOW}⚠️  Skipping GET specific project - no project ID available${NC}"
fi

# Test 4: UPDATE Project (PUT)
print_section "TEST 4: UPDATE PROJECT (PUT)"

if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
    update_data="{
        \"projectName\": \"UPDATED API Test Solar Project - $timestamp\",
        \"address\": \"456 Updated Solar Avenue, API City, State 12345\",
        \"clientInfo\": \"Updated API Test Corp - Contact: Jane Smith (555-987-6543)\",
        \"status\": \"InProgress\",
        \"startDate\": \"2025-07-01T00:00:00Z\",
        \"estimatedEndDate\": \"2025-11-30T00:00:00Z\",
        \"projectManagerId\": \"$MANAGER_ID\"
    }"
    
    result=$(api_call "PUT" "$BASE_ENDPOINT/$PROJECT_ID" "$update_data" "Update entire project")
else
    echo -e "${YELLOW}⚠️  Skipping PUT update - no project ID available${NC}"
fi

# Test 5: PARTIAL UPDATE Project (PATCH)
print_section "TEST 5: PARTIAL UPDATE PROJECT (PATCH)"

if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
    patch_data="{
        \"projectName\": \"PATCHED Solar Project - $timestamp\",
        \"status\": \"OnHold\"
    }"
    
    result=$(api_call "PATCH" "$BASE_ENDPOINT/$PROJECT_ID" "$patch_data" "Partial update project")
else
    echo -e "${YELLOW}⚠️  Skipping PATCH update - no project ID available${NC}"
fi

# Test 6: Error Cases
print_section "TEST 6: ERROR HANDLING TESTS"

result=$(api_call "GET" "$BASE_ENDPOINT/00000000-0000-0000-0000-000000000000" "" "Get non-existent project")
result=$(api_call "POST" "$BASE_ENDPOINT" '{"projectName": ""}' "Create project with invalid data")
result=$(api_call "PUT" "$BASE_ENDPOINT/00000000-0000-0000-0000-000000000000" '{"projectName": "Test"}' "Update non-existent project")

# Test 7: Query Parameters
print_section "TEST 7: ADVANCED QUERY TESTS"

result=$(api_call "GET" "$BASE_ENDPOINT?search=Solar" "" "Search projects by name")
result=$(api_call "GET" "$BASE_ENDPOINT?status=Planning&pageSize=2" "" "Filter and paginate")
result=$(api_call "GET" "$BASE_ENDPOINT?managerId=$MANAGER_ID" "" "Filter by manager")

# Test 8: DELETE Project (if created)
print_section "TEST 8: DELETE PROJECT"

if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
    echo -e "${YELLOW}🗑️  About to delete test project: $PROJECT_ID${NC}"
    read -p "Delete the test project? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        result=$(api_call "DELETE" "$BASE_ENDPOINT/$PROJECT_ID" "" "Delete test project")
        
        # Verify deletion
        result=$(api_call "GET" "$BASE_ENDPOINT/$PROJECT_ID" "" "Verify project deletion")
    else
        echo -e "${CYAN}ℹ️  Test project preserved: $PROJECT_ID${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  No project to delete${NC}"
fi

# Summary
print_section "TEST SUMMARY"

echo -e "${BOLD}🌞 Solar Project API Test Suite Complete${NC}"
echo ""
echo -e "${BLUE}📊 Endpoints Tested:${NC}"
echo -e "   • ${CYAN}GET $BASE_ENDPOINT${NC} - List all projects"
echo -e "   • ${CYAN}GET $BASE_ENDPOINT/{id}${NC} - Get specific project"
echo -e "   • ${CYAN}POST $BASE_ENDPOINT${NC} - Create new project"
echo -e "   • ${CYAN}PUT $BASE_ENDPOINT/{id}${NC} - Update entire project"
echo -e "   • ${CYAN}PATCH $BASE_ENDPOINT/{id}${NC} - Partial update project"
echo -e "   • ${CYAN}DELETE $BASE_ENDPOINT/{id}${NC} - Delete project"
echo ""
echo -e "${BLUE}🔧 Features Tested:${NC}"
echo -e "   • Authentication & Authorization"
echo -e "   • Solar-specific field creation"
echo -e "   • Equipment details (inverters)"
echo -e "   • Location coordinates"
echo -e "   • Business values (FTS, Revenue, PQM)"
echo -e "   • Pagination & Filtering"
echo -e "   • Error handling"
echo ""

if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
    echo -e "${GREEN}✅ Test project created: $PROJECT_ID${NC}"
    echo -e "${CYAN}🔗 View at: ${API_BASE_URL}$BASE_ENDPOINT/$PROJECT_ID${NC}"
fi

echo ""
echo -e "${BOLD}${GREEN}🎉 All tests completed!${NC}"
