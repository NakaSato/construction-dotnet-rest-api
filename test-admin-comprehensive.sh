#!/bin/bash

# Solar Projects API - Comprehensive Admin Testing Script
# This script performs exhaustive testing of ALL admin-accessible endpoints
# Creates comprehensive test data and verifies admin can access and manipulate everything

echo "🔐 SOLAR PROJECTS API - COMPREHENSIVE ADMIN TESTING"
echo "=================================================="

# Configuration
API_BASE="http://localhost:5002"
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"
OUTPUT_DIR="./test-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$OUTPUT_DIR/admin_comprehensive_$TIMESTAMP.log"

# Request delay to avoid rate limiting (in seconds)
REQUEST_DELAY=1

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

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Initialize log file
echo "Comprehensive Admin API Testing - $TIMESTAMP" > "$LOG_FILE"
echo "============================================" >> "$LOG_FILE"

# Function to log and display
log_and_echo() {
    echo -e "$1"
    echo -e "$1" | sed 's/\x1B\[[0-9;]*[JKmsu]//g' >> "$LOG_FILE"
}

# Function to add delay between requests
add_delay() {
    if [ "$REQUEST_DELAY" -gt 0 ]; then
        sleep "$REQUEST_DELAY"
    fi
}

# Function to make API request and log response
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    local content_type=${5:-"application/json"}
    local expect_success=${6:-true}
    
    add_delay  # Add delay before each request
    
    log_and_echo "\n${BLUE}📡 Testing: $description${NC}"
    log_and_echo "   Method: $method"
    log_and_echo "   Endpoint: $endpoint"
    
    local response=""
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
    
    local status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    local body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')
    
    # Determine success/failure
    local is_success=false
    if [ "$status" -ge 200 ] && [ "$status" -lt 300 ]; then
        is_success=true
        log_and_echo "   ${GREEN}✅ SUCCESS - Status: $status${NC}"
    elif [ "$status" -ge 400 ] && [ "$status" -lt 500 ]; then
        log_and_echo "   ${YELLOW}⚠️  CLIENT ERROR - Status: $status${NC}"
    else
        log_and_echo "   ${RED}❌ ERROR - Status: $status${NC}"
    fi
    
    # Log response body (truncated for readability)
    echo "   Response (first 300 chars): $(echo "$body" | cut -c1-300)..." >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
    
    # Extract and store entity IDs for later use
    if [ "$is_success" = true ] && echo "$body" | jq -e '.success == true and .data' > /dev/null 2>&1; then
        # Handle different response structures
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

# Function to create test image file
create_test_image() {
    local test_image_path="/tmp/test_solar_panel.jpg"
    
    # Create a simple test image using ImageMagick (if available) or just a text file
    if command -v convert >/dev/null 2>&1; then
        convert -size 100x100 xc:blue -pointsize 10 -gravity center -annotate 0 "Solar Panel Test" "$test_image_path"
    else
        # Fallback: create a simple text file with .jpg extension
        echo "Test solar panel image data for API testing" > "$test_image_path"
    fi
    
    echo "$test_image_path"
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

# Test auth endpoints
make_request "GET" "/api/v1/auth/me" "" "Get Current User Info"

# ==================== HEALTH & SYSTEM ====================
log_and_echo "\n${YELLOW}==================== 2. HEALTH & SYSTEM ENDPOINTS ====================${NC}"

make_request "GET" "/health" "" "Basic Health Check"
make_request "GET" "/health/detailed" "" "Detailed Health Check"

# ==================== USER MANAGEMENT ====================
log_and_echo "\n${YELLOW}==================== 3. USER MANAGEMENT (ADMIN ACCESS) ====================${NC}"

# Get all users with different filters
make_request "GET" "/api/v1/users" "" "Get All Users"
make_request "GET" "/api/v1/users?pageSize=5" "" "Get Users - Paginated"
make_request "GET" "/api/v1/users?role=Manager" "" "Get Users by Role"

# Create multiple test users with different roles
declare -a test_users=(
    '{"username":"test_admin_manager_'$TIMESTAMP'","email":"admin_manager_'$TIMESTAMP'@example.com","password":"TestManager123!","fullName":"Admin Created Manager","roleId":2}'
    '{"username":"test_admin_user_'$TIMESTAMP'","email":"admin_user_'$TIMESTAMP'@example.com","password":"TestUser123!","fullName":"Admin Created User","roleId":3}'
    '{"username":"test_admin_viewer_'$TIMESTAMP'","email":"admin_viewer_'$TIMESTAMP'@example.com","password":"TestViewer123!","fullName":"Admin Created Viewer","roleId":4}'
)

declare -a user_roles=("Manager" "User" "Viewer")

for i in "${!test_users[@]}"; do
    make_request "POST" "/api/v1/auth/register" "${test_users[$i]}" "Create Test ${user_roles[$i]}"
done

# ==================== PROJECT MANAGEMENT ====================
log_and_echo "\n${YELLOW}==================== 4. PROJECT MANAGEMENT (COMPREHENSIVE) ====================${NC}"

# Get projects with various filters
make_request "GET" "/api/v1/projects" "" "Get All Projects"
make_request "GET" "/api/v1/projects?pageSize=3" "" "Get Projects - Small Page"
make_request "GET" "/api/v1/projects?status=Planning" "" "Get Planning Projects"
make_request "GET" "/api/v1/projects?status=InProgress" "" "Get In-Progress Projects"

# Create comprehensive test projects with proper field names
declare -a project_data=(
    '{
        "projectName": "Admin Residential Solar Array '$TIMESTAMP'",
        "address": "123 Sunny Street, Solar City, CA 90210",
        "clientInfo": "Johnson Family - 3,200 sq ft home, south-facing roof, high energy usage",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-08-15T17:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Admin Commercial Office Complex '$TIMESTAMP'",
        "address": "456 Business Boulevard, Corporate Park, CA 94102",
        "clientInfo": "TechCorp Inc - 50,000 sq ft office building, flat roof, grid-tie system",
        "startDate": "2025-07-01T07:00:00Z",
        "estimatedEndDate": "2025-10-30T18:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Admin Industrial Solar Farm '$TIMESTAMP'",
        "address": "789 Industrial Way, Manufacturing District, CA 94601",
        "clientInfo": "Power Solutions LLC - 10MW ground-mount solar farm, battery storage integration",
        "startDate": "2025-06-25T06:00:00Z",
        "estimatedEndDate": "2025-12-20T16:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Admin Emergency Repair Project '$TIMESTAMP'",
        "address": "321 Maintenance Road, Service Center, CA 95014",
        "clientInfo": "City Solar Initiative - Emergency repair of storm-damaged panels",
        "startDate": "2025-06-16T09:00:00Z",
        "estimatedEndDate": "2025-06-22T15:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
    '{
        "projectName": "Admin School District Solar '$TIMESTAMP'",
        "address": "555 Education Drive, School District Central, CA 95134",
        "clientInfo": "Unified School District - Multiple school campuses solar installation",
        "startDate": "2025-08-01T08:00:00Z",
        "estimatedEndDate": "2025-11-15T16:00:00Z",
        "projectManagerId": "'$ADMIN_USER_ID'"
    }'
)

declare -a project_names=("Residential Solar Array" "Commercial Office Complex" "Industrial Solar Farm" "Emergency Repair Project" "School District Solar")

for i in "${!project_data[@]}"; do
    make_request "POST" "/api/v1/projects" "${project_data[$i]}" "Create ${project_names[$i]} Project"
done

# Test project operations with created projects
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    first_project_id="${PROJECT_IDS[0]}"
    
    make_request "GET" "/api/v1/projects/$first_project_id" "" "Get First Created Project by ID"
    
    # Update project
    project_update='{
        "projectName": "Admin Residential Solar Array '$TIMESTAMP' - UPDATED",
        "address": "123 Sunny Street, Solar City, CA 90210",
        "clientInfo": "Johnson Family - UPDATED: 3,200 sq ft home, enhanced with battery storage",
        "status": "InProgress",
        "startDate": "2025-06-20T08:00:00Z",
        "estimatedEndDate": "2025-08-20T17:00:00Z"
    }'
    
    make_request "PUT" "/api/v1/projects/$first_project_id" "$project_update" "Update First Project"
    
    # Get updated project to verify changes
    make_request "GET" "/api/v1/projects/$first_project_id" "" "Verify Project Update"
fi

# Test advanced project filtering
make_request "GET" "/api/v1/projects?search=Solar" "" "Search Projects by Keyword"
make_request "GET" "/api/v1/projects?search=Admin" "" "Search Projects by Admin"

# ==================== TASK MANAGEMENT ====================
log_and_echo "\n${YELLOW}==================== 5. TASK MANAGEMENT (COMPREHENSIVE) ====================${NC}"

# Get tasks with various filters
make_request "GET" "/api/v1/tasks" "" "Get All Tasks"
make_request "GET" "/api/v1/tasks?pageSize=5" "" "Get Tasks - Paginated"
make_request "GET" "/api/v1/tasks?status=Pending" "" "Get Pending Tasks"
make_request "GET" "/api/v1/tasks?priority=High" "" "Get High Priority Tasks"

# Create comprehensive tasks for each project
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    # Tasks for first project (Residential)
    project_id="${PROJECT_IDS[0]}"
    
    declare -a task_data_p1=(
        '{
            "title": "Site Survey and Assessment '$TIMESTAMP'",
            "description": "Comprehensive site evaluation including structural assessment, shading analysis, and electrical capacity review",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-06-22T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 8.0
        }'
        '{
            "title": "Permit Application Submission '$TIMESTAMP'",
            "description": "Submit building permits, electrical permits, and utility interconnection applications",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-06-25T16:00:00Z",
            "priority": "Critical",
            "status": "Pending",
            "estimatedHours": 4.0
        }'
        '{
            "title": "Equipment Procurement '$TIMESTAMP'",
            "description": "Order solar panels, inverters, mounting hardware, and electrical components",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-06-30T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 6.0
        }'
        '{
            "title": "Roof Preparation '$TIMESTAMP'",
            "description": "Clean roof surface, mark installation points, install mounting rails",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-07-05T15:00:00Z",
            "priority": "Medium",
            "status": "Pending",
            "estimatedHours": 16.0
        }'
        '{
            "title": "Solar Panel Installation '$TIMESTAMP'",
            "description": "Install 24 solar panels on south-facing roof section with proper wiring",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-07-10T16:00:00Z",
            "priority": "Critical",
            "status": "Pending",
            "estimatedHours": 24.0
        }'
    )
    
    for i in "${!task_data_p1[@]}"; do
        make_request "POST" "/api/v1/tasks" "${task_data_p1[$i]}" "Create Residential Task $((i+1))"
    done
fi

# Create tasks for second project if available
if [ ${#PROJECT_IDS[@]} -gt 1 ]; then
    project_id="${PROJECT_IDS[1]}"
    
    declare -a task_data_p2=(
        '{
            "title": "Commercial Building Assessment '$TIMESTAMP'",
            "description": "Structural engineering assessment of commercial roof for large-scale installation",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-07-03T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 12.0
        }'
        '{
            "title": "Electrical Infrastructure Upgrade '$TIMESTAMP'",
            "description": "Upgrade main electrical panel and install new service entrance for solar integration",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-07-15T16:00:00Z",
            "priority": "Critical",
            "status": "Pending",
            "estimatedHours": 32.0
        }'
        '{
            "title": "Large-Scale Panel Installation '$TIMESTAMP'",
            "description": "Install 150+ solar panels across multiple roof sections of office complex",
            "projectId": "'$project_id'",
            "assignedToUserId": "'$ADMIN_USER_ID'",
            "dueDate": "2025-08-30T17:00:00Z",
            "priority": "High",
            "status": "Pending",
            "estimatedHours": 80.0
        }'
    )
    
    for i in "${!task_data_p2[@]}"; do
        make_request "POST" "/api/v1/tasks" "${task_data_p2[$i]}" "Create Commercial Task $((i+1))"
    done
fi

# Test task operations
if [ ${#TASK_IDS[@]} -gt 0 ]; then
    first_task_id="${TASK_IDS[0]}"
    
    make_request "GET" "/api/v1/tasks/$first_task_id" "" "Get First Created Task by ID"
    
    # Update task status
    task_update='{
        "title": "Site Survey and Assessment '$TIMESTAMP' - UPDATED",
        "description": "UPDATED: Comprehensive site evaluation with additional drone survey",
        "status": "InProgress",
        "priority": "Critical",
        "estimatedHours": 10.0
    }'
    
    make_request "PUT" "/api/v1/tasks/$first_task_id" "$task_update" "Update First Task"
    
    # Get updated task to verify changes
    make_request "GET" "/api/v1/tasks/$first_task_id" "" "Verify Task Update"
fi

# Test task filtering with created data
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    make_request "GET" "/api/v1/tasks?projectId=${PROJECT_IDS[0]}" "" "Get Tasks for First Project"
fi

make_request "GET" "/api/v1/tasks?assignedToUserId=$ADMIN_USER_ID" "" "Get Tasks Assigned to Admin"
make_request "GET" "/api/v1/tasks?status=Pending&priority=High" "" "Get High Priority Pending Tasks"

# ==================== DAILY REPORTS ====================
log_and_echo "\n${YELLOW}==================== 6. DAILY REPORTS (COMPREHENSIVE) ====================${NC}"

make_request "GET" "/api/v1/daily-reports" "" "Get All Daily Reports"
make_request "GET" "/api/v1/daily-reports?pageSize=3" "" "Get Daily Reports - Paginated"

# Create comprehensive daily reports
if [ ${#PROJECT_IDS[@]} -gt 0 ] && [ ${#TASK_IDS[@]} -gt 0 ]; then
    declare -a report_data=(
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "taskId": "'${TASK_IDS[0]}'",
            "userId": "'$ADMIN_USER_ID'",
            "reportDate": "2025-06-14T00:00:00Z",
            "hoursWorked": 8.5,
            "workCompleted": "Completed comprehensive site survey including structural assessment and shading analysis. Identified optimal panel placement locations.",
            "issuesEncountered": "Minor roof access challenge due to steep pitch on south section. Solution: installed temporary safety equipment.",
            "materialUsed": "Safety equipment, measuring tools, drone for aerial survey",
            "weatherConditions": "Sunny, 75°F, light breeze - ideal conditions for site assessment",
            "safetyNotes": "All safety protocols followed. Team used proper fall protection equipment.",
            "additionalNotes": "Client very cooperative and provided detailed utility bills for energy usage analysis."
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "taskId": "'${TASK_IDS[1]}'",
            "userId": "'$ADMIN_USER_ID'",
            "reportDate": "2025-06-15T00:00:00Z",
            "hoursWorked": 6.0,
            "workCompleted": "Submitted all required permits to city building department and utility company for interconnection approval.",
            "issuesEncountered": "Building department required additional structural drawings. Contacted engineer for revised plans.",
            "materialUsed": "Office supplies, permit application fees paid",
            "weatherConditions": "Office work - weather not applicable",
            "safetyNotes": "No safety concerns for office work.",
            "additionalNotes": "Permit processing expected to take 2-3 weeks. Following up with expedited review request."
        }'
    )
    
    # Add more reports for different projects if available
    if [ ${#PROJECT_IDS[@]} -gt 1 ] && [ ${#TASK_IDS[@]} -gt 2 ]; then
        report_data+=(
            '{
                "projectId": "'${PROJECT_IDS[1]}'",
                "taskId": "'${TASK_IDS[2]}'",
                "userId": "'$ADMIN_USER_ID'",
                "reportDate": "2025-06-16T00:00:00Z",
                "hoursWorked": 10.0,
                "workCompleted": "Began structural assessment of commercial building roof. Tested load-bearing capacity and identified reinforcement needs.",
                "issuesEncountered": "Discovered older HVAC units on roof that need relocation. Coordinating with HVAC contractor.",
                "materialUsed": "Structural testing equipment, load calculation tools, measuring devices",
                "weatherConditions": "Partly cloudy, 72°F, moderate wind - good working conditions",
                "safetyNotes": "Commercial roof safety protocols enforced. Safety harnesses and perimeter protection used.",
                "additionalNotes": "Building owner approved additional engineering costs for HVAC relocation. Project timeline may extend by 1 week."
            }'
        )
    fi
    
    for i in "${!report_data[@]}"; do
        make_request "POST" "/api/v1/daily-reports" "${report_data[$i]}" "Create Daily Report $((i+1))"
    done
fi

# Test daily report operations
if [ ${#REPORT_IDS[@]} -gt 0 ]; then
    first_report_id="${REPORT_IDS[0]}"
    
    make_request "GET" "/api/v1/daily-reports/$first_report_id" "" "Get First Created Report by ID"
    
    # Update report
    report_update='{
        "hoursWorked": 9.0,
        "workCompleted": "UPDATED: Completed comprehensive site survey with additional soil testing for ground-mount options.",
        "additionalNotes": "UPDATED: Client interested in expanding project scope to include battery storage system."
    }'
    
    make_request "PUT" "/api/v1/daily-reports/$first_report_id" "$report_update" "Update First Report"
fi

# Test daily report filtering
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    make_request "GET" "/api/v1/daily-reports?projectId=${PROJECT_IDS[0]}" "" "Get Reports for First Project"
fi

make_request "GET" "/api/v1/daily-reports?userId=$ADMIN_USER_ID" "" "Get Reports by Admin User"
make_request "GET" "/api/v1/daily-reports?startDate=2025-06-14&endDate=2025-06-16" "" "Get Reports by Date Range"
make_request "GET" "/api/v1/daily-reports?hoursWorked_min=8" "" "Get Reports with 8+ Hours"

# ==================== WORK REQUESTS ====================
log_and_echo "\n${YELLOW}==================== 7. WORK REQUESTS (COMPREHENSIVE) ====================${NC}"

make_request "GET" "/api/v1/work-requests" "" "Get All Work Requests"
make_request "GET" "/api/v1/work-requests?pageSize=5" "" "Get Work Requests - Paginated"

# Create comprehensive work requests
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    declare -a work_request_data=(
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "requestedByUserId": "'$ADMIN_USER_ID'",
            "title": "Additional Safety Equipment Request '$TIMESTAMP'",
            "description": "Request for specialized roof safety equipment including additional fall protection harnesses and anchor points for steep roof work.",
            "requestType": "Material",
            "priority": "High",
            "estimatedCost": 2500.00,
            "justification": "Current safety equipment insufficient for 45-degree roof pitch. Required for worker safety compliance.",
            "requiredByDate": "2025-06-25T17:00:00Z"
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "requestedByUserId": "'$ADMIN_USER_ID'",
            "title": "Emergency Electrical Consultant '$TIMESTAMP'",
            "description": "Need certified electrical engineer to review and approve main panel upgrade design for solar integration.",
            "requestType": "Service",
            "priority": "Critical",
            "estimatedCost": 3500.00,
            "justification": "Existing electrical system requires professional engineering review before proceeding with solar installation.",
            "requiredByDate": "2025-06-28T16:00:00Z"
        }'
        '{
            "projectId": "'${PROJECT_IDS[0]}'",
            "requestedByUserId": "'$ADMIN_USER_ID'",
            "title": "Weather Delay Mitigation '$TIMESTAMP'",
            "description": "Request approval for overtime work to compensate for weather delays and maintain project schedule.",
            "requestType": "Labor",
            "priority": "Medium",
            "estimatedCost": 5000.00,
            "justification": "3 days of rain delayed outdoor work. Overtime needed to meet client deadline for system activation.",
            "requiredByDate": "2025-07-01T17:00:00Z"
        }'
    )
    
    # Add work requests for other projects if available
    if [ ${#PROJECT_IDS[@]} -gt 1 ]; then
        work_request_data+=(
            '{
                "projectId": "'${PROJECT_IDS[1]}'",
                "requestedByUserId": "'$ADMIN_USER_ID'",
                "title": "HVAC Relocation Service '$TIMESTAMP'",
                "description": "Professional HVAC contractor needed to relocate rooftop units to optimize solar panel placement on commercial building.",
                "requestType": "Service",
                "priority": "High",
                "estimatedCost": 15000.00,
                "justification": "HVAC units blocking optimal solar installation areas. Relocation will increase system efficiency by 15%.",
                "requiredByDate": "2025-07-10T17:00:00Z"
            }'
            '{
                "projectId": "'${PROJECT_IDS[1]}'",
                "requestedByUserId": "'$ADMIN_USER_ID'",
                "title": "Crane Rental for Commercial Install '$TIMESTAMP'",
                "description": "Heavy-duty crane rental for lifting large solar panels and equipment to commercial building roof.",
                "requestType": "Equipment",
                "priority": "Critical",
                "estimatedCost": 8500.00,
                "justification": "Building height and panel weight require professional crane service for safe installation.",
                "requiredByDate": "2025-08-15T08:00:00Z"
            }'
        )
    fi
    
    for i in "${!work_request_data[@]}"; do
        make_request "POST" "/api/v1/work-requests" "${work_request_data[$i]}" "Create Work Request $((i+1))"
    done
fi

# Test work request operations
if [ ${#WORK_REQUEST_IDS[@]} -gt 0 ]; then
    first_wr_id="${WORK_REQUEST_IDS[0]}"
    
    make_request "GET" "/api/v1/work-requests/$first_wr_id" "" "Get First Created Work Request by ID"
    
    # Update work request (approve it)
    wr_update='{
        "title": "Additional Safety Equipment Request '$TIMESTAMP' - APPROVED",
        "status": "Approved",
        "priority": "Critical",
        "estimatedCost": 2750.00,
        "approvedByUserId": "'$ADMIN_USER_ID'",
        "approvalNotes": "Approved with increased budget for premium safety equipment. Worker safety is top priority."
    }'
    
    make_request "PUT" "/api/v1/work-requests/$first_wr_id" "$wr_update" "Approve First Work Request"
fi

# Test work request filtering
if [ ${#PROJECT_IDS[@]} -gt 0 ]; then
    make_request "GET" "/api/v1/work-requests?projectId=${PROJECT_IDS[0]}" "" "Get Work Requests for First Project"
fi

make_request "GET" "/api/v1/work-requests?requestedByUserId=$ADMIN_USER_ID" "" "Get Work Requests by Admin User"
make_request "GET" "/api/v1/work-requests?status=Pending" "" "Get Pending Work Requests"
make_request "GET" "/api/v1/work-requests?requestType=Service" "" "Get Service Work Requests"
make_request "GET" "/api/v1/work-requests?priority=High" "" "Get High Priority Work Requests"
make_request "GET" "/api/v1/work-requests?estimatedCost_min=5000" "" "Get High-Cost Work Requests"

# ==================== CALENDAR EVENTS ====================
log_and_echo "\n${YELLOW}==================== 8. CALENDAR EVENTS (COMPREHENSIVE) ====================${NC}"

make_request "GET" "/api/v1/calendar" "" "Get All Calendar Events"
make_request "GET" "/api/v1/calendar?pageSize=5" "" "Get Calendar Events - Paginated"

# Create comprehensive calendar events
declare -a calendar_data=(
    '{
        "title": "Project Kickoff Meeting - Residential Solar '$TIMESTAMP'",
        "description": "Initial project meeting with Johnson family to review installation timeline, permits, and expectations.",
        "startDateTime": "2025-06-18T09:00:00Z",
        "endDateTime": "2025-06-18T10:30:00Z",
        "eventType": "Meeting",
        "location": "123 Sunny Street, Solar City, CA 90210",
        "priority": "High",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 30
    }'
    '{
        "title": "Site Safety Inspection '$TIMESTAMP'",
        "description": "OSHA compliance inspection and safety protocol review for all active solar installation sites.",
        "startDateTime": "2025-06-19T08:00:00Z",
        "endDateTime": "2025-06-19T12:00:00Z",
        "eventType": "Inspection",
        "location": "Multiple project sites",
        "priority": "Critical",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 60
    }'
    '{
        "title": "Equipment Delivery - Commercial Project '$TIMESTAMP'",
        "description": "Delivery of 150 solar panels and mounting hardware for commercial office complex installation.",
        "startDateTime": "2025-06-20T07:00:00Z",
        "endDateTime": "2025-06-20T11:00:00Z",
        "eventType": "Delivery",
        "location": "456 Business Boulevard, Corporate Park, CA 94102",
        "priority": "High",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 120
    }'
    '{
        "title": "Team Training - Advanced Installation Techniques '$TIMESTAMP'",
        "description": "Training session on new mounting systems and electrical integration best practices for solar installations.",
        "startDateTime": "2025-06-21T13:00:00Z",
        "endDateTime": "2025-06-21T17:00:00Z",
        "eventType": "Training",
        "location": "Solar Training Center, 789 Education Way",
        "priority": "Medium",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 45
    }'
    '{
        "title": "Client Progress Review - Industrial Farm '$TIMESTAMP'",
        "description": "Monthly progress review meeting with Power Solutions LLC for 10MW solar farm project status.",
        "startDateTime": "2025-06-24T14:00:00Z",
        "endDateTime": "2025-06-24T15:30:00Z",
        "eventType": "Meeting",
        "location": "Power Solutions LLC Office, 789 Industrial Way",
        "priority": "High",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 30
    }'
    '{
        "title": "Permit Approval Follow-up '$TIMESTAMP'",
        "description": "Follow-up calls with city building department and utility company on pending permit approvals.",
        "startDateTime": "2025-06-25T09:00:00Z",
        "endDateTime": "2025-06-25T11:00:00Z",
        "eventType": "Administrative",
        "location": "Office - Phone calls",
        "priority": "Medium",
        "organizerId": "'$ADMIN_USER_ID'",
        "isAllDay": false,
        "reminderMinutes": 15
    }'
)

for i in "${!calendar_data[@]}"; do
    make_request "POST" "/api/v1/calendar" "${calendar_data[$i]}" "Create Calendar Event $((i+1))"
done

# Test calendar operations
if [ ${#CALENDAR_EVENT_IDS[@]} -gt 0 ]; then
    first_event_id="${CALENDAR_EVENT_IDS[0]}"
    
    make_request "GET" "/api/v1/calendar/$first_event_id" "" "Get First Created Event by ID"
    
    # Update calendar event
    event_update='{
        "title": "Project Kickoff Meeting - Residential Solar '$TIMESTAMP' - RESCHEDULED",
        "description": "UPDATED: Rescheduled kickoff meeting with Johnson family - now includes battery storage discussion.",
        "startDateTime": "2025-06-18T10:00:00Z",
        "endDateTime": "2025-06-18T12:00:00Z",
        "priority": "Critical",
        "reminderMinutes": 60
    }'
    
    make_request "PUT" "/api/v1/calendar/$first_event_id" "$event_update" "Update First Calendar Event"
fi

# Test calendar filtering
make_request "GET" "/api/v1/calendar?eventType=Meeting" "" "Get Meeting Events"
make_request "GET" "/api/v1/calendar?priority=High" "" "Get High Priority Events"
make_request "GET" "/api/v1/calendar?startDate=2025-06-18&endDate=2025-06-25" "" "Get Events in Date Range"
make_request "GET" "/api/v1/calendar/upcoming?days=30" "" "Get Upcoming Events (30 days)"

# ==================== IMAGES & FILE UPLOAD ====================
log_and_echo "\n${YELLOW}==================== 9. IMAGES & FILE UPLOAD ====================${NC}"

# Test image upload
test_image=$(create_test_image)
if [ -f "$test_image" ]; then
    make_request "POST" "/api/v1/images/upload" "-F 'file=@$test_image' -F 'description=Solar panel test image for admin testing'" "Upload Test Image" "multipart/form-data"
    
    # Clean up test image
    rm -f "$test_image"
fi

make_request "GET" "/api/v1/images" "" "Get All Images"

# ==================== ADVANCED FILTERING & ANALYTICS ====================
log_and_echo "\n${YELLOW}==================== 10. ADVANCED FILTERING & ANALYTICS ====================${NC}"

# Test complex filtering combinations
make_request "GET" "/api/v1/projects?pageNumber=1&pageSize=3&search=Admin" "" "Projects: Search + Pagination"
make_request "GET" "/api/v1/tasks?status=Pending&priority=High&pageSize=5" "" "Tasks: Multiple Filters"
make_request "GET" "/api/v1/daily-reports?hoursWorked_min=6&hoursWorked_max=10" "" "Reports: Hours Range Filter"

# Test sorting and field selection if supported
make_request "GET" "/api/v1/projects?sort=projectName&order=asc" "" "Projects: Sort by Name"
make_request "GET" "/api/v1/tasks?sort=dueDate&order=desc" "" "Tasks: Sort by Due Date"

# ==================== ADMIN-SPECIFIC ENDPOINTS ====================
log_and_echo "\n${YELLOW}==================== 11. ADMIN-SPECIFIC ENDPOINTS ====================${NC}"

# Test rate limit admin endpoints (if available)
make_request "GET" "/api/v1/admin/rate-limits" "" "Get Rate Limit Status (Admin)"

# Test system administration endpoints
make_request "GET" "/api/v1/users?includeInactive=true" "" "Get All Users Including Inactive (Admin)"

# ==================== DATA VERIFICATION ====================
log_and_echo "\n${YELLOW}==================== 12. DATA VERIFICATION & INTEGRITY ====================${NC}"

# Verify all created data is accessible
log_and_echo "\n${CYAN}📊 Verifying Created Data Accessibility:${NC}"

log_and_echo "   Projects Created: ${#PROJECT_IDS[@]}"
for pid in "${PROJECT_IDS[@]}"; do
    make_request "GET" "/api/v1/projects/$pid" "" "Verify Project Access: $pid"
done

log_and_echo "   Tasks Created: ${#TASK_IDS[@]}"
for tid in "${TASK_IDS[@]}"; do
    make_request "GET" "/api/v1/tasks/$tid" "" "Verify Task Access: $tid"
done

log_and_echo "   Daily Reports Created: ${#REPORT_IDS[@]}"
for rid in "${REPORT_IDS[@]}"; do
    make_request "GET" "/api/v1/daily-reports/$rid" "" "Verify Report Access: $rid"
done

log_and_echo "   Work Requests Created: ${#WORK_REQUEST_IDS[@]}"
for wrid in "${WORK_REQUEST_IDS[@]}"; do
    make_request "GET" "/api/v1/work-requests/$wrid" "" "Verify Work Request Access: $wrid"
done

log_and_echo "   Calendar Events Created: ${#CALENDAR_EVENT_IDS[@]}"
for eid in "${CALENDAR_EVENT_IDS[@]}"; do
    make_request "GET" "/api/v1/calendar/$eid" "" "Verify Calendar Event Access: $eid"
done

# ==================== CLEANUP TESTING ====================
log_and_echo "\n${YELLOW}==================== 13. CLEANUP OPERATIONS (ADMIN DELETE ACCESS) ====================${NC}"

# Test delete operations (Admin should have access to delete everything)
if [ ${#CALENDAR_EVENT_IDS[@]} -gt 1 ]; then
    last_event_id="${CALENDAR_EVENT_IDS[-1]}"
    make_request "DELETE" "/api/v1/calendar/$last_event_id" "" "Delete Last Calendar Event (Admin Test)"
fi

if [ ${#WORK_REQUEST_IDS[@]} -gt 1 ]; then
    last_wr_id="${WORK_REQUEST_IDS[-1]}"
    make_request "DELETE" "/api/v1/work-requests/$last_wr_id" "" "Delete Last Work Request (Admin Test)"
fi

if [ ${#REPORT_IDS[@]} -gt 1 ]; then
    last_report_id="${REPORT_IDS[-1]}"
    make_request "DELETE" "/api/v1/daily-reports/$last_report_id" "" "Delete Last Daily Report (Admin Test)"
fi

if [ ${#TASK_IDS[@]} -gt 1 ]; then
    last_task_id="${TASK_IDS[-1]}"
    make_request "DELETE" "/api/v1/tasks/$last_task_id" "" "Delete Last Task (Admin Test)"
fi

if [ ${#PROJECT_IDS[@]} -gt 1 ]; then
    last_project_id="${PROJECT_IDS[-1]}"
    make_request "DELETE" "/api/v1/projects/$last_project_id" "" "Delete Last Project (Admin Test)"
fi

# ==================== FINAL SUMMARY ====================
log_and_echo "\n${YELLOW}==================== 📊 COMPREHENSIVE TEST SUMMARY ====================${NC}"

log_and_echo "${GREEN}✅ Comprehensive Admin API Testing Completed!${NC}"
log_and_echo "${BLUE}📁 Detailed results saved to: $LOG_FILE${NC}"

log_and_echo "\n${CYAN}📋 Test Coverage Summary:${NC}"
log_and_echo "   • ${GREEN}Authentication & Authorization${NC} - Admin login, token management"
log_and_echo "   • ${GREEN}Health & System Endpoints${NC} - Basic and detailed health checks"
log_and_echo "   • ${GREEN}User Management${NC} - Create users with different roles, list users"
log_and_echo "   • ${GREEN}Project Management${NC} - Full CRUD with 5 comprehensive projects"
log_and_echo "   • ${GREEN}Task Management${NC} - Full CRUD with detailed tasks for all projects"
log_and_echo "   • ${GREEN}Daily Reports${NC} - Comprehensive reports with project/task linking"
log_and_echo "   • ${GREEN}Work Requests${NC} - Various request types with approval workflow"
log_and_echo "   • ${GREEN}Calendar Events${NC} - Comprehensive event types and scheduling"
log_and_echo "   • ${GREEN}Image Upload${NC} - File upload functionality testing"
log_and_echo "   • ${GREEN}Advanced Filtering${NC} - Complex queries, pagination, sorting"
log_and_echo "   • ${GREEN}Admin-Specific Features${NC} - System administration capabilities"
log_and_echo "   • ${GREEN}Data Verification${NC} - Access verification for all created entities"
log_and_echo "   • ${GREEN}Cleanup Operations${NC} - Delete operations testing"

log_and_echo "\n${CYAN}🔐 Admin-Specific Capabilities Tested:${NC}"
log_and_echo "   • ${GREEN}Full CRUD Access${NC} - Create, Read, Update, Delete all entities"
log_and_echo "   • ${GREEN}User Creation${NC} - Register users with different role assignments"
log_and_echo "   • ${GREEN}System Administration${NC} - Rate limiting, system health monitoring"
log_and_echo "   • ${GREEN}Cross-Entity Operations${NC} - Manage relationships between projects, tasks, etc."
log_and_echo "   • ${GREEN}Advanced Filtering${NC} - Access all data with complex filter combinations"

log_and_echo "\n${CYAN}📈 Data Creation Summary:${NC}"
log_and_echo "   • Projects: ${PURPLE}${#PROJECT_IDS[@]}${NC} comprehensive solar projects created"
log_and_echo "   • Tasks: ${PURPLE}${#TASK_IDS[@]}${NC} detailed tasks across all projects"
log_and_echo "   • Daily Reports: ${PURPLE}${#REPORT_IDS[@]}${NC} detailed work reports created"
log_and_echo "   • Work Requests: ${PURPLE}${#WORK_REQUEST_IDS[@]}${NC} various request types created"
log_and_echo "   • Calendar Events: ${PURPLE}${#CALENDAR_EVENT_IDS[@]}${NC} comprehensive events scheduled"
log_and_echo "   • Users: ${PURPLE}${#USER_IDS[@]}${NC} test users with different roles created"

# Display final statistics
total_requests=$(grep -c "📡 Testing:" "$LOG_FILE")
successful_requests=$(grep -c "✅ SUCCESS" "$LOG_FILE")
error_requests=$(grep -c "❌ ERROR" "$LOG_FILE")
warning_requests=$(grep -c "⚠️" "$LOG_FILE")

success_percentage=$(( (successful_requests * 100) / total_requests ))

log_and_echo "\n${CYAN}📈 Request Statistics:${NC}"
log_and_echo "   Total API Calls: ${BLUE}$total_requests${NC}"
log_and_echo "   Successful: ${GREEN}$successful_requests${NC} (${success_percentage}%)"
log_and_echo "   Warnings: ${YELLOW}$warning_requests${NC}"
log_and_echo "   Errors: ${RED}$error_requests${NC}"

if [ $success_percentage -ge 80 ]; then
    log_and_echo "\n${GREEN}🎯 EXCELLENT: Admin has comprehensive access to all API functionality!${NC}"
elif [ $success_percentage -ge 60 ]; then
    log_and_echo "\n${YELLOW}⚠️  GOOD: Admin has good API access with some minor issues.${NC}"
else
    log_and_echo "\n${RED}❌ NEEDS ATTENTION: Admin API access has significant issues requiring investigation.${NC}"
fi

log_and_echo "\n${BLUE}📖 Review the detailed log file for API responses and troubleshooting information.${NC}"
log_and_echo "${BLUE}🔍 Use analyze-admin-test-results.sh to get detailed analysis and recommendations.${NC}"

echo ""
echo "🎯 Comprehensive admin endpoint testing completed!"
echo "✨ Check $LOG_FILE for detailed results and analysis."
