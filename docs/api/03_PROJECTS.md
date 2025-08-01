# Project Management

**⚡ Real-Time Live Updates**: All project operations support real-time data synchronization. When any user creates, updates, or deletes projects, all connected users see changes immediately.

Project management endpoints for the Solar Projects API.

**Authentication Required**: All endpoints require JWT authentication  
**Role Required**: Admin/Manager (full CRUD), User/Viewer (read-only)  
**📡 Live Updates**: SignalR WebSocket broadcasting for all operations

## 📊 Total Project Information

**Quick Access to System-Wide Statistics:**
- **GET** `/api/v1/projects/analytics` - Dedicated endpoint for comprehensive project statistics
- **GET** `/api/v1/projects` - Main projects endpoint includes real-time statistics in response
- **Real-time Statistics**: All totals are calculated dynamically and reflect current system state

**Available Statistics:**
- Total projects across all statuses
- Active, completed, planning, on-hold, and cancelled project counts
- Total system capacity (kW) and PV modules
- Financial totals (FTS, revenue, PQM values)
- Geographic coverage and project manager distribution

## Admin & Manager Capabilities

- Create new projects
- Update all project fields
- Modify project assignments and team configurations  
- Update technical specifications
- Edit location coordinates and connection details
- Change project status and timelines
- Delete projects (Admin only)

## Update Permissions

Admin & Manager roles have full update access to all project data fields:

| Data Category | Fields | API Endpoints |
|---------------|--------|---------------|
| Basic Info | `projectName`, `address`, `clientInfo` | PUT/PATCH `/projects/{id}` |
| Timeline | `startDate`, `estimatedEndDate`, `actualEndDate` | PUT/PATCH `/projects/{id}` |
| Team | `projectManagerId`, `team` assignments | PUT/PATCH `/projects/{id}` |
| Technical | `totalCapacityKw`, `pvModuleCount`, `connectionType` | PUT/PATCH `/projects/{id}` |
| Equipment | Inverter details and specifications | PUT/PATCH `/projects/{id}` |
| Financial | `ftsValue`, `revenueValue`, `pqmValue` | PUT/PATCH `/projects/{id}` |
| Location | `locationCoordinates.latitude`, `longitude` | PUT/PATCH `/projects/{id}` |
| Status | Project status and workflow management | PUT/PATCH `/projects/{id}` |

**Admin-Only Operations:**
- Project deletion (`DELETE /projects/{id}`)

**User & Viewer Access:**
- Read-only access to all project data
- Can submit reports related to their work

## Get All Projects

**GET** `/api/v1/projects`

Retrieve a paginated list of projects with filtering, sorting, and field selection.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status 
- `search` (string): Search in project name or description
- `sortBy` (string): Sort field
- `sortOrder` (string): Sort direction ("asc" or "desc")
- `fields` (string): Comma-separated list of fields to include
- `managerId` (Guid): Filter by project manager ID

**Success Response (200)**:
*JSON response removed - includes the following structure:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Object containing:
  - `items`: Array of project objects with:
    - Basic information (projectId, projectName, address, clientInfo)
    - Status and timeline (status, startDate, estimatedEndDate, actualEndDate)
    - Project manager details (userId, username, email, fullName, roleName)
    - Progress tracking (taskCount, completedTaskCount)
    - Technical details (team, connectionType, totalCapacityKw, pvModuleCount)
    - Equipment specifications (inverter details)
    - Financial information (ftsValue, revenueValue, pqmValue)
    - Location coordinates (latitude, longitude)
    - Timestamps (createdAt, updatedAt)
  - `pagination`: Pagination metadata with:
    - `totalCount`: Total number of projects in the system
    - `pageNumber`: Current page number
    - `pageSize`: Number of items per page
    - `totalPages`: Total number of pages available
    - `hasPreviousPage`: Boolean indicating if previous page exists
    - `hasNextPage`: Boolean indicating if next page exists
  - `statistics`: **Enhanced project statistics** including:
    - `totalProjects`: Total number of projects across all statuses
    - `activeProjects`: Number of projects currently in progress
    - `completedProjects`: Number of successfully completed projects
    - `planningProjects`: Number of projects in planning phase
    - `onHoldProjects`: Number of projects temporarily on hold
    - `cancelledProjects`: Number of cancelled projects
    - `totalCapacityKw`: Total system capacity in kilowatts
    - `totalPvModules`: Total number of PV modules
    - `totalFtsValue`: Total FTS value across all projects
    - `totalRevenueValue`: Total revenue value across all projects
    - `totalPqmValue`: Total PQM value across all projects
    - `projectManagerCount`: Number of unique project managers
    - `geographicCoverage`: Geographic coverage summary
    - `lastUpdated`: Statistics calculation timestamp
- `errors`: Array of validation errors

**Enhanced Project Statistics:**
The API now provides comprehensive project statistics in real-time:
- **Total Projects**: Complete count across all statuses
- **Active Projects**: Currently in progress
- **Completed Projects**: Successfully finished projects
- **Planning Projects**: Projects in planning phase
- **On Hold Projects**: Temporarily suspended projects
- **Cancelled Projects**: Terminated projects
- **Total Capacity**: Combined capacity across all projects (in kW)
- **Total PV Modules**: Sum of all PV modules across projects
- **Financial Metrics**: Total FTS, revenue, and PQM values
- **Geographic Coverage**: Multi-province and regional distribution
- **Project Manager Statistics**: Unique manager count and distribution

*Note: All statistics are calculated in real-time and reflect the current state of the system.*

## Get Project by ID

**GET** `/api/v1/projects/{id}`

Retrieve detailed information about a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
*JSON response removed - includes detailed project information with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Complete project object containing:
  - Basic details (projectId, projectName, address, clientInfo)
  - Status and timeline information (status, startDate, estimatedEndDate, actualEndDate)
  - Project manager details with role information (userId, username, email, fullName, roleName, isActive)
  - Technical specifications (team, connectionType, connectionNotes, totalCapacityKw, pvModuleCount)
  - Equipment details (inverter configurations for 125kW, 80kW, 60kW, 40kW units)
  - Financial values (ftsValue, revenueValue, pqmValue)
  - Geographic coordinates (latitude, longitude)
  - Creation and update timestamps (createdAt, updatedAt)
- `errors`: Array of validation errors

**Note**: For comprehensive project statistics and total project counts across the system, use the `/api/v1/projects/analytics` endpoint or the main `/api/v1/projects` endpoint which includes real-time statistics in the response.

## Get My Projects

**GET** `/api/v1/projects/me`

Get projects associated with the current authenticated user.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Success Response (200)**:
*JSON response removed - includes user's associated projects with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Object containing:
  - `items`: Array of projects associated with the authenticated user
  - Pagination metadata (totalCount, pageNumber, pageSize, navigation flags)
  - Each project includes complete project details similar to the get-by-id response
- `errors`: Array of validation errors

## Get Project Status

**GET** `/api/v1/projects/{id}/status`

Get real-time status information for a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
*JSON response removed - includes real-time project status with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Project status object containing:
  - Basic project identification (projectId, projectName)
  - Current status information
  - Timeline details (planned and actual dates)
  - Progress metrics (completion percentage, schedule adherence)
  - Task summary (active, completed, total counts)
  - Update timestamp
  - Related resource links (project details, master plans, tasks, documents)
- `errors`: Array of validation errors

## Create New Project

**POST** `/api/v1/projects`

**🔒 Required Roles**: Admin, Manager

Create a new solar installation project with detailed information.

**Request Body**:
*JSON request body removed - includes the following required and optional fields:*

- `projectName`: Project name (string, required)
- `address`: Project location address (string, required)
- `clientInfo`: Client information (string, optional)
- `startDate`: Project start date (ISO 8601 format, required)
- `estimatedEndDate`: Estimated completion date (ISO 8601 format, optional)
- `projectManagerId`: Project manager identifier (GUID, required)
- `team`: Team assignment (string, optional)
- `connectionType`: Electrical connection type (string, optional)
- `connectionNotes`: Connection details (string, optional)
- `totalCapacityKw`: Total capacity in kilowatts (decimal, optional)
- `pvModuleCount`: Number of PV modules (integer, optional)
- `equipmentDetails`: Equipment specifications object with inverter details
- `ftsValue`: FTS financial value (decimal, optional)
- `revenueValue`: Revenue value (decimal, optional)
- `pqmValue`: PQM value (decimal, optional)
- `locationCoordinates`: Geographic coordinates object with latitude and longitude

**Success Response (201)**:
*JSON success response removed - includes created project details with:*

- `success`: Operation status (boolean)
- `message`: Confirmation message
- `data`: Created project object containing all project details including:
  - Generated project ID
  - All submitted project information
  - Default status ("Planning")
  - Project manager details
  - Initial task and progress counts (0 for new projects)
  - Creation timestamp
- `errors`: Array of validation errors (empty on success)

## Update Project

**PUT** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update all fields of an existing project.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body**:
*JSON request body removed - includes all project fields that can be updated:*

- All fields from the create request (projectName, address, clientInfo, etc.)
- Additional fields: `status`, `actualEndDate`
- Equipment details and financial information
- Location coordinates and technical specifications
- Note: All fields are typically required for PUT operations

**Success Response (200)**:
*JSON success response removed - includes updated project details with:*

- `success`: Operation status (boolean)
- `message`: Confirmation message
- `data`: Updated project object with all current field values including:
  - Modified project information
  - Updated timestamp
  - Current progress and task counts
  - All technical and financial details
- `errors`: Array of validation errors (empty on success)

## Partially Update Project

**PATCH** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update specific fields of an existing project without affecting other fields.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body** (All fields are optional):
*JSON request body removed - includes optional fields for partial updates:*

- Any subset of project fields can be included
- Only specified fields will be updated
- Common fields: `projectName`, `address`, `clientInfo`, `status`
- Timeline fields: `startDate`, `estimatedEndDate`, `actualEndDate`
- Assignment fields: `projectManagerId`
- Technical fields: capacity, equipment details, location coordinates

**Success Response (200)**:
*JSON success response removed - includes updated project with modified fields and current status*

## Delete Project

**DELETE** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin only

Delete a project and all associated data (tasks, reports, etc.).

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
*JSON success response removed - includes confirmation of successful deletion*

**Error Response (403)**:
*JSON error response removed - includes insufficient permissions message for non-admin users*

## Project Statuses

| Status | Description | Allowed Transitions |
|--------|-------------|---------------------|
| **Planning** | Initial project setup | InProgress, Cancelled |
| **InProgress** | Current work in progress | OnHold, Completed |
| **OnHold** | Temporarily suspended | InProgress, Cancelled |
| **Completed** | All work finished | (Final state) |
| **Cancelled** | Project terminated | (Final state) |

## 📊 Project Analytics

**GET** `/api/v1/projects/analytics`

**🎯 Primary endpoint for total project information and comprehensive statistics.** This endpoint provides real-time calculated statistics across all projects in the system.

**Key Features:**
- **Real-time calculations**: All statistics reflect current system state
- **Comprehensive coverage**: Includes all project statuses and financial data
- **Performance optimized**: Cached for 1 hour for optimal performance
- **No query parameters**: Simple GET request returns all available statistics

**Success Response (200)**:
*JSON response removed - includes comprehensive project analytics with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: **Complete project statistics object** containing:
  - `totalProjects`: **Total number of projects in the system**
  - `activeProjects`: Number of projects currently in progress
  - `completedProjects`: Number of successfully completed projects
  - `planningProjects`: Number of projects in planning phase
  - `onHoldProjects`: Number of projects temporarily on hold
  - `cancelledProjects`: Number of cancelled projects
  - `totalCapacityKw`: **Total capacity across all projects in kilowatts**
  - `totalPvModules`: **Total number of PV modules across all projects**
  - `totalFtsValue`: **Total FTS value across all projects**
  - `totalRevenueValue`: **Total revenue value across all projects**
  - `totalPqmValue`: **Total PQM value across all projects**
  - `projectManagerCount`: Number of unique project managers
  - `geographicCoverage`: Geographic coverage summary
  - `lastUpdated`: Timestamp of when statistics were calculated
- `errors`: Array of validation errors

**Usage Examples:**
- Dashboard displays showing total project counts
- Financial reporting with total values
- Capacity planning and resource allocation
- Performance monitoring and KPI tracking

## Project Performance Tracking

**GET** `/api/v1/projects/{id}/performance`

Track detailed performance metrics for a specific project.

**Path Parameters**:
- `id` (guid) - Project ID

**Success Response (200)**:
*JSON response removed - includes detailed project performance metrics with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Performance object containing:
  - Project identification and overall performance score
  - `kpis`: Key performance indicators (timeline adherence, budget, quality, safety, satisfaction)
  - `milestones`: Project milestones with target vs actual dates and variance
  - `resourceUtilization`: Team, equipment, and material efficiency metrics
  - `riskAssessment`: Risk level, active/mitigated risks, and trend analysis
  - `progressHistory`: Historical progress data with completion percentages and issues
- `errors`: Array of validation errors

## Project Status Workflow

**PATCH** `/api/v1/projects/{id}/status`

Update project status with workflow validation.

**Path Parameters**:
- `id` (guid) - Project ID

**Request Body**:
*JSON request body removed - includes the following fields:*

- `status`: New project status (string, required)
- `reason`: Reason for status change (string, optional)
- `effectiveDate`: When status change takes effect (ISO 8601 format, optional)
- `notifyStakeholders`: Whether to send notifications (boolean, optional)

**Success Response (200)**:
*JSON response removed - includes status change confirmation with:*

- `success`: Operation status (boolean)
- `message`: Confirmation message
- `data`: Status change details including:
  - Project ID and status transition (previous/new status)
  - Effective date and updating user information
  - Notification results (sent count, failed count, recipient types)
- `errors`: Array of validation errors

## Project Templates

**GET** `/api/v1/projects/templates`

Get available project templates for quick project creation.

**Success Response (200)**:
*JSON response removed - includes available project templates with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Templates object containing:
  - `templates`: Array of template objects with:
    - Basic template information (ID, name, description, category)
    - Estimated duration and usage statistics
    - Default tasks with phases and estimated hours
    - Required equipment lists
- `errors`: Array of validation errors

**POST** `/api/v1/projects/from-template/{templateId}`

Create a new project from a template.

**Path Parameters**:
- `templateId` (guid) - Template ID

**Request Body**:
*JSON request body removed - includes the following fields:*

- `projectName`: Name for the new project (string, required)
- `address`: Project location (string, required)
- `clientInfo`: Client information (string, required)
- `totalCapacityKw`: Project capacity (decimal, optional)
- `projectManagerId`: Project manager assignment (GUID, required)
- `startDate`: Project start date (ISO 8601 format, required)
- `customizations`: Template customization object with:
  - `skipTasks`: Array of task IDs to skip from template
  - `additionalTasks`: Array of custom tasks to add

## Advanced Project Search

**GET** `/api/v1/projects/search`

Advanced search with full-text search and filters.

**Query Parameters**:
- `q` (string): Search query
- `filters` (object): Advanced filter criteria
- `coordinates` (string): Location-based search (lat,lng,radius)
- `dateRange` (string): Date range filter
- `facets` (bool): Include facet information

**Success Response (200)**:
*JSON response removed - includes advanced search results with:*

- `success`: Operation status (boolean)
- `message`: Response message
- `data`: Search results object containing:
  - `query`: Original search query
  - `results`: Array of matching projects with relevance scores and highlighted matches
  - `facets`: Categorized counts for filtering (status, capacity, location)
  - `suggestions`: Related search suggestions
  - `totalResults`: Total number of matching projects
  - `searchTime`: Query execution time in seconds
- `errors`: Array of validation errors

## Project Error Codes

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
| **PRJ009** | Template not found | Verify template ID exists |
| **PRJ010** | Invalid search query | Check search syntax and parameters |
| **PRJ011** | Location coordinates invalid | Provide valid latitude and longitude |
| **PRJ012** | Performance data unavailable | Project must have activity to show performance |

## Summary

### Key Features
- **Comprehensive CRUD Operations**: Full project lifecycle management
- **📊 Total Project Analytics**: Real-time statistics via `/api/v1/projects/analytics`
- **Enhanced Project Lists**: Main `/api/v1/projects` includes total project statistics
- **Advanced Search & Filtering**: Powerful search capabilities with facets
- **Performance Analytics**: Detailed metrics and KPI tracking
- **Template System**: Quick project creation from predefined templates
- **Status Workflow**: Structured status transitions with validation
- **Real-time Notifications**: Automatic stakeholder notifications
- **Geographic Integration**: Location-based project management

### Integration Points
- **Master Plans**: Automatic master plan creation for new projects
- **Task Management**: Linked task creation and tracking
- **Daily Reports**: Progress reporting tied to projects
- **File Management**: Document and image storage per project
- **Calendar Integration**: Project milestones and schedule management

### Best Practices
- Use templates for consistent project setup
- Maintain accurate project status throughout lifecycle
- Regular performance monitoring and reporting
- Proper role-based access control implementation
- Geographic data for location-based insights
- Regular analytics review for process improvement

---
*Last Updated: July 11, 2025*
