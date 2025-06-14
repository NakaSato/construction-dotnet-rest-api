#!/bin/bash

# Solar Projects API - Admin Endpoint Testing Script
# This script tests ALL available API endpoints using Admin credentials
# Admin has full access to all endpoints and operations

echo "🔐 SOLAR PROJECTS API - ADMIN ENDPOINT TESTING"
echo "=============================================="

# Configuration
API_BASE="http://localhost:5002"
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"
OUTPUT_DIR="./test-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$OUTPUT_DIR/admin_test_$TIMESTAMP.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Initialize log file
echo "Admin API Testing - $TIMESTAMP" > "$LOG_FILE"
echo "================================" >> "$LOG_FILE"

# Function to log and display
log_and_echo() {
    echo -e "$1"
    echo -e "$1" | sed 's/\x1B\[[0-9;]*[JKmsu]//g' >> "$LOG_FILE"
}

# Function to make API request and log response
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    local content_type=${5:-"application/json"}
    
    log_and_echo "\n${BLUE}📡 Testing: $description${NC}"
    log_and_echo "   Method: $method"
    log_and_echo "   Endpoint: $endpoint"
    
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
    
    if [ "$status" -ge 200 ] && [ "$status" -lt 300 ]; then
        log_and_echo "   ${GREEN}✅ SUCCESS - Status: $status${NC}"
    elif [ "$status" -ge 400 ] && [ "$status" -lt 500 ]; then
        log_and_echo "   ${YELLOW}⚠️  CLIENT ERROR - Status: $status${NC}"
    else
        log_and_echo "   ${RED}❌ ERROR - Status: $status${NC}"
    fi
    
    # Log response body (truncated for readability)
    echo "   Response (first 200 chars): $(echo "$body" | cut -c1-200)..." >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
    
    # Store important IDs for later use
    if echo "$body" | jq -e '.success == true and .data.id' > /dev/null 2>&1; then
        local entity_id=$(echo "$body" | jq -r '.data.id')
        case "$endpoint" in
            */projects) PROJECT_ID="$entity_id" ;;
            */tasks) TASK_ID="$entity_id" ;;
            */daily-reports) REPORT_ID="$entity_id" ;;
            */work-requests) WORK_REQUEST_ID="$entity_id" ;;
            */calendar) CALENDAR_EVENT_ID="$entity_id" ;;
            */auth/register) USER_ID="$entity_id" ;;
            */images/upload) IMAGE_ID="$entity_id" ;;
        esac
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

# Test 1: Health Endpoints (No Auth Required)
log_and_echo "\n${YELLOW}==================== 1. HEALTH ENDPOINTS ====================${NC}"

make_request "GET" "/health" "" "Basic Health Check"
make_request "GET" "/health/detailed" "" "Detailed Health Check"

# Test 2: Authentication
log_and_echo "\n${YELLOW}==================== 2. AUTHENTICATION ====================${NC}"

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

# Test refresh token
refresh_data='{"refreshToken": "'$REFRESH_TOKEN'"}'
make_request "POST" "/api/v1/auth/refresh" "$refresh_data" "Refresh Token"

# Test 3: User Management (Admin Only)
log_and_echo "\n${YELLOW}==================== 3. USER MANAGEMENT (ADMIN ONLY) ====================${NC}"

# Create a test user
test_user_data='{
    "username": "admin_test_user_'$TIMESTAMP'",
    "email": "admin_test_'$TIMESTAMP'@example.com",
    "password": "TestUser123!",
    "fullName": "Admin Created Test User",
    "roleId": 3
}'

make_request "POST" "/api/v1/auth/register" "$test_user_data" "Create New User"

# Get all users (if endpoint exists)
make_request "GET" "/api/v1/users" "" "Get All Users"

# Test 4: Project Management
log_and_echo "\n${YELLOW}==================== 4. PROJECT MANAGEMENT ====================${NC}"

# Get all projects
make_request "GET" "/api/v1/projects" "" "Get All Projects"
make_request "GET" "/api/v1/projects?pageSize=5&status=Active" "" "Get Active Projects (Filtered)"

# Create multiple test projects with proper address field
project_data_1='{
    "name": "Admin Solar Installation Project '$TIMESTAMP'",
    "description": "Large-scale solar installation for residential complex",
    "startDate": "2025-06-15",
    "endDate": "2025-08-30",
    "location": "Sunnydale Residential Complex",
    "address": "123 Solar Avenue, Sunnydale, CA 90210",
    "budget": 250000.00
}'

make_request "POST" "/api/v1/projects" "$project_data_1" "Create Solar Installation Project"
PROJECT_ID_1="$PROJECT_ID"

project_data_2='{
    "name": "Admin Commercial Solar Project '$TIMESTAMP'",
    "description": "Commercial rooftop solar system installation",
    "startDate": "2025-07-01",
    "endDate": "2025-09-15",
    "location": "Business District Office Complex",
    "address": "456 Commerce Street, San Francisco, CA 94102",
    "budget": 500000.00
}'

make_request "POST" "/api/v1/projects" "$project_data_2" "Create Commercial Solar Project"
PROJECT_ID_2="$PROJECT_ID"

project_data_3='{
    "name": "Admin Emergency Repair Project '$TIMESTAMP'",
    "description": "Emergency solar panel repair and maintenance",
    "startDate": "2025-06-16",
    "endDate": "2025-06-20",
    "location": "Industrial Solar Farm",
    "address": "789 Power Plant Road, Oakland, CA 94601",
    "budget": 75000.00
}'

make_request "POST" "/api/v1/projects" "$project_data_3" "Create Emergency Repair Project"
PROJECT_ID_3="$PROJECT_ID"

# Test project-specific endpoints with created projects
if [ ! -z "$PROJECT_ID_1" ]; then
    make_request "GET" "/api/v1/projects/$PROJECT_ID_1" "" "Get Solar Installation Project by ID"
    
    # Update project
    project_update_data='{
        "name": "Admin Solar Installation Project '$TIMESTAMP' - Updated",
        "description": "Updated large-scale solar installation with expanded scope",
        "budget": 275000.00,
        "address": "123 Solar Avenue, Sunnydale, CA 90210"
    }'
    
    make_request "PUT" "/api/v1/projects/$PROJECT_ID_1" "$project_update_data" "Update Solar Installation Project"
fi

# Test advanced project filtering
make_request "GET" "/api/v1/projects?search=Solar" "" "Search Projects by Name"
make_request "GET" "/api/v1/projects?budget_min=100000&budget_max=300000" "" "Filter Projects by Budget Range"

# Test 5: Task Management
log_and_echo "\n${YELLOW}==================== 5. TASK MANAGEMENT ====================${NC}"

# Get all tasks
make_request "GET" "/api/v1/tasks" "" "Get All Tasks"
make_request "GET" "/api/v1/tasks?pageSize=10&status=Pending" "" "Get Pending Tasks"

# Create multiple tasks for different projects
if [ ! -z "$PROJECT_ID_1" ]; then
    task_data_1='{
        "title": "Site Preparation and Assessment '$TIMESTAMP'",
        "description": "Conduct site survey, soil testing, and electrical assessment for solar installation",
        "projectId": "'$PROJECT_ID_1'",
        "assignedToUserId": "'$ADMIN_USER_ID'",
        "dueDate": "2025-06-25",
        "priority": "High",
        "status": "Pending",
        "estimatedHours": 16.0
    }'
    
    make_request "POST" "/api/v1/tasks" "$task_data_1" "Create Site Preparation Task"
    TASK_ID_1="$TASK_ID"
    
    task_data_2='{
        "title": "Solar Panel Installation Phase 1 '$TIMESTAMP'",
        "description": "Install first batch of 24 solar panels on south-facing roof section",
        "projectId": "'$PROJECT_ID_1'",
        "assignedToUserId": "'$ADMIN_USER_ID'",
        "dueDate": "2025-07-10",
        "priority": "High",
        "status": "Pending",
        "estimatedHours": 32.0
    }'
    
    make_request "POST" "/api/v1/tasks" "$task_data_2" "Create Solar Panel Installation Task"
    TASK_ID_2="$TASK_ID"
    
    task_data_3='{
        "title": "Electrical Wiring and Connection '$TIMESTAMP'",
        "description": "Complete electrical connections, inverter installation, and grid connection",
        "projectId": "'$PROJECT_ID_1'",
        "assignedToUserId": "'$ADMIN_USER_ID'",
        "dueDate": "2025-07-20",
        "priority": "Critical",
        "status": "Pending",
        "estimatedHours": 24.0
    }'
    
    make_request "POST" "/api/v1/tasks" "$task_data_3" "Create Electrical Wiring Task"
    TASK_ID_3="$TASK_ID"
fi

# Create tasks for second project
if [ ! -z "$PROJECT_ID_2" ]; then
    task_data_4='{
        "title": "Commercial Roof Assessment '$TIMESTAMP'",
        "description": "Structural assessment of commercial building roof for solar installation",
        "projectId": "'$PROJECT_ID_2'",
        "assignedToUserId": "'$ADMIN_USER_ID'",
        "dueDate": "2025-07-05",
        "priority": "Medium",
        "status": "Pending",
        "estimatedHours": 8.0
    }'
    
    make_request "POST" "/api/v1/tasks" "$task_data_4" "Create Commercial Roof Assessment Task"
    TASK_ID_4="$TASK_ID"
fi

# Test task-specific endpoints
if [ ! -z "$TASK_ID_1" ]; then
    make_request "GET" "/api/v1/tasks/$TASK_ID_1" "" "Get Site Preparation Task by ID"
    
    # Update task status and progress
    task_update_data='{
        "title": "Site Preparation and Assessment '$TIMESTAMP' - In Progress",
        "status": "InProgress",
        "progressPercentage": 35.0,
        "actualHours": 6.0
    }'
    
    make_request "PUT" "/api/v1/tasks/$TASK_ID_1" "$task_update_data" "Update Task Status to In Progress"
fi

# Test task filtering and admin access
make_request "GET" "/api/v1/tasks?projectId=$PROJECT_ID_1" "" "Get Tasks by Project"
make_request "GET" "/api/v1/tasks?assignedToUserId=$ADMIN_USER_ID" "" "Get Tasks Assigned to Admin"
make_request "GET" "/api/v1/tasks?priority=High" "" "Get High Priority Tasks"
make_request "GET" "/api/v1/tasks?status=Pending&pageSize=20" "" "Get All Pending Tasks"

# Test 6: Daily Reports
log_and_echo "\n${YELLOW}==================== 6. DAILY REPORTS ====================${NC}"

# Get all daily reports
make_request "GET" "/api/v1/daily-reports" "" "Get All Daily Reports"
make_request "GET" "/api/v1/daily-reports?pageSize=5&includeImages=true" "" "Get Daily Reports with Images"

# Create multiple comprehensive daily reports
if [ ! -z "$PROJECT_ID_1" ]; then
    report_data_1='{
        "projectId": "'$PROJECT_ID_1'",
        "reportDate": "2025-06-14",
        "workDescription": "Completed initial site survey and equipment delivery. Installed foundation anchors for 12 solar panel mounts. Conducted electrical panel assessment and identified upgrade requirements.",
        "hoursWorked": 8.5,
        "weatherConditions": "Sunny, 78°F, light winds 5-10 mph",
        "safetyIncidents": null,
        "notes": "Site access confirmed. All safety protocols followed. Equipment staged for tomorrow installation. Client meeting scheduled for progress review.",
        "location": {
            "latitude": 37.7749,
            "longitude": -122.4194,
            "address": "123 Solar Avenue, Sunnydale, CA 90210"
        }
    }'
    
    make_request "POST" "/api/v1/daily-reports" "$report_data_1" "Create Daily Report - Site Survey"
    REPORT_ID_1="$REPORT_ID"
    
    report_data_2='{
        "projectId": "'$PROJECT_ID_1'",
        "reportDate": "2025-06-13",
        "workDescription": "Project kickoff and team briefing. Delivered all solar panels and mounting equipment to site. Performed safety walkthrough and established work zones.",
        "hoursWorked": 6.0,
        "weatherConditions": "Partly cloudy, 72°F, calm winds",
        "safetyIncidents": null,
        "notes": "All team members briefed on safety procedures. Equipment inventory completed - all items accounted for.",
        "location": {
            "latitude": 37.7749,
            "longitude": -122.4194,
            "address": "123 Solar Avenue, Sunnydale, CA 90210"
        }
    }'
    
    make_request "POST" "/api/v1/daily-reports" "$report_data_2" "Create Daily Report - Project Kickoff"
    REPORT_ID_2="$REPORT_ID"
fi

# Create report for second project
if [ ! -z "$PROJECT_ID_2" ]; then
    report_data_3='{
        "projectId": "'$PROJECT_ID_2'",
        "reportDate": "2025-06-14",
        "workDescription": "Commercial building roof structural assessment completed. Measured roof dimensions and calculated load capacity. Identified optimal panel placement locations.",
        "hoursWorked": 7.0,
        "weatherConditions": "Clear skies, 75°F, moderate winds 10-15 mph",
        "safetyIncidents": null,
        "notes": "Roof structure approved for solar installation. Building permits submitted. Engineering drawings in progress.",
        "location": {
            "latitude": 37.7849,
            "longitude": -122.4094,
            "address": "456 Commerce Street, San Francisco, CA 94102"
        }
    }'
    
    make_request "POST" "/api/v1/daily-reports" "$report_data_3" "Create Daily Report - Commercial Assessment"
    REPORT_ID_3="$REPORT_ID"
fi

# Create report with safety incident
if [ ! -z "$PROJECT_ID_3" ]; then
    report_data_4='{
        "projectId": "'$PROJECT_ID_3'",
        "reportDate": "2025-06-14",
        "workDescription": "Emergency repair of damaged solar panel connections due to storm damage. Replaced 3 damaged panels and rewired affected circuits.",
        "hoursWorked": 10.0,
        "weatherConditions": "Overcast, 65°F, strong winds 20-25 mph",
        "safetyIncidents": "Minor equipment malfunction - inverter alarm triggered. Investigated and resolved - loose connection identified and secured.",
        "notes": "Emergency repair completed successfully. All systems restored to full operation. Follow-up inspection scheduled for next week.",
        "location": {
            "latitude": 37.6849,
            "longitude": -122.3094,
            "address": "789 Power Plant Road, Oakland, CA 94601"
        }
    }'
    
    make_request "POST" "/api/v1/daily-reports" "$report_data_4" "Create Daily Report - Emergency Repair"
    REPORT_ID_4="$REPORT_ID"
fi

# Test daily report specific endpoints
if [ ! -z "$REPORT_ID_1" ]; then
    make_request "GET" "/api/v1/daily-reports/$REPORT_ID_1" "" "Get Daily Report by ID"
    
    # Update report
    report_update_data='{
        "workDescription": "Completed initial site survey and equipment delivery. Installed foundation anchors for 12 solar panel mounts. Conducted electrical panel assessment and identified upgrade requirements. UPDATED: Additional grounding work completed.",
        "hoursWorked": 9.0,
        "notes": "Site access confirmed. All safety protocols followed. Equipment staged for tomorrow installation. Client meeting scheduled for progress review. UPDATE: Client approved electrical upgrades."
    }'
    
    make_request "PUT" "/api/v1/daily-reports/$REPORT_ID_1" "$report_update_data" "Update Daily Report"
fi

# Test comprehensive daily report filtering (admin access to all reports)
make_request "GET" "/api/v1/daily-reports?projectId=$PROJECT_ID_1" "" "Get Reports by Project 1"
make_request "GET" "/api/v1/daily-reports?startDate=2025-06-13&endDate=2025-06-14" "" "Get Reports by Date Range"
make_request "GET" "/api/v1/daily-reports?userId=$ADMIN_USER_ID" "" "Get Reports by Admin User"
make_request "GET" "/api/v1/daily-reports?hoursWorked_min=8" "" "Get Reports with 8+ Hours"

# Test 7: Work Requests
log_and_echo "\n${YELLOW}==================== 7. WORK REQUESTS ====================${NC}"

# Get all work requests
make_request "GET" "/api/v1/work-requests" "" "Get All Work Requests"
make_request "GET" "/api/v1/work-requests?status=Pending" "" "Get Pending Work Requests"

# Create multiple comprehensive work requests
if [ ! -z "$PROJECT_ID_1" ]; then
    work_request_data_1='{
        "title": "Additional Panel Installation Request '$TIMESTAMP'",
        "description": "Client requested 8 additional solar panels on garage roof section. Requires structural assessment and electrical capacity evaluation.",
        "requestType": "AdditionalWork",
        "priority": "Medium",
        "projectId": "'$PROJECT_ID_1'",
        "estimatedCost": 12000.00,
        "estimatedHours": 16.0,
        "targetCompletionDate": "2025-07-25"
    }'
    
    make_request "POST" "/api/v1/work-requests" "$work_request_data_1" "Create Additional Work Request"
    WORK_REQUEST_ID_1="$WORK_REQUEST_ID"
    
    work_request_data_2='{
        "title": "Electrical Upgrade Change Order '$TIMESTAMP'",
        "description": "Upgrade main electrical panel from 200A to 400A to accommodate increased solar capacity. Client approved cost increase.",
        "requestType": "ChangeOrder",
        "priority": "High",
        "projectId": "'$PROJECT_ID_1'",
        "estimatedCost": 8500.00,
        "estimatedHours": 12.0,
        "targetCompletionDate": "2025-07-15"
    }'
    
    make_request "POST" "/api/v1/work-requests" "$work_request_data_2" "Create Change Order Request"
    WORK_REQUEST_ID_2="$WORK_REQUEST_ID"
fi

if [ ! -z "$PROJECT_ID_2" ]; then
    work_request_data_3='{
        "title": "Commercial Building Permit Expedite '$TIMESTAMP'",
        "description": "Fast-track building permit approval to meet client deadline. Requires additional engineering documentation and city meetings.",
        "requestType": "Other",
        "priority": "High",
        "projectId": "'$PROJECT_ID_2'",
        "estimatedCost": 3500.00,
        "estimatedHours": 8.0,
        "targetCompletionDate": "2025-06-20"
    }'
    
    make_request "POST" "/api/v1/work-requests" "$work_request_data_3" "Create Permit Expedite Request"
    WORK_REQUEST_ID_3="$WORK_REQUEST_ID"
fi

if [ ! -z "$PROJECT_ID_3" ]; then
    work_request_data_4='{
        "title": "Emergency Storm Damage Repair '$TIMESTAMP'",
        "description": "Immediate repair required for storm-damaged solar panels and mounting systems. Safety critical - requires immediate attention.",
        "requestType": "Emergency",
        "priority": "Critical",
        "projectId": "'$PROJECT_ID_3'",
        "estimatedCost": 15000.00,
        "estimatedHours": 24.0,
        "targetCompletionDate": "2025-06-16"
    }'
    
    make_request "POST" "/api/v1/work-requests" "$work_request_data_4" "Create Emergency Repair Request"
    WORK_REQUEST_ID_4="$WORK_REQUEST_ID"
fi

# Test work request specific endpoints
if [ ! -z "$WORK_REQUEST_ID_1" ]; then
    make_request "GET" "/api/v1/work-requests/$WORK_REQUEST_ID_1" "" "Get Work Request by ID"
    
    # Update work request status (admin approval)
    work_request_update_data='{
        "title": "Additional Panel Installation Request '$TIMESTAMP' - Approved",
        "status": "Approved",
        "estimatedCost": 11500.00,
        "approvalNotes": "Approved by admin with minor cost adjustment. Structural assessment completed successfully."
    }'
    
    make_request "PUT" "/api/v1/work-requests/$WORK_REQUEST_ID_1" "$work_request_update_data" "Approve Work Request (Admin)"
fi

# Approve emergency request
if [ ! -z "$WORK_REQUEST_ID_4" ]; then
    emergency_approval_data='{
        "status": "Approved",
        "priority": "Critical",
        "approvalNotes": "Emergency repair approved immediately. Safety team dispatched."
    }'
    
    make_request "PUT" "/api/v1/work-requests/$WORK_REQUEST_ID_4" "$emergency_approval_data" "Approve Emergency Request"
fi

# Test comprehensive work request filtering (admin access to all requests)
make_request "GET" "/api/v1/work-requests?projectId=$PROJECT_ID_1" "" "Get Work Requests by Project"
make_request "GET" "/api/v1/work-requests?requestType=Emergency" "" "Get Emergency Work Requests"
make_request "GET" "/api/v1/work-requests?priority=High" "" "Get High Priority Work Requests"
make_request "GET" "/api/v1/work-requests?status=Approved" "" "Get Approved Work Requests"
make_request "GET" "/api/v1/work-requests?estimatedCost_min=10000" "" "Get High-Cost Work Requests"

# Test 8: Calendar Events
log_and_echo "\n${YELLOW}==================== 8. CALENDAR EVENTS ====================${NC}"

# Add small delay to avoid rate limiting
sleep 2

# Get all calendar events
make_request "GET" "/api/v1/calendar" "" "Get All Calendar Events"
make_request "GET" "/api/v1/calendar?pageSize=10&eventType=Meeting" "" "Get Meeting Events"

# Create comprehensive calendar events for projects
if [ ! -z "$PROJECT_ID_1" ]; then
    calendar_event_data_1='{
        "title": "Project Kickoff Meeting '$TIMESTAMP'",
        "description": "Initial project planning and team coordination meeting for solar installation project",
        "startDateTime": "2025-06-20T09:00:00Z",
        "endDateTime": "2025-06-20T10:30:00Z",
        "eventType": "Meeting",
        "status": "Scheduled",
        "priority": "High",
        "location": "Main Conference Room",
        "projectId": "'$PROJECT_ID_1'",
        "isAllDay": false,
        "notes": "Review project timeline, safety procedures, and team assignments",
        "reminderMinutes": 30,
        "attendees": "project_manager@company.com, lead_tech@company.com, safety_officer@company.com"
    }'
    
    make_request "POST" "/api/v1/calendar" "$calendar_event_data_1" "Create Project Kickoff Meeting"
    CALENDAR_EVENT_ID_1="$CALENDAR_EVENT_ID"
    
    sleep 1
    
    calendar_event_data_2='{
        "title": "Solar Panel Installation Deadline '$TIMESTAMP'",
        "description": "Deadline for completion of solar panel installation phase",
        "startDateTime": "2025-07-10T17:00:00Z",
        "endDateTime": "2025-07-10T17:00:00Z",
        "eventType": "Deadline",
        "status": "Scheduled",
        "priority": "Critical",
        "location": "123 Solar Avenue, Sunnydale, CA",
        "projectId": "'$PROJECT_ID_1'",
        "taskId": "'$TASK_ID_2'",
        "isAllDay": false,
        "notes": "All 24 solar panels must be installed and secured by this date",
        "reminderMinutes": 1440
    }'
    
    make_request "POST" "/api/v1/calendar" "$calendar_event_data_2" "Create Installation Deadline"
    CALENDAR_EVENT_ID_2="$CALENDAR_EVENT_ID"
    
    sleep 1
    
    calendar_event_data_3='{
        "title": "Safety Training Session '$TIMESTAMP'",
        "description": "Mandatory safety training for all team members working on solar installations",
        "startDateTime": "2025-06-18T14:00:00Z",
        "endDateTime": "2025-06-18T16:00:00Z",
        "eventType": "Training",
        "status": "Scheduled",
        "priority": "High",
        "location": "Training Center Room B",
        "projectId": "'$PROJECT_ID_1'",
        "isAllDay": false,
        "notes": "Cover roof safety, electrical safety, and emergency procedures",
        "reminderMinutes": 60,
        "attendees": "all_team@company.com"
    }'
    
    make_request "POST" "/api/v1/calendar" "$calendar_event_data_3" "Create Safety Training Event"
    CALENDAR_EVENT_ID_3="$CALENDAR_EVENT_ID"
fi

sleep 1

# Create maintenance event
if [ ! -z "$PROJECT_ID_3" ]; then
    calendar_event_data_4='{
        "title": "Emergency Maintenance Work '$TIMESTAMP'",
        "description": "Emergency repair and maintenance of storm-damaged solar installation",
        "startDateTime": "2025-06-15T08:00:00Z",
        "endDateTime": "2025-06-15T18:00:00Z",
        "eventType": "Maintenance",
        "status": "InProgress",
        "priority": "Critical",
        "location": "789 Power Plant Road, Oakland, CA",
        "projectId": "'$PROJECT_ID_3'",
        "isAllDay": false,
        "notes": "Emergency response to storm damage. All safety protocols in effect.",
        "reminderMinutes": 15
    }'
    
    make_request "POST" "/api/v1/calendar" "$calendar_event_data_4" "Create Emergency Maintenance Event"
    CALENDAR_EVENT_ID_4="$CALENDAR_EVENT_ID"
fi

sleep 1

# Test calendar event specific endpoints
if [ ! -z "$CALENDAR_EVENT_ID_1" ]; then
    make_request "GET" "/api/v1/calendar/$CALENDAR_EVENT_ID_1" "" "Get Calendar Event by ID"
    
    # Update calendar event
    calendar_update_data='{
        "title": "Project Kickoff Meeting '$TIMESTAMP' - Confirmed",
        "status": "Confirmed",
        "notes": "Review project timeline, safety procedures, and team assignments. All attendees confirmed.",
        "attendees": "project_manager@company.com, lead_tech@company.com, safety_officer@company.com, client_rep@example.com"
    }'
    
    make_request "PUT" "/api/v1/calendar/$CALENDAR_EVENT_ID_1" "$calendar_update_data" "Update Calendar Event"
fi

sleep 1

# Test comprehensive calendar filtering (admin access to all events)
make_request "GET" "/api/v1/calendar/upcoming?days=30" "" "Get Upcoming Events"
make_request "GET" "/api/v1/calendar?startDate=2025-06-15&endDate=2025-07-15" "" "Get Events in Date Range"
make_request "GET" "/api/v1/calendar?eventType=Meeting&priority=High" "" "Get High Priority Meetings"
make_request "GET" "/api/v1/calendar?status=Scheduled" "" "Get Scheduled Events"

if [ ! -z "$PROJECT_ID_1" ]; then
    make_request "GET" "/api/v1/calendar/project/$PROJECT_ID_1" "" "Get Events by Project"
fi

if [ ! -z "$TASK_ID_2" ]; then
    make_request "GET" "/api/v1/calendar/task/$TASK_ID_2" "" "Get Events by Task"
fi

# Test 9: Image Upload
log_and_echo "\n${YELLOW}==================== 9. IMAGE UPLOAD ====================${NC}"

# Create a dummy image file for testing
test_image_path="/tmp/test_image_$TIMESTAMP.jpg"
echo "Dummy image content for API testing" > "$test_image_path"

# Test image upload
make_request "POST" "/api/v1/images/upload" "-F \"file=@$test_image_path\" -F \"category=test\" -F \"description=Admin API test image\"" "Upload Test Image" "multipart/form-data"

# If image was uploaded, test image endpoints
if [ ! -z "$IMAGE_ID" ]; then
    make_request "GET" "/api/v1/images/$IMAGE_ID" "" "Get Image Metadata"
fi

# Clean up test image
rm -f "$test_image_path"

# Test 10: Advanced Filtering and Pagination
log_and_echo "\n${YELLOW}==================== 10. ADVANCED FILTERING & PAGINATION ====================${NC}"

# Test pagination with different page sizes
make_request "GET" "/api/v1/projects?pageNumber=1&pageSize=5" "" "Projects - Page 1, Size 5"
make_request "GET" "/api/v1/daily-reports?pageNumber=1&pageSize=3" "" "Daily Reports - Page 1, Size 3"

# Test filtering by date ranges
start_date="2025-06-01"
end_date="2025-06-30"
make_request "GET" "/api/v1/daily-reports?startDate=$start_date&endDate=$end_date" "" "Daily Reports - Date Range Filter"

# Test search functionality
make_request "GET" "/api/v1/projects?search=test" "" "Projects - Search Filter"

# Test 11: Conflict Detection and Validation
log_and_echo "\n${YELLOW}==================== 11. CONFLICT DETECTION & VALIDATION ====================${NC}"

# Test calendar conflict detection
conflict_check_data='{
    "startDateTime": "2025-06-20T10:30:00Z",
    "endDateTime": "2025-06-20T11:30:00Z",
    "userId": "'$ADMIN_USER_ID'"
}'

make_request "POST" "/api/v1/calendar/conflicts" "$conflict_check_data" "Check Calendar Conflicts"

# Test 12: Cleanup Operations (DELETE endpoints)
log_and_echo "\n${YELLOW}==================== 12. CLEANUP OPERATIONS ====================${NC}"

# Delete created entities (Admin has delete permissions)
if [ ! -z "$CALENDAR_EVENT_ID" ]; then
    make_request "DELETE" "/api/v1/calendar/$CALENDAR_EVENT_ID" "" "Delete Calendar Event"
fi

if [ ! -z "$WORK_REQUEST_ID" ]; then
    make_request "DELETE" "/api/v1/work-requests/$WORK_REQUEST_ID" "" "Delete Work Request"
fi

if [ ! -z "$REPORT_ID" ]; then
    make_request "DELETE" "/api/v1/daily-reports/$REPORT_ID" "" "Delete Daily Report"
fi

if [ ! -z "$TASK_ID" ]; then
    make_request "DELETE" "/api/v1/tasks/$TASK_ID" "" "Delete Task"
fi

if [ ! -z "$PROJECT_ID" ]; then
    make_request "DELETE" "/api/v1/projects/$PROJECT_ID" "" "Delete Project"
fi

if [ ! -z "$IMAGE_ID" ]; then
    make_request "DELETE" "/api/v1/images/$IMAGE_ID" "" "Delete Image"
fi

# Test Summary
log_and_echo "\n${YELLOW}==================== 📊 TEST SUMMARY ====================${NC}"
log_and_echo "${GREEN}✅ Admin API endpoint testing completed!${NC}"
log_and_echo ""
log_and_echo "📁 Detailed results saved to: $LOG_FILE"
log_and_echo ""
log_and_echo "${BLUE}📋 Test Coverage:${NC}"
log_and_echo "   • Health endpoints (public)"
log_and_echo "   • Authentication & token refresh"
log_and_echo "   • User management (admin-only)"
log_and_echo "   • Project CRUD operations"
log_and_echo "   • Task CRUD operations"
log_and_echo "   • Daily Reports CRUD operations"
log_and_echo "   • Work Requests CRUD operations"
log_and_echo "   • Calendar Events CRUD operations"
log_and_echo "   • Image upload and management"
log_and_echo "   • Advanced filtering & pagination"
log_and_echo "   • Conflict detection"
log_and_echo "   • Cleanup operations (DELETE)"
log_and_echo ""
log_and_echo "${BLUE}🔐 Admin-Specific Features Tested:${NC}"
log_and_echo "   • Full CRUD access to all entities"
log_and_echo "   • User creation and management"
log_and_echo "   • System administration endpoints"
log_and_echo "   • Delete operations on all resources"
log_and_echo ""
log_and_echo "${YELLOW}📖 Review the log file for detailed API responses and troubleshooting.${NC}"

# Display final statistics
total_requests=$(grep -c "📡 Testing:" "$LOG_FILE")
successful_requests=$(grep -c "✅ SUCCESS" "$LOG_FILE")
error_requests=$(grep -c "❌ ERROR" "$LOG_FILE")
warning_requests=$(grep -c "⚠️" "$LOG_FILE")

log_and_echo ""
log_and_echo "${BLUE}📈 Statistics:${NC}"
log_and_echo "   Total API calls: $total_requests"
log_and_echo "   Successful: ${GREEN}$successful_requests${NC}"
log_and_echo "   Warnings: ${YELLOW}$warning_requests${NC}"
log_and_echo "   Errors: ${RED}$error_requests${NC}"

echo ""
echo "🎯 Admin endpoint testing completed successfully!"
echo "Check $LOG_FILE for detailed results."
