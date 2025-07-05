# Master Plan Management

**⚡ Real-Time Live Updates**: This API supports real-time data synchronization. All changes are instantly broadcast to connected users via WebSocket.


**Authentication Required**: All endpoints require JWT authentication  
**Role Required**: Admin/Manager (full CRUD), User/Viewer (read-only)

Master plans serve as the central planning schedule that coordinates all schedules within a solar project. Each master plan belongs to a project and acts as the comprehensive scheduling framework.

## Project-Master Plan Relationship

- **Project** is the primary business entity containing high-level project information
- **Master Plan** belongs to a Project (1:1 relationship) and serves as the central planning schedule
- A Project can have only one active Master Plan at a time
- Master Plans coordinate and contain all project schedules:
  - Phase Schedules: Detailed timelines for each project phase
  - Task Schedules: Individual work item timelines and dependencies
  - Milestone Schedules: Key deliverable and checkpoint dates
  - Resource Schedules: Personnel, equipment, and material allocation
  - Approval Schedules: Workflow and approval process timelines

## Administrator & Project Manager Capabilities

- Create new master plans for projects
- Update plan details (name, description, dates, budget)
- Manage plan status and versioning
- Coordinate all project schedules within the master plan
- Track progress across all integrated schedules
- Resolve schedule conflicts and optimize resource allocation
- Generate comprehensive scheduling reports
- Approve and submit plans for execution

## User & Viewer Access

- Read-only access to approved master plans and all contained schedules
- Cannot modify plan information
- Can view project progress and milestone tracking
- Access to schedule-based reports

## Master Plan as Central Scheduling Framework

The Master Plan serves as the central planning schedule for all schedules within a project.

### Integrated Schedule Management
- **Phase Schedules**: Breaks down the project into manageable time-based phases
- **Task Schedules**: Individual work items with start/end dates and dependencies
- **Milestone Schedules**: Key deliverables and checkpoint dates across all phases
- **Resource Schedules**: Personnel, equipment, and material allocation timelines
- **Approval Schedules**: Workflow deadlines and approval process timelines

### ⚡ Schedule Coordination Features
- **Dependency Management**: Links between different schedule components
- **Conflict Resolution**: Identifies and resolves scheduling conflicts across all schedules
- **Resource Optimization**: Prevents double-booking and optimizes resource utilization
- **Timeline Synchronization**: Ensures all sub-schedules align with master timeline
- **Progress Tracking**: Unified progress monitoring across all schedule components

### 📊 Schedule Analytics
- **Critical Path Analysis**: Identifies bottlenecks across all integrated schedules
- **Resource Utilization**: Tracks allocation efficiency across all schedule types
- **Schedule Performance**: Monitors adherence to planned vs. actual timelines
- **Variance Analysis**: Identifies deviations from planned schedules

## 🏗️ Create Master Plan

**POST** `/api/v1/master-plans`

**🔒 Required Roles**: Administrator, ProjectManager

Create a new master plan for an existing project with comprehensive scheduling framework. The master plan serves as the central planning schedule that will coordinate all project activities, timelines, and resource allocation schedules.

**⚠️ Prerequisites**: 
- A valid Project must exist before creating a Master Plan
- Each Project can only have one active Master Plan at a time
- Master Plan becomes the central scheduling authority for all project activities

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1",
  "description": "Central planning schedule for residential solar installation covering all project schedules from site preparation through final commissioning",
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-09-30T00:00:00Z",
  "budget": 275000.00,
  "priority": "High",
  "notes": "Master schedule includes all sub-schedules: phase schedules, task schedules, milestone schedules, resource schedules, and approval workflow schedules"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Master plan created successfully as central scheduling framework",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Central planning schedule for residential solar installation covering all project schedules from site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Draft",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": null,
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": null
  },
  "errors": []
}
```

## 🔍 Get Master Plan by ID

**GET** `/api/v1/master-plans/{id}`

Retrieve detailed information about a specific master plan.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Approved",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": "2025-06-16T14:30:00Z",
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": "Sarah Administrator"
  },
  "errors": []
}
```

## 🔍 Get Master Plan by Project ID

**GET** `/api/v1/master-plans/project/{projectId}`

Retrieve the master plan associated with a specific project.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "InProgress",
    "budget": 275000.00,
    "priority": "High",
    "projectName": "Solar Installation Project Alpha"
  },
  "errors": []
}
```

## 🔄 Update Master Plan

**PUT** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Update an existing master plan with new information.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1 (Updated)",
  "description": "Updated comprehensive plan with revised timeline and budget",
  "startDate": "2025-07-15T00:00:00Z",
  "endDate": "2025-10-15T00:00:00Z",
  "status": "InProgress",
  "budget": 285000.00,
  "priority": "High",
  "notes": "Updated to include additional safety requirements and equipment upgrades"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan updated successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1 (Updated)",
    "description": "Updated comprehensive plan with revised timeline and budget",
    "startDate": "2025-07-15T00:00:00Z",
    "endDate": "2025-10-15T00:00:00Z",
    "status": "InProgress",
    "budget": 285000.00,
    "priority": "High",
    "notes": "Updated to include additional safety requirements and equipment upgrades",
    "updatedAt": "2025-06-16T15:45:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Master Plan

**DELETE** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Delete a master plan. Only plans in "Draft" status can be deleted.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan deleted successfully",
  "data": null,
  "errors": []
}
```

## 🏗️ Phase Management

Manage project phases within a master plan, including creation, updates, and progress tracking.

### Create Project Phase

**POST** `/api/v1/master-plans/{id}/phases`

**🔒 Required Roles**: Administrator, ProjectManager

Add a new phase to an existing master plan with weight and duration specifications.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "phaseName": "Construction & Installation",
  "description": "Main construction phase including all installation activities",
  "plannedStartDate": "2025-08-01T00:00:00Z",
  "plannedEndDate": "2025-11-30T00:00:00Z",
  "weightPercentage": 0.65,
  "phaseOrder": 3
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Project phase created successfully",
  "data": {
    "phaseId": "789e0123-e89b-12d3-a456-426614174003",
    "phaseName": "Construction & Installation",
    "description": "Main construction phase including all installation activities",
    "plannedStartDate": "2025-08-01T00:00:00Z",
    "plannedEndDate": "2025-11-30T00:00:00Z",
    "actualStartDate": null,
    "actualEndDate": null,
    "status": "NotStarted",
    "completionPercentage": 0.0,
    "weightPercentage": 0.65,
    "phaseOrder": 3,
    "masterPlanId": "123e4567-e89b-12d3-a456-426614174000",
    "tasksCompleted": 0,
    "totalTasks": 0,
    "isOnSchedule": true,
    "isOnBudget": true
  },
  "errors": []
}
```

### Update Phase Progress

**PATCH** `/api/v1/master-plans/{masterPlanId}/phases/{phaseId}/progress`

**🔒 Required Roles**: Administrator, ProjectManager

Update the progress and status of a specific phase with detailed tracking information.

**Path Parameters**:
- `masterPlanId` (Guid): Master plan ID
- `phaseId` (Guid): Phase ID

**Request Body**:
```json
{
  "completionPercentage": 45.5,
  "status": "InProgress",
  "notes": "Foundation work completed, beginning structural installation",
  "activitiesCompleted": "Site preparation, foundation pouring, electrical rough-in",
  "issues": "Weather delays caused 3-day extension",
  "actualStartDate": "2025-08-03T00:00:00Z"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Phase progress updated successfully",
  "data": true,
  "errors": []
}
```

### Get All Phases for Master Plan

**GET** `/api/v1/master-plans/{id}/phases`

Retrieve all phases associated with a master plan, including progress and status information.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Phases retrieved successfully",
  "data": [
    {
      "phaseId": "456e7890-e89b-12d3-a456-426614174001",
      "phaseName": "Planning & Permitting",
      "description": "Initial planning and permit acquisition phase",
      "plannedStartDate": "2025-07-01T00:00:00Z",
      "plannedEndDate": "2025-07-31T00:00:00Z",
      "actualStartDate": "2025-07-01T00:00:00Z",
      "actualEndDate": "2025-07-28T00:00:00Z",
      "status": "Completed",
      "completionPercentage": 100.0,
      "weightPercentage": 0.15,
      "phaseOrder": 1,
      "tasksCompleted": 8,
      "totalTasks": 8,
      "isOnSchedule": true,
      "isOnBudget": true
    },
    {
      "phaseId": "789e0123-e89b-12d3-a456-426614174003",
      "phaseName": "Construction & Installation",
      "status": "InProgress",
      "completionPercentage": 45.5,
      "weightPercentage": 0.65,
      "phaseOrder": 3,
      "tasksCompleted": 12,
      "totalTasks": 28,
      "isOnSchedule": false,
      "isOnBudget": true
    }
  ],
  "errors": []
}
```

## 🎯 Milestone Management

Track and manage key project milestones within a master plan.

### Create Project Milestone

**POST** `/api/v1/master-plans/{id}/milestones`

**🔒 Required Roles**: Administrator, ProjectManager

Create a new milestone within a master plan for tracking key project deliverables.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "milestoneName": "Electrical System Commissioning Complete",
  "description": "All electrical systems tested and commissioned, ready for grid connection",
  "targetDate": "2025-11-15T00:00:00Z",
  "priority": "Critical"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Project milestone created successfully",
  "data": {
    "id": "abc123de-f456-789g-h012-ijklmnop3456",
    "milestoneName": "Electrical System Commissioning Complete",
    "description": "All electrical systems tested and commissioned, ready for grid connection",
    "dueDate": "2025-11-15T00:00:00Z",
    "isCompleted": false,
    "completedDate": null,
    "masterPlanId": "123e4567-e89b-12d3-a456-426614174000",
    "evidence": null,
    "phaseName": "Testing & Handover",
    "verifiedByName": null,
    "daysFromPlanned": 0,
    "isOverdue": false
  },
  "errors": []
}
```

### Mark Milestone Complete

**POST** `/api/v1/master-plans/{masterPlanId}/milestones/{milestoneId}/complete`

**🔒 Required Roles**: Administrator, ProjectManager

Mark a milestone as completed with evidence and verification details.

**Path Parameters**:
- `masterPlanId` (Guid): Master plan ID
- `milestoneId` (Guid): Milestone ID

**Request Body**:
```json
"Electrical commissioning completed successfully. All systems tested and certified by licensed electrician. Grid connection approval received from utility company."
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Milestone marked as complete",
  "data": true,
  "errors": []
}
```

### Get Upcoming Milestones

**GET** `/api/v1/master-plans/{id}/milestones/upcoming`

Retrieve milestones that are due within a specified timeframe for proactive management.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Query Parameters**:
- `days` (int, optional): Number of days to look ahead (default: 30)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Upcoming milestones retrieved successfully",
  "data": [
    {
      "id": "def456gh-i789-012j-k345-lmnopqrs6789",
      "milestoneName": "Final Inspection Approval",
      "description": "All work completed and approved by local building authority",
      "dueDate": "2025-12-01T00:00:00Z",
      "daysFromPlanned": 5,
      "isOverdue": false,
      "phaseName": "Testing & Handover"
    }
  ],
  "errors": []
}
```

## 📊 Progress Tracking & Analytics

### Get Master Plan Progress Summary

**GET** `/api/v1/master-plans/{id}/progress`

Retrieve comprehensive progress analytics including weighted calculations and health status.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Progress summary retrieved successfully",
  "data": {
    "overallCompletion": 52.8,
    "healthStatus": "OnTrack",
    "completedPhases": 2,
    "totalPhases": 4,
    "completedMilestones": 8,
    "totalMilestones": 15,
    "daysRemaining": 95,
    "isOnSchedule": true,
    "isOnBudget": true,
    "lastUpdated": "2025-06-29T10:30:00Z",
    "phaseBreakdown": [
      {
        "phaseName": "Planning & Permitting",
        "weightPercentage": 0.15,
        "completionPercentage": 100.0,
        "contributionToOverall": 15.0
      },
      {
        "phaseName": "Procurement & Logistics", 
        "weightPercentage": 0.10,
        "completionPercentage": 80.0,
        "contributionToOverall": 8.0
      },
      {
        "phaseName": "Construction & Installation",
        "weightPercentage": 0.65,
        "completionPercentage": 45.5,
        "contributionToOverall": 29.6
      },
      {
        "phaseName": "Testing & Handover",
        "weightPercentage": 0.10,
        "completionPercentage": 0.0,
        "contributionToOverall": 0.0
      }
    ]
  },
  "errors": []
}
```

### Get Overall Completion Percentage

**GET** `/api/v1/master-plans/{id}/completion`

Get the calculated overall completion percentage using weighted phase calculations.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Completion percentage calculated successfully",
  "data": 52.8,
  "errors": []
}
```

## 📈 Progress Reports

### Create Progress Report

**POST** `/api/v1/master-plans/{id}/progress-reports`

**🔒 Required Roles**: Administrator, ProjectManager

Generate a comprehensive progress report for stakeholders with detailed analysis.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "title": "Weekly Progress Report - Week 12",
  "summary": "Construction phase progressing well with minor weather delays",
  "detailedReport": "Phase 3 (Construction & Installation) is currently 45.5% complete...",
  "completionPercentage": 52.8,
  "keyAccomplishments": "Completed electrical rough-in for buildings 1-3, solar panel installation 60% complete",
  "currentChallenges": "Weather delays affecting rooftop work, supply chain delays for inverters",
  "upcomingActivities": "Complete remaining panel installation, begin inverter commissioning",
  "reportDate": "2025-06-29T00:00:00Z",
  "weatherConditions": "Partly cloudy, suitable for installation work",
  "workersOnSite": 24,
  "safetyNotes": "Zero incidents this week, safety training conducted for new team members",
  "qualityNotes": "All installations meeting quality standards, daily QC inspections ongoing"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Progress report created successfully",
  "data": {
    "progressReportId": "report-123-456",
    "masterPlanId": "123e4567-e89b-12d3-a456-426614174000",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "reportDate": "2025-06-29T00:00:00Z",
    "overallCompletionPercentage": 52.8,
    "schedulePerformanceIndex": 0.95,
    "costPerformanceIndex": 1.02,
    "actualCostToDate": 280500.00,
    "estimatedCostAtCompletion": 532000.00,
    "budgetVariance": -12000.00,
    "scheduleVarianceDays": -3,
    "projectedCompletionDate": "2025-12-18T00:00:00Z",
    "activeIssuesCount": 2,
    "completedMilestonesCount": 8,
    "totalMilestonesCount": 15,
    "healthStatus": "OnTrack",
    "createdAt": "2025-06-29T10:00:00Z",
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "createdByName": "John Manager",
    "projectName": "Solar Installation Project Alpha"
  },
  "errors": []
}
```

### Get Progress Reports

**GET** `/api/v1/master-plans/{id}/progress-reports`

Retrieve paginated list of progress reports for a master plan.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Query Parameters**:
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Progress reports retrieved successfully",
  "data": [
    {
      "progressReportId": "report-123-456",
      "reportDate": "2025-06-29T00:00:00Z",
      "overallCompletionPercentage": 52.8,
      "healthStatus": "OnTrack",
      "createdByName": "John Manager",
      "keyAccomplishments": "Completed electrical rough-in for buildings 1-3...",
      "currentChallenges": "Weather delays affecting rooftop work..."
    }
  ],
  "errors": []
}
```

## 📊 Master Plan Status Workflow & Business Logic

Master plans follow a sophisticated status workflow with advanced business logic for coordinating all project schedules, managing dependencies across schedule types, and maintaining hierarchical project structure as the central planning authority.

### Status Workflow
| Status | Description | Available Actions | Who Can Change |
|--------|-------------|-------------------|----------------|
| **Draft** | Initial creation, all schedules editable | Edit schedules, Submit for Approval, Delete | Creator, Admin |
| **Pending** | Submitted for approval with all schedules | Approve, Reject, Request Changes | Admin, Senior PM |
| **Approved** | Ready for execution, schedules locked | Start Execution, Archive | Admin, PM |
| **InProgress** | Currently being executed, live schedule tracking | Update Progress, Complete | Admin, PM |
| **Completed** | Successfully finished, all schedules complete | Archive, Generate Report | Admin, PM |
| **Cancelled** | Cancelled before completion, schedules voided | Archive, Restart | Admin |

### Advanced Schedule Coordination & Integration

The system implements sophisticated, multi-level schedule coordination using the master plan as the central scheduling authority:

#### 🔢 Schedule Integration Formulas

**Phase Schedule Completion (Weighted Average)**:
```
PhaseScheduleCompletion = Σ(TaskScheduleCompletion[i] × TaskDuration[i]) / Σ(TaskDuration[i])
```

**Overall Master Schedule Completion**:
```
MasterScheduleCompletion = Σ(PhaseScheduleCompletion[p] × PhaseWeight[p])
```

**Schedule Performance Index (SPI)**:
```
SPI = Earned Schedule Value / Planned Schedule Value
```

For solar projects, the master plan typically coordinates these schedule weights:
- Planning & Permitting Schedule: 15% weight
- Procurement & Logistics Schedule: 10% weight  
- Construction & Installation Schedule: 65% weight
- Testing & Handover Schedule: 10% weight

#### 🔗 Schedule Dependencies & Critical Path Analysis

The master plan supports comprehensive dependency management across all schedule types:

| Dependency Type | Code | Description | Example Use Case |
|------|------|-------------|------------------|
| **Finish-to-Start** | FS | Schedule B starts after Schedule A finishes | Phase schedule → Next phase schedule |
| **Start-to-Start** | SS | Schedule B starts when Schedule A starts | Parallel task schedules |
| **Finish-to-Finish** | FF | Schedule B finishes when Schedule A finishes | Milestone schedules alignment |
| **Start-to-Finish** | SF | Schedule B finishes when Schedule A starts | Resource handoff schedules |

#### 🏗️ Resource Schedule Dependencies

Beyond logical schedule dependencies, the master plan models **Resource Schedule Dependencies** where multiple schedules compete for limited resources (crews, equipment). This prevents scheduling conflicts and identifies potential bottlenecks across all project schedules.

#### 📈 Hierarchical Schedule Structure

The master plan uses **recursive scheduling algorithms** to handle unlimited nesting levels with **Project as the root scheduling entity**:
- **Project** (Root Schedule) → **Master Plan** (Central Schedule) → **Phase Schedules** → **Task Schedules** → **Sub-task Schedules**
- Each schedule level contributes to the parent schedule completion
- Project-level schedule aggregates Master Plan schedule completion
- Master Plan inherits Project schedule constraints and stakeholder requirements
- Supports future schedule expansion without architectural changes

### Project-Master Plan Schedule Model

```
Project (1) ←→ (1) MasterPlan (Central Schedule Authority)
    ↓                ↓
Project.Budget ≥ MasterPlan.Budget
Project.StartDate ≤ MasterPlan.StartDate  
Project.EndDate ≥ MasterPlan.EndDate
Project.Stakeholders → MasterPlan.ScheduleNotifications
MasterPlan.Schedules → All Phase/Task/Milestone/Resource Schedules
```

## 📊 Advanced Analytics

### Get Critical Path Analysis

**GET** `/api/v1/master-plans/{id}/critical-path`

Analyze the critical path for the master plan, identifying bottlenecks and resource constraints.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Critical path analysis completed successfully",
  "data": {
    "criticalPathDuration": 120,
    "criticalPathTasks": [
      {
        "taskId": "task-001",
        "taskName": "Site Survey & Assessment",
        "duration": 7,
        "startDate": "2025-07-01T00:00:00Z",
        "endDate": "2025-07-08T00:00:00Z",
        "float": 0,
        "isCritical": true
      },
      {
        "taskId": "task-005",
        "taskName": "Permit Approval",
        "duration": 21,
        "startDate": "2025-07-09T00:00:00Z",
        "endDate": "2025-07-30T00:00:00Z",
        "float": 0,
        "isCritical": true
      }
    ],
    "resourceBottlenecks": [
      {
        "resourceType": "Electrical Crew",
        "conflictingTasks": ["task-012", "task-015"],
        "recommendedAction": "Schedule sequential or add additional crew"
      }
    ]
  },
  "errors": []
}
```

### Get Earned Value Analysis

**GET** `/api/v1/master-plans/{id}/earned-value`

Calculate Earned Value Management (EVM) metrics for project performance analysis.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Earned value analysis completed successfully",
  "data": {
    "budgetAtCompletion": 275000.00,
    "actualCostWorkPerformed": 145000.00,
    "budgetedCostWorkPerformed": 132000.00,
    "budgetedCostWorkScheduled": 150000.00,
    "costVariance": -13000.00,
    "scheduleVariance": -18000.00,
    "costPerformanceIndex": 0.91,
    "schedulePerformanceIndex": 0.88,
    "estimateAtCompletion": 302197.80,
    "estimateToComplete": 157197.80,
    "varianceAtCompletion": -27197.80,
    "toCompletePerformanceIndex": 1.21,
    "analysis": {
      "status": "Behind schedule and over budget",
      "recommendations": [
        "Review resource allocation for critical path tasks",
        "Consider value engineering opportunities",
        "Implement corrective action plan for schedule recovery"
      ]
    }
  },
  "errors": []
}
```

### Get Resource Utilization Report

**GET** `/api/v1/master-plans/{id}/resource-utilization`

Analyze resource utilization across all phases and identify optimization opportunities.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Query Parameters**:
- `startDate` (DateTime, optional): Analysis start date
- `endDate` (DateTime, optional): Analysis end date
- `resourceType` (string, optional): Filter by resource type

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Resource utilization analysis completed successfully",
  "data": {
    "analysisperiod": {
      "startDate": "2025-07-01T00:00:00Z",
      "endDate": "2025-09-30T00:00:00Z"
    },
    "resourceUtilization": [
      {
        "resourceType": "Electrical Crew",
        "totalCapacity": 480,
        "allocatedHours": 432,
        "utilizationRate": 0.90,
        "peakUtilization": 1.15,
        "conflicts": [
          {
            "date": "2025-08-15T00:00:00Z",
            "overallocation": 6,
            "conflictingTasks": ["task-012", "task-015"]
          }
        ]
      },
      {
        "resourceType": "Installation Equipment",
        "totalCapacity": 720,
        "allocatedHours": 650,
        "utilizationRate": 0.90,
        "peakUtilization": 0.95,
        "conflicts": []
      }
    ],
    "recommendations": [
      "Consider staggering electrical work to avoid 115% overallocation on Aug 15",
      "Installation equipment utilization is optimal"
    ]
  },
  "errors": []
}
```

## 🔗 Dependency Management & Workflow Automation

Advanced dependency management capabilities with automated workflow triggers and constraint validation.

### Create Task Dependencies

**POST** `/api/v1/master-plans/{id}/dependencies`

**🔒 Required Roles**: Administrator, ProjectManager

Create logical or resource dependencies between tasks with validation.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "predecessorTaskId": "task-001",
  "successorTaskId": "task-002",
  "dependencyType": "FS",
  "lagTime": 2,
  "dependencyDescription": "Building permit required before construction can begin"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Task dependency created successfully",
  "data": {
    "dependencyId": "dep-001",
    "predecessorTaskId": "task-001",
    "successorTaskId": "task-002",
    "dependencyType": "FS",
    "lagTime": 2,
    "dependencyDescription": "Building permit required before construction can begin",
    "isActive": true,
    "createdDate": "2025-06-29T10:30:00Z"
  },
  "errors": []
}
```

### Validate Schedule Constraints

**POST** `/api/v1/master-plans/{id}/validate-constraints`

**🔒 Required Roles**: Administrator, ProjectManager

Validate all schedule constraints and dependencies before plan approval.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Schedule constraint validation completed",
  "data": {
    "isValid": false,
    "validationResults": [
      {
        "constraintType": "TaskDependency",
        "severity": "Error",
        "message": "Circular dependency detected between tasks: task-005 → task-012 → task-005",
        "affectedTasks": ["task-005", "task-012"],
        "resolution": "Remove one of the conflicting dependencies"
      },
      {
        "constraintType": "ResourceConflict",
        "severity": "Warning",
        "message": "Electrical Crew overallocated by 15% on 2025-08-15",
        "affectedTasks": ["task-012", "task-015"],
        "resolution": "Stagger tasks or allocate additional crew"
      }
    ],
    "criticalIssues": 1,
    "warnings": 1
  },
  "errors": []
}
```

### Trigger Workflow Automation

**POST** `/api/v1/master-plans/{id}/trigger-workflow`

**🔒 Required Roles**: Administrator, ProjectManager

Trigger automated workflow actions based on milestone completion or status changes.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "triggerType": "MilestoneCompleted",
  "triggerData": {
    "milestoneId": "milestone-003",
    "completedDate": "2025-07-30T16:00:00Z"
  },
  "automationRules": [
    "SendNotificationToStakeholders",
    "AdvanceToNextPhase",
    "UpdateBudgetAllocations"
  ]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Workflow automation triggered successfully",
  "data": {
    "workflowExecutionId": "wf-exec-001",
    "triggeredActions": [
      {
        "actionType": "SendNotificationToStakeholders",
        "status": "Completed",
        "executedAt": "2025-07-30T16:05:00Z",
        "details": "Notifications sent to 12 stakeholders"
      },
      {
        "actionType": "AdvanceToNextPhase",
        "status": "Completed",
        "executedAt": "2025-07-30T16:06:00Z",
        "details": "Phase 2 status updated to 'InProgress'"
      },
      {
        "actionType": "UpdateBudgetAllocations",
        "status": "Pending",
        "scheduledFor": "2025-07-31T09:00:00Z",
        "details": "Budget reallocation scheduled for next business day"
      }
    ]
  },
  "errors": []
}
```

## 📊 Advanced Reporting & Export

Comprehensive reporting capabilities with multiple export formats and stakeholder-specific views.

### Generate Executive Dashboard

**GET** `/api/v1/master-plans/{id}/executive-dashboard`

**🔒 Required Roles**: Administrator, ProjectManager, Executive

Generate executive-level dashboard with KPIs and high-level metrics.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Executive dashboard generated successfully",
  "data": {
    "projectOverview": {
      "projectName": "Solar Installation Project Alpha",
      "currentPhase": "Construction & Installation",
      "overallProgress": 52.8,
      "budgetUtilization": 52.7,
      "schedulePerformance": 88.0,
      "riskLevel": "Medium"
    },
    "keyMetrics": {
      "costPerformanceIndex": 0.91,
      "schedulePerformanceIndex": 0.88,
      "qualityIndex": 0.96,
      "safetyIndex": 1.0
    },
    "milestoneStatus": {
      "completed": 8,
      "inProgress": 2,
      "upcoming": 5,
      "overdue": 1
    },
    "criticalIssues": [
      {
        "issueType": "Schedule",
        "description": "Electrical installation 3 days behind schedule",
        "impact": "High",
        "mitigation": "Additional crew scheduled for overtime"
      }
    ],
    "nextKeyMilestones": [
      {
        "milestoneName": "Electrical Rough-in Complete",
        "targetDate": "2025-08-15T00:00:00Z",
        "riskLevel": "Medium"
      }
    ]
  },
  "errors": []
}
```

### Export Project Report

**GET** `/api/v1/master-plans/{id}/export`

**🔒 Required Roles**: Administrator, ProjectManager

Export comprehensive project report in various formats.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Query Parameters**:
- `format` (string): Export format (pdf, excel, csv, json)
- `sections` (string[]): Report sections to include
- `dateRange` (string): Date range for time-based data

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project report exported successfully",
  "data": {
    "exportId": "export-001",
    "downloadUrl": "/api/v1/exports/export-001/download",
    "format": "pdf",
    "fileSize": 2048576,
    "generatedAt": "2025-06-29T14:30:00Z",
    "expiresAt": "2025-07-06T14:30:00Z",
    "sections": [
      "ProjectOverview",
      "ProgressSummary",
      "BudgetAnalysis",
      "ResourceUtilization",
      "RiskAssessment"
    ]
  },
  "errors": []
}
```

### Generate Stakeholder Communication

**POST** `/api/v1/master-plans/{id}/stakeholder-communication`

**🔒 Required Roles**: Administrator, ProjectManager

Generate tailored communication for different stakeholder groups.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "communicationType": "ProgressUpdate",
  "stakeholderGroup": "Investors",
  "customMessage": "Project continues to progress with minor schedule adjustments",
  "includeFinancialData": true,
  "includeRiskAnalysis": true,
  "attachments": ["progress-photos", "budget-summary"]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Stakeholder communication generated successfully",
  "data": {
    "communicationId": "comm-001",
    "subject": "Solar Project Alpha - Progress Update Week 12",
    "recipientCount": 8,
    "deliveryMethod": "Email",
    "scheduledDelivery": "2025-06-29T17:00:00Z",
    "content": {
      "executiveSummary": "Project is 52.8% complete with minor schedule adjustments...",
      "keyHighlights": [
        "Electrical rough-in completed ahead of schedule",
        "Weather contingency plan activated successfully",
        "Budget variance within acceptable range"
      ],
      "attachments": [
        {
          "type": "progress-photos",
          "description": "Latest construction progress photos",
          "url": "/api/v1/attachments/progress-photos-week12.zip"
        }
      ]
    }
  },
  "errors": []
}
```

## 🔧 Master Plan Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **MP001** | Master plan not found | Verify the plan ID exists |
| **MP002** | Project already has a master plan | Update existing plan or create new version |
| **MP003** | Invalid project ID | Ensure project exists and is accessible |
| **MP004** | Insufficient permissions | Check user role requirements |
| **MP005** | Cannot delete plan with active tasks | Complete or remove associated tasks first |
| **MP006** | Invalid status transition | Follow the defined workflow sequence |
| **MP007** | Project not found | Verify the parent project exists before creating master plan |
| **MP008** | Project-Master Plan relationship violation | Cannot modify project reference after master plan creation |

## 🔗 Project Context Integration

Master Plans are deeply integrated with their parent Project and inherit key contextual information:

### Inherited Project Attributes
- **Project Budget**: Master Plan budget cannot exceed Project total budget
- **Project Timeline**: Master Plan dates must fall within Project start/end dates  
- **Project Stakeholders**: Notification and approval workflows use Project stakeholder lists
- **Project Location**: Geographic and site-specific constraints from Project
- **Project Type**: Solar installation specifications and compliance requirements

### Project-Level Constraints
- Each Project can have only **one active Master Plan** at any time
- Archived Master Plans remain linked to Project for historical tracking
- Master Plan status changes may trigger Project status updates
- Project completion requires Master Plan completion

### Cross-Reference Endpoints

**Get Project Information from Master Plan**:
```http
GET /api/v1/master-plans/{masterPlanId}/project
```

**Get All Master Plans for Project (including archived)**:
```http
GET /api/v1/projects/{projectId}/master-plans?includeArchived=true
```

## 🎯 Summary: Master Plan as Central Scheduling Authority

The Master Plan serves as the **single source of truth for all project scheduling**, functioning as:

### 📅 Unified Schedule Management
- **Central Coordination Point**: All project schedules flow through and are managed by the master plan
- **Schedule Integration**: Combines phase schedules, task schedules, milestone schedules, resource schedules, and approval schedules into one coherent framework
- **Conflict Resolution**: Identifies and resolves scheduling conflicts across all schedule types before they impact project delivery

### ⚡ Real-Time Schedule Synchronization
- **Live Updates**: Changes to any sub-schedule automatically update the master plan and all related schedules
- **Dependency Cascade**: Schedule changes trigger automatic updates to dependent schedules throughout the project
- **Resource Optimization**: Prevents double-booking and optimizes resource allocation across all project schedules

### 📊 Comprehensive Schedule Analytics
- **Critical Path Tracking**: Identifies the longest sequence of dependent schedules that determines project completion
- **Schedule Performance Monitoring**: Tracks planned vs. actual progress across all integrated schedules
- **Predictive Scheduling**: Uses historical data and current progress to forecast schedule completion and identify potential delays

### 🔗 Stakeholder Schedule Communication
- **Unified Reporting**: Provides stakeholders with a single, coherent view of all project schedules
- **Role-Based Schedule Views**: Delivers schedule information tailored to different stakeholder needs (executives, project managers, technicians)
- **Automated Schedule Notifications**: Alerts stakeholders to schedule changes, conflicts, or milestones across all schedule types

The master plan ensures that all project activities are properly scheduled, coordinated, and tracked within a single, integrated scheduling framework that serves as the definitive planning schedule for the entire project.
