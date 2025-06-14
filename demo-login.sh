#!/bin/bash

# Solar Projects API - Login Demo for Test Accounts
# Demonstrates how to login with each test account and get JWT tokens

API_BASE="http://localhost:5002"

echo "🔐=================================================================="
echo "Solar Projects API - Test Account Login Demonstration"
echo "===================================================================="

# Function to demonstrate login
demo_login() {
    local username=$1
    local password=$2
    local role=$3
    local description=$4
    
    echo "🔑 Logging in as $role: $username"
    echo "   Password: $password"
    echo "   Description: $description"
    echo
    
    echo "   Command:"
    echo "   curl -X POST $API_BASE/api/v1/auth/login \\"
    echo "     -H \"Content-Type: application/json\" \\"
    echo "     -d '{\"username\":\"$username\",\"password\":\"$password\"}'"
    echo
    
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"username\":\"$username\",\"password\":\"$password\"}")
    
    if echo "$response" | grep -q '"token":'; then
        token=$(echo "$response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        echo "   ✅ Login successful!"
        echo "   🎫 JWT Token: ${token:0:50}..."
        echo
        echo "   📋 Use this token for API calls:"
        echo "   curl -H \"Authorization: Bearer $token\" \\"
        echo "     $API_BASE/api/v1/projects"
        echo
    else
        echo "   ❌ Login failed!"
        echo "   Response: $response"
        echo
    fi
    
    echo "   ──────────────────────────────────────────────"
}

echo "🔍 Checking API availability..."
if ! curl -s -f "$API_BASE/health" > /dev/null; then
    echo "❌ API is not running. Please start with: docker-compose up -d"
    exit 1
fi
echo "✅ API is available"
echo

echo "Demonstrating login for each test account..."
echo "============================================="
echo

demo_login "test_admin" "Admin123!" "ADMIN" "Full system access, user management, all operations"

demo_login "test_manager" "Manager123!" "MANAGER" "Project management, team oversight, reporting"

demo_login "test_user" "User123!" "USER" "Task management, daily reports, work requests"

demo_login "test_viewer" "Viewer123!" "VIEWER" "View-only access to projects and reports"

echo "===================================================================="
echo "📚 QUICK REFERENCE - TEST ACCOUNTS"
echo "===================================================================="
echo
echo "🔐 All test accounts use the pattern: [role]123!"
echo
echo "👨‍💼 ADMIN ACCOUNT:"
echo "   Username: test_admin"
echo "   Password: Admin123!"
echo "   Email: test_admin@example.com"
echo "   Role: Full access to all system features"
echo
echo "👩‍💼 MANAGER ACCOUNT:"
echo "   Username: test_manager"
echo "   Password: Manager123!"
echo "   Email: test_manager@example.com"
echo "   Role: Project and team management capabilities"
echo
echo "👨‍🔧 USER ACCOUNT:"
echo "   Username: test_user"
echo "   Password: User123!"
echo "   Email: test_user@example.com"
echo "   Role: Field operations and task management"
echo
echo "👁️ VIEWER ACCOUNT:"
echo "   Username: test_viewer"
echo "   Password: Viewer123!"
echo "   Email: test_viewer@example.com"
echo "   Role: Read-only access to view data"
echo
echo "===================================================================="
echo "🧪 TESTING WORKFLOW:"
echo "===================================================================="
echo
echo "1. 🔑 LOGIN: Use the commands above to get JWT tokens"
echo
echo "2. 📊 TEST ENDPOINTS: Use tokens to access protected endpoints"
echo "   Example:"
echo "   TOKEN=\"your_jwt_token_here\""
echo "   curl -H \"Authorization: Bearer \$TOKEN\" \\"
echo "     $API_BASE/api/v1/projects"
echo
echo "3. 🔒 VERIFY PERMISSIONS: Test different operations with each role"
echo "   - Admin: Try creating/editing/deleting resources"
echo "   - Manager: Try project management operations"
echo "   - User: Try operational tasks and reporting"
echo "   - Viewer: Try read operations (should work) and write operations (should fail)"
echo
echo "4. 🚫 TEST UNAUTHORIZED: Try accessing endpoints without tokens"
echo "   curl $API_BASE/api/v1/projects  # Should return 401 Unauthorized"
echo
echo "===================================================================="
echo "💡 TIPS:"
echo "   • Save JWT tokens in environment variables for easier testing"
echo "   • Use tools like Postman or Insomnia for GUI-based testing"
echo "   • Check the API_REFERENCE.md for complete endpoint documentation"
echo "   • Run ./test-permissions.sh for automated permission testing"
echo "===================================================================="
