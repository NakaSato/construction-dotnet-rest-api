#!/bin/bash

# Test Authorization Script for Solar Projects API
# This script tests the authentication and authorization functionality

API_BASE="http://localhost:5001/api/v1"
ADMIN_USERNAME="admin@example.com"
ADMIN_PASSWORD="Admin123!"

echo "🚀 Testing Solar Projects API Authorization..."
echo "=============================================="

# Step 1: Test login
echo "📝 Step 1: Testing login with admin credentials..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "'$ADMIN_USERNAME'",
    "password": "'$ADMIN_PASSWORD'"
  }')

echo "Login Response: $LOGIN_RESPONSE"

# Extract JWT token from response
JWT_TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$JWT_TOKEN" ]; then
    echo "❌ Failed to get JWT token. Login might have failed."
    exit 1
fi

echo "✅ Successfully obtained JWT token: ${JWT_TOKEN:0:50}..."
echo ""

# Step 2: Test unauthorized access (without token)
echo "📝 Step 2: Testing unauthorized access (without token)..."
UNAUTH_RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X GET "$API_BASE/projects")
echo "Unauthorized Response: $UNAUTH_RESPONSE"
echo ""

# Step 3: Test authorized access (with token)
echo "📝 Step 3: Testing authorized access (with valid token)..."
AUTH_RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X GET "$API_BASE/projects" \
  -H "Authorization: Bearer $JWT_TOKEN")
echo "Authorized Response: $AUTH_RESPONSE"
echo ""

# Step 4: Test role-based authorization - Create project (Admin/Manager only)
echo "📝 Step 4: Testing role-based authorization - Create project..."
CREATE_RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X POST "$API_BASE/projects" \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "Test Authorization Project",
    "description": "Testing authorization functionality",
    "address": "123 Test St, Test City, TC 12345",
    "clientInfo": "Test Client for Authorization",
    "estimatedBudget": 50000,
    "startDate": "2025-08-01T00:00:00Z",
    "estimatedEndDate": "2025-12-31T00:00:00Z",
    "status": "Planning"
  }')
echo "Create Project Response: $CREATE_RESPONSE"
echo ""

# Step 5: Test invalid token
echo "📝 Step 5: Testing with invalid token..."
INVALID_TOKEN_RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X GET "$API_BASE/projects" \
  -H "Authorization: Bearer invalid_token_here")
echo "Invalid Token Response: $INVALID_TOKEN_RESPONSE"
echo ""

echo "🏁 Authorization testing completed!"
echo "=============================================="
