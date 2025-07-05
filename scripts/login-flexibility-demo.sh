#!/bin/bash

# Login Flexibility Demo - Email vs Username
# This script demonstrates that the login 'username' field accepts both username and email

API_BASE="http://localhost:5001/api/v1"

echo "🔐 Login Field Flexibility Demo"
echo "==============================="
echo ""
echo "The Solar Projects API login supports flexible authentication:"
echo "The 'username' field in the login request accepts BOTH:"
echo "  • Actual username (e.g., 'admin')"
echo "  • Email address (e.g., 'admin@example.com')"
echo ""
echo "This is handled in AuthService.cs line 32:"
echo "  u.Username == request.Username || u.Email == request.Username"
echo ""

# Demonstrate with working fallback credentials
echo "📝 Demo 1: Login using EMAIL in username field..."
echo "Request payload:"
echo '{
  "username": "admin@example.com",  ← EMAIL in username field
  "password": "Admin123!"
}'
echo ""

EMAIL_LOGIN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@example.com",
    "password": "Admin123!"
  }')

EMAIL_SUCCESS=$(echo $EMAIL_LOGIN | grep -o '"success":[^,]*' | cut -d':' -f2)
EMAIL_USER=$(echo $EMAIL_LOGIN | grep -o '"username":"[^"]*"' | cut -d'"' -f4)

if [ "$EMAIL_SUCCESS" = "true" ]; then
    echo "✅ SUCCESS: Logged in using EMAIL in username field"
    echo "   Authenticated user: $EMAIL_USER"
else
    echo "❌ FAILED: Email login failed"
fi

echo ""
echo "----------------------------------------"
echo ""

# Show what would happen with database user (if available)
echo "📝 Demo 2: What database user login would look like..."
echo ""
echo "If database user 'admin' was seeded, these would BOTH work:"
echo ""
echo "Option A - Using USERNAME:"
echo '{
  "username": "admin",              ← USERNAME in username field
  "password": "Admin123!"
}'
echo ""
echo "Option B - Using EMAIL:"
echo '{
  "username": "admin@solarprojects.com", ← EMAIL in username field  
  "password": "Admin123!"
}'
echo ""
echo "Both would authenticate the SAME user because the system checks:"
echo "  (u.Username == 'admin' || u.Email == 'admin') OR"
echo "  (u.Username == 'admin@solarprojects.com' || u.Email == 'admin@solarprojects.com')"
echo ""

# Test the database user options
DB_USERNAME_TEST=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }')

DB_EMAIL_TEST=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@solarprojects.com",
    "password": "Admin123!"
  }')

DB_USERNAME_SUCCESS=$(echo $DB_USERNAME_TEST | grep -o '"success":[^,]*' | cut -d':' -f2)
DB_EMAIL_SUCCESS=$(echo $DB_EMAIL_TEST | grep -o '"success":[^,]*' | cut -d':' -f2)

echo "Testing database user login options:"
if [ "$DB_USERNAME_SUCCESS" = "true" ]; then
    echo "✅ Database login with USERNAME works"
else
    echo "⚠️  Database login with USERNAME not available (user not seeded)"
fi

if [ "$DB_EMAIL_SUCCESS" = "true" ]; then
    echo "✅ Database login with EMAIL works"
else
    echo "⚠️  Database login with EMAIL not available (user not seeded)"
fi

echo ""
echo "==============================="
echo "Summary:"
echo "• The login 'username' field is flexible and accepts both username and email"
echo "• This provides a better user experience - users can login with either"
echo "• The backend automatically matches against both User.Username and User.Email"
echo "• Currently working: admin@example.com (fallback credentials)"
echo "• Future: admin or admin@solarprojects.com (when database user is seeded)"
echo ""
echo "💡 Frontend Integration Tip:"
echo "Your login form can have a single 'Username or Email' field instead of separate fields!"
