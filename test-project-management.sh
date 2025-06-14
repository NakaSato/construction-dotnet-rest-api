#!/bin/bash

# =============================================================================
# Solar Projects REST API - Project Management Test Suite
# =============================================================================
# This script tests all Project Management API endpoints
# Usage: ./test-project-management.sh [base_url] [username] [password]
# =============================================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
API_BASE="${1:-http://localhost:5002}"
TEST_USERNAME="${2:-john_doe}"
TEST_PASSWORD="${3:-TestPass123!}"
CONTENT_TYPE="Content-Type: application/json"

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Helper functions
print_header() {
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

print_test() {
    echo -e "${CYAN}🧪 Testing: $1${NC}"
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

check_status() {
    local status=$1
    local expected=$2
    local test_name=$3
    
    if [ "$status" = "$expected" ]; then
        print_success "$test_name"
    else
        print_error "$test_name (Expected: $expected, Got: $status)"
        if [ "$4" != "" ]; then
            echo "Response: $4"
        fi
    fi
}

# Global variables
AUTH_TOKEN=""
USER_ID=""
PROJECT_ID=""
MANAGER_USER_ID=""

# Authentication function
authenticate() {
    print_header "AUTHENTICATION"
    
    print_test "User Login"
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "$CONTENT_TYPE" \
        -d "{\"username\":\"$TEST_USERNAME\",\"password\":\"$TEST_PASSWORD\"}")
    
    if [ $? -eq 0 ] && echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        AUTH_TOKEN=$(echo "$response" | jq -r '.data.token')
        USER_ID=$(echo "$response" | jq -r '.data.user.userId')
        print_success "User login"
        print_info "Auth token obtained: ${AUTH_TOKEN:0:20}..."
        print_info "User ID: $USER_ID"
        return 0
    else
        print_error "User login"
        echo "Response: $response"
        echo "Cannot proceed without authentication. Exiting..."
        exit 1
    fi
}

# Wait for API to be ready
wait_for_api() {
    print_info "Waiting for API to be ready..."
    for i in {1..30}; do
        if curl -f -s "$API_BASE/health" > /dev/null; then
            print_success "API is ready!"
            return 0
        fi
        echo -n "."
        sleep 1
    done
    print_error "API is not responding after 30 seconds"
    exit 1
}

# Test Project Management endpoints
test_project_management() {
    print_header "PROJECT MANAGEMENT TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping project management tests - no auth token"
        return
    fi
    
    # Test 1: Get All Projects
    print_test "Get All Projects"
    response=$(curl -s -X GET "$API_BASE/api/v1/projects" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if [ $? -eq 0 ] && echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Get all projects"
        
        # Extract a project ID for further tests
        PROJECT_ID=$(echo "$response" | jq -r '.data.items[0].projectId // empty')
        if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
            print_info "Found project ID: $PROJECT_ID"
        fi
        
        # Extract project manager ID for create tests
        MANAGER_USER_ID=$(echo "$response" | jq -r '.data.items[0].projectManager.userId // empty')
        if [ -n "$MANAGER_USER_ID" ] && [ "$MANAGER_USER_ID" != "null" ]; then
            print_info "Found manager ID: $MANAGER_USER_ID"
        else
            # Fallback to current user ID
            MANAGER_USER_ID="$USER_ID"
        fi
    else
        print_error "Get all projects"
        echo "Response: $response"
    fi
    
    # Test 2: Get All Projects with Pagination
    print_test "Get Projects with Pagination"
    response=$(curl -s -X GET "$API_BASE/api/v1/projects?pageNumber=1&pageSize=5" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if [ $? -eq 0 ] && echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        page_number=$(echo "$response" | jq -r '.data.pageNumber')
        page_size=$(echo "$response" | jq -r '.data.pageSize')
        if [ "$page_number" = "1" ] && [ "$page_size" = "5" ]; then
            print_success "Get projects with pagination"
        else
            print_error "Get projects with pagination (incorrect pagination values)"
        fi
    else
        print_error "Get projects with pagination"
        echo "Response: $response"
    fi
    
    # Test 3: Get Project by ID (if we have one)
    if [ -n "$PROJECT_ID" ] && [ "$PROJECT_ID" != "null" ]; then
        print_test "Get Project by ID"
        response=$(curl -s -X GET "$API_BASE/api/v1/projects/$PROJECT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        
        if [ $? -eq 0 ] && echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
            project_id_response=$(echo "$response" | jq -r '.data.projectId')
            if [ "$project_id_response" = "$PROJECT_ID" ]; then
                print_success "Get project by ID"
            else
                print_error "Get project by ID (ID mismatch)"
            fi
        else
            print_error "Get project by ID"
            echo "Response: $response"
        fi
    else
        print_warning "Skipping Get Project by ID - no project ID available"
    fi
    
    # Test 4: Create New Project
    print_test "Create New Project"
    current_date=$(date -u +"%Y-%m-%dT%H:%M:%S.000Z")
    future_date=$(date -u -d "+90 days" +"%Y-%m-%dT%H:%M:%S.000Z" 2>/dev/null || date -u -v+90d +"%Y-%m-%dT%H:%M:%S.000Z" 2>/dev/null)
    
    if [ -z "$future_date" ]; then
        future_date="2025-12-31T00:00:00.000Z"
    fi
    
    create_response=$(curl -s -X POST "$API_BASE/api/v1/projects" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d "{
            \"projectName\": \"Test Project - API Test Suite $(date +%s)\",
            \"address\": \"123 Test Street, Test City, TC 12345\",
            \"clientInfo\": \"Test Client Corp - Contact: Test Manager (555-TEST-001)\",
            \"startDate\": \"$current_date\",
            \"estimatedEndDate\": \"$future_date\",
            \"projectManagerId\": \"$MANAGER_USER_ID\"
        }")
    
    if [ $? -eq 0 ] && echo "$create_response" | jq -e '.success == true' > /dev/null 2>&1; then
        NEW_PROJECT_ID=$(echo "$create_response" | jq -r '.data.projectId')
        print_success "Create new project"
        print_info "Created project ID: $NEW_PROJECT_ID"
        
        # Test 5: Update the newly created project
        if [ -n "$NEW_PROJECT_ID" ] && [ "$NEW_PROJECT_ID" != "null" ]; then
            print_test "Update Project"
            update_response=$(curl -s -X PUT "$API_BASE/api/v1/projects/$NEW_PROJECT_ID" \
                -H "$CONTENT_TYPE" \
                -H "Authorization: Bearer $AUTH_TOKEN" \
                -d "{
                    \"projectName\": \"Updated Test Project - API Test Suite\",
                    \"address\": \"456 Updated Street, Updated City, UC 54321\",
                    \"clientInfo\": \"Updated Test Client Corp - Contact: Updated Manager (555-TEST-002)\",
                    \"status\": \"In Progress\",
                    \"startDate\": \"$current_date\",
                    \"estimatedEndDate\": \"$future_date\",
                    \"projectManagerId\": \"$MANAGER_USER_ID\"
                }")
            
            if [ $? -eq 0 ] && echo "$update_response" | jq -e '.success == true' > /dev/null 2>&1; then
                print_success "Update project"
            else
                print_error "Update project"
                echo "Response: $update_response"
            fi
            
            # Test 6: Delete the test project (cleanup)
            print_test "Delete Test Project"
            delete_status=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$API_BASE/api/v1/projects/$NEW_PROJECT_ID" \
                -H "Authorization: Bearer $AUTH_TOKEN")
            
            if [ "$delete_status" = "204" ] || [ "$delete_status" = "200" ]; then
                print_success "Delete test project"
            else
                print_error "Delete test project (Status: $delete_status)"
            fi
        fi
    else
        print_error "Create new project"
        echo "Response: $create_response"
        print_warning "Skipping update and delete tests - project creation failed"
    fi
    
    # Test 7: Filter Projects by Manager
    if [ -n "$MANAGER_USER_ID" ] && [ "$MANAGER_USER_ID" != "null" ]; then
        print_test "Filter Projects by Manager"
        response=$(curl -s -X GET "$API_BASE/api/v1/projects?managerId=$MANAGER_USER_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        
        if [ $? -eq 0 ] && echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
            print_success "Filter projects by manager"
        else
            print_error "Filter projects by manager"
            echo "Response: $response"
        fi
    else
        print_warning "Skipping Filter Projects by Manager - no manager ID available"
    fi
    
    # Test 8: Test Invalid Project ID
    print_test "Get Project with Invalid ID"
    invalid_response=$(curl -s -X GET "$API_BASE/api/v1/projects/00000000-0000-0000-0000-000000000000" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if [ $? -eq 0 ] && echo "$invalid_response" | jq -e '.success == false' > /dev/null 2>&1; then
        print_success "Get project with invalid ID (correctly returned error)"
    else
        print_error "Get project with invalid ID (should have returned error)"
    fi
    
    # Test 9: Test Unauthorized Access (without token)
    print_test "Unauthorized Access (No Token)"
    unauth_status=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_BASE/api/v1/projects")
    
    if [ "$unauth_status" = "401" ]; then
        print_success "Unauthorized access correctly blocked"
    else
        print_error "Unauthorized access (Expected: 401, Got: $unauth_status)"
    fi
    
    # Test 10: Test Rich Pagination Endpoint (if available)
    print_test "Rich Pagination Projects"
    rich_response=$(curl -s -X GET "$API_BASE/api/v1/projects/rich?pageSize=3" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if [ $? -eq 0 ] && echo "$rich_response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Rich pagination projects"
    else
        print_warning "Rich pagination projects (endpoint may not be available)"
    fi
}

# Test Project Statistics (if endpoint exists)
test_project_statistics() {
    print_header "PROJECT STATISTICS TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping project statistics tests - no auth token"
        return
    fi
    
    # Test project statistics endpoint if it exists
    print_test "Get Project Statistics"
    stats_response=$(curl -s -X GET "$API_BASE/api/v1/projects/statistics" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    
    if [ $? -eq 0 ] && echo "$stats_response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "Get project statistics"
    else
        print_warning "Get project statistics (endpoint may not be available)"
    fi
}

# Test error handling
test_error_handling() {
    print_header "ERROR HANDLING TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_warning "Skipping error handling tests - no auth token"
        return
    fi
    
    # Test 1: Invalid JSON in create request
    print_test "Create Project with Invalid JSON"
    invalid_response=$(curl -s -X POST "$API_BASE/api/v1/projects" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{"projectName": "Test", "invalid": json}')
    
    status=$(echo "$invalid_response" | jq -e '.success' 2>/dev/null)
    if [ "$status" = "false" ] || [ $? -ne 0 ]; then
        print_success "Create project with invalid JSON (correctly rejected)"
    else
        print_error "Create project with invalid JSON (should have been rejected)"
    fi
    
    # Test 2: Missing required fields
    print_test "Create Project with Missing Fields"
    missing_response=$(curl -s -X POST "$API_BASE/api/v1/projects" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{"projectName": ""}')
    
    if [ $? -eq 0 ] && echo "$missing_response" | jq -e '.success == false' > /dev/null 2>&1; then
        print_success "Create project with missing fields (correctly rejected)"
    else
        print_error "Create project with missing fields (should have been rejected)"
    fi
    
    # Test 3: Update non-existent project
    print_test "Update Non-existent Project"
    nonexistent_response=$(curl -s -X PUT "$API_BASE/api/v1/projects/99999999-9999-9999-9999-999999999999" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "projectName": "Updated Project",
            "address": "Test Address",
            "clientInfo": "Test Client"
        }')
    
    if [ $? -eq 0 ] && echo "$nonexistent_response" | jq -e '.success == false' > /dev/null 2>&1; then
        print_success "Update non-existent project (correctly returned error)"
    else
        print_error "Update non-existent project (should have returned error)"
    fi
}

# Main execution
main() {
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║          Solar Projects REST API - Project Management Tests     ║"
    echo "║                     Comprehensive Testing                       ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    
    print_info "API Base URL: $API_BASE"
    print_info "Test User: $TEST_USERNAME"
    print_info "Starting Project Management API tests..."
    
    # Wait for API and authenticate
    wait_for_api
    authenticate
    
    # Run all test suites
    test_project_management
    test_project_statistics
    test_error_handling
    
    # Print summary
    print_header "TEST SUMMARY"
    echo -e "${CYAN}Total Tests: $TOTAL_TESTS${NC}"
    echo -e "${GREEN}Passed: $PASSED_TESTS${NC}"
    echo -e "${RED}Failed: $FAILED_TESTS${NC}"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 All Project Management tests passed!${NC}"
        exit 0
    else
        echo -e "${YELLOW}⚠️  Some tests failed. Check the output above for details.${NC}"
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    echo "Error: curl is required but not installed."
    exit 1
fi

if ! command -v jq &> /dev/null; then
    echo "Error: jq is required but not installed."
    exit 1
fi

# Run main function
main "$@"
