#!/bin/bash

# Production API Authentication Testing Script
# This script tests authenticated endpoints with JWT tokens
echo "🔐 Production API Authentication Testing"
echo "========================================"

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Default test credentials (these would be environment-specific)
DEFAULT_USERNAME="test@example.com"
DEFAULT_PASSWORD="TestPassword123!"

# Function to get JWT token
get_jwt_token() {
    local username="${1:-$DEFAULT_USERNAME}"
    local password="${2:-$DEFAULT_PASSWORD}"
    
    echo -e "${BLUE}🔑 Attempting to authenticate...${NC}"
    echo "Username: $username"
    
    local auth_payload="{\"email\":\"$username\",\"password\":\"$password\"}"
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$auth_payload" \
        "$API_BASE/auth/login" \
        -w '%{http_code}')
    
    local body=$(echo "$response" | sed 's/[0-9]*$//')
    local status=$(echo "$response" | grep -o '[0-9]*$')
    
    if [[ "$status" == "200" ]]; then
        local token=$(echo "$body" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        if [[ -n "$token" ]]; then
            echo -e "${GREEN}✅ Authentication successful${NC}"
            echo "$token"
            return 0
        else
            echo -e "${RED}❌ Authentication failed: No token in response${NC}"
            return 1
        fi
    else
        echo -e "${RED}❌ Authentication failed: HTTP $status${NC}"
        echo "Response: $body"
        return 1
    fi
}

# Function to test authenticated endpoint
test_authenticated_endpoint() {
    local name="$1"
    local url="$2"
    local token="$3"
    local expected_status="${4:-200}"
    local method="${5:-GET}"
    local payload="$6"
    
    echo -e "\n${CYAN}🧪 Testing: $name${NC}"
    
    local curl_cmd="curl -s -w '%{http_code}|%{time_total}'"
    curl_cmd="$curl_cmd -H 'Authorization: Bearer $token'"
    curl_cmd="$curl_cmd -H 'Content-Type: application/json'"
    
    if [[ "$method" == "POST" && -n "$payload" ]]; then
        curl_cmd="$curl_cmd -X POST -d '$payload'"
    elif [[ "$method" != "GET" ]]; then
        curl_cmd="$curl_cmd -X $method"
    fi
    
    curl_cmd="$curl_cmd '$url'"
    
    local response=$(eval $curl_cmd)
    local body=$(echo "$response" | sed 's/|[^|]*|[^|]*$//')
    local status=$(echo "$response" | grep -o '[0-9]*|[^|]*$' | cut -d'|' -f1)
    local time=$(echo "$response" | grep -o '[0-9]*|[^|]*$' | cut -d'|' -f2)
    
    if [[ "$status" == "$expected_status" ]]; then
        echo -e "${GREEN}✅ PASS: $name - Status: $status, Time: ${time}s${NC}"
        
        # Show response preview for successful requests
        if [[ "$status" == "200" && ${#body} -lt 500 ]]; then
            echo -e "${BLUE}📄 Response preview:${NC}"
            echo "$body" | python3 -m json.tool 2>/dev/null || echo "$body" | head -c 200
            if [[ ${#body} -gt 200 ]]; then echo "..."; fi
        fi
        return 0
    else
        echo -e "${RED}❌ FAIL: $name - Expected: $expected_status, Got: $status${NC}"
        if [[ -n "$body" && ${#body} -lt 300 ]]; then
            echo -e "${YELLOW}Response: $body${NC}"
        fi
        return 1
    fi
}

# Function to test all authenticated endpoints
test_authenticated_endpoints() {
    local token="$1"
    local passed=0
    local total=0
    
    echo -e "\n${CYAN}🔒 Testing Authenticated Endpoints${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Test projects endpoints
    if test_authenticated_endpoint "Get Projects" "$API_BASE/projects" "$token"; then ((passed++)); fi; ((total++))
    
    if test_authenticated_endpoint "Get Projects Legacy" "$API_BASE/projects/legacy" "$token"; then ((passed++)); fi; ((total++))
    
    # Test users endpoints
    if test_authenticated_endpoint "Get Users" "$API_BASE/users" "$token"; then ((passed++)); fi; ((total++))
    
    # Test daily reports
    if test_authenticated_endpoint "Get Daily Reports" "$API_BASE/daily-reports" "$token"; then ((passed++)); fi; ((total++))
    
    # Test work requests
    if test_authenticated_endpoint "Get Work Requests" "$API_BASE/work-requests" "$token"; then ((passed++)); fi; ((total++))
    
    # Test master plans
    if test_authenticated_endpoint "Get Master Plans" "$API_BASE/master-plans" "$token"; then ((passed++)); fi; ((total++))
    
    # Test calendar
    if test_authenticated_endpoint "Get Calendar Events" "$API_BASE/calendar/events" "$token"; then ((passed++)); fi; ((total++))
    
    # Test tasks
    if test_authenticated_endpoint "Get Tasks" "$API_BASE/tasks" "$token"; then ((passed++)); fi; ((total++))
    
    # Test documents
    if test_authenticated_endpoint "Get Documents" "$API_BASE/documents" "$token"; then ((passed++)); fi; ((total++))
    
    # Test resources
    if test_authenticated_endpoint "Get Resources" "$API_BASE/resources" "$token"; then ((passed++)); fi; ((total++))
    
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo -e "${BLUE}📊 Authenticated Tests Summary: $passed/$total passed${NC}"
    
    return $((total - passed))
}

# Function to test token expiration
test_token_security() {
    echo -e "\n${CYAN}🔐 Testing Token Security${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Test with invalid token
    echo -e "${CYAN}🧪 Testing: Invalid Token${NC}"
    local invalid_response=$(curl -s -w '%{http_code}' -o /dev/null \
        -H "Authorization: Bearer invalid_token_12345" \
        "$API_BASE/projects")
    
    if [[ "$invalid_response" == "401" ]]; then
        echo -e "${GREEN}✅ PASS: Invalid token correctly rejected (401)${NC}"
    else
        echo -e "${RED}❌ FAIL: Invalid token not rejected (got $invalid_response)${NC}"
    fi
    
    # Test with no token
    echo -e "${CYAN}🧪 Testing: No Token${NC}"
    local no_token_response=$(curl -s -w '%{http_code}' -o /dev/null "$API_BASE/projects")
    
    if [[ "$no_token_response" == "401" ]]; then
        echo -e "${GREEN}✅ PASS: Missing token correctly rejected (401)${NC}"
    else
        echo -e "${RED}❌ FAIL: Missing token not rejected (got $no_token_response)${NC}"
    fi
    
    # Test with malformed Authorization header
    echo -e "${CYAN}🧪 Testing: Malformed Header${NC}"
    local malformed_response=$(curl -s -w '%{http_code}' -o /dev/null \
        -H "Authorization: InvalidFormat token123" \
        "$API_BASE/projects")
    
    if [[ "$malformed_response" == "401" ]]; then
        echo -e "${GREEN}✅ PASS: Malformed header correctly rejected (401)${NC}"
    else
        echo -e "${RED}❌ FAIL: Malformed header not rejected (got $malformed_response)${NC}"
    fi
}

# Main function
main() {
    echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
    echo -e "${BLUE}⏰ Started: $(date)${NC}"
    
    # Check if credentials were provided
    local username="$1"
    local password="$2"
    
    if [[ -z "$username" ]]; then
        echo ""
        echo -e "${YELLOW}⚠️  No credentials provided. Using default test credentials.${NC}"
        echo -e "${YELLOW}   For custom credentials, run: $0 <username> <password>${NC}"
        echo -e "${YELLOW}   Note: Default credentials are likely to fail in production.${NC}"
        echo ""
        username="$DEFAULT_USERNAME"
        password="$DEFAULT_PASSWORD"
    fi
    
    # Test authentication
    local token=$(get_jwt_token "$username" "$password")
    
    if [[ $? -eq 0 && -n "$token" ]]; then
        # Test authenticated endpoints
        test_authenticated_endpoints "$token"
        auth_result=$?
        
        # Test token security
        test_token_security
        
        echo ""
        echo -e "${CYAN}🎯 Final Results${NC}"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        
        if [[ $auth_result -eq 0 ]]; then
            echo -e "${GREEN}🎉 All authenticated endpoints working correctly!${NC}"
        else
            echo -e "${YELLOW}⚠️  Some authenticated endpoints may have issues.${NC}"
            echo -e "${BLUE}💡 This could be due to:${NC}"
            echo "   • Placeholder service implementations"
            echo "   • Missing database data"
            echo "   • Different authentication requirements"
        fi
        
        exit $auth_result
    else
        echo ""
        echo -e "${RED}❌ Cannot test authenticated endpoints without valid token.${NC}"
        echo ""
        echo -e "${BLUE}💡 Possible reasons:${NC}"
        echo "   • Authentication service not fully implemented"
        echo "   • Invalid test credentials"
        echo "   • Authentication endpoint not available"
        echo ""
        echo -e "${BLUE}🔧 Alternatives:${NC}"
        echo "   • Test with valid production credentials"
        echo "   • Check authentication implementation"
        echo "   • Use quick health check for basic validation:"
        echo "     ./scripts/quick-health-check.sh"
        
        exit 1
    fi
}

# Help function
show_help() {
    echo "Usage: $0 [username] [password]"
    echo ""
    echo "Test authenticated endpoints in production API"
    echo ""
    echo "Arguments:"
    echo "  username    Email address for authentication (optional)"
    echo "  password    Password for authentication (optional)"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Use default test credentials"
    echo "  $0 user@example.com mypassword       # Use provided credentials"
    echo ""
    echo "Note: Default test credentials are likely to fail in production."
    echo "      Provide valid production credentials for full testing."
}

# Check for help flag
if [[ "$1" == "-h" || "$1" == "--help" ]]; then
    show_help
    exit 0
fi

# Run main function
main "$@"
