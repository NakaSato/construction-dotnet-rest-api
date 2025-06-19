#!/bin/bash

# Comprehensive API Endpoint Testing Script
# Tests all endpoints in the Solar Projects REST API

set -e

# Configuration
API_BASE="http://localhost:5002"
API_VERSION="v1"
API_URL="${API_BASE}/api/${API_VERSION}"
LOG_FILE="api-test-results-$(date +%Y%m%d_%H%M%S).log"
TOKEN=""
TEST_PROJECT_ID=""
TEST_USER_ID=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Test results arrays
declare -a PASSED_ENDPOINTS
declare -a FAILED_ENDPOINTS

log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

success() {
    echo -e "${GREEN}✅ $1${NC}" | tee -a "$LOG_FILE"
    ((PASSED_TESTS++))
    PASSED_ENDPOINTS+=("$1")
}

error() {
    echo -e "${RED}❌ $1${NC}" | tee -a "$LOG_FILE"
    ((FAILED_TESTS++))
    FAILED_ENDPOINTS+=("$1")
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}" | tee -a "$LOG_FILE"
}

info() {
    echo -e "${BLUE}ℹ️  $1${NC}" | tee -a "$LOG_FILE"
}

# Test helper function
test_endpoint() {
    local method="$1"
    local endpoint="$2"
    local description="$3"
    local auth_header="$4"
    local data="$5"
    local expected_status="${6:-200}"
    
    ((TOTAL_TESTS++))
    
    local curl_cmd="curl -s -w '%{http_code}' -X $method"
    
    if [[ -n "$auth_header" ]]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $TOKEN'"
    fi
    
    if [[ -n "$data" ]]; then
        curl_cmd="$curl_cmd -H 'Content-Type: application/json' -d '$data'"
    fi
    
    curl_cmd="$curl_cmd '$endpoint'"
    
    log "Testing: $method $endpoint - $description"
    
    local response
    response=$(eval "$curl_cmd" 2>/dev/null)
    local status_code="${response: -3}"
    local body="${response%???}"
    
    if [[ "$status_code" =~ ^[2-3][0-9][0-9]$ ]]; then
        if [[ "$status_code" == "500" ]]; then
            error "$method $endpoint (Status: $status_code)"
            log "Response body: $body"
        else
            success "$method $endpoint (Status: $status_code)"
        fi
    else
        if [[ "$status_code" == "401" && -z "$auth_header" ]]; then
            success "$method $endpoint (Status: $status_code - Expected unauthorized)"
        elif [[ "$status_code" == "404" ]]; then
            warning "$method $endpoint (Status: $status_code - Not found)"
        else
            error "$method $endpoint (Status: $status_code)"
            log "Response body: $body"
        fi
    fi
    
    # Small delay to prevent rate limiting
    sleep 0.1
}

# Login and get token
login() {
    info "Attempting to login and get JWT token..."
    
    local login_data='{
        "username": "admin@example.com",
        "password": "Admin123!"
    }'
    
    local response
    response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$login_data" \
        "$API_URL/auth/login" 2>/dev/null)
    
    # Extract token from response (assuming JSON structure)
    TOKEN=$(echo "$response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4 | head -1)
    
    if [[ -n "$TOKEN" && ${#TOKEN} -gt 10 ]]; then
        success "Authentication successful - Token acquired"
        info "Token: ${TOKEN:0:20}..."
    else
        error "Authentication failed - Could not acquire token"
        log "Login response: $response"
        exit 1
    fi
}

# Test Health Endpoints
test_health_endpoints() {
    info "🏥 Testing Health Endpoints"
    
    test_endpoint "GET" "$API_BASE/health" "Basic health check" "" "" "200"
    test_endpoint "GET" "$API_BASE/health/detailed" "Detailed health check" "" "" "200"
}

# Test Authentication Endpoints
test_auth_endpoints() {
    info "🔐 Testing Authentication Endpoints"
    
    local login_data='{
        "username": "admin@example.com",
        "password": "Admin123!"
    }'
    
    test_endpoint "POST" "$API_URL/auth/login" "User login" "" "$login_data" "200"
    
    local register_data='{
        "username": "testuser123",
        "email": "testuser123@example.com",
        "password": "TestPass123!",
        "fullName": "Test User",
        "roleId": 3
    }'
    
    test_endpoint "POST" "$API_URL/auth/register" "User registration" "auth" "$register_data" "200"
    test_endpoint "POST" "$API_URL/auth/refresh" "Token refresh" "auth" '"test_refresh_token"' "400"
}

# Test User Endpoints
test_user_endpoints() {
    info "👥 Testing User Management Endpoints"
    
    test_endpoint "GET" "$API_URL/users" "Get all users" "auth" "" "200"
    test_endpoint "GET" "$API_URL/users/advanced" "Get users with advanced filtering" "auth" "" "200"
    
    # Test with dummy GUID for individual user endpoints
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/users/$dummy_guid" "Get user by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/users/username/admin" "Get user by username" "auth" "" "404"
    
    local create_user_data='{
        "username": "newuser123",
        "email": "newuser123@example.com",
        "password": "NewPass123!",
        "fullName": "New Test User",
        "roleId": 3
    }'
    
    test_endpoint "POST" "$API_URL/users" "Create new user" "auth" "$create_user_data" "201"
    
    local update_user_data='{
        "fullName": "Updated Test User",
        "email": "updated@example.com"
    }'
    
    test_endpoint "PUT" "$API_URL/users/$dummy_guid" "Update user" "auth" "$update_user_data" "404"
    test_endpoint "PATCH" "$API_URL/users/$dummy_guid" "Patch user" "auth" "$update_user_data" "404"
    test_endpoint "PATCH" "$API_URL/users/$dummy_guid/activate" "Activate user" "auth" "" "404"
    test_endpoint "PATCH" "$API_URL/users/$dummy_guid/deactivate" "Deactivate user" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/users/$dummy_guid" "Delete user" "auth" "" "404"
}

# Test Project Endpoints
test_project_endpoints() {
    info "📋 Testing Project Management Endpoints"
    
    test_endpoint "GET" "$API_URL/projects" "Get all projects" "auth" "" "200"
    test_endpoint "GET" "$API_URL/projects/legacy" "Get projects (legacy)" "auth" "" "200"
    test_endpoint "GET" "$API_URL/projects/me" "Get my projects" "auth" "" "200"
    test_endpoint "GET" "$API_URL/projects/advanced" "Get projects with advanced filtering" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/projects/$dummy_guid" "Get project by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/projects/$dummy_guid/status" "Get project status" "auth" "" "404"
    
    local create_project_data='{
        "name": "Test Solar Project",
        "description": "Test project for API testing",
        "startDate": "2025-07-01T00:00:00Z",
        "endDate": "2025-12-31T23:59:59Z",
        "location": "Test Location",
        "budget": 150000.00,
        "managerId": "'"$dummy_guid"'"
    }'
    
    test_endpoint "POST" "$API_URL/projects" "Create new project" "auth" "$create_project_data" "201"
    
    local update_project_data='{
        "name": "Updated Test Project",
        "description": "Updated description",
        "budget": 200000.00
    }'
    
    test_endpoint "PUT" "$API_URL/projects/$dummy_guid" "Update project" "auth" "$update_project_data" "404"
    test_endpoint "PATCH" "$API_URL/projects/$dummy_guid" "Patch project" "auth" "$update_project_data" "404"
    test_endpoint "DELETE" "$API_URL/projects/$dummy_guid" "Delete project" "auth" "" "404"
}

# Test Task Endpoints
test_task_endpoints() {
    info "✅ Testing Task Management Endpoints"
    
    test_endpoint "GET" "$API_URL/tasks" "Get all tasks" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/tasks/$dummy_guid" "Get task by ID" "auth" "" "404"
    
    local create_task_data='{
        "title": "Test Task",
        "description": "Test task for API testing",
        "projectId": "'"$dummy_guid"'",
        "assignedToUserId": "'"$dummy_guid"'",
        "status": "Pending",
        "priority": "Medium",
        "estimatedHours": 8,
        "dueDate": "2025-07-15T17:00:00Z"
    }'
    
    test_endpoint "POST" "$API_URL/tasks" "Create new task" "auth" "$create_task_data" "201"
    
    local update_task_data='{
        "title": "Updated Test Task",
        "status": "InProgress"
    }'
    
    test_endpoint "PUT" "$API_URL/tasks/$dummy_guid" "Update task" "auth" "$update_task_data" "404"
    test_endpoint "DELETE" "$API_URL/tasks/$dummy_guid" "Delete task" "auth" "" "404"
}

# Test Daily Report Endpoints
test_daily_report_endpoints() {
    info "📊 Testing Daily Report Endpoints"
    
    test_endpoint "GET" "$API_URL/daily-reports" "Get all daily reports" "auth" "" "200"
    test_endpoint "GET" "$API_URL/daily-reports/weekly-summary" "Get weekly summary" "auth" "" "200"
    test_endpoint "GET" "$API_URL/daily-reports/export" "Export daily reports" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/daily-reports/$dummy_guid" "Get daily report by ID" "auth" "" "404"
    
    local create_report_data='{
        "projectId": "'"$dummy_guid"'",
        "date": "2025-06-19T00:00:00Z",
        "weatherConditions": "Sunny",
        "workPerformed": "Installed solar panels",
        "hoursWorked": 8,
        "issuesEncountered": "None",
        "nextDayPlans": "Continue installation"
    }'
    
    test_endpoint "POST" "$API_URL/daily-reports" "Create daily report" "auth" "$create_report_data" "201"
    
    local update_report_data='{
        "workPerformed": "Updated work description",
        "hoursWorked": 10
    }'
    
    test_endpoint "PUT" "$API_URL/daily-reports/$dummy_guid" "Update daily report" "auth" "$update_report_data" "404"
    test_endpoint "POST" "$API_URL/daily-reports/$dummy_guid/submit" "Submit daily report for approval" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/daily-reports/$dummy_guid" "Delete daily report" "auth" "" "404"
    
    # Test attachment endpoints
    test_endpoint "POST" "$API_URL/daily-reports/$dummy_guid/attachments" "Add attachment" "auth" "" "404"
}

# Test Work Request Endpoints
test_work_request_endpoints() {
    info "🔧 Testing Work Request Endpoints"
    
    test_endpoint "GET" "$API_URL/work-requests" "Get all work requests" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/work-requests/$dummy_guid" "Get work request by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/work-requests/$dummy_guid/approval-status" "Get approval status" "auth" "" "404"
    
    local create_request_data='{
        "projectId": "'"$dummy_guid"'",
        "title": "Test Work Request",
        "description": "Test work request for API testing",
        "requestType": "AdditionalWork",
        "priority": "Medium",
        "estimatedCost": 5000.00,
        "requestedDate": "2025-07-01T00:00:00Z"
    }'
    
    test_endpoint "POST" "$API_URL/work-requests" "Create work request" "auth" "$create_request_data" "201"
    
    local update_request_data='{
        "title": "Updated Work Request",
        "priority": "High"
    }'
    
    test_endpoint "PUT" "$API_URL/work-requests/$dummy_guid" "Update work request" "auth" "$update_request_data" "404"
    test_endpoint "POST" "$API_URL/work-requests/$dummy_guid/assign/$dummy_guid" "Assign work request" "auth" "" "404"
    test_endpoint "POST" "$API_URL/work-requests/$dummy_guid/complete" "Complete work request" "auth" "" "404"
    test_endpoint "POST" "$API_URL/work-requests/$dummy_guid/submit-for-approval" "Submit for approval" "auth" '{"notes":"Test submission"}' "404"
    test_endpoint "POST" "$API_URL/work-requests/$dummy_guid/process-approval" "Process approval" "auth" '{"approved":true,"notes":"Approved"}' "404"
    test_endpoint "DELETE" "$API_URL/work-requests/$dummy_guid" "Delete work request" "auth" "" "404"
}

# Test Calendar Endpoints
test_calendar_endpoints() {
    info "📅 Testing Calendar Endpoints"
    
    test_endpoint "GET" "$API_URL/calendar" "Get all calendar events" "auth" "" "200"
    test_endpoint "GET" "$API_URL/calendar/upcoming" "Get upcoming events" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/calendar/$dummy_guid" "Get calendar event by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/calendar/project/$dummy_guid" "Get events by project" "auth" "" "404"
    
    local create_event_data='{
        "title": "Test Calendar Event",
        "description": "Test event for API testing",
        "startTime": "2025-07-01T09:00:00Z",
        "endTime": "2025-07-01T17:00:00Z",
        "projectId": "'"$dummy_guid"'",
        "location": "Test Site"
    }'
    
    test_endpoint "POST" "$API_URL/calendar" "Create calendar event" "auth" "$create_event_data" "201"
    
    local update_event_data='{
        "title": "Updated Calendar Event",
        "description": "Updated description"
    }'
    
    test_endpoint "PUT" "$API_URL/calendar/$dummy_guid" "Update calendar event" "auth" "$update_event_data" "404"
    test_endpoint "DELETE" "$API_URL/calendar/$dummy_guid" "Delete calendar event" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/calendar/recurring/$dummy_guid" "Delete recurring event" "auth" "" "404"
}

# Test Master Plan Endpoints
test_master_plan_endpoints() {
    info "🗺️ Testing Master Plan Endpoints"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/master-plans/$dummy_guid" "Get master plan by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/master-plans/project/$dummy_guid" "Get master plan by project" "auth" "" "404"
    test_endpoint "GET" "$API_URL/master-plans/$dummy_guid/progress" "Get progress summary" "auth" "" "404"
    test_endpoint "GET" "$API_URL/master-plans/$dummy_guid/completion" "Get overall progress" "auth" "" "404"
    test_endpoint "GET" "$API_URL/master-plans/$dummy_guid/phases" "Get phases" "auth" "" "404"
    
    local create_master_plan_data='{
        "projectId": "'"$dummy_guid"'",
        "name": "Test Master Plan",
        "description": "Test master plan for API testing",
        "startDate": "2025-07-01T00:00:00Z",
        "endDate": "2025-12-31T23:59:59Z",
        "totalBudget": 200000.00
    }'
    
    test_endpoint "POST" "$API_URL/master-plans" "Create master plan" "auth" "$create_master_plan_data" "201"
    
    local update_master_plan_data='{
        "name": "Updated Master Plan",
        "totalBudget": 250000.00
    }'
    
    test_endpoint "PUT" "$API_URL/master-plans/$dummy_guid" "Update master plan" "auth" "$update_master_plan_data" "404"
    test_endpoint "POST" "$API_URL/master-plans/$dummy_guid/approve" "Approve master plan" "auth" '{"notes":"Approved"}' "404"
    test_endpoint "POST" "$API_URL/master-plans/$dummy_guid/activate" "Activate master plan" "auth" "" "404"
    
    local create_phase_data='{
        "name": "Test Phase",
        "description": "Test phase for API testing",
        "startDate": "2025-07-01T00:00:00Z",
        "endDate": "2025-08-31T23:59:59Z",
        "budget": 50000.00
    }'
    
    test_endpoint "POST" "$API_URL/master-plans/$dummy_guid/phases" "Add phase to master plan" "auth" "$create_phase_data" "404"
}

# Test Phase Endpoints
test_phase_endpoints() {
    info "🏗️ Testing Phase Endpoints"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/phases/$dummy_guid" "Get phase by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/phases/$dummy_guid/tasks" "Get phase tasks" "auth" "" "404"
    
    local update_phase_data='{
        "name": "Updated Phase",
        "description": "Updated description"
    }'
    
    test_endpoint "PATCH" "$API_URL/phases/$dummy_guid" "Update phase" "auth" "$update_phase_data" "404"
    test_endpoint "DELETE" "$API_URL/phases/$dummy_guid" "Delete phase" "auth" "" "404"
}

# Test Document Endpoints
test_document_endpoints() {
    info "📄 Testing Document Endpoints"
    
    test_endpoint "GET" "$API_URL/documents" "Get all documents" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/documents/$dummy_guid" "Get document by ID" "auth" "" "404"
    
    local create_document_data='{
        "title": "Test Document",
        "description": "Test document for API testing",
        "documentType": "Report",
        "projectId": "'"$dummy_guid"'",
        "filePath": "/test/path/document.pdf"
    }'
    
    test_endpoint "POST" "$API_URL/documents" "Create document" "auth" "$create_document_data" "201"
    
    local update_document_data='{
        "title": "Updated Document",
        "description": "Updated description"
    }'
    
    test_endpoint "PATCH" "$API_URL/documents/$dummy_guid" "Update document" "auth" "$update_document_data" "404"
    test_endpoint "DELETE" "$API_URL/documents/$dummy_guid" "Delete document" "auth" "" "404"
}

# Test Resource Endpoints
test_resource_endpoints() {
    info "🔧 Testing Resource Endpoints"
    
    test_endpoint "GET" "$API_URL/resources" "Get all resources" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/resources/$dummy_guid" "Get resource by ID" "auth" "" "404"
    
    local create_resource_data='{
        "name": "Test Resource",
        "description": "Test resource for API testing",
        "resourceType": "Equipment",
        "availabilityStatus": "Available",
        "costPerHour": 50.00
    }'
    
    test_endpoint "POST" "$API_URL/resources" "Create resource" "auth" "$create_resource_data" "201"
    
    local update_resource_data='{
        "name": "Updated Resource",
        "costPerHour": 75.00
    }'
    
    test_endpoint "PATCH" "$API_URL/resources/$dummy_guid" "Update resource" "auth" "$update_resource_data" "404"
    test_endpoint "DELETE" "$API_URL/resources/$dummy_guid" "Delete resource" "auth" "" "404"
}

# Test Image Endpoints
test_image_endpoints() {
    info "🖼️ Testing Image Endpoints"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/images/$dummy_guid" "Get image metadata" "auth" "" "404"
    test_endpoint "GET" "$API_URL/images/$dummy_guid/url" "Get image URL" "auth" "" "404"
    test_endpoint "GET" "$API_URL/images/project/$dummy_guid" "Get project images" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/images/$dummy_guid" "Delete image" "auth" "" "404"
    
    # Note: File upload testing would require actual file handling
    info "Skipping image upload tests (requires multipart/form-data)"
}

# Test Weekly Report Endpoints
test_weekly_report_endpoints() {
    info "📈 Testing Weekly Report Endpoints"
    
    test_endpoint "GET" "$API_URL/weekly-reports" "Get all weekly reports" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/weekly-reports/$dummy_guid" "Get weekly report by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/projects/$dummy_guid/weekly-reports" "Get project weekly reports" "auth" "" "404"
    
    local create_weekly_report_data='{
        "projectId": "'"$dummy_guid"'",
        "weekStartDate": "2025-06-16T00:00:00Z",
        "weekEndDate": "2025-06-22T23:59:59Z",
        "summary": "Test weekly summary",
        "accomplishments": "Test accomplishments",
        "issues": "No issues",
        "nextWeekPlans": "Continue work"
    }'
    
    test_endpoint "POST" "$API_URL/weekly-reports" "Create weekly report" "auth" "$create_weekly_report_data" "201"
    
    local update_weekly_report_data='{
        "summary": "Updated weekly summary"
    }'
    
    test_endpoint "PUT" "$API_URL/weekly-reports/$dummy_guid" "Update weekly report" "auth" "$update_weekly_report_data" "404"
    test_endpoint "POST" "$API_URL/weekly-reports/$dummy_guid/submit" "Submit weekly report" "auth" "" "404"
    test_endpoint "POST" "$API_URL/weekly-reports/$dummy_guid/approve" "Approve weekly report" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/weekly-reports/$dummy_guid" "Delete weekly report" "auth" "" "404"
}

# Test Weekly Work Request Endpoints
test_weekly_work_request_endpoints() {
    info "📋 Testing Weekly Work Request Endpoints"
    
    test_endpoint "GET" "$API_URL/weekly-requests" "Get all weekly work requests" "auth" "" "200"
    
    local dummy_guid="12345678-1234-1234-1234-123456789012"
    test_endpoint "GET" "$API_URL/weekly-requests/$dummy_guid" "Get weekly work request by ID" "auth" "" "404"
    test_endpoint "GET" "$API_URL/projects/$dummy_guid/weekly-requests" "Get project weekly work requests" "auth" "" "404"
    
    local create_weekly_request_data='{
        "projectId": "'"$dummy_guid"'",
        "weekStartDate": "2025-06-16T00:00:00Z",
        "title": "Test Weekly Work Request",
        "description": "Test weekly work request",
        "priority": "Medium",
        "estimatedHours": 40
    }'
    
    test_endpoint "POST" "$API_URL/weekly-requests" "Create weekly work request" "auth" "$create_weekly_request_data" "201"
    
    local update_weekly_request_data='{
        "title": "Updated Weekly Work Request"
    }'
    
    test_endpoint "PUT" "$API_URL/weekly-requests/$dummy_guid" "Update weekly work request" "auth" "$update_weekly_request_data" "404"
    test_endpoint "POST" "$API_URL/weekly-requests/$dummy_guid/submit" "Submit weekly work request" "auth" "" "404"
    test_endpoint "POST" "$API_URL/weekly-requests/$dummy_guid/approve" "Approve weekly work request" "auth" "" "404"
    test_endpoint "DELETE" "$API_URL/weekly-requests/$dummy_guid" "Delete weekly work request" "auth" "" "404"
}

# Test Debug Endpoints
test_debug_endpoints() {
    info "🐛 Testing Debug Endpoints"
    
    test_endpoint "GET" "$API_URL/debug/info" "Get debug info" "" "" "200"
    test_endpoint "GET" "$API_URL/debug/status" "Get debug status" "" "" "200"
}

# Main test execution
main() {
    echo "======================================"
    echo "🚀 COMPREHENSIVE API ENDPOINT TESTING"
    echo "======================================"
    echo ""
    
    log "Starting comprehensive API tests..."
    log "API Base URL: $API_BASE"
    log "API Version: $API_VERSION"
    log "Log file: $LOG_FILE"
    echo ""
    
    # Check if API is running
    info "Checking if API is running..."
    if ! curl -s "$API_BASE/health" > /dev/null; then
        error "API is not running at $API_BASE"
        echo "Please start the API with: dotnet run --urls \"http://localhost:5002\""
        exit 1
    fi
    success "API is running"
    echo ""
    
    # Authenticate first
    login
    echo ""
    
    # Run all test suites
    test_health_endpoints
    echo ""
    
    test_auth_endpoints
    echo ""
    
    test_user_endpoints
    echo ""
    
    test_project_endpoints
    echo ""
    
    test_task_endpoints
    echo ""
    
    test_daily_report_endpoints
    echo ""
    
    test_work_request_endpoints
    echo ""
    
    test_calendar_endpoints
    echo ""
    
    test_master_plan_endpoints
    echo ""
    
    test_phase_endpoints
    echo ""
    
    test_document_endpoints
    echo ""
    
    test_resource_endpoints
    echo ""
    
    test_image_endpoints
    echo ""
    
    test_weekly_report_endpoints
    echo ""
    
    test_weekly_work_request_endpoints
    echo ""
    
    test_debug_endpoints
    echo ""
    
    # Print summary
    echo "======================================"
    echo "📊 TEST SUMMARY"
    echo "======================================"
    echo ""
    log "Total tests run: $TOTAL_TESTS"
    log "Tests passed: $PASSED_TESTS"
    log "Tests failed: $FAILED_TESTS"
    log "Success rate: $(( (PASSED_TESTS * 100) / TOTAL_TESTS ))%"
    echo ""
    
    if [[ $FAILED_TESTS -gt 0 ]]; then
        echo "❌ ENDPOINTS WITH ISSUES (500 errors):"
        for endpoint in "${FAILED_ENDPOINTS[@]}"; do
            echo "   ❌ $endpoint"
        done
        echo ""
    fi
    
    if [[ $PASSED_TESTS -gt 0 ]]; then
        echo "✅ WORKING ENDPOINTS:"
        for endpoint in "${PASSED_ENDPOINTS[@]}"; do
            echo "   ✅ $endpoint"
        done
        echo ""
    fi
    
    log "Test results saved to: $LOG_FILE"
    echo ""
    
    if [[ $FAILED_TESTS -gt 0 ]]; then
        exit 1
    else
        success "All tests completed successfully!"
        exit 0
    fi
}

# Run the main function
main "$@"
