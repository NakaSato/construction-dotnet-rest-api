#!/bin/bash

# Solar Projects API - Create Test Accounts for Permission Testing
# This script creates one test account for each role with simple credentials

echo "=================================="
echo "Creating Test Accounts for Permission Testing"
echo "=================================="

API_BASE="http://localhost:5002"

# Test if API is running
echo "🔍 Checking if API is available..."
if ! curl -s -f "$API_BASE/health" > /dev/null; then
    echo "❌ API is not running. Please start with: docker-compose up -d"
    exit 1
fi
echo "✅ API is available"
echo

# Function to register a user
register_user() {
    local username=$1
    local email=$2
    local password=$3
    local fullname=$4
    local roleid=$5
    local rolename=$6
    
    echo "📝 Creating $rolename account: $username"
    
    response=$(curl -s -w "HTTP_STATUS:%{http_code}" -X POST "$API_BASE/api/v1/auth/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"fullName\": \"$fullname\",
            \"roleId\": $roleid
        }")
    
    status=$(echo "$response" | grep -o "HTTP_STATUS:[0-9]*" | cut -d: -f2)
    body=$(echo "$response" | sed 's/HTTP_STATUS:[0-9]*$//')
    
    if [ "$status" = "201" ] || [ "$status" = "200" ]; then
        echo "   ✅ Successfully created $username ($rolename)"
        echo "   📧 Email: $email"
        echo "   🔑 Password: $password"
    elif [ "$status" = "400" ] && echo "$body" | grep -q "already exists"; then
        echo "   ⚠️  User $username already exists (skipping)"
    else
        echo "   ❌ Failed to create $username (HTTP $status)"
        echo "   Response: $body"
    fi
    echo
}

echo "Creating test accounts with role-based permissions..."
echo

# Create Admin test account
register_user "test_admin" "test_admin@example.com" "Admin123!" "Test Administrator" 1 "Admin"

# Create Manager test account  
register_user "test_manager" "test_manager@example.com" "Manager123!" "Test Manager" 2 "Manager"

# Create User test account
register_user "test_user" "test_user@example.com" "User123!" "Test User" 3 "User"

# Create Viewer test account
register_user "test_viewer" "test_viewer@example.com" "Viewer123!" "Test Viewer" 4 "Viewer"

echo "=================================="
echo "✅ Test Account Creation Complete!"
echo "=================================="
echo
echo "🔐 TEST ACCOUNTS FOR PERMISSION TESTING:"
echo
echo "👨‍💼 ADMIN (Full Access)"
echo "   Username: test_admin"
echo "   Password: Admin123!"
echo "   Email: test_admin@example.com"
echo "   Permissions: Full system access, user management, all operations"
echo
echo "👩‍💼 MANAGER (Project Management)"
echo "   Username: test_manager" 
echo "   Password: Manager123!"
echo "   Email: test_manager@example.com"
echo "   Permissions: Project management, team oversight, reporting"
echo
echo "👨‍🔧 USER (Field Operations)"
echo "   Username: test_user"
echo "   Password: User123!"
echo "   Email: test_user@example.com" 
echo "   Permissions: Task management, daily reports, work requests"
echo
echo "👁️ VIEWER (Read-Only)"
echo "   Username: test_viewer"
echo "   Password: Viewer123!"
echo "   Email: test_viewer@example.com"
echo "   Permissions: View-only access to projects and reports"
echo
echo "=================================="
echo "🧪 TESTING INSTRUCTIONS:"
echo "=================================="
echo "1. Login with each account to get JWT tokens:"
echo "   curl -X POST $API_BASE/api/v1/auth/login \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d '{\"username\":\"test_admin\",\"password\":\"Admin123!\"}'"
echo
echo "2. Use the JWT token to test protected endpoints:"
echo "   curl -H \"Authorization: Bearer YOUR_JWT_TOKEN\" \\"
echo "     $API_BASE/api/v1/projects"
echo
echo "3. Try different operations with each role to test permissions:"
echo "   - Admin: Can create/edit/delete everything"
echo "   - Manager: Can manage projects and teams"
echo "   - User: Can update tasks and create reports"
echo "   - Viewer: Can only view/read data"
echo
echo "4. Use the test scripts with these accounts:"
echo "   ./test-quick-endpoints.sh"
echo "   ./test-all-endpoints.sh"
echo
echo "=================================="
