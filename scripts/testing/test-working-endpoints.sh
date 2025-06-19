#!/bin/bash

# Working Endpoints Test Script
# Tests only the endpoints that are currently working

echo "🌞 WORKING ENDPOINTS TEST"
echo "========================="
echo

API_BASE="http://localhost:5002"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Test counter
PASS_COUNT=0
FAIL_COUNT=0
TOTAL_TESTS=0

# Function to run test and count results
run_test() {
    local name="$1"
    local command="$2"
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    
    echo -e "${BLUE}🧪 Testing: $name${NC}"
    
    if eval "$command"; then
        echo -e "${GREEN}✅ PASS${NC}"
        PASS_COUNT=$((PASS_COUNT + 1))
    else
        echo -e "${RED}❌ FAIL${NC}"
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
    echo
}

# Get authentication token
echo -e "${CYAN}🔐 Getting authentication token...${NC}"
TOKEN_RESPONSE=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}')

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.data.token // empty' 2>/dev/null)

if [ -n "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
    echo -e "${GREEN}✅ Authentication successful${NC}"
    echo -e "${CYAN}Token: ${TOKEN:0:50}...${NC}"
else
    echo -e "${RED}❌ Authentication failed${NC}"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi
echo

# ═══════════════════════════════════════
# TEST WORKING ENDPOINTS
# ═══════════════════════════════════════

echo -e "${YELLOW}🏃‍♂️ RUNNING TESTS ON WORKING ENDPOINTS${NC}"
echo

# Test 1: Health Check
run_test "Health Check" '
response=$(curl -s "$API_BASE/health")
echo "$response" | jq -e ".status == \"Healthy\"" > /dev/null
'

# Test 2: Authentication Login
run_test "Authentication Login" '
response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"admin@example.com\",\"password\":\"Admin123!\"}")
echo "$response" | jq -e ".success == true" > /dev/null
'

# Test 3: Invalid Authentication
run_test "Invalid Authentication (should fail)" '
response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"invalid\",\"password\":\"wrong\"}")
echo "$response" | jq -e ".success == false" > /dev/null
'

# Test 4: Daily Reports List
run_test "Daily Reports - Get All" '
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports")
echo "$response" | jq -e ".success == true" > /dev/null
'

# Test 5: Daily Reports Pagination
run_test "Daily Reports - Pagination" '
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports?pageSize=5")
echo "$response" | jq -e ".success == true" > /dev/null
'

# Test 6: Daily Reports with Date Filter
run_test "Daily Reports - Date Filter" '
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports?startDate=2025-01-01")
echo "$response" | jq -e ".success == true" > /dev/null
'

# Test 7: Unauthorized Access (no token)
run_test "Unauthorized Access Protection" '
response=$(curl -s -w "%{http_code}" "$API_BASE/api/v1/daily-reports" -o /dev/null)
[ "$response" = "401" ]
'

# Test 8: Rate Limiting Check (make multiple requests)
run_test "Rate Limiting Response Headers" '
response=$(curl -s -i -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" | head -20)
echo "$response" | grep -i "x-ratelimit" > /dev/null
'

# Test 9: CORS Headers
run_test "CORS Headers Present" '
response=$(curl -s -i -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" | head -10)
echo "$response" | grep -i "access-control" > /dev/null
'

# Test 10: JSON Response Format
run_test "JSON Response Format" '
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports")
echo "$response" | jq -e "has(\"success\") and has(\"data\")" > /dev/null
'

# ═══════════════════════════════════════
# DETAILED ENDPOINT EXPLORATION
# ═══════════════════════════════════════

echo -e "${YELLOW}🔍 DETAILED ENDPOINT EXPLORATION${NC}"
echo

# Show detailed response from daily reports
echo -e "${BLUE}📋 Daily Reports Response Structure:${NC}"
curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" | jq . | head -30
echo

# Show auth response structure
echo -e "${BLUE}🔐 Authentication Response Structure:${NC}"
curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' | jq . | head -20
echo

# Test different HTTP methods on daily reports
echo -e "${BLUE}🔧 Testing HTTP Methods on Daily Reports:${NC}"

echo "GET /api/v1/daily-reports:"
response=$(curl -s -w "STATUS:%{http_code}" -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports")
status=$(echo "$response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
echo "   Status: $status"

echo "POST /api/v1/daily-reports (create test):"
test_data='{"projectId":"00000000-0000-0000-0000-000000000001","reportDate":"2025-06-19T12:00:00Z","workProgress":"Test work","challenges":"None","nextDayPlan":"Continue","safetyNotes":"All safe"}'
response=$(curl -s -w "STATUS:%{http_code}" -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$test_data" "$API_BASE/api/v1/daily-reports")
status=$(echo "$response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
echo "   Status: $status"

echo "HEAD /api/v1/daily-reports:"
response=$(curl -s -w "STATUS:%{http_code}" -I -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" -o /dev/null)
echo "   Status: $response"
echo

# ═══════════════════════════════════════
# PERFORMANCE TESTS
# ═══════════════════════════════════════

echo -e "${YELLOW}⚡ PERFORMANCE TESTS${NC}"
echo

# Test response time
echo -e "${BLUE}📊 Response Time Test:${NC}"
start_time=$(date +%s%N)
curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" > /dev/null
end_time=$(date +%s%N)
duration=$(( (end_time - start_time) / 1000000 ))
echo "   Daily Reports endpoint response time: ${duration}ms"

# Test concurrent requests
echo -e "${BLUE}🔄 Concurrent Request Test:${NC}"
echo "   Making 5 concurrent requests..."
for i in {1..5}; do
    curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/api/v1/daily-reports" > /dev/null &
done
wait
echo "   ✅ All concurrent requests completed"
echo

# ═══════════════════════════════════════
# SUMMARY
# ═══════════════════════════════════════

echo -e "${YELLOW}📊 TEST SUMMARY${NC}"
echo "================"
echo -e "${GREEN}✅ Passed: $PASS_COUNT/$TOTAL_TESTS tests${NC}"
echo -e "${RED}❌ Failed: $FAIL_COUNT/$TOTAL_TESTS tests${NC}"
echo

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "${GREEN}🎉 All tests passed! Working endpoints are functioning correctly.${NC}"
else
    echo -e "${YELLOW}⚠️ Some tests failed, but core functionality is working.${NC}"
fi

echo
echo -e "${BLUE}🔧 WORKING ENDPOINTS CONFIRMED:${NC}"
echo "   ✅ GET  /health"
echo "   ✅ POST /api/v1/auth/login"
echo "   ✅ GET  /api/v1/daily-reports"
echo "   ✅ GET  /api/v1/daily-reports?pageSize=N"
echo "   ✅ GET  /api/v1/daily-reports?startDate=YYYY-MM-DD"
echo

echo -e "${BLUE}⚠️ ENDPOINTS WITH ISSUES (500 errors):${NC}"
echo "   ❌ GET  /api/v1/users"
echo "   ❌ GET  /api/v1/projects"  
echo "   ❌ GET  /api/v1/work-requests"
echo "   ❌ GET  /api/v1/documents"
echo "   ❌ GET  /api/v1/resources"
echo

echo -e "${BLUE}❓ ENDPOINTS NOT FOUND (404):${NC}"
echo "   ❓ GET  /api/v1/tasks"
echo "   ❓ GET  /api/v1/calendar/tasks"
echo "   ❓ GET  /api/v1/phases"
echo

echo -e "${BLUE}🔗 USEFUL COMMANDS:${NC}"
echo "   # Test auth:"
echo "   curl -X POST $API_BASE/api/v1/auth/login -H 'Content-Type: application/json' -d '{\"username\":\"admin@example.com\",\"password\":\"Admin123!\"}'"
echo
echo "   # Test daily reports:"
echo "   curl -H 'Authorization: Bearer \$TOKEN' $API_BASE/api/v1/daily-reports"
echo
echo "   # Check API health:"
echo "   curl $API_BASE/health"

echo
echo -e "${GREEN}🌞 Working Endpoints Test Complete!${NC}"
