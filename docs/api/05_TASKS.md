# ✅ Task Management

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full CRUD access), Task Assignees (can update assigned tasks), All authenticated users (view only)

Task management provides the ability to create, assign, and track specific work items for solar installation projects.

## ⚡ Task Management Capabilities

### Admin & Manager
- ✅ Create tasks for any project
- ✅ Assign tasks to team members
- ✅ Update task status and details
- ✅ Set priority and due dates
- ✅ Track progress and completion
- ✅ Delete tasks when needed

### Task Assignees
- ✅ Update status of assigned tasks
- ✅ Record actual hours worked
- ✅ Update progress percentage
- ✅ Add comments and notes
- ❌ Cannot reassign tasks to others
- ❌ Cannot delete tasks

### All Users
- ✅ View tasks they have access to
- ✅ Filter and search for tasks
- ❌ Cannot modify task data

## 📋 Get All Tasks

**GET** `/api/v1/tasks`

Retrieve a list of tasks with filtering options.

**Query Parameters**:
- `projectId` (Guid): Filter tasks by project
- `assignedToUserId` (Guid): Filter tasks by assigned user
- `status` (string): Filter by status
- `priority` (string): Filter by priority
- `dueDate` (DateTime): Filter by due date
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "tasks": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Install Solar Panels - Section A",
        "description": "Install 24 solar panels on the south-facing roof section",
        "status": "InProgress",
        "priority": "High",
        "dueDate": "2025-06-20",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
        "assignedToUserName": "John Technician",
        "estimatedHours": 16.0,
        "actualHours": 12.5,
        "progressPercentage": 75.0,
        "createdAt": "2025-06-01T10:00:00Z",
        "updatedAt": "2025-06-14T14:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 15,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 2,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

## 🔍 Get Task by ID

**GET** `/api/v1/tasks/{id}`

Retrieve detailed information about a specific task.

**Path Parameters**:
- `id` (Guid): Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Install Solar Panels - Section A",
    "description": "Install 24 solar panels on the south-facing roof section",
    "status": "InProgress",
    "priority": "High",
    "dueDate": "2025-06-20",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
    "assignedToUserName": "John Technician",
    "estimatedHours": 16.0,
    "actualHours": 12.5,
    "progressPercentage": 75.0,
    "comments": [
      {
        "id": "aaa12345-e89b-12d3-a456-426614174000",
        "text": "Weather delay on Tuesday, rescheduled for Thursday",
        "createdBy": "John Technician",
        "createdAt": "2025-06-10T09:15:00Z"
      }
    ],
    "attachments": [
      {
        "id": "bbb12345-e89b-12d3-a456-426614174000",
        "fileName": "site_layout.pdf",
        "fileSize": 1024000,
        "uploadedBy": "Sarah Manager",
        "uploadedAt": "2025-06-01T14:30:00Z"
      }
    ],
    "createdAt": "2025-06-01T10:00:00Z",
    "createdBy": "Sarah Manager",
    "updatedAt": "2025-06-14T14:30:00Z",
    "updatedBy": "John Technician"
  },
  "errors": []
}
```

## 📝 Create Task

**POST** `/api/v1/tasks`

**🔒 Required Roles**: Admin, Manager

Create a new task assigned to a team member.

**Request Body**:
```json
{
  "title": "Install Electrical Panel",
  "description": "Install and configure the main electrical panel for solar system integration",
  "priority": "High",
  "dueDate": "2025-06-25T00:00:00Z",
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
  "estimatedHours": 8.0,
  "status": "NotStarted"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Task created successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "title": "Install Electrical Panel",
    "status": "NotStarted",
    "assignedToUserName": "John Technician",
    "createdAt": "2025-06-15T11:00:00Z"
  },
  "errors": []
}
```

## 🔄 Update Task

**PUT** `/api/v1/tasks/{id}`

**🔒 Requires**: Admin, Manager, or task assignee

Update all fields of an existing task.

**Path Parameters**:
- `id` (Guid): Task ID

**Request Body**:
```json
{
  "title": "Install Electrical Panel - Updated",
  "description": "Install and configure the main electrical panel for solar system integration with additional safety checks",
  "priority": "High",
  "dueDate": "2025-06-26T00:00:00Z",
  "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
  "estimatedHours": 10.0,
  "actualHours": 5.0,
  "status": "InProgress",
  "progressPercentage": 50.0
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task updated successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "title": "Install Electrical Panel - Updated",
    "status": "InProgress",
    "progressPercentage": 50.0,
    "updatedAt": "2025-06-15T12:00:00Z"
  },
  "errors": []
}
```

## 🔄 Partially Update Task

**PATCH** `/api/v1/tasks/{id}`

**🔒 Requires**: Admin, Manager, or task assignee

Update specific fields of an existing task without affecting other fields.

**Path Parameters**:
- `id` (Guid): Task ID

**Request Body**:
```json
{
  "status": "Completed",
  "progressPercentage": 100.0,
  "actualHours": 9.5
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task updated successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "status": "Completed",
    "progressPercentage": 100.0,
    "updatedAt": "2025-06-15T15:30:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Task

**DELETE** `/api/v1/tasks/{id}`

**🔒 Required Roles**: Admin, Manager

Delete a task from the system.

**Path Parameters**:
- `id` (Guid): Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task deleted successfully",
  "data": null,
  "errors": []
}
```

## 💬 Add Comment to Task

**POST** `/api/v1/tasks/{id}/comments`

**🔒 Requires**: Admin, Manager, or task assignee

Add a comment to an existing task.

**Path Parameters**:
- `id` (Guid): Task ID

**Request Body**:
```json
{
  "text": "Completed wiring of the main panel, ready for inspection before connecting inverters"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Comment added successfully",
  "data": {
    "id": "comment-uuid-here",
    "text": "Completed wiring of the main panel, ready for inspection before connecting inverters",
    "createdAt": "2025-06-15T16:45:00Z",
    "createdBy": "John Technician"
  },
  "errors": []
}
```

## 📊 Task Status Workflow

Tasks follow a defined status workflow:

| Status | Description | Completion % | Next Status |
|--------|-------------|--------------|-------------|
| **NotStarted** | Task created but no work begun | 0% | InProgress |
| **InProgress** | Work has started | 1-99% | OnHold, Completed |
| **OnHold** | Temporarily paused | Same as before | InProgress, Cancelled |
| **Completed** | Task finished successfully | 100% | Verified |
| **Verified** | Quality check completed | 100% | (Final state) |
| **Cancelled** | Task was cancelled | N/A | (Final state) |

## 🔧 Task Priority Levels

| Priority | Description | Default SLA |
|----------|-------------|-------------|
| **Critical** | Blocking project progress, safety issues | 24 hours |
| **High** | Important task on critical path | 3 days |
| **Medium** | Standard importance task | 5 days |
| **Low** | Non-critical task | 10 days |

## ❌ Task Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **TSK001** | Task not found | Verify task ID exists |
| **TSK002** | Insufficient permissions | Check user role or assignment |
| **TSK003** | Invalid task data | Check request body for required fields |
| **TSK004** | Invalid status transition | Follow the defined workflow sequence |
| **TSK005** | Assigned user not found | Verify user ID exists |
| **TSK006** | Project not found | Verify project ID exists |

---
*Last Updated: June 15, 2025*
