#!/bin/bash

# Rate Limit Testing Script
echo "🚀 Testing Rate Limit Improvements"
echo "=================================="

# Get auth token
echo "🔐 Getting authentication token..."
TOKEN_RESPONSE=$(curl -s -X POST "http://localhost:5002/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@example.com",
    "password": "Admin123!"
  }')

TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.data.token')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
    echo "❌ Authentication failed"
    exit 1
fi

echo "✅ Authentication successful"

# Test multiple API calls in quick succession
echo ""
echo "📊 Testing API Rate Limits (should handle more requests now)..."

# Test GET requests
echo "🔍 Testing GET requests (should allow 5000/min)..."
for i in {1..10}; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
        -H "Authorization: Bearer $TOKEN" \
        "http://localhost:5002/api/v1/users")
    echo "   Request $i: $STATUS"
done

echo ""
echo "🗑️  Testing DELETE requests (should allow 100/min)..."

# Test DELETE requests
for i in {1..15}; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
        -X DELETE \
        -H "Authorization: Bearer $TOKEN" \
        "http://localhost:5002/api/v1/users/12345678-1234-1234-1234-123456789012")
    echo "   DELETE Request $i: $STATUS"
    
    # If we hit rate limit, stop
    if [ "$STATUS" = "429" ]; then
        echo "   ⚠️  Hit rate limit at request $i (this is expected)"
        break
    fi
done

echo ""
echo "🎯 Rate Limit Test Complete!"
echo "If you see fewer 429 errors, the rate limits have been successfully increased."
