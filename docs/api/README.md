# 🔌 Solar Project Management API Documentation

Welcome to the comprehensive API documentation for the Solar Project Management System. This documentation is organized into focused sections for easy navigation and maintenance.

## 📖 Documentation Structure

### 🏛️ Architecture
- **[Architecture Overview](../architecture/README.md)** - System architecture and design
- **[Implementation Guide](../architecture/IMPLEMENTATION_GUIDE.md)** - Development guidelines
- **[Master Plan Architecture](../architecture/MASTER_PLAN_ARCHITECTURE.md)** - Detailed system blueprint

### 🚀 Getting Started
- **[Quick Start Guide](./01_QUICK_START.md)** - Get up and running quickly
- **[Authentication Guide](./02_AUTHENTICATION.md)** - User registration, login, and security
- **[Role Access Matrix](./ROLE_ACCESS_MATRIX.md)** - Permission levels and access control

### 📋 Core Modules
- **[Project Management](./03_PROJECTS.md)** - Complete project CRUD operations
- **[Master Plan Management](./04_MASTER_PLANS.md)** - Project planning and workflow
- **[Task Management](./05_TASKS.md)** - Task assignment and tracking
- **[Daily Reports](./06_DAILY_REPORTS.md)** - Progress reporting and time tracking
- **[Work Requests](./07_WORK_REQUESTS.md)** - Change orders and approvals

### 🔧 Additional Features
- **[Calendar Events](./08_CALENDAR.md)** - Scheduling and event management
- **[Weekly Planning](./09_WEEKLY_PLANNING.md)** - Weekly reports and planning
- **[Image Upload](./10_IMAGE_UPLOAD.md)** - File and image handling

### 🛠️ Technical Reference
- **[Error Handling](./11_ERROR_HANDLING.md)** - Error codes and troubleshooting
- **[Rate Limiting](./12_RATE_LIMITING.md)** - API limits and throttling
- **[Health Monitoring](./13_HEALTH.md)** - System status and diagnostics

### 📱 Integration Guides
- **[Flutter Integration](./FLUTTER_API_GUIDE.md)** - Mobile app development
- **[Code Examples](./14_CODE_EXAMPLES.md)** - Language-specific examples

## 🚀 Quick Start

### 1. 🏃‍♂️ Deploy the API
```bash
# Clone and start with Docker
git clone <repository>
cd dotnet-rest-api
docker-compose up -d
```

### 2. 🔑 Get Authentication Token
```bash
curl -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test_admin@example.com","password":"Admin123!"}'
```

### 3. 📋 Test API Access
```bash
curl -X GET http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🎯 API Base Information

| Environment | Base URL | Status |
|-------------|----------|---------|
| **Development** | `http://localhost:5002/api/v1` | ✅ Active |
| **Production** | `https://your-domain.com/api/v1` | 🔧 Configure |

## 🔐 Authentication Overview

All API endpoints (except health checks) require authentication via JWT tokens:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### 👥 User Roles & Access Levels

| Role | Project Access | Master Plans | Tasks | Reports | Admin Functions |
|------|---------------|--------------|-------|---------|-----------------|
| **Admin** | Full CRUD + Delete | Full CRUD | Full CRUD | Full CRUD | System Config |
| **Manager** | Full CRUD | Full CRUD | Full CRUD | Full CRUD | Limited Admin |
| **User** | Read Only | Read Only | Assigned Tasks | Own Reports | None |

## 📊 Current System Status

- **🗄️ Database**: PostgreSQL with 97+ solar projects
- **🐳 Deployment**: Docker containers (API + Database)
- **⚡ Performance**: Rate limiting configured (100 req/min)
- **🔒 Security**: JWT authentication with role-based access
- **📈 Features**: Complete CRUD, approval workflows, file uploads

## 📞 Support & Contact

For technical support or questions about this API:
- 📧 **Email**: support@solarproject.com
- 📚 **Documentation**: This guide covers all endpoints
- 🐛 **Issues**: Report bugs via your preferred method
- 📖 **Updates**: Check individual module docs for latest changes

---
*Last Updated: June 15, 2025*
