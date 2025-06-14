#!/bin/bash

# Quick Admin API Status Check
# Fast verification of admin endpoint status

echo "🔐 ADMIN API QUICK STATUS CHECK"
echo "==============================="

API_BASE="http://localhost:5002"
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Quick health check
echo -n "🏥 API Health: "
if curl -s -f "$API_BASE/health" > /dev/null; then
    echo -e "${GREEN}✅ Online${NC}"
else
    echo -e "${RED}❌ Offline${NC}"
    exit 1
fi

# Quick admin login test
echo -n "🔐 Admin Login: "
login_response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"username\": \"$ADMIN_USERNAME\", \"password\": \"$ADMIN_PASSWORD\"}")

if echo "$login_response" | grep -q '"success":true'; then
    echo -e "${GREEN}✅ Working${NC}"
    JWT_TOKEN=$(echo "$login_response" | jq -r '.data.token' 2>/dev/null)
else
    echo -e "${RED}❌ Failed${NC}"
    exit 1
fi

# Quick endpoint checks
test_endpoint() {
    local endpoint=$1
    local description=$2
    
    echo -n "$description: "
    
    response=$(curl -s -w "%{http_code}" -o /dev/null \
        -H "Authorization: Bearer $JWT_TOKEN" \
        "$API_BASE$endpoint")
    
    case $response in
        200|201) echo -e "${GREEN}✅ $response${NC}" ;;
        400|422) echo -e "${YELLOW}⚠️  $response (Validation)${NC}" ;;
        429) echo -e "${YELLOW}⚠️  $response (Rate Limited)${NC}" ;;
        *) echo -e "${RED}❌ $response${NC}" ;;
    esac
}

echo ""
echo "📊 Core Endpoints:"
test_endpoint "/api/v1/users" "👥 User Management"
test_endpoint "/api/v1/projects" "📋 Projects"
test_endpoint "/api/v1/tasks" "✅ Tasks"
test_endpoint "/api/v1/daily-reports" "📊 Daily Reports"
test_endpoint "/api/v1/work-requests" "🔧 Work Requests"
test_endpoint "/api/v1/calendar" "📅 Calendar"

echo ""
echo "📋 Admin Test Commands:"
echo "  Full Test:    ./test-admin-endpoints.sh"
echo "  Analyze:      ./analyze-admin-test-results.sh"
echo "  Docs:         cat ADMIN_ENDPOINTS_REFERENCE.md"
echo "  Results:      cat ADMIN_TEST_RESULTS.md"
