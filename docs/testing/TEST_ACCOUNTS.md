# 🔐 Test Accounts - Role-Based Permission Testing

## ✅ Created Successfully

Four dedicated test accounts have been created for testing role-based permissions in the Solar Projects API:

## 📋 Test Account Credentials

### 👨‍💼 ADMIN Account
- **Username:** `test_admin`
- **Password:** `Admin123!`
- **Email:** `test_admin@example.com`
- **Role ID:** 1
- **Permissions:** Full system access, user management, all CRUD operations

### 👩‍💼 MANAGER Account  
- **Username:** `test_manager`
- **Password:** `Manager123!`
- **Email:** `test_manager@example.com`
- **Role ID:** 2
- **Permissions:** Project management, team oversight, reporting capabilities

### 👨‍🔧 USER Account
- **Username:** `test_user`
- **Password:** `User123!`
- **Email:** `test_user@example.com`
- **Role ID:** 3
- **Permissions:** Field operations, task management, daily reports, work requests

### 👁️ VIEWER Account
- **Username:** `test_viewer`
- **Password:** `Viewer123!`
- **Email:** `test_viewer@example.com`
- **Role ID:** 4
- **Permissions:** Read-only access to view projects, reports, and data

## 🧪 How to Test Permissions

### 1. Login and Get JWT Token
```bash
# Example: Login as Admin
curl -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

### 2. Use Token in API Requests
```bash
# Replace YOUR_JWT_TOKEN with actual token from login response
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  http://localhost:5002/api/v1/projects
```

### 3. Test Different Operations by Role

#### Admin Testing (test_admin)
```bash
# Should have access to ALL endpoints
curl -H "Authorization: Bearer $ADMIN_TOKEN" http://localhost:5002/api/v1/projects
curl -H "Authorization: Bearer $ADMIN_TOKEN" http://localhost:5002/api/v1/users
curl -X POST -H "Authorization: Bearer $ADMIN_TOKEN" http://localhost:5002/api/v1/projects -d '{...}'
```

#### Manager Testing (test_manager)
```bash
# Should have access to project management endpoints
curl -H "Authorization: Bearer $MANAGER_TOKEN" http://localhost:5002/api/v1/projects
curl -H "Authorization: Bearer $MANAGER_TOKEN" http://localhost:5002/api/v1/tasks
curl -X POST -H "Authorization: Bearer $MANAGER_TOKEN" http://localhost:5002/api/v1/projects -d '{...}'
```

#### User Testing (test_user)
```bash
# Should have access to operational endpoints
curl -H "Authorization: Bearer $USER_TOKEN" http://localhost:5002/api/v1/projects
curl -H "Authorization: Bearer $USER_TOKEN" http://localhost:5002/api/v1/tasks
curl -H "Authorization: Bearer $USER_TOKEN" http://localhost:5002/api/v1/daily-reports
```

#### Viewer Testing (test_viewer)
```bash
# Should only have READ access (GET requests)
curl -H "Authorization: Bearer $VIEWER_TOKEN" http://localhost:5002/api/v1/projects
curl -H "Authorization: Bearer $VIEWER_TOKEN" http://localhost:5002/api/v1/tasks

# These should FAIL (403 Forbidden or 401 Unauthorized)
curl -X POST -H "Authorization: Bearer $VIEWER_TOKEN" http://localhost:5002/api/v1/projects -d '{...}'
curl -X DELETE -H "Authorization: Bearer $VIEWER_TOKEN" http://localhost:5002/api/v1/projects/123
```

## 🛠️ Available Testing Scripts

### 1. Permission Testing Script
```bash
./test-permissions.sh
```
Automatically tests all roles against various endpoints to verify permission levels.

### 2. Login Demonstration
```bash
./demo-login.sh
```
Shows how to login with each test account and get JWT tokens.

### 3. Create Additional Test Accounts
```bash
./create-test-accounts.sh
```
Re-run this script to create the test accounts again if needed.

## 📊 Expected Permission Matrix

| Endpoint | Admin | Manager | User | Viewer |
|----------|-------|---------|------|--------|
| GET /api/v1/projects | ✅ | ✅ | ✅ | ✅ |
| POST /api/v1/projects | ✅ | ✅ | ❌ | ❌ |
| PUT /api/v1/projects/{id} | ✅ | ✅ | ❌ | ❌ |
| DELETE /api/v1/projects/{id} | ✅ | ❌ | ❌ | ❌ |
| GET /api/v1/users | ✅ | ✅ | ❌ | ❌ |
| GET /api/v1/tasks | ✅ | ✅ | ✅ | ✅ |
| POST /api/v1/tasks | ✅ | ✅ | ✅ | ❌ |
| GET /api/v1/daily-reports | ✅ | ✅ | ✅ | ✅ |
| POST /api/v1/daily-reports | ✅ | ✅ | ✅ | ❌ |
| GET /api/v1/work-requests | ✅ | ✅ | ✅ | ✅ |

*Note: Actual permissions may vary based on current implementation. Use the testing scripts to verify current behavior.*

## 🔍 Database Verification

Check that accounts exist in database:
```bash
docker exec -it solar-projects-db psql -U postgres -d SolarProjectsDb -c "
SELECT u.\"Username\", u.\"Email\", r.\"RoleName\", u.\"IsActive\" 
FROM \"Users\" u 
INNER JOIN \"Roles\" r ON u.\"RoleId\" = r.\"RoleId\" 
WHERE u.\"Username\" IN ('test_admin', 'test_manager', 'test_user', 'test_viewer') 
ORDER BY r.\"RoleId\";
"
```

## 🚀 Quick Start Testing

1. **Start the API** (if not running):
   ```bash
   docker-compose up -d
   ```

2. **Login as Admin**:
   ```bash
   curl -X POST http://localhost:5002/api/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"test_admin","password":"Admin123!"}'
   ```

3. **Copy the JWT token from response**

4. **Test protected endpoint**:
   ```bash
   curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     http://localhost:5002/api/v1/projects
   ```

5. **Repeat for other roles** to compare permissions

## 💡 Tips for Testing

- **Save tokens in variables** for easier testing:
  ```bash
  ADMIN_TOKEN="your_admin_jwt_token_here"
  curl -H "Authorization: Bearer $ADMIN_TOKEN" http://localhost:5002/api/v1/projects
  ```

- **Use tools like Postman** for GUI-based testing with token management

- **Check response codes**:
  - `200/201`: Success
  - `401`: Unauthorized (no/invalid token)
  - `403`: Forbidden (valid token, insufficient permissions)
  - `429`: Rate limited (too many requests)

- **Test edge cases**:
  - Expired tokens
  - Invalid tokens
  - Missing Authorization header
  - Wrong HTTP methods

## 🔒 Security Notes

- These are **test accounts** for development/testing only
- Use strong, unique passwords in production
- Regularly rotate credentials
- Monitor for unauthorized access attempts
- Implement proper session management in production

---

**All test accounts are ready for immediate use!** 🎉
