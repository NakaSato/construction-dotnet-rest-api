# 📅 Weekly Planning

**⚡ Real-Time Live Updates**: This API supports real-time data synchronization. All changes are instantly broadcast to connected users via WebSocket.


**Base URL**: `/api/v1/weekly-planning`

**🔒 Authentication Required**  
**🎯 Status**: ✅ Available

The Weekly Planning module provides comprehensive weekly work planning, resource allocation, and progress tracking for solar installation projects.

## ⚡ Authorization & Access Control

| Role | Create Plans | Update Plans | Delete Plans | View Plans | Analytics | Approval |
|------|--------------|--------------|--------------|------------|-----------|----------|
| **Admin** | ✅ All Projects | ✅ All Projects | ✅ All Projects | ✅ All Plans | ✅ Full Access | ✅ Override |
| **Manager** | ✅ Managed Projects | ✅ Managed Projects | ✅ Managed Projects | ✅ All Plans | ✅ Full Access | ✅ Approve |
| **Supervisor** | ✅ Assigned Projects | ✅ Own Plans | ❌ No | ✅ Own Plans | ✅ Limited | ✅ Submit |
| **User** | ❌ No | ❌ No | ❌ No | ✅ Assigned Plans | ❌ No | ❌ No |

## 📋 Core Features

### 🎯 Planning Capabilities
- **Weekly Work Scheduling**: Plan tasks, resources, and timelines for upcoming weeks
- **Resource Allocation**: Assign team members, equipment, and materials
- **Progress Forecasting**: Predict completion timelines based on current progress
- **Risk Assessment**: Identify potential delays and resource conflicts

### 📊 Analytics & Reporting
- **Utilization Metrics**: Track team and equipment utilization rates
- **Performance Trends**: Analyze weekly productivity and completion rates
- **Resource Optimization**: Recommendations for better resource allocation
- **Timeline Accuracy**: Compare planned vs actual completion times

### 🔄 Workflow Integration
- **Master Plan Sync**: Automatically align with master plan schedules
- **Task Dependencies**: Manage task sequences and dependencies
- **Progress Updates**: Real-time updates from daily reports
- **Change Management**: Handle scope changes and timeline adjustments

## 📋 Get Weekly Plans

**GET** `/api/v1/weekly-planning`

Retrieve weekly plans with advanced filtering and analytics.

**Query Parameters**:
- `weekStartDate` (datetime): Start date for week filter
- `weekEndDate` (datetime): End date for week filter  
- `projectId` (guid): Filter by specific project
- `teamMemberId` (guid): Filter by assigned team member
- `status` (string): Filter by plan status
- `includeAnalytics` (bool): Include performance analytics (default: false)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly plans retrieved successfully",
  "data": {
    "items": [
      {
        "weeklyPlanId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
        "projectName": "Solar Installation - Building A",
        "weekStartDate": "2024-06-17T00:00:00Z",
        "weekEndDate": "2024-06-23T23:59:59Z",
        "status": "Active",
        "plannerInfo": {
          "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
          "fullName": "John Smith",
          "role": "Manager"
        },
        "summary": {
          "totalTasks": 15,
          "completedTasks": 8,
          "inProgressTasks": 5,
          "pendingTasks": 2,
          "completionPercentage": 53.3,
          "estimatedHours": 120,
          "actualHours": 64
        },
        "taskBreakdown": [
          {
            "category": "Installation",
            "plannedTasks": 8,
            "completedTasks": 4,
            "estimatedHours": 60,
            "actualHours": 32
          },
          {
            "category": "Electrical",
            "plannedTasks": 4,
            "completedTasks": 3,
            "estimatedHours": 40,
            "actualHours": 24
          }
        ],
        "resourceAllocation": {
          "teamMembers": [
            {
              "userId": "user123",
              "fullName": "Mike Johnson",
              "role": "Technician",
              "allocatedHours": 40,
              "utilization": 85.5
            }
          ],
          "equipment": [
            {
              "equipmentId": "eq001",
              "name": "Crane - 25 Ton",
              "allocatedDays": 3,
              "utilizationRate": 75.0
            }
          ]
        },
        "risks": [
          {
            "riskId": "risk001",
            "description": "Weather delay potential",
            "probability": "Medium",
            "impact": "Low",
            "mitigation": "Indoor backup tasks prepared"
          }
        ],
        "createdAt": "2024-06-15T08:00:00Z",
        "updatedAt": "2024-06-16T14:30:00Z"
      }
    ],
    "analytics": {
      "averageCompletionRate": 67.8,
      "teamUtilization": 82.3,
      "equipmentUtilization": 74.6,
      "onTimeDelivery": 89.2,
      "trends": {
        "productivityTrend": "Increasing",
        "resourceEfficiency": "Stable",
        "planAccuracy": "Improving"
      }
    },
    "pagination": {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3
    }
  }
}
```

## 🔍 Get Weekly Plan by ID

**GET** `/api/v1/weekly-planning/{id}`

Retrieve detailed information for a specific weekly plan.

**Path Parameters**:
- `id` (guid) - Weekly plan ID

**Query Parameters**:
- `includeDetails` (bool): Include detailed task and resource information (default: true)
- `includeAnalytics` (bool): Include performance analytics (default: false)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly plan retrieved successfully",
  "data": {
    "weeklyPlanId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "projectName": "Solar Installation - Building A",
    "weekStartDate": "2024-06-17T00:00:00Z",
    "weekEndDate": "2024-06-23T23:59:59Z",
    "status": "Active",
    "description": "Week 25 installation plan focusing on rooftop mounting",
    "objectives": [
      "Complete mounting system installation",
      "Install 50% of solar panels",
      "Begin electrical connections"
    ],
    "plannerInfo": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "fullName": "John Smith",
      "role": "Manager",
      "email": "john.smith@company.com"
    },
    "detailedTasks": [
      {
        "taskId": "task001",
        "title": "Install mounting rails - Section A",
        "description": "Install mounting rails for solar panels on building section A",
        "category": "Installation",
        "priority": "High",
        "status": "Completed",
        "plannedStartDate": "2024-06-17T08:00:00Z",
        "plannedEndDate": "2024-06-17T16:00:00Z",
        "actualStartDate": "2024-06-17T08:15:00Z",
        "actualEndDate": "2024-06-17T15:45:00Z",
        "estimatedHours": 8,
        "actualHours": 7.5,
        "assignedTo": [
          {
            "userId": "user123",
            "fullName": "Mike Johnson",
            "role": "Technician"
          }
        ],
        "dependencies": [],
        "materials": [
          {
            "itemId": "mat001",
            "name": "Mounting Rails - 3m",
            "quantity": 20,
            "unit": "pieces"
          }
        ]
      }
    ],
    "resourceDetails": {
      "teamAssignments": [
        {
          "userId": "user123",
          "fullName": "Mike Johnson",
          "role": "Technician",
          "plannedHours": 40,
          "actualHours": 36,
          "efficiency": 90.0,
          "assignments": [
            {
              "taskId": "task001",
              "hours": 8,
              "date": "2024-06-17"
            }
          ]
        }
      ],
      "equipmentSchedule": [
        {
          "equipmentId": "eq001",
          "name": "Crane - 25 Ton",
          "plannedDays": 3,
          "actualDays": 2.5,
          "schedule": [
            {
              "date": "2024-06-17",
              "startTime": "08:00",
              "endTime": "16:00",
              "taskId": "task001"
            }
          ]
        }
      ],
      "materialConsumption": [
        {
          "itemId": "mat001",
          "name": "Mounting Rails - 3m",
          "plannedQuantity": 20,
          "actualQuantity": 18,
          "unit": "pieces",
          "costPlanned": 2000.00,
          "costActual": 1800.00
        }
      ]
    },
    "milestones": [
      {
        "milestoneId": "ms001",
        "title": "Mounting System Complete",
        "targetDate": "2024-06-19T17:00:00Z",
        "actualDate": "2024-06-19T16:30:00Z",
        "status": "Completed",
        "impact": "Critical Path"
      }
    ],
    "riskAssessment": {
      "overallRiskLevel": "Medium",
      "risks": [
        {
          "riskId": "risk001",
          "category": "Weather",
          "description": "Potential rain delays midweek",
          "probability": "Medium",
          "impact": "Low",
          "mitigation": "Indoor backup tasks identified",
          "status": "Active",
          "owner": "John Smith"
        }
      ]
    },
    "performance": {
      "scheduledAdherence": 94.2,
      "budgetAdherence": 97.8,
      "qualityScore": 96.5,
      "safetyScore": 100.0,
      "teamMorale": 92.0
    },
    "createdAt": "2024-06-15T08:00:00Z",
    "updatedAt": "2024-06-20T16:45:00Z",
    "approvalStatus": "Approved",
    "approvedBy": {
      "userId": "mgr001",
      "fullName": "Sarah Davis",
      "role": "Manager",
      "approvedAt": "2024-06-15T09:30:00Z"
    }
  }
}
```

## 📝 Create Weekly Plan

**POST** `/api/v1/weekly-planning`

Create a new weekly plan for a project.

**Authorization**: Admin, Manager, Supervisor (for assigned projects)

**Request Body**:
```json
{
  "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
  "weekStartDate": "2024-06-24T00:00:00Z",
  "weekEndDate": "2024-06-30T23:59:59Z",
  "description": "Week 26 plan focusing on electrical connections",
  "objectives": [
    "Complete electrical panel installation",
    "Connect 75% of solar panels",
    "Conduct initial system testing"
  ],
  "tasks": [
    {
      "title": "Install electrical panel",
      "description": "Install main electrical panel for solar system",
      "category": "Electrical",
      "priority": "Critical",
      "plannedStartDate": "2024-06-24T08:00:00Z",
      "plannedEndDate": "2024-06-24T17:00:00Z",
      "estimatedHours": 8,
      "assignedUserIds": ["user123"],
      "dependencies": ["task002"],
      "requiredMaterials": [
        {
          "itemId": "mat002",
          "quantity": 1,
          "unit": "piece"
        }
      ]
    }
  ],
  "resourceRequirements": {
    "teamMembers": [
      {
        "userId": "user123",
        "plannedHours": 40,
        "role": "Lead Technician"
      }
    ],
    "equipment": [
      {
        "equipmentId": "eq002",
        "plannedDays": 2,
        "purpose": "Panel installation"
      }
    ]
  },
  "risks": [
    {
      "description": "Electrical inspection delay",
      "probability": "Low",
      "impact": "Medium",
      "mitigation": "Pre-schedule inspection for early week"
    }
  ],
  "budgetEstimate": {
    "laborCost": 4800.00,
    "materialCost": 2500.00,
    "equipmentCost": 800.00,
    "totalEstimate": 8100.00
  }
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Weekly plan created successfully",
  "data": {
    "weeklyPlanId": "new-plan-guid",
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "weekStartDate": "2024-06-24T00:00:00Z",
    "weekEndDate": "2024-06-30T23:59:59Z",
    "status": "Draft",
    "approvalStatus": "Pending",
    "createdAt": "2024-06-20T10:00:00Z"
  }
}
```

## ✏️ Update Weekly Plan

**PUT** `/api/v1/weekly-planning/{id}`

Update an existing weekly plan.

**Authorization**: Admin, Manager, Supervisor (for own plans)

**Path Parameters**:
- `id` (guid) - Weekly plan ID

**Request Body**: Similar to create request, with updated fields

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly plan updated successfully",
  "data": {
    "weeklyPlanId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "updatedAt": "2024-06-20T14:30:00Z",
    "changes": [
      {
        "field": "tasks",
        "action": "added",
        "description": "Added 2 new tasks"
      },
      {
        "field": "resources",
        "action": "modified",
        "description": "Updated team member allocation"
      }
    ]
  }
}
```

## 🗑️ Delete Weekly Plan

**DELETE** `/api/v1/weekly-planning/{id}`

Delete a weekly plan.

**Authorization**: Admin, Manager (all plans), Supervisor (own plans in draft status)

**Path Parameters**:
- `id` (guid) - Weekly plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly plan deleted successfully",
  "data": {
    "weeklyPlanId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "deletedAt": "2024-06-20T15:00:00Z"
  }
}
```

## 📊 Weekly Plan Analytics

**GET** `/api/v1/weekly-planning/analytics`

Get comprehensive analytics for weekly planning performance.

**Query Parameters**:
- `projectId` (guid): Filter by specific project
- `dateFrom` (datetime): Start date for analytics period
- `dateTo` (datetime): End date for analytics period
- `granularity` (string): Data granularity ("week", "month", "quarter")
- `metrics` (string): Comma-separated list of metrics to include

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Analytics retrieved successfully",
  "data": {
    "period": {
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-06-20T23:59:59Z",
      "totalWeeks": 25
    },
    "overallMetrics": {
      "planAccuracy": 87.3,
      "averageCompletionRate": 92.1,
      "resourceUtilization": 85.7,
      "budgetVariance": -2.4,
      "timelineAdherence": 94.8,
      "qualityScore": 96.2
    },
    "trends": {
      "planningEfficiency": {
        "trend": "Improving",
        "changePercent": 12.5,
        "periodComparison": "vs last quarter"
      },
      "resourceOptimization": {
        "trend": "Stable",
        "changePercent": 1.2,
        "periodComparison": "vs last quarter"
      }
    },
    "performanceBreakdown": {
      "byProject": [
        {
          "projectId": "proj001",
          "projectName": "Building A Solar",
          "completionRate": 94.5,
          "budgetVariance": -1.2,
          "timelineAdherence": 96.8
        }
      ],
      "byTeam": [
        {
          "teamMember": "John Smith",
          "role": "Manager",
          "plansCreated": 15,
          "averageAccuracy": 91.2,
          "productivity": 105.3
        }
      ],
      "byCategory": [
        {
          "category": "Installation",
          "completionRate": 95.2,
          "averageDuration": 6.5,
          "resourceEfficiency": 88.9
        }
      ]
    },
    "recommendations": [
      {
        "type": "Resource Optimization",
        "priority": "High",
        "description": "Consider redistributing electricians to balance workload",
        "potentialImpact": "15% efficiency improvement"
      },
      {
        "type": "Timeline Optimization",
        "priority": "Medium", 
        "description": "Implement weather contingency buffers",
        "potentialImpact": "Reduce weather delays by 30%"
      }
    ],
    "riskAnalysis": {
      "commonRisks": [
        {
          "riskType": "Weather",
          "frequency": 45.2,
          "averageImpact": "Low",
          "mitigation": "Indoor backup tasks"
        }
      ],
      "upcomingRisks": [
        {
          "weekStartDate": "2024-06-24T00:00:00Z",
          "riskLevel": "Medium",
          "description": "Holiday week - reduced workforce"
        }
      ]
    }
  }
}
```

## 🔄 Plan Status Management

**PATCH** `/api/v1/weekly-planning/{id}/status`

Update the status of a weekly plan.

**Authorization**: Admin, Manager, Supervisor (for own plans)

**Path Parameters**:
- `id` (guid) - Weekly plan ID

**Request Body**:
```json
{
  "status": "Active",
  "reason": "All resources confirmed and ready to proceed",
  "effectiveDate": "2024-06-24T08:00:00Z"
}
```

**Available Statuses**:
- `Draft` - Plan in development
- `PendingApproval` - Submitted for approval
- `Approved` - Approved and ready for execution
- `Active` - Currently being executed
- `Completed` - Plan execution completed
- `Cancelled` - Plan cancelled
- `OnHold` - Plan temporarily suspended

## 🎯 Plan Approval Workflow

**POST** `/api/v1/weekly-planning/{id}/approve`

Approve a weekly plan for execution.

**Authorization**: Manager, Admin

**Path Parameters**:
- `id` (guid) - Weekly plan ID

**Request Body**:
```json
{
  "approved": true,
  "comments": "Plan approved with minor resource adjustments",
  "conditions": [
    "Ensure weather monitoring in place",
    "Confirm equipment availability before start"
  ]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly plan approval processed successfully",
  "data": {
    "weeklyPlanId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "approvalStatus": "Approved",
    "approvedBy": {
      "userId": "mgr001",
      "fullName": "Sarah Davis",
      "role": "Manager"
    },
    "approvedAt": "2024-06-20T11:30:00Z",
    "nextSteps": [
      "Plan status will be automatically updated to 'Active' on start date",
      "Team notifications will be sent",
      "Resource bookings will be confirmed"
    ]
  }
}
```

## ⚠️ Error Codes

| Code | Message | Description | Solution |
|------|---------|-------------|----------|
| **WP001** | Plan validation failed | Required fields missing or invalid | Check all required fields are provided |
| **WP002** | Resource conflict detected | Assigned resources not available | Review resource allocation |
| **WP003** | Invalid date range | Week dates are invalid or overlap | Ensure proper week boundaries |
| **WP004** | Project not found | Referenced project doesn't exist | Verify project ID |
| **WP005** | Insufficient permissions | User lacks permission for operation | Check user role and permissions |
| **WP006** | Plan locked for editing | Plan is approved and cannot be modified | Create new version or contact admin |
| **WP007** | Dependency conflict | Task dependencies create circular reference | Review task dependencies |
| **WP008** | Budget exceeded | Planned costs exceed project budget | Review cost estimates |
| **WP009** | Resource unavailable | Required resources not available for dates | Adjust dates or find alternatives |
| **WP010** | Approval required | Plan must be approved before execution | Submit for approval workflow |

## 📋 Summary

### Key Features
- **Comprehensive Planning**: Full weekly work planning with task, resource, and timeline management
- **Resource Optimization**: Advanced resource allocation and utilization tracking
- **Performance Analytics**: Detailed metrics and trend analysis
- **Risk Management**: Proactive risk identification and mitigation planning
- **Approval Workflow**: Structured approval process for plan execution
- **Real-time Updates**: Integration with daily reports and task management

### Integration Points
- **Master Plans**: Automatic alignment with long-term project schedules
- **Daily Reports**: Real-time progress updates feed into plan tracking
- **Task Management**: Tasks created from weekly plans automatically sync
- **Resource Management**: Equipment and team availability checking
- **Budget Tracking**: Cost estimation and variance analysis

### Best Practices
- Create plans at least one week in advance
- Include contingency time for weather and unforeseen issues  
- Regularly update resource availability and skills
- Use analytics to improve future planning accuracy
- Maintain clear task dependencies and sequencing
- Document lessons learned for process improvement
