# Quick Start Guide

Get started with the Solar Projects API quickly.

## Quick Start Checklist

- Deploy API & Database: Docker containers running on `http://localhost:5002`
- Authenticate: Get JWT token with valid credentials
- Make API Calls: Use token in Authorization header
- Review Responses: Check success/error fields in JSON
- Handle Errors: Use appropriate error codes for troubleshooting

## Authentication

1. **Get your authentication token**:

```bash
curl -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test_admin","password":"Admin123!"}'
```

2. **Save your token** from the response and **use the token** in subsequent requests:

```bash
curl -X GET http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## API Base URLs

| Environment | Base URL | Status |
|-------------|----------|--------|
| **Development** | `http://localhost:5002/api/v1` | ✅ Active |
| **Production** | `https://your-domain.com/api/v1` | 🔧 Configure |

## 📌 Core Endpoints

### Projects

- `GET /projects` - Get all projects
- `GET /projects/{id}` - Get project by ID
- `POST /projects` - Create new project (Admin/Manager)
- `PUT /projects/{id}` - Update project (Admin/Manager)
- `DELETE /projects/{id}` - Delete project (Admin only)

### Master Plans

- `GET /master-plans/project/{projectId}` - Get master plan by project
- `POST /master-plans` - Create master plan (Admin/Manager)
- `PUT /master-plans/{id}` - Update master plan (Admin/Manager)

### Tasks

- `GET /tasks?projectId={projectId}` - Get tasks for project
- `POST /tasks` - Create new task (Admin/Manager)
- `PUT /tasks/{id}` - Update task (Admin/Manager/Assignee)

## 📦 Current System Status

- **Current Project Count**: 97 solar projects
- **Database**: PostgreSQL database with EF Core
- **Authentication**: JWT tokens with role-based access
- **Rate Limiting**: 100 requests per minute

## 📈 Common Use Cases

### 1. Create a New Project

```bash
curl -X POST http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "New Solar Installation",
    "address": "123 Solar Street",
    "clientInfo": "Client Company Name",
    "totalCapacityKw": 150.5,
    "projectManagerId": "manager-guid-here"
  }'
```

### 2. Get All Projects with Pagination

```bash
curl -X GET "http://localhost:5002/api/v1/projects?pageNumber=1&pageSize=20&status=Active" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. Update Project Status

```bash
curl -X PATCH http://localhost:5002/api/v1/projects/project-id-here \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "InProgress"
  }'
```

## ❓ Troubleshooting

| Issue | Solution |
|-------|----------|
| **401 Unauthorized** | Check your token or login again |
| **403 Forbidden** | Your role doesn't have permission for this action |
| **404 Not Found** | Check the ID or endpoint path |
| **429 Too Many Requests** | You've exceeded the rate limit, wait and try again |
| **500 Server Error** | Check logs or contact system administrator |

## 🔄 Next Steps

1. Review the [Authentication Guide](./02_AUTHENTICATION.md) for details on user management
2. Explore [Project Management](./03_PROJECTS.md) for project operations
3. Learn about [Master Plans](./04_MASTER_PLANS.md) for detailed project planning

---
*Last Updated: June 15, 2025*
