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
| **⚡ Technical** | `totalCapacityKw`, `pvModuleCount`, `connectionType` | PUT/PATCH `/projects/{id}` |
| **🔧 Equipment** | `inverter125Kw`, `inverter80Kw`, `inverter60Kw`, `inverter40Kw` | PUT/PATCH `/projects/{id}` |
| **💰 Financial** | `ftsValue`, `revenueValue`, `pqmValue` | PUT/PATCH `/projects/{id}` |
| **📍 Location** | `latitude`, `longitude`, `connectionNotes` | PUT/PATCH `/projects/{id}` |
| **📊 Status** | Project status and phase management | PUT/PATCH `/projects/{id}` |

**🚫 Admin-Only Operations:**
- Project deletion (`DELETE /projects/{id}`)
- System configuration changes

**📖 User & Viewer Access:**
- Read-only access to all project data
- Cannot modify any project information
- Can submit reports and updates related to their work

## 📊 Get All Projects

**GET** `/api/v1/projects`

Retrieve a paginated list of projects with optional filtering.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status ("Planning", "Active", "Completed", "OnHold", "Cancelled")
- `search` (string): Search in project name or description

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
        "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
        "totalCapacityKw": 996.0,
        "pvModuleCount": 1800,
        "ftsValue": 18700000.0,
        "revenueValue": 22440000.0,
        "pqmValue": 3740000.0,
        "inverter125Kw": 8,
        "inverter80Kw": 0,
        "inverter60Kw": 0,
        "inverter40Kw": 0,
        "latitude": 13.4098,
        "longitude": 99.9969,
        "connectionType": "LV",
        "connectionNotes": "ระบบจำหน่ายแรงต่ำ",
        "createdAt": "2025-06-01T14:30:00Z",
        "updatedAt": "2025-06-15T10:00:00Z"
      },
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
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "name": "Solar Installation Project Alpha",
    "description": "Residential solar panel installation for 50-home subdivision with 3MW total capacity",
    "status": "Active",
    "startDate": "2025-06-01",
    "endDate": "2025-08-15",
    "location": "Sunnydale Subdivision, California",
    "budget": 250000.00,
    "actualCost": 180000.00,
    "totalTasks": 12,
    "completedTasks": 8,
    "progressPercentage": 66.7,
    "tasks": [
      {
        "id": "task123",
        "title": "Site Preparation",
        "status": "Completed",
        "dueDate": "2025-06-10"
      }
    ],
    "recentReports": [
      {
        "id": "report123",
        "reportDate": "2025-06-14",
        "userName": "John Tech",
        "hoursWorked": 8.5
      }
    ],
    "createdAt": "2025-05-15T10:00:00Z",
    "updatedAt": "2025-06-14T16:30:00Z"
  },
  "errors": []
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
  "status": "Planning",
  "startDate": "2025-07-15",
  "estimatedEndDate": "2025-10-30",
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "totalCapacityKw": 850.5,
  "pvModuleCount": 1546,
  "ftsValue": 15800000.0,
  "revenueValue": 18960000.0,
  "pqmValue": 3160000.0,
  "inverter125Kw": 7,
  "inverter80Kw": 0,
  "inverter60Kw": 0,
  "inverter40Kw": 0,
  "latitude": 13.5619,
  "longitude": 100.2743,
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV"
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
    "status": "Planning",
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
  "startDate": "2025-07-01",
  "estimatedEndDate": "2025-10-15",
  "actualEndDate": null,
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "totalCapacityKw": 875.0,
  "pvModuleCount": 1600,
  "ftsValue": 16200000.0,
  "revenueValue": 19440000.0,
  "pqmValue": 3240000.0,
  "inverter125Kw": 7,
  "inverter80Kw": 0,
  "inverter60Kw": 0,
  "inverter40Kw": 0,
  "latitude": 13.5619,
  "longitude": 100.2743,
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV"
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
    "status": "Active",
    "updatedAt": "2025-06-15T15:45:00Z"
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

**Request Body**:
```json
{
  "status": "Active",
  "startDate": "2025-07-01",
  "totalCapacityKw": 875.0
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project updated successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "status": "Active",
    "updatedAt": "2025-06-15T16:00:00Z"
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
| **Planning** | Initial project setup | Active, Cancelled |
| **Active** | Current work in progress | OnHold, Completed |
| **OnHold** | Temporarily suspended | Active, Cancelled |
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
*Last Updated: June 15, 2025*
