#!/bin/bash

# Simple WBS API Test Script
# Tests endpoints that don't require authentication

BASE_URL="http://localhost:5001"
API_VERSION="v1"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Solar Project Management API - WBS Testing ===${NC}"
echo -e "${BLUE}Testing basic endpoints without authentication${NC}"
echo ""

# Test 1: Health Check
echo -e "${YELLOW}1. Testing Health Endpoint${NC}"
response=$(curl -s -w "HTTPSTATUS:%{http_code}" "${BASE_URL}/Health")
http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ Health check passed${NC}"
    echo "Response: $body"
else
    echo -e "${RED}✗ Health check failed (HTTP $http_code)${NC}"
    echo "Response: $body"
fi
echo ""

# Test 2: Try to access WBS endpoints (should fail without auth)
echo -e "${YELLOW}2. Testing WBS Endpoints (should require authentication)${NC}"

endpoints=(
    "GET /api/${API_VERSION}/wbs"
    "GET /api/${API_VERSION}/wbs/hierarchy"
    "POST /api/${API_VERSION}/wbs/seed"
)

for endpoint in "${endpoints[@]}"; do
    method=$(echo $endpoint | cut -d' ' -f1)
    path=$(echo $endpoint | cut -d' ' -f2)
    
    echo -e "${BLUE}Testing: $method $path${NC}"
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" "${BASE_URL}${path}")
    elif [ "$method" = "POST" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST -H "Content-Type: application/json" "${BASE_URL}${path}")
    fi
    
    http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
    
    if [ "$http_code" = "401" ]; then
        echo -e "${GREEN}✓ Correctly requires authentication (HTTP 401)${NC}"
    elif [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        echo -e "${YELLOW}⚠ Endpoint accessible without auth (HTTP $http_code)${NC}"
        echo "Response: $body"
    else
        echo -e "${RED}✗ Unexpected response (HTTP $http_code)${NC}"
        echo "Response: $body"
    fi
    echo ""
done

# Test 3: Try authentication endpoints
echo -e "${YELLOW}3. Testing Authentication Endpoints${NC}"

# Try registration (should work)
echo -e "${BLUE}Testing Registration${NC}"
response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "${BASE_URL}/api/${API_VERSION}/auth/register" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "script_test_user",
        "email": "scripttest@test.com",
        "password": "ScriptTest123!",
        "fullName": "Script Test User",
        "roleId": 1
    }')

http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')

if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
    echo -e "${GREEN}✓ Registration endpoint accessible${NC}"
    echo "Response: $body"
elif [ "$http_code" = "400" ]; then
    echo -e "${YELLOW}⚠ Registration failed (likely user exists)${NC}"
    echo "Response: $body"
else
    echo -e "${RED}✗ Registration failed (HTTP $http_code)${NC}"
    echo "Response: $body"
fi
echo ""

# Try login
echo -e "${BLUE}Testing Login${NC}"
response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST "${BASE_URL}/api/${API_VERSION}/auth/login" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "admin@example.com",
        "password": "Admin123!"
    }')

http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ Login successful${NC}"
    echo "Response: $body"
    
    # Extract token if possible
    token=$(echo "$body" | jq -r '.data.token // .token // empty')
    if [ -n "$token" ]; then
        echo -e "${GREEN}✓ JWT Token extracted${NC}"
        echo "Token: $token"
        
        # Test WBS endpoint with token
        echo ""
        echo -e "${YELLOW}4. Testing WBS with Authentication${NC}"
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" "${BASE_URL}/api/${API_VERSION}/wbs" \
            -H "Authorization: Bearer $token")
        
        http_code=$(echo "$response" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
        body=$(echo "$response" | sed 's/HTTPSTATUS:[0-9]*$//')
        
        if [ "$http_code" = "200" ]; then
            echo -e "${GREEN}✓ WBS endpoint accessible with auth${NC}"
            echo "Response: $body"
        else
            echo -e "${RED}✗ WBS endpoint failed with auth (HTTP $http_code)${NC}"
            echo "Response: $body"
        fi
    fi
else
    echo -e "${RED}✗ Login failed (HTTP $http_code)${NC}"
    echo "Response: $body"
fi

echo ""
echo -e "${BLUE}=== Test Summary ===${NC}"
echo "Basic API connectivity and authentication flow tested."
echo "If authentication is working, you can run the full test script with:"
echo "  ./scripts/test-wbs-api.sh [jwt-token]"
