# 🔧 Work Requests

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (create requests), All authenticated users (view)

Work requests provide a comprehensive system for managing change orders, additional work, scope modifications, and client-requested enhancements within solar installation projects. Each Work Request belongs to a Project and follows an advanced approval workflow with cost tracking, resource allocation, and complete audit trails. The system supports complex approval hierarchies, budget controls, and integration with project schedules and master plans.

## 🏗️ Project-Work Request Relationship

- **Project** is the primary business entity containing the original scope and budget constraints
- **Work Request** belongs to a Project (many:1 relationship) and represents scope changes or additions
- Work Requests integrate with the project's **Master Plan** for schedule impact assessment
- Multiple Work Requests can exist per Project, each tracked independently through approval workflows
- Work Requests inherit project context (client information, location, team assignments) from their parent Project
- Approved Work Requests can trigger updates to project budgets, schedules, and resource allocations
- All cost tracking and scope changes are consolidated at the Project level for comprehensive oversight

## ⚡ Work Request Capabilities

### Admin & Manager
- ✅ Create work requests for any project and on behalf of team members
- ✅ Review and approve/reject requests with multi-level authorization
- ✅ Modify request details, pricing, and scope even after submission
- ✅ Update status, priority, and approval workflow settings
- ✅ Generate formal change orders and client documentation
- ✅ Override approval requirements for urgent situations
- ✅ Access comprehensive analytics and cost impact reports
- ✅ Configure approval workflows and authorization limits
- ✅ Manage bulk operations and workflow automation
- ✅ Delete work requests with complete audit trail

### Project Managers
- ✅ Create and manage work requests for assigned projects
- ✅ Approve requests within their authorization limits
- ✅ Review cost and schedule impact on project plans
- ✅ Coordinate with clients for approval and acceptance
- ✅ Generate project-specific change order reports
- ✅ Access project-level work request analytics

### Users (Technicians & Field Staff)
- ✅ Create work requests for assigned projects with guided templates
- ✅ Submit requests for approval workflow with supporting documentation
- ✅ Track request status and receive automated notifications
- ✅ Attach photos, drawings, and supporting documents
- ✅ Provide detailed justification and cost estimates
- ✅ Update draft requests before submission
- ❌ Cannot approve their own requests (conflict of interest)
- ❌ Cannot modify requests after submission without authorization
- ❌ Cannot delete requests once submitted
- ❌ Limited access to pricing and margin information

### Clients (with Portal Access)
- ✅ View work requests that affect their project
- ✅ Approve or reject change orders
- ✅ Provide feedback and comments on proposed changes
- ✅ Access cost breakdown and timeline impact information
- ❌ Cannot create work requests directly
- ❌ Cannot see internal cost breakdowns or margins

## 🎯 Work Request Features & Capabilities

### 💼 Advanced Request Management
- **Scope Change Tracking**: Detailed documentation of scope modifications with impact analysis
- **Cost Management**: Comprehensive cost breakdown with materials, labor, and overhead tracking
- **Schedule Impact Assessment**: Integration with master plans for timeline impact evaluation
- **Risk Assessment**: Identification and mitigation of risks associated with scope changes
- **Client Communication**: Professional change order generation with client approval workflows

### 🔄 Sophisticated Approval Workflows
- **Multi-Level Approvals**: Configurable approval hierarchies based on cost thresholds and project types
- **Authorization Limits**: Role-based spending limits with automatic escalation for larger requests
- **Parallel Approvals**: Support for concurrent approvals from multiple stakeholders
- **Emergency Override**: Fast-track approval process for urgent safety or compliance issues
- **Automated Routing**: Intelligent routing based on request type, cost, and project parameters

### 📊 Analytics & Financial Controls
- **Budget Impact Analysis**: Real-time assessment of budget implications and variance tracking
- **Profitability Analysis**: Margin calculation and profitability impact assessment
- **Trend Analysis**: Historical analysis of request patterns and cost trends
- **Compliance Monitoring**: Tracking of regulatory and contractual compliance requirements
- **Performance Metrics**: Approval times, cost accuracy, and completion rates

### 🔗 Integration & Automation
- **Master Plan Integration**: Automatic schedule updates and milestone adjustments
- **Task Creation**: Automatic generation of tasks and work orders from approved requests
- **Resource Allocation**: Integration with resource management for personnel and equipment scheduling
- **Document Generation**: Automated creation of change orders, contracts, and client communications
- **Notification System**: Real-time notifications for stakeholders throughout the approval process

## 🚀 API Overview

The Work Requests API provides comprehensive endpoints for managing change orders and scope modifications:

### 📖 Core CRUD Operations
- **Create Requests**: `/api/v1/work-requests` and `/api/v1/work-requests/enhanced`
- **Retrieve Requests**: `/api/v1/work-requests`, `/api/v1/work-requests/{id}`, `/api/v1/work-requests/projects/{projectId}`
- **Update Requests**: `/api/v1/work-requests/{id}` (with workflow state validation)
- **Delete Requests**: `/api/v1/work-requests/{id}` (Admin/Manager only with audit trail)

### 🔄 Workflow Management
- **Approval Operations**: Submit, approve, reject, and escalate operations
- **Status Tracking**: Draft → Submitted → Under Review → Approved/Rejected → In Progress → Completed
- **Workflow Configuration**: Customizable approval chains and authorization limits

### 📊 Analytics & Reporting
- **Financial Analysis**: `/api/v1/work-requests/projects/{projectId}/financial-analysis`
- **Approval Analytics**: `/api/v1/work-requests/approval-analytics`
- **Performance Reports**: `/api/v1/work-requests/performance-reports`
- **Export Options**: Comprehensive export capabilities for accounting and project management

### 🔧 Advanced Features
- **Template Management**: Project-specific request templates with cost estimation
- **Bulk Operations**: Mass operations for large projects with multiple requests
- **Integration APIs**: Connections to accounting systems, project management tools, and client portals

## 📋 Get All Work Requests

**GET** `/api/v1/work-requests`

Retrieve work requests with filtering options.

**Query Parameters**:
- `projectId` (Guid): Filter by specific project
- `requesterId` (Guid): Filter by request creator
- `assignedToId` (Guid): Filter by assigned reviewer/approver
- `status` (string): Filter by current status (Draft, Submitted, Approved, Rejected, InProgress, Completed, Closed)
- `priority` (string): Filter by priority level (Low, Medium, High, Critical, Emergency)
- `approvalStatus` (string): Filter by approval workflow status
- `type` (string): Filter by request type (ChangeOrder, AdditionalWork, ScopeModification, ClientRequest, Emergency)
- `costRange` (string): Filter by cost range (Under1000, 1000To5000, 5000To10000, Over10000)
- `startDate` (DateTime): Filter requests created from date
- `endDate` (DateTime): Filter requests created to date
- `requiredByDate` (DateTime): Filter by required completion date
- `hasClientApproval` (bool): Filter by client approval status
- `isUrgent` (bool): Filter urgent/emergency requests
- `sortBy` (string): Sort field (requestDate, priority, estimatedCost, status, approvalDate)
- `sortDirection` (string): Sort direction (asc, desc)
- `includeAnalytics` (bool): Include summary analytics data
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work requests retrieved successfully",
  "data": {
    "requests": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Additional Electrical Outlets",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "requesterId": "789e0123-e89b-12d3-a456-426614174002",
        "requesterName": "John Technician",
        "requestDate": "2025-06-10T09:15:00Z",
        "requiredByDate": "2025-06-25T00:00:00Z",
        "type": "ClientRequest",
        "priority": "Medium",
        "status": "Submitted",
        "approvalStatus": "PendingManagerApproval",
        "currentApprover": "Sarah Manager",
        "estimatedCost": 1250.00,
        "estimatedHours": 10,
        "actualCost": null,
        "actualHours": null,
        "budgetImpact": 4.2,
        "scheduleImpact": "None",
        "clientApprovalRequired": true,
        "clientApprovalStatus": "Pending",
        "urgencyLevel": "Normal",
        "riskLevel": "Low",
        "description": "Client requested 5 additional electrical outlets in the garage area for EV charging capability",
        "createdAt": "2025-06-10T09:15:00Z",
        "updatedAt": "2025-06-10T14:30:00Z"
      },
      {
        "id": "234f6789-e89b-12d3-a456-426614174000",
        "title": "Upgraded Inverter System",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "requesterId": "890f1234-e89b-12d3-a456-426614174002",
        "requesterName": "Sarah Manager",
        "requestDate": "2025-06-12T11:30:00Z",
        "requiredByDate": "2025-07-01T00:00:00Z",
        "type": "UpgradeRequest",
        "priority": "High",
        "status": "Approved",
        "approvalStatus": "FullyApproved",
        "currentApprover": null,
        "estimatedCost": 3800.00,
        "estimatedHours": 16,
        "actualCost": null,
        "actualHours": null,
        "budgetImpact": 12.8,
        "scheduleImpact": "2 days extension",
        "clientApprovalRequired": true,
        "clientApprovalStatus": "Approved",
        "urgencyLevel": "High",
        "riskLevel": "Medium",
        "description": "Upgrade to higher capacity inverter system to accommodate future expansion",
        "createdAt": "2025-06-12T11:30:00Z",
        "updatedAt": "2025-06-13T09:45:00Z"
      }
    ],
    "pagination": {
      "totalCount": 8,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    },
    "analytics": {
      "totalRequests": 8,
      "totalEstimatedCost": 15750.00,
      "averageApprovalTime": "2.3 days",
      "approvalRate": 87.5,
      "averageCost": 1968.75,
      "statusBreakdown": {
        "Draft": 1,
        "Submitted": 2,
        "Approved": 4,
        "InProgress": 1,
        "Completed": 0
      },
      "priorityBreakdown": {
        "Low": 1,
        "Medium": 4,
        "High": 2,
        "Critical": 1
      },
      "budgetImpact": 8.4
    }
  },
  "errors": []
}
```

## 🔍 Get Work Request by ID

**GET** `/api/v1/work-requests/{id}`

Retrieve details of a specific work request.

**Path Parameters**:
- `id` (Guid): Work request ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Additional Electrical Outlets",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "requesterId": "789e0123-e89b-12d3-a456-426614174002",
    "requesterName": "John Technician",
    "requestDate": "2025-06-10T09:15:00Z",
    "requiredByDate": "2025-06-25T00:00:00Z",
    "type": "ClientRequest",
    "category": "Electrical",
    "priority": "Medium",
    "status": "Submitted",
    "approvalStatus": "PendingManagerApproval",
    "currentApprover": {
      "userId": "manager1-id",
      "name": "Sarah Manager",
      "role": "ProjectManager",
      "authorizationLimit": 5000.00
    },
    "estimatedCost": 1250.00,
    "actualCost": null,
    "estimatedHours": 10,
    "actualHours": null,
    "budgetImpact": 4.2,
    "scheduleImpact": "No impact on critical path",
    "clientApprovalRequired": true,
    "clientApprovalStatus": "Pending",
    "urgencyLevel": "Normal",
    "riskLevel": "Low",
    "profitMargin": 25.0,
    "description": "Client requested 5 additional electrical outlets in the garage area for EV charging capability",
    "justification": "Client plans to purchase two electric vehicles within the next 3 months and requires additional charging capacity",
    "scope": "Install 5 new 220V outlets with appropriate wiring and circuit breakers",
    "materials": [
      {
        "name": "220V Outlets",
        "quantity": 5,
        "unitCost": 45.00,
        "totalCost": 225.00
      },
      {
        "name": "Circuit Breakers",
        "quantity": 5,
        "unitCost": 65.00,
        "totalCost": 325.00
      },
      {
        "name": "Wiring (feet)",
        "quantity": 100,
        "unitCost": 2.00,
        "totalCost": 200.00
      }
    ],
    "labor": {
      "hours": 10,
      "rate": 50.00,
      "totalCost": 500.00
    },
    "approvalHistory": [
      {
        "id": "approval1-id",
        "stage": "Submitted",
        "status": "Completed",
        "date": "2025-06-10T09:15:00Z",
        "by": "John Technician",
        "byRole": "Technician",
        "action": "Submitted",
        "comments": "Initial submission with detailed cost analysis",
        "attachments": ["cost_breakdown.pdf"]
      },
      {
        "id": "approval2-id",
        "stage": "Manager Review",
        "status": "Pending",
        "date": null,
        "by": "Sarah Manager",
        "byRole": "ProjectManager",
        "action": "Pending",
        "comments": null,
        "dueDate": "2025-06-12T17:00:00Z",
        "remindersSent": 1
      }
    ],
    "workflowSteps": [
      {
        "stepOrder": 1,
        "stepName": "Technical Review",
        "required": true,
        "completed": true,
        "approver": "John Technician",
        "completedDate": "2025-06-10T09:15:00Z"
      },
      {
        "stepOrder": 2,
        "stepName": "Manager Approval",
        "required": true,
        "completed": false,
        "approver": "Sarah Manager",
        "dueDate": "2025-06-12T17:00:00Z"
      },
      {
        "stepOrder": 3,
        "stepName": "Client Approval",
        "required": true,
        "completed": false,
        "approver": "Client Portal",
        "dependsOn": "Manager Approval"
      }
    ],
    "attachments": [
      {
        "id": "att1-id",
        "fileName": "garage_outlet_locations.pdf",
        "fileType": "application/pdf",
        "fileSize": 1200000,
        "uploadedAt": "2025-06-10T09:10:00Z"
      }
    ],
    "createdAt": "2025-06-10T09:15:00Z",
    "updatedAt": "2025-06-10T14:30:00Z"
  },
  "errors": []
}
```

## 📝 Create Work Request

**POST** `/api/v1/work-requests`

Create a new work request for additional work or changes.

**Request Body**:
```json
{
  "title": "Battery Backup System Addition",
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "priority": "High",
  "description": "Client requests addition of battery backup system for critical power needs during outages",
  "justification": "Recent power outages in the area have led client to request emergency backup capability",
  "scope": "Install 10kWh battery backup system with automatic transfer switch integration",
  "estimatedCost": 8500.00,
  "materials": [
    {
      "name": "10kWh Battery System",
      "quantity": 1,
      "unitCost": 6000.00,
      "totalCost": 6000.00
    },
    {
      "name": "Transfer Switch",
      "quantity": 1,
      "unitCost": 800.00,
      "totalCost": 800.00
    },
    {
      "name": "Wiring and Components",
      "quantity": 1,
      "unitCost": 700.00,
      "totalCost": 700.00
    }
  ],
  "labor": {
    "hours": 20,
    "rate": 50.00,
    "totalCost": 1000.00
  }
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Work request created successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "title": "Battery Backup System Addition",
    "status": "Draft",
    "estimatedCost": 8500.00,
    "createdAt": "2025-06-15T13:45:00Z"
  },
  "errors": []
}
```

## 🔄 Update Work Request

**PUT** `/api/v1/work-requests/{id}`

**🔒 Requires**: Admin, Manager, or request creator (for Draft status only)

Update an existing work request. Only requests in Draft status can be modified by creators.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "title": "Battery Backup System Addition (Updated)",
  "priority": "Critical",
  "description": "Client requests addition of battery backup system for critical power needs during outages - URGENT for upcoming storm season",
  "justification": "Recent severe weather warnings and multiple power outages have led client to request emergency backup capability",
  "scope": "Install 15kWh battery backup system with automatic transfer switch integration and smart monitoring",
  "estimatedCost": 12500.00,
  "requiredByDate": "2025-06-30T00:00:00Z",
  "materials": [
    {
      "name": "15kWh Battery System",
      "quantity": 1,
      "unitCost": 8500.00,
      "totalCost": 8500.00
    },
    {
      "name": "Smart Transfer Switch",
      "quantity": 1,
      "unitCost": 1200.00,
      "totalCost": 1200.00
    },
    {
      "name": "Monitoring System",
      "quantity": 1,
      "unitCost": 800.00,
      "totalCost": 800.00
    }
  ],
  "labor": {
    "hours": 24,
    "rate": 50.00,
    "totalCost": 1200.00
  }
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request updated successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "title": "Battery Backup System Addition (Updated)",
    "status": "Draft",
    "priority": "Critical",
    "estimatedCost": 12500.00,
    "updatedAt": "2025-06-15T15:30:00Z",
    "canSubmit": true,
    "requiresClientApproval": true
  },
  "errors": []
}
```

## 🔍 Get Work Requests by Project

**GET** `/api/v1/work-requests/projects/{projectId}`

Retrieve all work requests for a specific project with enhanced filtering and analytics.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Query Parameters**: Same as main work requests endpoint

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project work requests retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "projectBudget": 75000.00,
    "originalBudget": 65000.00,
    "budgetVariance": 15.38,
    "requests": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Additional Electrical Outlets",
        "type": "ClientRequest",
        "status": "Approved",
        "estimatedCost": 1250.00,
        "budgetImpact": 1.9,
        "approvedDate": "2025-06-11T14:30:00Z"
      }
    ],
    "summary": {
      "totalRequests": 4,
      "totalApprovedCost": 8750.00,
      "totalPendingCost": 3500.00,
      "budgetImpactPercentage": 18.8,
      "averageApprovalTime": "1.8 days",
      "clientApprovalPending": 2
    }
  },
  "errors": []
}
```

## 🔄 Work Request Approval Workflow

The approval workflow system enables multi-level approval for work requests with complete audit trail and notifications.

### 📤 Submit Work Request for Approval

**POST** `/api/v1/work-requests/{id}/submit`

Submit a work request for management approval. Only the request creator can submit their own requests.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "comments": "Ready for review and approval. All details and pricing have been verified."
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request submitted for approval",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "title": "Battery Backup System Addition",
    "status": "Submitted",
    "approvalStatus": "PendingApproval",
    "updatedAt": "2025-06-15T14:30:00Z",
    "nextApprover": {
      "role": "Manager",
      "name": "Sarah Manager"
    }
  },
  "errors": []
}
```

### ✅ Process Approval (Approve/Reject)

**POST** `/api/v1/work-requests/{id}/approve`

Process approval for a work request. Managers can approve requests up to their authority level, Admins can provide final approval.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "approved": true,
  "comments": "Approved. This change aligns with our service offerings and the client's needs.",
  "authorizationCode": "MGR2025-06-15"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request approved successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "title": "Battery Backup System Addition",
    "status": "Approved",
    "approvalStatus": "Approved",
    "updatedAt": "2025-06-15T16:15:00Z",
    "approvedBy": "Sarah Manager",
    "approvalDate": "2025-06-15T16:15:00Z",
    "nextStep": "Generate Work Order"
  },
  "errors": []
}
```

### 🔍 Get Approval Status

**GET** `/api/v1/work-requests/{id}/approval-status`

Get the current approval workflow status for a work request.

**Path Parameters**:
- `id` (Guid): Work request ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Approval status retrieved successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "title": "Battery Backup System Addition",
    "currentStatus": "Approved",
    "approvalStages": [
      {
        "stage": "Submitted",
        "status": "Completed",
        "by": "John Technician",
        "date": "2025-06-15T14:30:00Z",
        "comments": "Ready for review and approval. All details and pricing have been verified."
      },
      {
        "stage": "Manager Review",
        "status": "Completed",
        "by": "Sarah Manager",
        "date": "2025-06-15T16:15:00Z",
        "comments": "Approved. This change aligns with our service offerings and the client's needs."
      },
      {
        "stage": "Admin Approval",
        "status": "Pending",
        "by": null,
        "date": null,
        "comments": null
      },
      {
        "stage": "Client Acceptance",
        "status": "NotStarted",
        "by": null,
        "date": null,
        "comments": null
      }
    ],
    "estimatedCompletion": "2025-06-17T00:00:00Z"
  },
  "errors": []
}
```

## 📊 Advanced Analytics & Reporting

### Get Work Request Analytics

**GET** `/api/v1/work-requests/projects/{projectId}/analytics`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get comprehensive analytics for work requests within a project.

**Query Parameters**:
- `startDate` (DateTime): Analysis start date (default: project start)
- `endDate` (DateTime): Analysis end date (default: today)
- `includeFinancial` (bool): Include detailed financial analysis
- `includeTrends` (bool): Include trend analysis data

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request analytics retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "analysisPeriod": {
      "startDate": "2025-06-01",
      "endDate": "2025-06-29"
    },
    "summary": {
      "totalRequests": 12,
      "totalEstimatedCost": 24750.00,
      "totalApprovedCost": 18250.00,
      "averageRequestCost": 2062.50,
      "averageApprovalTime": 2.3,
      "approvalRate": 83.3,
      "budgetImpact": 28.1
    },
    "statusDistribution": {
      "Draft": 1,
      "Submitted": 2,
      "Approved": 7,
      "Rejected": 1,
      "InProgress": 1,
      "Completed": 0
    },
    "typeDistribution": {
      "ClientRequest": 5,
      "ChangeOrder": 3,
      "AdditionalWork": 2,
      "ScopeModification": 1,
      "Emergency": 1
    },
    "costAnalysis": {
      "materialsPercentage": 68.5,
      "laborPercentage": 24.2,
      "overheadPercentage": 7.3,
      "averageMargin": 22.8
    },
    "approvalPerformance": {
      "averageApprovalTime": 2.3,
      "fastestApproval": 0.5,
      "slowestApproval": 5.2,
      "approvalsByRole": {
        "Manager": 8,
        "Admin": 2,
        "Client": 6
      }
    },
    "trends": [
      {
        "month": "2025-06",
        "requestCount": 12,
        "totalCost": 24750.00,
        "approvalRate": 83.3
      }
    ]
  },
  "errors": []
}
```

### Generate Change Order Document

**POST** `/api/v1/work-requests/{id}/generate-change-order`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Generate a formal change order document for client approval.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "includeDetailedCosts": true,
  "includeScheduleImpact": true,
  "clientSignatureRequired": true,
  "deliveryMethod": "Email",
  "clientEmail": "client@example.com",
  "additionalNotes": "This change order reflects the client's request for enhanced electrical capacity."
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Change order generated successfully",
  "data": {
    "changeOrderId": "CO-2025-06-001",
    "workRequestId": "345g7890-e89b-12d3-a456-426614174000",
    "documentUrl": "/api/v1/documents/change-orders/CO-2025-06-001.pdf",
    "status": "Generated",
    "sentToClient": true,
    "sentAt": "2025-06-15T16:45:00Z",
    "expirationDate": "2025-06-22T23:59:59Z",
    "trackingCode": "CO2025060001"
  },
  "errors": []
}
```

### Get Financial Impact Report

**GET** `/api/v1/work-requests/projects/{projectId}/financial-impact`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get detailed financial impact analysis for all work requests in a project.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Financial impact report generated successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "originalBudget": 65000.00,
    "currentBudget": 83250.00,
    "totalWorkRequestCost": 18250.00,
    "budgetVariance": 28.08,
    "profitabilityAnalysis": {
      "originalMargin": 25.0,
      "currentMargin": 22.8,
      "marginImpact": -2.2,
      "profitDollarImpact": -1430.00
    },
    "cashFlowImpact": {
      "additionalRevenue": 18250.00,
      "additionalCosts": 14196.00,
      "netCashFlowImprovement": 4054.00
    },
    "riskAssessment": {
      "riskLevel": "Medium",
      "riskFactors": [
        "Budget variance above 25%",
        "Multiple scope changes may indicate scope creep"
      ],
      "recommendations": [
        "Implement stricter change control procedures",
        "Regular client communication about scope boundaries"
      ]
    }
  },
  "errors": []
}
```

## 🔄 Bulk Operations

### Bulk Approve Work Requests

**POST** `/api/v1/work-requests/bulk-approve`

**🔒 Required Roles**: Admin, Manager

Approve multiple work requests in a single operation.

**Request Body**:
```json
{
  "workRequestIds": [
    "request1-id",
    "request2-id",
    "request3-id"
  ],
  "comments": "Bulk approval for standard electrical upgrade requests",
  "authorizationCode": "BULK-2025-06-15"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Bulk approval completed",
  "data": {
    "totalRequested": 3,
    "successCount": 3,
    "failureCount": 0,
    "totalApprovedCost": 4750.00,
    "results": [
      {
        "workRequestId": "request1-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "workRequestId": "request2-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "workRequestId": "request3-id",
        "success": true,
        "details": "Approved successfully"
      }
    ]
  },
  "errors": []
}
```

## 📊 Work Request Status Types & Workflow

| Status | Description | Can be Edited | Next Status | Required Role | Automated Actions |
|--------|-------------|--------------|-------------|---------------|-------------------|
| **Draft** | Initial creation, editable | Yes, by creator | Submitted | Creator | Auto-save, validation checks |
| **Submitted** | Sent for approval | No | Approved, Rejected, UnderReview | N/A | Notifications sent to approvers |
| **UnderReview** | Being reviewed by approvers | No | Approved, Rejected, RevisionRequired | Approver | Reminder notifications |
| **RevisionRequired** | Needs changes before approval | Yes, by creator | Submitted | Creator | Notification to creator |
| **Approved** | Approved for execution | Admin only | InProgress, Cancelled | Manager/Admin | Change order generation |
| **Rejected** | Declined, needs major revision | Yes, by creator | Submitted | Creator | Rejection notification |
| **InProgress** | Work has started | Admin only | Completed, OnHold | Manager/Admin | Resource allocation |
| **OnHold** | Temporarily suspended | Admin only | InProgress, Cancelled | Manager/Admin | Hold notification |
| **Completed** | Work finished successfully | No | Closed | Manager/Admin | Completion documentation |
| **Cancelled** | Cancelled before/during execution | No | (Final state) | Admin | Cancellation documentation |
| **Closed** | Administratively closed | No | (Final state) | Admin | Archive and reporting |

### 🔄 Approval Workflow Types

| Workflow Type | Description | Approval Levels | Cost Threshold | Timeline |
|---------------|-------------|-----------------|----------------|----------|
| **Standard** | Regular change requests | Technical → Manager → Client | $0 - $2,500 | 2-3 business days |
| **Enhanced** | Moderate cost requests | Technical → Manager → Admin → Client | $2,500 - $10,000 | 3-5 business days |
| **Executive** | High-value requests | Technical → Manager → Admin → Executive → Client | $10,000+ | 5-7 business days |
| **Emergency** | Urgent safety/compliance | Technical → Manager (concurrent) | Any amount | 4-24 hours |
| **Client-Direct** | Client-initiated requests | Client → Manager → Admin | Any amount | 1-3 business days |

## 🎯 Summary: Work Requests as Project Change Management Hub

Work Requests serve as the **comprehensive change management and scope control system** for solar installation projects, providing:

### 💼 Professional Change Management
- **Scope Control**: Rigorous documentation and approval processes for all project changes
- **Cost Management**: Detailed cost analysis with profit margin protection and budget impact assessment
- **Client Relations**: Professional change order generation with clear documentation and approval workflows
- **Risk Mitigation**: Risk assessment and mitigation strategies for scope changes and additions

### 🔄 Advanced Workflow Automation
- **Intelligent Routing**: Automatic routing based on cost thresholds, project types, and organizational hierarchy
- **Approval Optimization**: Parallel and sequential approval processes optimized for efficiency
- **Escalation Management**: Automatic escalation for overdue approvals and high-value requests
- **Integration Automation**: Seamless integration with project schedules, budgets, and resource allocation

### � Financial Intelligence & Controls
- **Budget Protection**: Real-time budget impact analysis with variance tracking and alerting
- **Profitability Monitoring**: Margin analysis and profit protection for all scope changes
- **Cash Flow Management**: Cash flow impact assessment and payment scheduling
- **Cost Validation**: Multi-level cost validation with market rate comparisons

### 🤖 AI-Powered Optimization
- **Predictive Analytics**: Machine learning-powered cost estimation and timeline prediction
- **Pattern Recognition**: Identification of scope creep patterns and client behavior analysis
- **Risk Assessment**: Automated risk scoring based on historical data and project parameters
- **Performance Analytics**: Approval efficiency analysis and process optimization recommendations

### 🔗 Ecosystem Integration
- **Master Plan Synchronization**: Automatic schedule updates and resource reallocation
- **Task Generation**: Automatic creation of work orders and task assignments from approved requests
- **Accounting Integration**: Direct integration with accounting systems for billing and cost tracking
- **Client Portal Connection**: Real-time client visibility and approval capabilities

### 📈 Strategic Business Value
- **Change Order Profitability**: Optimization of change order pricing and margin protection
- **Client Satisfaction**: Professional change management processes enhance client relationships
- **Project Delivery**: Improved project delivery through systematic scope management
- **Operational Efficiency**: Streamlined approval processes reduce administrative overhead

### 🎨 Advanced Features
- **Template Management**: Project-type-specific templates with intelligent cost estimation
- **Bulk Operations**: Efficient management of multiple requests for large projects
- **Document Generation**: Automated creation of professional change orders and contracts
- **Performance Dashboards**: Real-time visibility into change management performance

Work Requests transform ad-hoc change management into a strategic business process that protects profitability, enhances client relationships, and ensures successful project delivery while maintaining full control over scope and costs.

## �🔧 Enhanced Work Request Error Codes

## 🔧 Enhanced Work Request Error Codes

### Core Operations
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR001** | Work request not found | Verify request ID exists and user has access |
| **WR002** | Insufficient permissions | Check user role requirements and project assignment |
| **WR003** | Invalid request data | Check request body for required fields and validation rules |
| **WR004** | Invalid status transition | Follow the defined workflow sequence |
| **WR005** | Cannot modify submitted request | Create new request or contact admin for revision |
| **WR006** | Cannot approve own request | Request approval from another authorized user |
| **WR007** | Missing approval information | Provide all required approval fields and authorization |

### Business Logic & Validation
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR008** | Cost exceeds authorization limit | Escalate to higher authority or reduce scope/cost |
| **WR009** | Budget impact exceeds threshold | Review project budget or seek executive approval |
| **WR010** | Required approval level not met | Ensure appropriate approval authority for cost level |
| **WR011** | Client approval required | Cannot proceed without client approval for this request type |
| **WR012** | Duplicate request exists | Check for existing similar requests before creating new |
| **WR013** | Invalid cost breakdown | Verify materials, labor, and overhead calculations |
| **WR014** | Schedule conflict detected | Request conflicts with existing project timeline |

### Workflow & Approval
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR015** | Approval workflow not configured | Configure approval workflow for project type |
| **WR016** | Approver not available | Assign alternate approver or escalate |
| **WR017** | Approval deadline exceeded | Contact approver or escalate for urgent approval |
| **WR018** | Invalid authorization code | Verify authorization code with approving authority |
| **WR019** | Workflow step cannot be skipped | Complete all required approval steps in sequence |
| **WR020** | Bulk operation partially failed | Check individual operation results for details |

### Project & Integration
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR021** | Project inactive or suspended | Work requests disabled for inactive projects |
| **WR022** | Project budget locked | Cannot create requests for budget-locked projects |
| **WR023** | Master plan integration failed | Check master plan status and integration settings |
| **WR024** | Resource allocation conflict | Request conflicts with existing resource assignments |
| **WR025** | Client portal access required | Client must access request through client portal |

### Financial & Compliance
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR026** | Cost validation failed | Verify cost calculations and market rates |
| **WR027** | Margin below threshold | Adjust pricing to meet minimum margin requirements |
| **WR028** | Change order generation failed | Check template configuration and data completeness |
| **WR029** | Financial approval required | Request requires finance department approval |
| **WR030** | Contract modification needed | Change requires formal contract amendment |

### System & Performance
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR031** | Document generation failed | Check template availability and data completeness |
| **WR032** | Email notification failed | Verify recipient email addresses and notification settings |
| **WR033** | Attachment upload failed | Check file size, format, and upload permissions |
| **WR034** | Analytics data unavailable | Insufficient data for requested analysis period |
| **WR035** | Export limit exceeded | Reduce export scope or request administrator assistance |
| **WR036** | Integration service unavailable | Third-party service temporarily unavailable |

---
*Last Updated: July 4, 2025*
