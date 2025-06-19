#!/bin/bash

# Quick Production API Health Check Script
# Simple script for fast validation of production API health
echo "⚡ Quick Production API Health Check"
echo "===================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Quick test function
quick_test() {
    local name="$1"
    local url="$2"
    local expected="$3"
    
    echo -n "Testing $name... "
    local response=$(curl -s -w '%{http_code}' -o /dev/null --max-time 10 "$url")
    
    if [[ "$response" == "$expected" ]]; then
        echo -e "${GREEN}✅ PASS ($response)${NC}"
        return 0
    else
        echo -e "${RED}❌ FAIL (got $response, expected $expected)${NC}"
        return 1
    fi
}

# Function to get API status with details
get_api_details() {
    echo -e "\n${BLUE}📋 API Details:${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Get health details
    local health_response=$(curl -s "$PROD_URL/health" 2>/dev/null)
    if [[ $? -eq 0 && -n "$health_response" ]]; then
        echo "Health Check Response:"
        echo "$health_response" | python3 -m json.tool 2>/dev/null || echo "$health_response"
    else
        echo "Could not retrieve health details"
    fi
    
    echo ""
    
    # Get test endpoint details
    local test_response=$(curl -s "$API_BASE/projects/test" 2>/dev/null)
    if [[ $? -eq 0 && -n "$test_response" ]]; then
        echo "Test Endpoint Response:"
        echo "$test_response" | python3 -m json.tool 2>/dev/null || echo "$test_response"
    else
        echo "Could not retrieve test endpoint details"
    fi
}

# Main execution
echo -e "${BLUE}🌐 Production URL: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Timestamp: $(date)${NC}"
echo ""

# Track results
passed=0
total=0

# Core health checks
echo "Running core health checks..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if quick_test "Health Endpoint" "$PROD_URL/health" "200"; then ((passed++)); fi
((total++))

if quick_test "Test Endpoint" "$API_BASE/projects/test" "200"; then ((passed++)); fi
((total++))

if quick_test "Auth Check (expect 401)" "$API_BASE/projects" "401"; then ((passed++)); fi
((total++))

if quick_test "404 Handling" "$API_BASE/nonexistent" "404"; then ((passed++)); fi
((total++))

# Quick performance test
echo -n "Performance Check... "
start_time=$(date +%s.%N)
response=$(curl -s -w '%{http_code}' -o /dev/null "$PROD_URL/health")
end_time=$(date +%s.%N)
duration=$(echo "$end_time - $start_time" | bc -l 2>/dev/null || echo "N/A")

if [[ "$response" == "200" ]]; then
    if [[ "$duration" != "N/A" ]] && (( $(echo "$duration < 3.0" | bc -l 2>/dev/null || echo "0") )); then
        echo -e "${GREEN}✅ PASS (${duration}s)${NC}"
        ((passed++))
    else
        echo -e "${YELLOW}⚠️  SLOW (${duration}s)${NC}"
    fi
else
    echo -e "${RED}❌ FAIL${NC}"
fi
((total++))

# Summary
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "${BLUE}📊 Summary: $passed/$total tests passed${NC}"

if [[ $passed -eq $total ]]; then
    echo -e "${GREEN}🎉 All checks passed! API is healthy and ready.${NC}"
    get_api_details
    exit 0
elif [[ $passed -ge $((total - 1)) ]]; then
    echo -e "${YELLOW}⚠️  Most checks passed. API appears functional with minor issues.${NC}"
    get_api_details
    exit 0
else
    echo -e "${RED}❌ Multiple checks failed. API may have significant issues.${NC}"
    echo ""
    echo -e "${BLUE}💡 Run the comprehensive test for detailed analysis:${NC}"
    echo "   ./scripts/test-production-api-enhanced.sh"
    exit 1
fi
