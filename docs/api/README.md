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
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_admin",
    "password": "Admin123!"
  }'
```

### 3. Test API Access
```bash
# Use the token from step 2
curl -X GET "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Create Your First Project
```bash
curl -X POST "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "My First Solar Project",
    "address": "123 Solar Street, Bangkok, Thailand",
    "clientInfo": "Test Client Company",
    "startDate": "2025-08-01T00:00:00Z"
## API Information

| Environment | Base URL | Status |
|-------------|----------|---------|
| Development | `http://localhost:5001/api/v1` | Active |
| Docker | `http://localhost:8080/api/v1` | Active |
| Production | `https://your-domain.com/api/v1` | Configure |

## Authentication

All API endpoints require authentication via JWT tokens:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Test Account
- **Username**: `test_admin`
- **Password**: `Admin123!`
- **Role**: Admin (full access)

### User Roles

| Role | Project Access | Reports | Admin Functions |
|------|---------------|---------|-----------------|
| Admin | Full CRUD + Delete | Full CRUD | System Config |
| Manager | Full CRUD | Full CRUD | Limited Admin |
| User | Read Only | Own Reports | None |

## System Status

- **Database**: PostgreSQL/In-Memory (Docker)
- **Deployment**: Docker containers + Local development
- **Performance**: Rate limiting configured
- **Security**: JWT authentication with role-based access
- **Real-Time**: SignalR WebSocket connections for live updates

## 🚀 Quick Testing

### Complete API Test Script
```bash
#!/bin/bash

# 1. Test authentication
echo "🔐 Testing authentication..."
TOKEN=$(curl -s -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "test_admin", "password": "Admin123!"}' \
  | jq -r '.data.token')

echo "Token: ${TOKEN:0:50}..."

# 2. Test project creation
echo "📋 Testing project creation..."
PROJECT_RESPONSE=$(curl -s -X POST "http://localhost:5001/api/v1/projects" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectName": "API Test Project",
    "address": "123 Test Street, Bangkok, Thailand",
    "clientInfo": "Test Client",
    "startDate": "2025-08-01T00:00:00Z"
  }')

PROJECT_ID=$(echo $PROJECT_RESPONSE | jq -r '.data.projectId')
echo "Created project: $PROJECT_ID"

# 3. Test project retrieval
echo "🔍 Testing project retrieval..."
curl -s -X GET "http://localhost:5001/api/v1/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN" | jq '.data.projectName'

# 4. Test real-time features (if SignalR client available)
echo "⚡ Real-time features available at: ws://localhost:5001/notificationHub"

echo "✅ API test completed successfully!"
```

## 📚 Additional Resources

- **[User Management Guide](../USER_MANAGEMENT_GUIDE.md)** - Authentication & user creation
- **[Real-Time Features](./00_REAL_TIME_LIVE_UPDATES.md)** - SignalR implementation
- **[Curl Commands Reference](../CURL_COMMANDS.md)** - Quick API reference
- **[Database Schema](../schema.sql)** - Complete database structure

## Support

For technical support and questions:
- **Documentation**: Check individual module docs for detailed information
- **Issues**: Report bugs and feature requests via your preferred method
- **Updates**: API documentation is updated regularly

---
*Last Updated: July 2025 - Solar Projects API v1.0*
