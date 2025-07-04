# 📋 Project Management

This guide covers all project management endpoints for the Solar Projects API.

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full CRUD access - create/read/update/delete all project data), All authenticated users (view only)

## ⚡ Admin & Manager Capabilities

- ✅ Create new projects
- ✅ Update all project fields (name, address, dates, equipment, etc.)
- ✅ Modify project assignments and team configurations  
- ✅ Update technical specifications (capacity, modules, inverters)
- ✅ Edit location coordinates and connection details
- ✅ Change project status and timelines
- 🚫 Delete projects (Admin only)

## 🔑 Detailed Update Permissions

**Admin & Manager roles have FULL UPDATE ACCESS to all project data fields:**

| Data Category | Fields Admin/Manager Can Update | API Endpoints |
|---------------|----------------------------------|---------------|
| **📋 Basic Info** | `projectName`, `address`, `clientInfo` | PUT/PATCH `/projects/{id}` |
| **📅 Timeline** | `startDate`, `estimatedEndDate`, `actualEndDate` | PUT/PATCH `/projects/{id}` |
| **👥 Team** | `projectManagerId`, `team` assignments | PUT/PATCH `/projects/{id}` |
| **⚡ Technical** | `totalCapacityKw`, `pvModuleCount`, `connectionType`, `connectionNotes` | PUT/PATCH `/projects/{id}` |
| **🔧 Equipment** | `equipmentDetails.inverter125kw`, `inverter80kw`, `inverter60kw`, `inverter40kw` | PUT/PATCH `/projects/{id}` |
| **💰 Financial** | `ftsValue`, `revenueValue`, `pqmValue` | PUT/PATCH `/projects/{id}` |
| **📍 Location** | `locationCoordinates.latitude`, `locationCoordinates.longitude` | PUT/PATCH `/projects/{id}` |
| **📊 Status** | Project status and workflow management | PUT/PATCH `/projects/{id}` |

**🚫 Admin-Only Operations:**
- Project deletion (`DELETE /projects/{id}`)
- System configuration changes

**📖 User & Viewer Access:**
- Read-only access to all project data
- Cannot modify any project information
- Can submit reports and updates related to their work

## 📊 Get All Projects

**GET** `/api/v1/projects`

Retrieve a paginated list of projects with advanced filtering, sorting, and field selection.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status ("Planning", "InProgress", "Completed", "OnHold", "Cancelled")
- `search` (string): Search in project name or description
- `sortBy` (string): Sort field (projectName, startDate, status, etc.)
- `sortOrder` (string): Sort direction ("asc" or "desc")
- `fields` (string): Comma-separated list of fields to include in response
- `managerId` (Guid): Filter by project manager ID
- `filter` (string): Advanced filter expressions

**🆕 Current Database**: 97+ solar projects imported and available

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "projectName": "สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "address": "ต.แม่กลอง อ.เมืองสมุทรสงคราม จ.สมุทรสงคราม 75000",
        "clientInfo": "101 สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "status": "Planning",
        "startDate": "2024-01-01T00:00:00Z",
        "estimatedEndDate": null,
        "actualEndDate": null,
        "updatedAt": "2025-06-15T10:00:00Z",
        "projectManager": {
          "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
          "username": "project.manager",
          "email": "pm@company.com",
          "fullName": "Project Manager",
          "roleName": "Manager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8,
        "team": "Solar Team Alpha",
        "connectionType": "LV",
        "connectionNotes": "ระบบจำหน่ายแรงต่ำ",
        "totalCapacityKw": 996.0,
        "pvModuleCount": 1800,
        "equipmentDetails": {
          "inverter125kw": 8,
          "inverter80kw": 0,
          "inverter60kw": 0,
          "inverter40kw": 0
        },
        "ftsValue": 18700000,
        "revenueValue": 22440000,
        "pqmValue": 3740000,
        "locationCoordinates": {
          "latitude": 13.4098,
          "longitude": 99.9969
        },
        "createdAt": "2025-06-01T14:30:00Z"
      }
      // More projects...
    ],
    "pagination": {
      "totalCount": 97,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 10,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

## 🔍 Get Project by ID

**GET** `/api/v1/projects/{id}`

Retrieve detailed information about a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "address": "123 Solar Street, Sunnydale, CA 90210",
    "clientInfo": "Residential solar panel installation for 50-home subdivision with 3MW total capacity",
    "status": "Active",
    "startDate": "2025-06-01T00:00:00Z",
    "estimatedEndDate": "2025-08-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-14T16:30:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "john.manager",
      "email": "john@company.com",
      "fullName": "John Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 12,
    "completedTaskCount": 8,
    "team": "Solar Installation Team Beta",
    "connectionType": "MV",
    "connectionNotes": "22kV medium voltage connection with smart metering",
    "totalCapacityKw": 3000.0,
    "pvModuleCount": 5000,
    "equipmentDetails": {
      "inverter125kw": 15,
      "inverter80kw": 5,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 45000000,
    "revenueValue": 54000000,
    "pqmValue": 9000000,
    "locationCoordinates": {
      "latitude": 34.0522,
      "longitude": -118.2437
    },
    "createdAt": "2025-05-15T10:00:00Z"
  },
  "errors": []
}
```

## 👤 Get My Projects

**GET** `/api/v1/projects/me`

Get projects associated with the current authenticated user.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "User projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "projectName": "สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "address": "ต.แม่กลอง อ.เมืองสมุทรสงคราม จ.สมุทรสงคราม 75000",
        "clientInfo": "101 สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "status": "Planning",
        "startDate": "2024-01-01T00:00:00Z",
        "estimatedEndDate": null,
        "actualEndDate": null,
        "updatedAt": "2025-06-15T10:00:00Z",
        "projectManager": {
          "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
          "username": "project.manager",
          "email": "pm@company.com",
          "fullName": "Project Manager",
          "roleName": "Manager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8,
        "team": "Solar Team Alpha",
        "connectionType": "LV",
        "connectionNotes": "ระบบจำหน่ายแรงต่ำ",
        "totalCapacityKw": 996.0,
        "pvModuleCount": 1800,
        "equipmentDetails": {
          "inverter125kw": 8,
          "inverter80kw": 0,
          "inverter60kw": 0,
          "inverter40kw": 0
        },
        "ftsValue": 18700000,
        "revenueValue": 22440000,
        "pqmValue": 3740000,
        "locationCoordinates": {
          "latitude": 13.4098,
          "longitude": 99.9969
        },
        "createdAt": "2025-06-01T14:30:00Z"
      }
      // More projects...
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "errors": []
}
```

## 📊 Get Project Status

**GET** `/api/v1/projects/{id}/status`

Get real-time status information for a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project status retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "status": "Active",
    "plannedStartDate": "2025-06-01T00:00:00Z",
    "plannedEndDate": "2025-08-15T00:00:00Z",
    "actualStartDate": "2025-06-01T00:00:00Z",
    "overallCompletionPercentage": 67.5,
    "isOnSchedule": true,
    "isOnBudget": true,
    "activeTasks": 4,
    "completedTasks": 8,
    "totalTasks": 12,
    "lastUpdated": "2025-06-14T16:30:00Z",
    "links": [
      {
        "href": "/api/v1/projects/456e7890-e89b-12d3-a456-426614174001",
        "rel": "project",
        "method": "GET"
      },
      {
        "href": "/api/v1/master-plans?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "master-plans",
        "method": "GET"
      },
      {
        "href": "/api/v1/tasks?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "tasks",
        "method": "GET"
      },
      {
        "href": "/api/v1/documents?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "documents",
        "method": "GET"
      }
    ]
  },
  "errors": []
}
```

## 🧪 API Test Endpoint

**GET** `/api/v1/projects/test`

**🔓 No Authentication Required**

Test endpoint to verify the Projects API is functioning correctly.

**Success Response (200)**:
```json
{
  "message": "Projects API is working",
  "timestamp": "2025-06-15T10:00:00Z",
  "environment": "Development",
  "apiVersion": "v1.0",
  "sampleProjects": [
    {
      "id": 1,
      "name": "Solar Farm Project A",
      "status": "Active",
      "location": "California"
    },
    {
      "id": 2,
      "name": "Solar Installation B",
      "status": "Planning",
      "location": "Texas"
    },
    {
      "id": 3,
      "name": "Residential Solar C",
      "status": "Completed",
      "location": "Florida"
    }
  ]
}
```

## 📝 Create New Project

**POST** `/api/v1/projects`

**🔒 Required Roles**: Admin, Manager

Create a new solar installation project with detailed information.

**Request Body**:
```json
{
  "projectName": "โรงพยาบาลปากน้ำ",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "startDate": "2025-07-15T00:00:00Z",
  "estimatedEndDate": "2025-10-30T00:00:00Z",
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "team": "Solar Installation Team Gamma",
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
  "totalCapacityKw": 850.5,
  "pvModuleCount": 1546,
  "equipmentDetails": {
    "inverter125kw": 7,
    "inverter80kw": 0,
    "inverter60kw": 0,
    "inverter40kw": 0
  },
  "ftsValue": 15800000,
  "revenueValue": 18960000,
  "pqmValue": 3160000,
  "locationCoordinates": {
    "latitude": 13.5619,
    "longitude": 100.2743
  }
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Planning",
    "startDate": "2025-07-15T00:00:00Z",
    "estimatedEndDate": "2025-10-30T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": null,
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 0,
    "completedTaskCount": 0,
    "team": "Solar Installation Team Gamma",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 850.5,
    "pvModuleCount": 1546,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 15800000,
    "revenueValue": 18960000,
    "pqmValue": 3160000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## 🔄 Update Project

**PUT** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update all fields of an existing project.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body**:
```json
{
  "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "status": "Active",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-10-15T00:00:00Z",
  "actualEndDate": null,
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "team": "Solar Installation Team Delta",
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
  "totalCapacityKw": 875.0,
  "pvModuleCount": 1600,
  "equipmentDetails": {
    "inverter125kw": 7,
    "inverter80kw": 0,
    "inverter60kw": 0,
    "inverter40kw": 0
  },
  "ftsValue": 16200000,
  "revenueValue": 19440000,
  "pqmValue": 3240000,
  "locationCoordinates": {
    "latitude": 13.5619,
    "longitude": 100.2743
  }
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project updated successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Active",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-10-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-15T15:45:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 5,
    "completedTaskCount": 2,
    "team": "Solar Installation Team Delta",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 875.0,
    "pvModuleCount": 1600,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 16200000,
    "revenueValue": 19440000,
    "pqmValue": 3240000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## 🔄 Partially Update Project

**PATCH** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update specific fields of an existing project without affecting other fields.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body** (All fields are optional):
```json
{
  "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "status": "InProgress",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-10-15T00:00:00Z",
  "actualEndDate": null,
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project updated successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Active",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-10-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-15T16:00:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 5,
    "completedTaskCount": 2,
    "team": "Solar Installation Team Delta",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 875.0,
    "pvModuleCount": 1600,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 1
    },
    "ftsValue": 16200000,
    "revenueValue": 19440000,
    "pqmValue": 3240000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Project

**DELETE** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin only

Delete a project and all associated data (tasks, reports, etc.).

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project deleted successfully",
  "data": null,
  "errors": []
}
```

**Error Response (403)**:
```json
{
  "success": false,
  "message": "Insufficient permissions",
  "data": null,
  "errors": ["Only Admin users can delete projects"]
}
```

## 📊 Project Statuses

| Status | Description | Allowed Transitions |
|--------|-------------|---------------------|
| **Planning** | Initial project setup | InProgress, Cancelled |
| **InProgress** | Current work in progress | OnHold, Completed |
| **OnHold** | Temporarily suspended | InProgress, Cancelled |
| **Completed** | All work finished | (Final state) |
| **Cancelled** | Project terminated | (Final state) |

## ❌ Project Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **PRJ001** | Project not found | Verify project ID exists |
| **PRJ002** | Insufficient permissions | Verify user role requirements |
| **PRJ003** | Invalid project data | Check request body for required fields |
| **PRJ004** | Cannot delete active project | Change status before deletion |
| **PRJ005** | Project manager not found | Verify project manager ID |
| **PRJ006** | Invalid status transition | Follow allowed status transitions |
| **PRJ007** | Duplicate project name | Choose a unique project name |
| **PRJ008** | Missing required fields | Add all required fields to request |

---
*Last Updated: July 4, 2025*
