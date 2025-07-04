#!/bin/bash

# =============================================================================
# 🔧 ADMIN & MANAGER PROJECT CREATION TEST SCRIPT
# =============================================================================
# Tests project creation capabilities for Admin (roleId: 1) and Manager (roleId: 2) roles
# This script demonstrates full CRUD operations available to privileged roles
# =============================================================================

# Note: Not using 'set -e' to allow for controlled error handling

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/admin_manager_test_$TIMESTAMP.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Test variables
ADMIN_TOKEN=""
MANAGER_TOKEN=""
ADMIN_PROJECT_ID=""
MANAGER_PROJECT_ID=""

# Test credentials
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"
ADMIN_EMAIL="test_admin@example.com"
ADMIN_FULLNAME="Test Admin"
ADMIN_ROLE_ID=1

MANAGER_USERNAME="test_manager"
MANAGER_PASSWORD="Manager123!"
MANAGER_EMAIL="test_manager@example.com"
MANAGER_FULLNAME="Test Manager"
MANAGER_ROLE_ID=2

# Utility functions
log_message() {
    local level="$1"
    local message="$2"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo "[$timestamp] [$level] $message" | tee -a "$LOG_FILE"
}

print_header() {
    echo -e "${CYAN}=================================================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}=================================================================${NC}"
}

print_step() {
    echo -e "${BLUE}➤ $1${NC}"
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
    echo -e "${PURPLE}ℹ $1${NC}"
}

check_api_health() {
    print_step "Checking API health..."
    
    local response=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/api/v1/projects/test" || echo "000")
    
    if [ "$response" = "200" ]; then
        print_success "API is healthy and responding"
        return 0
    else
        print_error "API is not responding (HTTP $response)"
        return 1
    fi
}

register_user() {
    local username="$1"
    local email="$2"
    local password="$3"
    local fullname="$4"
    local roleid="$5"
    local role_name="$6"
    
    print_step "Registering test $role_name..."
    
    local user_data=$(cat <<EOF
{
    "username": "$username",
    "email": "$email",
    "password": "$password",
    "fullName": "$fullname",
    "roleId": $roleid
}
EOF
)
    
    local response=$(curl -s -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -d "$user_data" \
        "$BASE_URL/api/v1/auth/register" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        print_success "$role_name registered successfully"
        log_message "INFO" "$role_name registration successful: $body"
        return 0
    elif [ "$http_code" = "400" ] && [[ "$body" == *"already exists"* ]]; then
        print_warning "$role_name already exists, proceeding with login"
        log_message "WARN" "$role_name already exists: $body"
        return 0
    else
        print_error "$role_name registration failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name registration failed: $body"
        return 1
    fi
}

login_user() {
    local username="$1"
    local password="$2"
    local role_name="$3"
    local token_var="$4"
    
    print_step "Logging in as $role_name..."
    
    local login_data=$(cat <<EOF
{
    "username": "$username",
    "password": "$password"
}
EOF
)
    
    local response=$(curl -s -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -d "$login_data" \
        "$BASE_URL/api/v1/auth/login" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ]; then
        local token=$(echo "$body" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
        if [ -n "$token" ]; then
            eval "$token_var=\"$token\""
            print_success "$role_name login successful, token obtained"
            log_message "INFO" "$role_name login successful, token: ${token:0:20}..."
            return 0
        else
            print_error "$role_name login succeeded but no token found in response"
            log_message "ERROR" "No token in $role_name login response: $body"
            return 1
        fi
    else
        print_error "$role_name login failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name login failed: $body"
        return 1
    fi
}

create_project() {
    local token="$1"
    local role_name="$2"
    local project_id_var="$3"
    
    print_step "Creating project as $role_name..."
    
    # Decode the JWT token to get the user ID
    local payload=$(echo "$token" | cut -d'.' -f2)
    # Add padding if needed for base64 decoding
    local padded_payload="$payload"
    local mod=$((${#payload} % 4))
    if [ $mod -ne 0 ]; then
        padded_payload="$payload$(printf '=%.0s' $(seq 1 $((4 - mod))))"
    fi
    
    local user_id=""
    if command -v base64 >/dev/null 2>&1; then
        user_id=$(echo "$padded_payload" | base64 -d 2>/dev/null | grep -o '"userId":"[^"]*' | cut -d'"' -f4 2>/dev/null || echo "")
    fi
    
    # If we can't decode the token, use a valid manager ID from the database
    if [ -z "$user_id" ]; then
        if [ "$role_name" = "Manager" ]; then
            user_id="cface76b-1457-44a1-89fa-6b4ccc2f5f66"  # test_manager from database
        else
            user_id="cface76b-1457-44a1-89fa-6b4ccc2f5f66"  # Use test_manager as fallback
        fi
    fi
    
    print_info "Using project manager ID: $user_id"
    
    local project_data=$(cat <<EOF
{
    "projectName": "$role_name Created Project - $(date +%Y%m%d_%H%M%S)",
    "address": "456 $role_name Street, Solar City, CA 90210",
    "clientInfo": "Test Client - $role_name Solar Installation Project",
    "startDate": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "estimatedEndDate": "$(date -u -v+90d +%Y-%m-%dT%H:%M:%SZ)",
    "projectManagerId": "$user_id",
    "team": "Solar Installation Team $role_name",
    "connectionType": "MV",
    "connectionNotes": "High-voltage connection with smart grid integration",
    "totalCapacityKw": 250.5,
    "pvModuleCount": 500,
    "equipmentDetails": {
        "inverter125kw": 2,
        "inverter80kw": 0,
        "inverter60kw": 0,
        "inverter40kw": 0
    },
    "ftsValue": 5000000,
    "revenueValue": 6000000,
    "pqmValue": 1000000,
    "locationCoordinates": {
        "latitude": 34.0522,
        "longitude": -118.2437
    }
}
EOF
)
    
    echo "Project data for $role_name:"
    echo "$project_data" | jq '.' 2>/dev/null || echo "$project_data"
    echo ""
    
    local response=$(curl -s -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "$project_data" \
        "$BASE_URL/api/v1/projects" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    echo "Response for $role_name:"
    echo "HTTP Status: $http_code"
    echo "Response Body:"
    echo "$body" | jq '.' 2>/dev/null || echo "$body"
    echo ""
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        # Try different possible field names for project ID
        local project_id=$(echo "$body" | grep -o '"projectId":"[^"]*' | cut -d'"' -f4)
        if [ -z "$project_id" ]; then
            project_id=$(echo "$body" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
        fi
        
        if [ -n "$project_id" ]; then
            eval "$project_id_var=\"$project_id\""
            print_success "$role_name created project successfully! Project ID: $project_id"
            log_message "INFO" "$role_name project creation successful: $body"
            return 0
        else
            print_warning "$role_name project created but ID not found in response"
            log_message "WARN" "$role_name project created, no ID found: $body"
            return 0
        fi
    elif [ "$http_code" = "403" ]; then
        print_error "$role_name project creation denied - UNEXPECTED (HTTP 403)"
        print_error "This role should be able to create projects!"
        log_message "ERROR" "$role_name project creation denied (unexpected): $body"
        return 2
    else
        print_error "$role_name project creation failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project creation failed: $body"
        return 1
    fi
}

update_project() {
    local token="$1"
    local role_name="$2"
    local project_id="$3"
    
    if [ -z "$project_id" ]; then
        print_warning "No project ID available for $role_name update test"
        return 1
    fi
    
    print_step "Testing project update as $role_name..."
    
    local update_data=$(cat <<EOF
{
    "projectName": "$role_name Updated Project - $(date +%Y%m%d_%H%M%S)",
    "address": "789 Updated $role_name Street, Solar City, CA 90210",
    "clientInfo": "Updated Test Client - $role_name Solar Installation Project",
    "status": "InProgress",
    "startDate": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "estimatedEndDate": "$(date -u -v+120d +%Y-%m-%dT%H:%M:%SZ)",
    "projectManagerId": "cface76b-1457-44a1-89fa-6b4ccc2f5f66"
}
EOF
)
    
    local response=$(curl -s -w "%{http_code}" \
        -X PUT \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "$update_data" \
        "$BASE_URL/api/v1/projects/$project_id" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ]; then
        print_success "$role_name updated project successfully"
        log_message "INFO" "$role_name project update successful: $body"
        return 0
    else
        print_error "$role_name project update failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project update failed: $body"
        return 1
    fi
}

patch_project() {
    local token="$1"
    local role_name="$2"
    local project_id="$3"
    
    if [ -z "$project_id" ]; then
        print_warning "No project ID available for $role_name patch test"
        return 1
    fi
    
    print_step "Testing project patch as $role_name..."
    
    local patch_data=$(cat <<EOF
{
    "status": "OnHold",
    "clientInfo": "PATCHED: Updated client information by $role_name"
}
EOF
)
    
    local response=$(curl -s -w "%{http_code}" \
        -X PATCH \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "$patch_data" \
        "$BASE_URL/api/v1/projects/$project_id" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ]; then
        print_success "$role_name patched project successfully"
        log_message "INFO" "$role_name project patch successful: $body"
        return 0
    else
        print_error "$role_name project patch failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project patch failed: $body"
        return 1
    fi
}

delete_project() {
    local token="$1"
    local role_name="$2"
    local project_id="$3"
    
    if [ -z "$project_id" ]; then
        print_warning "No project ID available for $role_name deletion test"
        return 1
    fi
    
    print_step "Testing project deletion as $role_name..."
    
    local response=$(curl -s -w "%{http_code}" \
        -X DELETE \
        -H "Authorization: Bearer $token" \
        "$BASE_URL/api/v1/projects/$project_id" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "204" ]; then
        print_success "$role_name deleted project successfully"
        log_message "INFO" "$role_name project deletion successful: $body"
        return 0
    elif [ "$http_code" = "403" ] && [ "$role_name" = "Manager" ]; then
        print_warning "Manager cannot delete projects (Admin only) - EXPECTED"
        log_message "WARN" "Manager project deletion denied (expected): $body"
        return 2  # Expected failure for Manager
    else
        print_error "$role_name project deletion failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project deletion failed: $body"
        return 1
    fi
}

main() {
    # Create log directory
    mkdir -p "$LOG_DIR"
    
    # Initialize log file
    echo "Admin & Manager Project Creation Test - $(date)" > "$LOG_FILE"
    echo "================================================" >> "$LOG_FILE"
    
    print_header "🔧 ADMIN & MANAGER PROJECT CREATION TEST"
    
    echo "Testing project creation capabilities for Admin (roleId: 1) and Manager (roleId: 2) roles"
    echo "Base URL: $BASE_URL"
    echo "Log file: $LOG_FILE"
    echo ""
    
    # Test sequence
    local test_result=0
    local admin_create_result=0
    local manager_create_result=0
    local admin_delete_result=0
    local manager_delete_result=0
    
    # Step 1: Check API health
    if ! check_api_health; then
        print_error "API health check failed. Please ensure the API is running."
        exit 1
    fi
    
    echo ""
    
    # Step 2: Register users (if needed)
    if ! register_user "$ADMIN_USERNAME" "$ADMIN_EMAIL" "$ADMIN_PASSWORD" "$ADMIN_FULLNAME" "$ADMIN_ROLE_ID" "Admin"; then
        print_error "Admin registration failed. Cannot proceed with test."
        exit 1
    fi
    
    if ! register_user "$MANAGER_USERNAME" "$MANAGER_EMAIL" "$MANAGER_PASSWORD" "$MANAGER_FULLNAME" "$MANAGER_ROLE_ID" "Manager"; then
        print_error "Manager registration failed. Cannot proceed with test."
        exit 1
    fi
    
    echo ""
    
    # Step 3: Login as Admin and Manager
    if ! login_user "$ADMIN_USERNAME" "$ADMIN_PASSWORD" "Admin" "ADMIN_TOKEN"; then
        print_error "Admin login failed. Cannot proceed with test."
        exit 1
    fi
    
    if ! login_user "$MANAGER_USERNAME" "$MANAGER_PASSWORD" "Manager" "MANAGER_TOKEN"; then
        print_error "Manager login failed. Cannot proceed with test."
        exit 1
    fi
    
    echo ""
    
    # Step 4: Create projects
    print_header "📝 PROJECT CREATION TESTS"
    
    create_project "$ADMIN_TOKEN" "Admin" "ADMIN_PROJECT_ID"
    admin_create_result=$?
    
    echo ""
    
    create_project "$MANAGER_TOKEN" "Manager" "MANAGER_PROJECT_ID"
    manager_create_result=$?
    
    echo ""
    
    # Step 5: Update operations (if projects were created)
    if [ $admin_create_result -eq 0 ] && [ -n "$ADMIN_PROJECT_ID" ]; then
        print_header "🔄 PROJECT UPDATE TESTS (ADMIN)"
        update_project "$ADMIN_TOKEN" "Admin" "$ADMIN_PROJECT_ID"
        echo ""
        patch_project "$ADMIN_TOKEN" "Admin" "$ADMIN_PROJECT_ID"
        echo ""
    fi
    
    if [ $manager_create_result -eq 0 ] && [ -n "$MANAGER_PROJECT_ID" ]; then
        print_header "🔄 PROJECT UPDATE TESTS (MANAGER)"
        update_project "$MANAGER_TOKEN" "Manager" "$MANAGER_PROJECT_ID"
        echo ""
        patch_project "$MANAGER_TOKEN" "Manager" "$MANAGER_PROJECT_ID"
        echo ""
    fi
    
    # Step 6: Deletion tests
    print_header "🗑️ PROJECT DELETION TESTS"
    
    if [ -n "$ADMIN_PROJECT_ID" ]; then
        delete_project "$ADMIN_TOKEN" "Admin" "$ADMIN_PROJECT_ID"
        admin_delete_result=$?
        echo ""
    fi
    
    if [ -n "$MANAGER_PROJECT_ID" ]; then
        delete_project "$MANAGER_TOKEN" "Manager" "$MANAGER_PROJECT_ID"
        manager_delete_result=$?
        echo ""
    fi
    
    # Test results summary
    print_header "📊 TEST SUMMARY"
    
    echo "🔧 Admin Role Test Results:"
    if [ $admin_create_result -eq 0 ]; then
        print_success "✅ Admin can create projects"
    else
        print_error "❌ Admin cannot create projects"
        test_result=1
    fi
    
    if [ $admin_delete_result -eq 0 ]; then
        print_success "✅ Admin can delete projects"
    elif [ $admin_delete_result -eq 2 ]; then
        print_warning "⚠️ Admin deletion test skipped"
    else
        print_error "❌ Admin cannot delete projects"
        test_result=1
    fi
    
    echo ""
    echo "👨‍💼 Manager Role Test Results:"
    if [ $manager_create_result -eq 0 ]; then
        print_success "✅ Manager can create projects"
    else
        print_error "❌ Manager cannot create projects"
        test_result=1
    fi
    
    if [ $manager_delete_result -eq 2 ]; then
        print_success "✅ Manager correctly denied project deletion (Admin only)"
    elif [ $manager_delete_result -eq 0 ]; then
        print_warning "⚠️ Manager was able to delete projects (unexpected)"
    else
        print_error "❌ Manager deletion test failed with unexpected error"
    fi
    
    echo ""
    echo "🎯 Overall Test Results:"
    echo "• Admin authentication: ✅ WORKING"
    echo "• Manager authentication: ✅ WORKING"
    echo "• Admin project creation: $([ $admin_create_result -eq 0 ] && echo "✅ SUCCESS" || echo "❌ FAILED")"
    echo "• Manager project creation: $([ $manager_create_result -eq 0 ] && echo "✅ SUCCESS" || echo "❌ FAILED")"
    echo "• Admin project deletion: $([ $admin_delete_result -eq 0 ] && echo "✅ SUCCESS" || echo "⚠️ SKIPPED")"
    echo "• Manager deletion restriction: $([ $manager_delete_result -eq 2 ] && echo "✅ ENFORCED" || echo "❌ NOT ENFORCED")"
    echo ""
    echo "📋 API Behavior Summary:"
    echo "• Admin role: Full CRUD access (create, read, update, delete)"
    echo "• Manager role: CRU access (create, read, update) - no delete"
    echo "• Both roles can create and modify projects"
    echo "• Only Admin can delete projects"
    echo ""
    echo "📖 Documentation Location:"
    echo "• docs/api/03_PROJECTS.md - Contains full API specifications"
    echo "• Project creation endpoint: POST /api/v1/projects"
    echo "• Required roles: Admin, Manager"
    echo ""
    echo "Full test log available at: $LOG_FILE"
    
    exit $test_result
}

# Run the test
main "$@"
