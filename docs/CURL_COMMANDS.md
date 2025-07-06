# Solar Projects API - Quick Curl Commands

## 🔐 Authentication

### Register a new user (Public endpoint)
```bash
curl -X POST "http://localhost:5001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser123",
    "email": "newuser@example.com",
    "password": "SecurePass123!",
    "fullName": "New User",
    "roleId": 3
  }'
```

### Login to get JWT token
```bash
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

## 👥 User Management

### Create Admin User
```bash
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "new_admin",
    "email": "new_admin@solarprojects.com",
    "password": "SecurePassword123!",
    "fullName": "New Administrator",
    "roleId": 1
  }'
```

### Create Manager User
```bash
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "project_manager",
    "email": "manager@solarprojects.com",
    "password": "ManagerPass123!",
    "fullName": "Project Manager",
    "roleId": 2
  }'
```

### Create Standard User
```bash
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "username": "field_worker",
    "email": "worker@solarprojects.com",
    "password": "WorkerPass123!",
    "fullName": "Field Worker",
    "roleId": 3
  }'
```

### Get All Users
```bash
curl -X GET "http://localhost:5001/api/v1/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Get User by ID
```bash
curl -X GET "http://localhost:5001/api/v1/users/{user-id}" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## 📋 Project Management

### Create Project
```bash
curl -X POST "http://localhost:5001/api/v1/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "projectName": "Solar Installation Project",
    "address": "123 Solar Street, Bangkok, Thailand",
    "clientInfo": "ABC Solar Company",
    "startDate": "2025-01-20T00:00:00Z",
    "estimatedEndDate": "2025-06-20T00:00:00Z"
  }'
```

### Get All Projects
```bash
curl -X GET "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Update Project Status
```bash
curl -X PUT "http://localhost:5001/api/v1/projects/{project-id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "projectName": "Updated Solar Project",
    "status": "In Progress",
    "address": "456 Updated Street, Bangkok, Thailand"
  }'
```

## 🔗 Role Reference
- **1**: Admin (Full system access)
- **2**: Manager (Project management access)  
- **3**: User (Standard user access)
- **4**: Viewer (Read-only access)

## 🛠 Helper Script
Use the automated script for easier admin user creation:
```bash
./scripts/create_admin_user.sh
```

Or with custom parameters:
```bash
./scripts/create_admin_user.sh "custom_admin" "admin@company.com" "Custom Administrator"
```
