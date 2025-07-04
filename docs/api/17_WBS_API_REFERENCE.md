# Work Breakdown Structure (WBS) API Reference

The WBS API provides comprehensive management of Work Breakdown Structure tasks for solar PV installation projects. This implementation follows the detailed Solar PV WBS Implementation Plan and supports the complete project lifecycle from initiation to closure.

## Overview

The WBS API allows for:
- Hierarchical task management with parent-child relationships
- Task dependency tracking (prerequisite relationships)
- Progress monitoring and reporting
- Evidence attachment (photos, documents)
- Status updates and workflow management
- Project progress calculation based on weighted task completion

## Authentication

All WBS endpoints require authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Base URL

```
/api/v1/wbs
```

## Endpoints

### 1. Get All WBS Tasks

**GET** `/api/v1/wbs`

Retrieve all WBS tasks for a project with optional filtering.

**Query Parameters:**
- `projectId` (required): GUID - Project ID
- `installationArea` (optional): string - Filter by installation area (e.g., "Inverter Room", "Carport", "Water Tank Roof")
- `status` (optional): string - Filter by status (NotStarted, InProgress, Completed, OnHold, Cancelled)

**Example Request:**
```
GET /api/v1/wbs?projectId=123e4567-e89b-12d3-a456-426614174000&installationArea=Carport&status=InProgress
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "wbsId": "4.2.1",
      "parentWbsId": "4.2",
      "taskNameEN": "Foundation and Structural Works",
      "taskNameTH": "งานฐานรากและโครงสร้าง",
      "description": "Ground leveling, pouring concrete for footings and piers, and installing the columns, beams, and roof structure of the carport",
      "status": "InProgress",
      "weightPercent": 15.0,
      "installationArea": "Carport",
      "acceptanceCriteria": "All structural elements installed as per design drawings",
      "plannedStartDate": "2025-07-10T08:00:00Z",
      "actualStartDate": "2025-07-11T09:30:00Z",
      "plannedEndDate": "2025-07-15T17:00:00Z",
      "actualEndDate": null,
      "projectId": "123e4567-e89b-12d3-a456-426614174000",
      "assignedUserId": "456e7890-e89b-12d3-a456-426614174000",
      "assignedUserName": "John Smith",
      "dependencies": ["4.1.3"],
      "evidenceCount": 2,
      "createdAt": "2025-07-05T10:00:00Z",
      "updatedAt": "2025-07-05T14:30:00Z"
    }
  ],
  "message": "WBS tasks retrieved successfully"
}
```

### 2. Get Single WBS Task

**GET** `/api/v1/wbs/{wbsId}`

Retrieve details of a specific WBS task including dependencies and evidence.

**Path Parameters:**
- `wbsId` (required): string - The WBS ID (e.g., "4.3.2.3")

**Example Request:**
```
GET /api/v1/wbs/4.3.2.3
```

### 3. Get Task Hierarchy

**GET** `/api/v1/wbs/hierarchy/{projectId}`

Retrieve the hierarchical tree structure of all WBS tasks for a project.

**Path Parameters:**
- `projectId` (required): GUID - Project ID

**Response Structure:**
```json
{
  "success": true,
  "data": [
    {
      "wbsId": "4",
      "taskNameEN": "Phase 3: Installation and Construction",
      "taskNameTH": "ระยะที่ 3: การติดตั้งและก่อสร้าง",
      "status": "InProgress",
      "weightPercent": 60.0,
      "installationArea": null,
      "children": [
        {
          "wbsId": "4.1",
          "taskNameEN": "Inverter Room Installation",
          "taskNameTH": "การติดตั้งห้องอินเวอร์เตอร์",
          "status": "InProgress",
          "weightPercent": 25.0,
          "installationArea": "Inverter Room",
          "children": []
        }
      ]
    }
  ],
  "message": "WBS task hierarchy retrieved successfully"
}
```

### 4. Create WBS Task

**POST** `/api/v1/wbs`

Create a new WBS task. Requires Admin or ProjectManager role.

**Request Body:**
```json
{
  "wbsId": "4.3.2.4",
  "parentWbsId": "4.3.2",
  "taskNameEN": "Electrical Connections",
  "taskNameTH": "การเชื่อมต่อระบบไฟฟ้า",
  "description": "Installing cable trays, running DC cables, and terminating connections",
  "status": "NotStarted",
  "weightPercent": 5.0,
  "installationArea": "Water Tank Roof",
  "acceptanceCriteria": "All electrical connections completed and tested",
  "projectId": "123e4567-e89b-12d3-a456-426614174000",
  "assignedUserId": "456e7890-e89b-12d3-a456-426614174000",
  "plannedStartDate": "2025-07-20T08:00:00Z",
  "plannedEndDate": "2025-07-22T17:00:00Z",
  "dependencies": ["4.3.2.3"]
}
```

### 5. Update WBS Task

**PUT** `/api/v1/wbs/{wbsId}`

Update an existing WBS task. Requires Admin or ProjectManager role.

**Path Parameters:**
- `wbsId` (required): string - The WBS ID to update

**Request Body:** Same structure as create, excluding `wbsId`

### 6. Delete WBS Task

**DELETE** `/api/v1/wbs/{wbsId}`

Delete a WBS task. Only allowed if task has no child tasks. Requires Admin role.

**Path Parameters:**
- `wbsId` (required): string - The WBS ID to delete

### 7. Update Task Status

**PATCH** `/api/v1/wbs/{wbsId}/status`

Update the status of a WBS task. Automatically tracks actual start/end dates.

**Path Parameters:**
- `wbsId` (required): string - The WBS ID

**Request Body:**
```json
"Completed"
```

**Available Statuses:**
- `NotStarted` - Task not yet started
- `InProgress` - Task is currently being worked on
- `Completed` - Task has been completed
- `OnHold` - Task is temporarily paused
- `Cancelled` - Task has been cancelled
- `UnderReview` - Task is under review
- `Approved` - Task has been approved

### 8. Add Evidence

**POST** `/api/v1/wbs/{wbsId}/evidence`

Add evidence (photos, documents) to a WBS task.

**Path Parameters:**
- `wbsId` (required): string - The WBS ID

**Request Body:**
```json
{
  "url": "https://storage.example.com/evidence/photo1.jpg",
  "type": "photo",
  "title": "Foundation Work Progress",
  "description": "Photo showing concrete pouring completed as per specifications"
}
```

**Evidence Types:**
- `photo` - Progress photos
- `document` - Technical documents, certificates
- `video` - Video documentation
- `report` - Inspection reports

### 9. Get Task Evidence

**GET** `/api/v1/wbs/{wbsId}/evidence`

Retrieve all evidence for a specific WBS task.

### 10. Calculate Project Progress

**GET** `/api/v1/wbs/progress/{projectId}`

Calculate overall project progress based on WBS task completion and weights.

**Response:**
```json
{
  "success": true,
  "data": {
    "projectId": "123e4567-e89b-12d3-a456-426614174000",
    "progressPercentage": 67.5,
    "totalTasks": 25,
    "completedTasks": 15,
    "statusSummary": {
      "NotStarted": 5,
      "InProgress": 5,
      "Completed": 15,
      "OnHold": 0
    },
    "areaProgress": [
      {
        "installationArea": "Inverter Room",
        "tasksCompleted": 8,
        "totalTasks": 10,
        "progressPercentage": 80.0
      }
    ]
  },
  "message": "Project progress calculated successfully"
}
```

### 11. Get Ready-to-Start Tasks

**GET** `/api/v1/wbs/ready-to-start/{projectId}`

Retrieve tasks that are ready to start (all dependencies completed but not yet started).

### 12. Seed Sample Data

**POST** `/api/v1/wbs/seed-data/{projectId}`

Seed sample WBS data for a project (development/testing purposes). Requires Admin role.

**Path Parameters:**
- `projectId` (required): GUID - Project ID

This endpoint creates a comprehensive set of sample WBS tasks based on the Solar PV Installation Implementation Plan, including:
- Project Initiation & Permitting tasks
- Inverter Room Installation tasks
- Carport Installation tasks
- Water Tank Roof Installation tasks
- Testing and Commissioning tasks

## WBS Task Status Workflow

The recommended workflow for WBS task statuses:

1. **NotStarted** → **InProgress**: When work begins on the task
2. **InProgress** → **UnderReview**: When task is completed and needs review
3. **UnderReview** → **Approved**: When task passes review
4. **Approved** → **Completed**: Final completion status
5. **Any Status** → **OnHold**: When task needs to be paused
6. **OnHold** → **InProgress**: When task resumes
7. **Any Status** → **Cancelled**: When task is cancelled

## Error Handling

All endpoints follow standard HTTP status codes:

- **200 OK**: Successful operation
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

Error responses include descriptive messages:

```json
{
  "success": false,
  "message": "WBS task with ID '4.3.2.9' not found"
}
```

## Installation Areas

The system recognizes these standard installation areas for solar PV projects:

- **Inverter Room**: Equipment installation area
- **Carport**: Vehicle parking structure with solar panels
- **Water Tank Roof**: Rooftop installation on water tank
- **Main Building Roof**: Primary building rooftop
- **Ground Mount**: Ground-mounted solar array
- **Electrical Room**: Main electrical distribution area

## Dependencies and Prerequisites

WBS tasks can have dependencies on other tasks. The system:
- Prevents deletion of tasks that are prerequisites for other tasks
- Validates dependency relationships during task creation
- Provides ready-to-start task identification based on completed prerequisites
- Supports critical path analysis for project scheduling

## Integration Notes

This WBS API integrates with:
- **Project Management**: Links to main project entities
- **User Management**: Task assignment and permissions
- **File Storage**: Evidence and document management
- **Notifications**: Status change notifications
- **Reporting**: Progress tracking and analytics

For frontend implementation, consider using the hierarchy endpoint for tree views and the progress endpoint for dashboard widgets.
