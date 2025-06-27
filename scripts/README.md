# 🧪 API Testing Scripts - Usage Guide

## 📁 Available Scripts

### 1. `setup-test-users.sh` - Test User Setup
**Purpose:** Creates test users for all roles (Admin, Manager, User, Viewer)

**Usage:**
```bash
./scripts/setup-test-users.sh
```

**What it does:**
- Creates test users via registration endpoint
- Tests login for each created user
- Provides credentials for testing

**Required:** Run this once before running the comprehensive tests.

### 2. `test-api-endpoints.sh` - Comprehensive API Testing
**Purpose:** Tests all API endpoints with role-based permissions

**Usage:**
```bash
./scripts/test-api-endpoints.sh
```

**Features:**
- ✅ Tests 39 different endpoint scenarios
- ✅ Validates role-based access control
- ✅ Provides detailed colored output
- ✅ Generates comprehensive logs
- ✅ Shows success/failure statistics

## 🔧 Prerequisites

1. **API Running:** Ensure the .NET API is running on `http://localhost:5001`
2. **Test Users:** Run `setup-test-users.sh` first to create test accounts
3. **Network Access:** Ensure curl is available and can access localhost

## 📊 Test Results

The comprehensive test script checks:

- **Health Endpoints** - System status and database connectivity
- **Authentication** - Login functionality for all roles
- **User Management** - CRUD operations with proper authorization
- **Project Management** - Project access and creation permissions
- **Task Management** - Task access across different roles
- **Daily Reports** - Report access and role restrictions
- **Work Requests** - Request management and permissions
- **Master Plans** - Master plan access control
- **Calendar Events** - Calendar functionality and permissions

## 📝 Log Files

Test logs are automatically created in:
```
./scripts/test-logs/api_test_YYYYMMDD_HHMMSS.log
```

## 🎯 Test Users Created

| Role | Username | Password | Email |
|------|----------|----------|--------|
| **Admin** | testadmin | Admin123! | test@admin.com |
| **Manager** | manager1 | Admin123! | manager1@solarprojects.com |
| **User** | tech1 | Admin123! | tech1@solarprojects.com |
| **Viewer** | viewer | Admin123! | viewer@solarprojects.com |

## 🔍 Interpreting Results

### ✅ Success Indicators
- **Green ✅ PASS** - Test passed as expected
- **Proper HTTP Status Codes** - 200 for success, 403 for denied access

### ❌ Failure Indicators
- **Red ❌ FAIL** - Test failed unexpectedly
- **Wrong HTTP Status Codes** - 400/500 instead of 200/403

### ⏭️ Skipped Tests
- **Yellow ⏭️ SKIP** - Test skipped due to missing prerequisites

## 🚀 Quick Start

```bash
# 1. Ensure API is running
dotnet run --urls "http://localhost:5001"

# 2. Set up test users (in another terminal)
cd /path/to/dotnet-rest-api
./scripts/setup-test-users.sh

# 3. Run comprehensive tests
./scripts/test-api-endpoints.sh

# 4. Check results in the terminal and log files
```

## 🛠️ Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure API is running on port 5001
   - Check firewall settings

2. **Authentication Failures**
   - Run `setup-test-users.sh` first
   - Verify test users were created successfully

3. **Permission Denied**
   - Make scripts executable: `chmod +x scripts/*.sh`

4. **Unexpected 400 Errors**
   - Check API logs for detailed error messages
   - Verify endpoint parameter requirements

### Debug Mode

To see raw HTTP responses, modify the `make_request` function in the test script to include `-v` flag for verbose curl output.

---

*These scripts provide comprehensive testing for the Solar Projects .NET REST API with role-based access control validation.*
