#!/bin/bash

# =============================================================================
# 🧪 COMPREHENSIVE API ENDPOINT TESTING SCRIPT
# =============================================================================
# Solar Projects API - Complete Role-Based Testing
# Supports all user roles: Admin, Manager, User, Viewer
# =============================================================================

set -e  # Exit on any error

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/api_test_$TIMESTAMP.log"

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
SKIPPED_TESTS=0

# JWT Tokens (will be populated during login)
ADMIN_TOKEN=""
MANAGER_TOKEN=""
USER_TOKEN=""
VIEWER_TOKEN=""

# Test user credentials
ADMIN_USER="testadmin:Admin123!"
MANAGER_USER="manager1:Admin123!"
USER_USER="tech1:Admin123!"
VIEWER_USER="viewer:Admin123!"

# =============================================================================
# UTILITY FUNCTIONS
# =============================================================================

setup_directories() {
    mkdir -p "$LOG_DIR"
    echo "$(date): Starting API endpoint testing" > "$LOG_FILE"
}

log_message() {
    local level=$1
    local message=$2
    echo "$(date '+%Y-%m-%d %H:%M:%S') [$level] $message" >> "$LOG_FILE"
}

print_header() {
    echo -e "\n${PURPLE}======================================================================${NC}"
    echo -e "${PURPLE}🧪 $1${NC}"
    echo -e "${PURPLE}======================================================================${NC}\n"
}

print_section() {
    echo -e "\n${CYAN}📋 $1${NC}"
    echo -e "${CYAN}----------------------------------------${NC}"
}

print_test() {
    local role=$1
    local method=$2
    local endpoint=$3
    local description=$4
    echo -e "${BLUE}🔍 Testing:${NC} $role - $method $endpoint"
    echo -e "${BLUE}   Description:${NC} $description"
}

print_result() {
    local status=$1
    local expected=$2
    local actual=$3
    local message=$4
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    
    if [[ "$status" == "PASS" ]]; then
        echo -e "${GREEN}✅ PASS${NC} - Expected: $expected, Got: $actual"
        [[ -n "$message" ]] && echo -e "   ${GREEN}$message${NC}"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        log_message "PASS" "$method $endpoint - $message"
    elif [[ "$status" == "FAIL" ]]; then
        echo -e "${RED}❌ FAIL${NC} - Expected: $expected, Got: $actual"
        [[ -n "$message" ]] && echo -e "   ${RED}$message${NC}"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        log_message "FAIL" "$method $endpoint - $message"
    else
        echo -e "${YELLOW}⏭️  SKIP${NC} - $message"
        SKIPPED_TESTS=$((SKIPPED_TESTS + 1))
        log_message "SKIP" "$method $endpoint - $message"
    fi
    echo ""
}

# Make HTTP request with proper error handling
make_request() {
    local method=$1
    local endpoint=$2
    local token=$3
    local data=$4
    local content_type=${5:-"application/json"}
    
    local url="$BASE_URL$endpoint"
    local auth_header=""
    
    if [[ -n "$token" ]]; then
        auth_header="-H \"Authorization: Bearer $token\""
    fi
    
    local cmd="curl -s -w \"HTTPSTATUS:%{http_code}\" -X $method"
    cmd="$cmd -H \"Content-Type: $content_type\""
    cmd="$cmd $auth_header"
    
    if [[ -n "$data" && "$method" != "GET" ]]; then
        cmd="$cmd -d '$data'"
    fi
    
    cmd="$cmd \"$url\""
    
    log_message "REQUEST" "$method $url"
    eval $cmd
}

# Extract HTTP status from curl response
extract_status() {
    echo "$1" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2
}

# Extract body from curl response
extract_body() {
    echo "$1" | sed 's/HTTPSTATUS:[0-9]*$//'
}

# =============================================================================
# AUTHENTICATION FUNCTIONS
# =============================================================================

test_health_endpoints() {
    print_section "Health & System Endpoints"
    
    # Basic health check
    print_test "PUBLIC" "GET" "/health" "Basic health check"
    response=$(make_request "GET" "/health" "")
    status=$(extract_status "$response")
    
    if [[ "$status" == "200" ]]; then
        print_result "PASS" "200" "$status" "Health endpoint accessible"
    else
        print_result "FAIL" "200" "$status" "Health endpoint failed"
    fi
    
    # Detailed health check
    print_test "PUBLIC" "GET" "/health/detailed" "Detailed health with DB check"
    response=$(make_request "GET" "/health/detailed" "")
    status=$(extract_status "$response")
    
    if [[ "$status" == "200" ]]; then
        print_result "PASS" "200" "$status" "Detailed health endpoint accessible"
    else
        print_result "FAIL" "200" "$status" "Detailed health endpoint failed"
    fi
}

authenticate_users() {
    print_section "User Authentication"
    
    # Test admin login
    local credentials=$ADMIN_USER
    local username=$(echo $credentials | cut -d: -f1)
    local password=$(echo $credentials | cut -d: -f2)
    
    print_test "admin" "POST" "/api/v1/auth/login" "Login for admin role"
    local login_data="{\"username\":\"$username\",\"password\":\"$password\"}"
    response=$(make_request "POST" "/api/v1/auth/login" "" "$login_data")
    status=$(extract_status "$response")
    body=$(extract_body "$response")
    
    if [[ "$status" == "200" ]]; then
        token=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        if [[ -n "$token" ]]; then
            ADMIN_TOKEN=$token
            print_result "PASS" "200" "$status" "Successfully authenticated admin"
        else
            print_result "FAIL" "200" "$status" "Authentication succeeded but no token received for admin"
        fi
    else
        print_result "FAIL" "200" "$status" "Authentication failed for admin"
    fi
    
    # Test manager login
    credentials=$MANAGER_USER
    username=$(echo $credentials | cut -d: -f1)
    password=$(echo $credentials | cut -d: -f2)
    
    print_test "manager" "POST" "/api/v1/auth/login" "Login for manager role"
    login_data="{\"username\":\"$username\",\"password\":\"$password\"}"
    response=$(make_request "POST" "/api/v1/auth/login" "" "$login_data")
    status=$(extract_status "$response")
    body=$(extract_body "$response")
    
    if [[ "$status" == "200" ]]; then
        token=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        if [[ -n "$token" ]]; then
            MANAGER_TOKEN=$token
            print_result "PASS" "200" "$status" "Successfully authenticated manager"
        else
            print_result "FAIL" "200" "$status" "Authentication succeeded but no token received for manager"
        fi
    else
        print_result "FAIL" "200" "$status" "Authentication failed for manager"
    fi
    
    # Test user login
    credentials=$USER_USER
    username=$(echo $credentials | cut -d: -f1)
    password=$(echo $credentials | cut -d: -f2)
    
    print_test "user" "POST" "/api/v1/auth/login" "Login for user role"
    login_data="{\"username\":\"$username\",\"password\":\"$password\"}"
    response=$(make_request "POST" "/api/v1/auth/login" "" "$login_data")
    status=$(extract_status "$response")
    body=$(extract_body "$response")
    
    if [[ "$status" == "200" ]]; then
        token=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        if [[ -n "$token" ]]; then
            USER_TOKEN=$token
            print_result "PASS" "200" "$status" "Successfully authenticated user"
        else
            print_result "FAIL" "200" "$status" "Authentication succeeded but no token received for user"
        fi
    else
        print_result "FAIL" "200" "$status" "Authentication failed for user"
    fi
    
    # Test viewer login
    credentials=$VIEWER_USER
    username=$(echo $credentials | cut -d: -f1)
    password=$(echo $credentials | cut -d: -f2)
    
    print_test "viewer" "POST" "/api/v1/auth/login" "Login for viewer role"
    login_data="{\"username\":\"$username\",\"password\":\"$password\"}"
    response=$(make_request "POST" "/api/v1/auth/login" "" "$login_data")
    status=$(extract_status "$response")
    body=$(extract_body "$response")
    
    if [[ "$status" == "200" ]]; then
        token=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        if [[ -n "$token" ]]; then
            VIEWER_TOKEN=$token
            print_result "PASS" "200" "$status" "Successfully authenticated viewer"
        else
            print_result "FAIL" "200" "$status" "Authentication succeeded but no token received for viewer"
        fi
    else
        print_result "FAIL" "200" "$status" "Authentication failed for viewer"
    fi
}

# =============================================================================
# ENDPOINT TESTING FUNCTIONS
# =============================================================================

test_user_management() {
    print_section "User Management Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        # GET /api/v1/users - List users
        print_test "admin" "GET" "/api/v1/users" "List all users"
        response=$(make_request "GET" "/api/v1/users" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can list users"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to list users"
        fi
        
        # POST /api/v1/users - Create user
        print_test "admin" "POST" "/api/v1/users" "Create new user"
        local user_data='{"username":"testuser","email":"test@example.com","password":"Admin123!","fullName":"Test User","roleId":3}'
        response=$(make_request "POST" "/api/v1/users" "$ADMIN_TOKEN" "$user_data")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" || "$status" == "201" ]]; then
            print_result "PASS" "200/201" "$status" "Admin can create users"
        else
            print_result "FAIL" "200/201" "$status" "Admin should be able to create users"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin user management tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/users" "List all users"
        response=$(make_request "GET" "/api/v1/users" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can list users"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to list users"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager user management tests"
    fi
    
    # Test with regular user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/users" "List all users (should be denied)"
        response=$(make_request "GET" "/api/v1/users" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "401" || "$status" == "403" ]]; then
            print_result "PASS" "401/403" "$status" "User correctly denied access to user list"
        else
            print_result "FAIL" "401/403" "$status" "User should not be able to list users"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user management tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/users" "List all users (should be denied)"
        response=$(make_request "GET" "/api/v1/users" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "401" || "$status" == "403" ]]; then
            print_result "PASS" "401/403" "$status" "Viewer correctly denied access to user list"
        else
            print_result "FAIL" "401/403" "$status" "Viewer should not be able to list users"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer user management tests"
    fi
}

test_project_management() {
    print_section "Project Management Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        # GET /api/v1/projects - List projects
        print_test "admin" "GET" "/api/v1/projects" "List projects"
        response=$(make_request "GET" "/api/v1/projects" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access project list"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access project list"
        fi
        
        # POST /api/v1/projects - Create project
        print_test "admin" "POST" "/api/v1/projects" "Create new project"
        local project_data='{"projectName":"Test Project","address":"123 Test St","clientInfo":"Test client","status":"Planning"}'
        response=$(make_request "POST" "/api/v1/projects" "$ADMIN_TOKEN" "$project_data")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" || "$status" == "201" ]]; then
            print_result "PASS" "200/201" "$status" "Admin can create projects"
        else
            print_result "FAIL" "200/201" "$status" "Admin should be able to create projects"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin project tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/projects" "List projects"
        response=$(make_request "GET" "/api/v1/projects" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access project list"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access project list"
        fi
        
        print_test "manager" "POST" "/api/v1/projects" "Create new project"
        local project_data='{"projectName":"Manager Test Project","address":"456 Manager St","clientInfo":"Manager client","status":"Planning"}'
        response=$(make_request "POST" "/api/v1/projects" "$MANAGER_TOKEN" "$project_data")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" || "$status" == "201" ]]; then
            print_result "PASS" "200/201" "$status" "Manager can create projects"
        else
            print_result "FAIL" "200/201" "$status" "Manager should be able to create projects"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager project tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/projects" "List projects"
        response=$(make_request "GET" "/api/v1/projects" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access project list"
        else
            print_result "FAIL" "200" "$status" "User should be able to access project list"
        fi
        
        print_test "user" "POST" "/api/v1/projects" "Create new project (should be denied)"
        local project_data='{"projectName":"User Test Project","address":"789 User St","clientInfo":"User client","status":"Planning"}'
        response=$(make_request "POST" "/api/v1/projects" "$USER_TOKEN" "$project_data")
        status=$(extract_status "$response")
        
        if [[ "$status" == "401" || "$status" == "403" ]]; then
            print_result "PASS" "401/403" "$status" "User correctly denied project creation"
        else
            print_result "FAIL" "401/403" "$status" "User should not be able to create projects"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user project tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/projects" "List projects"
        response=$(make_request "GET" "/api/v1/projects" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access project list"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access project list"
        fi
        
        print_test "viewer" "POST" "/api/v1/projects" "Create new project (should be denied)"
        local project_data='{"projectName":"Viewer Test Project","address":"101 Viewer St","clientInfo":"Viewer client","status":"Planning"}'
        response=$(make_request "POST" "/api/v1/projects" "$VIEWER_TOKEN" "$project_data")
        status=$(extract_status "$response")
        
        if [[ "$status" == "401" || "$status" == "403" ]]; then
            print_result "PASS" "401/403" "$status" "Viewer correctly denied project creation"
        else
            print_result "FAIL" "401/403" "$status" "Viewer should not be able to create projects"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer project tests"
    fi
}

test_task_management() {
    print_section "Task Management Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        print_test "admin" "GET" "/api/v1/tasks" "List tasks"
        response=$(make_request "GET" "/api/v1/tasks" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access task list"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access task list"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin task tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/tasks" "List tasks"
        response=$(make_request "GET" "/api/v1/tasks" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access task list"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access task list"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager task tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/tasks" "List tasks"
        response=$(make_request "GET" "/api/v1/tasks" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access task list"
        else
            print_result "FAIL" "200" "$status" "User should be able to access task list"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user task tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/tasks" "List tasks"
        response=$(make_request "GET" "/api/v1/tasks" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access task list"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access task list"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer task tests"
    fi
}

test_daily_reports() {
    print_section "Daily Reports Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        print_test "admin" "GET" "/api/v1/daily-reports" "List daily reports"
        response=$(make_request "GET" "/api/v1/daily-reports" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access daily reports"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access daily reports"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin daily report tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/daily-reports" "List daily reports"
        response=$(make_request "GET" "/api/v1/daily-reports" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access daily reports"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access daily reports"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager daily report tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/daily-reports" "List daily reports"
        response=$(make_request "GET" "/api/v1/daily-reports" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access daily reports"
        else
            print_result "FAIL" "200" "$status" "User should be able to access daily reports"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user daily report tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/daily-reports" "List daily reports"
        response=$(make_request "GET" "/api/v1/daily-reports" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access daily reports"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access daily reports"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer daily report tests"
    fi
}

test_work_requests() {
    print_section "Work Requests Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        print_test "admin" "GET" "/api/v1/work-requests" "List work requests"
        response=$(make_request "GET" "/api/v1/work-requests" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access work requests"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access work requests"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin work request tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/work-requests" "List work requests"
        response=$(make_request "GET" "/api/v1/work-requests" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access work requests"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access work requests"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager work request tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/work-requests" "List work requests"
        response=$(make_request "GET" "/api/v1/work-requests" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access work requests"
        else
            print_result "FAIL" "200" "$status" "User should be able to access work requests"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user work request tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/work-requests" "List work requests"
        response=$(make_request "GET" "/api/v1/work-requests" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access work requests"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access work requests"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer work request tests"
    fi
}

test_master_plans() {
    print_section "Master Plans Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        print_test "admin" "GET" "/api/v1/master-plans" "List master plans"
        response=$(make_request "GET" "/api/v1/master-plans" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access master plans"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access master plans"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin master plan tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/master-plans" "List master plans"
        response=$(make_request "GET" "/api/v1/master-plans" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access master plans"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access master plans"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager master plan tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/master-plans" "List master plans"
        response=$(make_request "GET" "/api/v1/master-plans" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access master plans"
        else
            print_result "FAIL" "200" "$status" "User should be able to access master plans"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user master plan tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/master-plans" "List master plans"
        response=$(make_request "GET" "/api/v1/master-plans" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access master plans"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access master plans"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer master plan tests"
    fi
}

test_calendar_endpoints() {
    print_section "Calendar Endpoints"
    
    # Test with admin
    if [[ -n "$ADMIN_TOKEN" ]]; then
        print_test "admin" "GET" "/api/v1/calendar" "List calendar events"
        response=$(make_request "GET" "/api/v1/calendar" "$ADMIN_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Admin can access calendar events"
        else
            print_result "FAIL" "200" "$status" "Admin should be able to access calendar events"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No admin token - skipping admin calendar tests"
    fi
    
    # Test with manager
    if [[ -n "$MANAGER_TOKEN" ]]; then
        print_test "manager" "GET" "/api/v1/calendar" "List calendar events"
        response=$(make_request "GET" "/api/v1/calendar" "$MANAGER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Manager can access calendar events"
        else
            print_result "FAIL" "200" "$status" "Manager should be able to access calendar events"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No manager token - skipping manager calendar tests"
    fi
    
    # Test with user
    if [[ -n "$USER_TOKEN" ]]; then
        print_test "user" "GET" "/api/v1/calendar" "List calendar events"
        response=$(make_request "GET" "/api/v1/calendar" "$USER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "User can access calendar events"
        else
            print_result "FAIL" "200" "$status" "User should be able to access calendar events"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No user token - skipping user calendar tests"
    fi
    
    # Test with viewer
    if [[ -n "$VIEWER_TOKEN" ]]; then
        print_test "viewer" "GET" "/api/v1/calendar" "List calendar events"
        response=$(make_request "GET" "/api/v1/calendar" "$VIEWER_TOKEN")
        status=$(extract_status "$response")
        
        if [[ "$status" == "200" ]]; then
            print_result "PASS" "200" "$status" "Viewer can access calendar events"
        else
            print_result "FAIL" "200" "$status" "Viewer should be able to access calendar events"
        fi
    else
        print_result "SKIP" "N/A" "N/A" "No viewer token - skipping viewer calendar tests"
    fi
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

main() {
    print_header "SOLAR PROJECTS API - COMPREHENSIVE TESTING"
    
    echo -e "🚀 ${GREEN}Starting comprehensive API testing...${NC}"
    echo -e "📍 Base URL: ${CYAN}$BASE_URL${NC}"
    echo -e "📝 Log File: ${CYAN}$LOG_FILE${NC}"
    echo -e "⏰ Timestamp: ${CYAN}$(date)${NC}\n"
    
    # Setup
    setup_directories
    
    # Test execution
    test_health_endpoints
    authenticate_users
    test_user_management
    test_project_management
    test_task_management
    test_daily_reports
    test_work_requests
    test_master_plans
    test_calendar_endpoints
    
    # Final results
    print_header "TEST RESULTS SUMMARY"
    
    echo -e "${BLUE}📊 Test Statistics:${NC}"
    echo -e "   Total Tests: ${CYAN}$TOTAL_TESTS${NC}"
    echo -e "   Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "   Failed: ${RED}$FAILED_TESTS${NC}"
    echo -e "   Skipped: ${YELLOW}$SKIPPED_TESTS${NC}"
    
    local success_rate=0
    if [[ $TOTAL_TESTS -gt 0 ]]; then
        success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    fi
    
    echo -e "   Success Rate: ${CYAN}$success_rate%${NC}\n"
    
    if [[ $FAILED_TESTS -eq 0 ]]; then
        echo -e "${GREEN}🎉 ALL TESTS PASSED! API is functioning correctly.${NC}"
        log_message "SUMMARY" "All tests passed - Success rate: $success_rate%"
        exit 0
    else
        echo -e "${RED}⚠️  SOME TESTS FAILED. Please check the log file for details.${NC}"
        echo -e "📝 Log file: ${CYAN}$LOG_FILE${NC}"
        log_message "SUMMARY" "Some tests failed - Success rate: $success_rate%"
        exit 1
    fi
}

# Handle script interruption
trap 'echo -e "\n${YELLOW}⚠️  Testing interrupted by user${NC}"; exit 130' INT

# Run main function
main "$@"
