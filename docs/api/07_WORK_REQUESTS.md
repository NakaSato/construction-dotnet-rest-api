# 🔧 Work Requests

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (create requests), All authenticated users (view)

Work requests are used to document change orders, additional work, or modifications to the original project scope.

## ⚡ Work Request Capabilities

### Admin & Manager
- ✅ Create work requests for any project
- ✅ Review and approve/reject requests
- ✅ Modify request details and pricing
- ✅ Update status and priority
- ✅ Generate change orders
- ✅ Delete work requests

### Users (Technicians)
- ✅ Create work requests for assigned projects
- ✅ Submit requests for approval
- ✅ Track request status
- ❌ Cannot approve their own requests
- ❌ Cannot modify after submission
- ❌ Cannot delete requests

## 📋 Get All Work Requests

**GET** `/api/v1/work-requests`

Retrieve work requests with filtering options.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `requesterId` (Guid): Filter by requester
- `status` (string): Filter by status
- `priority` (string): Filter by priority
- `approvalStatus` (string): Filter by approval status
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

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
        "priority": "Medium",
        "status": "Submitted",
        "approvalStatus": "PendingApproval",
        "estimatedCost": 1250.00,
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
        "priority": "High",
        "status": "Approved",
        "approvalStatus": "Approved",
        "estimatedCost": 3800.00,
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
    "priority": "Medium",
    "status": "Submitted",
    "approvalStatus": "PendingApproval",
    "estimatedCost": 1250.00,
    "actualCost": null,
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
        "stage": "Submitted",
        "date": "2025-06-10T09:15:00Z",
        "by": "John Technician",
        "comments": "Initial submission"
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

## 📊 Work Request Status Types

| Status | Description | Can be Edited | Next Status |
|--------|-------------|--------------|-------------|
| **Draft** | Initial creation, editable | Yes, by creator | Submitted |
| **Submitted** | Sent for approval | No | Approved, Rejected |
| **Approved** | Approved for execution | No | InProgress |
| **Rejected** | Declined, needs revision | Yes, by creator | Submitted (after revision) |
| **InProgress** | Work has started | No | Completed |
| **Completed** | Work finished | No | Closed |
| **Closed** | Administratively closed | No | (Final state) |

## 🔧 Work Request Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **WR001** | Work request not found | Verify request ID exists |
| **WR002** | Insufficient permissions | Check user role requirements |
| **WR003** | Invalid request data | Check request body for required fields |
| **WR004** | Invalid status transition | Follow the defined workflow sequence |
| **WR005** | Cannot modify submitted request | Create new request or contact admin |
| **WR006** | Cannot approve own request | Request approval from another user |
| **WR007** | Missing approval information | Provide all required approval fields |

---
*Last Updated: June 15, 2025*
