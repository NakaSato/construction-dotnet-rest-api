#!/bin/bash

# Production API Testing Script for Solar Projects API
echo "🧪 Production API Testing Script"
echo "================================"

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
NC='\033[0m' # No Color

# Test results
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Function to print colored output
print_test_header() {
    echo -e "\n${BLUE}📋 Testing: $1${NC}"
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
}

print_info() {
    echo -e "${BLUE}ℹ️ $1${NC}"
}

# Function to make HTTP request and validate response
test_endpoint() {
    local method="$1"
    local endpoint="$2"
    local expected_status="$3"
    local description="$4"
    local data="$5"
    
    ((TOTAL_TESTS++))
    
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
    body=$(echo "$response" | head -n -1)
    
    if [ "$status_code" = "$expected_status" ]; then
        print_success "$description - Status: $status_code"
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
    
    ((TOTAL_TESTS++))
    
    print_info "Testing JSON format: $endpoint"
    
    response=$(curl -s "$endpoint")
    
    if echo "$response" | jq . > /dev/null 2>&1; then
        print_success "$description - Valid JSON response"
        return 0
    else
        print_fail "$description - Invalid JSON response"
        echo "   Response: $(echo "$response" | head -c 200)..."
        return 1
    fi
}

# Function to check if service is available
check_service_availability() {
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
    print_test_header "Health Endpoint"
    
    test_endpoint "GET" "$HEALTH_ENDPOINT" "200" "Health check endpoint"
    test_json_response "$HEALTH_ENDPOINT" "Health endpoint JSON format"
    
    # Test specific health response content
    print_info "Validating health response content..."
    response=$(curl -s "$HEALTH_ENDPOINT")
    
    if echo "$response" | jq -e '.status' > /dev/null 2>&1; then
        status=$(echo "$response" | jq -r '.status')
        if [ "$status" = "Healthy" ]; then
            print_success "Health status is 'Healthy'"
        else
            print_fail "Health status is '$status' (expected 'Healthy')"
        fi
    else
        print_fail "Health response missing 'status' field"
    fi
    
    if echo "$response" | jq -e '.version' > /dev/null 2>&1; then
        version=$(echo "$response" | jq -r '.version')
        print_success "Version information present: $version"
    else
        print_warning "Version information not found in health response"
    fi
}

# Function to test Swagger documentation
test_swagger_endpoint() {
    print_test_header "API Documentation (Swagger)"
    
    test_endpoint "GET" "$SWAGGER_ENDPOINT" "200" "Swagger UI page"
    
    # Test swagger.json endpoint
    test_endpoint "GET" "$PROD_URL/swagger/v1/swagger.json" "200" "Swagger JSON specification"
    test_json_response "$PROD_URL/swagger/v1/swagger.json" "Swagger JSON format"
}

# Function to test core API endpoints
test_core_endpoints() {
    print_test_header "Core API Endpoints"
    
    # Test Projects endpoints
    test_endpoint "GET" "$API_BASE/projects" "200" "Get all projects"
    test_json_response "$API_BASE/projects" "Projects JSON response"
    
    # Test Daily Reports endpoints
    test_endpoint "GET" "$API_BASE/daily-reports" "200" "Get all daily reports"
    test_json_response "$API_BASE/daily-reports" "Daily reports JSON response"
    
    # Test Work Requests endpoints
    test_endpoint "GET" "$API_BASE/work-requests" "200" "Get all work requests"
    test_json_response "$API_BASE/work-requests" "Work requests JSON response"
    
    # Test Debug endpoint (if available)
    test_endpoint "GET" "$API_BASE/debug/info" "200" "Debug info endpoint"
}

# Function to test error handling
test_error_handling() {
    print_test_header "Error Handling"
    
    # Test 404 for non-existent endpoint
    test_endpoint "GET" "$API_BASE/non-existent-endpoint" "404" "Non-existent endpoint returns 404"
    
    # Test 404 for non-existent resource
    test_endpoint "GET" "$API_BASE/projects/99999" "404" "Non-existent project returns 404"
    
    # Test 405 for unsupported method (if applicable)
    test_endpoint "PATCH" "$API_BASE/projects" "405" "Unsupported HTTP method returns 405" || true
}

# Function to test CRUD operations (with test data)
test_crud_operations() {
    print_test_header "CRUD Operations Test"
    
    # Create test project data
    test_project='{
        "name": "Test Solar Project API",
        "description": "Test project created by API testing script",
        "location": "Test Location",
        "status": "Planning"
    }'
    
    print_info "Testing project creation..."
    test_endpoint "POST" "$API_BASE/projects" "201" "Create new project" "$test_project"
    
    # If creation was successful, test getting the created project
    if [ $? -eq 0 ]; then
        print_info "Testing project retrieval after creation..."
        # Note: In a real test, you'd parse the ID from the creation response
        # For now, just test the general projects endpoint
        test_endpoint "GET" "$API_BASE/projects" "200" "Get projects after creation"
    fi
}

# Function to test security headers
test_security_headers() {
    print_test_header "Security Headers"
    
    print_info "Checking security headers..."
    
    headers=$(curl -s -I "$PROD_URL")
    
    if echo "$headers" | grep -i "strict-transport-security" > /dev/null; then
        print_success "HTTPS Strict Transport Security header present"
    else
        print_warning "HTTPS Strict Transport Security header missing"
    fi
    
    if echo "$headers" | grep -i "x-content-type-options" > /dev/null; then
        print_success "X-Content-Type-Options header present"
    else
        print_warning "X-Content-Type-Options header missing"
    fi
    
    if echo "$headers" | grep -i "x-frame-options" > /dev/null; then
        print_success "X-Frame-Options header present"
    else
        print_warning "X-Frame-Options header missing"
    fi
}

# Function to test performance
test_performance() {
    print_test_header "Performance Test"
    
    print_info "Measuring response times..."
    
    # Test health endpoint response time
    start_time=$(date +%s%N)
    curl -s "$HEALTH_ENDPOINT" > /dev/null
    end_time=$(date +%s%N)
    response_time=$(( (end_time - start_time) / 1000000 )) # Convert to milliseconds
    
    if [ $response_time -lt 2000 ]; then
        print_success "Health endpoint response time: ${response_time}ms (< 2s)"
    elif [ $response_time -lt 5000 ]; then
        print_warning "Health endpoint response time: ${response_time}ms (2-5s)"
    else
        print_fail "Health endpoint response time: ${response_time}ms (> 5s)"
    fi
    
    # Test API endpoint response time
    start_time=$(date +%s%N)
    curl -s "$API_BASE/projects" > /dev/null
    end_time=$(date +%s%N)
    api_response_time=$(( (end_time - start_time) / 1000000 ))
    
    if [ $api_response_time -lt 3000 ]; then
        print_success "Projects API response time: ${api_response_time}ms (< 3s)"
    elif [ $api_response_time -lt 8000 ]; then
        print_warning "Projects API response time: ${api_response_time}ms (3-8s)"
    else
        print_fail "Projects API response time: ${api_response_time}ms (> 8s)"
    fi
}

# Function to generate test report
generate_report() {
    echo -e "\n${BLUE}📊 Test Report${NC}"
    echo "=============="
    echo "Total Tests: $TOTAL_TESTS"
    echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
    
    success_rate=$(( PASSED_TESTS * 100 / TOTAL_TESTS ))
    echo "Success Rate: $success_rate%"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "\n${GREEN}🎉 All tests passed! Production API is working correctly.${NC}"
        return 0
    elif [ $success_rate -ge 80 ]; then
        echo -e "\n${YELLOW}⚠️ Most tests passed, but some issues detected.${NC}"
        return 1
    else
        echo -e "\n${RED}❌ Multiple test failures detected. Please investigate.${NC}"
        return 1
    fi
}

# Function to test with authentication (if needed)
test_authentication() {
    print_test_header "Authentication Test"
    
    print_info "Testing endpoints that might require authentication..."
    
    # Test if any endpoints require authentication
    # This will help identify which endpoints need JWT tokens
    
    # Try accessing a potentially protected endpoint
    response=$(curl -s -w "\n%{http_code}" "$API_BASE/projects" -H "Authorization: Bearer invalid-token")
    status_code=$(echo "$response" | tail -n1)
    
    if [ "$status_code" = "401" ]; then
        print_info "Authentication is required for some endpoints (401 Unauthorized)"
        print_warning "To test authenticated endpoints, add JWT token to the script"
    elif [ "$status_code" = "200" ]; then
        print_info "Endpoints are accessible without authentication"
    else
        print_info "Unexpected response for authentication test: $status_code"
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
        print_warning "jq is not installed - JSON validation tests will be skipped"
    fi
    
    # Run all tests
    check_service_availability || exit 1
    test_health_endpoint
    test_swagger_endpoint
    test_core_endpoints
    test_error_handling
    test_crud_operations
    test_security_headers
    test_performance
    test_authentication
    
    # Generate final report
    echo
    generate_report
    
    echo -e "\n${BLUE}🔗 Useful Links:${NC}"
    echo "Application: $PROD_URL"
    echo "Health Check: $HEALTH_ENDPOINT"
    echo "API Documentation: $SWAGGER_ENDPOINT"
    echo "API Base URL: $API_BASE"
    
    echo -e "\n${BLUE}💡 Next Steps:${NC}"
    echo "1. Review any failed tests above"
    echo "2. Check application logs if needed:"
    echo "   az webapp log tail --name solar-projects-api --resource-group solar-projects-rg"
    echo "3. Monitor the application in Azure portal"
    echo "4. Set up automated monitoring and alerts"
}

# Run the tests
main "$@"
