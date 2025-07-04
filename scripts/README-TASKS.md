# 📋 Solar Projects Tasks API Scripts

This directory contains comprehensive bash scripts for interacting with the Solar Projects Task Management API. The scripts provide role-based access testing and easy task management operations.

## 🚀 Quick Start

1. **Ensure the API server is running:**
   ```bash
   dotnet run --urls "http://localhost:5001"
   ```

2. **Make scripts executable:**
   ```bash
   chmod +x scripts/test-tasks-api.sh scripts/quick-tasks.sh scripts/test-tasks-permissions.sh
   ```

3. **Start with the quick operations script:**
   ```bash
   ./scripts/quick-tasks.sh help
   ```

## 📁 Available Scripts

### 1. `quick-tasks.sh` - Interactive Task Operations

**Purpose:** Simple command-line interface for common task operations

**Usage:**
```bash
./scripts/quick-tasks.sh [command] [options...]
```

**Available Commands:**
- `list` - List all tasks
- `list-project [project-id]` - List tasks for specific project
- `create [project-id]` - Create a new task interactively
- `view [task-id]` - View specific task details
- `update-status [task-id]` - Update task status interactively
- `assign [task-id] [user-id]` - Assign task to user
- `delete [task-id]` - Delete a task
- `progress [task-id]` - Create progress report

**Examples:**
```bash
# List all tasks
./scripts/quick-tasks.sh list

# Create a new task
./scripts/quick-tasks.sh create 1c91433d-fc43-40d9-a49b-e9f2d2a9e922

# Update task status
./scripts/quick-tasks.sh update-status cf8939ad-b21c-4c36-8cf9-a4f6a8cccd3d

# View task details
./scripts/quick-tasks.sh view cf8939ad-b21c-4c36-8cf9-a4f6a8cccd3d

# List tasks for a specific project
./scripts/quick-tasks.sh list-project 1c91433d-fc43-40d9-a49b-e9f2d2a9e922
```

### 2. `test-tasks-api.sh` - Comprehensive API Testing

**Purpose:** Complete testing of all Task API endpoints with different user roles

**Usage:**
```bash
./scripts/test-tasks-api.sh
```

**Features:**
- ✅ Tests all CRUD operations
- ✅ Tests different user role permissions
- ✅ Tests task assignment and status updates
- ✅ Tests progress report functionality
- ✅ Tests advanced search and filtering
- ✅ Generates detailed logs and summary reports

**What it tests:**
- Task creation (Admin/Manager)
- Task assignment to technicians
- Task viewing (All users)
- Status updates (Users + Assignees)
- Task updates (Admin/Manager/Supervisor)
- Advanced search and filtering
- Progress report management
- Permission restrictions testing
- Task deletion (Admin/Manager only)

### 3. `test-tasks-permissions.sh` - Role-Based Access Control Testing

**Purpose:** Validates role-based permissions according to API documentation

**Usage:**
```bash
./scripts/test-tasks-permissions.sh
```

**Role Testing Matrix:**

| Role | Create | View | Update | Assign | Delete | Status Update |
|------|--------|------|--------|--------|--------|---------------|
| **Admin** | ✅ All | ✅ All | ✅ All | ✅ All | ✅ All | ✅ All |
| **Manager** | ✅ All | ✅ All | ✅ All | ✅ All | ✅ All | ✅ All |
| **Supervisor** | ✅ Managed | ✅ Managed | ✅ Managed | ✅ Managed | ❌ None | ✅ Managed |
| **Technician** | ❌ None | ✅ Assigned | ❌ None | ❌ None | ❌ None | ✅ Assigned |
| **General User** | ❌ None | ✅ Accessible | ❌ None | ❌ None | ❌ None | ❌ None |

**Features:**
- ✅ Tests each role's permissions individually
- ✅ Validates expected success/failure responses
- ✅ Generates pass/fail report for each test
- ✅ Documents expected vs actual behavior
- ✅ Saves detailed logs for debugging

## 🔧 API Endpoints Tested

All scripts interact with these Task Management API endpoints:

### Core Task Operations
- `GET /api/v1/tasks` - List all tasks with pagination
- `GET /api/v1/tasks/{id}` - Get specific task details
- `POST /api/v1/tasks` - Create new task
- `PUT /api/v1/tasks/{id}` - Full task update
- `PATCH /api/v1/tasks/{id}` - Partial task update
- `DELETE /api/v1/tasks/{id}` - Delete task

### Task Status Management
- `PATCH /api/v1/tasks/{id}/status` - Update task status only

### Advanced Features
- `GET /api/v1/tasks/advanced` - Advanced search with filtering
- `POST /api/v1/tasks/{id}/progress-reports` - Create progress report
- `GET /api/v1/tasks/{id}/progress-reports` - Get progress reports

## 🎯 Task Status Values

The API supports these task statuses:

| Status | Description | Usage |
|--------|-------------|-------|
| `NotStarted` | Task created but work not begun | Initial state |
| `InProgress` | Work has started | Active work |
| `Completed` | Task finished successfully | Work done |
| `OnHold` | Temporarily paused | Waiting for resources |
| `Cancelled` | Task was cancelled | No longer needed |

## 🔐 Authentication

All scripts use the following authentication:

- **Admin User:** `test_admin` / `Admin123!`
- **Manager User:** Retrieved from `manager_login.json` if available
- **Other Roles:** Uses admin token (for demo purposes)

In production, each role would have separate credentials.

## 📊 Example Workflow

### 1. Basic Task Management
```bash
# Start by listing existing tasks
./scripts/quick-tasks.sh list

# Get a project ID (from the projects you imported earlier)
PROJECT_ID="1c91433d-fc43-40d9-a49b-e9f2d2a9e922"

# Create a new task
./scripts/quick-tasks.sh create $PROJECT_ID
# Follow prompts to enter task details

# Update task status
./scripts/quick-tasks.sh update-status [task-id]

# View updated task
./scripts/quick-tasks.sh view [task-id]
```

### 2. Comprehensive Testing
```bash
# Run full API test suite
./scripts/test-tasks-api.sh

# Run role-based permission tests
./scripts/test-tasks-permissions.sh

# Check logs for detailed results
ls scripts/test-logs/
```

### 3. Project-Specific Task Management
```bash
# List tasks for a specific project
./scripts/quick-tasks.sh list-project $PROJECT_ID

# Create multiple tasks for the project
for i in {1..3}; do
    echo -e "Task $i\nDescription for task $i\n2025-08-$(printf "%02d" $((15+i)))T10:00:00Z\n" | \
    ./scripts/quick-tasks.sh create $PROJECT_ID
done
```

## 📝 Log Files

All scripts generate detailed logs in the `scripts/test-logs/` directory:

- `tasks_api_test_YYYYMMDD_HHMMSS.log` - Comprehensive API test logs
- `tasks_permissions_YYYYMMDD_HHMMSS.log` - Permission test logs
- Individual operation logs for troubleshooting

## 🚨 Error Handling

The scripts handle common scenarios:

- **API Server Down:** Scripts check health endpoint before proceeding
- **Authentication Failures:** Clear error messages for auth issues
- **Invalid Task IDs:** Proper error handling for non-existent tasks
- **Permission Denied:** Expected behavior for unauthorized operations
- **Malformed JSON:** Robust parsing with fallback error display

## 🔧 Customization

### Adding New Operations

To add new task operations to `quick-tasks.sh`:

1. Add the command to the usage function
2. Create a `cmd_[operation]` function
3. Add the case to the main switch statement

### Modifying Test Scenarios

To customize `test-tasks-api.sh`:

1. Add new test functions following the existing pattern
2. Call them from the main execution flow
3. Update the summary generation

### Role-Based Testing

To add new roles to `test-tasks-permissions.sh`:

1. Add authentication for the new role
2. Create test functions for role-specific scenarios
3. Update the test report generation

## 📋 Requirements

- **bash** - Shell environment
- **curl** - HTTP client for API calls
- **jq** - JSON processor for parsing responses
- **Solar Projects API** - Running on http://localhost:5001

## 🎉 Success Examples

When everything works correctly, you should see:

```bash
$ ./scripts/quick-tasks.sh list
ℹ Authenticating as admin...
✓ Authenticated successfully
ℹ Fetching all tasks...
ID: cf8939ad-b21c-4c36-8cf9-a4f6a8cccd3d | Install Solar Panels | Status: InProgress | Project: Admin Created Project
✓ Found 1 tasks

$ ./scripts/test-tasks-permissions.sh
🔐 ROLE-BASED PERMISSIONS TEST
✓ PASS - Create Task (Admin)
✓ PASS - View All Tasks (Admin)
✓ PASS - Update Task (Admin)
🎉 All permission tests passed!
```

---

*These scripts provide a complete toolkit for managing and testing the Solar Projects Task Management API. They demonstrate proper role-based access control and provide practical examples for all supported operations.*
