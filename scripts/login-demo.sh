#!/bin/bash

# Login Methods Demonstration for Solar Projects API
# This script shows all available ways to login to the API

API_BASE="http://localhost:5001/api/v1"

echo "🔐 Solar Projects API - Login Methods Demo"
echo "=========================================="
echo ""

echo "Available Login Credentials:"
echo "1. Fallback Admin (Always Available):"
echo "   Username: admin@example.com"
echo "   Password: Admin123!"
echo ""
echo "2. Database Admin (If Seeded):"
echo "   Username: admin"
echo "   Email: admin@solarprojects.com" 
echo "   Password: Admin123!"
echo ""

# Test Method 1: Fallback Admin
echo "📝 Testing Method 1: Fallback Admin Login..."
echo "Request:"
echo 'curl -X POST "http://localhost:5001/api/v1/auth/login" \'
echo '  -H "Content-Type: application/json" \'
echo '  -d '"'"'{'
echo '    "username": "admin@example.com",'
echo '    "password": "Admin123!"'
echo '  }'"'"''
echo ""

FALLBACK_LOGIN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@example.com",
    "password": "Admin123!"
  }')

FALLBACK_TOKEN=$(echo $FALLBACK_LOGIN | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
FALLBACK_SUCCESS=$(echo $FALLBACK_LOGIN | grep -o '"success":[^,]*' | cut -d':' -f2)

if [ "$FALLBACK_SUCCESS" = "true" ]; then
    echo "✅ Method 1 SUCCESS: Fallback admin login works"
    echo "   Token: ${FALLBACK_TOKEN:0:50}..."
    USER_INFO=$(echo $FALLBACK_LOGIN | grep -o '"user":{[^}]*}' | sed 's/"user"://')
    echo "   User: $USER_INFO"
else
    echo "❌ Method 1 FAILED: Fallback admin login failed"
fi

echo ""
echo "----------------------------------------"

# Test Method 2: Database Admin by Username
echo "📝 Testing Method 2: Database Admin Login (Username)..."
echo "Request:"
echo 'curl -X POST "http://localhost:5001/api/v1/auth/login" \'
echo '  -H "Content-Type: application/json" \'
echo '  -d '"'"'{'
echo '    "username": "admin",'
echo '    "password": "Admin123!"'
echo '  }'"'"''
echo ""

DB_USERNAME_LOGIN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }')

DB_USERNAME_TOKEN=$(echo $DB_USERNAME_LOGIN | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
DB_USERNAME_SUCCESS=$(echo $DB_USERNAME_LOGIN | grep -o '"success":[^,]*' | cut -d':' -f2)

if [ "$DB_USERNAME_SUCCESS" = "true" ]; then
    echo "✅ Method 2 SUCCESS: Database admin username login works"
    echo "   Token: ${DB_USERNAME_TOKEN:0:50}..."
    USER_INFO=$(echo $DB_USERNAME_LOGIN | grep -o '"user":{[^}]*}' | sed 's/"user"://')
    echo "   User: $USER_INFO"
else
    echo "⚠️  Method 2 NOT AVAILABLE: Database admin username login failed"
    echo "   This is normal if database user is not seeded yet"
fi

echo ""
echo "----------------------------------------"

# Test Method 3: Database Admin by Email
echo "📝 Testing Method 3: Database Admin Login (Email)..."
echo "Request:"
echo 'curl -X POST "http://localhost:5001/api/v1/auth/login" \'
echo '  -H "Content-Type: application/json" \'
echo '  -d '"'"'{'
echo '    "username": "admin@solarprojects.com",'
echo '    "password": "Admin123!"'
echo '  }'"'"''
echo ""

DB_EMAIL_LOGIN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@solarprojects.com",
    "password": "Admin123!"
  }')

DB_EMAIL_TOKEN=$(echo $DB_EMAIL_LOGIN | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
DB_EMAIL_SUCCESS=$(echo $DB_EMAIL_LOGIN | grep -o '"success":[^,]*' | cut -d':' -f2)

if [ "$DB_EMAIL_SUCCESS" = "true" ]; then
    echo "✅ Method 3 SUCCESS: Database admin email login works"
    echo "   Token: ${DB_EMAIL_TOKEN:0:50}..."
    USER_INFO=$(echo $DB_EMAIL_LOGIN | grep -o '"user":{[^}]*}' | sed 's/"user"://')
    echo "   User: $USER_INFO"
else
    echo "⚠️  Method 3 NOT AVAILABLE: Database admin email login failed"
    echo "   This is normal if database user is not seeded yet"
fi

echo ""
echo "========================================"
echo "Summary of Working Login Methods:"

if [ "$FALLBACK_SUCCESS" = "true" ]; then
    echo "✅ Fallback Admin: admin@example.com / Admin123!"
fi

if [ "$DB_USERNAME_SUCCESS" = "true" ]; then
    echo "✅ Database Admin (username): admin / Admin123!"
fi

if [ "$DB_EMAIL_SUCCESS" = "true" ]; then
    echo "✅ Database Admin (email): admin@solarprojects.com / Admin123!"
fi

echo ""
echo "💡 How to use your token:"
echo "TOKEN=\"your_jwt_token_here\""
echo "curl -X GET \"http://localhost:5001/api/v1/projects\" \\"
echo "  -H \"Authorization: Bearer \$TOKEN\""
echo ""
echo "🌐 Or test in Swagger UI: http://localhost:5001/swagger"
