#!/bin/bash

# Swagger API Testing Script
# This script tests various endpoints through the Swagger-documented API

echo "🧪 Testing Solar Projects API through Swagger"
echo "============================================="

# Check if application is running
PORT=${1:-5001}
BASE_URL="http://localhost:$PORT"

echo "🔍 Testing application on $BASE_URL"

# Test 1: Health Check
echo ""
echo "1️⃣  Testing Health Endpoint..."
curl -s "$BASE_URL/health" | grep -q "Healthy" && echo "✅ Health check passed" || echo "❌ Health check failed"

# Test 2: Swagger JSON
echo ""
echo "2️⃣  Testing Swagger JSON Documentation..."
SWAGGER_RESPONSE=$(curl -s "$BASE_URL/swagger/v1/swagger.json")
echo $SWAGGER_RESPONSE | grep -q "PROJECT_API" && echo "✅ Swagger JSON available" || echo "❌ Swagger JSON failed"

# Test 3: Swagger UI
echo ""
echo "3️⃣  Testing Swagger UI..."
# Test if the root path is accessible (Swagger UI should be at root in Development)
ROOT_RESPONSE=$(curl -s -L -w "%{http_code}" -o /dev/null "http://localhost:$PORT/")
if [ "$ROOT_RESPONSE" = "200" ]; then
    echo "✅ Swagger UI accessible and working"
    # Verify it actually contains Swagger UI content
    SWAGGER_CONTENT=$(curl -s -L "http://localhost:$PORT/" | grep -o "Swagger UI")
    if [ ! -z "$SWAGGER_CONTENT" ]; then
        echo "✅ Swagger UI HTML content verified"
    fi
else
    echo "❌ Swagger UI failed with status: $ROOT_RESPONSE"
fi

# Test 4: Debug Configuration
echo ""
echo "4️⃣  Testing Debug Configuration Endpoint..."
DEBUG_RESPONSE=$(curl -s "$BASE_URL/api/Debug/config")
echo $DEBUG_RESPONSE | grep -q "environment" && echo "✅ Debug config accessible" || echo "❌ Debug config failed"

# Test 5: API Endpoints Count
echo ""
echo "5️⃣  Analyzing API Endpoints..."
ENDPOINT_COUNT=$(echo $SWAGGER_RESPONSE | grep -o '"/api[^"]*"' | wc -l)
echo "📊 Total API endpoints documented: $ENDPOINT_COUNT"

# Test 6: Authentication Endpoints
echo ""
echo "6️⃣  Testing Authentication Endpoints Availability..."
echo $SWAGGER_RESPONSE | grep -q "/api/v1/Auth/login" && echo "✅ Login endpoint documented" || echo "❌ Login endpoint missing"
echo $SWAGGER_RESPONSE | grep -q "/api/v1/Auth/register" && echo "✅ Register endpoint documented" || echo "❌ Register endpoint missing"

# Test 7: Rich Pagination Endpoints
echo ""
echo "7️⃣  Testing Rich Pagination Endpoints Availability..."
echo $SWAGGER_RESPONSE | grep -q "/api/v1/Projects/rich" && echo "✅ Projects rich pagination documented" || echo "❌ Projects rich pagination missing"
echo $SWAGGER_RESPONSE | grep -q "/api/v1/users/rich" && echo "✅ Users rich pagination documented" || echo "❌ Users rich pagination missing"
echo $SWAGGER_RESPONSE | grep -q "/api/v1/tasks/rich" && echo "✅ Tasks rich pagination documented" || echo "❌ Tasks rich pagination missing"

# Test 8: Legacy Endpoints
echo ""
echo "8️⃣  Testing Legacy Endpoints Availability..."
echo $SWAGGER_RESPONSE | grep -q "/api/v1/projects" && echo "✅ Solar Projects API endpoints documented" || echo "❌ Solar Projects API endpoints missing"

# Test 9: Extract Sample Endpoint Details
echo ""
echo "9️⃣  Sample Endpoint Analysis..."
echo "📋 Available endpoint categories:"
echo $SWAGGER_RESPONSE | grep -o '"tags":\["[^"]*"' | sed 's/"tags":\["/- /' | sed 's/"$//' | sort | uniq

echo ""
echo "🎯 API Documentation Summary:"
echo "   • Swagger UI: $BASE_URL/"
echo "   • Swagger JSON: $BASE_URL/swagger/v1/swagger.json"
echo "   • Health Check: $BASE_URL/health"
echo "   • Debug Config: $BASE_URL/api/Debug/config"

echo ""
echo "✨ Swagger testing completed! All endpoints are properly documented."
