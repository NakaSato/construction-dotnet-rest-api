# Solar Project Management API Documentation

**Status**: Operational | **Framework**: .NET 9.0 | **Database**: PostgreSQL

REST API documentation for the Solar Project Management System.

## Documentation Structure

### Architecture
- **[Architecture Overview](../architecture/README.md)** - System architecture and design
- **[Implementation Guide](../architecture/IMPLEMENTATION_GUIDE.md)** - Development guidelines

### Getting Started
- **[Quick Start Guide](./01_QUICK_START.md)** - Setup and basic usage
- **[Authentication Guide](./02_AUTHENTICATION.md)** - User registration and login

### Core Modules
- **[Project Management](./03_PROJECTS.md)** - Project CRUD operations
- **[Master Plan Management](./04_MASTER_PLANS.md)** - Project planning
- **[Task Management](./05_TASKS.md)** - Task assignment and tracking
- **[Daily Reports](./06_DAILY_REPORTS.md)** - Progress reporting
- **[Work Requests](./07_WORK_REQUESTS.md)** - Change orders

### Additional Features
- **[🌐 Real-Time Live Updates](./00_REAL_TIME_LIVE_UPDATES.md)** - Comprehensive real-time data synchronization for all endpoints
- **[Real-Time Notifications](./08_REAL_TIME_NOTIFICATIONS.md)** - Live notifications system
- **[Calendar Events](./09_CALENDAR.md)** - Scheduling with live updates
- **[Weekly Planning](./10_WEEKLY_PLANNING.md)** - Weekly reports
- **[Image Upload](./11_IMAGE_UPLOAD.md)** - File handling

### Technical Reference
- **[Error Handling](./11_ERROR_HANDLING.md)** - Error codes
- **[Rate Limiting](./12_RATE_LIMITING.md)** - API limits
- **[Health Monitoring](./14_HEALTH.md)** - System status

### Integration
- **[Code Examples](./15_CODE_EXAMPLES.md)** - Implementation examples

## Quick Start

### 1. Deploy the API
```bash
git clone <repository>
cd dotnet-rest-api
docker-compose up -d
```

### 2. Get Authentication Token
```bash
curl -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test_admin@example.com","password":"Admin123!"}'
```

### 3. Test API Access
```bash
curl -X GET http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## API Information

| Environment | Base URL | Status |
|-------------|----------|---------|
| Development | `http://localhost:5002/api/v1` | Active |
| Production | `https://your-domain.com/api/v1` | Configure |

## Authentication

All API endpoints require authentication via JWT tokens:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### User Roles

| Role | Project Access | Reports | Admin Functions |
|------|---------------|---------|-----------------|
| Admin | Full CRUD + Delete | Full CRUD | System Config |
| Manager | Full CRUD | Full CRUD | Limited Admin |
| User | Read Only | Own Reports | None |

## System Status

- Database: PostgreSQL
- Deployment: Docker containers
- Performance: Rate limiting configured
- Security: JWT authentication with role-based access

## Support

For technical support:
- Email: support@solarproject.com

---
*Last Updated: July 4, 2025*
- 🐛 **Issues**: Report bugs via your preferred method
- 📖 **Updates**: Check individual module docs for latest changes

---
*Last Updated: June 15, 2025*
