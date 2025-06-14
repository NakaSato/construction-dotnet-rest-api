# 🚀 How to Access Solar Projects Data

## Project Summary Statistics:
- **Total Projects**: 14
- **Planning Status**: 11 projects (79%)
- **InProgress Status**: 3 projects (21%)
- **Managed by Admin users**: 13 projects
- **Managed by Manager users**: 1 project

## 📡 Method 1: REST API Access (Recommended)

### Start the API:
```bash
# Option 1: Use VS Code Task
# Press Ctrl+Shift+P → "Tasks: Run Task" → "Run API"

# Option 2: Command Line
dotnet run --urls "http://localhost:5002"
```

### Get All Projects:
```bash
curl -X GET "http://localhost:5002/api/v1/projects" \
  -H "Content-Type: application/json"
```

### Get Specific Project:
```bash
curl -X GET "http://localhost:5002/api/v1/projects/{project-id}" \
  -H "Content-Type: application/json"
```

### Get All Users (Project Managers):
```bash
curl -X GET "http://localhost:5002/api/v1/users" \
  -H "Content-Type: application/json"
```

## 🔐 Authentication (Required for Most Endpoints)

### 🧪 Ready-to-Use Test Accounts

We've created dedicated test accounts for each role:

| Role | Username | Password | Email | Permissions |
|------|----------|----------|-------|-------------|
| 👨‍💼 Admin | `test_admin` | `Admin123!` | test_admin@example.com | Full system access |
| 👩‍💼 Manager | `test_manager` | `Manager123!` | test_manager@example.com | Project management |
| 👨‍🔧 User | `test_user` | `User123!` | test_user@example.com | Field operations |
| 👁️ Viewer | `test_viewer` | `Viewer123!` | test_viewer@example.com | Read-only access |

### 1. Login with Test Account (Easiest):
```bash
curl -X POST "http://localhost:5002/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

### 2. Register a New User (Alternative):
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

### 3. Login to Get Token:
```bash
curl -X POST "http://localhost:5002/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "api_user",
    "password": "TestPassword123!"
  }'
```

### 4. Use Token in Requests:
```bash
curl -X GET "http://localhost:5002/api/v1/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 🗄️ Method 2: Direct Database Access

### Using Our Database Script:
```bash
./access-project-data.sh
```

### Direct PostgreSQL Access:
```bash
docker exec -it solar-projects-db psql -U postgres -d SolarProjectsDb
```

### Sample SQL Queries:
```sql
-- Get all projects
SELECT "ProjectName", "Status", "StartDate" FROM "Projects";

-- Get projects by status
SELECT * FROM "Projects" WHERE "Status" = 'Planning';

-- Get project managers
SELECT u."FullName", r."RoleName", COUNT(p."ProjectId") as project_count
FROM "Users" u 
JOIN "Roles" r ON u."RoleId" = r."RoleId"
LEFT JOIN "Projects" p ON u."UserId" = p."ProjectManagerId"
GROUP BY u."UserId", u."FullName", r."RoleName";
```

## 🧪 Method 3: Use Existing Test Scripts

### Comprehensive API Testing:
```bash
./test-all-api-mock-data.sh     # Full API test with realistic data
./test-project-management.sh    # Project CRUD operations
./test-quick-endpoints.sh       # Quick core functionality test
./api-data-access-demo.sh       # API data access demonstration
```

## 📊 Available Project Data

### Main Solar Projects:
1. **Downtown Solar Farm** (3 instances)
   - Status: Planning
   - Location: 1234 Solar Boulevard, Phoenix, AZ 85001
   - Managers: Solar Project Manager (Admin) × 2, Alex Project Manager (Manager) × 1

2. **Test Solar Installation** (3 instances + 1 project)
   - Status: Planning
   - Location: 123 Solar Street, Phoenix, AZ 85001
   - Manager: System Administrator (Admin)

### Development/Test Projects:
3. **PATCH Test Project** (2 instances)
   - Status: InProgress / Planning
   - Manager: Updated Patch Test User (Admin)

4. **API Test Suite Projects** (2 instances)
   - Status: Planning
   - Manager: Updated Patch Test User (Admin)

## 👥 User/Manager Data (25 total users)

### Role Distribution:
- **Admin**: 12 users (system administrators, senior managers)
- **Manager**: 4 users (project managers, team leads, supervisors)
- **User**: 5 users (technicians, installers, field workers)  
- **Viewer**: 4 users (clients, auditors, observers)

### Sample Managers:
- Alex Project Manager (Manager role)
- Solar Project Manager (Admin role)
- System Administrator (Admin role)
- Emma Team Lead (Manager role)
- David Site Supervisor (Manager role)

## 🎯 Quick Access Commands

### Start Everything:
```bash
# 1. Start PostgreSQL (if not running)
docker-compose up -d postgres

# 2. Start API
dotnet run --urls "http://localhost:5002"

# 3. Test API
curl "http://localhost:5002/api/v1/projects"
```

### One-Command Data Access:
```bash
# Get database summary
./access-project-data.sh

# Or test all API endpoints
./test-all-api-mock-data.sh
```

## 🔧 Troubleshooting

### If API doesn't start:
```bash
# Check if PostgreSQL is running
docker ps | grep solar-projects-db

# Start PostgreSQL if needed
docker-compose up -d postgres

# Check API logs
dotnet run --urls "http://localhost:5002"
```

### If Database connection fails:
```bash
# Check database container
docker exec -it solar-projects-db psql -U postgres -d SolarProjectsDb -c "SELECT 1;"
```

## 📈 Summary
- **14 projects** available via API endpoints
- **25 users** across 4 role types
- **Multiple access methods**: REST API, Direct DB, Test Scripts
- **Realistic solar industry data** for testing and development
