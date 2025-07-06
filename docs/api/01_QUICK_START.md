# Quick Start Guide

Get started with the Solar Projects API quickly.

## Quick Start Checklist

- ✅ Deploy API & Database: Docker containers or local development
- ✅ Authenticate: Get JWT token with test admin credentials
- ✅ Make API Calls: Use token in Authorization header
- ✅ Review Responses: Check success/error fields in JSON
- ✅ Handle Errors: Use appropriate error codes for troubleshooting

## 🚀 One-Minute Setup

### Option 1: Docker (Recommended)
```bash
# Clone and start
git clone <repository>
cd dotnet-rest-api
docker-compose up -d

# API available at: http://localhost:8080
```

### Option 2: Local Development
```bash
# Clone and start
git clone <repository>
cd dotnet-rest-api
dotnet run --urls "http://localhost:5001"

# API available at: http://localhost:5001
```

## Authentication

1. **Get your authentication token**:

```bash
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

2. **Save your token** from the response and **use the token** in subsequent requests:

```bash
curl -X GET "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## API Base URLs

| Environment | Base URL | Status |
|-------------|----------|--------|
| **Local Development** | `http://localhost:5001/api/v1` | ✅ Active |
| **Docker** | `http://localhost:8080/api/v1` | ✅ Active |
| **Production** | `https://your-domain.com/api/v1` | 🔧 Configure |

## 📌 Core Endpoints

### 🔐 Authentication
- `POST /auth/login` - User authentication
- `POST /users` - Create user (Admin only)

### 📋 Projects
- `GET /projects` - Get all projects
- `GET /projects/{id}` - Get project by ID
- `POST /projects` - Create new project (Admin/Manager)
- `PUT /projects/{id}` - Update project (Admin/Manager)
- `DELETE /projects/{id}` - Delete project (Admin only)

### 📊 Master Plans
- `GET /master-plans/project/{projectId}` - Get master plan by project
- `POST /master-plans` - Create master plan (Admin/Manager)
- `PUT /master-plans/{id}` - Update master plan (Admin/Manager)

### ✅ Tasks

- `GET /tasks?projectId={projectId}` - Get tasks for project
- `POST /tasks` - Create new task (Admin/Manager)
- `PUT /tasks/{id}` - Update task (Admin/Manager/Assignee)

## 📦 Current System Status

- **Projects**: 97+ solar projects in database
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT tokens with role-based access
- **Rate Limiting**: Configurable limits per endpoint
- **Real-Time**: SignalR WebSocket connections for live updates

## 📈 Common Use Cases

### 1. Create a New Project

```bash
curl -X POST "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "New Solar Installation",
    "address": "123 Solar Street, Bangkok, Thailand",
    "clientInfo": "Client Company Name",
    "startDate": "2025-08-01T00:00:00Z",
    "estimatedEndDate": "2025-12-01T00:00:00Z"
  }'
```

### 2. Get All Projects with Pagination

```bash
curl -X GET "http://localhost:5001/api/v1/projects?pageNumber=1&pageSize=20&status=Active" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. Update Project Status

```bash
curl -X PUT "http://localhost:5001/api/v1/projects/{project-id}" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "Updated Project Name",
    "status": "In Progress",
    "address": "Updated address"
  }'
```

### 4. Create Admin User

```bash
# Use the automated script
./scripts/create_admin_user.sh "new_admin" "admin@company.com"

# Or manual API call
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "new_admin",
    "email": "admin@company.com",
    "password": "SecurePass123!",
    "fullName": "New Administrator",
    "roleId": 1
  }'
```

## 🧪 Complete Test Script

```bash
#!/bin/bash

# Full API test workflow
API_BASE="http://localhost:5001/api/v1"

# 1. Authenticate
echo "🔐 Authenticating..."
TOKEN=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' \
  | jq -r '.data.token')

echo "✅ Got token: ${TOKEN:0:30}..."

# 2. Create project
echo "📋 Creating project..."
PROJECT_RESPONSE=$(curl -s -X POST "$API_BASE/projects" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "API Test Project",
    "address": "123 Test Street, Bangkok, Thailand",
    "clientInfo": "Test Client"
  }')

PROJECT_ID=$(echo $PROJECT_RESPONSE | jq -r '.data.projectId')
echo "✅ Created project: $PROJECT_ID"

# 3. Get project details
echo "🔍 Getting project details..."
curl -s -X GET "$API_BASE/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN" | jq '.data.projectName'

# 4. Update project
echo "✏️ Updating project..."
curl -s -X PUT "$API_BASE/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "Updated API Test Project",
    "address": "456 Updated Street, Bangkok, Thailand",
    "status": "In Progress"
  }' | jq '.success'

echo "🎉 API test completed successfully!"
```

## ❓ Troubleshooting

| Issue | Solution |
|-------|----------|
| **401 Unauthorized** | Check your token or login again with test_admin |
| **403 Forbidden** | Your role doesn't have permission for this action |
| **404 Not Found** | Check the ID or endpoint path |
| **429 Too Many Requests** | You've exceeded the rate limit, wait and try again |
| **500 Server Error** | Check logs or contact system administrator |
| **Connection Refused** | Ensure API is running on correct port (5001 or 8080) |

## 🔄 Next Steps

1. **[Authentication Guide](./02_AUTHENTICATION.md)** - Complete authentication and user management
2. **[Project Management](./03_PROJECTS.md)** - Full project CRUD operations
3. **[Real-Time Features](./00_REAL_TIME_LIVE_UPDATES.md)** - SignalR live updates
4. **[User Management Guide](../USER_MANAGEMENT_GUIDE.md)** - Admin user creation and management

## 📚 Additional Resources

- **[Curl Commands Reference](../CURL_COMMANDS.md)** - Quick command reference
- **[Database Schema](../schema.sql)** - Complete database structure
- **[Implementation Guide](../IMPLEMENTATION_COMPLETE.md)** - Full system overview

---
*Last Updated: July 2025 - Solar Projects API v1.0*
*Last Updated: June 15, 2025*
