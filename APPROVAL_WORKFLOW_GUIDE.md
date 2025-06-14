# Work Request Approval Workflow System

## Overview

The Work Request Approval Workflow System provides a comprehensive solution for managing the approval process of work requests with a multi-level approval chain, automated notifications, and detailed tracking capabilities.

## 🎯 Key Features Implemented

### ✅ Multi-Level Approval Chain
- **User → Manager → Admin** approval flow
- Configurable approval requirements based on cost thresholds
- Auto-approval for low-cost requests under threshold
- Role-based approval permissions

### ✅ Enhanced Status Management
- **New Status Values:**
  - `Open` - Initial state
  - `PendingManagerApproval` - Waiting for manager approval
  - `PendingAdminApproval` - Waiting for admin approval  
  - `Approved` - Fully approved and ready to proceed
  - `Rejected` - Rejected with reason
  - `InProgress` - Work in progress
  - `Completed` - Work completed
  - `Cancelled` - Cancelled
  - `OnHold` - Temporarily on hold

### ✅ Comprehensive Approval History
- Complete audit trail of all approval actions
- Approval comments and rejection reasons
- Timestamps and approver information
- Escalation tracking

### ✅ Email Notification System
- Automated notifications for:
  - Work request submission
  - Approval required alerts
  - Approval/rejection notifications
  - Escalation notifications
  - Overdue approval reminders
- Configurable notification templates
- Retry mechanism for failed emails

### ✅ Advanced Workflow Features
- **Bulk Approval:** Process multiple requests simultaneously
- **Escalation:** Escalate requests to higher authority
- **Auto-Approval:** Automatic approval based on cost thresholds
- **Approval Statistics:** Dashboard metrics and reporting
- **Overdue Tracking:** Identify and manage overdue approvals

## 🏗️ Architecture

### Models Enhanced

#### WorkRequest Model
```csharp
// New approval workflow fields
public Guid? ManagerApproverId { get; set; }
public Guid? AdminApproverId { get; set; }
public DateTime? ManagerApprovalDate { get; set; }
public DateTime? AdminApprovalDate { get; set; }
public DateTime? SubmittedForApprovalDate { get; set; }
public string? ManagerComments { get; set; }
public string? AdminComments { get; set; }
public string? RejectionReason { get; set; }
public bool RequiresManagerApproval { get; set; }
public bool RequiresAdminApproval { get; set; }
public decimal? AutoApprovalThreshold { get; set; }
public bool IsAutoApproved { get; set; }
```

#### New Models Added

**WorkRequestApproval** - Tracks approval history
```csharp
public class WorkRequestApproval
{
    public Guid ApprovalId { get; set; }
    public Guid WorkRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public ApprovalAction Action { get; set; }
    public ApprovalLevel Level { get; set; }
    public WorkRequestStatus PreviousStatus { get; set; }
    public WorkRequestStatus NewStatus { get; set; }
    public string? Comments { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsActive { get; set; }
    // Escalation tracking
    public Guid? EscalatedFromId { get; set; }
    public DateTime? EscalationDate { get; set; }
    public string? EscalationReason { get; set; }
}
```

**WorkRequestNotification** - Manages email notifications
```csharp
public class WorkRequestNotification
{
    public Guid NotificationId { get; set; }
    public Guid WorkRequestId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid? SenderId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public string? EmailTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### Services Added

#### IWorkRequestApprovalService
- `SubmitForApprovalAsync()` - Submit work request for approval
- `ProcessApprovalAsync()` - Process approval/rejection
- `BulkApprovalAsync()` - Process multiple approvals
- `GetApprovalStatusAsync()` - Get workflow status
- `GetPendingApprovalsAsync()` - Get pending approvals for user
- `GetApprovalStatisticsAsync()` - Get approval metrics
- `EscalateApprovalAsync()` - Escalate to higher authority
- `SendApprovalRemindersAsync()` - Send overdue reminders

#### INotificationService
- `SendWorkRequestNotificationAsync()` - Send single notification
- `SendBulkNotificationsAsync()` - Send multiple notifications
- `GetUserNotificationsAsync()` - Get user's notifications
- `MarkNotificationAsReadAsync()` - Mark notification as read

#### IEmailService
- `SendEmailAsync()` - Send single email
- `SendBulkEmailAsync()` - Send multiple emails

## 🚀 API Endpoints

### Approval Workflow Endpoints

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `POST` | `/work-requests/{id}/submit-approval` | Submit for approval | User |
| `POST` | `/work-requests/{id}/process-approval` | Approve/Reject | Manager/Admin |
| `GET` | `/work-requests/{id}/approval-status` | Get approval status | Any |
| `GET` | `/work-requests/{id}/approval-history` | Get approval history | Any |
| `POST` | `/work-requests/{id}/escalate` | Escalate approval | Manager/Admin |
| `GET` | `/work-requests/pending-approvals` | Get pending approvals | Manager/Admin |
| `GET` | `/work-requests/approval-statistics` | Get approval stats | Manager/Admin |
| `POST` | `/work-requests/bulk-approval` | Bulk approve/reject | Manager/Admin |
| `POST` | `/work-requests/send-approval-reminders` | Send reminders | Admin |

### Request/Response Examples

#### Submit for Approval
```bash
POST /api/v1/work-requests/{id}/submit-approval
Content-Type: application/json

{
    "preferredManagerId": "guid-here",
    "requiresAdminApproval": true,
    "comments": "Urgent installation required for Q3 deadline"
}
```

#### Process Approval
```bash
POST /api/v1/work-requests/{id}/process-approval
Content-Type: application/json

{
    "action": "Approve", // or "Reject", "Escalate"
    "comments": "Approved for Q3 implementation",
    "rejectionReason": "", // Required if action is "Reject"
    "escalateToUserId": "guid-here", // Required if action is "Escalate"
    "escalationReason": "Requires higher authority review"
}
```

#### Bulk Approval
```bash
POST /api/v1/work-requests/bulk-approval
Content-Type: application/json

{
    "workRequestIds": ["guid1", "guid2", "guid3"],
    "action": "Approve",
    "comments": "Bulk approval for Q3 installations"
}
```

## ⚙️ Configuration

### appsettings.json
```json
{
    "WorkRequest": {
        "AutoApprovalThreshold": 1000,
        "ManagerApprovalThreshold": 500,
        "AdminApprovalThreshold": 5000,
        "ApprovalReminderDays": 3,
        "OverdueApprovalDays": 7,
        "MaxRetryAttempts": 3
    },
    "Email": {
        "FromAddress": "noreply@solarprojects.com",
        "FromName": "Solar Projects System",
        "SmtpServer": "localhost",
        "SmtpPort": 587,
        "UseSsl": false,
        "Username": "",
        "Password": ""
    }
}
```

### Approval Logic

#### Approval Requirements
- **Manager Approval Required if:**
  - Estimated cost > $500 (configurable)
  - Priority is High or Critical
  - Explicitly requested

- **Admin Approval Required if:**
  - Estimated cost > $5,000 (configurable)
  - Priority is Critical
  - Explicitly requested
  - Manager escalates the request

#### Auto-Approval
- Requests under $1,000 (configurable) are auto-approved
- Only applies if no admin approval is required
- Creates approval record with "AutoApproved" action

## 🔄 Workflow Process

### Standard Approval Flow
1. **User Creates** work request (Status: `Open`)
2. **User Submits** for approval (Status: `PendingManagerApproval` or `PendingAdminApproval`)
3. **Manager Reviews** and approves/rejects
   - If approved and admin required: Status → `PendingAdminApproval`
   - If approved and no admin required: Status → `Approved`
   - If rejected: Status → `Rejected`
4. **Admin Reviews** (if required) and approves/rejects
   - If approved: Status → `Approved`
   - If rejected: Status → `Rejected`

### Escalation Flow
- Manager/Admin can escalate to higher authority
- Creates escalation record linked to original approval
- Transfers approval responsibility to escalated user
- Maintains audit trail of escalation reason

### Notification Flow
- **Submission:** Notifies assigned approver
- **Approval:** Notifies requestor and next approver (if applicable)
- **Rejection:** Notifies requestor with reason
- **Escalation:** Notifies escalated approver
- **Reminders:** Sent for overdue approvals (3+ days)

## 📊 Monitoring & Analytics

### Approval Statistics
- Total pending approvals by level
- Average approval time
- Overdue approval count
- Daily/weekly/monthly approval counts
- Urgent pending approvals list

### Approval History
- Complete audit trail per work request
- Approval timeline with timestamps
- Comments and rejection reasons
- Escalation tracking

## 🧪 Testing

### Test Script
Run the comprehensive test script:
```bash
./test-approval-workflow.sh
```

The script tests:
- ✅ Work request creation
- ✅ Approval submission
- ✅ Manager approval process
- ✅ Admin approval process
- ✅ Rejection workflow
- ✅ Approval history tracking
- ✅ Bulk approval functionality
- ✅ Statistics and reporting

## 🔒 Security & Permissions

### Role-Based Access
- **Users:** Can create and submit their own work requests
- **Managers:** Can approve requests assigned to them, view pending approvals
- **Admins:** Can approve any request, access all statistics, send reminders

### Authorization Rules
- Users can only submit their own requests for approval
- Managers can only approve requests in `PendingManagerApproval` status
- Admins can approve requests in any pending status
- Escalation requires manager/admin role

## 🚧 Future Enhancements

### Planned Features
1. **Email Integration:** Real email service (SendGrid, AWS SES)
2. **Automated Reminders:** Background job for overdue notifications
3. **Mobile Notifications:** Push notifications for mobile apps
4. **Approval Templates:** Predefined workflows for request types
5. **Analytics Dashboard:** Visual approval metrics and trends
6. **Integration APIs:** Webhook support for external systems
7. **Advanced Rules Engine:** Complex approval routing logic
8. **Document Attachments:** File uploads for approval decisions

### Performance Optimizations
- Implement caching for approval statistics
- Background job for notification processing
- Database indexing for approval queries
- Pagination for large approval histories

## 📚 Related Documentation

- [API Reference](./API_REFERENCE.md)
- [Database Schema](./database.md)
- [Authentication Guide](./authentication.md)
- [Deployment Guide](./deployment.md)

---

**Status:** ✅ **COMPLETE** - Enhanced Work Request Approval Workflow Successfully Implemented

The approval workflow system now provides a robust, enterprise-grade solution for managing work request approvals with comprehensive tracking, notifications, and reporting capabilities.
