#!/bin/bash

# Solar Projects API - Permission Testing Script
# Tests each role's access to different API endpoints

API_BASE="http://localhost:5002"

echo "🔐=================================================================="
echo "Solar Projects API - Role-Based Permission Testing"
echo "===================================================================="

# Function to login and get JWT token
login_user() {
    local username=$1
    local password=$2
    
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"username\":\"$username\",\"password\":\"$password\"}")
    
    echo "$response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4
}

# Function to test API endpoint with authentication
test_endpoint() {
    local method=$1
    local endpoint=$2
    local token=$3
    local data=$4
    local description=$5
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data")
    else
        response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Authorization: Bearer $token")
    fi
    
    status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    
    if [ "$status" = "200" ] || [ "$status" = "201" ]; then
        echo "   ✅ $description (HTTP $status)"
    elif [ "$status" = "401" ]; then
        echo "   🔒 $description - UNAUTHORIZED (HTTP $status)"
    elif [ "$status" = "403" ]; then
        echo "   🚫 $description - FORBIDDEN (HTTP $status)"
    else
        echo "   ❌ $description - ERROR (HTTP $status)"
    fi
}

echo "🔍 Testing API availability..."
if ! curl -s -f "$API_BASE/health" > /dev/null; then
    echo "❌ API is not running. Please start with: docker-compose up -d"
    exit 1
fi
echo "✅ API is available"
echo

# Login all test users
echo "🔑 Logging in test users..."
admin_token=$(login_user "test_admin" "Admin123!")
manager_token=$(login_user "test_manager" "Manager123!")
user_token=$(login_user "test_user" "User123!")
viewer_token=$(login_user "test_viewer" "Viewer123!")

if [ -z "$admin_token" ] || [ -z "$manager_token" ] || [ -z "$user_token" ] || [ -z "$viewer_token" ]; then
    echo "❌ Failed to login one or more test users. Please run ./create-test-accounts.sh first"
    exit 1
fi

echo "✅ All users logged in successfully"
echo

echo "===================================================================="
echo "👨‍💼 TESTING ADMIN PERMISSIONS (test_admin)"
echo "===================================================================="
echo "🧪 Testing Admin access to all endpoints..."
test_endpoint "GET" "/api/v1/projects" "$admin_token" "" "View Projects"
test_endpoint "POST" "/api/v1/projects" "$admin_token" '{
    "projectName": "Admin Test Project",
    "address": "123 Admin Street, Admin City, AC 12345",
    "clientInfo": "Admin Test Client",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-12-31T23:59:59Z"
}' "Create Project"
test_endpoint "GET" "/api/v1/users" "$admin_token" "" "View Users"
test_endpoint "GET" "/api/v1/tasks" "$admin_token" "" "View Tasks"
test_endpoint "GET" "/api/v1/daily-reports" "$admin_token" "" "View Daily Reports"
test_endpoint "GET" "/api/v1/work-requests" "$admin_token" "" "View Work Requests"

echo
echo "===================================================================="
echo "👩‍💼 TESTING MANAGER PERMISSIONS (test_manager)"
echo "===================================================================="
echo "🧪 Testing Manager access to project management endpoints..."
test_endpoint "GET" "/api/v1/projects" "$manager_token" "" "View Projects"
test_endpoint "POST" "/api/v1/projects" "$manager_token" '{
    "projectName": "Manager Test Project",
    "address": "456 Manager Street, Manager City, MC 12345",
    "clientInfo": "Manager Test Client",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-12-31T23:59:59Z"
}' "Create Project"
test_endpoint "GET" "/api/v1/users" "$manager_token" "" "View Users"
test_endpoint "GET" "/api/v1/tasks" "$manager_token" "" "View Tasks"
test_endpoint "GET" "/api/v1/daily-reports" "$manager_token" "" "View Daily Reports"
test_endpoint "GET" "/api/v1/work-requests" "$manager_token" "" "View Work Requests"

echo
echo "===================================================================="
echo "👨‍🔧 TESTING USER PERMISSIONS (test_user)"
echo "===================================================================="
echo "🧪 Testing User access to operational endpoints..."
test_endpoint "GET" "/api/v1/projects" "$user_token" "" "View Projects"
test_endpoint "POST" "/api/v1/projects" "$user_token" '{
    "projectName": "User Test Project",
    "address": "789 User Street, User City, UC 12345",
    "clientInfo": "User Test Client",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-12-31T23:59:59Z"
}' "Create Project"
test_endpoint "GET" "/api/v1/users" "$user_token" "" "View Users"
test_endpoint "GET" "/api/v1/tasks" "$user_token" "" "View Tasks"
test_endpoint "GET" "/api/v1/daily-reports" "$user_token" "" "View Daily Reports"
test_endpoint "GET" "/api/v1/work-requests" "$user_token" "" "View Work Requests"

echo
echo "===================================================================="
echo "👁️ TESTING VIEWER PERMISSIONS (test_viewer)"
echo "===================================================================="
echo "🧪 Testing Viewer read-only access..."
test_endpoint "GET" "/api/v1/projects" "$viewer_token" "" "View Projects"
test_endpoint "POST" "/api/v1/projects" "$viewer_token" '{
    "projectName": "Viewer Test Project",
    "address": "101 Viewer Street, Viewer City, VC 12345",
    "clientInfo": "Viewer Test Client",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-12-31T23:59:59Z"
}' "Create Project (Should Fail)"
test_endpoint "GET" "/api/v1/users" "$viewer_token" "" "View Users"
test_endpoint "GET" "/api/v1/tasks" "$viewer_token" "" "View Tasks"
test_endpoint "GET" "/api/v1/daily-reports" "$viewer_token" "" "View Daily Reports"
test_endpoint "GET" "/api/v1/work-requests" "$viewer_token" "" "View Work Requests"

echo
echo "===================================================================="
echo "🔒 TESTING UNAUTHENTICATED ACCESS"
echo "===================================================================="
echo "🧪 Testing endpoints without authentication (should fail)..."
test_endpoint "GET" "/api/v1/projects" "" "" "View Projects (No Auth)"
test_endpoint "GET" "/api/v1/users" "" "" "View Users (No Auth)"
test_endpoint "GET" "/api/v1/tasks" "" "" "View Tasks (No Auth)"

echo
echo "===================================================================="
echo "📊 PERMISSION TESTING SUMMARY"
echo "===================================================================="
echo "🔐 TEST ACCOUNTS USED:"
echo "   👨‍💼 Admin:   test_admin   / Admin123!"
echo "   👩‍💼 Manager: test_manager / Manager123!"
echo "   👨‍🔧 User:    test_user    / User123!"
echo "   👁️ Viewer:  test_viewer  / Viewer123!"
echo
echo "📋 EXPECTED RESULTS:"
echo "   ✅ Admin:   Should have access to ALL endpoints"
echo "   ✅ Manager: Should have access to most endpoints"
echo "   ✅ User:    Should have limited access to operational endpoints"
echo "   ✅ Viewer:  Should have READ-ONLY access (GET requests only)"
echo "   🔒 No Auth: Should be UNAUTHORIZED for all protected endpoints"
echo
echo "💡 NEXT STEPS:"
echo "   1. Review the test results above"
echo "   2. Use these accounts for manual testing"
echo "   3. Test specific role-based features in your application"
echo "   4. Modify permissions in the code if needed"
echo
echo "🚀 For manual testing, login with:"
echo "   curl -X POST $API_BASE/api/v1/auth/login \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d '{\"username\":\"test_admin\",\"password\":\"Admin123!\"}'"
echo
echo "===================================================================="
