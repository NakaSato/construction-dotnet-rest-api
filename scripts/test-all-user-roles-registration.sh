#!/bi# Configuration
API_BASE="http://localhost:5002/api/v1"
AUTH_ENDPOINT=# Function to test API connectivity
test_api_connectivity() {
    print_step "Testing API connectivity..."
    
    local health_response=$(curl -s -w '%{http_code}' -o /dev/null "http://localhost:5002/health" --max-time 10)
    
    if [[ "$health_response" == "200" ]]; then
        print_success "API is reachable and healthy"
        return 0
    else
        print_fail "API connectivity issue (HTTP: $health_response)"
        print_warning "Please ensure the application is running on http://localhost:5002"
        return 1
    fi
}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Test results tracking - use regular arrays instead of associative
test_names=()
test_results=()ehensive User Role Registration Testing Script
# Tests user registration for all available user roles in the Solar Projects API

# Configuration
API_BASE="http://localhost:5002/api/v1"
AUTH_ENDPOINT="$API_BASE/auth"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Test results tracking
declare -A test_results
total_tests=0
passed_tests=0
failed_tests=0

# Banner
echo -e "${CYAN}╔══════════════════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                      🧪 COMPREHENSIVE USER ROLE TESTING                      ║${NC}"
echo -e "${CYAN}║                     Solar Projects API Registration Test                     ║${NC}"
echo -e "${CYAN}╚══════════════════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}📍 API Endpoint: $API_BASE${NC}"
echo -e "${BLUE}⏰ Test Time: $(date)${NC}"
echo ""

# Function to print section headers
print_header() {
    echo -e "${PURPLE}════════════════════════════════════════════════════════════════════════════════${NC}"
    echo -e "${PURPLE}  $1${NC}"
    echo -e "${PURPLE}════════════════════════════════════════════════════════════════════════════════${NC}"
    echo ""
}

# Function to print test step
print_step() {
    echo -e "${CYAN}🔄 $1${NC}"
}

# Function to print success
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

# Function to print failure
print_fail() {
    echo -e "${RED}❌ $1${NC}"
}

# Function to print warning
print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Function to test API connectivity
test_api_connectivity() {
    print_step "Testing API connectivity..."
    
    local health_response=$(curl -s -w '%{http_code}' -o /dev/null "$API_BASE/../health" --max-time 10)
    
    if [[ "$health_response" == "200" ]]; then
        print_success "API is reachable and healthy"
        return 0
    else
        print_fail "API connectivity issue (HTTP: $health_response)"
        print_warning "Please ensure the application is running on http://localhost:5002"
        return 1
    fi
}

# Function to register a user and test login
register_and_test_user() {
    local role_id="$1"
    local role_name="$2"
    local username="$3"
    local email="$4"
    local password="$5"
    local full_name="$6"
    
    ((total_tests++))
    local test_name="$role_name Role Registration Test"
    
    print_step "Testing $role_name role registration (ID: $role_id)"
    echo "  Username: $username"
    echo "  Email: $email"
    echo "  Full Name: $full_name"
    echo ""
    
    # Create registration payload
    local payload=$(cat <<EOF
{
    "username": "$username",
    "email": "$email",
    "password": "$password",
    "fullName": "$full_name",
    "roleId": $role_id
}
EOF
)
    
    # Test registration
    print_step "  Sending registration request..."
    local response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$AUTH_ENDPOINT/register" \
        --max-time 30)
    
    local body=$(echo "$response" | sed '$d')
    local status_code=$(echo "$response" | tail -n1)
    
    # Store test result
    test_names+=("$test_name")
    
    # Check registration result
    case "$status_code" in
        200|201)
            print_success "  Registration successful!"
            
            # Extract user info if possible
            if command -v jq &> /dev/null; then
                local user_info=$(echo "$body" | jq -r '.data // .user // empty' 2>/dev/null)
                if [[ -n "$user_info" && "$user_info" != "null" ]]; then
                    echo "    User Details:"
                    echo "$user_info" | jq . 2>/dev/null || echo "    $user_info"
                fi
            fi
            
            # Test login immediately after registration
            print_step "  Testing login with new credentials..."
            test_login "$username" "$password" "$role_name"
            local login_result=$?
            
            if [[ $login_result -eq 0 ]]; then
                print_success "  $role_name user registration and login test PASSED"
                test_results+=("PASSED")
                ((passed_tests++))
            else
                print_fail "  $role_name user registration PASSED but login FAILED"
                test_results+=("PARTIAL")
                ((failed_tests++))
            fi
            ;;
        400)
            print_fail "  Registration failed - Bad Request (HTTP 400)"
            if command -v jq &> /dev/null; then
                local error_msg=$(echo "$body" | jq -r '.message // .error // .' 2>/dev/null)
                if [[ "$error_msg" != "null" && -n "$error_msg" ]]; then
                    print_warning "  Error: $error_msg"
                fi
            fi
            test_results+=("FAILED")
            ((failed_tests++))
            ;;
        409)
            print_warning "  User already exists (HTTP 409) - This might be expected if running multiple times"
            print_step "  Testing login with existing credentials..."
            test_login "$username" "$password" "$role_name"
            local login_result=$?
            
            if [[ $login_result -eq 0 ]]; then
                print_success "  $role_name user already exists and login works"
                test_results+=("PASSED (User Existed)")
                ((passed_tests++))
            else
                print_fail "  $role_name user exists but login failed"
                test_results+=("FAILED (Login Issue)")
                ((failed_tests++))
            fi
            ;;
        *)
            print_fail "  Registration failed - HTTP $status_code"
            if [[ -n "$body" ]]; then
                print_warning "  Response: $(echo "$body" | head -c 200)"
            fi
            test_results+=("FAILED")
            ((failed_tests++))
            ;;
    esac
    
    echo ""
}

# Function to test login
test_login() {
    local username="$1"
    local password="$2"
    local role_name="$3"
    
    local login_payload=$(cat <<EOF
{
    "username": "$username",
    "password": "$password"
}
EOF
)
    
    local login_response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$login_payload" \
        "$AUTH_ENDPOINT/login" \
        --max-time 30)
    
    local login_body=$(echo "$login_response" | sed '$d')
    local login_status=$(echo "$login_response" | tail -n1)
    
    if [[ "$login_status" == "200" ]]; then
        print_success "    Login successful for $role_name user"
        
        # Extract and display token info if possible
        if command -v jq &> /dev/null; then
            local token=$(echo "$login_body" | jq -r '.data.token // .token // empty' 2>/dev/null)
            local user_data=$(echo "$login_body" | jq -r '.data.user // .user // empty' 2>/dev/null)
            
            if [[ -n "$token" && "$token" != "null" ]]; then
                print_success "    JWT token generated successfully"
                echo "      Token length: ${#token} characters"
            fi
            
            if [[ -n "$user_data" && "$user_data" != "null" ]]; then
                echo "    User Profile:"
                echo "$user_data" | jq . 2>/dev/null | sed 's/^/      /'
            fi
        fi
        
        return 0
    else
        print_fail "    Login failed for $role_name user (HTTP $login_status)"
        if [[ -n "$login_body" ]]; then
            print_warning "    Login response: $(echo "$login_body" | head -c 150)"
        fi
        return 1
    fi
}

# Function to display test summary
display_test_summary() {
    print_header "📊 TEST SUMMARY REPORT"
    
    echo -e "${BLUE}Total Tests Executed: ${total_tests}${NC}"
    echo -e "${GREEN}Tests Passed: ${passed_tests}${NC}"
    echo -e "${RED}Tests Failed: ${failed_tests}${NC}"
    echo ""
    
    local success_rate=0
    if [[ $total_tests -gt 0 ]]; then
        success_rate=$((passed_tests * 100 / total_tests))
    fi
    
    echo -e "${CYAN}Success Rate: ${success_rate}%${NC}"
    echo ""
    
    # Detailed results
    echo -e "${PURPLE}Detailed Test Results:${NC}"
    echo "─────────────────────────────────────────────────────────────"
    
    for ((i=0; i<${#test_names[@]}; i++)); do
        local test_name="${test_names[$i]}"
        local result="${test_results[$i]}"
        case "$result" in
            "PASSED")
                echo -e "  ${GREEN}✅ $test_name: $result${NC}"
                ;;
            "PASSED (User Existed)")
                echo -e "  ${YELLOW}✅ $test_name: $result${NC}"
                ;;
            "PARTIAL")
                echo -e "  ${YELLOW}⚠️  $test_name: $result${NC}"
                ;;
            "FAILED"*)
                echo -e "  ${RED}❌ $test_name: $result${NC}"
                ;;
        esac
    done
    
    echo ""
    
    # Overall status
    if [[ $passed_tests -eq $total_tests ]]; then
        print_success "🎉 ALL TESTS PASSED! User registration is working correctly for all roles."
    elif [[ $passed_tests -gt 0 ]]; then
        print_warning "⚠️  PARTIAL SUCCESS: Some tests passed, some failed. Check individual results above."
    else
        print_fail "❌ ALL TESTS FAILED! There may be issues with the registration system."
    fi
}

# Function to clean up test users (optional)
cleanup_test_users() {
    echo ""
    echo -n "Do you want to clean up test users? (y/N): "
    read -r cleanup_choice
    
    if [[ "$cleanup_choice" =~ ^[Yy]$ ]]; then
        print_warning "Note: User cleanup functionality would need to be implemented via admin endpoints"
        print_warning "For now, test users remain in the system for further testing"
    fi
}

# Main execution function
main() {
    # Test API connectivity first
    if ! test_api_connectivity; then
        echo ""
        print_fail "Cannot proceed with tests due to API connectivity issues"
        exit 1
    fi
    
    echo ""
    print_header "🧪 STARTING COMPREHENSIVE USER ROLE REGISTRATION TESTS"
    
    # Generate unique timestamp for test users
    local timestamp=$(date +%Y%m%d_%H%M%S)
    
    # Test 1: Admin Role (ID: 1)
    register_and_test_user 1 "Admin" "test_admin_${timestamp}" "test_admin_${timestamp}@example.com" "AdminPass123!" "Test Administrator ${timestamp}"
    
    sleep 1
    
    # Test 2: Manager Role (ID: 2)  
    register_and_test_user 2 "Manager" "test_manager_${timestamp}" "test_manager_${timestamp}@example.com" "ManagerPass123!" "Test Project Manager ${timestamp}"
    
    sleep 1
    
    # Test 3: User Role (ID: 3)
    register_and_test_user 3 "User" "test_user_${timestamp}" "test_user_${timestamp}@example.com" "UserPass123!" "Test Field Technician ${timestamp}"
    
    sleep 1
    
    # Test 4: Viewer Role (ID: 4)
    register_and_test_user 4 "Viewer" "test_viewer_${timestamp}" "test_viewer_${timestamp}@example.com" "ViewerPass123!" "Test Viewer ${timestamp}"
    
    echo ""
    display_test_summary
    
    cleanup_test_users
    
    echo ""
    print_header "🏁 USER ROLE REGISTRATION TESTING COMPLETED"
    
    # Exit with appropriate code
    if [[ $failed_tests -eq 0 ]]; then
        exit 0
    else
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    print_fail "curl is required but not installed. Please install curl first."
    exit 1
fi

# Run main function
main "$@"
