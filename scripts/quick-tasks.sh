#!/bin/bash

# =============================================================================
# 📋 QUICK TASKS API OPERATIONS SCRIPT
# =============================================================================
# Simple script for common task operations
# Usage: ./quick-tasks.sh [command] [args...]
# =============================================================================

set -e

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Global variables
TOKEN=""
PROJECT_ID=""

# Utility functions
print_usage() {
    echo -e "${BLUE}📋 Quick Tasks API Operations${NC}"
    echo ""
    echo "Usage: $0 [command] [options...]"
    echo ""
    echo -e "${YELLOW}Available Commands:${NC}"
    echo "  list                    - List all tasks"
    echo "  list-project [id]       - List tasks for specific project"
    echo "  create [project-id]     - Create a new task interactively"
    echo "  view [task-id]          - View specific task details"
    echo "  update-status [task-id] - Update task status interactively"
    echo "  assign [task-id] [user-id] - Assign task to user"
    echo "  delete [task-id]        - Delete a task"
    echo "  progress [task-id]      - Create progress report"
    echo ""
    echo -e "${YELLOW}Examples:${NC}"
    echo "  $0 list"
    echo "  $0 create abc123-def456-ghi789"
    echo "  $0 update-status task123"
    echo "  $0 assign task123 user456"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Authentication function
authenticate() {
    print_info "Authenticating as admin..."
    
    local login_response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d '{"username": "test_admin", "password": "Admin123!"}' \
        "$BASE_URL/api/v1/auth/login")
    
    TOKEN=$(echo "$login_response" | jq -r '.data.token // empty')
    
    if [ -z "$TOKEN" ]; then
        print_error "Failed to authenticate"
        exit 1
    fi
    
    print_success "Authenticated successfully"
}

# API call helper
api_call() {
    local method="$1"
    local endpoint="$2"
    local data="$3"
    
    local curl_cmd="curl -s -w 'HTTPSTATUS:%{http_code}'"
    curl_cmd="$curl_cmd -H 'Authorization: Bearer $TOKEN'"
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [ -n "$data" ] && [ "$method" != "GET" ]; then
        curl_cmd="$curl_cmd -d '$data'"
    fi
    
    curl_cmd="$curl_cmd -X $method '$BASE_URL$endpoint'"
    
    local response=$(eval $curl_cmd)
    local http_status=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | sed 's/HTTPSTATUS://')
    local response_body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    if [ "$http_status" -ge 200 ] && [ "$http_status" -lt 300 ]; then
        echo "$response_body"
        return 0
    else
        print_error "API call failed (HTTP $http_status)"
        echo "$response_body" | jq . 2>/dev/null || echo "$response_body"
        return 1
    fi
}

# Command functions
cmd_list() {
    print_info "Fetching all tasks..."
    local response=$(api_call "GET" "/api/v1/tasks")
    
    if [ $? -eq 0 ]; then
        echo "$response" | jq -r '.data.items[] | "ID: \(.taskId) | \(.title) | Status: \(.status) | Project: \(.projectName)"'
        local count=$(echo "$response" | jq -r '.data.totalCount')
        print_success "Found $count tasks"
    fi
}

cmd_list_project() {
    local project_id="$1"
    if [ -z "$project_id" ]; then
        print_error "Project ID required"
        echo "Usage: $0 list-project [project-id]"
        exit 1
    fi
    
    print_info "Fetching tasks for project: $project_id"
    local response=$(api_call "GET" "/api/v1/tasks?projectId=$project_id")
    
    if [ $? -eq 0 ]; then
        echo "$response" | jq -r '.data.items[] | "ID: \(.taskId) | \(.title) | Status: \(.status)"'
        local count=$(echo "$response" | jq -r '.data.totalCount')
        print_success "Found $count tasks for project"
    fi
}

cmd_create() {
    local project_id="$1"
    if [ -z "$project_id" ]; then
        print_error "Project ID required"
        echo "Usage: $0 create [project-id]"
        exit 1
    fi
    
    echo -e "${YELLOW}Creating new task for project: $project_id${NC}"
    
    # Interactive input
    read -p "Task title: " title
    read -p "Task description (optional): " description
    read -p "Due date (YYYY-MM-DDTHH:MM:SSZ, optional): " due_date
    read -p "Assigned technician ID (optional): " technician_id
    
    # Build JSON
    local json='{"title": "'$title'"'
    
    if [ -n "$description" ]; then
        json="$json, \"description\": \"$description\""
    fi
    
    if [ -n "$due_date" ]; then
        json="$json, \"dueDate\": \"$due_date\""
    fi
    
    if [ -n "$technician_id" ]; then
        json="$json, \"assignedTechnicianId\": \"$technician_id\""
    fi
    
    json="$json}"
    
    print_info "Creating task..."
    local response=$(api_call "POST" "/api/v1/tasks?projectId=$project_id" "$json")
    
    if [ $? -eq 0 ]; then
        local task_id=$(echo "$response" | jq -r '.data.taskId')
        print_success "Task created with ID: $task_id"
        echo "$response" | jq '.data'
    fi
}

cmd_view() {
    local task_id="$1"
    if [ -z "$task_id" ]; then
        print_error "Task ID required"
        echo "Usage: $0 view [task-id]"
        exit 1
    fi
    
    print_info "Fetching task details: $task_id"
    local response=$(api_call "GET" "/api/v1/tasks/$task_id")
    
    if [ $? -eq 0 ]; then
        echo "$response" | jq '.data'
    fi
}

cmd_update_status() {
    local task_id="$1"
    if [ -z "$task_id" ]; then
        print_error "Task ID required"
        echo "Usage: $0 update-status [task-id]"
        exit 1
    fi
    
    echo -e "${YELLOW}Available statuses:${NC}"
    echo "1) NotStarted"
    echo "2) InProgress"
    echo "3) Completed"
    echo "4) OnHold"
    echo "5) Cancelled"
    
    read -p "Select status (1-5): " choice
    
    local status=""
    case $choice in
        1) status="NotStarted" ;;
        2) status="InProgress" ;;
        3) status="Completed" ;;
        4) status="OnHold" ;;
        5) status="Cancelled" ;;
        *) print_error "Invalid choice"; exit 1 ;;
    esac
    
    print_info "Updating task status to: $status"
    local response=$(api_call "PATCH" "/api/v1/tasks/$task_id/status" "\"$status\"")
    
    if [ $? -eq 0 ]; then
        print_success "Task status updated successfully"
    fi
}

cmd_assign() {
    local task_id="$1"
    local user_id="$2"
    
    if [ -z "$task_id" ] || [ -z "$user_id" ]; then
        print_error "Task ID and User ID required"
        echo "Usage: $0 assign [task-id] [user-id]"
        exit 1
    fi
    
    # First get current task details
    local current_task=$(api_call "GET" "/api/v1/tasks/$task_id")
    
    if [ $? -ne 0 ]; then
        print_error "Failed to fetch current task details"
        exit 1
    fi
    
    # Extract current values and update assignment
    local title=$(echo "$current_task" | jq -r '.data.title')
    local description=$(echo "$current_task" | jq -r '.data.description // ""')
    local status=$(echo "$current_task" | jq -r '.data.status')
    local due_date=$(echo "$current_task" | jq -r '.data.dueDate // null')
    
    local json="{
        \"title\": \"$title\",
        \"description\": \"$description\",
        \"status\": \"$status\",
        \"assignedTechnicianId\": \"$user_id\""
    
    if [ "$due_date" != "null" ]; then
        json="$json, \"dueDate\": \"$due_date\""
    fi
    
    json="$json}"
    
    print_info "Assigning task to user: $user_id"
    local response=$(api_call "PUT" "/api/v1/tasks/$task_id" "$json")
    
    if [ $? -eq 0 ]; then
        print_success "Task assigned successfully"
    fi
}

cmd_delete() {
    local task_id="$1"
    if [ -z "$task_id" ]; then
        print_error "Task ID required"
        echo "Usage: $0 delete [task-id]"
        exit 1
    fi
    
    read -p "Are you sure you want to delete task $task_id? (y/N): " confirm
    
    if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
        print_info "Deleting task: $task_id"
        local response=$(api_call "DELETE" "/api/v1/tasks/$task_id")
        
        if [ $? -eq 0 ]; then
            print_success "Task deleted successfully"
        fi
    else
        print_info "Delete cancelled"
    fi
}

cmd_progress() {
    local task_id="$1"
    if [ -z "$task_id" ]; then
        print_error "Task ID required"
        echo "Usage: $0 progress [task-id]"
        exit 1
    fi
    
    echo -e "${YELLOW}Creating progress report for task: $task_id${NC}"
    
    read -p "Completion percentage (0-100): " completion
    read -p "Status: " status
    read -p "Work completed: " work_completed
    read -p "Issues (optional): " issues
    read -p "Next steps (optional): " next_steps
    read -p "Hours worked: " hours_worked
    
    local json="{
        \"completionPercentage\": $completion,
        \"status\": \"$status\",
        \"workCompleted\": \"$work_completed\",
        \"hoursWorked\": $hours_worked"
    
    if [ -n "$issues" ]; then
        json="$json, \"issues\": \"$issues\""
    fi
    
    if [ -n "$next_steps" ]; then
        json="$json, \"nextSteps\": \"$next_steps\""
    fi
    
    json="$json}"
    
    print_info "Creating progress report..."
    local response=$(api_call "POST" "/api/v1/tasks/$task_id/progress-reports" "$json")
    
    if [ $? -eq 0 ]; then
        print_success "Progress report created successfully"
        echo "$response" | jq '.data'
    fi
}

# Main execution
main() {
    local command="$1"
    shift
    
    if [ -z "$command" ]; then
        print_usage
        exit 1
    fi
    
    # Check if API is running
    if ! curl -s "$BASE_URL/health" > /dev/null; then
        print_error "API is not running at $BASE_URL"
        print_info "Please start the API server first"
        exit 1
    fi
    
    # Authenticate
    authenticate
    
    # Execute command
    case "$command" in
        "list")
            cmd_list "$@"
            ;;
        "list-project")
            cmd_list_project "$@"
            ;;
        "create")
            cmd_create "$@"
            ;;
        "view")
            cmd_view "$@"
            ;;
        "update-status")
            cmd_update_status "$@"
            ;;
        "assign")
            cmd_assign "$@"
            ;;
        "delete")
            cmd_delete "$@"
            ;;
        "progress")
            cmd_progress "$@"
            ;;
        "help"|"-h"|"--help")
            print_usage
            ;;
        *)
            print_error "Unknown command: $command"
            print_usage
            exit 1
            ;;
    esac
}

# Run the main function
main "$@"
