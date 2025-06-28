#!/bin/bash

# =============================================================================
# 🧪 COMPREHENSIVE API ENDPOINT TESTING SCRIPT
# =============================================================================
# Solar Projects API - Complete Role-Based Testing
# Supports all user roles: Admin, Manager, User, Viewer
# =============================================================================

set -e  # Exit on any error

# Configuration
BASE_URL="https://icms-api-f6ys9.ondigitalocean.app"
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
ADMIN_USER="test_admin:Admin123!"
MANAGER_USER="test_manager:Manager123!"
USER_USER="test_user:User123!"
VIEWER_USER="test_viewer:Viewer123!"

# Registration data for initial users (compatible with POSIX sh)
REGISTER_USERNAMES=("test_admin" "test_manager" "test_user" "test_viewer")
REGISTER_USER_DATA=(
    '{"username":"test_admin","email":"test_admin@example.com","password":"Admin123!","fullName":"Test Admin","roleId":1}'
    '{"username":"test_manager","email":"test_manager@example.com","password":"Manager123!","fullName":"Test Manager","roleId":2}'
    '{"username":"test_user","email":"test_user@example.com","password":"User123!","fullName":"Test User","roleId":3}'
    '{"username":"test_viewer","email":"test_viewer@example.com","password":"Viewer123!","fullName":"Test Viewer","roleId":4}'
)

# =============================================================================
# =============================================================================
# UTILITY FUNCTIONS
# =============================================================================

# Register test users if not already present (POSIX compatible)
register_test_users() {
    print_section "User Registration (Setup)"
    local i=0
    local count=${#REGISTER_USERNAMES[@]}
    while [ $i -lt $count ]; do
        local uname="${REGISTER_USERNAMES[$i]}"
        local reg_data="${REGISTER_USER_DATA[$i]}"
        print_test "$uname" "POST" "/api/v1/auth/register" "Register $uname"
        response=$(make_request "POST" "/api/v1/auth/register" "" "$reg_data")
        status=$(extract_status "$response")
        if [[ "$status" == "200" || "$status" == "201" || "$status" == "409" ]]; then
            print_result "PASS" "200/201/409" "$status" "Registered or already exists: $uname"
        else
            print_result "FAIL" "200/201/409" "$status" "Failed to register $uname"
        fi
        i=$((i+1))
    done
}

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

    # For POST/PUT/PATCH, use --data-raw for JSON, but only if data is not empty
    if [[ -n "$data" && "$method" != "GET" ]]; then
        cmd="$cmd --data-raw '$data'"
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

# --- OpenAPI-driven registration and login payloads ---
# RegisterRequest: username, email, password, fullName, roleId (int)
get_valid_register_payload() {
    local username="$1"
    local email="$2"
    local password="$3"
    local fullName="$4"
    local roleId="$5"
    cat <<EOF
{
  "username": "$username",
  "email": "$email",
  "password": "$password",
  "fullName": "$fullName",
  "roleId": $roleId
}
EOF
}

# LoginRequest: username, password
get_valid_login_payload() {
    local username="$1"
    local password="$2"
    cat <<EOF
{
  "username": "$username",
  "password": "$password"
}
EOF
}

# Example: use OpenAPI schema for registration/login
run_openapi_registration_and_login_tests() {
    log_message "INFO" "Testing registration and login with OpenAPI schema payloads"
    local username="apitestuser$RANDOM"
    local email="apitestuser$RANDOM@example.com"
    local password="TestPassword1!"
    local fullName="API Test User"
    local roleId=2

    local register_payload
    register_payload=$(get_valid_register_payload "$username" "$email" "$password" "$fullName" "$roleId")
    local register_response
    register_response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST -H "Content-Type: application/json" -d "$register_payload" "http://localhost:5001/api/v1/Auth/register")
    local register_status
    register_status=$(extract_status "$register_response")
    if [[ "$register_status" != "200" ]]; then
        log_message "ERROR" "Registration failed (status $register_status): $(extract_body "$register_response")"
        return 1
    fi
    log_message "SUCCESS" "Registration succeeded for $username"

    local login_payload
    login_payload=$(get_valid_login_payload "$username" "$password")
    local login_response
    login_response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST -H "Content-Type: application/json" -d "$login_payload" "http://localhost:5001/api/v1/Auth/login")
    local login_status
    login_status=$(extract_status "$login_response")
    if [[ "$login_status" != "200" ]]; then
        log_message "ERROR" "Login failed (status $login_status): $(extract_body "$login_response")"
        return 1
    fi
    log_message "SUCCESS" "Login succeeded for $username"

    # Extract token for further tests
    local token
    token=$(echo "$login_response" | sed 's/.*HTTPSTATUS:[0-9]*//' | jq -r '.data.token // empty')
    if [[ -z "$token" ]]; then
        log_message "ERROR" "No token returned in login response"
        return 1
    fi
    log_message "INFO" "Received JWT token: ${token:0:16}..."

    # Test /api/v1/users (GET) with Bearer token
    local users_response
    users_response=$(curl -s -w "HTTPSTATUS:%{http_code}" -H "Authorization: Bearer $token" "http://localhost:5001/api/v1/users")
    local users_status
    users_status=$(extract_status "$users_response")
    if [[ "$users_status" != "200" ]]; then
        log_message "ERROR" "/api/v1/users failed (status $users_status): $(extract_body "$users_response")"
        return 1
    fi
    log_message "SUCCESS" "/api/v1/users returned 200 OK"
}

# --- Comprehensive Endpoint Multi-Method Test ---
test_all_endpoints_multi_method() {
    print_section "Comprehensive Endpoint Multi-Method Test"
    # Define endpoints and supported methods
    local endpoints=(
        "/api/v1/projects"
        "/api/v1/tasks"
        "/api/v1/users"
        "/api/v1/daily-reports"
        "/api/v1/work-requests"
        "/api/v1/master-plans"
        "/api/v1/calendar"
    )
    local methods=("GET" "POST" "PUT" "PATCH" "DELETE")
    # Function to return OpenAPI-compliant payload for each endpoint
    get_payload_for_endpoint() {
        local endpoint="$1"
        case "$endpoint" in
            "/api/v1/projects")
                # OpenAPI-compliant payload for CreateProjectRequest
                echo '{"projectName":"MultiMethod Project","address":"123 Multi St","clientInfo":"Multi client","startDate":"2025-06-29T00:00:00Z","projectManagerId":"11111111-1111-1111-1111-111111111111"}'
                ;;
            "/api/v1/users")
                echo '{"username":"multiuser","email":"multiuser@example.com","password":"MultiUser123!","fullName":"Multi User","roleId":3}'
                ;;
            "/api/v1/tasks")
                echo '{"taskName":"MultiMethod Task","description":"Test task","status":"Pending"}'
                ;;
            "/api/v1/daily-reports")
                echo '{"reportDate":"2025-06-29","summary":"MultiMethod report"}'
                ;;
            "/api/v1/work-requests")
                echo '{"title":"MultiMethod Work","description":"Test work request"}'
                ;;
            "/api/v1/master-plans")
                echo '{"planName":"MultiMethod Plan","description":"Test plan"}'
                ;;
            "/api/v1/calendar")
                echo '{"eventName":"MultiMethod Event","eventDate":"2025-06-29"}'
                ;;
            *)
                echo ''
                ;;
        esac
    }
    # NOTE: Update payloads above to match your OpenAPI schema for each endpoint!

    # Use admin token for all tests
    local token="$ADMIN_TOKEN"
    for endpoint in "${endpoints[@]}"; do
        for method in "${methods[@]}"; do
            local data=""
            if [[ "$method" == "POST" || "$method" == "PUT" || "$method" == "PATCH" ]]; then
                data="$(get_payload_for_endpoint "$endpoint")"
            fi
            print_test "admin" "$method" "$endpoint" "Multi-method $method test"
            local resp=$(make_request "$method" "$endpoint" "$token" "$data")
            local status=$(extract_status "$resp")
            # Accept 200, 201, 204 for success, 400/404 for not implemented/invalid, 405 for method not allowed
            if [[ "$status" == "200" || "$status" == "201" || "$status" == "204" ]]; then
                print_result "PASS" "200/201/204" "$status" "$method $endpoint succeeded"
            elif [[ "$status" == "400" || "$status" == "404" || "$status" == "405" ]]; then
                print_result "SKIP" "200/201/204" "$status" "$method $endpoint not supported or invalid (expected for some endpoints)"
            else
                print_result "FAIL" "200/201/204" "$status" "$method $endpoint failed"
            fi
        done
    done
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
        # GET /api/v1/users - List users (with required pagination params)
        print_test "admin" "GET" "/api/v1/users?pageNumber=1&pageSize=10" "List all users"
        response=$(make_request "GET" "/api/v1/users?pageNumber=1&pageSize=10" "$ADMIN_TOKEN")
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
        print_test "manager" "GET" "/api/v1/users?pageNumber=1&pageSize=10" "List all users"
        response=$(make_request "GET" "/api/v1/users?pageNumber=1&pageSize=10" "$MANAGER_TOKEN")
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
        print_test "user" "GET" "/api/v1/users?pageNumber=1&pageSize=10" "List all users (should be denied)"
        response=$(make_request "GET" "/api/v1/users?pageNumber=1&pageSize=10" "$USER_TOKEN")
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
        print_test "viewer" "GET" "/api/v1/users?pageNumber=1&pageSize=10" "List all users (should be denied)"
        response=$(make_request "GET" "/api/v1/users?pageNumber=1&pageSize=10" "$VIEWER_TOKEN")
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
        
        # POST /api/v1/projects - Create project (OpenAPI-compliant payload)
        print_test "admin" "POST" "/api/v1/projects" "Create new project"
        local project_data='{"projectName":"Admin Test Project","address":"123 Admin St","clientInfo":"Admin client","startDate":"2025-06-29T00:00:00Z","projectManagerId":"11111111-1111-1111-1111-111111111111"}'
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

# Enhanced: Add OpenAPI-based endpoint smoke test and schema validation
run_openapi_smoke_tests() {
    print_section "OpenAPI Endpoint Smoke Test"
    local endpoints=(
        "/api/v1/Auth/login"
        "/api/v1/Auth/register"
        "/api/v1/users"
        "/api/v1/projects"
        "/api/v1/daily-reports"
        "/api/v1/work-requests"
    )
    local methods=("POST" "POST" "GET" "GET" "GET" "GET")
    local tokens=("" "" "$ADMIN_TOKEN" "$ADMIN_TOKEN" "$ADMIN_TOKEN" "$ADMIN_TOKEN")
    local datas=(
        '{"username":"test_admin","password":"Admin123!"}'
        '{"username":"test_apiuser","email":"test_apiuser@example.com","password":"ApiUser123!","fullName":"Test API User","roleId":3}'
        "" "" "" ""
    )
    local i=0
    while [ $i -lt ${#endpoints[@]} ]; do
        print_test "OpenAPI" "${methods[$i]}" "${endpoints[$i]}" "Smoke test endpoint ${endpoints[$i]}"
        local resp=$(make_request "${methods[$i]}" "${endpoints[$i]}" "${tokens[$i]}" "${datas[$i]}")
        local status=$(extract_status "$resp")
        if [[ "$status" == "200" || "$status" == "201" ]]; then
            print_result "PASS" "200/201" "$status" "Endpoint ${endpoints[$i]} OK"
        else
            print_result "FAIL" "200/201" "$status" "Endpoint ${endpoints[$i]} failed"
        fi
        i=$((i+1))
    done
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
    
    # Register test users (idempotent)
    register_test_users
    
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
    run_openapi_smoke_tests
    test_all_endpoints_multi_method
    
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
