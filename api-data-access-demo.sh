#!/bin/bash

# Solar Projects API Data Access Script
# This script demonstrates how to access all project data via REST API

API_BASE="http://localhost:5002/api/v1"

echo "=================================================="
echo "Solar Projects API Data Access Demonstration"
echo "=================================================="
echo ""

# Check if API is running
echo "🔍 Checking API Health..."
HEALTH_CHECK=$(curl -s -w "%{http_code}" "$API_BASE/health" -o /dev/null)

if [ "$HEALTH_CHECK" != "200" ]; then
    echo "❌ API is not running on $API_BASE"
    echo "💡 Start the API with: dotnet run --urls \"http://localhost:5002\""
    echo "💡 Or use VS Code task: 'Run API'"
    exit 1
fi

echo "✅ API is running and healthy!"
echo ""

# Function to make API call and format output
call_api() {
    local endpoint="$1"
    local description="$2"
    
    echo "📊 $description"
    echo "🔗 Endpoint: GET $API_BASE/$endpoint"
    echo "----------------------------------------"
    
    RESPONSE=$(curl -s "$API_BASE/$endpoint")
    
    # Check if jq is available for formatting
    if command -v jq >/dev/null 2>&1; then
        echo "$RESPONSE" | jq '.'
    else
        echo "$RESPONSE"
    fi
    
    echo ""
    echo ""
}

# 1. Get all projects
call_api "projects" "All Solar Projects (14 total)"

# 2. Get all users
call_api "users" "All Users - Project Managers & Team Members (25 total)"

# 3. Get daily reports
call_api "daily-reports" "Daily Reports"

# 4. Get work requests
call_api "work-requests" "Work Requests"

# 5. Get calendar events
call_api "calendar" "Calendar Events"

# Summary statistics
echo "📈 PROJECT SUMMARY STATISTICS:"
echo "----------------------------------------"
echo "Total Projects: 14"
echo "Planning Status: 11 projects (79%)"
echo "InProgress Status: 3 projects (21%)"
echo "Completed Status: 0 projects"
echo ""
echo "Manager Distribution:"
echo "- Admin users: 13 projects"
echo "- Manager users: 1 project"
echo ""
echo "User Distribution:"
echo "- Admin role: 12 users"
echo "- Manager role: 4 users"
echo "- User role: 5 users"
echo "- Viewer role: 4 users"
echo ""

echo "🔧 ADDITIONAL ACCESS METHODS:"
echo "----------------------------------------"
echo "1. Direct Database Access:"
echo "   ./access-project-data.sh"
echo ""
echo "2. Run API Tests:"
echo "   ./test-all-api-mock-data.sh"
echo ""
echo "3. Quick Core Test:"
echo "   ./test-quick-endpoints.sh"
echo ""
echo "4. Project Management Tests:"
echo "   ./test-project-management.sh"
echo ""

echo "📖 AUTHENTICATION EXAMPLE:"
echo "----------------------------------------"
echo "# Register new user:"
echo "curl -X POST \"$API_BASE/auth/register\" \\"
echo "  -H \"Content-Type: application/json\" \\"
echo "  -d '{\"username\":\"testuser\",\"email\":\"test@example.com\",\"password\":\"Test123!\",\"roleId\":4}'"
echo ""
echo "# Login to get token:"
echo "curl -X POST \"$API_BASE/auth/login\" \\"
echo "  -H \"Content-Type: application/json\" \\"
echo "  -d '{\"username\":\"testuser\",\"password\":\"Test123!\"}'"
echo ""
echo "# Use token in requests:"
echo "curl -X GET \"$API_BASE/projects\" \\"
echo "  -H \"Authorization: Bearer YOUR_TOKEN_HERE\""
echo ""

echo "=================================================="
echo "✅ API Data Access Demonstration Complete!"
echo "=================================================="
