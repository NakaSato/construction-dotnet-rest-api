#!/bin/bash

# Configuration
API_BASE="http://localhost:5001/api/v1"
AUTH_ENDPOINT="$API_BASE/auth"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test results arrays
test_names=()
test_results=()
user_tokens=()

# Helper functions
print_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

add_test_result() {
    local test_name="$1"
    local result="$2"
    local token="$3"
    test_names+=("$test_name")
    test_results+=("$result")
    user_tokens+=("$token")
}

# Function to test API connectivity
test_api_connectivity() {
    print_step "Testing API connectivity..."
    
    local health_response=$(curl -s -w '%{http_code}' -o /dev/null "http://localhost:5001/health" --max-time 10)
    
    if [[ "$health_response" == "200" ]]; then
        print_success "API is reachable and healthy on port 5001"
        return 0
    else
        print_error "API is not reachable (HTTP $health_response). Make sure the container is running on port 5001."
        return 1
    fi
}

# Function to login a user and extract token
login_user() {
    local username="$1"
    local password="$2"
    local role_name="$3"
    
    print_step "Testing login for $role_name user: $username"
    
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"password\": \"$password\"
        }" \
        "$AUTH_ENDPOINT/login" --max-time 30)
    
    # Check if response contains success field
    local success=$(echo "$response" | jq -r '.success // false' 2>/dev/null)
    local token=""
    local user_info=""
    
    if [[ "$success" == "true" ]]; then
        token=$(echo "$response" | jq -r '.data.token // ""' 2>/dev/null)
        user_info=$(echo "$response" | jq -r '.data.user // {}' 2>/dev/null)
        local user_role=$(echo "$user_info" | jq -r '.roleName // "Unknown"' 2>/dev/null)
        local user_id=$(echo "$user_info" | jq -r '.userId // "Unknown"' 2>/dev/null)
        
        print_success "Login successful for $role_name user: $username"
        echo "  ├─ User ID: $user_id"
        echo "  ├─ Role: $user_role"
        echo "  └─ Token: ${token:0:50}..."
        
        add_test_result "Login $role_name ($username)" "PASS" "$token"
        return 0
    else
        local error_msg=$(echo "$response" | jq -r '.message // "Unknown error"' 2>/dev/null)
        print_error "Login failed for $role_name user: $username"
        echo "  └─ Error: $error_msg"
        
        add_test_result "Login $role_name ($username)" "FAIL" ""
        return 1
    fi
}

# Function to test authenticated endpoint
test_authenticated_endpoint() {
    local token="$1"
    local username="$2"
    local role_name="$3"
    
    if [[ -z "$token" ]]; then
        print_warning "Skipping authenticated test for $username (no token)"
        return 1
    fi
    
    print_step "Testing authenticated endpoint for $role_name user: $username"
    
    local response=$(curl -s -w '\n%{http_code}' -X GET \
        -H "Authorization: Bearer $token" \
        -H "Content-Type: application/json" \
        "http://localhost:5001/api/v1/projects" --max-time 30)
    
    local http_code=$(echo "$response" | tail -n1)
    local body=$(echo "$response" | head -n -1)
    
    if [[ "$http_code" == "200" ]]; then
        print_success "Authenticated request successful for $username"
        add_test_result "Auth Test $role_name ($username)" "PASS" ""
        return 0
    elif [[ "$http_code" == "403" ]]; then
        print_warning "Access denied for $username (expected for some roles)"
        add_test_result "Auth Test $role_name ($username)" "EXPECTED" ""
        return 0
    else
        print_error "Authenticated request failed for $username (HTTP $http_code)"
        echo "Response: $body"
        add_test_result "Auth Test $role_name ($username)" "FAIL" ""
        return 1
    fi
}

# Function to print test summary
print_test_summary() {
    echo ""
    echo "========================================"
    echo "           TEST SUMMARY"
    echo "========================================"
    
    local total_tests=${#test_names[@]}
    local passed_tests=0
    local expected_tests=0
    
    for i in "${!test_names[@]}"; do
        local test_name="${test_names[$i]}"
        local test_result="${test_results[$i]}"
        
        if [[ "$test_result" == "PASS" ]]; then
            echo -e "${GREEN}[PASS]${NC} $test_name"
            ((passed_tests++))
        elif [[ "$test_result" == "EXPECTED" ]]; then
            echo -e "${YELLOW}[EXPECTED]${NC} $test_name"
            ((expected_tests++))
        else
            echo -e "${RED}[FAIL]${NC} $test_name"
        fi
    done
    
    echo "========================================"
    echo "Total Tests: $total_tests"
    echo -e "Passed: ${GREEN}$passed_tests${NC}"
    echo -e "Expected: ${YELLOW}$expected_tests${NC}"
    echo -e "Failed: ${RED}$((total_tests - passed_tests - expected_tests))${NC}"
    echo "========================================"
    
    if [[ $((passed_tests + expected_tests)) -eq $total_tests ]]; then
        echo -e "${GREEN}All tests passed or behaved as expected! 🎉${NC}"
        return 0
    else
        echo -e "${RED}Some tests failed unexpectedly. Please check the output above.${NC}"
        return 1
    fi
}

# Main execution
main() {
    echo "========================================"
    echo "       Login Test for All Users"
    echo "========================================"
    echo ""
    
    # Test API connectivity first
    if ! test_api_connectivity; then
        print_error "Cannot proceed without API connectivity"
        exit 1
    fi
    
    echo ""
    print_step "Starting login tests for all user accounts..."
    echo ""
    
    # Test original accounts from documentation
    echo "--- Testing Original Test Accounts ---"
    login_user "test_admin" "Admin123!" "Admin"
    test_authenticated_endpoint "${user_tokens[-1]}" "test_admin" "Admin"
    echo ""
    
    login_user "test_manager" "Manager123!" "Manager"
    test_authenticated_endpoint "${user_tokens[-1]}" "test_manager" "Manager"
    echo ""
    
    login_user "test_user" "User123!" "User"
    test_authenticated_endpoint "${user_tokens[-1]}" "test_user" "User"
    echo ""
    
    login_user "test_viewer" "Viewer123!" "Viewer"
    test_authenticated_endpoint "${user_tokens[-1]}" "test_viewer" "Viewer"
    echo ""
    
    # Test newly created accounts (find the latest timestamp)
    echo "--- Testing Recently Created Accounts ---"
    
    # Get timestamp from the most recent registration
    timestamp=$(date +%s)
    
    # Try the timestamp from the previous registration (1750983264)
    login_user "admin_test_1750983264" "TestPassword123!" "Admin"
    test_authenticated_endpoint "${user_tokens[-1]}" "admin_test_1750983264" "Admin"
    echo ""
    
    login_user "manager_test_1750983264" "TestPassword123!" "Manager"
    test_authenticated_endpoint "${user_tokens[-1]}" "manager_test_1750983264" "Manager"
    echo ""
    
    login_user "user_test_1750983264" "TestPassword123!" "User"
    test_authenticated_endpoint "${user_tokens[-1]}" "user_test_1750983264" "User"
    echo ""
    
    login_user "viewer_test_1750983264" "TestPassword123!" "Viewer"
    test_authenticated_endpoint "${user_tokens[-1]}" "viewer_test_1750983264" "Viewer"
    echo ""
    
    # Print summary
    print_test_summary
    
    local exit_code=$?
    
    echo ""
    print_step "Login tests completed. Check the summary above for results."
    
    if [[ $exit_code -eq 0 ]]; then
        echo -e "${GREEN}All user accounts are working correctly!${NC}"
        echo ""
        echo "🔑 Available Accounts Summary:"
        echo "  Original Test Accounts:"
        echo "    - test_admin (Admin123!)"
        echo "    - test_manager (Manager123!)"
        echo "    - test_user (User123!)"
        echo "    - test_viewer (Viewer123!)"
        echo ""
        echo "  Recently Created Accounts:"
        echo "    - admin_test_1750983264 (TestPassword123!)"
        echo "    - manager_test_1750983264 (TestPassword123!)"
        echo "    - user_test_1750983264 (TestPassword123!)"
        echo "    - viewer_test_1750983264 (TestPassword123!)"
    else
        echo -e "${RED}Some login tests failed. Please review the API logs and fix any issues.${NC}"
    fi
    
    exit $exit_code
}

# Run main function
main "$@"
