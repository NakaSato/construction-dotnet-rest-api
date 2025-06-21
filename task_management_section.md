## Task Management

**Base URL**: `/api/v1/tasks`

**🔒 Authentication Required**  
**🎯 Status**: ✅ Available

### Endpoints

#### GET /api/v1/tasks
Get all tasks with pagination and filtering.

**Authorization**: Required (All authenticated users)

**Query Parameters**:
- `pageNumber` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page
- `projectId` (optional) - Filter by project ID
- `assigneeId` (optional) - Filter by assigned user ID

**Response**:
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

#### GET /api/v1/tasks/{id}
Get a specific task by ID.

**Authorization**: Required (All authenticated users)

**Parameters**:
- `id` (path) - Task ID

**Response**: Same structure as task item above

#### POST /api/v1/tasks
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

**Response**: Same structure as task item above

#### PUT /api/v1/tasks/{id}
Update an existing task.

**Authorization**: Required (Admin, Manager, Supervisor roles)

**Parameters**:
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

**Response**: Same structure as task item above

#### PATCH /api/v1/tasks/{id}
Partially update an existing task.

**Authorization**: Required (Admin, Manager, Supervisor roles)

**Parameters**:
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

**Response**: Same structure as task item above

#### PATCH /api/v1/tasks/{id}/status
Update only the status of a task.

**Authorization**: Required (All authenticated users for own tasks, Admin/Manager/Supervisor for all tasks)

**Parameters**:
- `id` (path) - Task ID

**Request Body**:
```json
"NotStarted" // or "InProgress", "Completed", "OnHold", "Cancelled"
```

**Response**:
```json
{
  "success": true,
  "message": "Task status updated successfully",
  "data": true
}
```

#### DELETE /api/v1/tasks/{id}
Delete a task.

**Authorization**: Required (Admin, Manager roles only)

**Parameters**:
- `id` (path) - Task ID

**Response**:
```json
{
  "success": true,
  "message": "Task deleted successfully",
  "data": true
}
```

#### GET /api/v1/tasks/advanced
Get tasks with advanced filtering, sorting, and pagination.

**Authorization**: Required (All authenticated users)

**Query Parameters**: Extended filtering and sorting options including date ranges, status filters, sorting by multiple fields.

**Response**: Enhanced paginated response with metadata and navigation links.

#### GET /api/v1/tasks/{taskId}/progress-reports
Get progress reports for a specific task.

**Authorization**: Required (All authenticated users)

**Parameters**:
- `taskId` (path) - Task ID
- `pageNumber`, `pageSize` - Standard pagination

**Response**: Paginated list of task progress reports with completion percentages, status updates, and work logs.

#### POST /api/v1/tasks/{taskId}/progress-reports  
Create a progress report for a task.

**Authorization**: Required (All authenticated users for assigned tasks, Admin/Manager/Supervisor for all tasks)

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

**Response**: Created progress report details

---
