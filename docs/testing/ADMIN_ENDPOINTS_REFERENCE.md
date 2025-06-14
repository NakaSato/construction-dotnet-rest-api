# 🔐 Admin API Endpoints Reference

## 📖 Complete List of Admin-Accessible Endpoints

This document lists ALL API endpoints available to **Admin** users, organized by category with permissions and descriptions.

### 🎯 Admin Privileges
- **Full System Access**: Create, read, update, delete all resources
- **User Management**: Create and manage user accounts
- **System Administration**: Access to all administrative functions
- **Override Permissions**: Can perform actions beyond normal role restrictions

---

## 📋 Endpoint Categories

### 1. 🔐 Authentication & Authorization
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/v1/auth/login` | Login with username/email | ❌ No |
| `POST` | `/api/v1/auth/register` | Create new user account | ✅ Yes (Admin only) |
| `POST` | `/api/v1/auth/refresh` | Refresh JWT token | ✅ Yes |

### 2. ❤️ Health & System Status
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/health` | Basic health check | ❌ No |
| `GET` | `/health/detailed` | Detailed system health | ❌ No |

### 3. 👥 User Management (Admin Only)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/v1/users` | Get all users (if implemented) | ✅ Yes (Admin) |
| `GET` | `/api/v1/users/{id}` | Get user by ID (if implemented) | ✅ Yes (Admin) |
| `PUT` | `/api/v1/users/{id}` | Update user details (if implemented) | ✅ Yes (Admin) |
| `DELETE` | `/api/v1/users/{id}` | Delete user (if implemented) | ✅ Yes (Admin) |

### 4. 📋 Project Management
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `GET` | `/api/v1/projects` | Get all projects | ✅ Yes | Full access |
| `GET` | `/api/v1/projects/{id}` | Get project by ID | ✅ Yes | Full access |
| `POST` | `/api/v1/projects` | Create new project | ✅ Yes | ✅ Yes |
| `PUT` | `/api/v1/projects/{id}` | Update project | ✅ Yes | ✅ Yes |
| `DELETE` | `/api/v1/projects/{id}` | Delete project | ✅ Yes | ✅ Yes |

**Query Parameters for GET /projects:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status ("Active", "Completed", "OnHold", "Cancelled")
- `search` (string): Search in project name or description

### 5. ✅ Task Management
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `GET` | `/api/v1/tasks` | Get all tasks | ✅ Yes | Full access |
| `GET` | `/api/v1/tasks/{id}` | Get task by ID | ✅ Yes | Full access |
| `POST` | `/api/v1/tasks` | Create new task | ✅ Yes | ✅ Yes |
| `PUT` | `/api/v1/tasks/{id}` | Update task | ✅ Yes | ✅ Yes |
| `DELETE` | `/api/v1/tasks/{id}` | Delete task | ✅ Yes | ✅ Yes |

**Query Parameters for GET /tasks:**
- `projectId` (Guid): Filter tasks by project
- `assignedToUserId` (Guid): Filter tasks by assigned user
- `status` (string): Filter by status ("Pending", "InProgress", "Completed", "Cancelled")
- `dueDate` (DateTime): Filter by due date
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

### 6. 📊 Daily Reports
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `GET` | `/api/v1/daily-reports` | Get all daily reports | ✅ Yes | Full access |
| `GET` | `/api/v1/daily-reports/{id}` | Get report by ID | ✅ Yes | Full access |
| `POST` | `/api/v1/daily-reports` | Create daily report | ✅ Yes | ✅ Yes |
| `PUT` | `/api/v1/daily-reports/{id}` | Update daily report | ✅ Yes | ✅ Yes |
| `DELETE` | `/api/v1/daily-reports/{id}` | Delete daily report | ✅ Yes | ✅ Yes |

**Query Parameters for GET /daily-reports:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `projectId` (Guid): Filter by specific project
- `userId` (Guid): Filter by user who created the report
- `startDate` (DateTime): Filter reports from this date
- `endDate` (DateTime): Filter reports until this date
- `includeImages` (bool): Include image metadata (default: false)

### 7. 🔧 Work Requests
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `GET` | `/api/v1/work-requests` | Get all work requests | ✅ Yes | Full access |
| `GET` | `/api/v1/work-requests/{id}` | Get work request by ID | ✅ Yes | Full access |
| `POST` | `/api/v1/work-requests` | Create work request | ✅ Yes | ✅ Yes |
| `PUT` | `/api/v1/work-requests/{id}` | Update work request | ✅ Yes | ✅ Yes |
| `DELETE` | `/api/v1/work-requests/{id}` | Delete work request | ✅ Yes | ✅ Yes |

**Query Parameters for GET /work-requests:**
- `projectId` (Guid): Filter by project
- `status` (string): Filter by status ("Pending", "Approved", "Rejected", "InProgress", "Completed")
- `requestType` (string): Filter by type ("ChangeOrder", "AdditionalWork", "Emergency", "Other")
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

### 8. 📅 Calendar Events
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `GET` | `/api/v1/calendar` | Get all calendar events | ✅ Yes | Full access |
| `GET` | `/api/v1/calendar/{id}` | Get event by ID | ✅ Yes | Full access |
| `POST` | `/api/v1/calendar` | Create calendar event | ✅ Yes | ✅ Yes |
| `PUT` | `/api/v1/calendar/{id}` | Update calendar event | ✅ Yes | ✅ Yes |
| `DELETE` | `/api/v1/calendar/{id}` | Delete calendar event | ✅ Yes | ✅ Yes |
| `GET` | `/api/v1/calendar/upcoming` | Get upcoming events | ✅ Yes | Full access |
| `GET` | `/api/v1/calendar/project/{projectId}` | Get events by project | ✅ Yes | Full access |
| `GET` | `/api/v1/calendar/task/{taskId}` | Get events by task | ✅ Yes | Full access |
| `GET` | `/api/v1/calendar/user/{userId}` | Get events by user | ✅ Yes | Full access |
| `POST` | `/api/v1/calendar/conflicts` | Check event conflicts | ✅ Yes | ✅ Yes |

**Query Parameters for GET /calendar:**
- `startDate` (DateTime): Filter events starting from this date
- `endDate` (DateTime): Filter events ending before this date
- `eventType` (EventType): Filter by event type (Meeting, Deadline, Installation, Maintenance, Training, Other)
- `status` (EventStatus): Filter by event status (Scheduled, InProgress, Completed, Cancelled, Postponed)
- `priority` (EventPriority): Filter by event priority (Low, Medium, High, Critical)
- `isAllDay` (bool): Filter all-day events
- `isRecurring` (bool): Filter recurring events
- `projectId` (Guid): Filter events for specific project
- `taskId` (Guid): Filter events for specific task
- `createdByUserId` (Guid): Filter events created by user
- `assignedToUserId` (Guid): Filter events assigned to user
- `pageNumber` (int): Page number for pagination (default: 1)
- `pageSize` (int): Number of items per page (default: 10, max: 100)

### 9. 🖼️ Image Management
| Method | Endpoint | Description | Auth Required | Admin Access |
|--------|----------|-------------|---------------|--------------|
| `POST` | `/api/v1/images/upload` | Upload image file | ✅ Yes | ✅ Yes |
| `GET` | `/api/v1/images/{id}` | Get image metadata | ✅ Yes | Full access |
| `GET` | `/api/v1/images/{id}/download` | Download image file | ✅ Yes | Full access |
| `DELETE` | `/api/v1/images/{id}` | Delete image | ✅ Yes | ✅ Yes |

**Upload Parameters:**
- `file` (File): Image file to upload (multipart/form-data)
- `category` (string): Image category (optional)
- `description` (string): Image description (optional)

---

## 🧪 Quick Testing Commands

### Authentication
```bash
# Login as Admin
curl -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'

# Extract token for use in other requests
TOKEN=$(curl -s -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' | \
  jq -r '.data.token')

echo "Bearer $TOKEN"
```

### Sample Admin Operations
```bash
# Get all projects
curl -X GET http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer $TOKEN"

# Create a new project
curl -X POST http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Admin Test Project",
    "description": "Test project created by admin",
    "startDate": "2025-06-15",
    "endDate": "2025-08-30",
    "location": "Test Location",
    "budget": 100000.00
  }'

# Create a new user (Admin only)
curl -X POST http://localhost:5002/api/v1/auth/register \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "new_user",
    "email": "new_user@example.com",
    "password": "NewUser123!",
    "fullName": "New Test User",
    "roleId": 3
  }'
```

---

## 🚀 Automated Testing

Use the comprehensive admin testing script:

```bash
# Run complete admin endpoint testing
./test-admin-endpoints.sh

# The script will:
# 1. Test all health endpoints
# 2. Login as admin and get JWT token
# 3. Test all CRUD operations for each entity
# 4. Test advanced filtering and pagination
# 5. Test admin-only operations (user creation, deletions)
# 6. Generate detailed test results log
# 7. Clean up test data
```

**Script Features:**
- ✅ Tests all 50+ admin-accessible endpoints
- ✅ Comprehensive CRUD operations testing
- ✅ Error handling and status code validation
- ✅ Detailed logging with timestamps
- ✅ Automatic cleanup of test data
- ✅ Statistical summary of test results

---

## 📊 Admin Permission Matrix

| Entity | Create | Read | Update | Delete | Special Permissions |
|--------|--------|------|--------|--------|--------------------|
| **Users** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | User management |
| **Projects** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | All projects access |
| **Tasks** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | All tasks access |
| **Daily Reports** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | All reports access |
| **Work Requests** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | All requests access |
| **Calendar Events** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | All events access |
| **Images** | ✅ Yes | ✅ Yes | ❌ No | ✅ Yes | All images access |

**Admin-Only Operations:**
- Create new user accounts
- Delete any project (regardless of ownership)
- Delete any task (regardless of assignment)
- Delete any daily report (regardless of creator)
- Access system health and diagnostics
- Override role-based restrictions

---

## 🔍 Error Handling for Admins

Admins have the highest level of access, but may still encounter errors:

### Common Admin Error Scenarios
1. **400 Bad Request**: Invalid data format or missing required fields
2. **409 Conflict**: Attempting to create duplicate resources
3. **422 Unprocessable Entity**: Data validation failures
4. **500 Internal Server Error**: Server-side issues

### Rate Limiting
- **Rate Limit**: 50 requests per minute (same as other users)
- **Headers**: `X-RateLimit-*` headers provide current status
- **Bypass**: Admins do not get rate limit exemptions

---

## 📝 Notes

1. **Authentication Required**: All endpoints except health checks require valid JWT token
2. **Role Verification**: Each request validates admin role permissions
3. **Audit Logging**: Admin actions are logged for security and compliance
4. **Data Validation**: All input data undergoes validation regardless of admin privileges
5. **Foreign Key Constraints**: Admin deletions must respect database relationships

---

## 🎯 Testing Checklist

- [ ] ✅ Health endpoints (no auth)
- [ ] 🔐 Authentication and token refresh
- [ ] 👥 User management (create, read, update, delete)
- [ ] 📋 Project CRUD operations
- [ ] ✅ Task CRUD operations
- [ ] 📊 Daily Report CRUD operations
- [ ] 🔧 Work Request CRUD operations
- [ ] 📅 Calendar Event CRUD operations
- [ ] 🖼️ Image upload and management
- [ ] 🔍 Advanced filtering and search
- [ ] 📄 Pagination testing
- [ ] ⚠️ Error handling validation
- [ ] 🧹 Cleanup operations (deletions)

Run `./test-admin-endpoints.sh` to automatically verify all these endpoints!
