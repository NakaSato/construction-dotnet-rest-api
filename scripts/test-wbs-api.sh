#!/bin/bash

# WBS API Test Script
# This script tests all endpoints of the Work Breakdown Structure API
# 
# Usage: ./test-wbs-api.sh [BASE_URL] [JWT_TOKEN]
# Example: ./test-wbs-api.sh "http://localhost:5001" "your-jwt-token-here"
#
# Note: Most WBS endpoints require authentication. For comprehensive testing,
# provide a valid JWT token. Without authentication, only health checks
# and error handling tests will pass.
#
# To get a JWT token:
# 1. Register a user account via /api/v1/auth/register
# 2. Login via /api/v1/auth/login to get a JWT token
# 3. Use that token with this script

set -e  # Exit on any error

# Configuration
BASE_URL="${1:-http://localhost:5001}"
JWT_TOKEN="${2:-}"
API_BASE="$BASE_URL/api/v1"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counter
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Sample project ID (using existing project)
PROJECT_ID="1c91433d-fc43-40d9-a49b-e9f2d2a9e922"

# Test account credentials
TEST_ADMIN_USERNAME="admin@example.com"
TEST_ADMIN_PASSWORD="Admin123!"
TEST_MANAGER_USERNAME="test_manager"
TEST_MANAGER_PASSWORD="Manager123!"
TEST_USER_USERNAME="test_user"
TEST_USER_PASSWORD="User123!"

# Function to authenticate and get JWT token
authenticate_user() {
    local username=$1
    local password=$2
    
    local login_data='{
        "username": "'$username'",
        "password": "'$password'"
    }'
    
    local response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$login_data" \
        "$BASE_URL/api/v1/auth/login")
    
    local http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    local response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    if [ "$http_code" = "200" ]; then
        echo "$response_body" | jq -r '.data.token // .token // empty'
    else
        echo ""
    fi
}

# Function to print colored output
print_status() {
    local status=$1
    local message=$2
    case $status in
        "INFO")
            echo -e "${BLUE}[INFO]${NC} $message"
            ;;
        "SUCCESS")
            echo -e "${GREEN}[SUCCESS]${NC} $message"
            PASSED_TESTS=$((PASSED_TESTS + 1))
            ;;
        "ERROR")
            echo -e "${RED}[ERROR]${NC} $message"
            FAILED_TESTS=$((FAILED_TESTS + 1))
            ;;
        "WARNING")
            echo -e "${YELLOW}[WARNING]${NC} $message"
            ;;
    esac
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
}

# Function to make HTTP requests
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local expected_status=${4:-200}
    
    local response
    
    if [ -n "$JWT_TOKEN" ] && [ -n "$data" ]; then
        # Auth + Data
        response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X "$method" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_BASE$endpoint")
    elif [ -n "$JWT_TOKEN" ]; then
        # Auth only
        response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X "$method" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            "$API_BASE$endpoint")
    elif [ -n "$data" ]; then
        # Data only
        response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_BASE$endpoint")
    else
        # Neither
        response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X "$method" \
            "$API_BASE$endpoint")
    fi
    
    # Extract HTTP status code from the response
    local http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    # Extract response body by removing the status line
    local response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    echo "$response_body"
    
    # Check if status code matches expected
    if [ "$http_code" = "$expected_status" ]; then
        return 0
    else
        echo "Expected status $expected_status, got $http_code" >&2
        return 1
    fi
}

# Function to validate JSON response structure
validate_json_response() {
    local response=$1
    local expected_success=${2:-true}
    
    # Check if response is valid JSON and has required fields
    if echo "$response" | jq -e '.success' > /dev/null 2>&1; then
        success_value=$(echo "$response" | jq -r '.success')
        if [ "$success_value" = "$expected_success" ]; then
            return 0
        else
            echo "Expected success: $expected_success, got: $success_value" >&2
            return 1
        fi
    else
        echo "Invalid JSON response or missing 'success' field" >&2
        return 1
    fi
}

# Function to extract data from JSON response
extract_data() {
    local response=$1
    local path=$2
    echo "$response" | jq -r "$path"
}

print_header() {
    echo ""
    echo "=========================================="
    echo "$1"
    echo "=========================================="
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check if jq is installed
    if ! command -v jq &> /dev/null; then
        print_status "ERROR" "jq is required but not installed. Please install jq to parse JSON responses."
        exit 1
    fi
    
    # Check if curl is installed
    if ! command -v curl &> /dev/null; then
        print_status "ERROR" "curl is required but not installed."
        exit 1
    fi
    
    print_status "INFO" "Using API Base URL: $API_BASE"
    
    # Try to authenticate with test admin account if no token provided
    if [ -z "$JWT_TOKEN" ]; then
        print_status "INFO" "No JWT token provided."
        print_status "INFO" "Note: In-memory database may not have test accounts pre-seeded."
        print_status "INFO" "For full functionality testing, please provide a valid JWT token."
        print_status "INFO" "Usage: $0 [BASE_URL] [JWT_TOKEN]"
        print_status "WARNING" "Running tests without authentication - most will return 401 Unauthorized."
    else
        print_status "INFO" "JWT token provided. Will test authenticated endpoints."
    fi
}

# Test 1: Health Check
test_health_check() {
    print_header "Test 1: API Health Check"
    
    if response=$(curl -s -w 'HTTPSTATUS:%{http_code}' "$BASE_URL/Health"); then
        http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
        response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
        
        if [ "$http_code" = "200" ]; then
            print_status "SUCCESS" "API is responding"
            echo "Response: $response_body"
        else
            print_status "ERROR" "API health check failed with status $http_code"
            echo "Response: $response_body"
            return 1
        fi
    else
        print_status "ERROR" "API health check failed"
        return 1
    fi
}

# Test 2: Get All WBS Tasks (No Auth Required for testing)
test_get_all_wbs_tasks() {
    print_header "Test 2: Get All WBS Tasks"
    
    if response=$(make_request "GET" "/wbs" "" 200); then
        if validate_json_response "$response"; then
            task_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved WBS tasks successfully. Count: $task_count"
        else
            print_status "ERROR" "Invalid response format"
            echo "Response: $response"
        fi
    else
        print_status "ERROR" "Failed to get WBS tasks"
        echo "Response: $response"
    fi
}

# Test 3: Get WBS Tasks with Filters
test_get_wbs_tasks_with_filters() {
    print_header "Test 3: Get WBS Tasks with Filters"
    
    # Test filter by project ID
    if response=$(make_request "GET" "/wbs?projectId=$PROJECT_ID" "" 200); then
        if validate_json_response "$response"; then
            task_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved WBS tasks by project ID. Count: $task_count"
        else
            print_status "ERROR" "Invalid response format for project filter"
        fi
    else
        print_status "ERROR" "Failed to get WBS tasks with project filter"
    fi
    
    # Test filter by status
    if response=$(make_request "GET" "/wbs?status=1" "" 200); then
        if validate_json_response "$response"; then
            task_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved WBS tasks by status. Count: $task_count"
        else
            print_status "ERROR" "Invalid response format for status filter"
        fi
    else
        print_status "ERROR" "Failed to get WBS tasks with status filter"
    fi
    
    # Test filter by installation area
    if response=$(make_request "GET" "/wbs?installationArea=Inverter%20Room" "" 200); then
        if validate_json_response "$response"; then
            task_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved WBS tasks by installation area. Count: $task_count"
        else
            print_status "ERROR" "Invalid response format for installation area filter"
        fi
    else
        print_status "ERROR" "Failed to get WBS tasks with installation area filter"
    fi
}

# Test 4: Create New WBS Task
test_create_wbs_task() {
    print_header "Test 4: Create New WBS Task"
    
    local test_wbs_id="TEST.$(date +%s)"  # Unique test ID
    local create_data='{
        "wbsId": "'$test_wbs_id'",
        "parentWbsId": null,
        "taskNameEN": "Test Task",
        "taskNameTH": "งานทดสอบ",
        "description": "This is a test task created by the API test script",
        "status": 0,
        "weightPercent": 5.0,
        "installationArea": "General",
        "acceptanceCriteria": "Task should be created successfully",
        "plannedStartDate": "2025-07-10T08:00:00Z",
        "plannedEndDate": "2025-07-10T17:00:00Z",
        "projectId": "'$PROJECT_ID'",
        "dependencies": []
    }'
    
    if response=$(make_request "POST" "/wbs" "$create_data" 201); then
        if validate_json_response "$response"; then
            created_wbs_id=$(extract_data "$response" '.data.wbsId')
            print_status "SUCCESS" "Created WBS task with ID: $created_wbs_id"
            CREATED_WBS_ID="$created_wbs_id"  # Store for later tests
        else
            print_status "ERROR" "Invalid response format for task creation"
        fi
    else
        print_status "ERROR" "Failed to create WBS task"
        echo "Response: $response"
    fi
}

# Test 5: Get Specific WBS Task
test_get_specific_wbs_task() {
    print_header "Test 5: Get Specific WBS Task"
    
    # Use created task ID if available, otherwise use a known ID
    local wbs_id="${CREATED_WBS_ID:-4.1.1.1}"
    
    if response=$(make_request "GET" "/wbs/$wbs_id" "" 200); then
        if validate_json_response "$response"; then
            retrieved_wbs_id=$(extract_data "$response" '.data.wbsId')
            task_name=$(extract_data "$response" '.data.taskNameEN')
            print_status "SUCCESS" "Retrieved WBS task: $retrieved_wbs_id - $task_name"
        else
            print_status "ERROR" "Invalid response format for specific task retrieval"
        fi
    else
        print_status "ERROR" "Failed to get specific WBS task: $wbs_id"
        echo "Response: $response"
    fi
}

# Test 6: Update WBS Task
test_update_wbs_task() {
    print_header "Test 6: Update WBS Task"
    
    if [ -z "$CREATED_WBS_ID" ]; then
        print_status "WARNING" "No created task ID available, skipping update test"
        return
    fi
    
    local update_data='{
        "taskNameEN": "Updated Test Task",
        "taskNameTH": "งานทดสอบที่อัปเดต",
        "description": "This task has been updated by the API test script",
        "status": 1,
        "weightPercent": 7.5,
        "acceptanceCriteria": "Task should be updated successfully",
        "actualStartDate": "2025-07-10T09:00:00Z"
    }'
    
    if response=$(make_request "PUT" "/wbs/$CREATED_WBS_ID" "$update_data" 200); then
        if validate_json_response "$response"; then
            updated_task_name=$(extract_data "$response" '.data.taskNameEN')
            updated_status=$(extract_data "$response" '.data.status')
            print_status "SUCCESS" "Updated WBS task: $updated_task_name (Status: $updated_status)"
        else
            print_status "ERROR" "Invalid response format for task update"
        fi
    else
        print_status "ERROR" "Failed to update WBS task: $CREATED_WBS_ID"
        echo "Response: $response"
    fi
}

# Test 7: Get Project Progress
test_get_project_progress() {
    print_header "Test 7: Get Project Progress"
    
    if response=$(make_request "GET" "/wbs/progress/$PROJECT_ID" "" 200); then
        if validate_json_response "$response"; then
            total_tasks=$(extract_data "$response" '.data.totalTasks')
            completed_tasks=$(extract_data "$response" '.data.completedTasks')
            progress_percentage=$(extract_data "$response" '.data.progressPercentage')
            print_status "SUCCESS" "Project progress: $completed_tasks/$total_tasks tasks completed ($progress_percentage%)"
        else
            print_status "ERROR" "Invalid response format for project progress"
        fi
    else
        print_status "ERROR" "Failed to get project progress"
        echo "Response: $response"
    fi
}

# Test 8: Get Task Hierarchy
test_get_task_hierarchy() {
    print_header "Test 8: Get Task Hierarchy"
    
    if response=$(make_request "GET" "/wbs/hierarchy/$PROJECT_ID" "" 200); then
        if validate_json_response "$response"; then
            hierarchy_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved task hierarchy with $hierarchy_count root tasks"
        else
            print_status "ERROR" "Invalid response format for task hierarchy"
        fi
    else
        print_status "ERROR" "Failed to get task hierarchy"
        echo "Response: $response"
    fi
}

# Test 9: Get Ready to Start Tasks
test_get_ready_to_start_tasks() {
    print_header "Test 9: Get Ready to Start Tasks"
    
    if response=$(make_request "GET" "/wbs/ready-to-start/$PROJECT_ID" "" 200); then
        if validate_json_response "$response"; then
            ready_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Found $ready_count tasks ready to start"
        else
            print_status "ERROR" "Invalid response format for ready to start tasks"
        fi
    else
        print_status "ERROR" "Failed to get ready to start tasks"
        echo "Response: $response"
    fi
}

# Test 10: Update Task Status
test_update_task_status() {
    print_header "Test 10: Update Task Status"
    
    if [ -z "$CREATED_WBS_ID" ]; then
        print_status "WARNING" "No created task ID available, skipping status update test"
        return
    fi
    
    local status_data='{"status": 2, "actualEndDate": "2025-07-10T17:00:00Z"}'
    
    if response=$(make_request "PATCH" "/wbs/$CREATED_WBS_ID/status" "$status_data" 200); then
        if validate_json_response "$response"; then
            updated_status=$(extract_data "$response" '.data.status')
            actual_end_date=$(extract_data "$response" '.data.actualEndDate')
            print_status "SUCCESS" "Updated task status to: $updated_status (Ended: $actual_end_date)"
        else
            print_status "ERROR" "Invalid response format for status update"
        fi
    else
        print_status "ERROR" "Failed to update task status"
        echo "Response: $response"
    fi
}

# Test 11: Add Task Evidence
test_add_task_evidence() {
    print_header "Test 11: Add Task Evidence"
    
    if [ -z "$CREATED_WBS_ID" ]; then
        print_status "WARNING" "No created task ID available, skipping evidence test"
        return
    fi
    
    local evidence_data='{
        "evidenceType": "Document",
        "fileName": "test-completion-report.pdf",
        "filePath": "/uploads/evidence/test-completion-report.pdf",
        "description": "Test completion report generated by API test script",
        "uploadedByUserId": "'$SAMPLE_USER_ID'"
    }'
    
    if response=$(make_request "POST" "/wbs/$CREATED_WBS_ID/evidence" "$evidence_data" 201); then
        if validate_json_response "$response"; then
            evidence_id=$(extract_data "$response" '.data.id')
            evidence_type=$(extract_data "$response" '.data.evidenceType')
            print_status "SUCCESS" "Added evidence: $evidence_type (ID: $evidence_id)"
        else
            print_status "ERROR" "Invalid response format for evidence addition"
        fi
    else
        print_status "ERROR" "Failed to add task evidence"
        echo "Response: $response"
    fi
}

# Test 12: Get Task Evidence
test_get_task_evidence() {
    print_header "Test 12: Get Task Evidence"
    
    if [ -z "$CREATED_WBS_ID" ]; then
        print_status "WARNING" "No created task ID available, skipping evidence retrieval test"
        return
    fi
    
    if response=$(make_request "GET" "/wbs/$CREATED_WBS_ID/evidence" "" 200); then
        if validate_json_response "$response"; then
            evidence_count=$(extract_data "$response" '.data | length')
            print_status "SUCCESS" "Retrieved $evidence_count evidence items for task"
        else
            print_status "ERROR" "Invalid response format for evidence retrieval"
        fi
    else
        print_status "ERROR" "Failed to get task evidence"
        echo "Response: $response"
    fi
}

# Test 13: Seed Sample Data
test_seed_sample_data() {
    print_header "Test 13: Seed Sample Data"
    
    if response=$(make_request "POST" "/wbs/seed-data/$PROJECT_ID" "" 201); then
        if validate_json_response "$response"; then
            seeded_count=$(extract_data "$response" '.data.seededTasksCount // .data | length // 0')
            print_status "SUCCESS" "Seeded sample data: $seeded_count tasks"
        else
            print_status "ERROR" "Invalid response format for data seeding"
        fi
    else
        print_status "ERROR" "Failed to seed sample data (may require Admin role)"
        echo "Response: $response"
    fi
}

# Test 14: Delete WBS Task (Cleanup)
test_delete_wbs_task() {
    print_header "Test 14: Delete WBS Task (Cleanup)"
    
    if [ -z "$CREATED_WBS_ID" ]; then
        print_status "WARNING" "No created task ID available, skipping delete test"
        return
    fi
    
    if response=$(make_request "DELETE" "/wbs/$CREATED_WBS_ID" "" 200); then
        if validate_json_response "$response"; then
            print_status "SUCCESS" "Deleted test WBS task: $CREATED_WBS_ID"
        else
            print_status "ERROR" "Invalid response format for task deletion"
        fi
    else
        print_status "ERROR" "Failed to delete WBS task (may require Admin role)"
        echo "Response: $response"
    fi
}

# Test 15: Error Handling Tests
test_error_handling() {
    print_header "Test 15: Error Handling"
    
    # Test 404 - Non-existent task
    if response=$(make_request "GET" "/wbs/NON_EXISTENT_TASK" "" 404); then
        if validate_json_response "$response" "false"; then
            print_status "SUCCESS" "404 error handling works correctly"
        else
            print_status "ERROR" "Invalid error response format"
        fi
    else
        print_status "ERROR" "404 error handling failed"
    fi
    
    # Test 400 - Invalid data format
    local invalid_data='{"invalid": "data", "wbsId": ""}'
    if response=$(make_request "POST" "/wbs" "$invalid_data" 400); then
        if validate_json_response "$response" "false"; then
            print_status "SUCCESS" "400 error handling works correctly"
        else
            print_status "ERROR" "Invalid validation error response format"
        fi
    else
        print_status "ERROR" "400 error handling failed"
    fi
}

# Test 16: Role-Based Access Control
test_role_based_access() {
    print_header "Test 16: Role-Based Access Control"
    
    if [ -z "$JWT_TOKEN" ]; then
        print_status "INFO" "No JWT token available for RBAC testing"
        print_status "INFO" "RBAC tests require authenticated access"
        return
    fi
    
    print_status "INFO" "Testing current token access level..."
    
    # Test read access with current token
    local response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X GET \
        -H "Authorization: Bearer $JWT_TOKEN" \
        "$API_BASE/wbs")
    
    local http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    
    if [ "$http_code" = "200" ]; then
        print_status "SUCCESS" "Current token can read WBS tasks"
    else
        print_status "ERROR" "Current token cannot read WBS tasks (Status: $http_code)"
    fi
    
    # Test admin-only operations
    local delete_response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X DELETE \
        -H "Authorization: Bearer $JWT_TOKEN" \
        "$API_BASE/wbs/TEST_RBAC_NON_EXISTENT")
    
    local delete_code=$(echo "$delete_response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    
    if [ "$delete_code" = "403" ]; then
        print_status "SUCCESS" "Token correctly denied DELETE access (insufficient permissions)"
    elif [ "$delete_code" = "404" ]; then
        print_status "SUCCESS" "Token has DELETE access (got 404 for non-existent task)"
    elif [ "$delete_code" = "401" ]; then
        print_status "ERROR" "Token authentication failed for DELETE operation"
    else
        print_status "INFO" "DELETE access test unclear (Status: $delete_code)"
    fi
    
    # Test POST (create) access
    local test_create_data='{
        "wbsId": "RBAC.TEST.$(date +%s)",
        "taskNameEN": "RBAC Test Task",
        "taskNameTH": "งานทดสอบ RBAC",
        "description": "Test task for RBAC validation",
        "status": "NotStarted",
        "weightPercent": 1.0,
        "projectId": "'$PROJECT_ID'",
        "dependencies": []
    }'
    
    local create_response=$(curl -s -w 'HTTPSTATUS:%{http_code}' -X POST \
        -H "Authorization: Bearer $JWT_TOKEN" \
        -H "Content-Type: application/json" \
        -d "$test_create_data" \
        "$API_BASE/wbs")
    
    local create_code=$(echo "$create_response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    
    if [ "$create_code" = "201" ]; then
        print_status "SUCCESS" "Token has CREATE access to WBS tasks"
    elif [ "$create_code" = "403" ]; then
        print_status "SUCCESS" "Token correctly denied CREATE access"
    elif [ "$create_code" = "401" ]; then
        print_status "ERROR" "Token authentication failed for CREATE operation"
    else
        print_status "INFO" "CREATE access test unclear (Status: $create_code)"
    fi
    
    print_status "INFO" "For comprehensive RBAC testing, test with different role tokens"
}

# Main execution
main() {
    echo "=========================================="
    echo "WBS API Test Suite"
    echo "=========================================="
    echo "Testing API at: $BASE_URL"
    echo "Timestamp: $(date)"
    echo ""
    
    check_prerequisites
    
    # Run all tests
    test_health_check
    test_get_all_wbs_tasks
    test_get_wbs_tasks_with_filters
    test_create_wbs_task
    test_get_specific_wbs_task
    test_update_wbs_task
    test_get_project_progress
    test_get_task_hierarchy
    test_get_ready_to_start_tasks
    test_update_task_status
    test_add_task_evidence
    test_get_task_evidence
    test_seed_sample_data
    test_delete_wbs_task
    test_error_handling
    test_role_based_access
    
    # Print summary
    print_header "Test Summary"
    echo "Total Tests: $TOTAL_TESTS"
    echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "\n${GREEN}All tests passed! 🎉${NC}"
        exit 0
    else
        echo -e "\n${RED}Some tests failed. Please review the output above.${NC}"
        exit 1
    fi
}

# Run main function
main "$@"
