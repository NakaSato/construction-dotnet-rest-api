#!/bin/bash

# Test script for Rich HATEOAS Pagination Endpoints
# This script tests the enhanced pagination features across all collection endpoints

echo "🧪 Testing Rich HATEOAS Pagination Endpoints"
echo "=============================================="

API_BASE="http://localhost:5001"

# Function to test endpoint with better error handling
test_endpoint() {
    local endpoint="$1"
    local description="$2"
    
    echo ""
    echo "📋 Testing: $description"
    echo "Endpoint: $endpoint"
    echo "---"
    
    # Use curl with better error handling and timeout
    response=$(curl -s -w "%{http_code}" --max-time 10 "$endpoint" 2>/dev/null)
    http_code="${response: -3}"
    body="${response%???}"
    
    if [ "$http_code" = "200" ]; then
        echo "✅ Status: 200 OK"
        # Parse and display key pagination info
        if command -v jq >/dev/null 2>&1; then
            echo "📊 Pagination Info:"
            echo "$body" | jq -r '.data.pagination | "  Total Items: \(.totalItems // "N/A"), Total Pages: \(.totalPages // "N/A"), Current Page: \(.currentPage // "N/A")"' 2>/dev/null || echo "  Could not parse pagination info"
            echo "🔗 Navigation Links:"
            echo "$body" | jq -r '.data.pagination.links | "  First: \(.first // "N/A")\n  Current: \(.current // "N/A")\n  Next: \(.next // "N/A")\n  Last: \(.last // "N/A")"' 2>/dev/null || echo "  Could not parse navigation links"
        else
            echo "  Response received (jq not available for parsing)"
        fi
    elif [ "$http_code" = "401" ]; then
        echo "🔒 Status: 401 Unauthorized (Expected - authentication required)"
    elif [ "$http_code" = "000" ]; then
        echo "❌ Connection failed - is the server running on localhost:5001?"
    else
        echo "⚠️  Status: $http_code"
        echo "   Response: $body"
    fi
}

# Test all rich pagination endpoints
echo "🚀 Starting tests..."

# 1. Projects Rich Pagination
test_endpoint "$API_BASE/api/v1/projects/rich?page=1&pageSize=5" "Projects Rich Pagination"

# 2. Users Rich Pagination  
test_endpoint "$API_BASE/api/v1/users/rich?page=1&pageSize=3" "Users Rich Pagination"

# 3. Tasks Rich Pagination
test_endpoint "$API_BASE/api/v1/tasks/rich?page=1&pageSize=5&sortBy=title&sortOrder=desc" "Tasks Rich Pagination with Sorting"

# 4. Test with complex query parameters
test_endpoint "$API_BASE/api/v1/projects/rich?page=2&pageSize=2&sortBy=projectName&sortOrder=asc" "Projects Rich Pagination - Page 2"

# 5. Health check to ensure API is responsive
echo ""
echo "🏥 Health Check"
echo "---"
test_endpoint "$API_BASE/health" "API Health Check"

echo ""
echo "🎉 Testing Complete!"
echo ""
echo "📝 Notes:"
echo "  - 401 Unauthorized responses are expected for protected endpoints"
echo "  - To test with authentication, first login via /api/v1/auth/login"
echo "  - Rich pagination endpoints provide enhanced metadata and HATEOAS links"
echo "  - Check the API documentation at: $API_BASE/swagger"
