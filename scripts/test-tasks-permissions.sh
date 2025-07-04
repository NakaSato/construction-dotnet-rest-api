#!/bin/bash

# =============================================================================
# 🔐 TASKS API ROLE-BASED PERMISSIONS TESTING SCRIPT
# =============================================================================
# Tests specific permission scenarios for different user roles
# Validates access control according to the API documentation
# =============================================================================

set -e

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/tasks_permissions_$TIMESTAMP.log"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m'

# Create log directory
mkdir -p "$LOG_DIR"

# Global variables
ADMIN_TOKEN=""
MANAGER_TOKEN=""
SUPERVISOR_TOKEN=""
TECHNICIAN_TOKEN=""
PROJECT_ID=""
TASK_ID=""
TEST_RESULTS=()

echo -e "${BLUE}=================================================================${NC}"
echo -e "${BLUE}🔐 TASKS API ROLE-BASED PERMISSIONS TEST${NC}"
echo -e "${BLUE}=================================================================${NC}"
echo "Testing role-based access control for Tasks API"
echo "Log File: $LOG_FILE"
echo ""

# Utility functions
log_result() {
    local test_name="$1"
    local role="$2"
    local expected="$3"
    local actual="$4"
    local details="$5"
    
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local result=""
    
    if [ "$expected" = "$actual" ]; then
        result="PASS"
        echo -e "${GREEN}✓ PASS${NC} - $test_name ($role)"
    else
        result="FAIL"
        echo -e "${RED}✗ FAIL${NC} - $test_name ($role) - Expected: $expected, Got: $actual"
    fi
    
    TEST_RESULTS+=("$result|$test_name|$role|$expected|$actual|$details")
    echo "[$timestamp] [$result] $test_name - Role: $role - Expected: $expected - Actual: $actual - Details: $details" >> "$LOG_FILE"
}

print_section() {
    echo ""
    echo -e "${CYAN}--- $1 ---${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Function to make API calls and return just the HTTP status
api_call_status() {
    local method="$1"
    local endpoint="$2"
    local token="$3"
    local data="$4"
    
    local curl_cmd="curl -s -w '%{http_code}' -o /dev/null"
    
    if [ -n "$token" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $token'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ -n "$data" ] && [ "$method" != "GET" ] && [ "$method" != "DELETE" ]; then
        curl_cmd="$curl_cmd -d '$data'"
    fi
    
    curl_cmd="$curl_cmd -X $method '$BASE_URL$endpoint'"
    
    eval $curl_cmd
}

# Function to make API calls and return response with status
api_call_full() {
    local method="$1"
    local endpoint="$2"
    local token="$3"
    local data="$4"
    
    local curl_cmd="curl -s -w 'HTTPSTATUS:%{http_code}'"
    
    if [ -n "$token" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $token'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ -n "$data" ] && [ "$method" != "GET" ] && [ "$method" != "DELETE" ]; then
        curl_cmd="$curl_cmd -d '$data'"
    fi
    
    curl_cmd="$curl_cmd -X $method '$BASE_URL$endpoint'"
    
    eval $curl_cmd
}

# Authentication function
authenticate_users() {
    print_section "🔑 User Authentication"
    
    # Admin login
    print_info "Authenticating as Admin..."
    local admin_response=$(api_call_full "POST" "/api/v1/auth/login" "" \
        '{"username": "test_admin", "password": "Admin123!"}')
    
    local admin_status=$(echo "$admin_response" | grep -o "HTTPSTATUS:[0-9]*" | sed 's/HTTPSTATUS://')
    local admin_body=$(echo "$admin_response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    if [ "$admin_status" = "200" ]; then
        ADMIN_TOKEN=$(echo "$admin_body" | jq -r '.data.token')
        echo -e "${GREEN}✓ Admin authenticated${NC}"
    else
        echo -e "${RED}✗ Admin authentication failed${NC}"
        exit 1
    fi
    
    # For this demo, we'll use the admin token for all roles
    # In production, you would have separate credentials for each role
    MANAGER_TOKEN="$ADMIN_TOKEN"
    SUPERVISOR_TOKEN="$ADMIN_TOKEN"
    TECHNICIAN_TOKEN="$ADMIN_TOKEN"
    
    echo -e "${YELLOW}Note: Using admin token for all roles (demo mode)${NC}"
}

# Get project ID for testing
get_test_project() {
    print_section "📋 Setup Test Environment"
    
    print_info "Getting project ID for testing..."
    local projects_response=$(api_call_full "GET" "/api/v1/projects?pageSize=1" "$ADMIN_TOKEN" "")
    local projects_body=$(echo "$projects_response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    PROJECT_ID=$(echo "$projects_body" | jq -r '.data.items[0].projectId // empty')
    
    if [ -n "$PROJECT_ID" ]; then
        echo -e "${GREEN}✓ Using project: $PROJECT_ID${NC}"
    else
        echo -e "${RED}✗ No projects found${NC}"
        exit 1
    fi
}

# Create test task for manipulation
create_test_task() {
    print_info "Creating test task..."
    
    local task_data='{
        "title": "Permission Test Task",
        "description": "Task created for testing role-based permissions"
    }'
    
    local response=$(api_call_full "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" "$task_data")
    local response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    TASK_ID=$(echo "$response_body" | jq -r '.data.taskId // empty')
    
    if [ -n "$TASK_ID" ]; then
        echo -e "${GREEN}✓ Test task created: $TASK_ID${NC}"
    else
        echo -e "${RED}✗ Failed to create test task${NC}"
        exit 1
    fi
}

# Test Admin & Manager permissions (should have full access)
test_admin_manager_permissions() {
    print_section "👑 Testing Admin & Manager Permissions"
    
    # Test task creation
    local create_data='{"title": "Admin Test Task", "description": "Created by admin"}'
    
    local admin_create_status=$(api_call_status "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" "$create_data")
    log_result "Create Task" "Admin" "201" "$admin_create_status" "Admin creating task"
    
    local manager_create_status=$(api_call_status "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$MANAGER_TOKEN" "$create_data")
    log_result "Create Task" "Manager" "201" "$manager_create_status" "Manager creating task"
    
    # Test task viewing
    local admin_view_status=$(api_call_status "GET" "/api/v1/tasks" "$ADMIN_TOKEN" "")
    log_result "View All Tasks" "Admin" "200" "$admin_view_status" "Admin viewing all tasks"
    
    local manager_view_status=$(api_call_status "GET" "/api/v1/tasks" "$MANAGER_TOKEN" "")
    log_result "View All Tasks" "Manager" "200" "$manager_view_status" "Manager viewing all tasks"
    
    # Test task updating
    local update_data='{"title": "Updated by Admin", "description": "Updated", "status": "InProgress"}'
    
    local admin_update_status=$(api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" "$update_data")
    log_result "Update Task" "Admin" "200" "$admin_update_status" "Admin updating task"
    
    local manager_update_status=$(api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$MANAGER_TOKEN" "$update_data")
    log_result "Update Task" "Manager" "200" "$manager_update_status" "Manager updating task"
    
    # Test task status update
    local admin_status_update=$(api_call_status "PATCH" "/api/v1/tasks/$TASK_ID/status" "$ADMIN_TOKEN" '"Completed"')
    log_result "Update Status" "Admin" "200" "$admin_status_update" "Admin updating task status"
    
    # Test task assignment
    local assignment_data='{"title": "Assigned Task", "description": "Assigned", "status": "InProgress", "assignedTechnicianId": null}'
    
    local admin_assign_status=$(api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" "$assignment_data")
    log_result "Assign Task" "Admin" "200" "$admin_assign_status" "Admin assigning task"
    
    # Test task deletion (create a task first)
    local delete_task_response=$(api_call_full "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" "$create_data")
    local delete_task_body=$(echo "$delete_task_response" | sed 's/HTTPSTATUS:[0-9]*$//')
    local delete_task_id=$(echo "$delete_task_body" | jq -r '.data.taskId // empty')
    
    if [ -n "$delete_task_id" ]; then
        local admin_delete_status=$(api_call_status "DELETE" "/api/v1/tasks/$delete_task_id" "$ADMIN_TOKEN" "")
        log_result "Delete Task" "Admin" "200" "$admin_delete_status" "Admin deleting task"
    fi
}

# Test Supervisor permissions
test_supervisor_permissions() {
    print_section "👮 Testing Supervisor Permissions"
    
    # Supervisors should be able to create tasks for managed projects
    local create_data='{"title": "Supervisor Test Task", "description": "Created by supervisor"}'
    
    local supervisor_create_status=$(api_call_status "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$SUPERVISOR_TOKEN" "$create_data")
    log_result "Create Task" "Supervisor" "201" "$supervisor_create_status" "Supervisor creating task for managed project"
    
    # Supervisors should be able to update tasks in managed projects
    local update_data='{"title": "Updated by Supervisor", "description": "Updated", "status": "InProgress"}'
    
    local supervisor_update_status=$(api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$SUPERVISOR_TOKEN" "$update_data")
    log_result "Update Task" "Supervisor" "200" "$supervisor_update_status" "Supervisor updating task in managed project"
    
    # Supervisors should be able to view tasks in managed projects
    local supervisor_view_status=$(api_call_status "GET" "/api/v1/tasks?projectId=$PROJECT_ID" "$SUPERVISOR_TOKEN" "")
    log_result "View Project Tasks" "Supervisor" "200" "$supervisor_view_status" "Supervisor viewing tasks in managed project"
    
    # Supervisors should NOT be able to delete tasks (should fail)
    local supervisor_delete_status=$(api_call_status "DELETE" "/api/v1/tasks/$TASK_ID" "$SUPERVISOR_TOKEN" "")
    
    # Expected to fail - supervisor cannot delete tasks
    if [ "$supervisor_delete_status" = "403" ] || [ "$supervisor_delete_status" = "401" ]; then
        log_result "Delete Task (Forbidden)" "Supervisor" "403" "$supervisor_delete_status" "Supervisor attempting to delete task (should fail)"
    else
        # If it doesn't fail as expected, log the actual result
        log_result "Delete Task (Unexpected)" "Supervisor" "403" "$supervisor_delete_status" "Supervisor delete should be forbidden"
    fi
}

# Test Technician/Task Assignee permissions
test_technician_permissions() {
    print_section "🔧 Testing Technician/Assignee Permissions"
    
    # First assign the test task to the technician (using admin privileges)
    local assignment_data='{
        "title": "Assigned to Technician",
        "description": "Task assigned for testing technician permissions",
        "status": "InProgress",
        "assignedTechnicianId": "'$(echo "$ADMIN_TOKEN" | cut -c1-36)'"
    }'
    
    api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" "$assignment_data" > /dev/null
    
    # Technicians should be able to update status of assigned tasks
    local technician_status_update=$(api_call_status "PATCH" "/api/v1/tasks/$TASK_ID/status" "$TECHNICIAN_TOKEN" '"Completed"')
    log_result "Update Assigned Task Status" "Technician" "200" "$technician_status_update" "Technician updating assigned task status"
    
    # Technicians should be able to view their assigned tasks
    local technician_view_status=$(api_call_status "GET" "/api/v1/tasks/$TASK_ID" "$TECHNICIAN_TOKEN" "")
    log_result "View Assigned Task" "Technician" "200" "$technician_view_status" "Technician viewing assigned task"
    
    # Technicians should NOT be able to reassign tasks to others (should fail)
    local reassign_data='{
        "title": "Reassigned Task",
        "description": "Attempting reassignment",
        "status": "InProgress",
        "assignedTechnicianId": "different-user-id"
    }'
    
    local technician_reassign_status=$(api_call_status "PUT" "/api/v1/tasks/$TASK_ID" "$TECHNICIAN_TOKEN" "$reassign_data")
    
    # This might succeed depending on implementation, but ideally should be restricted
    if [ "$technician_reassign_status" = "403" ] || [ "$technician_reassign_status" = "401" ]; then
        log_result "Reassign Task (Forbidden)" "Technician" "403" "$technician_reassign_status" "Technician attempting reassignment (should fail)"
    else
        log_result "Reassign Task (Allowed)" "Technician" "403" "$technician_reassign_status" "Technician reassignment should be restricted"
    fi
    
    # Technicians should NOT be able to delete tasks (should fail)
    local technician_delete_status=$(api_call_status "DELETE" "/api/v1/tasks/$TASK_ID" "$TECHNICIAN_TOKEN" "")
    
    if [ "$technician_delete_status" = "403" ] || [ "$technician_delete_status" = "401" ]; then
        log_result "Delete Task (Forbidden)" "Technician" "403" "$technician_delete_status" "Technician attempting to delete task (should fail)"
    else
        log_result "Delete Task (Unexpected)" "Technician" "403" "$technician_delete_status" "Technician delete should be forbidden"
    fi
}

# Test general authenticated user permissions
test_general_user_permissions() {
    print_section "👤 Testing General Authenticated User Permissions"
    
    # All authenticated users should be able to view tasks they have access to
    local user_view_status=$(api_call_status "GET" "/api/v1/tasks" "$TECHNICIAN_TOKEN" "")
    log_result "View Accessible Tasks" "General User" "200" "$user_view_status" "General user viewing accessible tasks"
    
    # All authenticated users should be able to filter and search tasks
    local user_search_status=$(api_call_status "GET" "/api/v1/tasks?projectId=$PROJECT_ID" "$TECHNICIAN_TOKEN" "")
    log_result "Filter Tasks" "General User" "200" "$user_search_status" "General user filtering tasks"
    
    # General users should NOT be able to modify task data unless assigned
    local modify_data='{"title": "Modified by General User", "description": "Should not work", "status": "Cancelled"}'
    
    # Create a task that's not assigned to this user
    local unassigned_task_response=$(api_call_full "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" '{"title": "Unassigned Task"}')
    local unassigned_task_body=$(echo "$unassigned_task_response" | sed 's/HTTPSTATUS:[0-9]*$//')
    local unassigned_task_id=$(echo "$unassigned_task_body" | jq -r '.data.taskId // empty')
    
    if [ -n "$unassigned_task_id" ]; then
        local user_modify_status=$(api_call_status "PUT" "/api/v1/tasks/$unassigned_task_id" "$TECHNICIAN_TOKEN" "$modify_data")
        
        if [ "$user_modify_status" = "403" ] || [ "$user_modify_status" = "401" ]; then
            log_result "Modify Unassigned Task (Forbidden)" "General User" "403" "$user_modify_status" "General user modifying unassigned task (should fail)"
        else
            log_result "Modify Unassigned Task (Unexpected)" "General User" "403" "$user_modify_status" "General user should not modify unassigned tasks"
        fi
    fi
}

# Test unauthorized access
test_unauthorized_access() {
    print_section "🚫 Testing Unauthorized Access"
    
    # Test without authentication token
    local no_auth_status=$(api_call_status "GET" "/api/v1/tasks" "" "")
    
    if [ "$no_auth_status" = "401" ]; then
        log_result "No Auth Access (Forbidden)" "Unauthenticated" "401" "$no_auth_status" "Access without authentication (should fail)"
    else
        log_result "No Auth Access (Unexpected)" "Unauthenticated" "401" "$no_auth_status" "Should require authentication"
    fi
    
    # Test with invalid token
    local invalid_auth_status=$(api_call_status "GET" "/api/v1/tasks" "invalid-token" "")
    
    if [ "$invalid_auth_status" = "401" ]; then
        log_result "Invalid Auth (Forbidden)" "Invalid Token" "401" "$invalid_auth_status" "Access with invalid token (should fail)"
    else
        log_result "Invalid Auth (Unexpected)" "Invalid Token" "401" "$invalid_auth_status" "Should reject invalid tokens"
    fi
}

# Generate detailed test report
generate_test_report() {
    echo ""
    echo -e "${BLUE}=================================================================${NC}"
    echo -e "${BLUE}📊 ROLE-BASED PERMISSIONS TEST REPORT${NC}"
    echo -e "${BLUE}=================================================================${NC}"
    
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    
    echo -e "${YELLOW}Test Results by Role:${NC}"
    echo ""
    
    # Count and display results
    for result in "${TEST_RESULTS[@]}"; do
        IFS='|' read -r status test_name role expected actual details <<< "$result"
        total_tests=$((total_tests + 1))
        
        if [ "$status" = "PASS" ]; then
            passed_tests=$((passed_tests + 1))
            echo -e "${GREEN}✓ PASS${NC} - $role: $test_name"
        else
            failed_tests=$((failed_tests + 1))
            echo -e "${RED}✗ FAIL${NC} - $role: $test_name (Expected: $expected, Got: $actual)"
        fi
    done
    
    echo ""
    echo -e "${BLUE}Summary:${NC}"
    echo -e "Total Tests: ${YELLOW}$total_tests${NC}"
    echo -e "Passed: ${GREEN}$passed_tests${NC}"
    echo -e "Failed: ${RED}$failed_tests${NC}"
    
    if [ $failed_tests -eq 0 ]; then
        echo -e "${GREEN}🎉 All permission tests passed!${NC}"
    else
        echo -e "${YELLOW}⚠ Some permission tests failed - check implementation${NC}"
    fi
    
    echo ""
    echo -e "${CYAN}Role Summary:${NC}"
    echo -e "${GREEN}✅ Admin & Manager:${NC} Full access - Create, Update, Delete, Assign"
    echo -e "${BLUE}✅ Supervisor:${NC} Create/Update managed projects, ❌ No Delete"
    echo -e "${PURPLE}✅ Technician:${NC} Update assigned tasks, ❌ No reassign/delete"
    echo -e "${CYAN}✅ All Users:${NC} View accessible tasks, ❌ No modify unless assigned"
    
    echo ""
    echo -e "${CYAN}Detailed log: $LOG_FILE${NC}"
}

# Main execution
main() {
    echo "Starting role-based permissions test..." | tee "$LOG_FILE"
    
    # Check if API is running
    if ! curl -s "$BASE_URL/health" > /dev/null; then
        echo -e "${RED}✗ API is not running at $BASE_URL${NC}"
        echo -e "${BLUE}Please start the API server first${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ API is running${NC}"
    
    # Run all permission tests
    authenticate_users
    get_test_project
    create_test_task
    test_admin_manager_permissions
    test_supervisor_permissions
    test_technician_permissions
    test_general_user_permissions
    test_unauthorized_access
    
    generate_test_report
    
    echo "Permission testing completed!" | tee -a "$LOG_FILE"
}

# Run the main function
main "$@"
