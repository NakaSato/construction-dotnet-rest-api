#!/bin/bash

# Solar Projects API - Smart Admin Testing Script with Rate Limit Management
# This script intelligently handles rate limiting and creates comprehensive test data

echo "🔐 SOLAR PROJECTS API - SMART ADMIN TESTING (RATE LIMIT AWARE)"
echo "============================================================="

# Configuration
API_BASE="http://localhost:5002"
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"
OUTPUT_DIR="./test-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$OUTPUT_DIR/admin_smart_$TIMESTAMP.log"

# Rate limiting configuration
MIN_REQUEST_DELAY=2  # Minimum delay between requests (seconds)
RATE_LIMIT_WAIT=60   # How long to wait when rate limited (seconds)
MAX_RETRIES=3        # Maximum retries for rate limited requests

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Global variables for storing created entity IDs
declare -a PROJECT_IDS=()
declare -a TASK_IDS=()
declare -a REPORT_IDS=()
declare -a WORK_REQUEST_IDS=()
declare -a CALENDAR_EVENT_IDS=()
declare -a USER_IDS=()

# Counters for statistics
TOTAL_REQUESTS=0
SUCCESSFUL_REQUESTS=0
RATE_LIMITED_REQUESTS=0
ERROR_REQUESTS=0

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Initialize log file
echo "Smart Admin API Testing with Rate Limit Management - $TIMESTAMP" > "$LOG_FILE"
echo "===============================================================" >> "$LOG_FILE"

# Function to log and display
log_and_echo() {
    echo -e "$1"
    echo -e "$1" | sed 's/\x1B\[[0-9;]*[JKmsu]//g' >> "$LOG_FILE"
}

# Function to add intelligent delay between requests
smart_delay() {
    log_and_echo "${CYAN}⏱️  Waiting $MIN_REQUEST_DELAY seconds to avoid rate limiting...${NC}"
    sleep "$MIN_REQUEST_DELAY"
}

# Function to handle rate limiting
handle_rate_limit() {
    local retry_after=${1:-$RATE_LIMIT_WAIT}
    log_and_echo "${YELLOW}🚦 Rate limit hit! Waiting $retry_after seconds before continuing...${NC}"
    RATE_LIMITED_REQUESTS=$((RATE_LIMITED_REQUESTS + 1))
    
    # Show countdown
    for ((i=retry_after; i>=1; i--)); do
        printf "\r${YELLOW}⏳ Rate limit cooldown: ${i}s remaining...${NC}"
        sleep 1
    done
    echo ""
    log_and_echo "${GREEN}✅ Rate limit cooldown complete. Resuming tests...${NC}"
}

# Function to make smart API request with rate limit handling
make_smart_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    local content_type=${5:-"application/json"}
    local max_retries=${6:-$MAX_RETRIES}
    
    TOTAL_REQUESTS=$((TOTAL_REQUESTS + 1))
    
    smart_delay  # Add intelligent delay before each request
    
    log_and_echo "\n${BLUE}📡 Testing: $description${NC}"
    log_and_echo "   Method: $method"
    log_and_echo "   Endpoint: $endpoint"
    
    local attempt=1
    local status=0
    local response=""
    local body=""
    
    while [ $attempt -le $max_retries ]; do
        if [ $attempt -gt 1 ]; then
            log_and_echo "   ${YELLOW}🔄 Retry attempt $attempt of $max_retries${NC}"
        fi
        
        # Make the actual request
        if [ "$method" = "GET" ]; then
            response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X GET "$API_BASE$endpoint" \
                -H "Authorization: Bearer $JWT_TOKEN" \
                -H "Content-Type: $content_type")
        elif [ "$method" = "POST" ]; then
            if [ "$content_type" = "multipart/form-data" ]; then
                response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE$endpoint" \
                    -H "Authorization: Bearer $JWT_TOKEN" \
                    $data)
            else
                response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE$endpoint" \
                    -H "Authorization: Bearer $JWT_TOKEN" \
                    -H "Content-Type: $content_type" \
                    -d "$data")
            fi
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
        
        status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
        body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')
        
        # Handle different response types
        if [ "$status" -ge 200 ] && [ "$status" -lt 300 ]; then
            SUCCESSFUL_REQUESTS=$((SUCCESSFUL_REQUESTS + 1))
            log_and_echo "   ${GREEN}✅ SUCCESS - Status: $status${NC}"
            break
        elif [ "$status" = "429" ]; then
            # Extract retry-after time from response if available
            local retry_after=$RATE_LIMIT_WAIT
            if echo "$body" | jq -e '.error.extensions.rateLimit.retryAfterSeconds' > /dev/null 2>&1; then
                retry_after=$(echo "$body" | jq -r '.error.extensions.rateLimit.retryAfterSeconds')
                # Add a few extra seconds to be safe
                retry_after=$((retry_after + 5))
            fi
            
            if [ $attempt -lt $max_retries ]; then
                log_and_echo "   ${YELLOW}⚠️  RATE LIMITED - Status: $status (Will retry after cooldown)${NC}"
                handle_rate_limit $retry_after
                attempt=$((attempt + 1))
            else
                log_and_echo "   ${RED}❌ RATE LIMITED - Status: $status (Max retries exceeded)${NC}"
                RATE_LIMITED_REQUESTS=$((RATE_LIMITED_REQUESTS + 1))
                break
            fi
        elif [ "$status" -ge 400 ] && [ "$status" -lt 500 ]; then
            log_and_echo "   ${YELLOW}⚠️  CLIENT ERROR - Status: $status${NC}"
            break
        else
            ERROR_REQUESTS=$((ERROR_REQUESTS + 1))
            log_and_echo "   ${RED}❌ ERROR - Status: $status${NC}"
            break
        fi
    done
    
    # Log response body (truncated for readability)
    echo "   Response (first 300 chars): $(echo "$body" | cut -c1-300)..." >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
    
    # Extract and store entity IDs for later use
    if [ "$status" -ge 200 ] && [ "$status" -lt 300 ] && echo "$body" | jq -e '.success == true and .data' > /dev/null 2>&1; then
        local entity_id=""
        
        # Try different ID field patterns
        if echo "$body" | jq -e '.data.projectId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.projectId')
            PROJECT_IDS+=("$entity_id")
        elif echo "$body" | jq -e '.data.taskId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.taskId')
            TASK_IDS+=("$entity_id")
        elif echo "$body" | jq -e '.data.reportId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.reportId')
            REPORT_IDS+=("$entity_id")
        elif echo "$body" | jq -e '.data.requestId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.requestId')
            WORK_REQUEST_IDS+=("$entity_id")
        elif echo "$body" | jq -e '.data.eventId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.eventId')
            CALENDAR_EVENT_IDS+=("$entity_id")
        elif echo "$body" | jq -e '.data.userId' > /dev/null 2>&1; then
            entity_id=$(echo "$body" | jq -r '.data.userId')
            USER_IDS+=("$entity_id")
        fi
        
        if [ ! -z "$entity_id" ]; then
            log_and_echo "   📝 Created Entity ID: $entity_id"
        fi
    fi
    
    return $status
}

# Check if API is running
log_and_echo "${BLUE}🔍 Checking API availability...${NC}"
if ! curl -s -f "$API_BASE/health" > /dev/null; then
    log_and_echo "${RED}❌ API is not running. Please start with: docker-compose up -d${NC}"
    exit 1
fi
log_and_echo "${GREEN}✅ API is available${NC}"

# ==================== AUTHENTICATION ====================
log_and_echo "\n${YELLOW}==================== 1. AUTHENTICATION & AUTHORIZATION ====================${NC}"

# Login as Admin
login_data='{
    "username": "'$ADMIN_USERNAME'",
    "password": "'$ADMIN_PASSWORD'"
}'

log_and_echo "${BLUE}🔐 Logging in as Admin...${NC}"
response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE/api/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d "$login_data")

status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')

if [ "$status" = "200" ]; then
    JWT_TOKEN=$(echo "$body" | jq -r '.data.token')
    REFRESH_TOKEN=$(echo "$body" | jq -r '.data.refreshToken')
    ADMIN_USER_ID=$(echo "$body" | jq -r '.data.user.userId')
    
    log_and_echo "${GREEN}✅ Admin login successful${NC}"
    log_and_echo "   User ID: $ADMIN_USER_ID"
    log_and_echo "   Token: ${JWT_TOKEN:0:50}..."
else
    log_and_echo "${RED}❌ Admin login failed - Status: $status${NC}"
    log_and_echo "Response: $body"
    exit 1
fi

# ==================== PHASE 1: READ OPERATIONS (LOW IMPACT) ====================
log_and_echo "\n${YELLOW}==================== 2. PHASE 1: READ OPERATIONS (SAFE) ====================${NC}"

# Test basic read operations first (these shouldn't hit rate limits as much)
make_smart_request "GET" "/health" "" "Basic Health Check"
make_smart_request "GET" "/health/detailed" "" "Detailed Health Check"

# User management reads
make_smart_request "GET" "/api/v1/users" "" "Get All Users"
make_smart_request "GET" "/api/v1/users?pageSize=3" "" "Get Users - Paginated"

# Project reads
make_smart_request "GET" "/api/v1/projects" "" "Get All Projects"
make_smart_request "GET" "/api/v1/projects?pageSize=3" "" "Get Projects - Paginated"

# Task reads
make_smart_request "GET" "/api/v1/tasks" "" "Get All Tasks"
make_smart_request "GET" "/api/v1/tasks?pageSize=3" "" "Get Tasks - Paginated"

# ==================== PHASE 2: CAREFUL DATA CREATION ====================
log_and_echo "\n${YELLOW}==================== 3. PHASE 2: STRATEGIC DATA CREATION ====================${NC}"

# Create users first (usually less rate limited)
log_and_echo "\n${CYAN}👥 Creating Test Users with Different Roles...${NC}"
declare -a user_roles=("Manager" "User" "Viewer")
declare -a test_users=(
    '{"username":"smart_manager_'$TIMESTAMP'","email":"smart_manager_'$TIMESTAMP'@example.com","password":"TestManager123!","fullName":"Smart Test Manager","roleId":2}'
    '{"username":"smart_user_'$TIMESTAMP'","email":"smart_user_'$TIMESTAMP'@example.com","password":"TestUser123!","fullName":"Smart Test User","roleId":3}'
    '{"username":"smart_viewer_'$TIMESTAMP'","email":"smart_viewer_'$TIMESTAMP'@example.com","password":"TestViewer123!","fullName":"Smart Test Viewer","roleId":4}'
)

for i in "${!test_users[@]}"; do
    make_smart_request "POST" "/api/v1/auth/register" "${test_users[$i]}" "Create Smart Test ${user_roles[$i]}"
done

# Create projects strategically (one at a time with proper delays)
log_and_echo "\n${CYAN}🏗️  Creating Solar Projects Strategically...${NC}"
declare -a project_data=(
    '{
        "projectName": "Smart Residential Solar '$TIMESTAMP'",
        "address": "123 Smart Solar Street, Efficiency City, CA 90210",
        "clientInfo": "Smart Home Owners - 2,800 sq ft home with south-facing roof",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-08-15T17:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Smart Commercial Complex '$TIMESTAMP'",
        "address": "456 Business Smart Blvd, Corporate Park, CA 94102",
        "clientInfo": "Smart Corp Inc - 40,000 sq ft office building with optimal roof exposure",
        "startDate": "2025-07-01T07:00:00Z",
        "estimatedEndDate": "2025-10-30T18:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Smart Industrial Farm '$TIMESTAMP'",
        "address": "789 Industrial Smart Way, Manufacturing Zone, CA 94601",
        "clientInfo": "Smart Energy Solutions - 8MW ground-mount solar with smart grid integration",
        "startDate": "2025-06-25T06:00:00Z",
        "estimatedEndDate": "2025-12-20T16:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
)

declare -a project_names=("Residential Solar" "Commercial Complex" "Industrial Farm")

for i in "${!project_data[@]}"; do
    make_smart_request "POST" "/api/v1/projects" "${project_data[$i]}" "Create Smart ${project_names[$i]} Project"
    
    # Extra delay after project creation (these seem to be heavily rate limited)
    if [ $i -lt $((${#project_data[@]} - 1)) ]; then
        log_and_echo "${CYAN}⏸️  Extended pause between project creations...${NC}"
        sleep 5
    fi
done

# ==================== PHASE 3: TASK CREATION ====================
log_and_echo "\n${YELLOW}==================== 4. PHASE 3: TASK CREATION FOR PROJECTS ====================${NC}"

# Create tasks for the first project if it was created successfully
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    project_id="${PROJECT_IDS[0]}"
    log_and_echo "\n${CYAN}📋 Creating Tasks for Project: $project_id${NC}"
    
    declare -a task_data=(
        '{
            "title": "Smart Site Assessment '$TIMESTAMP'",
            "description": "Comprehensive site evaluation using smart assessment tools and analytics",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-06-22T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 8.0
        }'
        '{
            "title": "Smart Permit Processing '$TIMESTAMP'",
            "description": "Streamlined permit applications using digital submission systems",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-06-25T16:00:00Z",
            "priority": "Critical",
            "status": "Pending",
            "estimatedHours": 4.0
        }'
        '{
            "title": "Smart Panel Installation '$TIMESTAMP'",
            "description": "Installation of smart solar panels with integrated monitoring systems",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-07-10T16:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 24.0
        }'
    )
    
    for i in "${!task_data[@]}"; do
        make_smart_request "POST" "/api/v1/tasks" "${task_data[$i]}" "Create Smart Task $((i+1))"
    done
fi

# ==================== PHASE 4: DAILY REPORTS (IF POSSIBLE) ====================
log_and_echo "\n${YELLOW}==================== 5. PHASE 4: DAILY REPORTS CREATION ====================${NC}"

# Try to create daily reports if we have projects and tasks
if [ ${#PROJECT_IDS[@]} -gt 0 ] && [ ${#TASK_IDS[@]} -gt 0 ]; then
    log_and_echo "\n${CYAN}📊 Creating Smart Daily Reports...${NC}"
    
    declare -a report_data=(
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "taskId": "'${TASK_IDS[0]}'",
            "userId": "'$ADMIN_USER_ID'",
            "reportDate": "2025-06-14T00:00:00Z",
            "hoursWorked": 8.0,
            "workCompleted": "Completed smart site assessment using advanced analytics and drone survey technology",
            "issuesEncountered": "None - smart tools enabled efficient workflow",
            "materialUsed": "Smart assessment tools, digital measuring devices, drone equipment",
            "weatherConditions": "Optimal - 75°F, clear skies, perfect for smart solar assessment",
            "safetyNotes": "All smart safety protocols followed with digital monitoring",
            "additionalNotes": "Client impressed with smart technology approach and digital presentation"
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "userId": "'$ADMIN_USER_ID'",
            "reportDate": "2025-06-15T00:00:00Z",
            "hoursWorked": 6.0,
            "workCompleted": "Submitted digital permit applications through smart city portal system",
            "issuesEncountered": "Minor delay in digital signature processing - resolved quickly",
            "materialUsed": "Digital documentation, online permit systems",
            "weatherConditions": "Office work - weather not applicable",
            "safetyNotes": "No safety concerns for digital work",
            "additionalNotes": "Smart permit system reduced processing time by 50%"
        }'
    )
    
    for i in "${!report_data[@]}"; do
        make_smart_request "POST" "/api/v1/daily-reports" "${report_data[$i]}" "Create Smart Daily Report $((i+1))"
    done
fi

# ==================== PHASE 5: VERIFICATION & TESTING ====================
log_and_echo "\n${YELLOW}==================== 6. PHASE 5: DATA VERIFICATION ====================${NC}"

# Test access to created data
log_and_echo "\n${CYAN}🔍 Verifying Access to Created Data...${NC}"

if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    for pid in "${PROJECT_IDS[@]}"; do
        make_smart_request "GET" "/api/v1/projects/$pid" "" "Verify Project Access: $pid"
    done
    
    # Test project filtering with created data
    make_smart_request "GET" "/api/v1/projects?search=Smart" "" "Search Smart Projects"
fi

if [ ${#TASK_IDS[@]} -gt 0 ]; then
    for tid in "${TASK_IDS[@]}"; do
        make_smart_request "GET" "/api/v1/tasks/$tid" "" "Verify Task Access: $tid"
    done
    
    # Test task filtering
    make_smart_request "GET" "/api/v1/tasks?assignedToUserId=$ADMIN_USER_ID" "" "Get Tasks Assigned to Admin"
fi

# ==================== PHASE 6: UPDATE OPERATIONS ====================
log_and_echo "\n${YELLOW}==================== 7. PHASE 6: UPDATE OPERATIONS ====================${NC}"

# Update operations on created data
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    first_project_id="${PROJECT_IDS[0]}"
    
    project_update='{
        "projectName": "Smart Residential Solar '$TIMESTAMP' - UPDATED",
        "address": "123 Smart Solar Street, Efficiency City, CA 90210",
        "clientInfo": "Smart Home Owners - UPDATED: Added battery storage and smart grid integration",
        "status": "InProgress",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-08-20T17:00:00Z"
    }'
    
    make_smart_request "PUT" "/api/v1/projects/$first_project_id" "$project_update" "Update First Smart Project"
fi

if [ ${#TASK_IDS[@]} -gt 0 ]; then
    first_task_id="${TASK_IDS[0]}"
    
    task_update='{
        "title": "Smart Site Assessment '$TIMESTAMP' - COMPLETED",
        "description": "COMPLETED: Smart site assessment with comprehensive analytics and recommendations",
        "status": "Completed",
        "priority": "High",
        "estimatedHours": 8.0
    }'
    
    make_smart_request "PUT" "/api/v1/tasks/$first_task_id" "$task_update" "Update First Smart Task"
fi

# ==================== FINAL SUMMARY ====================
log_and_echo "\n${YELLOW}==================== 📊 SMART TESTING SUMMARY ====================${NC}"

log_and_echo "${GREEN}✅ Smart Admin API Testing Completed!${NC}"
log_and_echo "${BLUE}📁 Detailed results saved to: $LOG_FILE${NC}"

# Calculate success rate
local success_rate=0
if [ $TOTAL_REQUESTS -gt 0 ]; then
    success_rate=$(( (SUCCESSFUL_REQUESTS * 100) / TOTAL_REQUESTS ))
fi

log_and_echo "\n${CYAN}📈 Smart Testing Statistics:${NC}"
log_and_echo "   Total API Calls: ${BLUE}$TOTAL_REQUESTS${NC}"
log_and_echo "   Successful: ${GREEN}$SUCCESSFUL_REQUESTS${NC} (${success_rate}%)"
log_and_echo "   Rate Limited: ${YELLOW}$RATE_LIMITED_REQUESTS${NC}"
log_and_echo "   Errors: ${RED}$ERROR_REQUESTS${NC}"

log_and_echo "\n${CYAN}📊 Data Creation Results:${NC}"
log_and_echo "   Smart Projects: ${PURPLE}${#PROJECT_IDS[@]}${NC} created successfully"
log_and_echo "   Smart Tasks: ${PURPLE}${#TASK_IDS[@]}${NC} created successfully"
log_and_echo "   Smart Reports: ${PURPLE}${#REPORT_IDS[@]}${NC} created successfully"
log_and_echo "   Smart Work Requests: ${PURPLE}${#WORK_REQUEST_IDS[@]}${NC} created successfully"
log_and_echo "   Smart Calendar Events: ${PURPLE}${#CALENDAR_EVENT_IDS[@]}${NC} created successfully"
log_and_echo "   Smart Users: ${PURPLE}${#USER_IDS[@]}${NC} created successfully"

if [ $success_rate -ge 75 ]; then
    log_and_echo "\n${GREEN}🎯 EXCELLENT: Smart testing achieved high success rate with proper rate limit management!${NC}"
elif [ $success_rate -ge 50 ]; then
    log_and_echo "\n${YELLOW}⚠️  GOOD: Smart testing achieved reasonable success rate despite rate limiting.${NC}"
else
    log_and_echo "\n${RED}❌ NEEDS ATTENTION: Rate limiting still causing significant issues.${NC}"
fi

log_and_echo "\n${BLUE}💡 Smart Testing Insights:${NC}"
log_and_echo "   • Rate limiting is aggressive (5 requests per window)"
log_and_echo "   • Project creation seems heavily rate limited"
log_and_echo "   • Read operations are more permissive"
log_and_echo "   • Proper delays and retry logic improve success rates"

echo ""
echo "🎯 Smart admin testing completed with intelligent rate limit management!"
echo "🧠 Check $LOG_FILE for detailed results and insights."
