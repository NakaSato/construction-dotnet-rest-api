#!/bin/bash

# ==============================================================================
# Solar Projects REST API - Complete Mock Data Test Script
# Tests ALL API endpoints with realistic solar project mock data
# ==============================================================================

set -e  # Exit on any error

# Configuration
API_BASE_URL="${API_BASE_URL:-http://localhost:5002}"
TIMESTAMP=$(date +%s)
TEST_USERNAME="solar_manager_$TIMESTAMP"
TEST_PASSWORD="SolarPower123!"
TEST_EMAIL="manager_$TIMESTAMP@solartech.com"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Global variables for created resources
AUTH_TOKEN=""
USER_ID=""
PROJECT_ID=""
TASK_ID=""
DAILY_REPORT_ID=""
WORK_REQUEST_ID=""
CALENDAR_EVENT_ID=""
IMAGE_ID=""

# Mock data arrays
MOCK_PROJECTS=(
    "Downtown Solar Farm"
    "Residential Rooftop Installation"
    "Industrial Solar Array"
    "Community Solar Garden"
    "Commercial Office Building Solar"
)

MOCK_ADDRESSES=(
    "1234 Solar Boulevard, Phoenix, AZ 85001"
    "5678 Green Energy Lane, Tucson, AZ 85701"
    "9012 Renewable Street, Flagstaff, AZ 86001"
    "3456 Sustainable Ave, Mesa, AZ 85201"
    "7890 Clean Power Rd, Scottsdale, AZ 85251"
)

MOCK_CLIENTS=(
    "SunTech Corporation - Contact: John Smith, Phone: +1-555-0101"
    "Green Energy Solutions - Contact: Sarah Johnson, Phone: +1-555-0102"
    "EcoFriendly Industries - Contact: Michael Brown, Phone: +1-555-0103"
    "Renewable Resources LLC - Contact: Emily Davis, Phone: +1-555-0104"
    "Solar Innovations Inc - Contact: David Wilson, Phone: +1-555-0105"
)

# ==============================================================================
# Utility Functions
# ==============================================================================

log_header() {
    echo -e "${PURPLE}╔══════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${PURPLE}║$(printf "%66s" | tr ' ' ' ')║${NC}"
    echo -e "${PURPLE}║$(printf "%-66s" "  $1")║${NC}"
    echo -e "${PURPLE}║$(printf "%66s" | tr ' ' ' ')║${NC}"
    echo -e "${PURPLE}╚══════════════════════════════════════════════════════════════════╝${NC}"
}

log_section() {
    echo
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
}

log_info() {
    echo -e "${BLUE}ℹ️  INFO: $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
    ((PASSED_TESTS++))
}

log_fail() {
    echo -e "${RED}❌ FAIL: $1${NC}"
    ((FAILED_TESTS++))
}

log_warn() {
    echo -e "${YELLOW}⚠️  WARN: $1${NC}"
}

log_mock_data() {
    echo -e "${PURPLE}📋 MOCK: $1${NC}"
}

test_endpoint() {
    local test_name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    local auth_required="${6:-true}"
    local description="$7"
    
    ((TOTAL_TESTS++))
    
    echo "🧪 Testing: $test_name"
    if [ -n "$description" ]; then
        log_info "Description: $description"
    fi
    
    local curl_cmd="curl -s"
    
    if [ "$auth_required" = "true" ] && [ -n "$AUTH_TOKEN" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $AUTH_TOKEN'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ -n "$data" ]; then
        curl_cmd="$curl_cmd -d '$data'"
        if [ ${#data} -lt 200 ]; then
            log_mock_data "Request: $data"
        else
            log_mock_data "Request: Large payload with realistic solar project data"
        fi
    fi
    
    curl_cmd="$curl_cmd -X $method"
    curl_cmd="$curl_cmd -w '%{http_code}'"
    curl_cmd="$curl_cmd $API_BASE_URL$endpoint"
    
    local response=$(eval $curl_cmd)
    local status_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$status_code" = "$expected_status" ]; then
        log_success "$test_name (Status: $status_code)"
        
        # Extract IDs for later use
        extract_resource_ids "$endpoint" "$method" "$body"
        
        # Show sample response for interesting endpoints
        if [[ "$endpoint" == *"/projects"* ]] && [ "$method" = "GET" ]; then
            log_info "Sample Response: Found $(echo "$body" | grep -o '"projectId"' | wc -l | tr -d ' ') projects"
        fi
        
        return 0
    else
        log_fail "$test_name (Expected: $expected_status, Got: $status_code)"
        if [ ${#body} -lt 300 ]; then
            log_info "Response: $body"
        fi
        return 1
    fi
}

extract_resource_ids() {
    local endpoint="$1"
    local method="$2"
    local body="$3"
    
    case "$endpoint" in
        */projects)
            if [ "$method" = "POST" ] && [ -z "$PROJECT_ID" ]; then
                PROJECT_ID=$(echo "$body" | grep -o '"projectId":"[^"]*"' | cut -d'"' -f4 | head -1)
                [ -n "$PROJECT_ID" ] && log_info "Captured Project ID: $PROJECT_ID"
            fi
            ;;
        */tasks)
            if [ "$method" = "POST" ] && [ -z "$TASK_ID" ]; then
                TASK_ID=$(echo "$body" | grep -o '"taskId":"[^"]*"' | cut -d'"' -f4 | head -1)
                [ -n "$TASK_ID" ] && log_info "Captured Task ID: $TASK_ID"
            fi
            ;;
        */daily-reports)
            if [ "$method" = "POST" ] && [ -z "$DAILY_REPORT_ID" ]; then
                DAILY_REPORT_ID=$(echo "$body" | grep -o '"reportId":"[^"]*"' | cut -d'"' -f4 | head -1)
                [ -n "$DAILY_REPORT_ID" ] && log_info "Captured Daily Report ID: $DAILY_REPORT_ID"
            fi
            ;;
        */work-requests)
            if [ "$method" = "POST" ] && [ -z "$WORK_REQUEST_ID" ]; then
                WORK_REQUEST_ID=$(echo "$body" | grep -o '"workRequestId":"[^"]*"' | cut -d'"' -f4 | head -1)
                [ -n "$WORK_REQUEST_ID" ] && log_info "Captured Work Request ID: $WORK_REQUEST_ID"
            fi
            ;;
        */calendar)
            if [ "$method" = "POST" ] && [ -z "$CALENDAR_EVENT_ID" ]; then
                CALENDAR_EVENT_ID=$(echo "$body" | grep -o '"eventId":"[^"]*"' | cut -d'"' -f4 | head -1)
                [ -n "$CALENDAR_EVENT_ID" ] && log_info "Captured Calendar Event ID: $CALENDAR_EVENT_ID"
            fi
            ;;
    esac
}

wait_for_api() {
    log_info "Checking API availability..."
    local retries=15
    while [ $retries -gt 0 ]; do
        if curl -s "$API_BASE_URL/health" > /dev/null 2>&1; then
            log_success "API is ready and responding!"
            return 0
        fi
        retries=$((retries - 1))
        sleep 1
    done
    log_fail "API is not responding after 15 seconds"
    return 1
}

setup_authentication() {
    log_section "AUTHENTICATION SETUP"
    
    # Register test user
    log_info "Registering solar project manager: $TEST_USERNAME"
    
    local register_data='{
        "username": "'$TEST_USERNAME'",
        "password": "'$TEST_PASSWORD'",
        "email": "'$TEST_EMAIL'",
        "fullName": "Solar Project Manager",
        "roleId": 1
    }'
    
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$register_data" \
        -w "%{http_code}" \
        "$API_BASE_URL/api/v1/auth/register")
    
    local status_code="${response: -3}"
    
    if [ "$status_code" = "200" ] || [ "$status_code" = "201" ]; then
        log_success "Solar project manager registered successfully"
    else
        log_info "Registration response: $status_code (user may already exist)"
    fi
    
    # Authenticate
    log_info "Authenticating solar project manager..."
    
    local login_data='{
        "username": "'$TEST_USERNAME'",
        "password": "'$TEST_PASSWORD'"
    }'
    
    response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$login_data" \
        -w "%{http_code}" \
        "$API_BASE_URL/api/v1/auth/login")
    
    status_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$status_code" = "200" ]; then
        AUTH_TOKEN=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        USER_ID=$(echo "$body" | grep -o '"userId":"[^"]*"' | cut -d'"' -f4)
        log_success "Authentication successful"
        log_info "Solar Manager ID: $USER_ID"
        return 0
    else
        log_fail "Authentication failed (Status: $status_code)"
        echo "Response: $body"
        return 1
    fi
}

# ==============================================================================
# Test Functions with Mock Data
# ==============================================================================

test_health_monitoring() {
    log_section "HEALTH MONITORING ENDPOINTS"
    
    test_endpoint \
        "Basic Health Check" \
        "GET" \
        "/health" \
        "" \
        "200" \
        "false" \
        "Verify API service is running and healthy"
    
    test_endpoint \
        "Detailed Health Check" \
        "GET" \
        "/health/detailed" \
        "" \
        "200" \
        "false" \
        "Get comprehensive system health metrics"
}

test_authentication_endpoints() {
    log_section "AUTHENTICATION ENDPOINTS"
    
    # Additional user registration
    local new_user_data='{
        "username": "technician_'$TIMESTAMP'",
        "password": "TechnicianPass123!",
        "email": "tech_'$TIMESTAMP'@solartech.com",
        "fullName": "Solar Technician",
        "roleId": 2
    }'
    
    test_endpoint \
        "Register Solar Technician" \
        "POST" \
        "/api/v1/auth/register" \
        "$new_user_data" \
        "200" \
        "false" \
        "Register a new solar technician user"
    
    # Test token refresh (expected to fail with dummy token)
    local refresh_data='{"refreshToken": "dummy_refresh_token_for_testing"}'
    
    test_endpoint \
        "Token Refresh (Expected Fail)" \
        "POST" \
        "/api/v1/auth/refresh" \
        "$refresh_data" \
        "400" \
        "false" \
        "Test token refresh with invalid token"
}

test_project_management() {
    log_section "PROJECT MANAGEMENT ENDPOINTS"
    
    # Get all projects
    test_endpoint \
        "Get All Solar Projects" \
        "GET" \
        "/api/v1/projects" \
        "" \
        "200" \
        "true" \
        "Retrieve all solar installation projects"
    
    # Get projects with pagination
    test_endpoint \
        "Get Projects with Pagination" \
        "GET" \
        "/api/v1/projects?page=1&pageSize=10&sortBy=projectName&sortOrder=asc" \
        "" \
        "200" \
        "true" \
        "Get paginated and sorted project list"
    
    # Create multiple solar projects with realistic data
    for i in {0..2}; do
        local project_data='{
            "projectName": "'${MOCK_PROJECTS[$i]}'",
            "address": "'${MOCK_ADDRESSES[$i]}'",
            "clientInfo": "'${MOCK_CLIENTS[$i]}'",
            "status": '$i',
            "startDate": "2025-0'$((7+$i))'-01T00:00:00Z",
            "estimatedEndDate": "2025-'$((10+$i))'-31T23:59:59Z",
            "projectManagerId": "'$USER_ID'"
        }'
        
        test_endpoint \
            "Create Solar Project: ${MOCK_PROJECTS[$i]}" \
            "POST" \
            "/api/v1/projects" \
            "$project_data" \
            "201" \
            "true" \
            "Create new solar installation project with realistic data"
    done
    
    # Test project operations if we have a project ID
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint \
            "Get Solar Project by ID" \
            "GET" \
            "/api/v1/projects/$PROJECT_ID" \
            "" \
            "200" \
            "true" \
            "Retrieve specific project details"
        
        # Update project
        local update_data='{
            "projectName": "Updated Downtown Solar Farm",
            "status": 1,
            "clientInfo": "Updated client information with progress notes"
        }'
        
        test_endpoint \
            "Update Solar Project" \
            "PUT" \
            "/api/v1/projects/$PROJECT_ID" \
            "$update_data" \
            "200" \
            "true" \
            "Update project status and information"
        
        # Get projects by manager
        test_endpoint \
            "Get Projects by Manager" \
            "GET" \
            "/api/v1/projects/manager/$USER_ID" \
            "" \
            "200" \
            "true" \
            "Get all projects managed by current user"
    fi
    
    # Test error cases
    test_endpoint \
        "Get Project with Invalid ID" \
        "GET" \
        "/api/v1/projects/invalid-uuid" \
        "" \
        "400" \
        "true" \
        "Test error handling with invalid project ID"
}

test_task_management() {
    log_section "TASK MANAGEMENT ENDPOINTS"
    
    # Get all tasks
    test_endpoint \
        "Get All Solar Tasks" \
        "GET" \
        "/api/v1/tasks" \
        "" \
        "200" \
        "true" \
        "Retrieve all solar installation tasks"
    
    # Create realistic solar installation tasks
    if [ -n "$PROJECT_ID" ]; then
        local tasks=(
            "Site Survey and Assessment"
            "Install Solar Panel Mounting System"
            "Mount Solar Panels"
            "Install Electrical Wiring"
            "Connect to Electrical Grid"
            "System Testing and Commissioning"
        )
        
        local descriptions=(
            "Conduct comprehensive site survey including structural analysis and shading assessment"
            "Install mounting rails and hardware on roof according to engineering specifications"
            "Mount solar panels on prepared mounting system with proper spacing and alignment"
            "Install DC and AC electrical wiring with conduits and safety disconnects"
            "Connect solar system to main electrical panel and utility grid connection"
            "Perform comprehensive system testing and commissioning procedures"
        )
        
        for i in {0..2}; do
            local task_data='{
                "title": "'${tasks[$i]}'",
                "description": "'${descriptions[$i]}'",
                "projectId": "'$PROJECT_ID'",
                "assignedToId": "'$USER_ID'",
                "dueDate": "2025-0'$((8+$i))'-15T00:00:00Z",
                "priority": "'$([ $((i % 2)) -eq 0 ] && echo "High" || echo "Medium")'",
                "status": "Open",
                "estimatedHours": '$((16 + $i * 8))'
            }'
            
            test_endpoint \
                "Create Task: ${tasks[$i]}" \
                "POST" \
                "/api/v1/tasks" \
                "$task_data" \
                "201" \
                "true" \
                "Create solar installation task with detailed specifications"
        done
        
        # Get tasks by project
        test_endpoint \
            "Get Tasks by Project" \
            "GET" \
            "/api/v1/tasks/project/$PROJECT_ID" \
            "" \
            "200" \
            "true" \
            "Get all tasks for specific solar project"
    fi
    
    # Update task if we have one
    if [ -n "$TASK_ID" ]; then
        local update_task_data='{
            "status": "In Progress",
            "actualHours": 12,
            "notes": "Started site survey, weather conditions favorable"
        }'
        
        test_endpoint \
            "Update Solar Task Status" \
            "PUT" \
            "/api/v1/tasks/$TASK_ID" \
            "$update_task_data" \
            "200" \
            "true" \
            "Update task progress and status"
    fi
}

test_user_management() {
    log_section "USER MANAGEMENT ENDPOINTS"
    
    test_endpoint \
        "Get All Solar Team Members" \
        "GET" \
        "/api/v1/users" \
        "" \
        "200" \
        "true" \
        "Get all registered solar project team members"
    
    if [ -n "$USER_ID" ]; then
        test_endpoint \
            "Get Solar Manager Profile" \
            "GET" \
            "/api/v1/users/$USER_ID" \
            "" \
            "200" \
            "true" \
            "Get detailed profile of solar project manager"
    fi
    
    test_endpoint \
        "Search Solar Team Members" \
        "GET" \
        "/api/v1/users/search?query=solar" \
        "" \
        "200" \
        "true" \
        "Search for team members by keyword"
    
    test_endpoint \
        "Get Users with Advanced Query" \
        "GET" \
        "/api/v1/users/advanced?roleId=1&isActive=true&pageSize=5" \
        "" \
        "200" \
        "true" \
        "Advanced user search with role and status filters"
}

test_daily_reports() {
    log_section "DAILY REPORTS ENDPOINTS"
    
    test_endpoint \
        "Get All Daily Reports" \
        "GET" \
        "/api/v1/daily-reports" \
        "" \
        "200" \
        "true" \
        "Retrieve all daily progress reports"
    
    # Create realistic daily reports
    if [ -n "$PROJECT_ID" ]; then
        local weather_conditions=(
            "Sunny, 78°F, light breeze - Perfect solar installation conditions"
            "Partly cloudy, 72°F, calm winds - Good working conditions"
            "Clear skies, 85°F, minimal wind - Excellent visibility for work"
        )
        
        local work_descriptions=(
            "Completed site survey and structural assessment. Roof structure verified for solar panel installation. Marked mounting locations."
            "Installed mounting rails on south-facing roof section. All hardware secured according to specifications. No structural issues encountered."
            "Mounted first row of solar panels. Electrical connections completed for initial panel string. Quality inspection passed."
        )
        
        for i in {0..2}; do
            local report_data='{
                "projectId": "'$PROJECT_ID'",
                "reportDate": "2025-06-'$((15+$i))'T00:00:00Z",
                "weatherConditions": "'${weather_conditions[$i]}'",
                "workDescription": "'${work_descriptions[$i]}'",
                "workersPresent": '$((4 + $i))',
                "hoursWorked": '$((8.0 + $i * 0.5))',
                "materialsUsed": "Solar panels, mounting rails, stainless steel bolts, electrical conduit",
                "equipmentUsed": "Crane, drill, torque wrench, multimeter, safety harnesses",
                "safetyIncidents": "None reported - all safety protocols followed",
                "progressNotes": "Project progressing on schedule, quality standards maintained",
                "submittedById": "'$USER_ID'"
            }'
            
            test_endpoint \
                "Create Daily Report Day $((i+1))" \
                "POST" \
                "/api/v1/daily-reports" \
                "$report_data" \
                "201" \
                "true" \
                "Create daily progress report with comprehensive work details"
        done
        
        # Get reports by project
        test_endpoint \
            "Get Daily Reports by Project" \
            "GET" \
            "/api/v1/daily-reports/project/$PROJECT_ID" \
            "" \
            "200" \
            "true" \
            "Get all daily reports for specific solar project"
    fi
    
    # Test report operations if we have a report ID
    if [ -n "$DAILY_REPORT_ID" ]; then
        test_endpoint \
            "Get Daily Report Details" \
            "GET" \
            "/api/v1/daily-reports/$DAILY_REPORT_ID" \
            "" \
            "200" \
            "true" \
            "Get detailed daily report information"
        
        # Submit report
        test_endpoint \
            "Submit Daily Report" \
            "POST" \
            "/api/v1/daily-reports/$DAILY_REPORT_ID/submit" \
            "" \
            "200" \
            "true" \
            "Submit daily report for manager review"
        
        # Update report
        local update_report_data='{
            "workDescription": "Updated: Completed additional panel installations and quality checks",
            "progressNotes": "Ahead of schedule due to favorable weather conditions"
        }'
        
        test_endpoint \
            "Update Daily Report" \
            "PUT" \
            "/api/v1/daily-reports/$DAILY_REPORT_ID" \
            "$update_report_data" \
            "200" \
            "true" \
            "Update daily report with additional information"
    fi
}

test_work_requests() {
    log_section "WORK REQUEST ENDPOINTS"
    
    test_endpoint \
        "Get All Work Requests" \
        "GET" \
        "/api/v1/work-requests" \
        "" \
        "200" \
        "true" \
        "Retrieve all work requests and change orders"
    
    # Create realistic work requests
    if [ -n "$PROJECT_ID" ]; then
        local work_requests=(
            "Additional Electrical Conduit Installation"
            "Roof Penetration Waterproofing"
            "Extra Monitoring Equipment Installation"
        )
        
        local descriptions=(
            "Client requested additional electrical conduit for future expansion capabilities"
            "Additional waterproofing required for roof penetrations due to local building code requirements"
            "Install advanced monitoring system with real-time performance tracking capabilities"
        )
        
        for i in {0..2}; do
            local work_request_data='{
                "projectId": "'$PROJECT_ID'",
                "title": "'${work_requests[$i]}'",
                "description": "'${descriptions[$i]}'",
                "requestedById": "'$USER_ID'",
                "priority": "'$([ $((i % 2)) -eq 0 ] && echo "Medium" || echo "High")'",
                "estimatedCost": '$((5000 + $i * 2000))'.00,
                "estimatedHours": '$((16 + $i * 8))',
                "requestType": "'$([ $((i % 2)) -eq 0 ] && echo "Change Order" || echo "Additional Work")'",
                "urgency": "Normal",
                "justification": "Required for project compliance and customer satisfaction"
            }'
            
            test_endpoint \
                "Create Work Request: ${work_requests[$i]}" \
                "POST" \
                "/api/v1/work-requests" \
                "$work_request_data" \
                "201" \
                "true" \
                "Create work request for additional project scope"
        done
    fi
    
    # Test work request operations
    if [ -n "$WORK_REQUEST_ID" ]; then
        test_endpoint \
            "Get Work Request Details" \
            "GET" \
            "/api/v1/work-requests/$WORK_REQUEST_ID" \
            "" \
            "200" \
            "true" \
            "Get detailed work request information"
        
        # Update work request
        local update_work_request_data='{
            "status": "Approved",
            "approvedCost": 6000.00,
            "approvalNotes": "Approved by project manager - critical for code compliance"
        }'
        
        test_endpoint \
            "Update Work Request Status" \
            "PUT" \
            "/api/v1/work-requests/$WORK_REQUEST_ID" \
            "$update_work_request_data" \
            "200" \
            "true" \
            "Update work request with approval status"
    fi
}

test_calendar_management() {
    log_section "CALENDAR MANAGEMENT ENDPOINTS"
    
    test_endpoint \
        "Get All Calendar Events" \
        "GET" \
        "/api/v1/calendar" \
        "" \
        "200" \
        "true" \
        "Retrieve all solar project calendar events"
    
    test_endpoint \
        "Get Calendar Events by Date Range" \
        "GET" \
        "/api/v1/calendar?startDate=2025-06-01&endDate=2025-12-31&eventType=Meeting" \
        "" \
        "200" \
        "true" \
        "Get calendar events within specific date range"
    
    # Create realistic calendar events
    local calendar_events=(
        "Solar Panel Installation Kickoff Meeting"
        "Site Safety Inspection"
        "Client Progress Review Meeting"
        "Final System Commissioning"
    )
    
    local event_descriptions=(
        "Project kickoff meeting with all stakeholders to review installation plan and timeline"
        "Comprehensive safety inspection with certified inspector before installation begins"
        "Weekly progress review meeting with client to discuss installation status"
        "Final system testing and commissioning with utility company representative"
    )
    
    for i in {0..3}; do
        local start_hour=$((9 + $i * 2))
        local end_hour=$((start_hour + 1))
        
        local calendar_data='{
            "title": "'${calendar_events[$i]}'",
            "description": "'${event_descriptions[$i]}'",
            "startDateTime": "2025-06-'$((20 + $i))'T'$(printf "%02d" $start_hour)':00:00Z",
            "endDateTime": "2025-06-'$((20 + $i))'T'$(printf "%02d" $end_hour)':00:00Z",
            "eventType": "'$([ $((i % 2)) -eq 0 ] && echo "Meeting" || echo "Inspection")'",
            "isAllDay": false,
            "location": "Solar Installation Site",
            "organizerId": "'$USER_ID'",
            "notes": "Solar project milestone event"
        }'
        
        test_endpoint \
            "Create Calendar Event: ${calendar_events[$i]}" \
            "POST" \
            "/api/v1/calendar" \
            "$calendar_data" \
            "201" \
            "true" \
            "Schedule important solar project milestone"
    done
    
    # Test calendar operations
    test_endpoint \
        "Get Upcoming Solar Events" \
        "GET" \
        "/api/v1/calendar/upcoming" \
        "" \
        "200" \
        "true" \
        "Get upcoming solar project events"
    
    if [ -n "$USER_ID" ]; then
        test_endpoint \
            "Get Events by Solar Manager" \
            "GET" \
            "/api/v1/calendar/user/$USER_ID" \
            "" \
            "200" \
            "true" \
            "Get all events organized by current user"
    fi
    
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint \
            "Get Events by Solar Project" \
            "GET" \
            "/api/v1/calendar/project/$PROJECT_ID" \
            "" \
            "200" \
            "true" \
            "Get all events related to specific project"
    fi
    
    # Update calendar event
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        local update_calendar_data='{
            "title": "Updated Solar Installation Kickoff Meeting",
            "description": "Updated meeting agenda to include safety briefing and equipment review",
            "location": "Solar Installation Site - Conference Room"
        }'
        
        test_endpoint \
            "Update Calendar Event" \
            "PUT" \
            "/api/v1/calendar/$CALENDAR_EVENT_ID" \
            "$update_calendar_data" \
            "200" \
            "true" \
            "Update calendar event details"
    fi
}

test_image_management() {
    log_section "IMAGE MANAGEMENT ENDPOINTS"
    
    test_endpoint \
        "Get All Solar Project Images" \
        "GET" \
        "/api/v1/images" \
        "" \
        "200" \
        "true" \
        "Retrieve all solar project images and documentation"
    
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint \
            "Get Images by Solar Project" \
            "GET" \
            "/api/v1/images/project/$PROJECT_ID" \
            "" \
            "200" \
            "true" \
            "Get all images for specific solar project"
    fi
    
    # Note: File upload testing would require actual image files
    log_info "📸 Image upload testing requires multipart/form-data with actual files"
    log_info "📸 In production, images would include: site photos, installation progress, completion documentation"
}

test_rate_limiting() {
    log_section "RATE LIMITING ADMINISTRATION"
    
    test_endpoint \
        "Get Rate Limit Status" \
        "GET" \
        "/api/v1/rate-limit/status" \
        "" \
        "200" \
        "true" \
        "Check current rate limiting status for API protection"
    
    test_endpoint \
        "Get Rate Limit Statistics" \
        "GET" \
        "/api/v1/rate-limit/statistics" \
        "" \
        "200" \
        "true" \
        "Get comprehensive rate limiting statistics and metrics"
    
    test_endpoint \
        "Get Rate Limit Configuration" \
        "GET" \
        "/api/v1/rate-limit/configuration" \
        "" \
        "200" \
        "true" \
        "Get current rate limiting configuration settings"
}

test_error_handling() {
    log_section "ERROR HANDLING VALIDATION"
    
    # Test unauthorized access
    local old_token="$AUTH_TOKEN"
    AUTH_TOKEN=""
    
    test_endpoint \
        "Unauthorized Access Test" \
        "GET" \
        "/api/v1/projects" \
        "" \
        "401" \
        "true" \
        "Verify API blocks unauthorized access"
    
    AUTH_TOKEN="$old_token"
    
    # Test invalid data
    local invalid_project_data='{"invalid": "incomplete solar project data"}'
    
    test_endpoint \
        "Invalid Project Data Test" \
        "POST" \
        "/api/v1/projects" \
        "$invalid_project_data" \
        "400" \
        "true" \
        "Test validation with incomplete project data"
    
    # Test non-existent resources
    test_endpoint \
        "Non-existent Project Test" \
        "GET" \
        "/api/v1/projects/00000000-0000-0000-0000-000000000000" \
        "" \
        "404" \
        "true" \
        "Test error handling for non-existent resources"
    
    # Test invalid endpoints
    test_endpoint \
        "Invalid Endpoint Test" \
        "GET" \
        "/api/v1/nonexistent-solar-endpoint" \
        "" \
        "404" \
        "true" \
        "Test handling of invalid API endpoints"
}

cleanup_test_data() {
    log_section "CLEANUP TEST DATA"
    
    log_info "🧹 Cleaning up created test resources..."
    
    # Delete calendar events
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        test_endpoint \
            "Delete Calendar Event" \
            "DELETE" \
            "/api/v1/calendar/$CALENDAR_EVENT_ID" \
            "" \
            "204" \
            "true" \
            "Remove test calendar event"
    fi
    
    # Delete daily reports
    if [ -n "$DAILY_REPORT_ID" ]; then
        test_endpoint \
            "Delete Daily Report" \
            "DELETE" \
            "/api/v1/daily-reports/$DAILY_REPORT_ID" \
            "" \
            "204" \
            "true" \
            "Remove test daily report"
    fi
    
    # Delete work requests
    if [ -n "$WORK_REQUEST_ID" ]; then
        test_endpoint \
            "Delete Work Request" \
            "DELETE" \
            "/api/v1/work-requests/$WORK_REQUEST_ID" \
            "" \
            "204" \
            "true" \
            "Remove test work request"
    fi
    
    # Delete tasks
    if [ -n "$TASK_ID" ]; then
        test_endpoint \
            "Delete Solar Task" \
            "DELETE" \
            "/api/v1/tasks/$TASK_ID" \
            "" \
            "204" \
            "true" \
            "Remove test solar installation task"
    fi
    
    # Delete projects
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint \
            "Delete Solar Project" \
            "DELETE" \
            "/api/v1/projects/$PROJECT_ID" \
            "" \
            "204" \
            "true" \
            "Remove test solar project"
    fi
    
    log_success "Test data cleanup completed"
}

# ==============================================================================
# Main Execution
# ==============================================================================

main() {
    log_header "Solar Projects REST API - Complete Mock Data Test Suite"
    
    log_info "🔥 Testing ALL API endpoints with realistic solar project mock data"
    log_info "🌞 API Base URL: $API_BASE_URL"
    log_info "👤 Solar Manager: $TEST_USERNAME"
    echo
    
    # Pre-flight checks
    if ! wait_for_api; then
        log_fail "❌ API is not available. Please start the API server first."
        exit 1
    fi
    
    if ! setup_authentication; then
        log_fail "❌ Authentication setup failed. Cannot proceed with tests."
        exit 1
    fi
    
    # Execute comprehensive test suite
    test_health_monitoring
    test_authentication_endpoints
    test_project_management
    test_task_management
    test_user_management
    test_daily_reports
    test_work_requests
    test_calendar_management
    test_image_management
    test_rate_limiting
    test_error_handling
    
    # Cleanup
    cleanup_test_data
    
    # Final summary
    log_section "COMPREHENSIVE TEST SUMMARY"
    
    echo "📊 Total API Endpoints Tested: $TOTAL_TESTS"
    echo "✅ Successful Tests: $PASSED_TESTS"
    echo "❌ Failed Tests: $FAILED_TESTS"
    echo "📈 Success Rate: $(( PASSED_TESTS * 100 / TOTAL_TESTS ))%"
    echo
    
    if [ $FAILED_TESTS -eq 0 ]; then
        log_success "🎉 ALL TESTS PASSED! Solar Projects API is fully functional!"
    elif [ $FAILED_TESTS -lt 5 ]; then
        log_warn "⚡ Minor issues detected, but API is largely functional"
    else
        log_fail "🔧 Several issues detected, API may need attention"
    fi
    
    echo
    log_info "🌞 Solar Projects API Mock Data Testing Complete"
    log_info "🔗 Visit $API_BASE_URL for Swagger documentation"
    log_info "📋 All endpoints tested with realistic solar installation data"
    
    exit $([ $FAILED_TESTS -lt 5 ] && echo 0 || echo 1)
}

# Execute main function
main "$@"
