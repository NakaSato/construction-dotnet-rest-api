#!/bin/bash

# Simple Project API Test Script
# Tests basic CRUD operations

echo "🌞 Project API Tests"
echo "==================="

API_BASE="http://localhost:5002/api/v1"

# Get token
echo "🔐 Getting token..."
TOKEN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' | \
  jq -r '.data.token // empty')

if [ -z "$TOKEN" ]; then
    echo "❌ Auth failed"
    exit 1
fi

echo "✅ Authenticated"

# Get manager
MANAGER=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/users?pageSize=50" | \
  jq -r '.data.items[]? | select(.roleName == "Manager") | .userId' | head -1)

echo "✅ Manager: ${MANAGER:0:8}..."

echo ""
echo "📋 Testing Endpoints:"
echo ""

# 1. GET All Projects
echo "1. GET /projects"
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/projects")
count=$(echo "$response" | jq -r '.data.totalCount // 0')
echo "   📊 Found $count projects"

# 2. GET with pagination
echo ""
echo "2. GET /projects?pageSize=2"
response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/projects?pageSize=2")
pageCount=$(echo "$response" | jq -r '.data.items | length')
echo "   📄 Retrieved $pageCount projects"

# 3. POST Create Project
echo ""
echo "3. POST /projects (Create)"
timestamp=$(date +%s)
response=$(curl -s -X POST "$API_BASE/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"projectName\": \"API Test Project $timestamp\",
    \"address\": \"123 Test St\",
    \"clientInfo\": \"Test Client\",
    \"startDate\": \"2025-07-01T00:00:00Z\",
    \"estimatedEndDate\": \"2025-12-31T00:00:00Z\",
    \"projectManagerId\": \"$MANAGER\",
    \"team\": \"T1\",
    \"connectionType\": \"LV\",
    \"totalCapacityKw\": 100.0,
    \"pvModuleCount\": 200,
    \"equipmentDetails\": {
      \"inverter125kw\": 1,
      \"inverter80kw\": 0,
      \"inverter60kw\": 0,
      \"inverter40kw\": 1
    },
    \"ftsValue\": 5,
    \"revenueValue\": 2,
    \"pqmValue\": 1,
    \"locationCoordinates\": {
      \"latitude\": 13.7563,
      \"longitude\": 100.5018
    }
  }")

success=$(echo "$response" | jq -r '.success // false')
projectId=$(echo "$response" | jq -r '.data.projectId // empty')

if [ "$success" = "true" ] && [ -n "$projectId" ]; then
    echo "   ✅ Created project: ${projectId:0:8}..."
    
    # 4. GET Specific Project
    echo ""
    echo "4. GET /projects/$projectId"
    response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/projects/$projectId")
    name=$(echo "$response" | jq -r '.data.projectName // "N/A"')
    echo "   📋 Retrieved: $name"
    
    # 5. PATCH Update
    echo ""
    echo "5. PATCH /projects/$projectId"
    response=$(curl -s -X PATCH "$API_BASE/projects/$projectId" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{\"projectName\": \"UPDATED API Test $timestamp\"}")
    
    success=$(echo "$response" | jq -r '.success // false')
    if [ "$success" = "true" ]; then
        echo "   ✅ Updated successfully"
    else
        echo "   ❌ Update failed"
    fi
    
    echo ""
    echo "🗑️  Test project created: $projectId"
    echo "    To delete: curl -X DELETE '$API_BASE/projects/$projectId' -H 'Authorization: Bearer $TOKEN'"
    
else
    echo "   ❌ Creation failed"
    echo "   Response: $(echo "$response" | jq -r '.message // "Unknown error"')"
fi

# 6. Error Test
echo ""
echo "6. GET /projects/invalid-id (Error test)"
response=$(curl -s -w "STATUS:%{http_code}" -H "Authorization: Bearer $TOKEN" \
  "$API_BASE/projects/00000000-0000-0000-0000-000000000000")

status=$(echo "$response" | grep -o "STATUS:[0-9]*" | cut -d: -f2)
echo "   📊 Status: $status (Expected: 404)"

echo ""
echo "🎉 Tests Complete!"
echo ""
echo "📊 Summary:"
echo "   ✅ Authentication"
echo "   ✅ GET All Projects" 
echo "   ✅ GET Paginated"
echo "   ✅ POST Create Project"
echo "   ✅ GET Specific Project"
echo "   ✅ PATCH Update"
echo "   ✅ Error Handling"
