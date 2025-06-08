#!/bin/bash

# Test script for comprehensive rate limiting functionality
BASE_URL="http://localhost:5002"
echo "🧪 Testing Rate Limiting System for Solar Projects API"
echo "=================================================="

# Function to make a request and show rate limit headers
test_endpoint() {
    local endpoint="$1"
    local method="${2:-GET}"
    local description="$3"
    
    echo ""
    echo "📍 Testing: $description"
    echo "   Endpoint: $method $endpoint"
    
    if [ "$method" = "POST" ]; then
        response=$(curl -s -i -X POST \
            -H "Content-Type: application/json" \
            -d '{"name":"Test Project","description":"Test Description"}' \
            "$BASE_URL$endpoint" 2>/dev/null)
    else
        response=$(curl -s -i -X "$method" "$BASE_URL$endpoint" 2>/dev/null)
    fi
    
    # Extract status code
    status_code=$(echo "$response" | head -n1 | cut -d' ' -f2)
    
    # Extract rate limit headers
    rate_limit=$(echo "$response" | grep -i "x-ratelimit-limit:" | cut -d' ' -f2- | tr -d '\r\n')
    rate_remaining=$(echo "$response" | grep -i "x-ratelimit-remaining:" | cut -d' ' -f2- | tr -d '\r\n')
    rate_reset=$(echo "$response" | grep -i "x-ratelimit-reset:" | cut -d' ' -f2- | tr -d '\r\n')
    retry_after=$(echo "$response" | grep -i "retry-after:" | cut -d' ' -f2- | tr -d '\r\n')
    
    echo "   Status: $status_code"
    [ -n "$rate_limit" ] && echo "   Rate Limit: $rate_limit"
    [ -n "$rate_remaining" ] && echo "   Remaining: $rate_remaining"
    [ -n "$rate_reset" ] && echo "   Reset Time: $rate_reset"
    [ -n "$retry_after" ] && echo "   Retry After: $retry_after seconds"
    
    # Show response body for errors
    if [ "$status_code" = "429" ]; then
        echo "   Response Body:"
        echo "$response" | grep -A 20 "^{" | head -10 | sed 's/^/     /'
    fi
}

# Test 1: Health endpoint (should be excluded from rate limiting)
test_endpoint "/api/health" "GET" "Health Check (should be excluded)"

# Test 2: Basic API endpoint
test_endpoint "/api/v1/projects" "GET" "Projects endpoint"

# Test 3: Auth endpoint (should have stricter limits)
test_endpoint "/api/v1/auth/login" "POST" "Auth login (stricter limits)"

# Test 4: Rate limit admin endpoints
test_endpoint "/api/v1/rate-limit/statistics" "GET" "Rate limit statistics"

# Test 5: Burst testing - make multiple requests quickly
echo ""
echo "🚀 Burst Testing - Making 10 rapid requests to projects endpoint"
echo "================================================================"

for i in {1..10}; do
    echo -n "Request $i: "
    response=$(curl -s -i "$BASE_URL/api/v1/projects" 2>/dev/null)
    status_code=$(echo "$response" | head -n1 | cut -d' ' -f2)
    rate_remaining=$(echo "$response" | grep -i "x-ratelimit-remaining:" | cut -d' ' -f2- | tr -d '\r\n')
    echo "Status: $status_code, Remaining: $rate_remaining"
    
    if [ "$status_code" = "429" ]; then
        echo "   🛑 Rate limit exceeded! Testing successful."
        retry_after=$(echo "$response" | grep -i "retry-after:" | cut -d' ' -f2- | tr -d '\r\n')
        echo "   Retry After: $retry_after seconds"
        break
    fi
    
    # Small delay to see rate limiting in action
    sleep 0.1
done

# Test 6: Test different IP simulation (using different User-Agent)
echo ""
echo "🌐 Testing with different User-Agent (simulating different client)"
echo "================================================================="

response=$(curl -s -i -H "User-Agent: TestClient/1.0" "$BASE_URL/api/v1/projects" 2>/dev/null)
status_code=$(echo "$response" | head -n1 | cut -d' ' -f2)
rate_remaining=$(echo "$response" | grep -i "x-ratelimit-remaining:" | cut -d' ' -f2- | tr -d '\r\n')
echo "Status: $status_code, Remaining: $rate_remaining"

# Test 7: Rate limit monitoring endpoints
echo ""
echo "📊 Testing Rate Limit Monitoring Endpoints"
echo "=========================================="

test_endpoint "/api/v1/rate-limit/statistics" "GET" "Overall statistics"
test_endpoint "/api/v1/rate-limit/clients/top" "GET" "Top clients"

echo ""
echo "✅ Rate limiting tests completed!"
echo "Check the above output for rate limit headers and 429 responses."
