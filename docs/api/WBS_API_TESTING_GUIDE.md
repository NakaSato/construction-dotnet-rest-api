# WBS API Testing Script - Complete Documentation

## Overview
This script comprehensively tests the Work Breakdown Structure (WBS) API for the Solar Project Management system. It validates all endpoints, authentication mechanisms, and role-based access control.

## Usage

### Basic Testing (Health Check Only)
```bash
./scripts/test-wbs-api.sh
```

### Full Testing with Authentication
```bash
./scripts/test-wbs-api.sh "http://localhost:5001" "your-jwt-token-here"
```

## Getting a JWT Token for Testing

Since the application uses an in-memory database for testing, you'll need to create a test account first:

### Method 1: Register a New Account
```bash
# 1. Register a new admin account (RoleId 1 = Admin)
curl -X POST "http://localhost:5001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testadmin",
    "email": "testadmin@example.com", 
    "password": "TestAdmin123!",
    "fullName": "Test Administrator",
    "roleId": 1
  }'

# 2. Login to get JWT token
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testadmin",
    "password": "TestAdmin123!"
  }'

# 3. Extract the token from the response and use it
./scripts/test-wbs-api.sh "http://localhost:5001" "eyJhbGciOiJIUzI1NiIs..."
```

### Method 2: Use Existing Default Admin (if available)
If the application has seeded default accounts, try:
```bash
# Try default admin credentials
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }'
```

## Test Coverage

The script tests the following WBS API functionality:

### ✅ Public Endpoints
- Health check (`/Health`)
- Basic connectivity

### 🔒 Authenticated Endpoints
- **CRUD Operations**: Create, Read, Update, Delete WBS tasks
- **Filtering**: Filter tasks by project, status, installation area
- **Hierarchy**: Get project task hierarchy
- **Progress**: Calculate project progress
- **Evidence**: Manage task evidence/attachments
- **Data Seeding**: Seed sample WBS data
- **Status Management**: Update task status
- **Error Handling**: Validate error responses

### 🛡️ Security Testing
- **Authentication**: Validates JWT token requirement
- **Authorization**: Tests role-based access control
- **Error Responses**: Validates proper HTTP status codes

## Expected Results

### Without Authentication
- ✅ **1 test passes**: Health check
- ❌ **~15 tests fail**: All authenticated endpoints return 401 Unauthorized
- ⚠️ **~10 tests skipped**: Tests requiring created data

### With Valid JWT Token
- ✅ **~25+ tests pass**: All endpoints function correctly
- ❌ **0-2 tests fail**: Only if there are actual API issues
- ℹ️ **Role-dependent**: Some operations may require specific roles

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure the API is running: `dotnet run --urls "http://localhost:5001"`
   - Check if port 5001 is available

2. **401 Unauthorized on All Endpoints**
   - This is expected without a JWT token
   - Register and login to get a token (see above)

3. **400 Bad Request on Registration**
   - Check password requirements (uppercase, lowercase, digit, special char)
   - Ensure RoleId is valid (1=Admin, 2=Manager, 3=User, 4=Viewer)

4. **Invalid Username or Password**
   - In-memory database doesn't persist between restarts
   - Register a new account if using in-memory database

### Role IDs
- **1**: Admin (full access)
- **2**: ProjectManager (project management access)  
- **3**: User (limited access)
- **4**: Viewer (read-only access)

## Script Features

- **Comprehensive Testing**: Tests all WBS API endpoints
- **Authentication Support**: Automatic token-based authentication
- **Color-coded Output**: Green=success, Red=error, Yellow=warning
- **Detailed Logging**: Shows request/response data for debugging
- **Error Validation**: Tests proper error handling
- **RBAC Testing**: Validates role-based access control
- **Statistics**: Final summary of passed/failed tests

## Sample Complete Test Run

```bash
# 1. Start the API
dotnet run --urls "http://localhost:5001"

# 2. Register test admin (in another terminal)
curl -X POST "http://localhost:5001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testadmin",
    "email": "testadmin@example.com", 
    "password": "TestAdmin123!",
    "fullName": "Test Administrator",
    "roleId": 1
  }'

# 3. Login and get token
TOKEN=$(curl -s -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "testadmin", "password": "TestAdmin123!"}' \
  | jq -r '.data.token')

# 4. Run comprehensive tests
./scripts/test-wbs-api.sh "http://localhost:5001" "$TOKEN"
```

This should result in nearly all tests passing, demonstrating a fully functional WBS API implementation.
