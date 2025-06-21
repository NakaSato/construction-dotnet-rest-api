#!/bin/bash

# Create 3 Sample Solar Projects Script
# Uses testuser001 credentials to create projects

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
USERNAME="testuser001"
PASSWORD="Password123!"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}╔══════════════════════════════════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                          🌞 Creating 3 Sample Solar Projects                                 ║${NC}"
echo -e "${CYAN}╚══════════════════════════════════════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Authenticate
echo -e "${BLUE}🔐 Authenticating with testuser001...${NC}"
TOKEN=$(curl -s -X POST \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}" \
  "$API_BASE/auth/login" | jq -r '.data.token')

if [[ "$TOKEN" == "null" || -z "$TOKEN" ]]; then
    echo -e "${RED}❌ Authentication failed${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Authentication successful${NC}"
echo "Token: ${TOKEN:0:50}..."
echo ""

# Function to create project
create_project() {
    local project_num="$1"
    local payload="$2"
    local name="$3"
    
    echo -e "${BLUE}📝 Creating Project $project_num: $name${NC}"
    
    response=$(curl -s -X POST \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$API_BASE/projects")
    
    success=$(echo "$response" | jq -r '.success')
    message=$(echo "$response" | jq -r '.message')
    
    if [[ "$success" == "true" ]]; then
        echo -e "${GREEN}✅ Project $project_num created successfully${NC}"
        project_id=$(echo "$response" | jq -r '.data.projectId // .data.id')
        echo "Project ID: $project_id"
    else
        echo -e "${RED}❌ Failed to create Project $project_num${NC}"
        echo "Error: $message"
        echo "Response: $response"
    fi
    echo ""
}

# Project 1: Small Residential Solar Installation
PROJECT1='{
  "projectName": "Residential Solar - Green Valley",
  "address": "123 Green Valley Road, Bangkok 10110",
  "clientInfo": "Smith Family Residence",
  "status": "Planning",
  "startDate": "2025-07-01",
  "estimatedEndDate": "2025-08-15",
  "projectManagerId": "1734cf14-23dc-4567-93b8-ec6f8a808f30",
  "totalCapacityKw": 25.5,
  "pvModuleCount": 50,
  "ftsValue": 450000,
  "revenueValue": 540000,
  "pqmValue": 90000,
  "inverter125Kw": 0,
  "inverter80Kw": 0,
  "inverter60Kw": 0,
  "inverter40Kw": 1,
  "latitude": 13.7563,
  "longitude": 100.5018,
  "connectionType": "LV",
  "connectionNotes": "Single-phase residential connection"
}'

# Project 2: Commercial Office Building
PROJECT2='{
  "projectName": "Office Complex Solar - Sathorn Plaza",
  "address": "456 Sathorn Road, Silom, Bangkok 10500",
  "clientInfo": "Sathorn Plaza Management Co., Ltd.",
  "status": "Active",
  "startDate": "2025-06-15",
  "estimatedEndDate": "2025-09-30",
  "projectManagerId": "1734cf14-23dc-4567-93b8-ec6f8a808f30",
  "totalCapacityKw": 150.0,
  "pvModuleCount": 300,
  "ftsValue": 2700000,
  "revenueValue": 3240000,
  "pqmValue": 540000,
  "inverter125Kw": 1,
  "inverter80Kw": 0,
  "inverter60Kw": 1,
  "inverter40Kw": 0,
  "latitude": 13.7248,
  "longitude": 100.5346,
  "connectionType": "MV",
  "connectionNotes": "Three-phase commercial connection 22kV"
}'

# Project 3: Industrial Solar Farm
PROJECT3='{
  "projectName": "Industrial Solar Farm - Eastern Seaboard",
  "address": "789 Industrial Estate, Rayong 21140",
  "clientInfo": "Eastern Industrial Development Corporation",
  "status": "Planning",
  "startDate": "2025-08-01",
  "estimatedEndDate": "2025-12-15",
  "projectManagerId": "1734cf14-23dc-4567-93b8-ec6f8a808f30",
  "totalCapacityKw": 500.0,
  "pvModuleCount": 1000,
  "ftsValue": 9000000,
  "revenueValue": 10800000,
  "pqmValue": 1800000,
  "inverter125Kw": 4,
  "inverter80Kw": 0,
  "inverter60Kw": 0,
  "inverter40Kw": 0,
  "latitude": 12.6819,
  "longitude": 101.2843,
  "connectionType": "HV",
  "connectionNotes": "High voltage industrial connection 115kV"
}'

# Create the projects
create_project "1" "$PROJECT1" "Residential Solar - Green Valley"
create_project "2" "$PROJECT2" "Office Complex Solar - Sathorn Plaza" 
create_project "3" "$PROJECT3" "Industrial Solar Farm - Eastern Seaboard"

# Verify projects were created
echo -e "${BLUE}🔍 Verifying created projects...${NC}"
projects_response=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE/projects")
total_count=$(echo "$projects_response" | jq -r '.data.totalCount // 0')

echo -e "${GREEN}✅ Total projects in database: $total_count${NC}"

if [[ "$total_count" -gt 0 ]]; then
    echo ""
    echo -e "${YELLOW}📋 Project Summary:${NC}"
    echo "$projects_response" | jq -r '.data.items[] | "• \(.projectName) (\(.status)) - \(.totalCapacityKw)kW"'
fi

echo ""
echo -e "${CYAN}🎉 Solar Projects Creation Complete!${NC}"
echo -e "${GREEN}✅ All 3 projects have been successfully created${NC}"
