#!/bin/bash

# Docker Deployment Verification Script
# Tests the deployed API in Docker containers

echo "=========================================================="
echo "🐳 DOCKER DEPLOYMENT VERIFICATION"
echo "=========================================================="
echo

API_BASE="http://localhost:5002"

# Function to test endpoint with timeout
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    local token=$5
    
    echo "🧪 Testing: $description"
    echo "   🌐 $method $API_BASE$endpoint"
    
    local auth_header=""
    if [ -n "$token" ]; then
        auth_header="-H \"Authorization: Bearer $token\""
    fi
    
    local data_param=""
    if [ -n "$data" ]; then
        data_param="-d '$data'"
    fi
    
    # Use timeout to prevent hanging
    local result
    if command -v timeout >/dev/null 2>&1; then
        result=$(timeout 10s curl -s -w "HTTP_STATUS:%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Content-Type: application/json" \
            $auth_header \
            $data_param 2>/dev/null)
    else
        # For macOS without timeout command
        result=$(curl -s -w "HTTP_STATUS:%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Content-Type: application/json" \
            $auth_header \
            $data_param 2>/dev/null)
    fi
    
    if [ $? -eq 0 ] && [[ "$result" == *"HTTP_STATUS:2"* ]]; then
        echo "   ✅ SUCCESS"
        status=$(echo "$result" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
        body=$(echo "$result" | sed 's/HTTP_STATUS:[0-9]*$//')
        echo "   📋 Status: $status"
        if [ ${#body} -lt 200 ]; then
            echo "   📋 Response: $body"
        else
            echo "   📋 Response: $(echo "$body" | head -c 100)..."
        fi
    else
        echo "   ❌ FAILED or TIMEOUT"
    fi
    echo
}

# Check container status
echo "=== STEP 1: CONTAINER STATUS ==="
echo "🐳 Checking Docker containers..."
docker-compose ps
echo

# Check API health
echo "=== STEP 2: API HEALTH CHECK ==="
test_endpoint "GET" "/health" "API Health Check"

# Test authentication
echo "=== STEP 3: AUTHENTICATION ==="
test_endpoint "POST" "/api/v1/auth/login" "Admin Login" '{"username":"test_admin","password":"Admin123!"}'

# Get token for further tests
echo "🔐 Getting authentication token..."
TOKEN_RESPONSE=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"username":"test_admin","password":"Admin123!"}' 2>/dev/null)

ADMIN_TOKEN=$(echo "$TOKEN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -n "$ADMIN_TOKEN" ]; then
    echo "   ✅ Token obtained successfully"
    echo "   🔑 Token: ${ADMIN_TOKEN:0:50}..."
else
    echo "   ❌ Failed to get token"
    echo "   📋 Response: $TOKEN_RESPONSE"
    ADMIN_TOKEN=""
fi
echo

# Test protected endpoints
echo "=== STEP 4: PROTECTED ENDPOINTS ==="
if [ -n "$ADMIN_TOKEN" ]; then
    test_endpoint "GET" "/api/v1/work-requests/pending-approval" "Pending Approvals" "" "$ADMIN_TOKEN"
    test_endpoint "GET" "/api/v1/users" "Users List" "" "$ADMIN_TOKEN"
else
    echo "⚠️  Skipping protected endpoint tests (no token)"
fi

# Test database connectivity
echo "=== STEP 5: DATABASE VERIFICATION ==="
echo "🗄️  Checking database tables..."
docker exec solar-projects-db psql -U postgres -d SolarProjectsDb -c "\dt" 2>/dev/null | grep -E "(WorkRequestApprovals|WorkRequestNotifications|WorkRequests)" || echo "Database tables check failed"
echo

# Check logs for errors
echo "=== STEP 6: LOG ANALYSIS ==="
echo "📊 Recent API logs:"
docker logs solar-projects-api --tail 5 2>/dev/null || echo "Failed to get API logs"
echo

echo "=========================================================="
echo "🎯 DEPLOYMENT VERIFICATION SUMMARY"
echo "=========================================================="
echo

# Final status check
if curl -s -f "$API_BASE/health" >/dev/null 2>&1; then
    echo "✅ API Status: HEALTHY"
else
    echo "❌ API Status: UNHEALTHY"
fi

if docker-compose ps | grep -q "Up"; then
    echo "✅ Containers: RUNNING"
else
    echo "❌ Containers: NOT RUNNING"
fi

if [ -n "$ADMIN_TOKEN" ]; then
    echo "✅ Authentication: WORKING"
else
    echo "❌ Authentication: FAILED"
fi

echo
echo "🔗 API Endpoints:"
echo "   📍 Health: $API_BASE/health"
echo "   📍 Swagger: $API_BASE/swagger"
echo "   📍 API: $API_BASE/api/v1/"
echo

echo "🐳 Docker Commands:"
echo "   📋 Check status: docker-compose ps"
echo "   📊 View logs: docker logs solar-projects-api"
echo "   📊 View DB logs: docker logs solar-projects-db"
echo "   🛑 Stop: docker-compose down"
echo "   🔄 Restart: docker-compose restart"
echo

echo "🎊 Docker deployment verification complete!"
echo "=========================================================="
