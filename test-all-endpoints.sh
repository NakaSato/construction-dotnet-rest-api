#!/bin/bash

# ==============================================================================
# Solar Projects REST API - Comprehensive All Endpoints Test Script
# Tests all available API endpoints with simple test data
# ==============================================================================

set -e  # Exit on any error

# Configuration
API_BASE_URL="${API_BASE_URL:-http://localhost:5002}"
TIMESTAMP=$(date +%s)
TEST_USERNAME="${TEST_USERNAME:-testuser_$TIMESTAMP}"
TEST_PASSWORD="${TEST_PASSWORD:-SecurePassword123!}"
TEST_EMAIL="${TEST_EMAIL:-testuser_$TIMESTAMP@example.com}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Global variables
AUTH_TOKEN=""
USER_ID=""
PROJECT_ID=""
TASK_ID=""
DAILY_REPORT_ID=""
WORK_REQUEST_ID=""
CALENDAR_EVENT_ID=""

# ==============================================================================
# Utility Functions
# ==============================================================================

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

test_endpoint() {
    local test_name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    local auth_required="${6:-true}"
    
    ((TOTAL_TESTS++))
    
    echo "🧪 Testing: $test_name"
    
    local curl_cmd="curl -s"
    
    if [ "$auth_required" = "true" ] && [ -n "$AUTH_TOKEN" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $AUTH_TOKEN'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ -n "$data" ]; then
        curl_cmd="$curl_cmd -d '$data'"
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
        case "$endpoint" in
            */projects*)
                if [ "$method" = "POST" ] && [ -z "$PROJECT_ID" ]; then
                    PROJECT_ID=$(echo "$body" | grep -o '"projectId":"[^"]*"' | cut -d'"' -f4 | head -1)
                fi
                ;;
            */tasks*)
                if [ "$method" = "POST" ] && [ -z "$TASK_ID" ]; then
                    TASK_ID=$(echo "$body" | grep -o '"taskId":"[^"]*"' | cut -d'"' -f4 | head -1)
                fi
                ;;
            */daily-reports*)
                if [ "$method" = "POST" ] && [ -z "$DAILY_REPORT_ID" ]; then
                    DAILY_REPORT_ID=$(echo "$body" | grep -o '"reportId":"[^"]*"' | cut -d'"' -f4 | head -1)
                fi
                ;;
            */work-requests*)
                if [ "$method" = "POST" ] && [ -z "$WORK_REQUEST_ID" ]; then
                    WORK_REQUEST_ID=$(echo "$body" | grep -o '"workRequestId":"[^"]*"' | cut -d'"' -f4 | head -1)
                fi
                ;;
            */calendar*)
                if [ "$method" = "POST" ] && [ -z "$CALENDAR_EVENT_ID" ]; then
                    CALENDAR_EVENT_ID=$(echo "$body" | grep -o '"eventId":"[^"]*"' | cut -d'"' -f4 | head -1)
                fi
                ;;
        esac
        
        return 0
    else
        log_fail "$test_name (Expected: $expected_status, Got: $status_code)"
        log_info "Response: $body"
        return 1
    fi
}

wait_for_api() {
    log_info "Waiting for API to be ready..."
    local retries=30
    while [ $retries -gt 0 ]; do
        if curl -s "$API_BASE_URL/health" > /dev/null 2>&1; then
            log_success "API is ready!"
            return 0
        fi
        retries=$((retries - 1))
        sleep 1
    done
    log_fail "API is not responding after 30 seconds"
    return 1
}

register_test_user() {
    log_info "Registering test user..."
    
    local register_data='{
        "username": "'$TEST_USERNAME'",
        "password": "'$TEST_PASSWORD'",
        "email": "'$TEST_EMAIL'",
        "fullName": "Test User",
        "roleId": 2
    }'
    
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$register_data" \
        -w "%{http_code}" \
        "$API_BASE_URL/api/v1/auth/register")
    
    local status_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$status_code" = "201" ] || [ "$status_code" = "200" ]; then
        log_success "Test user registered successfully"
        return 0
    elif [ "$status_code" = "400" ] && echo "$body" | grep -q "already exists"; then
        log_info "Test user already exists, proceeding with login"
        return 0
    else
        log_warn "User registration failed (Status: $status_code) - will try login anyway"
        log_info "Response: $body"
        return 0
    fi
}

authenticate() {
    log_info "Starting authentication process..."
    
    local login_data='{
        "username": "'$TEST_USERNAME'",
        "password": "'$TEST_PASSWORD'"
    }'
    
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$login_data" \
        -w "%{http_code}" \
        "$API_BASE_URL/api/v1/auth/login")
    
    local status_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$status_code" = "200" ]; then
        AUTH_TOKEN=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        USER_ID=$(echo "$body" | grep -o '"userId":"[^"]*"' | cut -d'"' -f4)
        log_success "Authentication successful"
        log_info "Auth token obtained: ${AUTH_TOKEN:0:20}..."
        log_info "User ID: $USER_ID"
        return 0
    else
        log_fail "Authentication failed (Status: $status_code)"
        log_info "Response: $body"
        return 1
    fi
}

# ==============================================================================
# Test Functions
# ==============================================================================

test_health_endpoints() {
    echo
    echo "========================================"
    echo "HEALTH ENDPOINTS"
    echo "========================================"
    
    test_endpoint "Basic Health Check" "GET" "/health" "" "200" "false"
    test_endpoint "Detailed Health Check" "GET" "/health/detailed" "" "200" "false"
}

test_auth_endpoints() {
    echo
    echo "========================================"
    echo "AUTHENTICATION ENDPOINTS"
    echo "========================================"
    
    # Login (already done in authenticate function)
    log_info "✅ Login test completed in authentication step"
    ((TOTAL_TESTS++))
    ((PASSED_TESTS++))
    
    # Test register endpoint (if available)
    local register_data='{
        "username": "test_user_'$(date +%s)'",
        "password": "TestPassword123!",
        "email": "test'$(date +%s)'@example.com",
        "fullName": "Test User",
        "roleId": 2
    }'
    
    test_endpoint "User Registration" "POST" "/api/v1/auth/register" "$register_data" "200" "false"
    
    # Test refresh token (if we have one)
    if [ -n "$AUTH_TOKEN" ]; then
        local refresh_data='{"refreshToken": "dummy_refresh_token_value"}'
        test_endpoint "Token Refresh" "POST" "/api/v1/auth/refresh" "$refresh_data" "400" "false" || true
    fi
}

test_project_endpoints() {
    echo
    echo "========================================"
    echo "PROJECT MANAGEMENT ENDPOINTS"
    echo "========================================"
    
    # Get all projects
    test_endpoint "Get All Projects" "GET" "/api/v1/projects" "" "200"
    
    # Get projects with pagination
    test_endpoint "Get Projects with Pagination" "GET" "/api/v1/projects?page=1&pageSize=10" "" "200"
    
    # Create new project
    local project_data='{
        "projectName": "Test Solar Installation Project",
        "address": "123 Solar Street, Phoenix, AZ 85001",
        "clientInfo": "Test Client Corp - Contact: John Smith, Phone: +1-555-0123, Email: john@testcorp.com",
        "status": 0,
        "startDate": "2025-07-01T00:00:00Z",
        "estimatedEndDate": "2025-12-31T23:59:59Z",
        "projectManagerId": "'$USER_ID'"
    }'
    
    test_endpoint "Create New Project" "POST" "/api/v1/projects" "$project_data" "201"
    
    # Get project by ID (if we have one)
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint "Get Project by ID" "GET" "/api/v1/projects/$PROJECT_ID" "" "200"
        
        # Update project
        local update_data='{
            "projectName": "Updated Test Solar Project",
            "clientInfo": "Updated client information for testing",
            "status": 1
        }'
        test_endpoint "Update Project" "PUT" "/api/v1/projects/$PROJECT_ID" "$update_data" "200"
    fi
    
    # Test with invalid ID
    test_endpoint "Get Project with Invalid ID" "GET" "/api/v1/projects/invalid-id" "" "400"
}

test_task_endpoints() {
    echo
    echo "========================================"
    echo "TASK MANAGEMENT ENDPOINTS"
    echo "========================================"
    
    # Get all tasks
    test_endpoint "Get All Tasks" "GET" "/api/v1/tasks" "" "200"
    
    # Create new task (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        local task_data='{
            "title": "Install Solar Panels",
            "description": "Install 20 solar panels on the roof",
            "projectId": "'$PROJECT_ID'",
            "assignedToId": "'$USER_ID'",
            "dueDate": "2025-08-15T00:00:00Z",
            "priority": "High",
            "status": "Open"
        }'
        
        test_endpoint "Create New Task" "POST" "/api/v1/tasks" "$task_data" "201"
        
        # Get task by ID (if we have one)
        if [ -n "$TASK_ID" ]; then
            test_endpoint "Get Task by ID" "GET" "/api/v1/tasks/$TASK_ID" "" "200"
            
            # Update task
            local update_task_data='{
                "title": "Updated Task Title",
                "status": "In Progress"
            }'
            test_endpoint "Update Task" "PUT" "/api/v1/tasks/$TASK_ID" "$update_task_data" "200"
        fi
    fi
    
    # Get tasks by project (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint "Get Tasks by Project" "GET" "/api/v1/tasks/project/$PROJECT_ID" "" "200"
    fi
}

test_user_endpoints() {
    echo
    echo "========================================"
    echo "USER MANAGEMENT ENDPOINTS"
    echo "========================================"
    
    # Get all users
    test_endpoint "Get All Users" "GET" "/api/v1/users" "" "200"
    
    # Get current user profile
    test_endpoint "Get User Profile" "GET" "/api/v1/users/profile" "" "200"
    
    # Get user by ID
    if [ -n "$USER_ID" ]; then
        test_endpoint "Get User by ID" "GET" "/api/v1/users/$USER_ID" "" "200"
    fi
    
    # Search users
    test_endpoint "Search Users" "GET" "/api/v1/users/search?query=john" "" "200"
}

test_daily_reports_endpoints() {
    echo
    echo "========================================"
    echo "DAILY REPORTS ENDPOINTS"
    echo "========================================"
    
    # Get all daily reports
    test_endpoint "Get All Daily Reports" "GET" "/api/v1/daily-reports" "" "200"
    
    # Create new daily report (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        local report_data='{
            "projectId": "'$PROJECT_ID'",
            "reportDate": "2025-06-14T00:00:00Z",
            "weatherConditions": "Sunny, 75°F",
            "workDescription": "Installed solar panels on south roof section",
            "workersPresent": 4,
            "hoursWorked": 8.5,
            "materialsUsed": "20 solar panels, mounting hardware",
            "equipmentUsed": "Crane, hand tools",
            "safetyIncidents": "None reported",
            "progressNotes": "Ahead of schedule",
            "submittedById": "'$USER_ID'"
        }'
        
        test_endpoint "Create Daily Report" "POST" "/api/v1/daily-reports" "$report_data" "201"
        
        # Get daily report by ID (if we have one)
        if [ -n "$DAILY_REPORT_ID" ]; then
            test_endpoint "Get Daily Report by ID" "GET" "/api/v1/daily-reports/$DAILY_REPORT_ID" "" "200"
            
            # Update daily report
            local update_report_data='{
                "workDescription": "Updated work description",
                "progressNotes": "Updated progress notes"
            }'
            test_endpoint "Update Daily Report" "PUT" "/api/v1/daily-reports/$DAILY_REPORT_ID" "$update_report_data" "200"
            
            # Submit daily report
            test_endpoint "Submit Daily Report" "POST" "/api/v1/daily-reports/$DAILY_REPORT_ID/submit" "" "200"
        fi
    fi
    
    # Get daily reports by project (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint "Get Daily Reports by Project" "GET" "/api/v1/daily-reports/project/$PROJECT_ID" "" "200"
    fi
}

test_work_requests_endpoints() {
    echo
    echo "========================================"
    echo "WORK REQUEST ENDPOINTS"
    echo "========================================"
    
    # Get all work requests
    test_endpoint "Get All Work Requests" "GET" "/api/v1/work-requests" "" "200"
    
    # Create new work request (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        local work_request_data='{
            "projectId": "'$PROJECT_ID'",
            "title": "Additional Electrical Work",
            "description": "Need to install additional electrical conduit",
            "requestedById": "'$USER_ID'",
            "priority": "Medium",
            "estimatedCost": 5000.00,
            "estimatedHours": 16,
            "requestType": "Change Order"
        }'
        
        test_endpoint "Create Work Request" "POST" "/api/v1/work-requests" "$work_request_data" "201"
        
        # Get work request by ID (if we have one)
        if [ -n "$WORK_REQUEST_ID" ]; then
            test_endpoint "Get Work Request by ID" "GET" "/api/v1/work-requests/$WORK_REQUEST_ID" "" "200"
            
            # Update work request
            local update_work_request_data='{
                "title": "Updated Work Request Title",
                "priority": "High"
            }'
            test_endpoint "Update Work Request" "PUT" "/api/v1/work-requests/$WORK_REQUEST_ID" "$update_work_request_data" "200"
        fi
    fi
}

test_calendar_endpoints() {
    echo
    echo "========================================"
    echo "CALENDAR ENDPOINTS"
    echo "========================================"
    
    # Get all calendar events
    test_endpoint "Get All Calendar Events" "GET" "/api/v1/calendar" "" "200"
    
    # Get calendar events with date range
    test_endpoint "Get Calendar Events by Date Range" "GET" "/api/v1/calendar?startDate=2025-06-01&endDate=2025-12-31" "" "200"
    
    # Create new calendar event
    local calendar_data='{
        "title": "Solar Panel Installation Meeting",
        "description": "Weekly progress meeting for solar installation project",
        "startDateTime": "2025-06-20T10:00:00Z",
        "endDateTime": "2025-06-20T11:00:00Z",
        "eventType": "Meeting",
        "isAllDay": false,
        "organizerId": "'$USER_ID'"
    }'
    
    test_endpoint "Create Calendar Event" "POST" "/api/v1/calendar" "$calendar_data" "201"
    
    # Get calendar event by ID (if we have one)
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        test_endpoint "Get Calendar Event by ID" "GET" "/api/v1/calendar/$CALENDAR_EVENT_ID" "" "200"
        
        # Update calendar event
        local update_calendar_data='{
            "title": "Updated Meeting Title",
            "description": "Updated meeting description"
        }'
        test_endpoint "Update Calendar Event" "PUT" "/api/v1/calendar/$CALENDAR_EVENT_ID" "$update_calendar_data" "200"
    fi
    
    # Get upcoming events
    test_endpoint "Get Upcoming Events" "GET" "/api/v1/calendar/upcoming" "" "200"
    
    # Get events by user (if we have a user ID)
    if [ -n "$USER_ID" ]; then
        test_endpoint "Get Events by User" "GET" "/api/v1/calendar/user/$USER_ID" "" "200"
    fi
}

test_image_endpoints() {
    echo
    echo "========================================"
    echo "IMAGE ENDPOINTS"
    echo "========================================"
    
    # Get all images
    test_endpoint "Get All Images" "GET" "/api/v1/images" "" "200"
    
    # Note: File upload tests would require actual files, so we'll skip the POST test
    log_info "⏭️  Skipping image upload test (requires multipart/form-data)"
    
    # Get images by project (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint "Get Images by Project" "GET" "/api/v1/images/project/$PROJECT_ID" "" "200"
    fi
}

test_rate_limiting_endpoints() {
    echo
    echo "========================================"
    echo "RATE LIMITING ADMIN ENDPOINTS"
    echo "========================================"
    
    # Get rate limit status
    test_endpoint "Get Rate Limit Status" "GET" "/api/v1/rate-limit/status" "" "200"
    
    # Get rate limit statistics
    test_endpoint "Get Rate Limit Statistics" "GET" "/api/v1/rate-limit/statistics" "" "200"
    
    # Test rate limit configuration
    test_endpoint "Get Rate Limit Configuration" "GET" "/api/v1/rate-limit/configuration" "" "200"
}

# ==============================================================================
# Cleanup Functions
# ==============================================================================

cleanup_test_data() {
    echo
    echo "========================================"
    echo "CLEANUP TEST DATA"
    echo "========================================"
    
    # Delete calendar event
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        test_endpoint "Delete Calendar Event" "DELETE" "/api/v1/calendar/$CALENDAR_EVENT_ID" "" "204"
    fi
    
    # Delete daily report
    if [ -n "$DAILY_REPORT_ID" ]; then
        test_endpoint "Delete Daily Report" "DELETE" "/api/v1/daily-reports/$DAILY_REPORT_ID" "" "204"
    fi
    
    # Delete work request
    if [ -n "$WORK_REQUEST_ID" ]; then
        test_endpoint "Delete Work Request" "DELETE" "/api/v1/work-requests/$WORK_REQUEST_ID" "" "204"
    fi
    
    # Delete task
    if [ -n "$TASK_ID" ]; then
        test_endpoint "Delete Task" "DELETE" "/api/v1/tasks/$TASK_ID" "" "204"
    fi
    
    # Delete project
    if [ -n "$PROJECT_ID" ]; then
        test_endpoint "Delete Project" "DELETE" "/api/v1/projects/$PROJECT_ID" "" "204"
    fi
}

# ==============================================================================
# Main Execution
# ==============================================================================

main() {
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║          Solar Projects REST API - All Endpoints Test           ║"
    echo "║                   Comprehensive Testing Suite                   ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    
    log_info "API Base URL: $API_BASE_URL"
    log_info "Test User: $TEST_USERNAME"
    log_info "Starting comprehensive API endpoint tests..."
    
    # Wait for API to be ready
    if ! wait_for_api; then
        log_fail "API is not ready. Exiting."
        exit 1
    fi
    
    # Register test user first
    register_test_user
    
    # Authenticate
    if ! authenticate; then
        log_fail "Authentication failed. Exiting."
        exit 1
    fi
    
    # Run all endpoint tests
    test_health_endpoints
    test_auth_endpoints
    test_project_endpoints
    test_task_endpoints
    test_user_endpoints
    test_daily_reports_endpoints
    test_work_requests_endpoints
    test_calendar_endpoints
    test_image_endpoints
    test_rate_limiting_endpoints
    
    # Cleanup test data
    cleanup_test_data
    
    # Print summary
    echo
    echo "========================================"
    echo "TEST SUMMARY"
    echo "========================================"
    echo "Total Tests: $TOTAL_TESTS"
    echo "Passed: $PASSED_TESTS"
    echo "Failed: $FAILED_TESTS"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        log_success "All tests passed! 🎉"
        exit 0
    else
        log_warn "Some tests failed. Check the output above for details."
        exit 1
    fi
}

# Run the main function
main "$@"
