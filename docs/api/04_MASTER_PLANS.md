# 🏗️ Master Plan Management

**🔒 Authentication Required**  
**🎯 Role Required**: Administrator, Project Manager (full CRUD access), All authenticated users (view only)

Master plans provide comprehensive project planning and management capabilities, allowing Project Managers to create detailed plans with phases, milestones, and budget tracking for solar installation projects.

## ⚡ Administrator & Project Manager Capabilities

- ✅ Create new master plans for projects
- ✅ Update plan details (name, description, dates, budget)
- ✅ Manage plan status and versioning
- ✅ Set project phases and milestones
- ✅ Track progress and budget allocation
- ✅ Approve and submit plans for execution

## 📖 User & Viewer Access

- Read-only access to approved master plans
- Cannot modify plan information
- Can view project progress and milestones

## 🏗️ Create Master Plan

**POST** `/api/v1/master-plans`

**🔒 Required Roles**: Administrator, ProjectManager

Create a new master plan for a project with detailed planning information.

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1",
  "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
  "projectId": 123,
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-09-30T00:00:00Z",
  "budget": 275000.00,
  "priority": "High",
  "notes": "Plan includes weather contingency and permit approval delays"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Master plan created successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Draft",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": null,
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": null
  },
  "errors": []
}
```

## 🔍 Get Master Plan by ID

**GET** `/api/v1/master-plans/{id}`

Retrieve detailed information about a specific master plan.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Approved",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": "2025-06-16T14:30:00Z",
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": "Sarah Administrator"
  },
  "errors": []
}
```

## 🔍 Get Master Plan by Project ID

**GET** `/api/v1/master-plans/project/{projectId}`

Retrieve the master plan associated with a specific project.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "InProgress",
    "budget": 275000.00,
    "priority": "High",
    "projectName": "Solar Installation Project Alpha"
  },
  "errors": []
}
```

## 🔄 Update Master Plan

**PUT** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Update an existing master plan with new information.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1 (Updated)",
  "description": "Updated comprehensive plan with revised timeline and budget",
  "startDate": "2025-07-15T00:00:00Z",
  "endDate": "2025-10-15T00:00:00Z",
  "status": "InProgress",
  "budget": 285000.00,
  "priority": "High",
  "notes": "Updated to include additional safety requirements and equipment upgrades"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan updated successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1 (Updated)",
    "description": "Updated comprehensive plan with revised timeline and budget",
    "startDate": "2025-07-15T00:00:00Z",
    "endDate": "2025-10-15T00:00:00Z",
    "status": "InProgress",
    "budget": 285000.00,
    "priority": "High",
    "notes": "Updated to include additional safety requirements and equipment upgrades",
    "updatedAt": "2025-06-16T15:45:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Master Plan

**DELETE** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Delete a master plan. Only plans in "Draft" status can be deleted.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan deleted successfully",
  "data": null,
  "errors": []
}
```

## 📊 Master Plan Status Workflow

Master plans follow a defined status workflow:

| Status | Description | Available Actions | Who Can Change |
|--------|-------------|-------------------|----------------|
| **Draft** | Initial creation, editable | Edit, Submit for Approval, Delete | Creator, Admin |
| **Pending** | Submitted for approval | Approve, Reject, Request Changes | Admin, Senior PM |
| **Approved** | Ready for execution | Start Execution, Archive | Admin, PM |
| **InProgress** | Currently being executed | Update Progress, Complete | Admin, PM |
| **Completed** | Successfully finished | Archive, Generate Report | Admin, PM |
| **Cancelled** | Cancelled before completion | Archive, Restart | Admin |

## 🔧 Master Plan Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **MP001** | Master plan not found | Verify the plan ID exists |
| **MP002** | Project already has a master plan | Update existing plan or create new version |
| **MP003** | Invalid project ID | Ensure project exists and is accessible |
| **MP004** | Insufficient permissions | Check user role requirements |
| **MP005** | Cannot delete plan with active tasks | Complete or remove associated tasks first |
| **MP006** | Invalid status transition | Follow the defined workflow sequence |

## 📊 Dashboard & Timeline API

### Overview
This section details the specialized endpoints designed to support a modern, interactive project management dashboard with real-time insights and visualizations.

### 📈 Gantt Chart Data API

**Endpoint**: `GET /api/v1/master-plans/{planId}/gantt-data`

Provides comprehensive task data structured for Gantt chart visualization, including dependencies and progress tracking.

#### Request Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| planId    | string | Yes | The unique identifier of the master plan |
| depth     | number | No | Maximum depth of task hierarchy to return (default: all) |
| baseline  | string | No | Baseline date for comparison (ISO 8601) |

#### Response Structure
```json
{
  "tasks": [
    {
      "id": "task-123",
      "text": "Task 1: Site Preparation",
      "start_date": "2025-07-01",
      "end_date": "2025-07-05",
      "progress": 0.75,
      "dependencies": [],
      "baseline": {
        "start_date": "2025-07-01",
        "end_date": "2025-07-04"
      }
    },
    {
      "id": "task-124",
      "text": "Task 2: Foundation Pouring",
      "start_date": "2025-07-06",
      "end_date": "2025-07-10",
      "progress": 0.5,
      "dependencies": ["task-123"]
    }
  ],
  "links": [
    {
      "id": "link-1",
      "source": "task-123",
      "target": "task-124",
      "type": "finish_to_start"
    }
  ]
}
```

### 📅 Weekly View API

**Endpoint**: `GET /api/v1/master-plans/{planId}/weekly-view`

Provides aggregated weekly data for calendar views and progress tracking.

#### Request Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| planId    | string | Yes | The unique identifier of the master plan |
| start_date| string | Yes | Start date for the view (YYYY-MM-DD) |
| end_date  | string | Yes | End date for the view (YYYY-MM-DD) |
| timezone  | string | No | Client timezone (default: UTC) |

#### Response Structure
```json
{
  "planId": "plan-abc",
  "viewStartDate": "2025-07-01",
  "viewEndDate": "2025-07-21",
  "weeks": [
    {
      "weekNumber": 27,
      "weekLabel": "Jul 01 - Jul 07",
      "summary": {
        "totalTasks": 12,
        "completedTasks": 5,
        "inProgressTasks": 4,
        "notStartedTasks": 3,
        "plannedHours": 160,
        "actualHours": 140,
        "efficiency": 0.875
      },
      "tasks": [
        {
          "id": "task-123",
          "name": "Site Preparation",
          "status": "IN_PROGRESS",
          "progress": {
            "percentageComplete": 75,
            "status": "On Track",
            "details": "Excavation complete, leveling in progress",
            "lastUpdated": "2025-07-03T14:30:00Z"
          }
        }
      ]
    }
  ]
}
```

### 📊 Progress Tracking

All task and master plan resources include a standardized progress object that provides real-time status information.

#### Progress Object Structure
```json
{
  "progress": {
    "percentageComplete": 45.5,
    "status": "On Track",
    "details": "Current phase proceeding as planned",
    "lastUpdated": "2025-07-10T15:30:00Z",
    "metrics": {
      "plannedHours": 100,
      "actualHours": 42,
      "remainingHours": 58,
      "efficiency": 0.95
    }
  }
}
```

### 🔄 Real-time Updates

To support real-time dashboard updates, the API provides WebSocket connections for live progress updates:

**WebSocket Endpoint**: `ws://api.example.com/ws/master-plans/{planId}/progress`

```json
// Example WebSocket message
{
  "type": "progress_update",
  "taskId": "task-123",
  "progress": {
    "percentageComplete": 80,
    "status": "On Track",
    "lastUpdated": "2025-07-10T15:35:00Z"
  }
}
```

### 📱 Mobile Optimization

For mobile clients, you can request condensed responses by including the `view=mobile` query parameter:

```http
GET /api/v1/master-plans/{planId}/weekly-view?view=mobile
```

This returns a simplified response optimized for mobile bandwidth and rendering:

```json
{
  "weeks": [
    {
      "weekLabel": "Jul 01 - 07",
      "progress": 75,
      "status": "On Track",
      "criticalTasks": 2
    }
  ]
}
```

### 🎯 Best Practices

1. **Efficient Data Loading**
   - Use the `fields` parameter to request only needed data
   - Implement pagination for large datasets
   - Cache frequently accessed dashboard data

2. **Real-time Updates**
   - Subscribe to WebSocket updates for live progress
   - Implement retry logic for connection drops
   - Use the `If-Modified-Since` header to optimize polling

3. **Error Handling**
   - Handle timezone conversion edge cases
   - Implement graceful degradation for offline mode
   - Cache last known good state for reliability

---
*Last Updated: June 15, 2025*
