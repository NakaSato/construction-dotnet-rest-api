#!/bin/bash

# =============================================================================
# 🔧 TEST USER SETUP SCRIPT
# =============================================================================
# Creates test users for comprehensive API testing (all roles)
# =============================================================================

BASE_URL="http://localhost:5001"

echo "🔧 Setting up test users for API testing..."

# Create test users via registration (publicly accessible endpoint)
echo "� Creating test users..."

# Create admin user
echo "🔑 Creating admin user..."
ADMIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "admin",
        "email": "admin@solarprojects.com",
        "password": "Admin123!",
        "fullName": "System Administrator",
        "roleId": 1
    }')

if echo "$ADMIN_RESPONSE" | grep -q '"success":true'; then
    echo "✅ Admin user created successfully"
else
    echo "⚠️  Admin user may already exist: $ADMIN_RESPONSE"
fi

# Create manager user
echo "👔 Creating manager user..."
MANAGER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "manager1",
        "email": "manager1@solarprojects.com",
        "password": "Admin123!",
        "fullName": "Sarah Johnson",
        "roleId": 2
    }')

if echo "$MANAGER_RESPONSE" | grep -q '"success":true'; then
    echo "✅ Manager user created successfully"
else
    echo "⚠️  Manager user may already exist: $MANAGER_RESPONSE"
fi

# Create regular user
echo "👤 Creating regular user..."
USER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "tech1",
        "email": "tech1@solarprojects.com",
        "password": "Admin123!",
        "fullName": "Mike Rodriguez",
        "roleId": 3
    }')

if echo "$USER_RESPONSE" | grep -q '"success":true'; then
    echo "✅ Regular user created successfully"
else
    echo "⚠️  Regular user may already exist: $USER_RESPONSE"
fi

# Create viewer user
echo "👁️  Creating viewer user..."
VIEWER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "viewer",
        "email": "viewer@solarprojects.com",
        "password": "Admin123!",
        "fullName": "Test Viewer",
        "roleId": 4
    }')

if echo "$VIEWER_RESPONSE" | grep -q '"success":true'; then
    echo "✅ Viewer user created successfully"
else
    echo "⚠️  Viewer user may already exist: $VIEWER_RESPONSE"
fi

# Test login for each user
echo ""
echo "🔍 Testing login for each user..."

for user in "admin" "manager1" "tech1" "viewer"; do
    echo "Testing login for $user..."
    LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v1/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"username\":\"$user\",\"password\":\"Admin123!\"}")
    
    if echo "$LOGIN_RESPONSE" | grep -q '"token"'; then
        echo "✅ $user can login successfully"
    else
        echo "❌ $user login failed: $LOGIN_RESPONSE"
    fi
done

echo ""
echo "🚀 Test users are ready! You can now run the comprehensive API tests."
echo "📋 Available test users:"
echo "   Admin:   admin:Admin123!"
echo "   Manager: manager1:Admin123!"
echo "   User:    tech1:Admin123!"
echo "   Viewer:  viewer:Admin123!"
