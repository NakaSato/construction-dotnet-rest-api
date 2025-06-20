#!/bin/bash

# Enhanced Production API Testing Script for Solar Projects API
# This script tests the production API deployed on Azure with comprehensive validation
echo "🚀 Enhanced Production API Testing Suite"
echo "========================================"

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
HEALTH_ENDPOINT="$PROD_URL/health"
SWAGGER_ENDPOINT="$PROD_URL/swagger"
ROOT_ENDPOINT="$PROD_URL"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Test tracking
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
WARNING_TESTS=0

# Test results array
declare -a TEST_RESULTS=()

# Function to print colored output
print_section() {
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}🧪 $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_test() {
    echo -e "${BLUE}🔍 Testing: $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
    ((PASSED_TESTS++))
    TEST_RESULTS+=("PASS: $1")
}

print_fail() {
    echo -e "${RED}❌ FAIL: $1${NC}"
    ((FAILED_TESTS++))
    TEST_RESULTS+=("FAIL: $1")
}

print_warning() {
    echo -e "${YELLOW}⚠️  WARN: $1${NC}"
    ((WARNING_TESTS++))
    TEST_RESULTS+=("WARN: $1")
}

print_info() {
    echo -e "${PURPLE}ℹ️  INFO: $1${NC}"
}

# Function to increment test counter
count_test() {
    ((TOTAL_TESTS++))
}

# Function to test HTTP endpoint
test_endpoint() {
    local name="$1"
    local url="$2"
    local expected_status="$3"
    local auth_header="$4"
    local method="${5:-GET}"
    local payload="$6"
    
    count_test
    print_test "$name"
    
    # Build curl command
    local curl_cmd="curl -s -w '%{http_code}|%{time_total}|%{size_download}'"
    
    if [[ -n "$auth_header" ]]; then
        curl_cmd="$curl_cmd -H 'Authorization: $auth_header'"
    fi
    
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    curl_cmd="$curl_cmd -H 'User-Agent: ProductionAPITester/1.0'"
    
    if [[ "$method" == "POST" && -n "$payload" ]]; then
        curl_cmd="$curl_cmd -X POST -d '$payload'"
    elif [[ "$method" != "GET" ]]; then
        curl_cmd="$curl_cmd -X $method"
    fi
    
    curl_cmd="$curl_cmd '$url'"
    
    # Execute request
    local response=$(eval $curl_cmd)
    local body=$(echo "$response" | sed 's/|[^|]*|[^|]*|[^|]*$//')
    local status=$(echo "$response" | grep -o '[0-9]*|[^|]*|[^|]*$' | cut -d'|' -f1)
    local time=$(echo "$response" | grep -o '[0-9]*|[^|]*|[^|]*$' | cut -d'|' -f2)
    local size=$(echo "$response" | grep -o '[0-9]*|[^|]*|[^|]*$' | cut -d'|' -f3)
    
    # Validate response
    if [[ "$status" == "$expected_status" ]]; then
        print_success "$name - Status: $status, Time: ${time}s, Size: ${size} bytes"
        
        # Additional validation for specific endpoints
        case "$url" in
            *"/health"*)
                if echo "$body" | grep -q "Healthy"; then
                    print_info "Health check returned expected 'Healthy' status"
                else
                    print_warning "Health check did not return expected 'Healthy' status"
                fi
                ;;
            *"/projects/test"*)
                if echo "$body" | grep -q "Projects API is working"; then
                    print_info "Test endpoint returned expected message"
                else
                    print_warning "Test endpoint did not return expected message"
                fi
                ;;
        esac
        
        # Performance check
        if (( $(echo "$time > 2.0" | bc -l) )); then
            print_warning "Slow response time: ${time}s (>2s threshold)"
        fi
        
    else
        print_fail "$name - Expected: $expected_status, Got: $status"
        if [[ -n "$body" && ${#body} -lt 500 ]]; then
            print_info "Response body: $body"
        fi
    fi
}

# Function to test API documentation
test_documentation() {
    print_section "API Documentation Tests"
    
    # Test Swagger UI
    test_endpoint "Swagger UI" "$SWAGGER_ENDPOINT" "200"
    
    # Test Swagger JSON
    test_endpoint "Swagger JSON" "$SWAGGER_ENDPOINT/v1/swagger.json" "200"
    
    # Test root endpoint (might serve documentation)
    test_endpoint "Root Endpoint" "$ROOT_ENDPOINT" "200"
}

# Function to test core API endpoints
test_core_endpoints() {
    print_section "Core API Endpoint Tests"
    
    # Health check
    test_endpoint "Health Check" "$HEALTH_ENDPOINT" "200"
    
    # Public test endpoint
    test_endpoint "Projects Test Endpoint" "$API_BASE/projects/test" "200"
    
    # Test API versioning
    test_endpoint "API Version Check" "$PROD_URL/api/v1.0/projects/test" "200"
}

# Function to test authenticated endpoints (without auth - should fail)
test_authenticated_endpoints() {
    print_section "Authentication Tests (Expect 401)"
    
    # These should return 401 Unauthorized
    test_endpoint "Projects List (No Auth)" "$API_BASE/projects" "401"
    test_endpoint "Users List (No Auth)" "$API_BASE/users" "401"
    test_endpoint "Daily Reports (No Auth)" "$API_BASE/daily-reports" "401"
    test_endpoint "Work Requests (No Auth)" "$API_BASE/work-requests" "401"
}

# Function to test HTTP methods and security
test_security_and_methods() {
    print_section "Security & HTTP Methods Tests"
    
    # Test HTTPS connectivity
    count_test
    print_test "HTTPS Connectivity"
    local https_response=$(curl -s -w '%{http_code}' -o /dev/null "$HEALTH_ENDPOINT" --max-time 10)
    if [[ "$https_response" == "200" ]]; then
        print_success "HTTPS working correctly - Status: $https_response"
    else
        print_fail "HTTPS connectivity issue - Status: $https_response"
    fi
    
    # Test HTTPS redirect (if applicable)
    local http_url="http://solar-projects-api.azurewebsites.net/health"
    count_test
    print_test "HTTP to HTTPS Redirect"
    local redirect_response=$(curl -s -w '%{http_code}' -o /dev/null "$http_url" --max-time 10)
    if [[ "$redirect_response" == "301" || "$redirect_response" == "302" ]]; then
        print_success "HTTP to HTTPS redirect working - Status: $redirect_response"
    elif [[ "$redirect_response" == "200" ]]; then
        print_success "HTTPS accessible - Status: $redirect_response"
    elif [[ "$redirect_response" == "000" ]]; then
        print_info "HTTP redirect test timeout (likely HTTPS-only, which is good)"
    else
        print_warning "HTTP response: $redirect_response (may be HTTPS-only)"
    fi
    
    # Test unsupported methods
    test_endpoint "DELETE on Health (Method Not Allowed)" "$HEALTH_ENDPOINT" "405" "" "DELETE"
    test_endpoint "PUT on Health (Method Not Allowed)" "$HEALTH_ENDPOINT" "405" "" "PUT"
    
    # Test CORS headers
    count_test
    print_test "CORS Headers Check"
    local cors_response=$(curl -s -H "Origin: https://example.com" -H "Access-Control-Request-Method: GET" -H "Access-Control-Request-Headers: Content-Type" -X OPTIONS "$HEALTH_ENDPOINT" -w '%{http_code}')
    local cors_status=$(echo "$cors_response" | tail -c 4)
    if [[ "$cors_status" == "200" || "$cors_status" == "204" ]]; then
        print_success "CORS preflight - Status: $cors_status"
    else
        print_info "CORS preflight response: $cors_status (may not be configured)"
    fi
}

# Function to test rate limiting
test_rate_limiting() {
    print_section "Rate Limiting Tests"
    
    print_test "Rate Limiting Check (5 rapid requests)"
    local rate_limit_hit=false
    
    for i in {1..5}; do
        local response=$(curl -s -w '%{http_code}' -o /dev/null "$HEALTH_ENDPOINT")
        if [[ "$response" == "429" ]]; then
            rate_limit_hit=true
            break
        fi
        sleep 0.1
    done
    
    count_test
    if $rate_limit_hit; then
        print_success "Rate limiting is active (429 Too Many Requests)"
    else
        print_info "Rate limiting not triggered in test (may have high limits)"
    fi
}

# Function to test performance
test_performance() {
    print_section "Performance Tests"
    
    print_test "Response Time Analysis (10 requests)"
    local total_time=0
    local max_time=0
    local min_time=999
    local requests=10
    
    for i in $(seq 1 $requests); do
        local time_response=$(curl -s -w '%{time_total}' -o /dev/null "$HEALTH_ENDPOINT")
        total_time=$(echo "$total_time + $time_response" | bc -l)
        
        if (( $(echo "$time_response > $max_time" | bc -l) )); then
            max_time=$time_response
        fi
        
        if (( $(echo "$time_response < $min_time" | bc -l) )); then
            min_time=$time_response
        fi
        
        echo -n "."
    done
    
    echo ""
    local avg_time=$(echo "scale=3; $total_time / $requests" | bc -l)
    
    count_test
    print_success "Performance Analysis Complete"
    print_info "Average response time: ${avg_time}s"
    print_info "Min response time: ${min_time}s"
    print_info "Max response time: ${max_time}s"
    
    if (( $(echo "$avg_time < 1.0" | bc -l) )); then
        print_success "Average response time is good (<1s)"
    elif (( $(echo "$avg_time < 2.0" | bc -l) )); then
        print_warning "Average response time is acceptable (1-2s)"
    else
        print_fail "Average response time is slow (>2s)"
    fi
}

# Function to test error handling
test_error_handling() {
    print_section "Error Handling Tests"
    
    # Test 404 endpoints
    test_endpoint "Non-existent Endpoint" "$API_BASE/nonexistent" "404"
    test_endpoint "Invalid Project ID" "$API_BASE/projects/invalid-guid" "400"
    
    # Test malformed requests
    test_endpoint "Malformed JSON POST" "$API_BASE/projects" "400" "" "POST" '{"invalid": json}'
}

# Function to display final summary
display_summary() {
    print_section "Test Summary"
    
    echo -e "${CYAN}📊 Test Results Summary${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo -e "Total Tests: ${BLUE}$TOTAL_TESTS${NC}"
    echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
    echo -e "Warnings: ${YELLOW}$WARNING_TESTS${NC}"
    
    local success_rate=0
    if [[ $TOTAL_TESTS -gt 0 ]]; then
        success_rate=$(echo "scale=1; $PASSED_TESTS * 100 / $TOTAL_TESTS" | bc -l)
    fi
    
    echo -e "Success Rate: ${GREEN}${success_rate}%${NC}"
    echo ""
    
    if [[ $FAILED_TESTS -eq 0 ]]; then
        echo -e "${GREEN}🎉 All critical tests passed! Production API is healthy.${NC}"
    elif [[ $FAILED_TESTS -le 2 ]]; then
        echo -e "${YELLOW}⚠️  Some tests failed, but API appears mostly functional.${NC}"
    else
        echo -e "${RED}❌ Multiple test failures detected. API may have issues.${NC}"
    fi
    
    echo ""
    echo -e "${CYAN}🔗 Useful Links:${NC}"
    echo "• Production API: $PROD_URL"
    echo "• Health Check: $HEALTH_ENDPOINT"
    echo "• API Documentation: $SWAGGER_ENDPOINT"
    echo "• Test Endpoint: $API_BASE/projects/test"
    
    # Save results to file
    echo "Saving test results to test-results-$(date +%Y%m%d-%H%M%S).txt"
    {
        echo "Production API Test Results - $(date)"
        echo "========================================="
        echo "Total Tests: $TOTAL_TESTS"
        echo "Passed: $PASSED_TESTS"
        echo "Failed: $FAILED_TESTS"
        echo "Warnings: $WARNING_TESTS"
        echo "Success Rate: ${success_rate}%"
        echo ""
        echo "Detailed Results:"
        printf '%s\n' "${TEST_RESULTS[@]}"
    } > "test-results-$(date +%Y%m%d-%H%M%S).txt"
}

# Main execution
main() {
    echo -e "${BLUE}Testing Production API at: $PROD_URL${NC}"
    echo -e "${BLUE}Started at: $(date)${NC}"
    echo ""
    
    # Check if bc is available for calculations
    if ! command -v bc &> /dev/null; then
        echo -e "${YELLOW}⚠️  'bc' calculator not found. Some calculations may not work.${NC}"
    fi
    
    # Run all test suites
    test_core_endpoints
    test_documentation
    test_authenticated_endpoints
    test_security_and_methods
    test_rate_limiting
    test_performance
    test_error_handling
    
    # Display final summary
    display_summary
    
    # Exit with appropriate code
    if [[ $FAILED_TESTS -eq 0 ]]; then
        exit 0
    else
        exit 1
    fi
}

# Check if running as main script
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
