# Work Breakdown Structure (WBS) API

The WBS API provides comprehensive Work Breakdown Structure management for solar PV installation projects, enabling hierarchical task management, progress tracking, and evidence collection.

## Overview

The Work Breakdown Structure (WBS) is a project management tool that breaks down complex solar installation projects into smaller, manageable work packages. Each task in the WBS represents a specific deliverable or work item with clear acceptance criteria, dependencies, and progress tracking.

## Authentication

All WBS endpoints require JWT authentication:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Admin | Full CRUD access to all WBS tasks |
| Manager | Full CRUD access to WBS tasks in assigned projects |
| User | Read access to assigned tasks, can update task status and add evidence |
| Viewer | Read-only access to WBS tasks |

## Core Endpoints

### Get All WBS Tasks

**GET** `/api/v1/wbs`

Retrieve all WBS tasks with optional filtering.

**Query Parameters:**
- `projectId` (Guid): Filter by project ID
- `installationArea` (string): Filter by installation area
- `status` (string): Filter by task status
- `assignedUserId` (Guid): Filter by assigned user

**Response:**
```json
{
  "success": true,
  "message": "WBS tasks retrieved successfully",
  "data": [
    {
      "wbsId": "4.3.2.3",
      "parentWbsId": "4.3.2",
      "taskNameEN": "Pull-out Test & Verification",
      "taskNameTH": "การทดสอบแรงดึงและตรวจสอบ",
      "description": "Perform pull-out test on anchors according to ASTM E488",
      "status": "InProgress",
      "weightPercent": 15.0,
      "installationArea": "Water Tank Roof",
      "acceptanceCriteria": "Pass pull-out test at 1.5x working load",
      "plannedStartDate": "2025-07-10T08:00:00Z",
      "actualStartDate": "2025-07-11T09:30:00Z",
      "plannedEndDate": "2025-07-12T17:00:00Z",
      "actualEndDate": null,
      "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
      "assignedUserId": "456e7890-e89b-12d3-a456-426614174001",
      "assignedUserName": "John Technician",
      "dependencies": ["4.3.2.2"],
      "evidenceCount": 3,
      "createdAt": "2025-07-01T10:00:00Z",
      "updatedAt": "2025-07-11T14:30:00Z"
    }
  ]
}
```

### Get Specific WBS Task

**GET** `/api/v1/wbs/{wbsId}`

Retrieve detailed information about a specific WBS task.

**Parameters:**
- `wbsId` (string): The WBS task identifier (e.g., "4.3.2.3")

**Response:**
```json
{
  "success": true,
  "message": "WBS task retrieved successfully",
  "data": {
    "wbsId": "4.3.2.3",
    "parentWbsId": "4.3.2",
    "taskNameEN": "Pull-out Test & Verification",
    "taskNameTH": "การทดสอบแรงดึงและตรวจสอบ",
    "description": "Perform pull-out test on anchors according to ASTM E488 standard",
    "status": "InProgress",
    "weightPercent": 15.0,
    "installationArea": "Water Tank Roof",
    "acceptanceCriteria": "Pass pull-out test at 1.5x working load",
    "plannedStartDate": "2025-07-10T08:00:00Z",
    "actualStartDate": "2025-07-11T09:30:00Z",
    "plannedEndDate": "2025-07-12T17:00:00Z",
    "actualEndDate": null,
    "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
    "assignedUserId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedUserName": "John Technician",
    "dependencies": ["4.3.2.2"],
    "evidenceCount": 3,
    "createdAt": "2025-07-01T10:00:00Z",
    "updatedAt": "2025-07-11T14:30:00Z"
  }
}
```

### Create New WBS Task

**POST** `/api/v1/wbs`

Create a new WBS task. Requires Admin or Manager role.

**Request Body:**
```json
{
  "wbsId": "4.1.1.6",
  "parentWbsId": "4.1.1",
  "taskNameEN": "Final Inspection",
  "taskNameTH": "การตรวจสอบขั้นสุดท้าย",
  "description": "Final check of foundation and structure before equipment installation",
  "status": "NotStarted",
  "weightPercent": 1.0,
  "installationArea": "Inverter Room",
  "acceptanceCriteria": "All structural elements match design drawings",
  "plannedStartDate": "2025-08-01T08:00:00Z",
  "plannedEndDate": "2025-08-01T17:00:00Z",
  "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
  "assignedUserId": "456e7890-e89b-12d3-a456-426614174001",
  "dependencies": ["4.1.1.5"]
}
```

**Response:**
```json
{
  "success": true,
  "message": "WBS task created successfully",
  "data": {
    "wbsId": "4.1.1.6",
    "parentWbsId": "4.1.1",
    "taskNameEN": "Final Inspection",
    "taskNameTH": "การตรวจสอบขั้นสุดท้าย",
    "description": "Final check of foundation and structure before equipment installation",
    "status": "NotStarted",
    "weightPercent": 1.0,
    "installationArea": "Inverter Room",
    "acceptanceCriteria": "All structural elements match design drawings",
    "plannedStartDate": "2025-08-01T08:00:00Z",
    "plannedEndDate": "2025-08-01T17:00:00Z",
    "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
    "assignedUserId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedUserName": "John Technician",
    "dependencies": ["4.1.1.5"],
    "evidenceCount": 0,
    "createdAt": "2025-07-01T10:00:00Z",
    "updatedAt": "2025-07-01T10:00:00Z"
  }
}
```

### Update WBS Task

**PUT** `/api/v1/wbs/{wbsId}`

Update an existing WBS task. Requires Admin or Manager role.

**Parameters:**
- `wbsId` (string): The WBS task identifier

**Request Body:**
```json
{
  "taskNameEN": "Final Inspection & Cleanup",
  "taskNameTH": "การตรวจสอบและทำความสะอาดขั้นสุดท้าย",
  "description": "Final check of foundation and structure, followed by site cleanup",
  "status": "InProgress",
  "weightPercent": 1.5,
  "acceptanceCriteria": "All structural elements match design drawings and area is clean",
  "actualStartDate": "2025-08-01T09:15:00Z",
  "assignedUserId": "456e7890-e89b-12d3-a456-426614174001"
}
```

**Response:**
```json
{
  "success": true,
  "message": "WBS task updated successfully",
  "data": {
    "wbsId": "4.1.1.6",
    "parentWbsId": "4.1.1",
    "taskNameEN": "Final Inspection & Cleanup",
    "taskNameTH": "การตรวจสอบและทำความสะอาดขั้นสุดท้าย",
    "description": "Final check of foundation and structure, followed by site cleanup",
    "status": "InProgress",
    "weightPercent": 1.5,
    "installationArea": "Inverter Room",
    "acceptanceCriteria": "All structural elements match design drawings and area is clean",
    "plannedStartDate": "2025-08-01T08:00:00Z",
    "actualStartDate": "2025-08-01T09:15:00Z",
    "plannedEndDate": "2025-08-01T17:00:00Z",
    "actualEndDate": null,
    "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
    "assignedUserId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedUserName": "John Technician",
    "dependencies": ["4.1.1.5"],
    "evidenceCount": 0,
    "createdAt": "2025-07-01T10:00:00Z",
    "updatedAt": "2025-08-01T09:15:00Z"
  }
}
```

### Delete WBS Task

**DELETE** `/api/v1/wbs/{wbsId}`

Delete a WBS task. Requires Admin role only.

**Parameters:**
- `wbsId` (string): The WBS task identifier

**Response:**
```json
{
  "success": true,
  "message": "WBS task deleted successfully",
  "data": true
}
```

### Get Project Progress

**GET** `/api/v1/wbs/project/{projectId}/progress`

Retrieve project progress based on WBS task completion.

**Parameters:**
- `projectId` (Guid): The project identifier

**Response:**
```json
{
  "success": true,
  "message": "Project progress retrieved successfully",
  "data": {
    "projectId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
    "totalTasks": 25,
    "completedTasks": 15,
    "progressPercentage": 65.5,
    "statusSummary": {
      "NotStarted": 5,
      "InProgress": 5,
      "Completed": 15
    },
    "areaProgress": [
      {
        "installationArea": "Inverter Room",
        "totalTasks": 8,
        "completedTasks": 6,
        "progressPercentage": 75.0
      },
      {
        "installationArea": "Carport",
        "totalTasks": 10,
        "completedTasks": 7,
        "progressPercentage": 70.0
      },
      {
        "installationArea": "Water Tank Roof",
        "totalTasks": 7,
        "completedTasks": 2,
        "progressPercentage": 28.6
      }
    ]
  }
}
```

## WBS Task Status Values

The following status values are supported:

| Status | Description |
|--------|-------------|
| `NotStarted` | Task has not been started |
| `InProgress` | Task is currently being worked on |
| `Completed` | Task has been completed successfully |
| `OnHold` | Task is temporarily paused |
| `Cancelled` | Task has been cancelled |
| `UnderReview` | Task is under review/inspection |
| `Approved` | Task has been reviewed and approved |

## Installation Areas

Common installation areas for solar PV projects:

- **Inverter Room**: Central control and power conversion area
- **Carport**: Solar panel installation on carport structures
- **Water Tank Roof**: Solar panels mounted on water tank rooftops
- **LV/HV System**: Low/High voltage electrical systems
- **Foundation**: Structural foundation work
- **General**: Tasks not specific to a particular area

## Error Handling

All endpoints return standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

## Common Error Codes

| HTTP Code | Description |
|-----------|-------------|
| 400 | Bad Request - Invalid input data |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - WBS task not found |
| 409 | Conflict - WBS ID already exists |
| 500 | Internal Server Error |

## Data Model

### WBS Task Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `wbsId` | string | Yes | Unique hierarchical identifier (e.g., "4.3.2.3") |
| `parentWbsId` | string | No | Parent task identifier |
| `taskNameEN` | string | Yes | Task name in English |
| `taskNameTH` | string | Yes | Task name in Thai |
| `description` | string | No | Detailed task description |
| `status` | enum | Yes | Current task status |
| `weightPercent` | double | Yes | Task weight (0-100) |
| `installationArea` | string | No | Installation area |
| `acceptanceCriteria` | string | No | Completion criteria |
| `plannedStartDate` | datetime | No | Planned start date |
| `actualStartDate` | datetime | No | Actual start date |
| `plannedEndDate` | datetime | No | Planned end date |
| `actualEndDate` | datetime | No | Actual end date |
| `projectId` | Guid | Yes | Associated project |
| `assignedUserId` | Guid | No | Assigned user |
| `dependencies` | array | No | Prerequisite task IDs |

### Best Practices

1. **WBS ID Numbering**: Use hierarchical numbering (e.g., 1.1.1, 1.1.2, 1.2.1)
2. **Task Weights**: Ensure child task weights sum to parent weight
3. **Dependencies**: Define clear prerequisite relationships
4. **Acceptance Criteria**: Specify measurable completion criteria
5. **Status Updates**: Update status as work progresses
6. **Evidence Collection**: Attach photos and documents as evidence

---
*Last Updated: January 2025*
