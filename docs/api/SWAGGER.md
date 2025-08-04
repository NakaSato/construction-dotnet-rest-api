### **Authentication & Authorization**
```
POST /api/v1/Auth/login              - User authentication
POST /api/v1/Auth/register           - User registration  
POST /api/v1/Auth/refresh            - Refresh JWT token
POST /api/v1/Auth/logout             - User logout
```

### **User Management**
```
GET    /api/v1/users                 - Get paginated users list
GET    /api/v1/users/{id}            - Get user by ID
GET    /api/v1/users/username/{username} - Get user by username  
POST   /api/v1/users                 - Create new user
PUT    /api/v1/users/{id}            - Update user (full)
PATCH  /api/v1/users/{id}            - Update user (partial)
DELETE /api/v1/users/{id}            - Delete user
PATCH  /api/v1/users/{id}/activate   - Activate user
PATCH  /api/v1/users/{id}/deactivate - Deactivate user
GET    /api/v1/users/advanced        - Advanced user queries with filtering
GET    /api/v1/users/rich            - Rich paginated user list with HATEOAS
```

### **Dashboard & Analytics**
```
GET  /api/v1/dashboard/overview              - Dashboard overview data
GET  /api/v1/dashboard/project-progress      - Real-time project progress
POST /api/v1/dashboard/broadcast-progress/{projectId} - Broadcast progress updates
GET  /api/v1/dashboard/live-activity         - Live user activity feed
POST /api/v1/dashboard/system-announcement  - Send system announcements
GET  /api/v1/dashboard/statistics            - Dashboard analytics & statistics
```

### **Master Plans & Project Management**
```
GET    /api/v1/master-plans                  - Get all master plans
GET    /api/v1/master-plans/{id}             - Get master plan by ID
POST   /api/v1/master-plans                  - Create master plan
PUT    /api/v1/master-plans/{id}             - Update master plan
DELETE /api/v1/master-plans/{id}             - Delete master plan
GET    /api/v1/master-plans/{id}/progress    - Get master plan progress
GET    /api/v1/master-plans/{id}/milestones/upcoming - Get upcoming milestones
POST   /api/v1/master-plans/{id}/progress-reports - Create progress report
```

### **Health & System Status**
```
GET /health                 - Basic health check
GET /health/detailed        - Detailed health with database status
```

### **Debug & Development** *(Debug endpoints)*
```
GET  /api/debug/config           - System configuration info
GET  /api/debug/cache-stats      - Cache statistics
GET  /api/debug/database         - Database connectivity status
POST /api/debug/migrate-database - Execute database migrations
GET  /api/debug/database-info    - Database information
```

### **🚧 Additional Controllers (Currently Disabled)**

The codebase also contains several **disabled controllers** (`.disabled` extension) that provide additional functionality once enabled:

#### **Work Breakdown Structure (WBS)**
```
GET    /api/v1/wbs                          - Get all WBS tasks
GET    /api/v1/wbs/{wbsId}                  - Get specific WBS task
GET    /api/v1/wbs/hierarchy/{projectId}    - Get task hierarchy
POST   /api/v1/wbs                          - Create WBS task
PUT    /api/v1/wbs/{wbsId}                  - Update WBS task
DELETE /api/v1/wbs/{wbsId}                  - Delete WBS task
PATCH  /api/v1/wbs/{wbsId}/status           - Update task status
POST   /api/v1/wbs/{wbsId}/evidence         - Add task evidence
GET    /api/v1/wbs/{wbsId}/progress         - Get progress summary
GET    /api/v1/wbs/{wbsId}/critical-path    - Get critical path
```

#### **Daily Reports**
```
GET    /api/v1/daily-reports                - Get daily reports
GET    /api/v1/daily-reports/{id}           - Get daily report by ID
POST   /api/v1/daily-reports                - Create daily report
PUT    /api/v1/daily-reports/{id}           - Update daily report
DELETE /api/v1/daily-reports/{id}           - Delete daily report
GET    /api/v1/daily-reports/project/{projectId} - Get project daily reports
POST   /api/v1/daily-reports/{id}/approve   - Approve daily report
GET    /api/v1/daily-reports/pending-approval - Get reports pending approval
```

#### **Work Requests**
```
GET    /api/v1/work-requests                - Get work requests
GET    /api/v1/work-requests/{id}           - Get work request by ID
POST   /api/v1/work-requests                - Create work request
PUT    /api/v1/work-requests/{id}           - Update work request
DELETE /api/v1/work-requests/{id}           - Delete work request
GET    /api/v1/work-requests/project/{projectId} - Get project work requests
POST   /api/v1/work-requests/{id}/approve   - Approve work request
GET    /api/v1/work-requests/analytics      - Work request analytics
```

#### **Projects**
```
GET    /api/v1/projects                     - Get projects
GET    /api/v1/projects/{id}                - Get project by ID
POST   /api/v1/projects                     - Create project
PUT    /api/v1/projects/{id}                - Update project
DELETE /api/v1/projects/{id}                - Delete project
GET    /api/v1/projects/mobile              - Mobile-optimized projects
```

### **🔐 Security & Authorization**

- **JWT Bearer Authentication** required for most endpoints
- **Role-based Authorization**: Admin, Manager, ProjectManager, User roles
- **Rate Limiting** and **Caching** implemented on various endpoints

### **📊 Features Available**

✅ **Currently Active**: Authentication, Users, Dashboard, Master Plans, Health/Debug  
🚧 **Available but Disabled**: WBS, Daily Reports, Work Requests, Projects, Weekly Reports

### **🔧 To Enable Disabled Controllers**

Simply rename the `.disabled` files to `.cs` to activate additional functionality:
- WbsController.cs.disabled → `WbsController.cs`
- DailyReportsController.cs.disabled → `DailyReportsController.cs`
- `WorkRequestsController.cs.disabled` → `WorkRequestsController.cs`
- ProjectsController.cs.disabled → `ProjectsController.cs`

This gives you a comprehensive Solar Project Management API with real-time capabilities, role-based access control, and extensive project tracking features!