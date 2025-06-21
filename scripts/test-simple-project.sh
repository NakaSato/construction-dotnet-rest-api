#!/bin/bash

# Simple project creation test

echo "Getting authentication token..."
TOKEN=$(curl -s -X POST \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser001","password":"Password123!"}' \
  https://solar-projects-api.azurewebsites.net/api/v1/auth/login | jq -r '.data.token')

echo "Token: ${TOKEN:0:50}..."

echo "Creating simple project..."
RESPONSE=$(curl -s -X POST \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "Test Solar Project",
    "address": "123 Test Street, Bangkok",
    "clientInfo": "Test Client Company",
    "status": "Planning",
    "startDate": "2025-07-01",
    "estimatedEndDate": "2025-08-15",
    "projectManagerId": "1734cf14-23dc-4567-93b8-ec6f8a808f30",
    "totalCapacityKw": 50.0,
    "pvModuleCount": 100,
    "ftsValue": 900000.0,
    "revenueValue": 1080000.0,
    "pqmValue": 180000.0,
    "inverter125Kw": 0,
    "inverter80Kw": 0,
    "inverter60Kw": 1,
    "inverter40Kw": 0,
    "latitude": 13.7563,
    "longitude": 100.5018,
    "connectionType": "LV",
    "connectionNotes": "Test connection"
  }' \
  https://solar-projects-api.azurewebsites.net/api/v1/projects)

echo "Full response:"
echo "$RESPONSE" | jq

echo ""
echo "Checking projects count:"
curl -s -H "Authorization: Bearer $TOKEN" \
  https://solar-projects-api.azurewebsites.net/api/v1/projects | jq '.data.totalCount'
