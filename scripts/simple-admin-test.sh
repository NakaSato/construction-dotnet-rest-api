#!/bin/bash

# Simple test for admin project creation
BASE_URL="http://localhost:5001"

echo "=== SIMPLE ADMIN PROJECT CREATION TEST ==="
echo "1. Getting admin token..."

TOKEN_RESPONSE=$(curl -s -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' \
  "$BASE_URL/api/v1/auth/login")

echo "Token response: $TOKEN_RESPONSE"

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.data.token')
echo "Token: ${TOKEN:0:50}..."

echo ""
echo "2. Creating project..."

PROJECT_DATA='{
    "projectName": "Simple Admin Test Project",
    "address": "123 Simple Street",
    "clientInfo": "Simple Test Client",
    "startDate": "2025-07-04T03:50:00Z",
    "estimatedEndDate": "2025-10-01T03:50:00Z",
    "projectManagerId": "cface76b-1457-44a1-89fa-6b4ccc2f5f66",
    "team": "Simple Team",
    "connectionType": "MV",
    "connectionNotes": "Simple test",
    "totalCapacityKw": 50.0,
    "pvModuleCount": 100,
    "equipmentDetails": {
        "inverter125kw": 0,
        "inverter80kw": 0,
        "inverter60kw": 0,
        "inverter40kw": 1
    },
    "ftsValue": 500000,
    "revenueValue": 600000,
    "pqmValue": 50000,
    "locationCoordinates": {
        "latitude": 34.0522,
        "longitude": -118.2437
    }
}'

echo "Project data: $PROJECT_DATA"
echo ""

PROJECT_RESPONSE=$(curl -s -w "\n%{http_code}" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "$PROJECT_DATA" \
    "$BASE_URL/api/v1/projects")

echo "Project creation response:"
echo "$PROJECT_RESPONSE"
echo ""

HTTP_CODE=$(echo "$PROJECT_RESPONSE" | tail -1)
RESPONSE_BODY=$(echo "$PROJECT_RESPONSE" | head -n -1)

echo "HTTP Status: $HTTP_CODE"
echo "Response Body: $RESPONSE_BODY"

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "201" ]; then
    echo "✅ SUCCESS: Project created successfully!"
    PROJECT_ID=$(echo "$RESPONSE_BODY" | jq -r '.data.id // .data.projectId // .id // .projectId' 2>/dev/null)
    echo "Project ID: $PROJECT_ID"
elif [ "$HTTP_CODE" = "400" ]; then
    echo "❌ FAILED: Bad request"
    echo "$RESPONSE_BODY" | jq '.errors // .message' 2>/dev/null || echo "$RESPONSE_BODY"
elif [ "$HTTP_CODE" = "403" ]; then
    echo "❌ FAILED: Forbidden - insufficient permissions"
else
    echo "❌ FAILED: HTTP $HTTP_CODE"
    echo "$RESPONSE_BODY"
fi
