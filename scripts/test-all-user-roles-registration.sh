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
    test_names+=("$test_name")
    test_results+=("$result")
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

# Function to register a user
register_user() {
    local username="$1"
    local email="$2"
    local password="$3"
    local role_id="$4"
    local role_name="$5"
    
    print_step "Registering $role_name user: $username"
    
    local response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"fullName\": \"Test $role_name User\",
            \"roleId\": $role_id
        }" \
        "$AUTH_ENDPOINT/register" --max-time 30)
    
    local http_code=$(echo "$response" | tail -n1)
    local body=$(echo "$response" | head -n -1)
    
    if [[ "$http_code" == "200" || "$http_code" == "201" ]]; then
        print_success "Registration successful for $role_name user: $username"
        add_test_result "Register $role_name ($username)" "PASS"
        return 0
    else
        print_error "Registration failed for $role_name user: $username (HTTP $http_code)"
        echo "Response: $body"
        add_test_result "Register $role_name ($username)" "FAIL"
        return 1
    fi
}

# Function to login a user
login_user() {
    local username="$1"
    local password="$2"
    local role_name="$3"
    
    print_step "Testing login for $role_name user: $username"
    
    local response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"password\": \"$password\"
        }" \
        "$AUTH_ENDPOINT/login" --max-time 30)
    
    local http_code=$(echo "$response" | tail -n1)
    local body=$(echo "$response" | head -n -1)
    
    if [[ "$http_code" == "200" ]]; then
        print_success "Login successful for $role_name user: $username"
        add_test_result "Login $role_name ($username)" "PASS"
        return 0
    else
        print_error "Login failed for $role_name user: $username (HTTP $http_code)"
        echo "Response: $body"
        add_test_result "Login $role_name ($username)" "FAIL"
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
    
    for i in "${!test_names[@]}"; do
        local test_name="${test_names[$i]}"
        local test_result="${test_results[$i]}"
        
        if [[ "$test_result" == "PASS" ]]; then
            echo -e "${GREEN}[PASS]${NC} $test_name"
            ((passed_tests++))
        else
            echo -e "${RED}[FAIL]${NC} $test_name"
        fi
    done
    
    echo "========================================"
    echo "Total Tests: $total_tests"
    echo -e "Passed: ${GREEN}$passed_tests${NC}"
    echo -e "Failed: ${RED}$((total_tests - passed_tests))${NC}"
    echo "========================================"
    
    if [[ $passed_tests -eq $total_tests ]]; then
        echo -e "${GREEN}All tests passed! 🎉${NC}"
        return 0
    else
        echo -e "${RED}Some tests failed. Please check the output above.${NC}"
        return 1
    fi
}

# Main execution
main() {
    echo "========================================"
    echo "  User Registration Test for All Roles"
    echo "========================================"
    echo ""
    
    # Test API connectivity first
    if ! test_api_connectivity; then
        print_error "Cannot proceed without API connectivity"
        exit 1
    fi
    
    echo ""
    print_step "Starting user registration tests..."
    
    # Generate timestamp for unique usernames
    timestamp=$(date +%s)
    
    # Define user roles and their details
    # Role IDs: Admin=1, Manager=2, User=3, Viewer=4
    
    # Test Admin registration and login
    register_user "admin_test_$timestamp" "admin_test_$timestamp@test.com" "TestPassword123!" 1 "Admin"
    login_user "admin_test_$timestamp" "TestPassword123!" "Admin"
    
    echo ""
    
    # Test Manager registration and login
    register_user "manager_test_$timestamp" "manager_test_$timestamp@test.com" "TestPassword123!" 2 "Manager"
    login_user "manager_test_$timestamp" "TestPassword123!" "Manager"
    
    echo ""
    
    # Test User registration and login
    register_user "user_test_$timestamp" "user_test_$timestamp@test.com" "TestPassword123!" 3 "User"
    login_user "user_test_$timestamp" "TestPassword123!" "User"
    
    echo ""
    
    # Test Viewer registration and login
    register_user "viewer_test_$timestamp" "viewer_test_$timestamp@test.com" "TestPassword123!" 4 "Viewer"
    login_user "viewer_test_$timestamp" "TestPassword123!" "Viewer"
    
    echo ""
    
    # Print summary
    print_test_summary
    
    local exit_code=$?
    
    echo ""
    print_step "Test completed. Check the summary above for results."
    
    if [[ $exit_code -eq 0 ]]; then
        echo -e "${GREEN}All user role registrations and logins are working correctly!${NC}"
    else
        echo -e "${RED}Some tests failed. Please review the API logs and fix any issues.${NC}"
    fi
    
    exit $exit_code
}

# Run main function
main "$@"
