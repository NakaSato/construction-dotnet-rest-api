# Solar Projects API Documentation

This folder contains comprehensive documentation for the Solar Projects API, a .NET 9.0 REST API for solar project management.

## 📁 Documentation Structure

### `/api/` - API Reference Documentation
Complete API endpoint documentation including:
- **01_AUTHENTICATION.md** - Authentication and authorization
- **02_TASKS.md** - Task management endpoints
- **03_PROJECTS.md** - Project management endpoints  
- **04_MASTER_PLANS.md** - Master plan management
- **05_CALENDAR_EVENTS.md** - Calendar and event management
- **06_DAILY_REPORTS.md** - Daily reporting system
- **07_WORK_REQUESTS.md** - Work request management
- **08_REAL_TIME_NOTIFICATIONS.md** - Real-time notifications via SignalR
- **09_DASHBOARD.md** - Dashboard and analytics endpoints
- **10_DOCUMENT_RESOURCES.md** - Document and resource management
- **11_USERS.md** - User management system
- **12_RATE_LIMITING.md** - API rate limiting and caching
- **API_REFERENCE.md** - Complete API reference
- **API_ACCESS_GUIDE.md** - Access patterns and usage guide
- **ROLE_ACCESS_MATRIX.md** - Permission matrix by user role

### `/guides/` - User and Developer Guides
- **QUICK_START_REALTIME.md** - Quick start guide for real-time features
- **USER_GUIDE.md** - End-user guide for API usage

### `/implementation/` - Implementation Documentation
- **FINAL_IMPLEMENTATION_STATUS.md** - Current implementation status
- **SIGNALR_IMPLEMENTATION_SUMMARY.md** - SignalR implementation details
- **LOGOUT_IMPLEMENTATION_SUMMARY.md** - Logout system implementation
- **DOCUMENTATION_UPDATE_SUMMARY.md** - Documentation change history

### `/testing/` - Testing Documentation
- **API_TESTING_RESULTS.md** - API testing results and validation
- **Project Creation Tests** - Role-based project creation validation scripts

## 🚀 Quick Start

1. **API Overview**: Start with `/api/API_REFERENCE.md`
2. **Authentication**: Read `/api/01_AUTHENTICATION.md`
3. **Project Management**: See `/api/03_PROJECTS.md`
4. **Real-time Features**: Check `/guides/QUICK_START_REALTIME.md`

## 🔐 Authentication & Permissions

The API uses JWT-based authentication with role-based access control:

- **Admin**: Full CRUD access to all resources
- **Manager**: Full CRUD access to projects, tasks, and reports
- **User**: Read access and limited write operations
- **Viewer**: Read-only access

See `/api/ROLE_ACCESS_MATRIX.md` for detailed permissions.

## 🛠️ Development

- **Framework**: .NET 9.0
- **Database**: Entity Framework Core with PostgreSQL
- **Real-time**: SignalR for notifications
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture (Controllers → Services → Data)

## 📊 Testing

Role-based testing scripts are available in `/scripts/`:
- `test-user-project-creation.sh` - Validates User role restrictions
- `test-admin-manager-final.sh` - Validates Admin/Manager permissions

## 🔗 Related Resources

- **API Endpoint**: `http://localhost:5001/api/v1/`
- **Swagger UI**: `http://localhost:5001/swagger`
- **Health Check**: `http://localhost:5001/health`

---

For questions or contributions, please refer to the specific documentation files or check the implementation status for current development progress.
