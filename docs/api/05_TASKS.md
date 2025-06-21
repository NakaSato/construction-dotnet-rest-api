# ✅ Task Management

**Base URL**: `/api/v1/tasks`

**🔒 Authentication Required**  
**🎯 Status**: ✅ Available

Task management provides the ability to create, assign, and track specific work items for solar installation projects.

## ⚡ Authorization & Access Control

### Admin & Manager Roles
- ✅ Create tasks for any project
- ✅ Assign tasks to team members  
- ✅ Update task status and details
- ✅ Delete tasks when needed
- ✅ View all tasks across projects

### Supervisor Roles
- ✅ Create tasks for managed projects
- ✅ Update tasks in managed projects
- ✅ View tasks in managed projects
- ❌ Cannot delete tasks

### Task Assignees
- ✅ Update status of assigned tasks
- ✅ View assigned tasks
- ❌ Cannot reassign tasks to others
- ❌ Cannot delete tasks

### All Authenticated Users
- ✅ View tasks they have access to
- ✅ Filter and search for tasks
- ❌ Cannot modify task data unless assigned

## 📋 Get All Tasks

**GET** `/api/v1/tasks`

Get all tasks with pagination and filtering.

**Authorization**: Required (All authenticated users)

**Query Parameters**:
- `pageNumber` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page
- `projectId` (optional) - Filter by project ID
- `assigneeId` (optional) - Filter by assigned user ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "items": [
      {
        "taskId": "guid",
        "projectId": "guid",
        "projectName": "string",
        "title": "string",
        "description": "string",
        "status": "NotStarted|InProgress|Completed|OnHold|Cancelled",
        "dueDate": "datetime",
        "assignedTechnician": {
          "userId": "guid",
          "username": "string",
          "firstName": "string",
          "lastName": "string",
          "email": "string",
          "role": "string"
        },
        "completionDate": "datetime",
        "createdAt": "datetime"
      }
    ],
    "totalCount": 0,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

## 🔍 Get Task by ID

**GET** `/api/v1/tasks/{id}`

Get a specific task by ID.

**Authorization**: Required (All authenticated users)

**Path Parameters**:
- `id` (path) - Task ID

**Success Response (200)**: Same structure as task item above

## 📝 Create Task

**POST** `/api/v1/tasks`

Create a new task.

**Authorization**: Required (Admin, Manager, Supervisor roles)

**Query Parameters**:
- `projectId` (required) - Project ID to create the task for

**Request Body**:
```json
{
  "title": "string", // Required, 3-200 characters
  "description": "string", // Optional, max 2000 characters
  "dueDate": "datetime", // Optional
  "assignedTechnicianId": "guid" // Optional
}
```

**Success Response (201)**: Same structure as task item above

## 🔄 Update Task (Full Update)

**PUT** `/api/v1/tasks/{id}`

Update an existing task.

**Authorization**: Required (Admin, Manager, Supervisor roles)

**Path Parameters**:
- `id` (path) - Task ID

**Request Body**:
```json
{
  "title": "string", // Required, 3-200 characters
  "description": "string", // Optional, max 2000 characters
  "status": "NotStarted|InProgress|Completed|OnHold|Cancelled", // Required
  "dueDate": "datetime", // Optional
  "assignedTechnicianId": "guid" // Optional
}
```

**Success Response (200)**: Same structure as task item above

## 🔄 Partially Update Task

**PATCH** `/api/v1/tasks/{id}`

Partially update an existing task.

**Authorization**: Required (Admin, Manager, Supervisor roles)

**Path Parameters**:
- `id` (path) - Task ID

**Request Body** (all fields optional):
```json
{
  "title": "string",
  "description": "string", 
  "status": "NotStarted|InProgress|Completed|OnHold|Cancelled",
  "dueDate": "datetime",
  "assignedTechnicianId": "guid"
}
```

**Success Response (200)**: Same structure as task item above

## 🏷️ Update Task Status

**PATCH** `/api/v1/tasks/{id}/status`

Update only the status of a task.

**Authorization**: Required (All authenticated users for own tasks, Admin/Manager/Supervisor for all tasks)

**Path Parameters**:
- `id` (path) - Task ID

**Request Body**:
```json
"NotStarted" // or "InProgress", "Completed", "OnHold", "Cancelled"
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task status updated successfully",
  "data": true
}
```

## 🗑️ Delete Task

**DELETE** `/api/v1/tasks/{id}`

Delete a task.

**Authorization**: Required (Admin, Manager roles only)

**Path Parameters**:
- `id` (path) - Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task deleted successfully",
  "data": true
}
```

## � Advanced Task Search

**GET** `/api/v1/tasks/advanced`

Get tasks with advanced filtering, sorting, and pagination.

**Authorization**: Required (All authenticated users)

**Query Parameters**: Extended filtering and sorting options including date ranges, status filters, sorting by multiple fields.

**Success Response (200)**: Enhanced paginated response with metadata and navigation links.

## 📊 Task Progress Reports

### Get Progress Reports

**GET** `/api/v1/tasks/{taskId}/progress-reports`

Get progress reports for a specific task.

**Authorization**: Required (All authenticated users)

**Path Parameters**:
- `taskId` (path) - Task ID

**Query Parameters**:
- `pageNumber`, `pageSize` - Standard pagination

**Success Response (200)**: Paginated list of task progress reports with completion percentages, status updates, and work logs.

### Create Progress Report

**POST** `/api/v1/tasks/{taskId}/progress-reports`

Create a progress report for a task.

**Authorization**: Required (All authenticated users for assigned tasks, Admin/Manager/Supervisor for all tasks)

**Path Parameters**:
- `taskId` (path) - Task ID

**Request Body**:
```json
{
  "completionPercentage": 0.0,
  "status": "string",
  "workCompleted": "string",
  "issues": "string",
  "nextSteps": "string",
  "hoursWorked": 0.0
}
```

**Success Response (201)**: Created progress report details

## 📊 Task Status Values

Tasks support the following status values:

| Status | Description | Typical Usage |
|--------|-------------|---------------|
| **NotStarted** | Task created but no work begun | Initial state when task is created |
| **InProgress** | Work has started | Active work in progress |
| **Completed** | Task finished successfully | Work completed successfully |
| **OnHold** | Temporarily paused | Waiting for dependencies or resources |
| **Cancelled** | Task was cancelled | Task no longer needed or feasible |

## 🔧 Common Use Cases

### Creating a Task
1. Use `POST /api/v1/tasks` with `projectId` parameter
2. Provide title (required) and optional description, due date, and assignee
3. Task starts in "NotStarted" status by default

### Updating Task Progress
1. Use `PATCH /api/v1/tasks/{id}/status` for quick status updates
2. Use `PATCH /api/v1/tasks/{id}` for partial field updates
3. Use `PUT /api/v1/tasks/{id}` for complete task updates

### Task Assignment
- Tasks can be assigned to technicians using `assignedTechnicianId`
- Assigned users can update their own task status
- Managers can reassign tasks to other users

### Progress Tracking
- Use progress reports to track detailed work completion
- Include hours worked, completion percentage, and notes
- Track issues and next steps for better project management

## ❌ Common Error Responses

| Status Code | Error Message | Resolution |
|-------------|---------------|------------|
| **400** | Invalid task data | Check request body format and required fields |
| **401** | Authentication required | Provide valid authentication token |
| **403** | Insufficient permissions | Check user role and task assignment |
| **404** | Task not found | Verify task ID exists |
| **404** | Project not found | Verify project ID when creating tasks |

---
*Last Updated: June 21, 2025*
