# API Access Guide - Solar Projects Database

## Overview
This guide shows how to access all project data through the Solar Projects REST API endpoints.

## Project Summary Statistics:
- **Total Projects**: 14
- **Planning Status**: 11 projects  
- **InProgress Status**: 3 projects
- **Managed by Admin users**: 13 projects
- **Managed by Manager users**: 1 project

## API Endpoints to Access Project Data

### 1. Get All Projects
```bash
# Get list of all projects
curl -X GET "http://localhost:5002/api/v1/projects" \
  -H "Content-Type: application/json"
```

### 2. Get Specific Project by ID
```bash
# Get specific project details
curl -X GET "http://localhost:5002/api/v1/projects/{project-id}" \
  -H "Content-Type: application/json"
```

### 3. Get Projects by Status
```bash
# Filter projects by status (if filtering is supported)
curl -X GET "http://localhost:5002/api/v1/projects?status=Planning" \
  -H "Content-Type: application/json"
```

### 4. Get Users (Project Managers)
```bash
# Get all users to see project managers
curl -X GET "http://localhost:5002/api/v1/users" \
  -H "Content-Type: application/json"
```

## Authentication Required

Most endpoints require authentication. First register/login:

### Register a User
```bash
curl -X POST "http://localhost:5002/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "api_user",
    "email": "api@test.com",
    "password": "TestPassword123!",
    "roleId": 4
  }'
```

### Login to Get Token
```bash
curl -X POST "http://localhost:5002/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "api_user",
    "password": "TestPassword123!"
  }'
```

### Use Token in Requests
```bash
# Use the token from login response
curl -X GET "http://localhost:5002/api/v1/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Sample Project Data Available

### Main Solar Projects:
1. **Downtown Solar Farm** (3 instances)
   - Status: Planning
   - Location: 1234 Solar Boulevard, Phoenix, AZ 85001
   - Managers: Solar Project Manager (Admin) × 2, Alex Project Manager (Manager) × 1

2. **Test Solar Installation** (3 instances)
   - Status: Planning  
   - Location: 123 Solar Street, Phoenix, AZ 85001
   - Manager: System Administrator (Admin)

### API Test Projects:
3. **Test Project - API Test Suite** (2 instances)
   - Status: Planning
   - Manager: Updated Patch Test User (Admin)

### Development/Test Projects:
4. **PATCH Test Project** (2 instances)
   - Status: InProgress / Planning
   - Manager: Updated Patch Test User (Admin)

## Complete API Testing Script

```bash
#!/bin/bash

API_BASE="http://localhost:5002/api/v1"

echo "=== Solar Projects API Data Access ==="
echo ""

# 1. Health check
echo "1. API Health Check:"
curl -s "$API_BASE/health" | jq '.'
echo ""

# 2. Get all projects
echo "2. All Projects:"
curl -s "$API_BASE/projects" | jq '.'
echo ""

# 3. Get all users
echo "3. All Users (Project Managers):"
curl -s "$API_BASE/users" | jq '.'
echo ""

# 4. Get daily reports
echo "4. Daily Reports:"
curl -s "$API_BASE/daily-reports" | jq '.'
echo ""

# 5. Get work requests  
echo "5. Work Requests:"
curl -s "$API_BASE/work-requests" | jq '.'
echo ""

# 6. Get calendar events
echo "6. Calendar Events:"
curl -s "$API_BASE/calendar" | jq '.'
echo ""

echo "=== API Data Access Complete ==="
```

## Role-Based Access Control

### Available Roles:
- **Admin** (roleId: 1): Full access to all data
- **Manager** (roleId: 2): Project management access  
- **User** (roleId: 3): Limited access to assigned projects
- **Viewer** (roleId: 4): Read-only access

### Sample Users in Database:
- **Admin Users**: 12 (system administrators)
- **Manager Users**: 4 (project managers, team leads)
- **User Role**: 5 (technicians, installers, field workers)
- **Viewer Role**: 4 (clients, auditors, observers)

## Project Management Operations

### Create New Project
```bash
curl -X POST "$API_BASE/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "New Solar Installation",
    "description": "New residential solar project",
    "address": "456 Solar Ave, Phoenix, AZ",
    "clientInfo": "New Client Corp",
    "status": "Planning",
    "startDate": "2025-08-01",
    "estimatedEndDate": "2025-11-30"
  }'
```

### Update Project
```bash
curl -X PUT "$API_BASE/projects/{project-id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Updated Project Name",
    "status": "InProgress"
  }'
```

### Delete Project
```bash
curl -X DELETE "$API_BASE/projects/{project-id}" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Database Direct Access (Alternative)

If you need direct database access:
```bash
# Access PostgreSQL directly
docker exec -it solar-projects-db psql -U postgres -d SolarProjectsDb

# Run our database access script
./access-project-data.sh
```

## Quick Test Commands

### Start the API (if not running)
```bash
dotnet run --urls "http://localhost:5002"
```

### Run All API Tests
```bash
./test-all-api-mock-data.sh
```

### Test Core Endpoints
```bash
./test-quick-endpoints.sh
```

## Project Status Distribution:
- **Planning**: 11 projects (79%)
- **InProgress**: 3 projects (21%)
- **Completed**: 0 projects

## Notes:
- API runs on `http://localhost:5002`
- PostgreSQL database accessible via Docker container `solar-projects-db`
- All 14 projects are realistic solar installation projects
- Authentication tokens may be required for most endpoints
- Use `jq` for JSON formatting in curl responses
