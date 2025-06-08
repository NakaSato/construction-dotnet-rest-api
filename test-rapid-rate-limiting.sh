#!/bin/bash

# Enhanced rate limiting test script
BASE_URL="http://localhost:5002"
echo "🚀 Enhanced Rate Limiting Test"
echo "=============================="

echo ""
echo "📊 Testing rapid-fire requests to trigger rate limiting..."

# Make rapid requests to trigger rate limiting
for i in {1..55}; do
    response=$(curl -s -I -X GET "$BASE_URL/api/v1/projects" 2>/dev/null)
    status_code=$(echo "$response" | head -n1 | cut -d' ' -f2)
    rate_remaining=$(echo "$response" | grep -i "x-ratelimit-remaining:" | cut -d' ' -f2- | tr -d '\r\n')
    rate_limit=$(echo "$response" | grep -i "x-ratelimit-limit:" | cut -d' ' -f2- | tr -d '\r\n')
    retry_after=$(echo "$response" | grep -i "retry-after:" | cut -d' ' -f2- | tr -d '\r\n')
    
    echo -n "Request $i: Status=$status_code"
    [ -n "$rate_remaining" ] && echo -n ", Remaining=$rate_remaining/$rate_limit"
    [ -n "$retry_after" ] && echo -n ", RetryAfter=${retry_after}s"
    echo ""
    
    if [ "$status_code" = "429" ]; then
        echo "🛑 Rate limit triggered! Success!"
        echo ""
        echo "📋 Full 429 Response Headers:"
        echo "$response" | grep -E "(HTTP/|X-RateLimit|Retry-After)" | sed 's/^/   /'
        echo ""
        echo "📄 Response Body:"
        curl -s -X GET "$BASE_URL/api/v1/projects" | jq . 2>/dev/null || curl -s -X GET "$BASE_URL/api/v1/projects"
        break
    fi
    
    # Small delay to see the rate limiting in action
    sleep 0.1
done

echo ""
echo "✅ Rate limiting test completed!"
