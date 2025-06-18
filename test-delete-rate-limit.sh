#!/bin/bash

# Test DELETE rate limiting
API_BASE="http://localhost:5002/api/v1"
AUTH_TOKEN="your-jwt-token-here"

echo "Testing DELETE rate limiting..."

# Test regular DELETE operations (10 per minute limit)
echo "Testing regular DELETE operations (should allow 10 requests):"
for i in {1..12}; do
    echo "Request $i:"
    response=$(curl -s -w "%{http_code}" -X DELETE \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -H "Content-Type: application/json" \
        "$API_BASE/work-requests/$(uuidgen)" \
        -o /dev/null)
    
    if [ "$response" = "429" ]; then
        echo "  Rate limited (429) - Expected after 10 requests"
        break
    else
        echo "  Status: $response"
    fi
    
    sleep 1
done

echo ""
echo "Testing critical DELETE operations (should allow 3 requests per 5 minutes):"

# Test critical DELETE operations (3 per 5 minutes limit)
for i in {1..5}; do
    echo "Request $i:"
    response=$(curl -s -w "%{http_code}" -X DELETE \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -H "Content-Type: application/json" \
        "$API_BASE/projects/$(uuidgen)" \
        -o /dev/null)
    
    if [ "$response" = "429" ]; then
        echo "  Rate limited (429) - Expected after 3 requests"
        break
    else
        echo "  Status: $response"
    fi
    
    sleep 1
done

echo ""
echo "Rate limiting test completed!"
