#!/bin/bash

# Monitor rate limiting for DELETE operations
API_BASE="http://localhost:5002/api/v1"
AUTH_TOKEN="your-jwt-token-here"

echo "=== Rate Limiting Statistics ==="
curl -s -H "Authorization: Bearer $AUTH_TOKEN" \
     "$API_BASE/rate-limit/statistics?hours=1" | jq .

echo ""
echo "=== Active Rate Limiting Rules ==="
curl -s -H "Authorization: Bearer $AUTH_TOKEN" \
     "$API_BASE/rate-limit/rules" | jq .

echo ""
echo "=== Recent Rate Limit Violations ==="
curl -s -H "Authorization: Bearer $AUTH_TOKEN" \
     "$API_BASE/rate-limit/violations?hours=1" | jq .

echo ""
echo "=== Top Clients by Request Count ==="
curl -s -H "Authorization: Bearer $AUTH_TOKEN" \
     "$API_BASE/rate-limit/clients/top?count=5" | jq .
