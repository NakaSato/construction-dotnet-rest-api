#!/bin/bash

# Super fast rate limiting test to trigger 429
BASE_URL="http://localhost:5002"
echo "⚡ Super Fast Rate Limiting Test"
echo "================================"

echo ""
echo "🔥 Making 60 rapid requests (no delay) to trigger rate limiting..."

for i in {1..60}; do
    response=$(curl -s -I -X GET "$BASE_URL/api/v1/projects" 2>/dev/null)
    status_code=$(echo "$response" | head -n1 | cut -d' ' -f2)
    rate_remaining=$(echo "$response" | grep -i "x-ratelimit-remaining:" | cut -d' ' -f2- | tr -d '\r\n')
    rate_limit=$(echo "$response" | grep -i "x-ratelimit-limit:" | cut -d' ' -f2- | tr -d '\r\n')
    retry_after=$(echo "$response" | grep -i "retry-after:" | cut -d' ' -f2- | tr -d '\r\n')
    
    printf "Request %2d: Status=%s" "$i" "$status_code"
    [ -n "$rate_remaining" ] && printf ", Remaining=%s/%s" "$rate_remaining" "$rate_limit"
    [ -n "$retry_after" ] && printf ", RetryAfter=%ss" "$retry_after"
    printf "\n"
    
    if [ "$status_code" = "429" ]; then
        echo ""
        echo "🎯 SUCCESS! Rate limit exceeded - 429 Too Many Requests!"
        echo ""
        echo "📋 Full Response Headers:"
        echo "$response"
        echo ""
        echo "📄 Response Body:"
        curl -s -X GET "$BASE_URL/api/v1/projects"
        break
    fi
done

echo ""
echo "✅ Test completed!"
