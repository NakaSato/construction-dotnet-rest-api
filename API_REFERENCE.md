# Solar Projects REST API - Complete Reference

**Base URL**: `http://localhost:5001` (Local) | `https://solar-projects-api-dev.azurewebsites.net` (Production)  
**Version**: 1.0  
**Authentication**: JWT Bearer Token (except public endpoints)

---

## 📋 Table of Contents

- [Authentication](#authentication)
- [Health Monitoring](#health-monitoring)
- [Todo Management (Legacy)](#todo-management-legacy)
- [Debug Information](#debug-information)
- [User Management](#user-management)
- [Project Management](#project-management)
- [Task Management](#task-management)
- [Image Management](#image-management)
- [Error Responses](#error-responses)
- [Testing Examples](#testing-examples)

---

## 🔐 Authentication

### Login
**POST** `/api/v1/auth/login`

**Request Body**:
```json
{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "john.doe",
      "email": "john.doe@example.com",
      "fullName": "John Doe",
      "roleName": "Technician",
      "isActive": true
    }
  },
  "errors": []
}
```

**Response (401 Unauthorized)**:
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": ["Authentication failed"]
}
```

### Register
**POST** `/api/v1/auth/register`

**Request Body**:
```json
{
  "username": "jane.smith",
  "email": "jane.smith@example.com",
  "password": "SecurePassword123!",
  "fullName": "Jane Smith",
  "roleId": 2
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "456e7890-e89b-12d3-a456-426614174001",
    "username": "jane.smith",
    "email": "jane.smith@example.com",
    "fullName": "Jane Smith",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Refresh Token
**POST** `/api/v1/auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": "new_jwt_token_here",
  "errors": []
}
```

---

## ❤️ Health Monitoring

### Basic Health Check
**GET** `/health`

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-08T07:30:11.227702Z",
  "version": "1.0.0",
  "environment": "Development"
}
```

### Detailed Health Check
**GET** `/health/detailed`

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-08T07:30:11.227702Z",
  "version": "1.0.0",
  "environment": "Development",
  "database": {
    "status": "Connected",
    "provider": "Npgsql.EntityFrameworkCore.PostgreSQL"
  },
  "memory": {
    "workingSet": 52428800,
    "gen0Collections": 12,
    "gen1Collections": 4,
    "gen2Collections": 1
  }
}
```

---

## ✅ Todo Management (Legacy)

### Get All Todos
**GET** `/api/todo`

**Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Complete solar panel installation",
    "isCompleted": false,
    "dueDate": "2025-06-15T00:00:00Z"
  },
  {
    "id": 2,
    "title": "Submit project documentation",
    "isCompleted": true,
    "dueDate": "2025-06-10T00:00:00Z"
  }
]
```

### Get Todo by ID
**GET** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Complete solar panel installation",
  "isCompleted": false,
  "dueDate": "2025-06-15T00:00:00Z"
}
```

**Response (404 Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-4bfbd643cef305ad68bd0b16d14a6998-51d8e25c1be1cfd9-00"
}
```

### Create Todo
**POST** `/api/todo`

**Request Body**:
```json
{
  "title": "Test Todo Item",
  "description": "This is a test todo",
  "isCompleted": false,
  "dueDate": "2025-06-20T00:00:00Z"
}
```

**Response (201 Created)**:
```json
{
  "id": 3,
  "title": "Test Todo Item",
  "isCompleted": false,
  "dueDate": "2025-06-20T00:00:00Z"
}
```

### Update Todo
**PUT** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Request Body**:
```json
{
  "id": 1,
  "title": "Updated Todo Title",
  "description": "Updated description",
  "isCompleted": true,
  "dueDate": "2025-06-15T00:00:00Z"
}
```

**Response (204 No Content)**

### Delete Todo
**DELETE** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Response (204 No Content)**

---

## 🔧 Debug Information

### Get Configuration
**GET** `/api/debug/config`

**Response (200 OK)**:
```json
{
  "environment": "Development",
  "connectionString": "Server=localhost;Database=SolarProjectsDb;...",
  "allConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SolarProjectsDb;..."
  }
}
```

---

## 👥 User Management

**🔒 Authentication Required**

### Get All Users
**GET** `/api/v1/users`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `role` (string, optional): Filter by role name

**Example Request**:
```
GET /api/v1/users?pageNumber=1&pageSize=20&role=Technician
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": {
    "items": [
      {
        "userId": "123e4567-e89b-12d3-a456-426614174000",
        "username": "john.doe",
        "email": "john.doe@example.com",
        "fullName": "John Doe",
        "roleName": "Technician",
        "isActive": true
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2
  },
  "errors": []
}
```

### Get User by ID
**GET** `/api/v1/users/{id}`

**Parameters**:
- `id` (path, GUID): User ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "username": "john.doe",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Create User
**POST** `/api/v1/users`  
**Required Role**: Administrator

**Request Body**:
```json
{
  "username": "new.user",
  "email": "new.user@example.com",
  "password": "SecurePassword123!",
  "fullName": "New User",
  "roleId": 2
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "username": "new.user",
    "email": "new.user@example.com",
    "fullName": "New User",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Update User
**PUT** `/api/v1/users/{id}`  
**Required Role**: Administrator

**Request Body**:
```json
{
  "username": "updated.user",
  "email": "updated.user@example.com",
  "fullName": "Updated User Name",
  "roleId": 3,
  "isActive": true
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "User updated successfully",
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "username": "updated.user",
    "email": "updated.user@example.com",
    "fullName": "Updated User Name",
    "roleName": "ProjectManager",
    "isActive": true
  },
  "errors": []
}
```

### Delete User
**DELETE** `/api/v1/users/{id}`  
**Required Role**: Administrator

**Response (204 No Content)**

---

## 🏗️ Project Management

**🔒 Authentication Required**

### Get All Projects
**GET** `/api/v1/projects`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10)
- `managerId` (GUID, optional): Filter by project manager ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "projectName": "Downtown Solar Installation",
        "address": "123 Main St, City, State 12345",
        "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)",
        "status": "In Progress",
        "startDate": "2025-05-01T00:00:00Z",
        "estimatedEndDate": "2025-07-15T00:00:00Z",
        "actualEndDate": null,
        "projectManager": {
          "userId": "123e4567-e89b-12d3-a456-426614174000",
          "username": "pm.jane",
          "email": "jane.pm@example.com",
          "fullName": "Jane Project Manager",
          "roleName": "ProjectManager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8
      }
    ],
    "totalCount": 12,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2
  },
  "errors": []
}
```

### Get Project by ID
**GET** `/api/v1/projects/{id}`

**Parameters**:
- `id` (path, GUID): Project ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Project retrieved successfully",
  "data": {
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "projectName": "Downtown Solar Installation",
    "address": "123 Main St, City, State 12345",
    "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)",
    "status": "In Progress",
    "startDate": "2025-05-01T00:00:00Z",
    "estimatedEndDate": "2025-07-15T00:00:00Z",
    "actualEndDate": null,
    "projectManager": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "pm.jane",
      "email": "jane.pm@example.com",
      "fullName": "Jane Project Manager",
      "roleName": "ProjectManager",
      "isActive": true
    },
    "taskCount": 15,
    "completedTaskCount": 8
  },
  "errors": []
}
```

### Create Project
**POST** `/api/v1/projects`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "projectName": "New Solar Installation Project",
  "address": "456 Oak Ave, Another City, State 67890",
  "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-09-30T00:00:00Z",
  "projectManagerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "projectId": "660f8500-e29b-41d4-a716-446655440001",
    "projectName": "New Solar Installation Project",
    "address": "456 Oak Ave, Another City, State 67890",
    "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
    "status": "Planning",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-09-30T00:00:00Z",
    "actualEndDate": null,
    "projectManager": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "pm.jane",
      "email": "jane.pm@example.com",
      "fullName": "Jane Project Manager",
      "roleName": "ProjectManager",
      "isActive": true
    },
    "taskCount": 0,
    "completedTaskCount": 0
  },
  "errors": []
}
```

### Update Project
**PUT** `/api/v1/projects/{id}`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "projectName": "Updated Project Name",
  "address": "Updated Address",
  "clientInfo": "Updated Client Info",
  "status": "Completed",
  "startDate": "2025-05-01T00:00:00Z",
  "estimatedEndDate": "2025-07-15T00:00:00Z",
  "actualEndDate": "2025-07-10T00:00:00Z",
  "projectManagerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (200 OK)**: Same structure as Create Project

### Delete Project
**DELETE** `/api/v1/projects/{id}`  
**Required Role**: Administrator

**Response (204 No Content)**

---

## 📋 Task Management

**🔒 Authentication Required**

### Get All Tasks
**GET** `/api/v1/tasks`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `projectId` (GUID, optional): Filter by project ID
- `assigneeId` (GUID, optional): Filter by assignee ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "items": [
      {
        "taskId": "770g8600-e29b-41d4-a716-446655440002",
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "projectName": "Downtown Solar Installation",
        "title": "Install solar panels on roof section A",
        "description": "Mount and wire solar panels on the eastern roof section",
        "status": "In Progress",
        "dueDate": "2025-06-20T00:00:00Z",
        "assignedTechnician": {
          "userId": "234f5678-e89b-12d3-a456-426614174001",
          "username": "tech.mike",
          "email": "mike.tech@example.com",
          "fullName": "Mike Technician",
          "roleName": "Technician",
          "isActive": true
        },
        "completionDate": null,
        "createdAt": "2025-05-15T00:00:00Z"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3
  },
  "errors": []
}
```

### Get Task by ID
**GET** `/api/v1/tasks/{id}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": {
    "taskId": "770g8600-e29b-41d4-a716-446655440002",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "projectName": "Downtown Solar Installation",
    "title": "Install solar panels on roof section A",
    "description": "Mount and wire solar panels on the eastern roof section",
    "status": "In Progress",
    "dueDate": "2025-06-20T00:00:00Z",
    "assignedTechnician": {
      "userId": "234f5678-e89b-12d3-a456-426614174001",
      "username": "tech.mike",
      "email": "mike.tech@example.com",
      "fullName": "Mike Technician",
      "roleName": "Technician",
      "isActive": true
    },
    "completionDate": null,
    "createdAt": "2025-05-15T00:00:00Z"
  },
  "errors": []
}
```

### Create Task
**POST** `/api/v1/tasks`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "title": "Install inverter system",
  "description": "Install and configure the main inverter system",
  "dueDate": "2025-06-25T00:00:00Z",
  "assignedTechnicianId": "234f5678-e89b-12d3-a456-426614174001"
}
```

**Response (201 Created)**: Same structure as Get Task

### Update Task
**PUT** `/api/v1/tasks/{id}`

**Request Body**:
```json
{
  "title": "Updated task title",
  "description": "Updated task description",
  "status": "Completed",
  "dueDate": "2025-06-25T00:00:00Z",
  "assignedTechnicianId": "234f5678-e89b-12d3-a456-426614174001"
}
```

**Response (200 OK)**: Same structure as Get Task

### Delete Task
**DELETE** `/api/v1/tasks/{id}`  
**Required Role**: Administrator, ProjectManager

**Response (204 No Content)**

---

## 📸 Image Management

**🔒 Authentication Required**

### Upload Image
**POST** `/api/v1/images/upload`

**Content-Type**: `multipart/form-data`

**Form Parameters**:
- `file` (file, required): Image file to upload
- `projectId` (GUID, required): Project ID to associate with the image
- `taskId` (GUID, optional): Task ID to associate with the image
- `captureTimestamp` (datetime, optional): When the image was captured
- `gpsLatitude` (decimal, optional): GPS latitude coordinate
- `gpsLongitude` (decimal, optional): GPS longitude coordinate
- `deviceModel` (string, optional): Device model used to capture the image
- `exifData` (string, optional): EXIF metadata

**Example cURL**:
```bash
curl -X POST http://localhost:5001/api/v1/images/upload \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -F "file=@/path/to/image.jpg" \
  -F "projectId=550e8400-e29b-41d4-a716-446655440000" \
  -F "taskId=770g8600-e29b-41d4-a716-446655440002" \
  -F "captureTimestamp=2025-06-08T10:30:00Z" \
  -F "gpsLatitude=40.7128" \
  -F "gpsLongitude=-74.0060" \
  -F "deviceModel=iPhone 14 Pro"
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Image uploaded successfully",
  "data": {
    "imageId": "880h8700-e29b-41d4-a716-446655440003",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "taskId": "770g8600-e29b-41d4-a716-446655440002",
    "originalFileName": "solar_panel_installation.jpg",
    "contentType": "image/jpeg",
    "fileSizeInBytes": 2048576,
    "uploadTimestamp": "2025-06-08T10:35:00Z",
    "captureTimestamp": "2025-06-08T10:30:00Z",
    "gpsLatitude": 40.7128,
    "gpsLongitude": -74.0060,
    "deviceModel": "iPhone 14 Pro",
    "imageUrl": "http://localhost:5001/files/images/880h8700-e29b-41d4-a716-446655440003.jpg",
    "uploadedBy": {
      "userId": "234f5678-e89b-12d3-a456-426614174001",
      "username": "tech.mike",
      "email": "mike.tech@example.com",
      "fullName": "Mike Technician",
      "roleName": "Technician",
      "isActive": true
    }
  },
  "errors": []
}
```

### Get Images by Project
**GET** `/api/v1/images/project/{projectId}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Images retrieved successfully",
  "data": [
    {
      "imageId": "880h8700-e29b-41d4-a716-446655440003",
      "projectId": "550e8400-e29b-41d4-a716-446655440000",
      "taskId": "770g8600-e29b-41d4-a716-446655440002",
      "originalFileName": "solar_panel_installation.jpg",
      "contentType": "image/jpeg",
      "fileSizeInBytes": 2048576,
      "uploadTimestamp": "2025-06-08T10:35:00Z",
      "captureTimestamp": "2025-06-08T10:30:00Z",
      "gpsLatitude": 40.7128,
      "gpsLongitude": -74.0060,
      "deviceModel": "iPhone 14 Pro",
      "imageUrl": "http://localhost:5001/files/images/880h8700-e29b-41d4-a716-446655440003.jpg",
      "uploadedBy": {
        "userId": "234f5678-e89b-12d3-a456-426614174001",
        "username": "tech.mike",
        "email": "mike.tech@example.com",
        "fullName": "Mike Technician",
        "roleName": "Technician",
        "isActive": true
      }
    }
  ],
  "errors": []
}
```

### Get Image by ID
**GET** `/api/v1/images/{id}`

**Response (200 OK)**: Same structure as single image from project list

### Delete Image
**DELETE** `/api/v1/images/{id}`

**Response (204 No Content)**

---

## ❌ Error Responses

### Common Error Formats

**400 Bad Request**:
```json
{
  "success": false,
  "message": "Invalid request data",
  "data": null,
  "errors": [
    "Username is required",
    "Password must be at least 8 characters"
  ]
}
```

**401 Unauthorized**:
```json
{
  "success": false,
  "message": "Authentication required",
  "data": null,
  "errors": ["Invalid or missing authorization token"]
}
```

**403 Forbidden**:
```json
{
  "success": false,
  "message": "Access denied",
  "data": null,
  "errors": ["Insufficient permissions for this operation"]
}
```

**404 Not Found**:
```json
{
  "success": false,
  "message": "Resource not found",
  "data": null,
  "errors": ["The requested resource does not exist"]
}
```

**500 Internal Server Error**:
```json
{
  "success": false,
  "message": "An internal server error occurred",
  "data": null,
  "errors": ["Please try again later or contact support"]
}
```

---

## 🧪 Testing Examples

### Using cURL

**Login and get token**:
```bash
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john.doe","password":"SecurePassword123!"}' \
  | jq -r '.data.token')
```

**Get projects with authentication**:
```bash
curl -X GET http://localhost:5001/api/v1/projects \
  -H "Authorization: Bearer $TOKEN"
```

**Create a new todo**:
```bash
curl -X POST http://localhost:5001/api/todo \
  -H "Content-Type: application/json" \
  -d '{"title":"New Todo","isCompleted":false,"dueDate":"2025-06-30T00:00:00Z"}'
```

### Using PowerShell

**Get health status**:
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/health" -Method GET
```

**Login and store token**:
```powershell
$loginData = @{
    username = "john.doe"
    password = "SecurePassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5001/api/v1/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $response.data.token
```

**Get projects with token**:
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "http://localhost:5001/api/v1/projects" -Method GET -Headers $headers
```

---

## 📝 API Features Summary

✅ **Authentication**: JWT-based login/register/refresh  
✅ **Versioning**: API v1.0 with URL-based versioning  
✅ **Authorization**: Role-based access control  
✅ **Pagination**: Configurable page sizes with metadata  
✅ **Error Handling**: Consistent error response format  
✅ **Health Monitoring**: Basic and detailed health checks  
✅ **File Upload**: Multipart form data support for images  
✅ **CORS**: Cross-origin requests enabled  
✅ **Swagger Documentation**: Interactive API explorer at `/`  
✅ **Database**: PostgreSQL with Entity Framework Core  
✅ **Logging**: Structured logging with different levels  

---

**📚 Interactive Documentation**: Visit http://localhost:5001 for live Swagger UI  
**🚀 Application Status**: http://localhost:5001/health  
**🔧 Production URL**: https://solar-projects-api-dev.azurewebsites.net (after deployment)
