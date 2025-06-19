#!/bin/bash

# Test script for specific endpoints with 500 errors
# Focus on: GET /api/v1/users, /api/v1/projects, /api/v1/work-requests, /api/v1/documents, /api/v1/resources

echo "🔍 Testing Specific Endpoints with 500 Errors"
echo "=============================================="

API_BASE="http://localhost:5002"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    
    echo -e "${BLUE}🔄 Testing: $method $endpoint${NC}"
    echo "   📝 $description"
    
    response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X $method \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        "$API_BASE$endpoint")
    
    status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')
    
    case $status in
        200|201)
            echo -e "   ${GREEN}✅ SUCCESS ($status)${NC}"
            ;;
        4*)
            echo -e "   ${YELLOW}⚠️  CLIENT ERROR ($status)${NC}"
            ;;
        500)
            echo -e "   ${RED}❌ SERVER ERROR ($status)${NC}"
            echo "   📄 Error: $(echo "$body" | head -3)"
            ;;
        *)
            echo -e "   ${YELLOW}❓ UNEXPECTED ($status)${NC}"
            ;;
    esac
    echo ""
}

# 1. Get authentication token
echo -e "${BLUE}🔐 Step 1: Authentication${NC}"
TOKEN=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin@example.com", "password": "Admin123!"}' | \
  jq -r '.data.token // empty')

if [ -z "$TOKEN" ]; then
    echo -e "${RED}❌ Authentication failed${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Authentication successful${NC}"
echo ""

# 2. Test the specific problematic endpoints
echo -e "${BLUE}📋 Step 2: Testing Problematic Endpoints${NC}"
echo ""

test_endpoint "GET" "/api/v1/users" "Get all users"
test_endpoint "GET" "/api/v1/projects" "Get all projects"
test_endpoint "GET" "/api/v1/work-requests" "Get all work requests"
test_endpoint "GET" "/api/v1/documents" "Get all documents"
test_endpoint "GET" "/api/v1/resources" "Get all resources"

# 3. Test health endpoints (should work)
echo -e "${BLUE}🏥 Step 3: Testing Working Endpoints (for comparison)${NC}"
echo ""

test_endpoint "GET" "/health" "Basic health check"
test_endpoint "GET" "/api/v1/daily-reports" "Get daily reports (should work)"

echo "🎯 Test Complete!"
echo ""
echo "Summary:"
echo "- If you see 500 errors, these are due to missing service registrations in Program.cs"
echo "- The services exist but are not registered in the DI container"
echo "- I'll fix these next..."
