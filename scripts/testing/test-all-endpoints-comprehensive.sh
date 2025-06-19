#!/bin/bash

# Comprehensive API Endpoint Testing Script
# Tests all major endpoints in the Solar Projects API

echo "🌞 COMPREHENSIVE API ENDPOINT TESTING"
echo "====================================="
echo

# API Configuration
API_BASE="http://localhost:5002"
AUTH_ENDPOINT="$API_BASE/api/v1/auth/login"

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
DAILY_REPORT_ID=""
WORK_REQUEST_ID=""
USER_ID=""

# Function to print section headers
print_section() {
    echo -e "${PURPLE}${BOLD}"
    echo "═══════════════════════════════════════"
    echo "  $1"
    echo "═══════════════════════════════════════"
    echo -e "${NC}"
}

# Function to print test results
print_result() {
    local test_name="$1"
    local status="$2"
    local details="$3"
    
    if [ "$status" = "PASS" ]; then
        echo -e "${GREEN}✅ $test_name${NC}"
    elif [ "$status" = "FAIL" ]; then
        echo -e "${RED}❌ $test_name${NC}"
    elif [ "$status" = "WARN" ]; then
        echo -e "${YELLOW}⚠️  $test_name${NC}"
    else
        echo -e "${BLUE}ℹ️  $test_name${NC}"
    fi
    
    if [ -n "$details" ]; then
        echo -e "${CYAN}   $details${NC}"
    fi
}

# Function to make API call and check response
api_test() {
    local method="$1"
    local endpoint="$2"
    local data="$3"
    local test_name="$4"
    local expected_status="$5"
    local auth_required="$6"
    
    local url="$API_BASE$endpoint"
    local headers="Content-Type: application/json"
    
    if [ "$auth_required" = "true" ] && [ -n "$TOKEN" ]; then
        headers="$headers Authorization: Bearer $TOKEN"
    fi
    
    local response
    local status
    
    if [ "$method" = "GET" ]; then
        if [ "$auth_required" = "true" ]; then
            response=$(curl -s -w "STATUS:%{http_code}" -H "Authorization: Bearer $TOKEN" "$url")
        else
            response=$(curl -s -w "STATUS:%{http_code}" "$url")
        fi
    elif [ "$method" = "POST" ]; then
        if [ "$auth_required" = "true" ]; then
            response=$(curl -s -w "STATUS:%{http_code}" -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$data" "$url")
        else
            response=$(curl -s -w "STATUS:%{http_code}" -X POST -H "Content-Type: application/json" -d "$data" "$url")
        fi
    elif [ "$method" = "PUT" ]; then
        response=$(curl -s -w "STATUS:%{http_code}" -X PUT -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$data" "$url")
    elif [ "$method" = "PATCH" ]; then
        response=$(curl -s -w "STATUS:%{http_code}" -X PATCH -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$data" "$url")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "STATUS:%{http_code}" -X DELETE -H "Authorization: Bearer $TOKEN" "$url")
    fi
    
    status=$(echo "$response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
    response_body=$(echo "$response" | sed 's/STATUS:[0-9]*$//')
    
    if [ -z "$expected_status" ]; then
        expected_status="200"
    fi
    
    if [[ "$status" =~ ^(200|201|204)$ ]] && [[ "$expected_status" =~ ^(200|201|204)$ ]]; then
        print_result "$test_name" "PASS" "Status: $status"
        echo "$response_body"
        return 0
    elif [ "$status" = "$expected_status" ]; then
        print_result "$test_name" "PASS" "Status: $status (Expected)"
        return 0
    else
        print_result "$test_name" "FAIL" "Status: $status (Expected: $expected_status)"
        if [ ${#response_body} -lt 500 ]; then
            echo -e "${CYAN}   Response: $response_body${NC}"
        fi
        return 1
    fi
}

# Start testing
echo -e "${CYAN}🚀 Starting comprehensive API endpoint testing...${NC}"
echo -e "${CYAN}📅 $(date)${NC}"
echo

# ═══════════════════════════════════════
# HEALTH AND SYSTEM ENDPOINTS
# ═══════════════════════════════════════
print_section "HEALTH & SYSTEM ENDPOINTS"

api_test "GET" "/health" "" "Health Check" "200" "false"
api_test "GET" "/api/v1/debug/info" "" "Debug Info" "200" "false"
echo

# ═══════════════════════════════════════
# AUTHENTICATION
# ═══════════════════════════════════════
print_section "AUTHENTICATION"

echo -e "${CYAN}🔐 Testing admin login...${NC}"
login_response=$(curl -s -X POST "$AUTH_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}')

TOKEN=$(echo "$login_response" | jq -r '.data.token // empty' 2>/dev/null)

if [ -n "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
    print_result "Admin Login" "PASS" "Token acquired"
    echo -e "${CYAN}   Token: ${TOKEN:0:50}...${NC}"
else
    print_result "Admin Login" "FAIL" "Could not get token"
    echo -e "${RED}   Response: $login_response${NC}"
    echo
    echo -e "${RED}❌ Cannot continue without authentication token${NC}"
    exit 1
fi

# Test invalid login
echo -e "${CYAN}🔐 Testing invalid login...${NC}"
api_test "POST" "/api/v1/auth/login" '{"username":"invalid","password":"wrong"}' "Invalid Login" "400" "false"
echo

# ═══════════════════════════════════════
# USER ENDPOINTS
# ═══════════════════════════════════════
print_section "USER MANAGEMENT"

api_test "GET" "/api/v1/users" "" "Get All Users" "200" "true"
api_test "GET" "/api/v1/users?pageSize=3" "" "Get Users (Paginated)" "200" "true"

# Get a user ID for further testing
echo -e "${CYAN}🔍 Getting user ID for detailed tests...${NC}"
users_response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/users?pageSize=10")
USER_ID=$(echo "$users_response" | jq -r '.data.items[0].userId // empty' 2>/dev/null)

if [ -n "$USER_ID" ] && [ "$USER_ID" != "null" ]; then
    print_result "User ID Found" "PASS" "ID: ${USER_ID:0:8}..."
    api_test "GET" "/api/v1/users/$USER_ID" "" "Get User by ID" "200" "true"
else
    print_result "User ID Found" "WARN" "No users found for detailed testing"
fi
echo

# ═══════════════════════════════════════
# PROJECT ENDPOINTS
# ═══════════════════════════════════════
print_section "PROJECT MANAGEMENT"

api_test "GET" "/api/v1/projects" "" "Get All Projects" "200" "true"
api_test "GET" "/api/v1/projects?pageSize=5" "" "Get Projects (Paginated)" "200" "true"
api_test "GET" "/api/v1/projects?status=Active" "" "Get Active Projects" "200" "true"

# Create a new project
echo -e "${CYAN}🏗️ Creating test project...${NC}"
timestamp=$(date +%s)
project_data="{
  \"projectName\": \"Test Solar Project $timestamp\",
  \"address\": \"123 Test St, Test City, TS 12345\",
  \"clientInfo\": \"Test Client Corp\",
  \"startDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
  \"estimatedEndDate\": \"$(date -u -d '+90 days' +%Y-%m-%dT%H:%M:%SZ)\",
  \"projectManagerId\": \"$USER_ID\",
  \"team\": \"E1\",
  \"connectionType\": \"LV\",
  \"totalCapacityKw\": 100.0,
  \"pvModuleCount\": 200,
  \"equipmentDetails\": {
    \"inverter125kw\": 0,
    \"inverter80kw\": 1,
    \"inverter60kw\": 0,
    \"inverter40kw\": 0
  },
  \"ftsValue\": 5,
  \"revenueValue\": 1,
  \"pqmValue\": 0,
  \"locationCoordinates\": {
    \"latitude\": 14.5,
    \"longitude\": 101.0
  }
}"

create_response=$(curl -s -w "STATUS:%{http_code}" -X POST "$API_BASE/api/v1/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "$project_data")

create_status=$(echo "$create_response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
create_body=$(echo "$create_response" | sed 's/STATUS:[0-9]*$//')

if [[ "$create_status" =~ ^(200|201)$ ]]; then
    PROJECT_ID=$(echo "$create_body" | jq -r '.data.projectId // empty' 2>/dev/null)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_result "Create Project" "PASS" "ID: ${PROJECT_ID:0:8}..."
        
        # Test GET specific project
        api_test "GET" "/api/v1/projects/$PROJECT_ID" "" "Get Project by ID" "200" "true"
        
        # Test UPDATE project
        echo -e "${CYAN}📝 Testing project update...${NC}"
        update_data="{\"projectName\": \"Updated Test Solar Project $timestamp\", \"status\": \"Active\"}"
        api_test "PUT" "/api/v1/projects/$PROJECT_ID" "$update_data" "Update Project (PUT)" "200" "true"
        
        # Test PATCH project
        patch_data="{\"status\": \"InProgress\"}"
        api_test "PATCH" "/api/v1/projects/$PROJECT_ID" "$patch_data" "Update Project (PATCH)" "200" "true"
        
    else
        print_result "Create Project" "FAIL" "Could not extract project ID"
    fi
else
    print_result "Create Project" "FAIL" "Status: $create_status"
fi
echo

# ═══════════════════════════════════════
# DAILY REPORTS ENDPOINTS
# ═══════════════════════════════════════
print_section "DAILY REPORTS"

api_test "GET" "/api/v1/daily-reports" "" "Get All Daily Reports" "200" "true"
api_test "GET" "/api/v1/daily-reports?pageSize=3" "" "Get Daily Reports (Paginated)" "200" "true"

# Create a daily report if we have a project
if [ -n "$PROJECT_ID" ]; then
    echo -e "${CYAN}📋 Creating test daily report...${NC}"
    report_data="{
      \"projectId\": \"$PROJECT_ID\",
      \"reportDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
      \"workProgress\": \"Installed 50 solar panels\",
      \"challenges\": \"Weather delays\",
      \"nextDayPlan\": \"Continue installation\",
      \"safetyNotes\": \"All safety protocols followed\"
    }"
    
    report_response=$(curl -s -w "STATUS:%{http_code}" -X POST "$API_BASE/api/v1/daily-reports" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "$report_data")
    
    report_status=$(echo "$report_response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
    report_body=$(echo "$report_response" | sed 's/STATUS:[0-9]*$//')
    
    if [[ "$report_status" =~ ^(200|201)$ ]]; then
        DAILY_REPORT_ID=$(echo "$report_body" | jq -r '.data.dailyReportId // .data.id // empty' 2>/dev/null)
        print_result "Create Daily Report" "PASS" "ID: ${DAILY_REPORT_ID:0:8}..."
        
        if [ -n "$DAILY_REPORT_ID" ]; then
            api_test "GET" "/api/v1/daily-reports/$DAILY_REPORT_ID" "" "Get Daily Report by ID" "200" "true"
        fi
    else
        print_result "Create Daily Report" "FAIL" "Status: $report_status"
    fi
fi
echo

# ═══════════════════════════════════════
# WORK REQUESTS ENDPOINTS
# ═══════════════════════════════════════
print_section "WORK REQUESTS"

api_test "GET" "/api/v1/work-requests" "" "Get All Work Requests" "200" "true"
api_test "GET" "/api/v1/work-requests?pageSize=3" "" "Get Work Requests (Paginated)" "200" "true"

# Create a work request if we have a project
if [ -n "$PROJECT_ID" ]; then
    echo -e "${CYAN}🔧 Creating test work request...${NC}"
    work_request_data="{
      \"projectId\": \"$PROJECT_ID\",
      \"requestType\": \"Maintenance\",
      \"description\": \"Equipment inspection required\",
      \"priority\": \"Medium\",
      \"requestorComments\": \"Routine maintenance check\"
    }"
    
    wr_response=$(curl -s -w "STATUS:%{http_code}" -X POST "$API_BASE/api/v1/work-requests" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "$work_request_data")
    
    wr_status=$(echo "$wr_response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
    wr_body=$(echo "$wr_response" | sed 's/STATUS:[0-9]*$//')
    
    if [[ "$wr_status" =~ ^(200|201)$ ]]; then
        WORK_REQUEST_ID=$(echo "$wr_body" | jq -r '.data.workRequestId // .data.id // empty' 2>/dev/null)
        print_result "Create Work Request" "PASS" "ID: ${WORK_REQUEST_ID:0:8}..."
        
        if [ -n "$WORK_REQUEST_ID" ]; then
            api_test "GET" "/api/v1/work-requests/$WORK_REQUEST_ID" "" "Get Work Request by ID" "200" "true"
        fi
    else
        print_result "Create Work Request" "FAIL" "Status: $wr_status"
    fi
fi
echo

# ═══════════════════════════════════════
# TASKS ENDPOINTS
# ═══════════════════════════════════════
print_section "TASK MANAGEMENT"

api_test "GET" "/api/v1/tasks" "" "Get All Tasks" "200" "true"
api_test "GET" "/api/v1/tasks?pageSize=3" "" "Get Tasks (Paginated)" "200" "true"

if [ -n "$PROJECT_ID" ]; then
    api_test "GET" "/api/v1/tasks?projectId=$PROJECT_ID" "" "Get Tasks by Project" "200" "true"
fi
echo

# ═══════════════════════════════════════
# ADDITIONAL ENDPOINTS
# ═══════════════════════════════════════
print_section "ADDITIONAL ENDPOINTS"

# Master Plans
api_test "GET" "/api/v1/master-plans" "" "Get Master Plans" "200" "true"

# Calendar
api_test "GET" "/api/v1/calendar/tasks" "" "Get Calendar Tasks" "200" "true"

# Documents
api_test "GET" "/api/v1/documents" "" "Get Documents" "200" "true"

# Resources  
api_test "GET" "/api/v1/resources" "" "Get Resources" "200" "true"

# Phases
api_test "GET" "/api/v1/phases" "" "Get Project Phases" "200" "true"
echo

# ═══════════════════════════════════════
# ERROR HANDLING TESTS
# ═══════════════════════════════════════
print_section "ERROR HANDLING"

api_test "GET" "/api/v1/projects/00000000-0000-0000-0000-000000000000" "" "Get Non-existent Project" "404" "true"
api_test "GET" "/api/v1/users/invalid-id" "" "Get Invalid User ID" "400" "true"
api_test "GET" "/api/v1/nonexistent" "" "Non-existent Endpoint" "404" "true"

# Test unauthorized access
echo -e "${CYAN}🔒 Testing unauthorized access...${NC}"
unauth_response=$(curl -s -w "STATUS:%{http_code}" "$API_BASE/api/v1/projects")
unauth_status=$(echo "$unauth_response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)

if [ "$unauth_status" = "401" ]; then
    print_result "Unauthorized Access Protection" "PASS" "Status: 401"
else
    print_result "Unauthorized Access Protection" "FAIL" "Status: $unauth_status (Expected: 401)"
fi
echo

# ═══════════════════════════════════════
# RATE LIMITING TESTS
# ═══════════════════════════════════════
print_section "RATE LIMITING"

echo -e "${CYAN}🚦 Testing rate limiting...${NC}"
rate_limit_hit=false

# Test with multiple requests
for i in {1..15}; do
    response=$(curl -s -w "STATUS:%{http_code}" -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/projects")
    status=$(echo "$response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
    
    if [ "$status" = "429" ]; then
        rate_limit_hit=true
        break
    fi
    
    sleep 0.1
done

if [ "$rate_limit_hit" = "true" ]; then
    print_result "Rate Limiting" "PASS" "Rate limit enforced (429 Too Many Requests)"
else
    print_result "Rate Limiting" "WARN" "No rate limit hit in 15 requests"
fi
echo

# ═══════════════════════════════════════
# CLEANUP (DELETE TESTS)
# ═══════════════════════════════════════
print_section "CLEANUP & DELETE OPERATIONS"

echo -e "${CYAN}🗑️ Testing DELETE operations...${NC}"

if [ -n "$WORK_REQUEST_ID" ]; then
    api_test "DELETE" "/api/v1/work-requests/$WORK_REQUEST_ID" "" "Delete Work Request" "200" "true"
fi

if [ -n "$DAILY_REPORT_ID" ]; then
    api_test "DELETE" "/api/v1/daily-reports/$DAILY_REPORT_ID" "" "Delete Daily Report" "200" "true"
fi

if [ -n "$PROJECT_ID" ]; then
    api_test "DELETE" "/api/v1/projects/$PROJECT_ID" "" "Delete Project" "200" "true"
fi
echo

# ═══════════════════════════════════════
# SUMMARY
# ═══════════════════════════════════════
print_section "TEST SUMMARY"

echo -e "${GREEN}✅ Comprehensive API endpoint testing completed!${NC}"
echo
echo -e "${BLUE}📊 Test Coverage:${NC}"
echo -e "${CYAN}   ✓ Health & System endpoints${NC}"
echo -e "${CYAN}   ✓ Authentication & Authorization${NC}"
echo -e "${CYAN}   ✓ User Management${NC}"
echo -e "${CYAN}   ✓ Project CRUD operations${NC}"
echo -e "${CYAN}   ✓ Daily Reports${NC}"
echo -e "${CYAN}   ✓ Work Requests${NC}"
echo -e "${CYAN}   ✓ Task Management${NC}"
echo -e "${CYAN}   ✓ Additional endpoints${NC}"
echo -e "${CYAN}   ✓ Error handling${NC}"
echo -e "${CYAN}   ✓ Rate limiting${NC}"
echo -e "${CYAN}   ✓ DELETE operations${NC}"
echo
echo -e "${BLUE}🌐 API Base URL: $API_BASE${NC}"
echo -e "${BLUE}📅 Test completed: $(date)${NC}"
echo
echo -e "${PURPLE}${BOLD}═══════════════════════════════════════${NC}"
echo -e "${PURPLE}${BOLD}🌞 Solar Projects API Testing Complete${NC}"
echo -e "${PURPLE}${BOLD}═══════════════════════════════════════${NC}"
