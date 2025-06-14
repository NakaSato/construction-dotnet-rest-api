# 👩‍💼 MANAGER ENDPOINTS REFERENCE

## 📋 Manager Role API Capabilities

This document provides a complete reference of API endpoints accessible to users with the **Manager** role in the Solar Projects API.

## 🔐 Authentication Required

All endpoints require a valid JWT token with Manager role permissions:

```bash
Authorization: Bearer <jwt_token>
```

## ✅ MANAGER ACCESSIBLE ENDPOINTS

### 🏠 System Health (Public Access)

#### GET /health
**Purpose:** Basic system health check  
**Access:** ✅ Public (No auth required)  
**Response:** System status and timestamp

```bash
curl -X GET "http://localhost:5002/health"
```

#### GET /health/detailed
**Purpose:** Detailed system health with database status  
**Access:** ✅ Public (No auth required)  
**Response:** Extended health information including memory usage

```bash
curl -X GET "http://localhost:5002/health/detailed"
```

### 👥 User Management (Limited Manager Access)

#### GET /api/v1/users
**Purpose:** View all users in the system  
**Access:** ✅ Manager can view team members  
**Capabilities:** Pagination, filtering

```bash
curl -X GET "http://localhost:5002/api/v1/users" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)
- `search` - Search by username or email
- `role` - Filter by role (User, Manager, Admin, Viewer)
- `isActive` - Filter by active status

#### POST /api/v1/auth/register
**Purpose:** Create new user accounts  
**Access:** ✅ Manager can create User role accounts  
**Limitation:** Cannot create Admin or Manager accounts

```bash
curl -X POST "http://localhost:5002/api/v1/auth/register" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "new_user",
    "email": "user@example.com",
    "password": "SecurePass123!",
    "fullName": "New Team Member",
    "role": "User"
  }'
```

### 🏗️ Project Management (Full CRUD Access)

#### GET /api/v1/projects
**Purpose:** View all projects  
**Access:** ✅ Full read access to all projects  
**Capabilities:** Advanced filtering, sorting, pagination

```bash
curl -X GET "http://localhost:5002/api/v1/projects" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `pageNumber`, `pageSize` - Pagination
- `status` - Filter by project status (Planning, InProgress, Completed, OnHold)
- `search` - Search project names and descriptions
- `sort` - Sort field (projectName, startDate, endDate, status)
- `order` - Sort order (asc, desc)
- `projectManagerId` - Filter by project manager

#### GET /api/v1/projects/{id}
**Purpose:** Get specific project details  
**Access:** ✅ Full project details access

```bash
curl -X GET "http://localhost:5002/api/v1/projects/{project_id}" \
  -H "Authorization: Bearer <manager_token>"
```

#### POST /api/v1/projects
**Purpose:** Create new projects  
**Access:** ✅ Full project creation capabilities

```bash
curl -X POST "http://localhost:5002/api/v1/projects" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "Manager Solar Installation",
    "address": "123 Solar St, Energy City, CA",
    "clientInfo": "Commercial client with 50kW installation",
    "status": "Planning",
    "startDate": "2025-07-01T08:00:00Z",
    "estimatedEndDate": "2025-09-30T17:00:00Z",
    "projectManagerId": "manager_user_id"
  }'
```

#### PUT /api/v1/projects/{id}
**Purpose:** Update existing projects  
**Access:** ✅ Full project modification rights

#### DELETE /api/v1/projects/{id}
**Purpose:** Delete projects  
**Access:** ✅ Project deletion authority

### 📋 Task Management (Full CRUD Access)

#### GET /api/v1/tasks
**Purpose:** View all tasks across projects  
**Access:** ✅ Complete task visibility

```bash
curl -X GET "http://localhost:5002/api/v1/tasks" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `projectId` - Filter by project
- `assignedToUserId` - Filter by assigned user
- `status` - Filter by status (Pending, InProgress, Completed)
- `priority` - Filter by priority (Low, Medium, High)
- `dueDate_before`, `dueDate_after` - Date range filtering

#### POST /api/v1/tasks
**Purpose:** Create new tasks  
**Access:** ✅ Task creation and assignment

```bash
curl -X POST "http://localhost:5002/api/v1/tasks" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Install Solar Panels - Section A",
    "description": "Install 20 solar panels on roof section A",
    "projectId": "project_id",
    "assignedToUserId": "team_member_id",
    "status": "Pending",
    "priority": "High",
    "estimatedHours": 16,
    "dueDate": "2025-07-15T17:00:00Z"
  }'
```

#### PUT /api/v1/tasks/{id}
**Purpose:** Update task details and status  
**Access:** ✅ Full task modification

#### DELETE /api/v1/tasks/{id}
**Purpose:** Remove tasks  
**Access:** ✅ Task deletion authority

### 📊 Daily Reports (Full Access)

#### GET /api/v1/daily-reports
**Purpose:** View team daily reports  
**Access:** ✅ All team reports visible

```bash
curl -X GET "http://localhost:5002/api/v1/daily-reports" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `userId` - Filter by team member
- `projectId` - Filter by project
- `reportDate_after`, `reportDate_before` - Date range
- `hoursWorked_min`, `hoursWorked_max` - Hours filtering

#### POST /api/v1/daily-reports
**Purpose:** Create daily reports  
**Access:** ✅ Report creation capabilities

```bash
curl -X POST "http://localhost:5002/api/v1/daily-reports" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "manager_user_id",
    "projectId": "project_id",
    "reportDate": "2025-06-15T00:00:00Z",
    "hoursWorked": 8,
    "tasksSummary": "Completed panel installation review",
    "issuesEncountered": "Minor weather delays",
    "notesForNextDay": "Begin electrical connections"
  }'
```

### 🔧 Work Requests (Full Management)

#### GET /api/v1/work-requests
**Purpose:** View all work requests  
**Access:** ✅ Complete request visibility

```bash
curl -X GET "http://localhost:5002/api/v1/work-requests" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `requestedByUserId` - Filter by requester
- `assignedToUserId` - Filter by assignee
- `status` - Filter by status (Pending, Approved, InProgress, Completed)
- `priority` - Filter by priority
- `requestType` - Filter by type (Maintenance, Installation, Inspection)

#### POST /api/v1/work-requests
**Purpose:** Create work requests  
**Access:** ✅ Request creation authority

```bash
curl -X POST "http://localhost:5002/api/v1/work-requests" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Additional Electrical Supplies",
    "description": "Need 50ft of 12AWG wire for installation",
    "requestType": "Material",
    "priority": "Medium",
    "requestedByUserId": "manager_user_id",
    "estimatedCost": 150.00,
    "justification": "Required for project completion"
  }'
```

#### PUT /api/v1/work-requests/{id}
**Purpose:** Update and approve work requests  
**Access:** ✅ Approval authority

### 📅 Calendar Events (Create & Manage)

#### GET /api/v1/calendar
**Purpose:** View calendar events  
**Access:** ✅ Full calendar visibility

```bash
curl -X GET "http://localhost:5002/api/v1/calendar" \
  -H "Authorization: Bearer <manager_token>"
```

**Query Parameters:**
- `eventType` - Filter by type (Meeting, Inspection, Deadline)
- `priority` - Filter by priority
- `startDate_after`, `startDate_before` - Date range
- `organizerId` - Filter by organizer

#### POST /api/v1/calendar
**Purpose:** Create calendar events  
**Access:** ✅ Event creation capabilities  
**Note:** Some validation issues may occur

```bash
curl -X POST "http://localhost:5002/api/v1/calendar" \
  -H "Authorization: Bearer <manager_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Project Planning Meeting",
    "description": "Weekly project status review",
    "eventType": "Meeting",
    "startDateTime": "2025-06-20T10:00:00Z",
    "endDateTime": "2025-06-20T11:00:00Z",
    "organizerId": "manager_user_id",
    "priority": "Medium",
    "location": "Conference Room A"
  }'
```

## 🚫 RESTRICTED ENDPOINTS (Manager Cannot Access)

### ❌ Admin-Only Endpoints
- `/api/v1/admin/*` - Administrative functions
- User role modifications
- System configuration endpoints
- Rate limit administration

### ❌ Limited User Management
- Cannot create Admin accounts
- Cannot modify user roles
- Cannot deactivate users
- Cannot access user sensitive data

## 📊 Response Format

All endpoints return standardized responses:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 15
  },
  "errors": [],
  "error": null
}
```

## 🔒 Security Considerations

### ✅ Manager Role Security Features
- JWT token required for all authenticated endpoints
- Role-based access control enforced
- Cannot escalate privileges
- Appropriate data visibility boundaries
- Input validation on all endpoints

### 🛡️ Best Practices
1. **Token Security:** Store JWT tokens securely
2. **HTTPS Only:** Use HTTPS in production
3. **Input Validation:** Always validate input data
4. **Error Handling:** Handle 403/401 responses appropriately
5. **Rate Limiting:** Respect API rate limits

## 📈 Manager Workflow Examples

### Project Management Workflow
```bash
# 1. View existing projects
GET /api/v1/projects

# 2. Create new project
POST /api/v1/projects

# 3. Create tasks for project
POST /api/v1/tasks

# 4. Assign team members
PUT /api/v1/tasks/{id}

# 5. Monitor progress
GET /api/v1/daily-reports?projectId={id}
```

### Team Management Workflow
```bash
# 1. View team members
GET /api/v1/users

# 2. Add new team member
POST /api/v1/auth/register

# 3. Assign tasks
POST /api/v1/tasks

# 4. Monitor work requests
GET /api/v1/work-requests

# 5. Approve requests
PUT /api/v1/work-requests/{id}
```

---

## 📝 Summary

The Manager role provides **comprehensive project management capabilities** with appropriate security boundaries. Managers can effectively lead teams, manage projects, and coordinate resources while maintaining proper access controls.

**Total Accessible Endpoints:** 25+ endpoints across all major functional areas  
**Security Level:** Appropriate for middle management  
**Capabilities:** Full project lifecycle management

*Document Version: 1.0*  
*Last Updated: 2025-06-15*  
*Based on Manager role testing results*
