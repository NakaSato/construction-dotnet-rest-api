# Task Management

**Base URL**: `/api/v1/tasks`

**Authentication Required**: All endpoints require JWT authentication  
**Status**: Available

Task management provides the ability to create, assign, and track specific work items for solar installation projects.

## Authorization & Access Control

### Admin & Manager Roles
- Create tasks for any project
- Assign tasks to team members  
- Update task status and details
- Delete tasks when needed
- View all tasks across projects

### Supervisor Roles
- Create tasks for managed projects
- Update tasks in managed projects
- View tasks in managed projects
- Cannot delete tasks

### Task Assignees
- Update status of assigned tasks
- View assigned tasks
- Cannot reassign tasks to others
- Cannot delete tasks

### All Authenticated Users
- View tasks they have access to
- Filter and search for tasks
- Cannot modify task data unless assigned

## Get All Tasks

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

## � Task Analytics

**GET** `/api/v1/tasks/analytics`

Get comprehensive task analytics and performance metrics.

**Query Parameters**:
- `projectId` (guid): Filter by specific project
- `assigneeId` (guid): Filter by assigned user
- `timeframe` (string): Analytics timeframe ("7d", "30d", "90d", "1y")
- `groupBy` (string): Group results by ("status", "assignee", "project", "day")

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task analytics retrieved successfully",
  "data": {
    "summary": {
      "totalTasks": 1245,
      "completedTasks": 892,
      "inProgressTasks": 234,
      "overdueTasks": 45,
      "completionRate": 71.6,
      "averageCompletionTime": 3.2
    },
    "performanceMetrics": {
      "tasksPerDay": 12.5,
      "averageTaskDuration": 3.2,
      "onTimeCompletionRate": 87.4,
      "productivityTrend": "Increasing",
      "qualityScore": 94.2
    },
    "statusBreakdown": {
      "NotStarted": 119,
      "InProgress": 234,
      "Completed": 892,
      "OnHold": 23,
      "Cancelled": 12
    },
    "assigneePerformance": [
      {
        "assigneeId": "user123",
        "fullName": "John Smith",
        "tasksAssigned": 45,
        "tasksCompleted": 38,
        "completionRate": 84.4,
        "averageCompletionTime": 2.8,
        "productivity": "High"
      }
    ],
    "trends": {
      "dailyCompletion": [
        {
          "date": "2024-06-20",
          "completed": 23,
          "started": 15,
          "overdue": 3
        }
      ],
      "categoryPerformance": [
        {
          "category": "Installation",
          "completionRate": 89.2,
          "averageDuration": 4.1
        }
      ]
    }
  }
}
```

## 🔄 Task Dependencies

**GET** `/api/v1/tasks/{id}/dependencies`

Get task dependencies and relationships.

**Path Parameters**:
- `id` (guid) - Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task dependencies retrieved successfully",
  "data": {
    "taskId": "task123",
    "taskTitle": "Install Solar Panels",
    "dependencies": {
      "prerequisites": [
        {
          "taskId": "task122",
          "title": "Complete Mounting System",
          "status": "Completed",
          "completedDate": "2024-06-19T16:30:00Z",
          "dependencyType": "FinishToStart"
        }
      ],
      "dependents": [
        {
          "taskId": "task124",
          "title": "Connect Electrical System",
          "status": "NotStarted",
          "estimatedStartDate": "2024-06-21T08:00:00Z",
          "dependencyType": "FinishToStart"
        }
      ]
    },
    "criticalPath": true,
    "schedulingConstraints": [
      {
        "type": "MustStartOn",
        "date": "2024-06-20T08:00:00Z",
        "reason": "Equipment delivery schedule"
      }
    ],
    "impactAnalysis": {
      "delayImpact": "High",
      "affectedTasks": 8,
      "projectDelayRisk": "Medium"
    }
  }
}
```

**POST** `/api/v1/tasks/{id}/dependencies`

Add task dependency.

**Path Parameters**:
- `id` (guid) - Task ID

**Request Body**:
```json
{
  "dependentTaskId": "task124",
  "dependencyType": "FinishToStart",
  "lag": 0,
  "reason": "Electrical work requires completed panel installation"
}
```

## 📈 Task Progress Tracking

**POST** `/api/v1/tasks/{id}/progress`

Update task progress with detailed tracking.

**Path Parameters**:
- `id` (guid) - Task ID

**Request Body**:
```json
{
  "completionPercentage": 75,
  "hoursWorked": 6,
  "progressNotes": "Installed 15 out of 20 planned solar panels",
  "issues": [
    {
      "description": "Weather delay in afternoon",
      "severity": "Low",
      "resolved": true
    }
  ],
  "nextSteps": [
    "Complete remaining 5 panels tomorrow morning",
    "Begin electrical connections"
  ],
  "resourcesUsed": [
    {
      "resourceType": "Equipment",
      "resourceId": "crane001",
      "hoursUsed": 4
    }
  ],
  "qualityChecks": [
    {
      "checkType": "Installation Alignment",
      "status": "Passed",
      "notes": "All panels properly aligned and secured"
    }
  ]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task progress updated successfully",
  "data": {
    "taskId": "task123",
    "completionPercentage": 75,
    "estimatedCompletionDate": "2024-06-21T16:00:00Z",
    "remainingWork": {
      "estimatedHours": 2,
      "estimatedDays": 0.25
    },
    "performanceMetrics": {
      "efficiency": 94.2,
      "qualityScore": 96.5,
      "scheduleAdherence": 102.3
    },
    "updatedAt": "2024-06-20T14:30:00Z"
  }
}
```

## 🏷️ Task Categories & Templates

**GET** `/api/v1/tasks/categories`

Get available task categories and templates.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task categories retrieved successfully",
  "data": {
    "categories": [
      {
        "categoryId": "cat001",
        "name": "Installation",
        "description": "Physical installation tasks",
        "defaultDuration": 8,
        "requiredSkills": ["Solar Installation", "Safety Training"],
        "templates": [
          {
            "templateId": "tmpl001",
            "title": "Install Solar Panels",
            "description": "Standard solar panel installation",
            "estimatedHours": 6,
            "requiredEquipment": ["Crane", "Safety Harness"],
            "usageCount": 89
          }
        ]
      }
    ]
  }
}
```

**POST** `/api/v1/tasks/from-template/{templateId}`

Create task from template.

**Path Parameters**:
- `templateId` (guid) - Template ID

**Request Body**:
```json
{
  "projectId": "proj123",
  "assignedTechnicianId": "user456",
  "dueDate": "2024-06-25T17:00:00Z",
  "customizations": {
    "title": "Install Solar Panels - Building A",
    "estimatedHours": 8,
    "priority": "High"
  }
}
```

## 🔍 Advanced Task Search

**GET** `/api/v1/tasks/search`

Advanced task search with filtering and full-text search.

**Query Parameters**:
- `q` (string): Search query
- `filters` (object): Advanced filter criteria
- `assigneeId` (guid): Filter by assignee
- `status` (string): Filter by status
- `priority` (string): Filter by priority
- `dueDate` (string): Filter by due date range
- `tags` (string): Filter by tags

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task search completed successfully",
  "data": {
    "query": "solar panel installation",
    "results": [
      {
        "taskId": "task123",
        "title": "Install Solar Panels - Section A",
        "relevanceScore": 94.5,
        "matchedFields": ["title", "description"],
        "highlights": [
          "Install <mark>Solar</mark> <mark>Panels</mark> - Section A",
          "Complete <mark>installation</mark> of mounting system"
        ],
        "projectName": "Building A Solar Project",
        "assigneeName": "John Smith",
        "status": "InProgress"
      }
    ],
    "facets": {
      "status": {
        "InProgress": 45,
        "NotStarted": 23,
        "Completed": 156
      },
      "priority": {
        "High": 34,
        "Medium": 78,
        "Low": 45
      },
      "assignee": {
        "John Smith": 23,
        "Jane Doe": 18
      }
    },
    "totalResults": 156,
    "searchTime": 0.045
  }
}
```

## 🔄 Bulk Task Operations

**POST** `/api/v1/tasks/bulk-update`

Update multiple tasks in a single operation.

**Request Body**:
```json
{
  "taskIds": ["task123", "task124", "task125"],
  "operation": "update_status",
  "data": {
    "status": "InProgress",
    "assignedTechnicianId": "user456",
    "startDate": "2024-06-21T08:00:00Z"
  },
  "reason": "Team reassignment due to schedule optimization"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Bulk task update completed",
  "data": {
    "totalTasks": 3,
    "successful": 3,
    "failed": 0,
    "results": [
      {
        "taskId": "task123",
        "status": "success",
        "message": "Task updated successfully"
      }
    ],
    "updatedAt": "2024-06-20T15:30:00Z"
  }
}
```

## 🚨 Task Alerts & Notifications

**GET** `/api/v1/tasks/alerts`

Get task-related alerts and notifications.

**Query Parameters**:
- `assigneeId` (guid): Filter by assignee
- `alertType` (string): Filter by alert type
- `severity` (string): Filter by severity

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task alerts retrieved successfully",
  "data": {
    "alerts": [
      {
        "alertId": "alert123",
        "taskId": "task456",
        "taskTitle": "Install Electrical Panel",
        "alertType": "DueDateApproaching",
        "severity": "Warning",
        "message": "Task due in 2 hours",
        "dueDate": "2024-06-20T17:00:00Z",
        "assigneeName": "John Smith",
        "projectName": "Building A Solar",
        "acknowledged": false,
        "createdAt": "2024-06-20T14:45:00Z"
      }
    ],
    "summary": {
      "total": 12,
      "critical": 2,
      "warning": 7,
      "info": 3,
      "unacknowledged": 8
    }
  }
}
```

## ⚠️ Error Codes

| Code | Message | Description | Solution |
|------|---------|-------------|----------|
| **TSK001** | Task not found | Task ID doesn't exist | Verify task ID |
| **TSK002** | Invalid task data | Required fields missing | Check request body |
| **TSK003** | Access denied | Insufficient permissions | Check user role and assignment |
| **TSK004** | Invalid status transition | Status change not allowed | Follow valid status flow |
| **TSK005** | Dependency conflict | Task dependency creates cycle | Review task dependencies |
| **TSK006** | Assignment failed | User cannot be assigned | Check user availability and skills |
| **TSK007** | Template not found | Task template doesn't exist | Verify template ID |
| **TSK008** | Bulk operation failed | Some tasks couldn't be updated | Check individual task errors |
| **TSK009** | Progress validation failed | Invalid progress data | Check progress values and constraints |
| **TSK010** | Schedule conflict | Task conflicts with other tasks | Adjust task scheduling |

## �🔧 Common Use Cases

### Creating a Task
1. Use `POST /api/v1/tasks` with `projectId` parameter
2. Provide title (required) and optional description, due date, and assignee
3. Task starts in "NotStarted" status by default

### Updating Task Progress
1. Use `PATCH /api/v1/tasks/{id}/status` for quick status updates
2. Use `POST /api/v1/tasks/{id}/progress` for detailed progress tracking
3. Use `PUT /api/v1/tasks/{id}` for complete task updates

### Task Assignment
- Tasks can be assigned to technicians using `assignedTechnicianId`
- Assigned users can update their own task status
- Managers can reassign tasks to other users

### Progress Tracking
- Use progress reports to track detailed work completion
- Include hours worked, completion percentage, and notes
- Track issues and next steps for better project management

### Dependencies Management
- Set up task dependencies for proper work sequencing
- Monitor critical path and schedule impacts
- Use dependency analysis for project planning

## 📋 Summary

### Key Features
- **Comprehensive Task Management**: Full CRUD operations with advanced features
- **Progress Tracking**: Detailed progress monitoring with analytics
- **Dependency Management**: Task relationships and critical path analysis
- **Template System**: Standardized task creation from templates
- **Search & Analytics**: Advanced search and performance analytics
- **Bulk Operations**: Efficient multi-task management
- **Alert System**: Proactive notifications for task management

### Integration Points
- **Project Management**: Tasks linked to projects and master plans
- **User Management**: Role-based access and assignment controls
- **Time Tracking**: Integration with daily reports and timesheets
- **Resource Management**: Equipment and material tracking
- **Notification System**: Real-time alerts and updates

### Best Practices
- Use templates for consistent task creation
- Set up proper task dependencies for workflow management
- Regular progress updates for accurate project tracking
- Monitor task analytics for process improvement
- Implement proper role-based access controls
- Use bulk operations for efficient task management

---
*Last Updated: July 4, 2025*
