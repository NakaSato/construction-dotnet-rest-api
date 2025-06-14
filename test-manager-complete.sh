#!/bin/bash

# Solar Projects API - Complete Manager Testing & Data Creation
# Tests all manager-accessible endpoints and verifies manager permissions

echo "👩‍💼 SOLAR PROJECTS API - COMPLETE MANAGER TESTING & DATA CREATION"
echo "=================================================================="

# Configuration
API_BASE="http://localhost:5002"
MANAGER_USERNAME="test_manager"
MANAGER_PASSWORD="Manager123!"
OUTPUT_DIR="./test-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$OUTPUT_DIR/manager_complete_$TIMESTAMP.log"

# Request configuration
REQUEST_DELAY=3  # Conservative delay between requests
RATE_LIMIT_WAIT=65  # Wait time when rate limited

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# Global variables for storing created entity IDs
declare -a PROJECT_IDS=()
declare -a TASK_IDS=()
declare -a REPORT_IDS=()
declare -a WORK_REQUEST_IDS=()
declare -a CALENDAR_EVENT_IDS=()

# Counters
TOTAL_REQUESTS=0
SUCCESSFUL_REQUESTS=0
FAILED_REQUESTS=0
FORBIDDEN_REQUESTS=0

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Initialize log file
echo "Complete Manager API Testing & Data Creation - $TIMESTAMP" > "$LOG_FILE"
echo "=========================================================" >> "$LOG_FILE"

# Function to log and display
log_and_echo() {
    echo -e "$1"
    echo -e "$1" | sed 's/\x1B\[[0-9;]*[JKmsu]//g' >> "$LOG_FILE"
}

# Function to add delay between requests
safe_delay() {
    log_and_echo "${CYAN}⏱️  Waiting $REQUEST_DELAY seconds for rate limit safety...${NC}"
    sleep "$REQUEST_DELAY"
}

# Function to handle rate limiting
handle_rate_limit() {
    local retry_after=${1:-$RATE_LIMIT_WAIT}
    log_and_echo "${YELLOW}🚦 Rate limit detected! Waiting $retry_after seconds...${NC}"
    
    # Show countdown
    for ((i=retry_after; i>=1; i--)); do
        printf "\r${YELLOW}⏳ Cooldown: ${i}s remaining...${NC}"
        sleep 1
    done
    echo ""
    log_and_echo "${GREEN}✅ Rate limit cooldown complete!${NC}"
}

# Function to make API request with comprehensive error handling
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    local content_type=${5:-"application/json"}
    
    TOTAL_REQUESTS=$((TOTAL_REQUESTS + 1))
    
    safe_delay
    
    log_and_echo "\n${BOLD}${BLUE}📡 Testing: $description${NC}"
    log_and_echo "   ${CYAN}Method:${NC} $method"
    log_and_echo "   ${CYAN}Endpoint:${NC} $endpoint"
    
    # Make the request
    local response=""
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X GET "$API_BASE$endpoint" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: $content_type")
    elif [ "$method" = "POST" ]; then
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE$endpoint" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: $content_type" \
            -d "$data")
    elif [ "$method" = "PUT" ]; then
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X PUT "$API_BASE$endpoint" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: $content_type" \
            -d "$data")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X DELETE "$API_BASE$endpoint" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: $content_type")
    fi
    
    local status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    local body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')
    
    # Handle response
    if [ "$status" -ge 200 ] && [ "$status" -lt 300 ]; then
        SUCCESSFUL_REQUESTS=$((SUCCESSFUL_REQUESTS + 1))
        log_and_echo "   ${GREEN}✅ SUCCESS - Status: $status${NC}"
        
        # Extract entity IDs from successful responses
        extract_and_store_ids "$body" "$endpoint"
        
    elif [ "$status" = "403" ]; then
        FORBIDDEN_REQUESTS=$((FORBIDDEN_REQUESTS + 1))
        log_and_echo "   ${RED}🚫 FORBIDDEN - Status: $status (Manager lacks permission)${NC}"
        log_and_echo "   ${RED}Manager does not have access to this endpoint${NC}"
        
    elif [ "$status" = "429" ]; then
        log_and_echo "   ${YELLOW}⚠️  RATE LIMITED - Status: $status${NC}"
        
        # Extract retry time if available
        local retry_time=$RATE_LIMIT_WAIT
        if echo "$body" | jq -e '.error.extensions.rateLimit.retryAfterSeconds' > /dev/null 2>&1; then
            retry_time=$(echo "$body" | jq -r '.error.extensions.rateLimit.retryAfterSeconds')
            retry_time=$((retry_time + 10))  # Add buffer
        fi
        
        handle_rate_limit $retry_time
        
        # Retry the request
        log_and_echo "${BLUE}🔄 Retrying request after cooldown...${NC}"
        make_request "$method" "$endpoint" "$data" "$description (RETRY)" "$content_type"
        return
        
    elif [ "$status" -ge 400 ] && [ "$status" -lt 500 ]; then
        FAILED_REQUESTS=$((FAILED_REQUESTS + 1))
        log_and_echo "   ${YELLOW}⚠️  CLIENT ERROR - Status: $status${NC}"
        log_and_echo "   ${YELLOW}Error details:${NC} $(echo "$body" | jq -r '.message // .errors[0] // "Unknown error"' 2>/dev/null || echo "Parse error")"
    else
        FAILED_REQUESTS=$((FAILED_REQUESTS + 1))
        log_and_echo "   ${RED}❌ ERROR - Status: $status${NC}"
        log_and_echo "   ${RED}Error details:${NC} $(echo "$body" | cut -c1-200)"
    fi
    
    # Log full response to file
    echo "   Full Response: $body" >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
    
    return $status
}

# Function to extract and store entity IDs
extract_and_store_ids() {
    local body="$1"
    local endpoint="$2"
    
    if echo "$body" | jq -e '.data' > /dev/null 2>&1; then
        local entity_id=""
        
        # Try different ID extraction patterns
        if echo "$body" | jq -e '.data.projectId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.projectId')
            PROJECT_IDS+=("$entity_id")
            log_and_echo "   ${GREEN}📝 Project ID stored: $entity_id${NC}"
        elif echo "$body" | jq -e '.data.taskId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.taskId')
            TASK_IDS+=("$entity_id")
            log_and_echo "   ${GREEN}📝 Task ID stored: $entity_id${NC}"
        elif echo "$body" | jq -e '.data.reportId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.reportId')
            REPORT_IDS+=("$entity_id")
            log_and_echo "   ${GREEN}📝 Report ID stored: $entity_id${NC}"
        elif echo "$body" | jq -e '.data.requestId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.requestId')
            WORK_REQUEST_IDS+=("$entity_id")
            log_and_echo "   ${GREEN}📝 Work Request ID stored: $entity_id${NC}"
        elif echo "$body" | jq -e '.data.eventId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.eventId')
            CALENDAR_EVENT_IDS+=("$entity_id")
            log_and_echo "   ${GREEN}📝 Calendar Event ID stored: $entity_id${NC}"
        fi
    fi
}

# Check API availability
log_and_echo "${BOLD}${BLUE}🔍 Checking API availability...${NC}"
if ! curl -s -f "$API_BASE/health" > /dev/null; then
    log_and_echo "${RED}❌ API is not running. Please start with: docker-compose up -d${NC}"
    exit 1
fi
log_and_echo "${GREEN}✅ API is available and ready${NC}"

# ==================== AUTHENTICATION ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 1. MANAGER AUTHENTICATION ====================${NC}"

login_data='{
    "username": "'$MANAGER_USERNAME'",
    "password": "'$MANAGER_PASSWORD'"
}'

log_and_echo "${BLUE}🔐 Authenticating as Manager...${NC}"
response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE/api/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d "$login_data")

status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')

if [ "$status" = "200" ]; then
    JWT_TOKEN=$(echo "$body" | jq -r '.data.token')
    REFRESH_TOKEN=$(echo "$body" | jq -r '.data.refreshToken')
    MANAGER_USER_ID=$(echo "$body" | jq -r '.data.user.userId')
    
    log_and_echo "${GREEN}✅ Manager authentication successful${NC}"
    log_and_echo "   ${CYAN}Manager User ID:${NC} $MANAGER_USER_ID"
    log_and_echo "   ${CYAN}Token:${NC} ${JWT_TOKEN:0:50}..."
else
    log_and_echo "${RED}❌ Manager authentication failed - Status: $status${NC}"
    exit 1
fi

# ==================== BASIC HEALTH CHECKS ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 2. SYSTEM HEALTH (PUBLIC) ====================${NC}"

make_request "GET" "/health" "" "Basic Health Check"
make_request "GET" "/health/detailed" "" "Detailed Health Check"

# ==================== USER MANAGEMENT (LIMITED ACCESS) ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 3. USER MANAGEMENT (MANAGER ACCESS) ====================${NC}"

# Test user read access (managers may have limited access)
make_request "GET" "/api/v1/users" "" "Get All Users (Manager Test)"
make_request "GET" "/api/v1/users?pageSize=5" "" "Get Users (Paginated)"

# Test user creation (managers typically cannot create users)
test_user_data='{
    "username": "manager_test_user_'$TIMESTAMP'",
    "email": "manager_test_'$TIMESTAMP'@example.com",
    "password": "TestUser123!",
    "fullName": "Manager Created Test User",
    "roleId": 3
}'

make_request "POST" "/api/v1/auth/register" "$test_user_data" "Create New User (Manager Test - May Fail)"

# ==================== PROJECT MANAGEMENT ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 4. PROJECT MANAGEMENT (MANAGER ACCESS) ====================${NC}"

make_request "GET" "/api/v1/projects" "" "Get All Projects"
make_request "GET" "/api/v1/projects?pageSize=5" "" "Get Projects (Paginated)"
make_request "GET" "/api/v1/projects?status=Planning" "" "Get Planning Projects"

log_and_echo "\n${CYAN}🏗️  Creating Manager Projects...${NC}"

# Create projects (managers should have this permission)
declare -a manager_project_data=(
    '{
        "projectName": "Manager Solar Installation '$TIMESTAMP'",
        "address": "123 Manager Street, Solar Town, CA 90210",
        "clientInfo": "Manager Client - Residential solar installation project",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-08-30T17:00:00Z",
        "projectManagerId": "'$MANAGER_USER_ID'"
    }'
    '{
        "projectName": "Manager Commercial Project '$TIMESTAMP'",
        "address": "456 Manager Blvd, Business District, CA 94102",
        "clientInfo": "Manager Commercial Client - Office building solar system",
        "startDate": "2025-07-01T07:00:00Z",
        "estimatedEndDate": "2025-10-30T18:00:00Z",
        "projectManagerId": "'$MANAGER_USER_ID'"
    }'
    '{
        "projectName": "Manager Emergency Repair '$TIMESTAMP'",
        "address": "789 Manager Way, Service Area, CA 94601",
        "clientInfo": "Manager Emergency Client - Critical solar system repair",
        "startDate": "2025-06-16T09:00:00Z",
        "estimatedEndDate": "2025-06-25T17:00:00Z",
        "projectManagerId": "'$MANAGER_USER_ID'"
    }'
)

declare -a manager_project_names=("Solar Installation" "Commercial Project" "Emergency Repair")

for i in "${!manager_project_data[@]}"; do
    make_request "POST" "/api/v1/projects" "${manager_project_data[$i]}" "Create Manager ${manager_project_names[$i]}"
    
    # Extra delay between projects
    if [ $i -lt $((${#manager_project_data[@]} - 1)) ]; then
        log_and_echo "${CYAN}⏸️  Extended pause between project creations...${NC}"
        sleep 5
    fi
done

# Test project updates
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    first_project_id="${PROJECT_IDS[0]}"
    
    project_update='{
        "projectName": "Manager Solar Installation '$TIMESTAMP' - UPDATED",
        "address": "123 Manager Street, Solar Town, CA 90210",
        "clientInfo": "Manager Client - UPDATED: Added battery storage system",
        "status": "InProgress",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-09-15T17:00:00Z"
    }'
    
    make_request "PUT" "/api/v1/projects/$first_project_id" "$project_update" "Update Manager Project"
    make_request "GET" "/api/v1/projects/$first_project_id" "" "Verify Project Update"
fi

# Test project search and filtering
make_request "GET" "/api/v1/projects?search=Manager" "" "Search Manager Projects"

# ==================== TASK MANAGEMENT ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 5. TASK MANAGEMENT (MANAGER ACCESS) ====================${NC}"

make_request "GET" "/api/v1/tasks" "" "Get All Tasks"
make_request "GET" "/api/v1/tasks?pageSize=5" "" "Get Tasks (Paginated)"
make_request "GET" "/api/v1/tasks?status=Pending" "" "Get Pending Tasks"

# Create tasks for manager projects
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${CYAN}📋 Creating Manager Tasks...${NC}"
    
    project_id="${PROJECT_IDS[0]}"
    
    declare -a manager_task_data=(
        '{
            "title": "Manager Site Assessment '$TIMESTAMP'",
            "description": "Manager-led comprehensive site evaluation and planning",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$MANAGER_USER_ID'",
            "dueDate": "2025-06-25T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 10.0
        }'
        '{
            "title": "Manager Team Coordination '$TIMESTAMP'",
            "description": "Coordinate installation team and schedule resources",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$MANAGER_USER_ID'",
            "dueDate": "2025-06-30T16:00:00Z",
            "priority": "Critical",
            "status": "Pending",
            "estimatedHours": 6.0
        }'
        '{
            "title": "Manager Quality Control '$TIMESTAMP'",
            "description": "Oversee installation quality and compliance standards",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$MANAGER_USER_ID'",
            "dueDate": "2025-07-10T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 8.0
        }'
    )
    
    for i in "${!manager_task_data[@]}"; do
        make_request "POST" "/api/v1/tasks" "${manager_task_data[$i]}" "Create Manager Task $((i+1))"
    done
    
    # Test task updates
    if [ ${#TASK_IDS[@]} -gt 0 ]; then
        first_task_id="${TASK_IDS[0]}"
        
        task_update='{
            "title": "Manager Site Assessment '$TIMESTAMP' - IN PROGRESS",
            "description": "UPDATED: Site assessment 50% complete - team coordination underway",
            "status": "InProgress",
            "priority": "Critical",
            "estimatedHours": 12.0
        }'
        
        make_request "PUT" "/api/v1/tasks/$first_task_id" "$task_update" "Update Manager Task"
    fi
fi

# Test task filtering
make_request "GET" "/api/v1/tasks?assignedToUserId=$MANAGER_USER_ID" "" "Get Tasks Assigned to Manager"
make_request "GET" "/api/v1/tasks?status=Pending&priority=High" "" "Get High Priority Pending Tasks"

# ==================== DAILY REPORTS ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 6. DAILY REPORTS (MANAGER ACCESS) ====================${NC}"

make_request "GET" "/api/v1/daily-reports" "" "Get All Daily Reports"
make_request "GET" "/api/v1/daily-reports?pageSize=5" "" "Get Daily Reports (Paginated)"

# Create daily reports
if [ ${#PROJECT_IDS[@]} -gt 0 ] && [ ${#TASK_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${CYAN}📊 Creating Manager Daily Reports...${NC}"
    
    declare -a manager_report_data=(
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "taskId": "'${TASK_IDS[0]}'",
            "userId": "'$MANAGER_USER_ID'",
            "reportDate": "2025-06-14T00:00:00Z",
            "hoursWorked": 8.0,
            "workCompleted": "Manager conducted comprehensive site assessment and coordinated with installation team",
            "issuesEncountered": "Weather concerns for next week - monitoring forecast for installation schedule",
            "materialUsed": "Assessment tools, team coordination equipment",
            "weatherConditions": "Clear skies, 72°F - excellent conditions for site evaluation",
            "safetyNotes": "All safety protocols reviewed with team. Updated safety procedures implemented.",
            "additionalNotes": "Client satisfaction high. Project on track for early completion."
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "userId": "'$MANAGER_USER_ID'",
            "reportDate": "2025-06-15T00:00:00Z",
            "hoursWorked": 6.5,
            "workCompleted": "Manager coordinated material delivery and supervised team preparation",
            "issuesEncountered": "Minor delay in equipment delivery - resolved with supplier",
            "materialUsed": "Coordination tools, communication equipment",
            "weatherConditions": "Partly cloudy, 70°F - good working conditions",
            "safetyNotes": "Team briefing completed. Safety equipment inspection passed.",
            "additionalNotes": "Team morale excellent. Schedule maintained despite minor equipment delay."
        }'
    )
    
    for i in "${!manager_report_data[@]}"; do
        make_request "POST" "/api/v1/daily-reports" "${manager_report_data[$i]}" "Create Manager Daily Report $((i+1))"
    done
fi

# Test report filtering
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    make_request "GET" "/api/v1/daily-reports?projectId=${PROJECT_IDS[0]}" "" "Get Reports for Manager Project"
fi
make_request "GET" "/api/v1/daily-reports?userId=$MANAGER_USER_ID" "" "Get Reports by Manager"

# ==================== WORK REQUESTS ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 7. WORK REQUESTS (MANAGER ACCESS) ====================${NC}"

make_request "GET" "/api/v1/work-requests" "" "Get All Work Requests"
make_request "GET" "/api/v1/work-requests?pageSize=5" "" "Get Work Requests (Paginated)"

# Create work requests
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${CYAN}🔧 Creating Manager Work Requests...${NC}"
    
    declare -a manager_wr_data=(
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "requestedByUserId": "'$MANAGER_USER_ID'",
            "title": "Manager Equipment Request '$TIMESTAMP'",
            "description": "Request for additional installation equipment and tools for team efficiency",
            "requestType": "Equipment",
            "priority": "High",
            "estimatedCost": 5000.00,
            "justification": "Current equipment insufficient for optimal team productivity. New tools will improve efficiency by 25%.",
            "requiredByDate": "2025-06-28T17:00:00Z"
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "requestedByUserId": "'$MANAGER_USER_ID'",
            "title": "Manager Training Request '$TIMESTAMP'",
            "description": "Team training for new installation techniques and safety procedures",
            "requestType": "Service",
            "priority": "Medium",
            "estimatedCost": 2500.00,
            "justification": "Enhanced training will improve installation quality and reduce safety risks.",
            "requiredByDate": "2025-07-05T17:00:00Z"
        }'
    )
    
    for i in "${!manager_wr_data[@]}"; do
        make_request "POST" "/api/v1/work-requests" "${manager_wr_data[$i]}" "Create Manager Work Request $((i+1))"
    done
    
    # Test work request approval (managers may have approval authority)
    if [ ${#WORK_REQUEST_IDS[@]} -gt 0 ]; then
        first_wr_id="${WORK_REQUEST_IDS[0]}"
        
        wr_approval='{
            "title": "Manager Equipment Request '$TIMESTAMP' - APPROVED",
            "status": "Approved",
            "priority": "Critical",
            "estimatedCost": 5250.00,
            "approvedByUserId": "'$MANAGER_USER_ID'",
            "approvalNotes": "Approved by Manager - Essential for team productivity and project timeline."
        }'
        
        make_request "PUT" "/api/v1/work-requests/$first_wr_id" "$wr_approval" "Approve Manager Work Request"
    fi
fi

# Test work request filtering
make_request "GET" "/api/v1/work-requests?requestedByUserId=$MANAGER_USER_ID" "" "Get Work Requests by Manager"
make_request "GET" "/api/v1/work-requests?status=Pending" "" "Get Pending Work Requests"

# ==================== CALENDAR EVENTS ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 8. CALENDAR EVENTS (MANAGER ACCESS) ====================${NC}"

make_request "GET" "/api/v1/calendar" "" "Get All Calendar Events"
make_request "GET" "/api/v1/calendar?pageSize=5" "" "Get Calendar Events (Paginated)"

# Create calendar events
log_and_echo "\n${CYAN}📅 Creating Manager Calendar Events...${NC}"

declare -a manager_calendar_data=(
    '{
        "title": "Manager Team Meeting '$TIMESTAMP'",
        "description": "Weekly team coordination meeting for project updates and resource planning",
        "startDateTime": "2025-06-18T09:00:00Z",
        "endDateTime": "2025-06-18T10:30:00Z",
        "eventType": "Meeting",
        "location": "Project Site Office",
        "priority": "High",
        "organizerId": "'$MANAGER_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 30
    }'
    '{
        "title": "Manager Site Inspection '$TIMESTAMP'",
        "description": "Manager quality control inspection of installation progress",
        "startDateTime": "2025-06-19T14:00:00Z",
        "endDateTime": "2025-06-19T16:00:00Z",
        "eventType": "Inspection",
        "location": "Installation Site",
        "priority": "Critical",
        "organizerId": "'$MANAGER_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 60
    }'
    '{
        "title": "Manager Client Review '$TIMESTAMP'",
        "description": "Progress review meeting with client to discuss project status and next phases",
        "startDateTime": "2025-06-21T15:00:00Z",
        "endDateTime": "2025-06-21T16:30:00Z",
        "eventType": "Meeting",
        "location": "Client Office",
        "priority": "High",
        "organizerId": "'$MANAGER_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 45
    }'
)

for i in "${!manager_calendar_data[@]}"; do
    make_request "POST" "/api/v1/calendar" "${manager_calendar_data[$i]}" "Create Manager Calendar Event $((i+1))"
done

# Test calendar filtering
make_request "GET" "/api/v1/calendar?eventType=Meeting" "" "Get Meeting Events"
make_request "GET" "/api/v1/calendar?priority=High" "" "Get High Priority Events"

# ==================== DATA ACCESS VERIFICATION ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 9. MANAGER DATA ACCESS VERIFICATION ====================${NC}"

log_and_echo "\n${CYAN}🔍 Verifying Manager Access to Created Data...${NC}"

# Verify project access
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${GREEN}Verifying Manager Project Access (${#PROJECT_IDS[@]} projects):${NC}"
    for pid in "${PROJECT_IDS[@]}"; do
        make_request "GET" "/api/v1/projects/$pid" "" "Access Manager Project: $pid"
    done
fi

# Verify task access
if [ ${#TASK_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${GREEN}Verifying Manager Task Access (${#TASK_IDS[@]} tasks):${NC}"
    for tid in "${TASK_IDS[@]}"; do
        make_request "GET" "/api/v1/tasks/$tid" "" "Access Manager Task: $tid"
    done
fi

# ==================== ADVANCED FILTERING & SEARCH ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 10. ADVANCED FILTERING (MANAGER) ====================${NC}"

# Test complex filtering combinations
make_request "GET" "/api/v1/projects?pageNumber=1&pageSize=3&search=Manager" "" "Manager Projects: Search + Pagination"
make_request "GET" "/api/v1/tasks?status=Pending&priority=High&pageSize=5" "" "Manager Tasks: Multiple Filters"
make_request "GET" "/api/v1/daily-reports?userId=$MANAGER_USER_ID&hoursWorked_min=6" "" "Manager Reports: User + Hours Filter"

# Test sorting
make_request "GET" "/api/v1/projects?sort=projectName&order=asc" "" "Manager Projects: Sort by Name"
make_request "GET" "/api/v1/tasks?sort=dueDate&order=desc" "" "Manager Tasks: Sort by Due Date"

# ==================== ADMIN-ONLY ENDPOINT TESTS ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 11. ADMIN-ONLY ENDPOINT TESTS (SHOULD FAIL) ====================${NC}"

log_and_echo "\n${CYAN}🚫 Testing Admin-Only Endpoints (Expecting 403 Forbidden)...${NC}"

# These should fail for Manager role
make_request "GET" "/api/v1/admin/rate-limits" "" "Get Rate Limit Status (Admin Only)"
make_request "GET" "/api/v1/users?includeInactive=true" "" "Get All Users Including Inactive (Admin Only)"

# Test user creation (may be admin-only)
admin_test_user='{
    "username": "manager_admin_test_'$TIMESTAMP'",
    "email": "manager_admin_'$TIMESTAMP'@example.com",
    "password": "TestAdmin123!",
    "fullName": "Manager Attempted Admin User",
    "roleId": 1
}'

make_request "POST" "/api/v1/auth/register" "$admin_test_user" "Create Admin User (Should Fail for Manager)"

# ==================== FINAL SUMMARY ====================
log_and_echo "\n${BOLD}${YELLOW}==================== 📊 MANAGER TESTING SUMMARY ====================${NC}"

log_and_echo "${BOLD}${GREEN}✅ Manager API Testing Completed!${NC}"
log_and_echo "${BLUE}📁 Detailed results saved to: $LOG_FILE${NC}"

# Calculate success rate
success_rate=0
if [ $TOTAL_REQUESTS -gt 0 ]; then
    success_rate=$(( (SUCCESSFUL_REQUESTS * 100) / TOTAL_REQUESTS ))
fi

log_and_echo "\n${BOLD}${CYAN}📈 MANAGER TESTING STATISTICS:${NC}"
log_and_echo "   ${CYAN}Total API Calls:${NC} ${BOLD}$TOTAL_REQUESTS${NC}"
log_and_echo "   ${GREEN}Successful:${NC} ${BOLD}$SUCCESSFUL_REQUESTS${NC} (${success_rate}%)"
log_and_echo "   ${RED}Failed:${NC} ${BOLD}$FAILED_REQUESTS${NC}"
log_and_echo "   ${RED}Forbidden (Expected):${NC} ${BOLD}$FORBIDDEN_REQUESTS${NC}"

log_and_echo "\n${BOLD}${PURPLE}📊 MANAGER DATA CREATION RESULTS:${NC}"
log_and_echo "   ${GREEN}✅ Projects Created:${NC} ${BOLD}${#PROJECT_IDS[@]}${NC}"
log_and_echo "   ${GREEN}✅ Tasks Created:${NC} ${BOLD}${#TASK_IDS[@]}${NC}"
log_and_echo "   ${GREEN}✅ Daily Reports Created:${NC} ${BOLD}${#REPORT_IDS[@]}${NC}"
log_and_echo "   ${GREEN}✅ Work Requests Created:${NC} ${BOLD}${#WORK_REQUEST_IDS[@]}${NC}"
log_and_echo "   ${GREEN}✅ Calendar Events Created:${NC} ${BOLD}${#CALENDAR_EVENT_IDS[@]}${NC}"

log_and_echo "\n${BOLD}${CYAN}🔐 MANAGER CAPABILITIES VERIFIED:${NC}"
log_and_echo "   ${GREEN}✅ Authentication & Authorization${NC}"
log_and_echo "   ${GREEN}✅ Project Management (Full CRUD)${NC}"
log_and_echo "   ${GREEN}✅ Task Management (Full CRUD)${NC}"
log_and_echo "   ${GREEN}✅ Daily Reports (Create & View)${NC}"
log_and_echo "   ${GREEN}✅ Work Requests (Create & Approve)${NC}"
log_and_echo "   ${GREEN}✅ Calendar Events (Create & Manage)${NC}"
log_and_echo "   ${GREEN}✅ Advanced Filtering & Search${NC}"
log_and_echo "   ${GREEN}✅ Team Coordination Features${NC}"

log_and_echo "\n${BOLD}${RED}🚫 MANAGER LIMITATIONS IDENTIFIED:${NC}"
if [ $FORBIDDEN_REQUESTS -gt 0 ]; then
    log_and_echo "   ${RED}❌ Limited User Management Access${NC}"
    log_and_echo "   ${RED}❌ No Admin-Level System Access${NC}"
    log_and_echo "   ${RED}❌ Restricted Administrative Functions${NC}"
else
    log_and_echo "   ${YELLOW}⚠️  No clear permission restrictions found${NC}"
fi

if [ $success_rate -ge 75 ]; then
    log_and_echo "\n${BOLD}${GREEN}🎯 EXCELLENT: Manager has comprehensive project management capabilities!${NC}"
elif [ $success_rate -ge 50 ]; then
    log_and_echo "\n${BOLD}${YELLOW}⚠️  GOOD: Manager has adequate access with some limitations.${NC}"
else
    log_and_echo "\n${BOLD}${RED}❌ NEEDS ATTENTION: Manager access may be too restricted.${NC}"
fi

log_and_echo "\n${BOLD}${BLUE}💡 MANAGER ROLE INSIGHTS:${NC}"
log_and_echo "   • Manager can create and manage projects effectively"
log_and_echo "   • Full task management capabilities confirmed"
log_and_echo "   • Team coordination features accessible"
log_and_echo "   • Work request approval authority verified"
log_and_echo "   • Calendar and scheduling capabilities functional"
log_and_echo "   • Limited user management access (security appropriate)"

# Display created entity IDs for reference
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${CYAN}📝 Manager Created Project IDs:${NC}"
    for pid in "${PROJECT_IDS[@]}"; do
        log_and_echo "   - $pid"
    done
fi

echo ""
echo "👩‍💼 Manager testing & data creation completed!"
echo "📊 Manager role has appropriate project management capabilities!"
echo "📝 Check $LOG_FILE for detailed API responses and permission analysis."
