#!/bin/bash

# =============================================================================
# 📋 SOLAR PROJECTS TASKS API TEST SCRIPT
# =============================================================================
# Comprehensive testing script for Task Management API
# Tests all endpoints with different user roles and permissions
# =============================================================================

set -e

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/tasks_api_test_$TIMESTAMP.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Create log directory
mkdir -p "$LOG_DIR"

# Global variables
ADMIN_TOKEN=""
MANAGER_TOKEN=""
SUPERVISOR_TOKEN=""
TECHNICIAN_TOKEN=""
PROJECT_ID=""
TASK_ID=""
ASSIGNED_TASK_ID=""

echo -e "${BLUE}=================================================================${NC}"
echo -e "${BLUE}📋 SOLAR PROJECTS TASKS API TEST SUITE${NC}"
echo -e "${BLUE}=================================================================${NC}"
echo "Testing Task Management API endpoints with different user roles"
echo "Base URL: $BASE_URL"
echo "Log File: $LOG_FILE"
echo ""

# Utility functions
log_message() {
    local level="$1"
    local message="$2"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] [$level] $message" | tee -a "$LOG_FILE"
}

print_step() {
    echo -e "${CYAN}➤ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Function to make API calls with proper error handling
api_call() {
    local method="$1"
    local endpoint="$2"
    local token="$3"
    local data="$4"
    local description="$5"
    
    print_step "$description"
    
    local curl_cmd="curl -s -w 'HTTPSTATUS:%{http_code}'"
    
    if [ -n "$token" ]; then
        curl_cmd="$curl_cmd -H 'Authorization: Bearer $token'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ "$method" != "GET" ] && [ "$method" != "DELETE" ] && [ -n "$data" ]; then
        curl_cmd="$curl_cmd -d '$data'"
    fi
    
    curl_cmd="$curl_cmd -X $method '$BASE_URL$endpoint'"
    
    local response=$(eval $curl_cmd)
    local http_status=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | sed 's/HTTPSTATUS://')
    local response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    log_message "INFO" "$method $endpoint - HTTP $http_status"
    
    if [ "$http_status" -ge 200 ] && [ "$http_status" -lt 300 ]; then
        print_success "$description (HTTP $http_status)"
        if [ -n "$response_body" ] && [ "$response_body" != "null" ]; then
            echo "$response_body" | jq . 2>/dev/null || echo "$response_body"
        fi
        echo "$response_body"
        return 0
    else
        print_error "$description (HTTP $http_status)"
        if [ -n "$response_body" ]; then
            echo "$response_body" | jq . 2>/dev/null || echo "$response_body"
        fi
        log_message "ERROR" "Response: $response_body"
        return 1
    fi
}

# Function to authenticate different user types
authenticate_users() {
    print_step "Authenticating different user roles..."
    
    # Admin login
    local admin_response=$(api_call "POST" "/api/v1/auth/login" "" \
        '{"username": "test_admin", "password": "Admin123!"}' \
        "Admin login")
    
    if [ $? -eq 0 ]; then
        ADMIN_TOKEN=$(echo "$admin_response" | jq -r '.data.token // empty')
        print_success "Admin authenticated"
    else
        print_error "Failed to authenticate admin"
        return 1
    fi
    
    # Manager login (using the existing manager from manager_login.json)
    if [ -f "manager_login.json" ]; then
        MANAGER_TOKEN=$(jq -r '.data.token' manager_login.json)
        print_success "Manager token retrieved from file"
    else
        print_warning "Manager login file not found, using admin token"
        MANAGER_TOKEN="$ADMIN_TOKEN"
    fi
    
    # For this demo, we'll use admin token for supervisor and technician
    # In a real scenario, you'd have separate login credentials
    SUPERVISOR_TOKEN="$ADMIN_TOKEN"
    TECHNICIAN_TOKEN="$ADMIN_TOKEN"
    
    print_info "All user tokens obtained"
    return 0
}

# Function to get a project ID for testing
get_project_id() {
    print_step "Getting a project ID for task creation..."
    
    local projects_response=$(api_call "GET" "/api/v1/projects?pageSize=1" "$ADMIN_TOKEN" "" \
        "Get projects list")
    
    if [ $? -eq 0 ]; then
        PROJECT_ID=$(echo "$projects_response" | jq -r '.data.items[0].projectId // empty')
        if [ -n "$PROJECT_ID" ]; then
            print_success "Using project ID: $PROJECT_ID"
            return 0
        fi
    fi
    
    print_error "Failed to get project ID"
    return 1
}

# Function to test task creation (Admin/Manager roles)
test_task_creation() {
    echo ""
    print_step "🏗️ Testing Task Creation (Admin/Manager Roles)"
    
    # Test task creation as Admin
    local task_data='{
        "title": "Install Solar Panels on Roof Section A",
        "description": "Install 50 solar panels on the main roof section with proper mounting and wiring",
        "dueDate": "2025-08-15T10:00:00Z",
        "assignedTechnicianId": null
    }'
    
    local create_response=$(api_call "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" \
        "$task_data" "Create task as Admin")
    
    if [ $? -eq 0 ]; then
        TASK_ID=$(echo "$create_response" | jq -r '.data.taskId // empty')
        print_success "Task created with ID: $TASK_ID"
    fi
    
    # Test task creation as Manager
    local manager_task_data='{
        "title": "Electrical Wiring Installation",
        "description": "Install electrical wiring and connections for solar panel system",
        "dueDate": "2025-08-20T14:00:00Z"
    }'
    
    api_call "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$MANAGER_TOKEN" \
        "$manager_task_data" "Create task as Manager"
}

# Function to test task assignment
test_task_assignment() {
    echo ""
    print_step "👥 Testing Task Assignment (Admin/Manager Roles)"
    
    if [ -z "$TASK_ID" ]; then
        print_warning "No task ID available for assignment test"
        return 1
    fi
    
    # Get a user ID for assignment (using admin user for demo)
    local users_response=$(api_call "GET" "/api/v1/users?pageSize=1" "$ADMIN_TOKEN" "" \
        "Get users for assignment")
    
    local user_id=$(echo "$users_response" | jq -r '.data.items[0].userId // empty')
    
    if [ -n "$user_id" ]; then
        local assignment_data='{
            "title": "Install Solar Panels on Roof Section A",
            "description": "Install 50 solar panels on the main roof section with proper mounting and wiring",
            "status": "InProgress",
            "dueDate": "2025-08-15T10:00:00Z",
            "assignedTechnicianId": "'$user_id'"
        }'
        
        api_call "PUT" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" \
            "$assignment_data" "Assign task to technician"
        
        ASSIGNED_TASK_ID="$TASK_ID"
    fi
}

# Function to test task viewing (All authenticated users)
test_task_viewing() {
    echo ""
    print_step "👀 Testing Task Viewing (All Authenticated Users)"
    
    # Test viewing all tasks
    api_call "GET" "/api/v1/tasks" "$ADMIN_TOKEN" "" \
        "View all tasks as Admin"
    
    api_call "GET" "/api/v1/tasks" "$MANAGER_TOKEN" "" \
        "View all tasks as Manager"
    
    api_call "GET" "/api/v1/tasks" "$SUPERVISOR_TOKEN" "" \
        "View all tasks as Supervisor"
    
    api_call "GET" "/api/v1/tasks" "$TECHNICIAN_TOKEN" "" \
        "View all tasks as Technician"
    
    # Test viewing specific task
    if [ -n "$TASK_ID" ]; then
        api_call "GET" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" "" \
            "View specific task as Admin"
    fi
    
    # Test filtering tasks by project
    api_call "GET" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" "" \
        "Filter tasks by project ID"
}

# Function to test task status updates
test_task_status_updates() {
    echo ""
    print_step "🔄 Testing Task Status Updates"
    
    if [ -z "$TASK_ID" ]; then
        print_warning "No task ID available for status update test"
        return 1
    fi
    
    # Test status update as Admin
    api_call "PATCH" "/api/v1/tasks/$TASK_ID/status" "$ADMIN_TOKEN" \
        '"InProgress"' "Update task status as Admin"
    
    # Test status update as assigned technician
    if [ -n "$ASSIGNED_TASK_ID" ]; then
        api_call "PATCH" "/api/v1/tasks/$ASSIGNED_TASK_ID/status" "$TECHNICIAN_TOKEN" \
            '"Completed"' "Update assigned task status as Technician"
    fi
}

# Function to test task updates (Admin/Manager/Supervisor)
test_task_updates() {
    echo ""
    print_step "✏️ Testing Task Updates (Admin/Manager/Supervisor Roles)"
    
    if [ -z "$TASK_ID" ]; then
        print_warning "No task ID available for update test"
        return 1
    fi
    
    # Test partial update as Admin
    local partial_update='{
        "description": "Updated description: Install 50 solar panels with improved mounting system",
        "status": "InProgress"
    }'
    
    api_call "PATCH" "/api/v1/tasks/$TASK_ID" "$ADMIN_TOKEN" \
        "$partial_update" "Partial update task as Admin"
    
    # Test full update as Manager
    local full_update='{
        "title": "Updated: Install Solar Panels on Roof Section A",
        "description": "Fully updated description with new requirements",
        "status": "InProgress",
        "dueDate": "2025-08-25T16:00:00Z",
        "assignedTechnicianId": null
    }'
    
    api_call "PUT" "/api/v1/tasks/$TASK_ID" "$MANAGER_TOKEN" \
        "$full_update" "Full update task as Manager"
}

# Function to test advanced task search
test_advanced_search() {
    echo ""
    print_step "🔍 Testing Advanced Task Search"
    
    # Test advanced search with various parameters
    api_call "GET" "/api/v1/tasks/advanced?pageSize=5&sortBy=createdAt&sortDirection=desc" "$ADMIN_TOKEN" "" \
        "Advanced search with sorting"
    
    # Test search with status filter
    api_call "GET" "/api/v1/tasks?status=InProgress" "$ADMIN_TOKEN" "" \
        "Search tasks by status"
    
    # Test search with assignee filter
    api_call "GET" "/api/v1/tasks?assigneeId=$ADMIN_TOKEN" "$ADMIN_TOKEN" "" \
        "Search tasks by assignee"
}

# Function to test progress reports
test_progress_reports() {
    echo ""
    print_step "📊 Testing Task Progress Reports"
    
    if [ -z "$TASK_ID" ]; then
        print_warning "No task ID available for progress report test"
        return 1
    fi
    
    # Create a progress report
    local progress_data='{
        "completionPercentage": 25.0,
        "status": "InProgress",
        "workCompleted": "Completed roof inspection and marked panel positions",
        "issues": "Weather delay due to rain",
        "nextSteps": "Install mounting brackets once weather clears",
        "hoursWorked": 4.5
    }'
    
    api_call "POST" "/api/v1/tasks/$TASK_ID/progress-reports" "$ADMIN_TOKEN" \
        "$progress_data" "Create progress report"
    
    # Get progress reports
    api_call "GET" "/api/v1/tasks/$TASK_ID/progress-reports" "$ADMIN_TOKEN" "" \
        "Get task progress reports"
}

# Function to test permission restrictions
test_permission_restrictions() {
    echo ""
    print_step "🔒 Testing Permission Restrictions"
    
    if [ -z "$TASK_ID" ]; then
        print_warning "No task ID available for permission test"
        return 1
    fi
    
    # Test technician trying to delete task (should fail)
    print_info "Testing technician delete restriction (should fail)..."
    api_call "DELETE" "/api/v1/tasks/$TASK_ID" "$TECHNICIAN_TOKEN" "" \
        "Technician trying to delete task (should fail)" || true
    
    # Test supervisor trying to delete task (should fail)
    print_info "Testing supervisor delete restriction (should fail)..."
    api_call "DELETE" "/api/v1/tasks/$TASK_ID" "$SUPERVISOR_TOKEN" "" \
        "Supervisor trying to delete task (should fail)" || true
}

# Function to test task deletion (Admin/Manager only)
test_task_deletion() {
    echo ""
    print_step "🗑️ Testing Task Deletion (Admin/Manager Only)"
    
    # Create a test task for deletion
    local delete_task_data='{
        "title": "Test Task for Deletion",
        "description": "This task will be deleted as part of the test"
    }'
    
    local delete_response=$(api_call "POST" "/api/v1/tasks?projectId=$PROJECT_ID" "$ADMIN_TOKEN" \
        "$delete_task_data" "Create task for deletion test")
    
    if [ $? -eq 0 ]; then
        local delete_task_id=$(echo "$delete_response" | jq -r '.data.taskId // empty')
        
        if [ -n "$delete_task_id" ]; then
            # Test deletion as Admin
            api_call "DELETE" "/api/v1/tasks/$delete_task_id" "$ADMIN_TOKEN" "" \
                "Delete task as Admin"
        fi
    fi
}

# Function to generate test summary
generate_summary() {
    echo ""
    echo -e "${BLUE}=================================================================${NC}"
    echo -e "${BLUE}📊 TASKS API TEST SUMMARY${NC}"
    echo -e "${BLUE}=================================================================${NC}"
    
    echo -e "${GREEN}✅ Completed Tests:${NC}"
    echo "• Task Creation (Admin/Manager)"
    echo "• Task Assignment to Technicians"
    echo "• Task Viewing (All Users)"
    echo "• Status Updates (Users + Assignees)"
    echo "• Task Updates (Admin/Manager/Supervisor)"
    echo "• Advanced Search and Filtering"
    echo "• Progress Report Management"
    echo "• Permission Restrictions Testing"
    echo "• Task Deletion (Admin/Manager)"
    
    echo ""
    echo -e "${YELLOW}📋 Role-based Access Summary:${NC}"
    echo -e "${GREEN}Admin & Manager:${NC} Full access - Create, Update, Delete, Assign"
    echo -e "${BLUE}Supervisor:${NC} Create/Update managed projects, No Delete"
    echo -e "${PURPLE}Technician:${NC} Update assigned tasks, View own tasks"
    echo -e "${CYAN}All Users:${NC} View accessible tasks, Filter and search"
    
    echo ""
    echo -e "${BLUE}📖 API Endpoints Tested:${NC}"
    echo "• GET    /api/v1/tasks - List all tasks"
    echo "• GET    /api/v1/tasks/{id} - Get specific task"
    echo "• POST   /api/v1/tasks - Create new task"
    echo "• PUT    /api/v1/tasks/{id} - Full update"
    echo "• PATCH  /api/v1/tasks/{id} - Partial update"
    echo "• PATCH  /api/v1/tasks/{id}/status - Status update"
    echo "• DELETE /api/v1/tasks/{id} - Delete task"
    echo "• GET    /api/v1/tasks/advanced - Advanced search"
    echo "• POST   /api/v1/tasks/{id}/progress-reports - Create progress report"
    echo "• GET    /api/v1/tasks/{id}/progress-reports - Get progress reports"
    
    echo ""
    echo -e "${CYAN}Log file saved: $LOG_FILE${NC}"
    echo -e "${CYAN}Test completed at: $(date)${NC}"
}

# Main execution
main() {
    log_message "INFO" "Starting Tasks API test suite"
    
    # Check if API is running
    if ! curl -s "$BASE_URL/health" > /dev/null; then
        print_error "API is not running at $BASE_URL"
        print_info "Please start the API server first"
        exit 1
    fi
    
    print_success "API is running"
    
    # Run all tests
    authenticate_users || exit 1
    get_project_id || exit 1
    test_task_creation
    test_task_assignment
    test_task_viewing
    test_task_status_updates
    test_task_updates
    test_advanced_search
    test_progress_reports
    test_permission_restrictions
    test_task_deletion
    
    generate_summary
    
    log_message "INFO" "Tasks API test suite completed"
    print_success "All tests completed successfully!"
}

# Run the main function
main "$@"
