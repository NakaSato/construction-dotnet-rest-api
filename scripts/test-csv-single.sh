#!/bin/bash

# =============================================================================
# 🧪 CSV IMPORT TEST - Single Project
# =============================================================================
# Test script to import just one project from CSV
# =============================================================================

BASE_URL="http://localhost:5001"
CSV_FILE="project.csv"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_step() { echo -e "${BLUE}➤ $1${NC}"; }
print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_error() { echo -e "${RED}✗ $1${NC}"; }

echo "🧪 Testing CSV Import - Single Project"
echo "======================================"

# Get admin token
print_step "Getting admin token..."
ADMIN_TOKEN=$(curl -s -H "Content-Type: application/json" \
    -d '{"username": "test_admin", "password": "Admin123!"}' \
    "$BASE_URL/api/v1/auth/login" | jq -r '.data.token')

if [ -z "$ADMIN_TOKEN" ] || [ "$ADMIN_TOKEN" = "null" ]; then
    print_error "Failed to get admin token"
    exit 1
fi

print_success "Token obtained"

# Read first data line from CSV (skip header)
print_step "Reading first project from CSV..."
FIRST_LINE=$(sed -n '2p' "$CSV_FILE")

if [ -z "$FIRST_LINE" ]; then
    print_error "No data found in CSV"
    exit 1
fi

echo "First line: $FIRST_LINE"

# Parse the line
IFS=',' read -ra FIELDS <<< "$FIRST_LINE"

PROJECT_ID="${FIELDS[0]}"
LOCATION="${FIELDS[1]}"
BRANCH="${FIELDS[2]}"
CAPACITY="${FIELDS[3]}"

echo ""
echo "Parsed data:"
echo "• Project ID: $PROJECT_ID"
echo "• Location: $LOCATION"
echo "• Branch: $BRANCH"
echo "• Capacity: $CAPACITY"

# Test project creation
print_step "Creating test project..."

PROJECT_JSON='{
    "projectName": "TEST-PWA-'$PROJECT_ID': '$LOCATION'",
    "address": "'$LOCATION', Thailand",
    "clientInfo": "การประปาส่วนภูมิภาค - '$BRANCH'",
    "startDate": "2025-07-04T00:00:00Z",
    "estimatedEndDate": "2025-09-02T00:00:00Z",
    "projectManagerId": "cface76b-1457-44a1-89fa-6b4ccc2f5f66",
    "team": "Solar Installation Team",
    "connectionType": "MV",
    "connectionNotes": "TEST: PWA Solar Project",
    "totalCapacityKw": '$CAPACITY',
    "pvModuleCount": 200,
    "equipmentDetails": {
        "inverter125kw": 1,
        "inverter80kw": 0,
        "inverter60kw": 0,
        "inverter40kw": 0
    },
    "ftsValue": 1000000,
    "revenueValue": 1200000,
    "pqmValue": 100000,
    "locationCoordinates": {
        "latitude": 13.7563,
        "longitude": 100.5018
    }
}'

echo ""
echo "Project JSON:"
echo "$PROJECT_JSON" | jq '.'

echo ""
print_step "Sending to API..."

RESPONSE=$(curl -s -w "%{http_code}" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -d "$PROJECT_JSON" \
    "$BASE_URL/api/v1/projects")

HTTP_CODE="${RESPONSE: -3}"
BODY="${RESPONSE%???}"

echo "HTTP Status: $HTTP_CODE"
echo "Response Body:"
echo "$BODY" | jq '.' 2>/dev/null || echo "$BODY"

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "201" ]; then
    print_success "✅ Test project created successfully!"
    echo ""
    echo "🎉 CSV import should work. You can now run:"
    echo "   ./scripts/simple-csv-import.sh"
else
    print_error "❌ Test failed"
    echo ""
    echo "🔧 Debug the issue above before running full import"
fi
