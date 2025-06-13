#!/bin/bash

# =============================================================================
# Solar Projects REST API - Comprehensive Test Suite
# =============================================================================
# This script tests all API endpoints documented in API_REFERENCE.md
# Usage: ./test-all-api.sh
# =============================================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# API Configuration
API_BASE="${API_BASE:-http://localhost:5002}"
CONTENT_TYPE="Content-Type: application/json"
RATE_LIMIT_DELAY=3  # Delay between requests to avoid rate limiting

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Test user credentials (these should exist in your database)
TEST_USERNAME="john_doe"
TEST_PASSWORD="TestPass123!"

# Global variables for storing IDs
AUTH_TOKEN=""
USER_ID=""
PROJECT_ID=""
TASK_ID=""
DAILY_REPORT_ID=""
WORK_REQUEST_ID=""
WORK_PROGRESS_ID=""
CALENDAR_EVENT_ID=""

# =============================================================================
# Utility Functions
# =============================================================================

print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
}

print_test() {
    echo -e "${BLUE}🧪 Testing: $1${NC}"
    ((TOTAL_TESTS++))
}

print_success() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
    ((PASSED_TESTS++))
}

print_error() {
    echo -e "${RED}❌ FAIL: $1${NC}"
    ((FAILED_TESTS++))
}

print_warning() {
    echo -e "${YELLOW}⚠️  WARN: $1${NC}"
}

print_info() {
    echo -e "${PURPLE}ℹ️  INFO: $1${NC}"
}

# Add delay to avoid rate limiting
rate_limit_delay() {
    if [ "${SKIP_DELAYS:-false}" != "true" ]; then
        sleep $RATE_LIMIT_DELAY
    fi
}

# Check if response contains success field
check_success() {
    local response="$1"
    local test_name="$2"
    
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "$test_name"
        return 0
    else
        print_error "$test_name"
        echo "Response: $response"
        return 1
    fi
}

# Check HTTP status code
check_status() {
    local status="$1"
    local expected="$2"
    local test_name="$3"
    
    if [ "$status" = "$expected" ]; then
        print_success "$test_name (Status: $status)"
        return 0
    else
        print_error "$test_name (Expected: $expected, Got: $status)"
        return 1
    fi
}

# Wait for API to be ready
wait_for_api() {
    print_info "Waiting for API to be ready..."
    local retries=30
    local count=0
    
    while [ $count -lt $retries ]; do
        if curl -s "$API_BASE/health" > /dev/null 2>&1; then
            print_success "API is ready!"
            return 0
        fi
        ((count++))
        echo -n "."
        sleep 1
    done
    
    print_error "API is not responding after $retries seconds"
    return 1
}

# =============================================================================
# Test Functions
# =============================================================================

test_health_endpoints() {
    print_header "HEALTH MONITORING TESTS"
    
    # Basic health check
    print_test "Basic Health Check"
    response=$(curl -s "$API_BASE/health")
    if echo "$response" | jq -e '.status == "Healthy"' > /dev/null 2>&1; then
        print_success "Basic health check"
    else
        print_error "Basic health check"
    fi
    
    # Detailed health check
    print_test "Detailed Health Check"
    response=$(curl -s "$API_BASE/health/detailed")
    if echo "$response" | jq -e '.status == "Healthy"' > /dev/null 2>&1; then
        print_success "Detailed health check"
    else
        print_error "Detailed health check"
    fi
}

test_authentication() {
    print_header "AUTHENTICATION TESTS"
    
    # Test login
    print_test "User Login"
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "$CONTENT_TYPE" \
        -d "{\"username\":\"$TEST_USERNAME\",\"password\":\"$TEST_PASSWORD\"}")
    
    if check_success "$response" "User login"; then
        AUTH_TOKEN=$(echo "$response" | jq -r '.data.token')
        USER_ID=$(echo "$response" | jq -r '.data.user.userId')
        print_info "Auth token obtained: ${AUTH_TOKEN:0:20}..."
        print_info "User ID: $USER_ID"
    else
        print_warning "Login failed - some tests may not work without authentication"
    fi
    
    # Test invalid login
    print_test "Invalid Login"
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "$CONTENT_TYPE" \
        -d '{"username":"invalid","password":"wrong"}')
    
    if echo "$response" | jq -e '.success == false' > /dev/null 2>&1; then
        print_success "Invalid login correctly rejected"
    else
        print_error "Invalid login should be rejected"
    fi
}

test_user_management() {
    print_header "USER MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping user management tests - no auth token"
        return
    fi
    
    # Get all users
    print_test "Get All Users"
    response=$(curl -s "$API_BASE/api/v1/users" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get all users"
    
    # Get user by ID
    if [ -n "$USER_ID" ]; then
        print_test "Get User by ID"
        response=$(curl -s "$API_BASE/api/v1/users/$USER_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get user by ID"
    fi
    
    # Test pagination
    print_test "User Pagination"
    response=$(curl -s "$API_BASE/api/v1/users?pageNumber=1&pageSize=5" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "User pagination"
}

test_project_management() {
    print_header "PROJECT MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping project management tests - no auth token"
        return
    fi
    
    # Get all projects
    print_test "Get All Projects"
    response=$(curl -s "$API_BASE/api/v1/projects" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if check_success "$response" "Get all projects"; then
        # Extract first project ID if available
        PROJECT_ID=$(echo "$response" | jq -r '.data.items[0].projectId // empty')
        if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
            print_info "Found project ID: $PROJECT_ID"
        fi
    fi
    
    # Get project by ID (if we have one)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_test "Get Project by ID"
        response=$(curl -s "$API_BASE/api/v1/projects/$PROJECT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get project by ID"
    fi
    
    # Test project creation (will likely fail without proper permissions)
    print_test "Create Project (Permission Test)"
    response=$(curl -s -X POST "$API_BASE/api/v1/projects" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "projectName": "Test Project",
            "address": "123 Test St, Test City, TC 12345",
            "clientInfo": "Test Client - Contact: Test User (555-123-4567)",
            "startDate": "2025-06-10T00:00:00Z",
            "estimatedEndDate": "2025-08-10T00:00:00Z"
        }')
    
    # This might fail due to permissions, which is expected
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Create project"
        NEW_PROJECT_ID=$(echo "$response" | jq -r '.data.projectId')
        print_info "Created project ID: $NEW_PROJECT_ID"
    else
        print_warning "Create project failed (may be due to permissions)"
    fi
}

test_task_management() {
    print_header "TASK MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping task management tests - no auth token"
        return
    fi
    
    # Get all tasks
    print_test "Get All Tasks"
    response=$(curl -s "$API_BASE/api/v1/tasks" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if check_success "$response" "Get all tasks"; then
        TASK_ID=$(echo "$response" | jq -r '.data.items[0].taskId // empty')
        if [ -n "$TASK_ID" ] && [ "$TASK_ID" != "null" ]; then
            print_info "Found task ID: $TASK_ID"
        fi
    fi
    
    # Get task by ID (if we have one)
    if [ -n "$TASK_ID" ] && [ "$TASK_ID" != "null" ]; then
        print_test "Get Task by ID"
        response=$(curl -s "$API_BASE/api/v1/tasks/$TASK_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get task by ID"
    fi
    
    # Test task filtering
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_test "Filter Tasks by Project"
        response=$(curl -s "$API_BASE/api/v1/tasks?projectId=$PROJECT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Filter tasks by project"
    fi
}

test_daily_reports() {
    print_header "DAILY REPORTS MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping daily reports tests - no auth token"
        return
    fi
    
    # Get all daily reports
    print_test "Get All Daily Reports"
    response=$(curl -s "$API_BASE/api/v1/daily-reports" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if check_success "$response" "Get all daily reports"; then
        DAILY_REPORT_ID=$(echo "$response" | jq -r '.data.items[0].reportId // empty')
        if [ -n "$DAILY_REPORT_ID" ] && [ "$DAILY_REPORT_ID" != "null" ]; then
            print_info "Found daily report ID: $DAILY_REPORT_ID"
        fi
    fi
    
    # Get daily report by ID (if we have one)
    if [ -n "$DAILY_REPORT_ID" ] && [ "$DAILY_REPORT_ID" != "null" ]; then
        print_test "Get Daily Report by ID"
        response=$(curl -s "$API_BASE/api/v1/daily-reports/$DAILY_REPORT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get daily report by ID"
        
        # Test work progress items
        print_test "Get Work Progress Items"
        response=$(curl -s "$API_BASE/api/v1/daily-reports/$DAILY_REPORT_ID/work-progress" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get work progress items"
        
        # Test personnel logs
        print_test "Get Personnel Logs"
        response=$(curl -s "$API_BASE/api/v1/daily-reports/$DAILY_REPORT_ID/personnel-logs" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get personnel logs"
        
        # Test material usage
        print_test "Get Material Usage"
        response=$(curl -s "$API_BASE/api/v1/daily-reports/$DAILY_REPORT_ID/material-usage" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get material usage"
        
        # Test equipment logs
        print_test "Get Equipment Logs"
        response=$(curl -s "$API_BASE/api/v1/daily-reports/$DAILY_REPORT_ID/equipment-logs" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get equipment logs"
    fi
    
    # Test daily reports filtering
    print_test "Filter Daily Reports by Status"
    response=$(curl -s "$API_BASE/api/v1/daily-reports?status=Draft&pageSize=5" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Filter daily reports by status"
    
    # Test create daily report (if we have a project)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_test "Create Daily Report"
        response=$(curl -s -X POST "$API_BASE/api/v1/daily-reports" \
            -H "$CONTENT_TYPE" \
            -H "Authorization: Bearer $AUTH_TOKEN" \
            -d "{
                \"projectId\": \"$PROJECT_ID\",
                \"reportDate\": \"2025-06-09T00:00:00Z\",
                \"workStartTime\": \"08:00:00\",
                \"workEndTime\": \"17:00:00\",
                \"weatherConditions\": \"Sunny, 75°F, Light breeze\",
                \"overallNotes\": \"Test report created by automated test\",
                \"safetyNotes\": \"All safety protocols followed during testing\",
                \"delaysOrIssues\": \"No issues - this is a test report\",
                \"photosCount\": 0
            }")
        
        if check_success "$response" "Create daily report"; then
            NEW_REPORT_ID=$(echo "$response" | jq -r '.data.reportId')
            print_info "Created daily report ID: $NEW_REPORT_ID"
            
            # Test submit report
            print_test "Submit Daily Report"
            response=$(curl -s -X POST "$API_BASE/api/v1/daily-reports/$NEW_REPORT_ID/submit" \
                -H "Authorization: Bearer $AUTH_TOKEN")
            check_success "$response" "Submit daily report"
        fi
    fi
}

test_work_requests() {
    print_header "WORK REQUEST MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping work request tests - no auth token"
        return
    fi
    
    # Get all work requests
    print_test "Get All Work Requests"
    response=$(curl -s "$API_BASE/api/v1/work-requests" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if check_success "$response" "Get all work requests"; then
        WORK_REQUEST_ID=$(echo "$response" | jq -r '.data.items[0].workRequestId // empty')
        if [ -n "$WORK_REQUEST_ID" ] && [ "$WORK_REQUEST_ID" != "null" ]; then
            print_info "Found work request ID: $WORK_REQUEST_ID"
        fi
    fi
    
    # Get work request by ID (if we have one)
    if [ -n "$WORK_REQUEST_ID" ] && [ "$WORK_REQUEST_ID" != "null" ]; then
        print_test "Get Work Request by ID"
        response=$(curl -s "$API_BASE/api/v1/work-requests/$WORK_REQUEST_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get work request by ID"
    fi
    
    # Test work request filtering
    print_test "Filter Work Requests by Priority"
    response=$(curl -s "$API_BASE/api/v1/work-requests?priority=Medium&pageSize=5" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Filter work requests by priority"
    
    # Test create work request (if we have a project)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_test "Create Work Request"
        response=$(curl -s -X POST "$API_BASE/api/v1/work-requests" \
            -H "$CONTENT_TYPE" \
            -H "Authorization: Bearer $AUTH_TOKEN" \
            -d "{
                \"projectId\": \"$PROJECT_ID\",
                \"requestType\": \"Change Order\",
                \"title\": \"Test Work Request\",
                \"description\": \"This is a test work request created by automated testing\",
                \"priority\": \"Low\",
                \"estimatedCost\": 500.00,
                \"estimatedHours\": 4.0
            }")
        
        if check_success "$response" "Create work request"; then
            NEW_WR_ID=$(echo "$response" | jq -r '.data.workRequestId')
            print_info "Created work request ID: $NEW_WR_ID"
        fi
    fi
}

test_calendar_management() {
    print_header "CALENDAR MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_error "No authentication token available. Calendar tests require authentication."
        return
    fi
    
    # Create a calendar event
    print_test "Create Calendar Event"
    rate_limit_delay
    response=$(curl -s -X POST "$API_BASE/api/v1/calendar" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "title": "Test Calendar Event",
            "description": "This is a test calendar event created by automated testing",
            "startDateTime": "2025-06-15T09:00:00Z",
            "endDateTime": "2025-06-15T10:30:00Z",
            "eventType": "Meeting",
            "status": "Scheduled",
            "priority": "Medium",
            "location": "Conference Room A",
            "isAllDay": false,
            "isRecurring": false,
            "notes": "Automated test event",
            "reminderMinutes": 15,
            "color": "#2196F3",
            "isPrivate": false,
            "attendees": "test@example.com"
        }')
    
    if check_success "$response" "Create calendar event"; then
        CALENDAR_EVENT_ID=$(echo "$response" | jq -r '.data.eventId')
        print_info "Created calendar event ID: $CALENDAR_EVENT_ID"
    fi
    
    # Get all calendar events with pagination
    print_test "Get All Calendar Events"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?pageSize=10" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get all calendar events"
    
    # Get calendar event by ID (if we have one)
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        print_test "Get Calendar Event by ID"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get calendar event by ID"
        
        # Update calendar event
        print_test "Update Calendar Event"
        rate_limit_delay
        response=$(curl -s -X PUT "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "$CONTENT_TYPE" \
            -H "Authorization: Bearer $AUTH_TOKEN" \
            -d '{
                "title": "Updated Test Calendar Event",
                "description": "Updated description for automated testing",
                "startDateTime": "2025-06-15T10:00:00Z",
                "endDateTime": "2025-06-15T11:30:00Z",
                "status": "InProgress",
                "priority": "High",
                "location": "Conference Room B",
                "notes": "Updated automated test event",
                "reminderMinutes": 30,
                "color": "#FF9800"
            }')
        check_success "$response" "Update calendar event"
    fi
    
    # Get upcoming events
    print_test "Get Upcoming Events"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar/upcoming?days=30" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get upcoming events"
    
    # Test conflict checking
    print_test "Check Event Conflicts"
    rate_limit_delay
    response=$(curl -s -X POST "$API_BASE/api/v1/calendar/conflicts" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "startDateTime": "2025-06-15T09:30:00Z",
            "endDateTime": "2025-06-15T11:00:00Z",
            "userId": "'$USER_ID'"
        }')
    check_success "$response" "Check event conflicts"
    
    # Test filtering by date range
    print_test "Get Events by Date Range"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?startDate=2025-06-01&endDate=2025-06-30" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by date range"
    
    # Test filtering by event type
    print_test "Get Events by Type"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?eventType=Meeting" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by type"
    
    # Test filtering by status
    print_test "Get Events by Status"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?status=Scheduled" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by status"
    
    # Test events by project (if we have a project ID)
    if [ -n "$PROJECT_ID" ]; then
        print_test "Get Events by Project"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/project/$PROJECT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get events by project"
    fi
    
    # Test events by task (if we have a task ID)
    if [ -n "$TASK_ID" ]; then
        print_test "Get Events by Task"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/task/$TASK_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get events by task"
    fi
    
    # Test events by user
    if [ -n "$USER_ID" ]; then
        print_test "Get Events by User"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/user/$USER_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get events by user"
    fi
    
    # Test recurring events endpoints (placeholders)
    print_test "Get Recurring Events (Placeholder)"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar/recurring" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    # We expect this to work even if it returns empty data
    check_success "$response" "Get recurring events"
    
    # Clean up - delete the test calendar event (if we have one)
    if [ -n "$CALENDAR_EVENT_ID" ]; then
        print_test "Delete Calendar Event"
        rate_limit_delay
        response=$(curl -s -X DELETE "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Delete calendar event"
    fi
}

test_debug_endpoints() {
    print_header "DEBUG INFORMATION TESTS"
    
    # Test configuration endpoint
    print_test "Get Configuration"
    response=$(curl -s "$API_BASE/api/debug/config")
    if [ $? -eq 0 ] && [ -n "$response" ]; then
        print_success "Get configuration"
    else
        print_error "Get configuration"
    fi
}

test_rate_limiting() {
    print_header "RATE LIMITING TESTS"
    
    print_test "Rate Limit Headers"
    response=$(curl -s -I "$API_BASE/health")
    if echo "$response" | grep -i "x-ratelimit" > /dev/null; then
        print_success "Rate limit headers present"
    else
        print_warning "Rate limit headers not found (may not be configured)"
    fi
    
    # Test rapid requests (be careful not to hit actual limits)
    print_test "Multiple Rapid Requests"
    local success_count=0
    for i in {1..5}; do
        response=$(curl -s "$API_BASE/health")
        if echo "$response" | jq -e '.status == "Healthy"' > /dev/null 2>&1; then
            ((success_count++))
        fi
        sleep 0.1
    done
    
    if [ $success_count -eq 5 ]; then
        print_success "Multiple rapid requests handled"
    else
        print_warning "Some rapid requests failed ($success_count/5 succeeded)"
    fi
}

test_advanced_querying() {
    print_header "ADVANCED QUERYING TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping advanced querying tests - no auth token"
        return
    fi
    
    # Test user advanced querying
    print_test "Advanced User Query"
    response=$(curl -s "$API_BASE/api/v1/users/advanced?pageSize=5&sortBy=fullName&sortOrder=asc" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Advanced user query"
    
    # Test project advanced querying
    print_test "Advanced Project Query"
    response=$(curl -s "$API_BASE/api/v1/projects/advanced?pageSize=3&sortBy=createdAt&sortOrder=desc" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Advanced project query"
    
    # Test field selection
    print_test "Field Selection Query"
    response=$(curl -s "$API_BASE/api/v1/users/advanced?fields=userId,username,email&pageSize=3" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Field selection query"
}

# =============================================================================
# Main Test Execution
# =============================================================================

main() {
    echo -e "${PURPLE}"
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║               Solar Projects REST API Test Suite                ║"
    echo "║                      Comprehensive Testing                      ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
    
    print_info "API Base URL: $API_BASE"
    print_info "Test User: $TEST_USERNAME"
    print_info "Starting comprehensive API tests..."
    
    # Check if API is running
    if ! wait_for_api; then
        print_error "API is not available. Please start the API first with: dotnet run --urls http://localhost:5002"
        exit 1
    fi
    
    # Run all test suites
    test_health_endpoints
    test_authentication
    test_user_management
    test_project_management
    test_task_management
    test_daily_reports
    test_work_requests
    test_calendar_management
    test_debug_endpoints
    test_rate_limiting
    test_advanced_querying
    
    # Print summary
    print_header "TEST SUMMARY"
    echo -e "${CYAN}Total Tests: $TOTAL_TESTS${NC}"
    echo -e "${GREEN}Passed: $PASSED_TESTS${NC}"
    echo -e "${RED}Failed: $FAILED_TESTS${NC}"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 All tests passed!${NC}"
        exit 0
    else
        echo -e "${YELLOW}⚠️  Some tests failed. Check the output above for details.${NC}"
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    print_error "curl is required but not installed"
    exit 1
fi

if ! command -v jq &> /dev/null; then
    print_error "jq is required but not installed. Install with: brew install jq"
    exit 1
fi

# Run main function
main "$@"
