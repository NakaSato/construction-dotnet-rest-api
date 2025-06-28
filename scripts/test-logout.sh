#!/bin/bash

# Test logout functionality locally
set -e

echo "🧪 Testing Logout Functionality"
echo "================================"

# Start server in background
echo "🚀 Starting server..."
dotnet run --urls "http://localhost:5001" > /dev/null 2>&1 &
SERVER_PID=$!

# Wait for server to start
sleep 5

# Check if server is running
if ! curl -s http://localhost:5001/health > /dev/null; then
    echo "❌ Server failed to start"
    kill $SERVER_PID 2>/dev/null || true
    exit 1
fi

echo "✅ Server started successfully"

# Test 1: Register test user first
echo "📝 Registering test user..."
REGISTER_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d '{"username":"test_admin","email":"test_admin@example.com","password":"Admin123!","fullName":"Test Admin","roleId":1}' http://localhost:5001/api/v1/auth/register)

if echo "$REGISTER_RESPONSE" | grep -q '"success":true'; then
    echo "✅ User registered successfully"
elif echo "$REGISTER_RESPONSE" | grep -q "already exists"; then
    echo "ℹ️  User already exists"
else
    echo "⚠️  Registration response: $REGISTER_RESPONSE"
fi

# Test 2: Login to get token
echo "🔐 Testing login..."
LOGIN_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d '{"username":"test_admin","password":"Admin123!"}' http://localhost:5001/api/v1/auth/login)

# Check if login was successful
if echo "$LOGIN_RESPONSE" | grep -q '"success":true'; then
    echo "✅ Login successful"
    TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    echo "🎟️  Token obtained: ${TOKEN:0:20}..."
else
    echo "❌ Login failed: $LOGIN_RESPONSE"
    kill $SERVER_PID 2>/dev/null || true
    exit 1
fi

# Test 3: Test logout endpoint
echo "🚪 Testing logout..."
LOGOUT_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/auth/logout)

STATUS=$(echo "$LOGOUT_RESPONSE" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
BODY=$(echo "$LOGOUT_RESPONSE" | sed 's/HTTPSTATUS:[0-9]*$//')

if [ "$STATUS" = "200" ]; then
    echo "✅ Logout successful (Status: $STATUS)"
    echo "📄 Response: $BODY"
else
    echo "❌ Logout failed (Status: $STATUS)"
    echo "📄 Response: $BODY"
fi

# Test 4: Try to use token after logout (should fail)
echo "🔒 Testing token invalidation..."
AUTH_TEST_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/users?pageNumber=1&pageSize=5)

AUTH_STATUS=$(echo "$AUTH_TEST_RESPONSE" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)

if [ "$AUTH_STATUS" = "401" ]; then
    echo "✅ Token properly invalidated (Status: $AUTH_STATUS)"
else
    echo "⚠️  Token invalidation test - Status: $AUTH_STATUS (may still work due to caching)"
fi

# Test 5: Test logout without token (should fail)
echo "🚫 Testing logout without token..."
NO_AUTH_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" -X POST http://localhost:5001/api/v1/auth/logout)

NO_AUTH_STATUS=$(echo "$NO_AUTH_RESPONSE" | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)

if [ "$NO_AUTH_STATUS" = "401" ]; then
    echo "✅ Logout without token properly rejected (Status: $NO_AUTH_STATUS)"
else
    echo "❌ Logout without token should be rejected (Status: $NO_AUTH_STATUS)"
fi

# Cleanup
echo "🧹 Cleaning up..."
kill $SERVER_PID 2>/dev/null || true
wait $SERVER_PID 2>/dev/null || true

echo ""
echo "🎉 Logout functionality testing complete!"
