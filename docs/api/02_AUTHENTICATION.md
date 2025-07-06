# Authentication & User Management

Authentication endpoints, user registration, and role-based access control for the Solar Project Management API.

## Authentication Overview

All API endpoints require JWT authentication:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Security Features
- JWT token authentication
- Refresh token rotation
- Role-based access control (RBAC)
- Password security policies
- Session management
- Audit logging

## User Roles & Permissions

| Role ID | Role Name | Project Access | Description |
|---------|-----------|----------------|-------------|
| `1` | Admin | Full CRUD + Delete | Complete system access |
| `2` | Manager | Full CRUD | Project management |
| `3` | User | Read + Own Reports | Field technician access |
| `4` | Viewer | Read Only | Client/reporting access |

### Permission Matrix

| Action | Admin | Manager | User | Viewer |
|--------|-------|---------|------|--------|
| Projects | Full CRUD + Delete | Full CRUD | Read Only | Read Only |
| Master Plans | Full CRUD + Delete | Full CRUD | Read Only | Read Only |
| Tasks | Full CRUD + Delete | Full CRUD | Read + Own CRUD | Read Only |
| Daily Reports | Full CRUD + Delete | Full CRUD + Approve | Own CRUD | Read Only |
| Work Requests | Full CRUD + Delete | Full CRUD + Approve | Own CRUD | Read Only |
| Users | Full CRUD | Read + Team Management | Read Own Profile | Read Own Profile |

### Test Accounts

| Username | Email | Password | Role |
|----------|-------|----------|------|
| `admin` | `admin@solarprojects.com` | `Admin123!` | Admin |
| `test_admin` | `test_admin@solarprojects.com` | `Admin123!` | Admin |

**Note**: Only test admin accounts are currently seeded. Create additional users via API endpoints or database scripts.

## Login

**POST** `/api/v1/auth/login`

### Request

```json
// Option 1: Login with username
{
  "username": "test_admin",
  "password": "Admin123!"
}
```

```json
// Option 2: Login with email
{
  "username": "test_admin@solarprojects.com",
  "password": "Admin123!"
}
```

### cURL Examples

```bash
# Login with username
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'

# Login with email
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin@solarprojects.com", 
    "password": "Admin123!"
  }'
```

### Success Response (200)

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "userId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
      "username": "test_admin",
      "email": "test_admin@solarprojects.com",
      "fullName": "Test Administrator",
      "roleName": "Admin",
      "isActive": true
    }
  },
  "errors": []
}
```

### Error Response (401)

```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": ["Authentication failed"]
}
```

## Register New User

**POST** `/api/v1/users`

**Authentication Required**: Admin role only

Create a new user account. Only administrators can create new users through the API.

### Request Headers

```http
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

### Request Body

```json
{
  "username": "john_tech",
  "email": "john@solartech.com",
  "password": "SecurePass123!",
  "fullName": "John Technician",
  "roleId": 3
}
```

### cURL Example

```bash
# Step 1: Login to get admin token
TOKEN=$(curl -s -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' \
  | jq -r '.data.token')

# Step 2: Create new user
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "username": "john_tech",
    "email": "john@solartech.com",
    "password": "SecurePass123!",
    "fullName": "John Technician",
    "roleId": 3
  }'
```

### Automated Script

Use the provided script for easier user creation:

```bash
# Create admin user
./scripts/create_admin_user.sh

# Create with custom details
./scripts/create_admin_user.sh "custom_admin" "admin@company.com" "Custom Administrator"
```

### Request Body

```json
{
  "username": "john_tech",
  "email": "john@solartech.com",
  "password": "SecurePass123!",
  "fullName": "John Technician",
  "roleId": 3
}
```

### Success Response (201)

```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userId": "456e7890-e89b-12d3-a456-426614174001",
    "username": "john_tech",
    "email": "john@solartech.com",
    "fullName": "John Technician",
    "roleName": "User",
    "isActive": true
  },
  "errors": []
}
```

### Error Response (400)

```json
{
  "success": false,
  "message": "Failed to create user",
  "data": null,
  "errors": [
    "Username already exists",
    "Password must contain at least one uppercase letter",
    "Email format is invalid"
  ]
}
```

### Error Response (403)

```json
{
  "success": false,
  "message": "Access denied",
  "data": null,
  "errors": ["Admin role required to create users"]
}
```

## Password Requirements

All passwords must meet the following criteria:
- **Minimum 8 characters**
- **At least one uppercase letter** (A-Z)
- **At least one lowercase letter** (a-z)
- **At least one digit** (0-9)
- **At least one special character** (@$!%*?&)

Example valid passwords: `Admin123!`, `SecurePass123!`, `MyPassword2024@`

## 🔄 Token Refresh

**POST** `/api/v1/auth/refresh`

Refresh an expired JWT token using a refresh token.

### Request Body

```json
{
  "refreshToken": "your_refresh_token_here"
}
```

### cURL Example

```bash
curl -X POST "http://localhost:5001/api/v1/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token_here"
  }'
```

### Success Response (200)

```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "token": "new_jwt_token_here",
    "refreshToken": "new_refresh_token_here"
  },
  "errors": []
}
```

## 🚪 Logout

**POST** `/api/v1/auth/logout`

**Headers Required**:
```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Invalidate the current session and tokens.

### cURL Example

```bash
curl -X POST "http://localhost:5001/api/v1/auth/logout" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Success Response (200)

```json
{
  "success": true,
  "message": "Logout successful",
  "data": null,
  "errors": []
}
```

## 🔒 Security Best Practices

### 1. Token Storage
- **Mobile Apps**: Use secure storage (Keychain/Keystore)
- **Web Apps**: Use httpOnly cookies or secure localStorage
- **Never** store tokens in plain text

### 2. Token Handling
- Always include tokens in Authorization header: `Bearer YOUR_TOKEN`
- Check token expiry before making requests
- Implement automatic token refresh when possible
- Clear tokens on logout

### 3. Password Security
- Enforce strong password requirements
- Use BCrypt hashing with high cost factor (11+)
- Implement account lockout after failed attempts
- Regular password rotation policies

### 4. API Security
```bash
# Always use HTTPS in production
curl -X POST "https://api.solarprojects.com/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "SecurePass123!"}'

# Include proper headers for all authenticated requests
curl -X GET "https://api.solarprojects.com/api/v1/projects" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

## ❌ Common Authentication Errors

| Error Code | Status | Description | Solution |
|------------|--------|-------------|----------|
| AUTH001 | 401 | Invalid credentials | Check username/email and password |
| AUTH002 | 401 | Token expired | Use refresh token or login again |
| AUTH003 | 401 | Invalid token format | Ensure Bearer token format |
| AUTH004 | 403 | Insufficient permissions | Check user role requirements |
| AUTH005 | 400 | Weak password | Follow password requirements |
| AUTH006 | 409 | Username/email exists | Choose different username or email |
| AUTH007 | 429 | Too many login attempts | Wait before retrying |
| AUTH008 | 400 | Invalid refresh token | Login again to get new tokens |

## 🧪 Testing Authentication

### Complete Authentication Flow

```bash
#!/bin/bash

# 1. Test login
echo "🔐 Testing login..."
LOGIN_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }')

echo "Login Response: $LOGIN_RESPONSE"

# 2. Extract token
TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token')
echo "🎫 Token: ${TOKEN:0:50}..."

# 3. Test authenticated endpoint
echo "📋 Testing protected endpoint..."
curl -X GET "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"

# 4. Test user creation (admin only)
echo "👤 Testing user creation..."
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_user_001",
    "email": "testuser001@solarprojects.com",
    "password": "TestUser123!",
    "fullName": "Test User 001",
    "roleId": 3
  }'
```

### Quick Authentication Tests

```bash
# Test 1: Valid login
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}'

# Test 2: Invalid credentials
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "WrongPassword"}'

# Test 3: Missing token
curl -X GET "http://localhost:5001/api/v1/projects"

# Test 4: Invalid token format
curl -X GET "http://localhost:5001/api/v1/projects" \
  -H "Authorization: InvalidToken"
```

## 📚 Additional Resources

- **[User Management Guide](../USER_MANAGEMENT_GUIDE.md)** - Complete user management documentation
- **[Curl Commands Reference](../CURL_COMMANDS.md)** - Quick API reference
- **[Admin User Creation Script](../../scripts/create_admin_user.sh)** - Automated user creation

## Advanced Security Features

### Multi-Factor Authentication
**Note**: Not yet implemented. Planned for future releases.

```http
POST /api/v1/auth/enable-mfa
POST /api/v1/auth/verify-mfa
```

### Password Policy
```http
GET /api/v1/auth/password-policy
```

### Session Management
```http
GET /api/v1/auth/active-sessions
DELETE /api/v1/auth/sessions/{sessionId}
```

### Account Recovery
**Note**: Not yet implemented. Use admin user creation for password resets.

```http
POST /api/v1/auth/forgot-password
POST /api/v1/auth/reset-password
```

---
*Last Updated: July 2025 - Solar Projects API v1.0*
