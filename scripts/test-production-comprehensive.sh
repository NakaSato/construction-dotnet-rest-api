#!/bin/bash

# Comprehensive Production API Testing Suite for Solar Projects API
echo "🔬 Comprehensive Production API Testing Suite"
echo "============================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
HEALTH_ENDPOINT="$PROD_URL/health"
SWAGGER_ENDPOINT="$PROD_URL/swagger"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Test results
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
WARNING_TESTS=0

# Test categories
FUNCTIONAL_TESTS=0
FUNCTIONAL_PASSED=0
PERFORMANCE_TESTS=0
PERFORMANCE_PASSED=0
SECURITY_TESTS=0
SECURITY_PASSED=0

# Function to print colored output
print_test_header() {
    echo -e "\n${CYAN}🧪 Testing: $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
    ((PASSED_TESTS++))
}

print_fail() {
    echo -e "${RED}❌ $1${NC}"
    ((FAILED_TESTS++))
}

print_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
    ((WARNING_TESTS++))
}

print_info() {
    echo -e "${BLUE}ℹ️ $1${NC}"
}

print_category() {
    echo -e "\n${PURPLE}📂 $1${NC}"
}

# Function to make HTTP request and validate response
test_endpoint() {
    local method="$1"
    local endpoint="$2"
    local expected_status="$3"
    local description="$4"
    local data="$5"
    local category="$6"
    
    ((TOTAL_TESTS++))
    
    case $category in
        "functional") ((FUNCTIONAL_TESTS++)) ;;
        "performance") ((PERFORMANCE_TESTS++)) ;;
        "security") ((SECURITY_TESTS++)) ;;
    esac
    
    print_info "Testing $method $endpoint"
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "\n%{http_code}" "$endpoint")
    elif [ "$method" = "POST" ]; then
        response=$(curl -s -w "\n%{http_code}" -X POST -H "Content-Type: application/json" -d "$data" "$endpoint")
    elif [ "$method" = "PUT" ]; then
        response=$(curl -s -w "\n%{http_code}" -X PUT -H "Content-Type: application/json" -d "$data" "$endpoint")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "\n%{http_code}" -X DELETE "$endpoint")
    fi
    
    # Extract HTTP status code (last line)
    status_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$status_code" = "$expected_status" ]; then
        print_success "$description - Status: $status_code"
        case $category in
            "functional") ((FUNCTIONAL_PASSED++)) ;;
            "performance") ((PERFORMANCE_PASSED++)) ;;
            "security") ((SECURITY_PASSED++)) ;;
        esac
        if [ -n "$body" ] && [ "$body" != "" ]; then
            echo "   Response: $(echo "$body" | head -c 100)..."
        fi
        return 0
    else
        print_fail "$description - Expected: $expected_status, Got: $status_code"
        if [ -n "$body" ] && [ "$body" != "" ]; then
            echo "   Error: $(echo "$body" | head -c 200)..."
        fi
        return 1
    fi
}

# Function to test JSON response format
test_json_response() {
    local endpoint="$1"
    local description="$2"
    local category="$3"
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    
    print_info "Testing JSON format: $endpoint"
    
    response=$(curl -s "$endpoint")
    
    if echo "$response" | jq . > /dev/null 2>&1; then
        print_success "$description - Valid JSON response"
        ((FUNCTIONAL_PASSED++))
        return 0
    else
        print_fail "$description - Invalid JSON response"
        echo "   Response: $(echo "$response" | head -c 200)..."
        return 1
    fi
}

# Function to check service availability
check_service_availability() {
    print_category "Infrastructure Tests"
    print_test_header "Service Availability Check"
    
    print_info "Checking if the service is reachable..."
    
    if curl -s --connect-timeout 10 "$PROD_URL" > /dev/null; then
        print_success "Service is reachable"
        return 0
    else
        print_fail "Service is not reachable"
        return 1
    fi
}

# Function to test health endpoint
test_health_endpoint() {
    print_category "Health & Monitoring Tests"
    print_test_header "Health Endpoint"
    
    test_endpoint "GET" "$HEALTH_ENDPOINT" "200" "Health check endpoint" "" "functional"
    test_json_response "$HEALTH_ENDPOINT" "Health endpoint JSON format" "functional"
    
    # Test specific health response content
    print_info "Validating health response content..."
    response=$(curl -s "$HEALTH_ENDPOINT")
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    if echo "$response" | jq -e '.status' > /dev/null 2>&1; then
        status=$(echo "$response" | jq -r '.status')
        if [ "$status" = "Healthy" ]; then
            print_success "Health status is 'Healthy'"
            ((FUNCTIONAL_PASSED++))
        else
            print_fail "Health status is '$status' (expected 'Healthy')"
        fi
    else
        print_fail "Health response missing 'status' field"
    fi
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    if echo "$response" | jq -e '.version' > /dev/null 2>&1; then
        version=$(echo "$response" | jq -r '.version')
        print_success "Version information present: $version"
        ((FUNCTIONAL_PASSED++))
    else
        print_warning "Version information not found in health response"
    fi
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    if echo "$response" | jq -e '.environment' > /dev/null 2>&1; then
        environment=$(echo "$response" | jq -r '.environment')
        print_success "Environment information present: $environment"
        ((FUNCTIONAL_PASSED++))
    else
        print_warning "Environment information not found in health response"
    fi
}

# Function to test API endpoints
test_api_endpoints() {
    print_category "Functional API Tests"
    print_test_header "Core API Endpoints"
    
    # Test Projects test endpoint (no auth required)
    test_endpoint "GET" "$API_BASE/projects/test" "200" "Projects test endpoint (no auth)" "" "functional"
    test_json_response "$API_BASE/projects/test" "Projects test JSON response" "functional"
    
    # Validate projects test response structure
    print_info "Validating projects test response structure..."
    response=$(curl -s "$API_BASE/projects/test")
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    if echo "$response" | jq -e '.message' > /dev/null 2>&1; then
        message=$(echo "$response" | jq -r '.message')
        print_success "Message field present: $message"
        ((FUNCTIONAL_PASSED++))
    else
        print_fail "Message field missing from projects test response"
    fi
    
    ((TOTAL_TESTS++))
    ((FUNCTIONAL_TESTS++))
    if echo "$response" | jq -e '.sampleProjects' > /dev/null 2>&1; then
        sample_count=$(echo "$response" | jq '.sampleProjects | length')
        print_success "Sample projects present: $sample_count items"
        ((FUNCTIONAL_PASSED++))
    else
        print_fail "Sample projects missing from test response"
    fi
    
    # Test authenticated endpoints (should return 401)
    print_test_header "Authentication-Required Endpoints"
    test_endpoint "GET" "$API_BASE/projects" "401" "Get all projects (requires auth)" "" "security"
    test_endpoint "GET" "$API_BASE/daily-reports" "401" "Get all daily reports (requires auth)" "" "security"
    test_endpoint "GET" "$API_BASE/work-requests" "401" "Get all work requests (requires auth)" "" "security"
}

# Function to test error handling
test_error_handling() {
    print_category "Error Handling Tests"
    print_test_header "Error Response Testing"
    
    # Test 404 for non-existent endpoint
    test_endpoint "GET" "$API_BASE/non-existent-endpoint" "404" "Non-existent endpoint returns 404" "" "functional"
    
    # Test 404 for non-existent resource
    test_endpoint "GET" "$API_BASE/projects/99999" "404" "Non-existent project returns 404" "" "functional"
    
    # Test various HTTP methods
    test_endpoint "PUT" "$API_BASE/projects/test" "405" "PUT on read-only endpoint returns 405" "" "functional" || true
    test_endpoint "DELETE" "$API_BASE/projects/test" "405" "DELETE on read-only endpoint returns 405" "" "functional" || true
}

# Function to test performance
test_performance() {
    print_category "Performance Tests"
    print_test_header "Response Time Testing"
    
    # Test health endpoint response time
    print_info "Measuring health endpoint response time..."
    start_time=$(date +%s%N)
    curl -s "$HEALTH_ENDPOINT" > /dev/null
    end_time=$(date +%s%N)
    response_time=$(( (end_time - start_time) / 1000000 )) # Convert to milliseconds
    
    ((TOTAL_TESTS++))
    ((PERFORMANCE_TESTS++))
    if [ $response_time -lt 2000 ]; then
        print_success "Health endpoint response time: ${response_time}ms (< 2s)"
        ((PERFORMANCE_PASSED++))
    elif [ $response_time -lt 5000 ]; then
        print_warning "Health endpoint response time: ${response_time}ms (2-5s)"
    else
        print_fail "Health endpoint response time: ${response_time}ms (> 5s)"
    fi
    
    # Test API endpoint response time
    print_info "Measuring API endpoint response time..."
    start_time=$(date +%s%N)
    curl -s "$API_BASE/projects/test" > /dev/null
    end_time=$(date +%s%N)
    api_response_time=$(( (end_time - start_time) / 1000000 ))
    
    ((TOTAL_TESTS++))
    ((PERFORMANCE_TESTS++))
    if [ $api_response_time -lt 3000 ]; then
        print_success "Projects API response time: ${api_response_time}ms (< 3s)"
        ((PERFORMANCE_PASSED++))
    elif [ $api_response_time -lt 8000 ]; then
        print_warning "Projects API response time: ${api_response_time}ms (3-8s)"
    else
        print_fail "Projects API response time: ${api_response_time}ms (> 8s)"
    fi
    
    # Test concurrent requests
    print_info "Testing concurrent request handling..."
    start_time=$(date +%s%N)
    for i in {1..5}; do
        curl -s "$HEALTH_ENDPOINT" > /dev/null &
    done
    wait
    end_time=$(date +%s%N)
    concurrent_time=$(( (end_time - start_time) / 1000000 ))
    
    ((TOTAL_TESTS++))
    ((PERFORMANCE_TESTS++))
    if [ $concurrent_time -lt 5000 ]; then
        print_success "Concurrent requests completed: ${concurrent_time}ms (< 5s)"
        ((PERFORMANCE_PASSED++))
    else
        print_warning "Concurrent requests took: ${concurrent_time}ms (> 5s)"
    fi
}

# Function to test security
test_security() {
    print_category "Security Tests"
    print_test_header "Security Headers"
    
    print_info "Checking security headers..."
    headers=$(curl -s -I "$PROD_URL")
    
    ((TOTAL_TESTS++))
    ((SECURITY_TESTS++))
    if echo "$headers" | grep -i "strict-transport-security" > /dev/null; then
        print_success "HTTPS Strict Transport Security header present"
        ((SECURITY_PASSED++))
    else
        print_warning "HTTPS Strict Transport Security header missing"
    fi
    
    ((TOTAL_TESTS++))
    ((SECURITY_TESTS++))
    if echo "$headers" | grep -i "x-content-type-options" > /dev/null; then
        print_success "X-Content-Type-Options header present"
        ((SECURITY_PASSED++))
    else
        print_warning "X-Content-Type-Options header missing"
    fi
    
    ((TOTAL_TESTS++))
    ((SECURITY_TESTS++))
    if echo "$headers" | grep -i "x-frame-options" > /dev/null; then
        print_success "X-Frame-Options header present"
        ((SECURITY_PASSED++))
    else
        print_warning "X-Frame-Options header missing"
    fi
    
    print_test_header "Authentication Testing"
    
    # Test authentication requirements
    print_info "Testing authentication enforcement..."
    
    ((TOTAL_TESTS++))
    ((SECURITY_TESTS++))
    response=$(curl -s -w "\n%{http_code}" "$API_BASE/projects" -H "Authorization: Bearer invalid-token")
    status_code=$(echo "$response" | tail -n1)
    
    if [ "$status_code" = "401" ]; then
        print_success "Invalid token properly rejected (401)"
        ((SECURITY_PASSED++))
    else
        print_fail "Invalid token not properly rejected (got $status_code)"
    fi
}

# Function to test load handling
test_load_handling() {
    print_category "Load Testing"
    print_test_header "Basic Load Testing"
    
    print_info "Running basic load test (20 requests)..."
    
    start_time=$(date +%s%N)
    success_count=0
    total_requests=20
    
    for i in $(seq 1 $total_requests); do
        response=$(curl -s -w "%{http_code}" "$HEALTH_ENDPOINT" -o /dev/null)
        if [ "$response" = "200" ]; then
            ((success_count++))
        fi
    done
    
    end_time=$(date +%s%N)
    load_test_time=$(( (end_time - start_time) / 1000000 ))
    success_rate=$(( success_count * 100 / total_requests ))
    
    ((TOTAL_TESTS++))
    ((PERFORMANCE_TESTS++))
    if [ $success_rate -ge 95 ]; then
        print_success "Load test success rate: $success_rate% ($success_count/$total_requests)"
        ((PERFORMANCE_PASSED++))
    else
        print_fail "Load test success rate: $success_rate% ($success_count/$total_requests)"
    fi
    
    print_info "Load test completed in ${load_test_time}ms"
}

# Function to generate comprehensive report
generate_comprehensive_report() {
    echo -e "\n${CYAN}📊 Comprehensive Test Report${NC}"
    echo "=============================="
    echo "Test Execution Time: $(date)"
    echo "Production URL: $PROD_URL"
    echo ""
    
    echo "📈 Overall Results:"
    echo "  Total Tests: $TOTAL_TESTS"
    echo -e "  Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "  Failed: ${RED}$FAILED_TESTS${NC}"
    echo -e "  Warnings: ${YELLOW}$WARNING_TESTS${NC}"
    
    if [ $TOTAL_TESTS -gt 0 ]; then
        success_rate=$(( PASSED_TESTS * 100 / TOTAL_TESTS ))
        echo "  Success Rate: $success_rate%"
    fi
    
    echo ""
    echo "📋 Results by Category:"
    
    if [ $FUNCTIONAL_TESTS -gt 0 ]; then
        functional_rate=$(( FUNCTIONAL_PASSED * 100 / FUNCTIONAL_TESTS ))
        echo -e "  Functional Tests: ${GREEN}$FUNCTIONAL_PASSED${NC}/$FUNCTIONAL_TESTS ($functional_rate%)"
    fi
    
    if [ $PERFORMANCE_TESTS -gt 0 ]; then
        performance_rate=$(( PERFORMANCE_PASSED * 100 / PERFORMANCE_TESTS ))
        echo -e "  Performance Tests: ${GREEN}$PERFORMANCE_PASSED${NC}/$PERFORMANCE_TESTS ($performance_rate%)"
    fi
    
    if [ $SECURITY_TESTS -gt 0 ]; then
        security_rate=$(( SECURITY_PASSED * 100 / SECURITY_TESTS ))
        echo -e "  Security Tests: ${GREEN}$SECURITY_PASSED${NC}/$SECURITY_TESTS ($security_rate%)"
    fi
    
    echo ""
    echo "🔗 Quick Links:"
    echo "  Application: $PROD_URL"
    echo "  Health Check: $HEALTH_ENDPOINT"
    echo "  Projects Test: $API_BASE/projects/test"
    
    echo ""
    echo "📝 Recommendations:"
    
    if [ $FAILED_TESTS -eq 0 ] && [ $WARNING_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 Excellent! All tests passed with no warnings.${NC}"
        echo "✅ Your API is production-ready!"
    elif [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${YELLOW}⚠️ All tests passed, but some warnings detected.${NC}"
        echo "📋 Consider addressing the warnings for optimal security."
    else
        echo -e "${RED}❌ Some tests failed. Please investigate and fix the issues.${NC}"
        echo "🔧 Check application logs and configuration."
    fi
    
    echo ""
    echo "🛠️ Useful Commands:"
    echo "  # View application logs"
    echo "  az webapp log tail --name solar-projects-api --resource-group solar-projects-rg"
    echo ""
    echo "  # Restart the application"
    echo "  az webapp restart --name solar-projects-api --resource-group solar-projects-rg"
    echo ""
    echo "  # Monitor application metrics"
    echo "  az monitor metrics list --resource solar-projects-api --resource-group solar-projects-rg --resource-type Microsoft.Web/sites"
    
    # Return appropriate exit code
    if [ $FAILED_TESTS -eq 0 ]; then
        return 0
    else
        return 1
    fi
}

# Main execution
main() {
    echo "Production API: $PROD_URL"
    echo "Test started at: $(date)"
    echo
    
    # Prerequisites check
    if ! command -v curl &> /dev/null; then
        print_fail "curl is required but not installed"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        print_warning "jq is not installed - JSON validation tests will be limited"
    fi
    
    # Run all test suites
    check_service_availability || exit 1
    test_health_endpoint
    test_api_endpoints
    test_error_handling
    test_performance
    test_security
    test_load_handling
    
    # Generate final report
    echo
    generate_comprehensive_report
}

# Run the comprehensive test suite
main "$@"
