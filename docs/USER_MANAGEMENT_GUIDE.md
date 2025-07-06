# 👤 User Management Guide

**📅 Last Updated**: January 15, 2025  
**🔐 Authentication**: JWT-based with role-based access control  

## 🚀 Default System Users

The Solar Project Management system comes with pre-configured users for testing and administration:

### 1. System Administrator
- **Username**: `admin`
- **Password**: `Admin123!`
- **Email**: `admin@solarprojects.com`
- **Role**: Admin
- **Purpose**: Primary system administrator account

### 2. Test Administrator
- **Username**: `test_admin`
- **Password**: `Admin123!`
- **Email**: `test_admin@solarprojects.com`
- **Role**: Admin
- **Purpose**: Testing and development admin account

## 🔑 Role Hierarchy

| Role ID | Role Name | Permissions | Description |
|---------|-----------|-------------|-------------|
| 1 | **Admin** | Full system access | Complete CRUD operations, user management, system configuration |
| 2 | **Manager** | Project management | Create/edit projects, manage teams, approve reports |
| 3 | **User** | Limited operations | View projects, submit reports, update assigned tasks |
| 4 | **Viewer** | Read-only access | View-only access to assigned projects and reports |

## 🔐 Authentication Endpoints

### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "test_admin",
  "password": "Admin123!"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "expiresAt": "2025-01-16T14:30:00Z",
    "user": {
      "userId": "11111111-1111-1111-1111-111111111111",
      "username": "test_admin",
      "email": "test_admin@solarprojects.com",
      "fullName": "Test Administrator",
      "roleName": "Admin",
      "isActive": true
    }
  }
}
```

## 📊 Admin Capabilities

With the `test_admin` account, you have full access to:

### Project Management
- ✅ Create, update, delete projects
- ✅ Assign project managers and teams
- ✅ Modify project status and timelines
- ✅ Access all project analytics and reports

### User Management
- ✅ Create new users and assign roles
- ✅ Modify user permissions and status
- ✅ View user activity and session logs
- ✅ Reset user passwords

### System Administration
- ✅ View system logs and performance metrics
- ✅ Manage system-wide notifications
- ✅ Configure application settings
- ✅ Access database administration tools

### Real-Time Features
- ✅ Monitor SignalR connections
- ✅ Broadcast system-wide announcements
- ✅ View real-time activity logs
- ✅ Manage notification groups

## 🛠️ Quick Setup Commands

### Option 1: Database Script
Run the provided SQL script to add the test admin user:

```bash
# Connect to your PostgreSQL database and run:
psql -h localhost -U your_username -d your_database -f scripts/create_test_admin_user.sql
```

### Option 2: Direct SQL Command
```sql
INSERT INTO "Users" (
    "UserId", 
    "Username", 
    "Email", 
    "PasswordHash", 
    "FullName", 
    "RoleId", 
    "IsActive", 
    "CreatedAt"
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    'test_admin',
    'test_admin@solarprojects.com',
    '$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu',
    'Test Administrator',
    1,
    TRUE,
    NOW()
);
```

### Option 3: API Endpoint (if user creation is enabled)
```http
POST /api/v1/users
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json

{
  "username": "test_admin",
  "email": "test_admin@solarprojects.com",
  "password": "Admin123!",
  "fullName": "Test Administrator",
  "roleId": 1,
  "isActive": true
}
```

## 🌐 API-Based User Creation

### Create Admin User via API

To create a new admin user via the REST API, you need to authenticate first with an existing admin account, then use the users endpoint.

#### Step 1: Authenticate to Get JWT Token
```bash
# Login with the test admin user to get a JWT token
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

This will return a response like:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": "...",
      "username": "test_admin",
      "role": "Admin"
    }
  }
}
```

#### Step 2: Create New Admin User
```bash
# Use the JWT token from step 1 to create a new admin user
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "new_admin",
    "email": "new_admin@solarprojects.com",
    "password": "SecurePassword123!",
    "fullName": "New Administrator",
    "roleId": 1
  }'
```

#### Role IDs Reference
- **1**: Admin (Full system access)
- **2**: Manager (Project management access)
- **3**: User (Standard user access)
- **4**: Viewer (Read-only access)

#### Complete Example Script
```bash
#!/bin/bash

# 1. Login and extract token
LOGIN_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }')

# Extract token using jq (install with: brew install jq)
TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "❌ Failed to authenticate. Response: $LOGIN_RESPONSE"
  exit 1
fi

echo "✅ Successfully authenticated. Token: ${TOKEN:0:20}..."

# 2. Create new admin user
CREATE_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "username": "production_admin",
    "email": "production_admin@solarprojects.com", 
    "password": "ProductionAdmin123!",
    "fullName": "Production Administrator",
    "roleId": 1
  }')

echo "📝 Create user response: $CREATE_RESPONSE"

# Check if creation was successful
SUCCESS=$(echo $CREATE_RESPONSE | jq -r '.success')
if [ "$SUCCESS" = "true" ]; then
  echo "✅ Successfully created production admin user!"
  echo "   Username: production_admin"
  echo "   Email: production_admin@solarprojects.com"
  echo "   Password: ProductionAdmin123!"
else
  echo "❌ Failed to create user. Check response above for details."
fi
```

#### Alternative Without jq
If you don't have `jq` installed, you can do this manually:

1. **Get Token**: Run the login curl command and copy the token from the response
2. **Create User**: Replace `YOUR_JWT_TOKEN_HERE` with the actual token

```bash
# Manual approach without jq
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "username": "manual_admin",
    "email": "manual_admin@solarprojects.com",
    "password": "ManualAdmin123!",
    "fullName": "Manual Administrator", 
    "roleId": 1
  }'
```

### Create Other User Types

#### Manager User
```bash
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "project_manager",
    "email": "manager@solarprojects.com",
    "password": "ManagerPass123!",
    "fullName": "Project Manager",
    "roleId": 2
  }'
```

#### Standard User
```bash
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "field_worker",
    "email": "worker@solarprojects.com", 
    "password": "WorkerPass123!",
    "fullName": "Field Worker",
    "roleId": 3
  }'
```

## 🔒 Password Security

The password `Admin123!` is hashed using BCrypt with the following characteristics:
- **Algorithm**: BCrypt
- **Cost Factor**: 11 (2^11 = 2048 rounds)
- **Salt**: Automatically generated per password
- **Hash**: `$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu`

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter  
- At least one number
- At least one special character

## 🎯 Real-Time Features Testing

With the test admin account, you can test all SignalR real-time features:

### 1. Connect to SignalR Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => "your_jwt_token_here"
    })
    .build();

await connection.start();
console.log("Connected as test_admin");
```

### 2. Join Admin Groups
```javascript
// Join admin-specific groups
await connection.invoke("JoinRoleGroup", "admin");
await connection.invoke("JoinMapViewersGroup");

// Listen for admin notifications
connection.on("AdminNotification", (data) => {
    console.log("Admin notification:", data);
});
```

## 📈 User Analytics Dashboard

Access comprehensive user analytics with admin privileges:

```http
GET /api/v1/admin/users/analytics
Authorization: Bearer {admin_token}
```

**Response includes**:
- User activity metrics
- Login patterns and frequency
- Role distribution analysis
- System usage statistics
- Performance metrics per user

## ⚠️ Security Best Practices

### For Production Deployment:
1. **Change Default Passwords**: Update all default user passwords
2. **Enable 2FA**: Implement two-factor authentication for admin accounts
3. **Regular Password Rotation**: Enforce password changes every 90 days
4. **Audit Logging**: Monitor all admin activities
5. **Session Management**: Implement proper session timeout and renewal

### For Development/Testing:
1. **Use Test Credentials**: Keep test credentials separate from production
2. **Limited Scope**: Test users should have minimal required permissions
3. **Regular Cleanup**: Remove test users before production deployment
4. **Environment Isolation**: Never use production credentials in test environments

## 🔄 Password Reset (Admin)

If you need to reset the test admin password:

```sql
-- Reset to "NewAdmin123!"
UPDATE "Users" 
SET "PasswordHash" = '$2a$11$different_hash_for_new_password_here'
WHERE "Username" = 'test_admin';
```

Generate new hash using BCrypt with cost factor 11.

## 📊 Verification Queries

### Check User Exists
```sql
SELECT "Username", "Email", "FullName", "IsActive" 
FROM "Users" 
WHERE "Username" = 'test_admin';
```

### Check User Role
```sql
SELECT u."Username", r."RoleName", u."IsActive"
FROM "Users" u
JOIN "Roles" r ON u."RoleId" = r."RoleId"
WHERE u."Username" = 'test_admin';
```

### Check Recent Login Activity
```sql
SELECT "Username", "LastLoginAt", "LoginCount"
FROM "Users" 
WHERE "Username" = 'test_admin';
```

---

**🎉 Your test admin user is ready!** Use `test_admin` / `Admin123!` to access all administrative features and test the complete Solar Project Management system.
