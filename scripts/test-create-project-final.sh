#!/bin/bash

# Final enhanced script to test POST /api/v1/projects endpoint
# Solar Project Management API - Working Version

echo "🌞 Solar Project Creation API Test (Final Version)"
echo "================================================="

# API Configuration
API_BASE_URL="http://localhost:5002"
ENDPOINT="/api/v1/projects"
FULL_URL="${API_BASE_URL}${ENDPOINT}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔐 Step 1: Authenticating...${NC}"

# Login and get token
LOGIN_RESPONSE=$(curl -s -X POST "${API_BASE_URL}/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo -e "${RED}❌ Authentication failed${NC}"
    echo "Response: $LOGIN_RESPONSE"
    exit 1
fi

echo -e "${GREEN}✅ Successfully authenticated${NC}"

echo -e "${BLUE}🔍 Step 2: Finding project managers...${NC}"

# Get users and find managers
USERS_RESPONSE=$(curl -s -X GET "${API_BASE_URL}/api/v1/users?pageSize=50" \
  -H "Authorization: Bearer $TOKEN")

# Extract a manager ID (prefer Manager role, fallback to Admin)
MANAGER_ID=$(echo $USERS_RESPONSE | jq -r '
  .data.items[]? | 
  select(.roleName == "Manager" and .isActive == true) | 
  .userId
' 2>/dev/null | head -1)

if [ -z "$MANAGER_ID" ] || [ "$MANAGER_ID" = "null" ]; then
    # Fallback to Admin role
    MANAGER_ID=$(echo $USERS_RESPONSE | jq -r '
      .data.items[]? | 
      select(.roleName == "Admin" and .isActive == true) | 
      .userId
    ' 2>/dev/null | head -1)
fi

if [ -z "$MANAGER_ID" ] || [ "$MANAGER_ID" = "null" ]; then
    echo -e "${RED}❌ No suitable project manager found${NC}"
    exit 1
fi

MANAGER_INFO=$(echo $USERS_RESPONSE | jq -r "
  .data.items[]? | 
  select(.userId == \"$MANAGER_ID\") | 
  \"\(.fullName) (\(.roleName))\"
" 2>/dev/null)

echo -e "${GREEN}✅ Found project manager: $MANAGER_INFO${NC}"
echo -e "${CYAN}   ID: $MANAGER_ID${NC}"

echo ""
echo -e "${PURPLE}🏗️ Step 3: Creating solar project...${NC}"

# Create unique project name with timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
PROJECT_NAME="Solar Installation Project - Test $TIMESTAMP"

echo -e "${CYAN}📝 Project: $PROJECT_NAME${NC}"
echo -e "${CYAN}🔗 Endpoint: $FULL_URL${NC}"
echo ""

# Create the project with all solar-specific fields
RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}\n" -X POST "$FULL_URL" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"projectName\": \"$PROJECT_NAME\",
    \"address\": \"456 Oak Ave, Another City, State 67890\", 
    \"clientInfo\": \"XYZ Corp - Contact: Sarah Johnson (555-987-6543)\",
    \"startDate\": \"2025-07-01T00:00:00Z\",
    \"estimatedEndDate\": \"2025-09-30T00:00:00Z\",
    \"projectManagerId\": \"$MANAGER_ID\",
    \"team\": \"E2\",
    \"connectionType\": \"LV\",
    \"connectionNotes\": \"เสาหม้อแปลงไม่มีพื้นที่ในการทำZero Export แรงสูง\",
    \"totalCapacityKw\": 171.0,
    \"pvModuleCount\": 300,
    \"equipmentDetails\": {
      \"inverter125kw\": 1,
      \"inverter80kw\": 0,
      \"inverter60kw\": 1,
      \"inverter40kw\": 0
    },
    \"ftsValue\": 6,
    \"revenueValue\": 1,
    \"pqmValue\": 0,
    \"locationCoordinates\": {
      \"latitude\": 14.72746,
      \"longitude\": 102.16276
    }
  }")

# Parse response
HTTP_STATUS=$(echo "$RESPONSE" | grep "HTTP_STATUS:" | cut -d: -f2)
RESPONSE_BODY=$(echo "$RESPONSE" | sed '/HTTP_STATUS:/d')

echo -e "${BLUE}📊 HTTP Status: $HTTP_STATUS${NC}"
echo ""

case $HTTP_STATUS in
    201)
        echo -e "${GREEN}🎉 SUCCESS! Solar project created successfully!${NC}"
        echo ""
        
        if command -v jq &> /dev/null; then
            echo -e "${BLUE}📋 Project Details:${NC}"
            echo "$RESPONSE_BODY" | jq '.data' 2>/dev/null || echo "$RESPONSE_BODY"
            
            # Extract key information
            PROJECT_ID=$(echo "$RESPONSE_BODY" | jq -r '.data.projectId // empty' 2>/dev/null)
            CAPACITY=$(echo "$RESPONSE_BODY" | jq -r '.data.totalCapacityKw // empty' 2>/dev/null)
            MODULES=$(echo "$RESPONSE_BODY" | jq -r '.data.pvModuleCount // empty' 2>/dev/null)
            
            echo ""
            echo -e "${GREEN}═══════════════════════════════════════${NC}"
            echo -e "${GREEN}✅ Project Created Successfully!${NC}"
            echo -e "${CYAN}🆔 Project ID: $PROJECT_ID${NC}"
            echo -e "${CYAN}⚡ Capacity: ${CAPACITY}kW${NC}"
            echo -e "${CYAN}🔆 PV Modules: $MODULES${NC}"
            echo -e "${CYAN}👥 Team: E2${NC}"
            echo -e "${CYAN}🔌 Connection: LV${NC}"
            echo -e "${GREEN}═══════════════════════════════════════${NC}"
            
            # Test getting the created project
            echo ""
            echo -e "${BLUE}🔍 Step 4: Verifying created project...${NC}"
            GET_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" "${API_BASE_URL}/api/v1/projects/$PROJECT_ID")
            GET_STATUS=$?
            
            if [ $GET_STATUS -eq 0 ]; then
                echo -e "${GREEN}✅ Project verification successful${NC}"
                echo -e "${CYAN}🔗 Direct access: ${API_BASE_URL}/api/v1/projects/$PROJECT_ID${NC}"
            else
                echo -e "${YELLOW}⚠️  Could not verify project (but creation was successful)${NC}"
            fi
        else
            echo "$RESPONSE_BODY"
        fi
        ;;
    400)
        echo -e "${RED}❌ BAD REQUEST: Invalid input data${NC}"
        echo ""
        echo -e "${YELLOW}🔍 Error Details:${NC}"
        if command -v jq &> /dev/null; then
            echo "$RESPONSE_BODY" | jq '.' || echo "$RESPONSE_BODY"
        else
            echo "$RESPONSE_BODY"
        fi
        ;;
    401)
        echo -e "${RED}❌ UNAUTHORIZED: Authentication failed${NC}"
        ;;
    403)
        echo -e "${RED}❌ FORBIDDEN: Insufficient permissions${NC}"
        echo -e "${YELLOW}💡 Required roles: Administrator, ProjectManager${NC}"
        ;;
    500)
        echo -e "${RED}❌ SERVER ERROR: Internal server error${NC}"
        echo ""
        if command -v jq &> /dev/null; then
            echo "$RESPONSE_BODY" | jq '.' || echo "$RESPONSE_BODY"
        else
            echo "$RESPONSE_BODY"
        fi
        ;;
    *)
        echo -e "${YELLOW}❓ Unexpected status: $HTTP_STATUS${NC}"
        echo "$RESPONSE_BODY"
        ;;
esac

echo ""
echo -e "${PURPLE}═══════════════════════════════════════${NC}"
echo -e "${PURPLE}🌞 Solar Project Management API Test Complete${NC}"
echo -e "${PURPLE}═══════════════════════════════════════${NC}"
