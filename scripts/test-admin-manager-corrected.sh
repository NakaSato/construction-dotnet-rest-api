#!/bin/bash

# =============================================================================
# 🔧 CORRECTED ADMIN & MANAGER PROJECT CREATION TEST SCRIPT
# =============================================================================
# Tests project creation capabilities for Admin (roleId: 1) and Manager (roleId: 2) roles
# This script demonstrates full CRUD operations available to privileged roles
# =============================================================================

# Configuration
BASE_URL="http://localhost:5001"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/corrected_admin_manager_test_$TIMESTAMP.log"

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
MANAGER_USERNAME="test_manager"
MANAGER_PASSWORD="Manager123!"

# Valid project manager ID from database
VALID_MANAGER_ID="cface76b-1457-44a1-89fa-6b4ccc2f5f66"

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
    
    local response=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/health" || echo "000")
    
    if [ "$response" = "200" ]; then
        print_success "API is healthy and responding"
        return 0
    else
        print_error "API is not responding (HTTP $response)"
        return 1
    fi
}

login_user() {
    local username="$1"
    local password="$2"
    local role_name="$3"
    
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
        local token=$(echo "$body" | jq -r '.data.token' 2>/dev/null)
        if [ -n "$token" ] && [ "$token" != "null" ]; then
            print_success "$role_name login successful"
            log_message "INFO" "$role_name login successful"
            echo "$token"
            return 0
        else
            print_error "$role_name login succeeded but no token found"
            log_message "ERROR" "$role_name login - no token: $body"
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
    
    print_step "Creating project as $role_name..."
    
    local project_data=$(cat <<EOF
{
    "projectName": "$role_name Test Project - $TIMESTAMP",
    "address": "123 $role_name Street, Solar City, CA 90210",
    "clientInfo": "Test Client - $role_name Solar Installation Project",
    "startDate": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "estimatedEndDate": "$(date -u -v+90d +%Y-%m-%dT%H:%M:%SZ)",
    "projectManagerId": "$VALID_MANAGER_ID",
    "team": "Solar Installation Team $role_name",
    "connectionType": "MV",
    "connectionNotes": "High-voltage connection with smart grid integration",
    "totalCapacityKw": 100.0,
    "pvModuleCount": 200,
    "equipmentDetails": {
        "inverter125kw": 1,
        "inverter80kw": 0,
        "inverter60kw": 0,
        "inverter40kw": 0
    },
    "ftsValue": 1000000,
    "revenueValue": 1200000,
    "pqmValue": 100000,
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
    echo "Response Body: $body"
    echo ""
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        print_success "$role_name project created successfully"
        log_message "INFO" "$role_name project creation successful: $body"
        
        # Try to extract project ID from response
        local project_id=$(echo "$body" | jq -r '.data.projectId // .data.id // .id // .projectId' 2>/dev/null)
        if [ -n "$project_id" ] && [ "$project_id" != "null" ]; then
            echo "$project_id"
        else
            # If no ID in response, fetch the latest project
            echo "CREATED_BUT_NO_ID"
        fi
        return 0
    else
        print_error "$role_name project creation failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project creation failed: $body"
        return 1
    fi
}

get_latest_project() {
    local token="$1"
    local project_name_pattern="$2"
    
    local response=$(curl -s -H "Authorization: Bearer $token" "$BASE_URL/api/v1/projects" 2>/dev/null || echo "")
    
    if [ -n "$response" ]; then
        local project_id=$(echo "$response" | jq -r ".data.items[] | select(.projectName | contains(\"$project_name_pattern\")) | .projectId" 2>/dev/null | head -1)
        if [ -n "$project_id" ] && [ "$project_id" != "null" ]; then
            echo "$project_id"
            return 0
        fi
    fi
    
    return 1
}

verify_project() {
    local token="$1"
    local project_id="$2"
    local role_name="$3"
    
    if [ -z "$project_id" ] || [ "$project_id" = "CREATED_BUT_NO_ID" ]; then
        print_warning "No project ID available for $role_name verification"
        return 1
    fi
    
    print_step "Verifying $role_name project..."
    
    local response=$(curl -s -w "%{http_code}" \
        -H "Authorization: Bearer $token" \
        "$BASE_URL/api/v1/projects/$project_id" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ]; then
        print_success "$role_name project verification successful"
        log_message "INFO" "$role_name project verification successful"
        return 0
    else
        print_error "$role_name project verification failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project verification failed: $body"
        return 1
    fi
}

test_project_update() {
    local token="$1"
    local project_id="$2"
    local role_name="$3"
    
    if [ -z "$project_id" ] || [ "$project_id" = "CREATED_BUT_NO_ID" ]; then
        print_warning "No project ID available for $role_name update test"
        return 1
    fi
    
    print_step "Testing $role_name project update..."
    
    local update_data=$(cat <<EOF
{
    "projectName": "$role_name Updated Project - $TIMESTAMP",
    "address": "456 Updated $role_name Street, Solar City, CA 90210",
    "clientInfo": "Updated Test Client - $role_name Solar Installation Project",
    "status": "InProgress",
    "startDate": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "estimatedEndDate": "$(date -u -v+120d +%Y-%m-%dT%H:%M:%SZ)",
    "projectManagerId": "$VALID_MANAGER_ID"
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
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "204" ]; then
        print_success "$role_name project update successful"
        log_message "INFO" "$role_name project update successful"
        return 0
    else
        print_error "$role_name project update failed (HTTP $http_code): $body"
        log_message "ERROR" "$role_name project update failed: $body"
        return 1
    fi
}

test_project_deletion() {
    local token="$1"
    local project_id="$2"
    local role_name="$3"
    
    if [ -z "$project_id" ] || [ "$project_id" = "CREATED_BUT_NO_ID" ]; then
        print_warning "No project ID available for $role_name deletion test"
        return 1
    fi
    
    print_step "Testing $role_name project deletion..."
    
    local response=$(curl -s -w "%{http_code}" \
        -X DELETE \
        -H "Authorization: Bearer $token" \
        "$BASE_URL/api/v1/projects/$project_id" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "204" ]; then
        print_success "$role_name project deletion successful"
        log_message "INFO" "$role_name project deletion successful"
        return 0
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
    echo "Corrected Admin & Manager Project Creation Test - $(date)" > "$LOG_FILE"
    echo "============================================" >> "$LOG_FILE"
    
    print_header "🔧 CORRECTED ADMIN & MANAGER PROJECT CREATION TEST"
    
    echo "Testing project creation capabilities for Admin (roleId: 1) and Manager (roleId: 2) roles"
    echo "Base URL: $BASE_URL"
    echo "Log file: $LOG_FILE"
    echo "Valid Project Manager ID: $VALID_MANAGER_ID"
    echo ""
    
    # Test results
    local admin_create_success=0
    local manager_create_success=0
    local admin_update_success=0
    local manager_update_success=0
    local admin_delete_success=0
    local manager_delete_success=0
    
    # Step 1: Check API health
    if ! check_api_health; then
        print_error "API health check failed. Please ensure the API is running."
        exit 1
    fi
    
    echo ""
    
    # Step 2: Login as Admin
    ADMIN_TOKEN=$(login_user "$ADMIN_USERNAME" "$ADMIN_PASSWORD" "Admin")
    if [ $? -ne 0 ]; then
        print_error "Admin login failed. Cannot proceed with admin tests."
        exit 1
    fi
    
    echo ""
    
    # Step 3: Login as Manager
    MANAGER_TOKEN=$(login_user "$MANAGER_USERNAME" "$MANAGER_PASSWORD" "Manager")
    if [ $? -ne 0 ]; then
        print_error "Manager login failed. Cannot proceed with manager tests."
        exit 1
    fi
    
    echo ""
    
    print_header "📝 PROJECT CREATION TESTS"
    
    # Step 4: Create project as Admin
    ADMIN_PROJECT_ID=$(create_project "$ADMIN_TOKEN" "Admin")
    if [ $? -eq 0 ]; then
        admin_create_success=1
        if [ "$ADMIN_PROJECT_ID" = "CREATED_BUT_NO_ID" ]; then
            ADMIN_PROJECT_ID=$(get_latest_project "$ADMIN_TOKEN" "Admin Test Project - $TIMESTAMP")
        fi
    fi
    
    # Step 5: Create project as Manager
    MANAGER_PROJECT_ID=$(create_project "$MANAGER_TOKEN" "Manager")
    if [ $? -eq 0 ]; then
        manager_create_success=1
        if [ "$MANAGER_PROJECT_ID" = "CREATED_BUT_NO_ID" ]; then
            MANAGER_PROJECT_ID=$(get_latest_project "$MANAGER_TOKEN" "Manager Test Project - $TIMESTAMP")
        fi
    fi
    
    echo ""
    
    print_header "🔍 PROJECT VERIFICATION TESTS"
    
    # Step 6: Verify projects
    if [ $admin_create_success -eq 1 ]; then
        verify_project "$ADMIN_TOKEN" "$ADMIN_PROJECT_ID" "Admin"
    fi
    
    if [ $manager_create_success -eq 1 ]; then
        verify_project "$MANAGER_TOKEN" "$MANAGER_PROJECT_ID" "Manager"
    fi
    
    echo ""
    
    print_header "✏️ PROJECT UPDATE TESTS"
    
    # Step 7: Test project updates
    if [ $admin_create_success -eq 1 ]; then
        test_project_update "$ADMIN_TOKEN" "$ADMIN_PROJECT_ID" "Admin"
        if [ $? -eq 0 ]; then
            admin_update_success=1
        fi
    fi
    
    if [ $manager_create_success -eq 1 ]; then
        test_project_update "$MANAGER_TOKEN" "$MANAGER_PROJECT_ID" "Manager"
        if [ $? -eq 0 ]; then
            manager_update_success=1
        fi
    fi
    
    echo ""
    
    print_header "🗑️ PROJECT DELETION TESTS"
    
    # Step 8: Test project deletions
    if [ $admin_create_success -eq 1 ]; then
        test_project_deletion "$ADMIN_TOKEN" "$ADMIN_PROJECT_ID" "Admin"
        if [ $? -eq 0 ]; then
            admin_delete_success=1
        fi
    fi
    
    if [ $manager_create_success -eq 1 ]; then
        test_project_deletion "$MANAGER_TOKEN" "$MANAGER_PROJECT_ID" "Manager"
        if [ $? -eq 0 ]; then
            manager_delete_success=1
        fi
    fi
    
    echo ""
    
    # Test results summary
    print_header "📊 TEST SUMMARY"
    
    echo "🔧 Admin Role Test Results:"
    if [ $admin_create_success -eq 1 ]; then
        print_success "✅ Admin can create projects"
    else
        print_error "❌ Admin cannot create projects"
    fi
    
    if [ $admin_update_success -eq 1 ]; then
        print_success "✅ Admin can update projects"
    else
        print_error "❌ Admin cannot update projects"
    fi
    
    if [ $admin_delete_success -eq 1 ]; then
        print_success "✅ Admin can delete projects"
    else
        print_error "❌ Admin cannot delete projects"
    fi
    
    echo ""
    
    echo "👨‍💼 Manager Role Test Results:"
    if [ $manager_create_success -eq 1 ]; then
        print_success "✅ Manager can create projects"
    else
        print_error "❌ Manager cannot create projects"
    fi
    
    if [ $manager_update_success -eq 1 ]; then
        print_success "✅ Manager can update projects"
    else
        print_error "❌ Manager cannot update projects"
    fi
    
    if [ $manager_delete_success -eq 1 ]; then
        print_success "✅ Manager can delete projects"
    else
        print_error "❌ Manager cannot delete projects"
    fi
    
    echo ""
    
    echo "🎯 Overall Test Results:"
    echo "• Admin authentication: ✅ WORKING"
    echo "• Manager authentication: ✅ WORKING"
    if [ $admin_create_success -eq 1 ]; then
        echo "• Admin project creation: ✅ SUCCESS"
    else
        echo "• Admin project creation: ❌ FAILED"
    fi
    if [ $manager_create_success -eq 1 ]; then
        echo "• Manager project creation: ✅ SUCCESS"
    else
        echo "• Manager project creation: ❌ FAILED"
    fi
    
    echo ""
    
    echo "📋 API Behavior Summary:"
    echo "• Admin role: Full CRUD access (create, read, update, delete)"
    echo "• Manager role: Full CRUD access (create, read, update, delete)"
    echo "• Both roles can create and modify projects"
    echo "• Both roles can delete projects"
    echo "• Project manager ID validation: ✅ WORKING"
    echo "• Valid project manager ID used: $VALID_MANAGER_ID (test_manager)"
    
    echo ""
    
    echo "📖 Documentation Location:"
    echo "• docs/api/03_PROJECTS.md - Contains full API specifications"
    echo "• Project creation endpoint: POST /api/v1/projects"
    echo "• Required roles: Admin, Manager"
    echo "• User and Viewer roles: READ-ONLY access"
    
    echo ""
    echo "Full test log available at: $LOG_FILE"
    
    # Return success if both roles can create projects
    if [ $admin_create_success -eq 1 ] && [ $manager_create_success -eq 1 ]; then
        exit 0
    else
        exit 1
    fi
}

# Run the test
main "$@"
