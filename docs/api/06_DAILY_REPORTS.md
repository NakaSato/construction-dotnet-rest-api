# 📊 Daily Reports

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (create own reports), All authenticated users (view)

Daily reports provide a way to track daily work progress, hours worked, and issues encountered within the context of a specific **Project**. Each Daily Report belongs to a Project and serves as a detailed work log for solar installation projects.

## 🏗️ Project-Daily Report Relationship

- **Project** is the primary reference entity containing high-level project information
- **Daily Report** belongs to a Project (many:1 relationship) and captures daily work activities
- A Project can have multiple Daily Reports (one per worker per day)
- Daily Reports inherit project context (location, team, objectives) from their parent Project
- All reporting and analysis is organized by Project for proper tracking

## ⚡ Daily Report Capabilities

### Admin & Manager
- ✅ Create reports for any project
- ✅ View all reports across projects
- ✅ Update any report details
- ✅ Generate summary reports
- ✅ Export report data
- ✅ Delete reports if needed

### Users (Technicians)
- ✅ Create their own daily reports
- ✅ Update their own reports (within 24h)
- ✅ Attach photos to reports
- ✅ Report issues and blockers
- ❌ Cannot modify others' reports
- ❌ Cannot delete reports

## 📋 Get All Daily Reports

**GET** `/api/v1/daily-reports`

Retrieve daily reports with filtering options.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `userId` (Guid): Filter by reporting user
- `startDate` (DateTime): Filter from date
- `endDate` (DateTime): Filter to date
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily reports retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "789e0123-e89b-12d3-a456-426614174002",
        "userName": "John Technician",
        "reportDate": "2025-06-14",
        "hoursWorked": 8.5,
        "weatherConditions": "Sunny, 85°F",
        "summary": "Completed installation of 24 solar panels on south-facing roof section",
        "issues": "None",
        "tasksCompleted": ["Install mounting rails", "Install panels in section A"],
        "createdAt": "2025-06-14T17:30:00Z",
        "hasAttachments": true
      },
      {
        "id": "234f6789-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "890f1234-e89b-12d3-a456-426614174002",
        "userName": "Sarah Electrician",
        "reportDate": "2025-06-14",
        "hoursWorked": 7.0,
        "weatherConditions": "Sunny, 85°F",
        "summary": "Completed electrical wiring for inverter connections",
        "issues": "Permit inspector delayed until tomorrow",
        "tasksCompleted": ["Wire inverters", "Test connections"],
        "createdAt": "2025-06-14T16:45:00Z",
        "hasAttachments": false
      }
    ],
    "pagination": {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 5,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

## 🔍 Get Daily Report by ID

**GET** `/api/v1/daily-reports/{id}`

Retrieve details of a specific daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "userName": "John Technician",
    "reportDate": "2025-06-14",
    "hoursWorked": 8.5,
    "weatherConditions": "Sunny, 85°F",
    "summary": "Completed installation of 24 solar panels on south-facing roof section",
    "issues": "None",
    "tasksCompleted": [
      {
        "taskId": "task1-id",
        "title": "Install mounting rails",
        "completionPercentage": 100
      },
      {
        "taskId": "task2-id",
        "title": "Install panels in section A",
        "completionPercentage": 100
      }
    ],
    "materialsUsed": [
      {
        "name": "Mounting rails",
        "quantity": 12,
        "unit": "pieces"
      },
      {
        "name": "Solar panels",
        "quantity": 24,
        "unit": "panels"
      }
    ],
    "attachments": [
      {
        "id": "att1-id",
        "fileName": "completed_installation.jpg",
        "fileType": "image/jpeg",
        "fileSize": 1540000,
        "uploadedAt": "2025-06-14T17:25:00Z",
        "thumbnailUrl": "/api/v1/attachments/att1-id/thumbnail"
      }
    ],
    "createdAt": "2025-06-14T17:30:00Z",
    "updatedAt": null
  },
  "errors": []
}
```

## 📝 Create Daily Report

**POST** `/api/v1/daily-reports`

Create a new daily report for work completed.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "reportDate": "2025-06-15",
  "hoursWorked": 9.0,
  "weatherConditions": "Partly cloudy, 82°F",
  "summary": "Completed electrical connections and initial testing of inverter system",
  "issues": "Waiting for city inspector clearance before final connection",
  "tasksCompleted": [
    {
      "taskId": "task3-id",
      "completionPercentage": 100
    },
    {
      "taskId": "task4-id",
      "completionPercentage": 75
    }
  ],
  "materialsUsed": [
    {
      "name": "Electrical cable",
      "quantity": 150,
      "unit": "feet"
    },
    {
      "name": "Conduit",
      "quantity": 40,
      "unit": "feet"
    }
  ]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Daily report created successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "projectName": "Solar Installation Project Alpha",
    "reportDate": "2025-06-15",
    "hoursWorked": 9.0,
    "createdAt": "2025-06-15T18:15:00Z"
  },
  "errors": []
}
```

## 🔄 Update Daily Report

**PUT** `/api/v1/daily-reports/{id}`

**🔒 Requires**: Admin, Manager, or report creator (within 24h)

Update an existing daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
```json
{
  "hoursWorked": 9.5,
  "weatherConditions": "Partly cloudy with afternoon rain, 80°F",
  "summary": "Completed electrical connections and initial testing of inverter system. Rain delayed final testing.",
  "issues": "Waiting for city inspector clearance before final connection. Weather delays.",
  "tasksCompleted": [
    {
      "taskId": "task3-id",
      "completionPercentage": 100
    },
    {
      "taskId": "task4-id",
      "completionPercentage": 85
    }
  ],
  "materialsUsed": [
    {
      "name": "Electrical cable",
      "quantity": 175,
      "unit": "feet"
    },
    {
      "name": "Conduit",
      "quantity": 45,
      "unit": "feet"
    }
  ]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report updated successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "hoursWorked": 9.5,
    "updatedAt": "2025-06-15T19:30:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Daily Report

**DELETE** `/api/v1/daily-reports/{id}`

**🔒 Required Roles**: Admin, Manager

Delete a daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report deleted successfully",
  "data": null,
  "errors": []
}
```

## 📎 Add Attachment to Report

**POST** `/api/v1/daily-reports/{id}/attachments`

**🔒 Requires**: Admin, Manager, or report creator

Add a photo or document attachment to a daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
- Multipart form data with file

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Attachment added successfully",
  "data": {
    "id": "att2-id",
    "fileName": "inverter_installation.jpg",
    "fileType": "image/jpeg",
    "fileSize": 2100000,
    "uploadedAt": "2025-06-15T19:45:00Z",
    "thumbnailUrl": "/api/v1/attachments/att2-id/thumbnail"
  },
  "errors": []
}
```

## 📊 Export Daily Reports

**GET** `/api/v1/daily-reports/export`

**🔒 Required Roles**: Admin, Manager

Export daily reports in various formats.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `startDate` (DateTime): Start date
- `endDate` (DateTime): End date
- `format` (string): Export format ("csv", "excel", "pdf")

**Success Response (200)**:
- File download response with appropriate content type

## 📑 Weekly Summary Report

**GET** `/api/v1/daily-reports/weekly-summary`

**🔒 Required Roles**: Admin, Manager

Get summary of hours, progress, and issues for a weekly period.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `weekStartDate` (DateTime): Beginning of week

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly summary retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "weekStarting": "2025-06-09",
    "weekEnding": "2025-06-15",
    "totalHoursLogged": 187.5,
    "totalTasksCompleted": 12,
    "overallProgress": 68,
    "dailySummaries": [
      {
        "date": "2025-06-09",
        "totalHours": 36.0,
        "employeeCount": 4,
        "majorTasks": ["Site preparation", "Material delivery"]
      },
      // More daily summaries...
    ],
    "issuesSummary": [
      {
        "category": "Weather",
        "occurrences": 2,
        "hoursImpacted": 8.0
      },
      {
        "category": "Permits",
        "occurrences": 1,
        "hoursImpacted": 4.0
      }
    ]
  },
  "errors": []
}
```

## 🏗️ Enhanced Project-Centric Daily Reports

### Get Project Daily Reports

**GET** `/api/v1/daily-reports/projects/{projectId}`

Retrieve daily reports for a specific project with enhanced filtering and analytics.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Query Parameters**:
- `startDate` (DateTime): Filter from date
- `endDate` (DateTime): Filter to date
- `approvalStatuses` (array): Filter by approval status
- `reporterIds` (array): Filter by reporter IDs
- `minSafetyScore` (int): Minimum safety score filter
- `minQualityScore` (int): Minimum quality score filter
- `hasCriticalIssues` (bool): Filter reports with critical issues
- `includeAnalytics` (bool): Include analytics data
- `includeInsights` (bool): Include AI insights
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project daily reports retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "projectCode": "SIP-2025-001",
        "projectLocation": "Austin, TX",
        "clientName": "Green Energy Solutions",
        "reportDate": "2025-06-29",
        "approvalStatus": "Approved",
        "reporterId": "789e0123-e89b-12d3-a456-426614174002",
        "reporterName": "John Technician",
        "totalWorkHours": 8.5,
        "personnelOnSite": 4,
        "weatherCondition": "Sunny",
        "temperature": 85.0,
        "humidity": 45,
        "weatherDescription": "Clear skies, ideal working conditions",
        "weatherImpact": "None",
        "workSummary": "Completed installation of 24 solar panels on south-facing roof section",
        "workAccomplished": "Installation of mounting rails and 24 solar panels",
        "workPlannedNextDay": "Electrical connections and inverter installation",
        "issuesEncountered": "None",
        "safetyScore": 10,
        "qualityScore": 9,
        "dailyProgressContribution": 12.5,
        "overallProgressPercentage": 68.0,
        "hasCriticalIssues": false,
        "requiresManagerAttention": false,
        "teamMembers": [
          {
            "userId": "user1-id",
            "name": "John Technician",
            "role": "Lead Installer",
            "hoursWorked": 8.5,
            "specialAssignments": "Panel alignment verification"
          }
        ],
        "taskProgressSummary": [
          {
            "taskId": "task1-id",
            "taskName": "Install mounting rails",
            "startPercentage": 0,
            "endPercentage": 100,
            "progressMade": 100,
            "status": "Completed",
            "isOnSchedule": true
          }
        ],
        "attachments": [
          {
            "id": "att1-id",
            "fileName": "installation_progress.jpg",
            "fileType": "image/jpeg",
            "category": "Photo",
            "uploadedAt": "2025-06-29T16:30:00Z",
            "thumbnailUrl": "/api/v1/attachments/att1-id/thumbnail"
          }
        ],
        "photoCount": 3,
        "documentCount": 1,
        "autoGeneratedInsights": [
          "Excellent progress rate - 25% above average",
          "Weather conditions optimal for solar installation",
          "Team efficiency high - completing tasks ahead of schedule"
        ],
        "recommendedActions": [
          "Continue current pace to maintain schedule",
          "Consider additional safety check for tomorrow's electrical work"
        ],
        "createdAt": "2025-06-29T17:00:00Z",
        "lastModifiedAt": "2025-06-29T17:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 25,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 3,
      "hasPreviousPage": false,
      "hasNextPage": true
    },
    "analytics": {
      "averageDailyProgress": 11.2,
      "averageSafetyScore": 9.4,
      "averageQualityScore": 8.8,
      "totalCriticalIssues": 2,
      "weatherImpactDays": 3
    }
  },
  "errors": []
}
```

### Create Enhanced Daily Report

**POST** `/api/v1/daily-reports/enhanced`

Create a comprehensive daily report with enhanced validation and project context.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "reportDate": "2025-06-29",
  "weatherCondition": "Sunny",
  "temperature": 85.0,
  "humidity": 45,
  "windSpeed": 12.0,
  "weatherDescription": "Clear blue skies with light breeze",
  "weatherImpact": "None",
  "workSummary": "Completed installation of 24 solar panels on south-facing roof section with high efficiency and safety standards",
  "workAccomplished": "Successfully installed mounting rails and 24 high-efficiency solar panels",
  "workPlannedNextDay": "Electrical connections, inverter installation, and initial system testing",
  "issuesEncountered": "None - all work proceeded as planned",
  "totalWorkHours": 8.5,
  "personnelOnSite": 4,
  "safetyScore": 10,
  "qualityScore": 9,
  "dailyProgressContribution": 12.5,
  "additionalNotes": "Team worked efficiently and maintained high safety standards throughout the day",
  "workProgressItems": [
    {
      "activity": "Solar panel installation",
      "description": "Installed 24 high-efficiency solar panels on south-facing roof",
      "hoursWorked": 6.0,
      "percentageComplete": 100,
      "workersAssigned": 3,
      "notes": "Installation completed without issues"
    },
    {
      "activity": "Mounting rail installation",
      "description": "Installed and secured mounting rails for panel attachment",
      "hoursWorked": 2.5,
      "percentageComplete": 100,
      "workersAssigned": 4,
      "notes": "Rails properly aligned and secured"
    }
  ],
  "personnelLogs": [
    {
      "userId": "user1-id",
      "hoursWorked": 8.5,
      "role": "Lead Installer",
      "specialAssignments": "Panel alignment verification",
      "notes": "Excellent performance, no safety issues"
    }
  ],
  "materialUsages": [
    {
      "materialId": "mat1-id",
      "quantity": 24,
      "unit": "panels",
      "unitCost": 450.00,
      "notes": "High-efficiency monocrystalline panels"
    },
    {
      "materialId": "mat2-id",
      "quantity": 12,
      "unit": "rails",
      "unitCost": 75.00,
      "notes": "Aluminum mounting rails"
    }
  ],
  "equipmentLogs": [
    {
      "equipmentId": "eq1-id",
      "hoursUsed": 4.0,
      "condition": "Excellent",
      "issues": "None",
      "maintenanceNotes": "Regular maintenance up to date"
    }
  ]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Enhanced daily report created successfully",
  "data": {
    "id": "new-report-id",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "approvalStatus": "Draft",
    "reportDate": "2025-06-29",
    "totalWorkHours": 8.5,
    "dailyProgressContribution": 12.5,
    "safetyScore": 10,
    "qualityScore": 9,
    "createdAt": "2025-06-29T18:00:00Z"
  },
  "errors": []
}
```

## 📊 Analytics & Reporting

### Get Daily Report Analytics

**GET** `/api/v1/daily-reports/projects/{projectId}/analytics`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get comprehensive analytics for daily reports within a project.

**Query Parameters**:
- `startDate` (DateTime): Analysis start date (default: 30 days ago)
- `endDate` (DateTime): Analysis end date (default: today)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Analytics retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "analysisPeriodStart": "2025-05-30",
    "analysisPeriodEnd": "2025-06-29",
    "totalReports": 30,
    "totalHoursLogged": 255.5,
    "averageHoursPerDay": 8.5,
    "averageSafetyScore": 9.2,
    "averageQualityScore": 8.8,
    "totalProgressContribution": 85.0,
    "averageProgressPerDay": 2.8,
    "daysAheadBehindSchedule": 2,
    "totalCriticalIssues": 3,
    "weatherDelayDays": 4,
    "topIssueCategories": ["Weather", "Material Delivery", "Permits"],
    "averageTeamSize": 4,
    "productivityIndex": 1.15,
    "topPerformers": [
      {
        "userId": "user1-id",
        "name": "John Technician",
        "reportsSubmitted": 15,
        "averageHoursPerDay": 8.2,
        "averageSafetyScore": 9.8,
        "averageQualityScore": 9.1,
        "productivityScore": 1.25
      }
    ],
    "weatherConditionDays": {
      "Sunny": 20,
      "PartlyCloudy": 6,
      "Rainy": 4
    },
    "weatherImpactScore": 0.13,
    "progressTrend": [
      {
        "date": "2025-06-25",
        "value": 2.5
      },
      {
        "date": "2025-06-26",
        "value": 3.2
      }
    ],
    "safetyTrend": [
      {
        "date": "2025-06-25",
        "value": 9.0
      },
      {
        "date": "2025-06-26",
        "value": 9.5
      }
    ]
  },
  "errors": []
}
```

### Get Weekly Progress Report

**GET** `/api/v1/daily-reports/projects/{projectId}/weekly-report`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Generate a comprehensive weekly progress report.

**Query Parameters**:
- `weekStartDate` (DateTime): Week start date (default: current week)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly progress report generated successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "weekStartDate": "2025-06-23",
    "weekEndDate": "2025-06-29",
    "reportsSubmitted": 7,
    "totalHours": 59.5,
    "progressMade": 18.5,
    "teamMemberCount": 4,
    "dailyProgress": [
      {
        "date": "2025-06-23",
        "hours": 8.5,
        "progress": 2.5,
        "teamSize": 4,
        "weatherCondition": "Sunny",
        "hasIssues": false,
        "safetyScore": 10.0,
        "qualityScore": 9.0
      }
    ],
    "keyAccomplishments": [
      "Completed installation of 48 solar panels",
      "Finished electrical connections for Phase 1",
      "Passed city safety inspection"
    ],
    "criticalIssues": [
      "Weather delay on Thursday due to thunderstorms"
    ],
    "upcomingMilestones": [
      "Inverter installation scheduled for next week",
      "Final inspection planned for end of month"
    ],
    "averageSafetyScore": 9.4,
    "averageQualityScore": 8.9,
    "isOnSchedule": true,
    "productivityIndex": 1.12
  },
  "errors": []
}
```

## 🔄 Bulk Operations

### Bulk Approve Daily Reports

**POST** `/api/v1/daily-reports/bulk-approve`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Approve multiple daily reports in a single operation.

**Request Body**:
```json
{
  "reportIds": [
    "report1-id",
    "report2-id",
    "report3-id"
  ],
  "comments": "Bulk approval for completed installation reports"
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
    "results": [
      {
        "itemId": "report1-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "itemId": "report2-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "itemId": "report3-id",
        "success": true,
        "details": "Approved successfully"
      }
    ],
    "summary": "All 3 reports approved successfully"
  },
  "errors": []
}
```

### Bulk Reject Daily Reports

**POST** `/api/v1/daily-reports/bulk-reject`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Reject multiple daily reports with reason.

**Request Body**:
```json
{
  "reportIds": [
    "report1-id",
    "report2-id"
  ],
  "rejectionReason": "Insufficient detail in work summary. Please provide more specific information about tasks completed and materials used."
}
```

## 🎯 AI-Powered Insights

### Get Daily Report Insights

**GET** `/api/v1/daily-reports/projects/{projectId}/insights`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get AI-generated insights and recommendations for daily reports.

**Query Parameters**:
- `reportId` (Guid): Specific report ID for targeted insights (optional)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Insights generated successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "generatedAt": "2025-06-29T18:00:00Z",
    "performanceInsights": [
      "Team productivity is 15% above industry average",
      "Safety scores consistently high with 9.2 average",
      "Weather-related delays are within expected range"
    ],
    "productivityRecommendations": [
      "Consider adding one additional team member to maintain current pace",
      "Schedule electrical work during optimal morning hours",
      "Implement pre-positioning of materials to reduce setup time"
    ],
    "riskLevel": "Low",
    "identifiedRisks": [
      "Potential weather delays during rainy season",
      "Material delivery schedule may impact timeline"
    ],
    "riskMitigationSuggestions": [
      "Maintain 2-day buffer for weather contingencies",
      "Establish backup material suppliers",
      "Consider covered work areas for sensitive operations"
    ],
    "isOnTrack": true,
    "progressVelocity": 2.8,
    "estimatedDaysToCompletion": 12,
    "safetyRecommendations": [
      "Continue current safety protocols",
      "Consider additional fall protection for roof work",
      "Regular safety briefings are effective"
    ],
    "qualityImprovements": [
      "Panel alignment procedures are excellent",
      "Consider implementing quality checkpoints",
      "Documentation quality could be enhanced"
    ],
    "trends": [
      {
        "category": "Safety",
        "trend": "Improving",
        "description": "Safety scores trending upward over past 2 weeks",
        "changePercent": 8.5,
        "recommendation": "Maintain current safety protocols"
      },
      {
        "category": "Productivity",
        "trend": "Stable",
        "description": "Consistent productivity levels maintained",
        "changePercent": 2.1,
        "recommendation": "Look for optimization opportunities"
      }
    ]
  },
  "errors": []
}
```

## ✅ Data Validation

### Validate Daily Report

**POST** `/api/v1/daily-reports/validate`

Validate daily report data before submission with AI-powered suggestions.

**Request Body**: Same as enhanced create daily report request

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Validation completed",
  "data": {
    "isValid": true,
    "errors": [],
    "warnings": [
      "Weather impact not specified - consider adding impact assessment"
    ],
    "suggestions": [
      "Add more detail to work accomplished section",
      "Consider including material cost information",
      "Safety score explanation would be helpful"
    ],
    "ruleResults": [
      {
        "ruleName": "Required Fields",
        "severity": "Error",
        "passed": true,
        "message": "All required fields are present"
      },
      {
        "ruleName": "Safety Score Range",
        "severity": "Warning", 
        "passed": true,
        "message": "Safety score is within acceptable range",
        "suggestion": "Consider documenting why safety score is perfect"
      }
    ],
    "autoCorrections": [
      {
        "field": "weatherCondition",
        "currentValue": "sunny",
        "suggestedValue": "Sunny",
        "reason": "Standardize weather condition format",
        "confidence": 0.95
      }
    ]
  },
  "errors": []
}
```

## 📝 Templates & Configuration

### Get Daily Report Templates

**GET** `/api/v1/daily-reports/projects/{projectId}/templates`

Get project-specific daily report templates for consistent data collection.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Templates retrieved successfully",
  "data": [
    {
      "id": "template1-id",
      "name": "Standard Solar Installation",
      "description": "Standard template for solar panel installation projects",
      "projectId": "456e7890-e89b-12d3-a456-426614174001",
      "projectType": "Solar Installation",
      "fields": [
        {
          "name": "workSummary",
          "label": "Work Summary",
          "type": "Text",
          "isRequired": true,
          "helpText": "Provide a comprehensive summary of work completed",
          "displayOrder": 1
        },
        {
          "name": "safetyScore",
          "label": "Safety Score",
          "type": "Number",
          "isRequired": true,
          "constraints": {
            "min": 1,
            "max": 10
          },
          "defaultValue": "10",
          "helpText": "Rate safety performance from 1-10",
          "displayOrder": 2
        }
      ],
      "requiredFields": ["workSummary", "safetyScore", "totalWorkHours"],
      "defaultValues": {
        "safetyScore": 10,
        "qualityScore": 10
      },
      "validationRules": [
        {
          "name": "Safety Score Minimum",
          "type": "Range",
          "field": "safetyScore",
          "parameters": {
            "min": 7
          },
          "errorMessage": "Safety score must be at least 7 for project approval",
          "severity": "Warning"
        }
      ],
      "isDefault": true,
      "isActive": true,
      "createdAt": "2025-06-01T10:00:00Z"
    }
  ],
  "errors": []
}
```

## 🔄 Workflow Management

### Get Pending Approvals

**GET** `/api/v1/daily-reports/pending-approval`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get daily reports pending approval.

**Query Parameters**:
- `projectId` (Guid): Filter by project (optional)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Pending approvals retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "report1-id",
        "projectId": "project1-id",
        "projectName": "Solar Installation Alpha",
        "reportDate": "2025-06-28",
        "reporterName": "John Technician",
        "submittedAt": "2025-06-28T17:00:00Z",
        "status": "Submitted",
        "priority": "Normal",
        "hasIssues": false,
        "totalWorkHours": 8.5,
        "safetyScore": 10,
        "qualityScore": 9
      }
    ],
    "pagination": {
      "totalCount": 5,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  },
  "errors": []
}
```

### Get Approval History

**GET** `/api/v1/daily-reports/{reportId}/approval-history`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get complete approval history for a daily report.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Approval history retrieved successfully",
  "data": [
    {
      "id": "history1-id",
      "action": "Submitted",
      "actorId": "user1-id",
      "actorName": "John Technician",
      "timestamp": "2025-06-28T17:00:00Z",
      "comments": "Initial submission"
    },
    {
      "id": "history2-id",
      "action": "Approved",
      "actorId": "manager1-id",
      "actorName": "Sarah Manager",
      "timestamp": "2025-06-28T18:30:00Z",
      "comments": "Excellent work quality and safety compliance"
    }
  ],
  "errors": []
}
```

## 📤 Enhanced Export

### Export Enhanced Daily Reports

**POST** `/api/v1/daily-reports/export-enhanced`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Export daily reports with comprehensive data and analytics.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "startDate": "2025-06-01",
  "endDate": "2025-06-29",
  "format": "excel",
  "includeAttachments": true,
  "includeAnalytics": true,
  "fieldsToInclude": [
    "workSummary",
    "safetyScore",
    "qualityScore",
    "totalWorkHours",
    "weatherCondition",
    "progressContribution"
  ]
}
```

**Success Response (200)**:
- File download with comprehensive daily report data

## ❌ Enhanced Daily Report Error Codes

### Core Operations
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR001** | Report not found | Verify report ID exists and user has access |
| **DR002** | Insufficient permissions | Check user role or report ownership |
| **DR003** | Invalid report data | Check request body for required fields and validation rules |
| **DR004** | Cannot update after 24h | Only Admins/Managers can update older reports |
| **DR005** | Project not found or access denied | Verify project ID exists and user has project access |
| **DR006** | File upload failed | Check file format, size, and upload permissions |

### Enhanced Validation & Business Logic
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR007** | Duplicate report for date | Only one report per user per day allowed |
| **DR008** | Invalid weather data | Check weather condition values and temperature ranges |
| **DR009** | Safety score below threshold | Safety score must be >= 7 for project approval |
| **DR010** | Invalid progress contribution | Progress contribution must be 0-100% |
| **DR011** | Work hours exceed daily limit | Total work hours cannot exceed 24 hours |
| **DR012** | Personnel count mismatch | Personnel on site must match personnel logs |
| **DR013** | Material usage validation failed | Check material IDs and quantity values |
| **DR014** | Equipment log validation failed | Verify equipment IDs and usage hours |

### Project & Access Control
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR015** | Project inactive or suspended | Report creation disabled for inactive projects |
| **DR016** | User not assigned to project | User must be assigned to project to create reports |
| **DR017** | Project template not found | Verify project has valid daily report template |
| **DR018** | Template validation failed | Check report data against project template requirements |

### Workflow & Approval
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR019** | Invalid approval status transition | Check valid status transitions (Draft→Submitted→Approved/Rejected) |
| **DR020** | Report already approved | Cannot modify approved reports |
| **DR021** | Approval permission denied | User lacks approval authority for this project |
| **DR022** | Bulk operation partially failed | Check individual operation results for details |
| **DR023** | Approval history not found | Report may not have been submitted for approval |

### Analytics & Reporting
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR024** | Analytics data unavailable | Insufficient data for requested analysis period |
| **DR025** | Export format not supported | Use supported formats: csv, excel, pdf, json |
| **DR026** | Export size limit exceeded | Reduce date range or field selection |
| **DR027** | Insights generation failed | AI service temporarily unavailable, try again later |

### Data Integrity & Validation
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR028** | Date validation failed | Report date cannot be future date or too far in past |
| **DR029** | Attachment size limit exceeded | Reduce file size or use compression |
| **DR030** | Invalid attachment type | Only images and documents allowed |
| **DR031** | Data consistency error | Report data conflicts with existing project data |
| **DR032** | Template field validation failed | Check required fields and field constraints |

### System & Performance
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR033** | Rate limit exceeded | Too many requests, wait before retrying |
| **DR034** | Cache refresh required | Clear cache or wait for automatic refresh |
| **DR035** | Service temporarily unavailable | System maintenance in progress, try again later |
| **DR036** | Database connection failed | Contact system administrator |

---
*Last Updated: June 29, 2025*
