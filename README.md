# Solar Projects API

.NET 9.0 REST API for solar project management.

## Quick Start

```bash
# Run locally
dotnet run --urls "http://localhost:5001"

# API Documentation
http://localhost:5001
```

## Authentication

### Default Admin Account
- **Username**: `admin@example.com`
- **Password**: `Admin123!`

### Login
```bash
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}'
```

### Register New User
```bash
curl -X POST http://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"user@example.com","password":"SecurePass123!","fullName":"New User","roleId":3}'
```

## API Endpoints

### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/register` - User registration

### Core Resources
- `GET /api/v1/projects` - List projects
- `POST /api/v1/projects` - Create project
- `GET /api/v1/tasks` - List tasks
- `POST /api/v1/daily-reports` - Submit daily report
- `GET /api/v1/users` - List users

### Notifications
- `GET /api/v1/notifications` - List user notifications
- `PATCH /api/v1/notifications/{id}/read` - Mark notification as read
- `GET /api/v1/notifications/preferences` - Get notification preferences
- `PUT /api/v1/notifications/preferences` - Update notification preferences
- `/notificationHub` - SignalR hub for real-time notifications

See [Notifications API Documentation](./docs/api/NOTIFICATIONS_API.md) for comprehensive documentation including Flutter integration, analytics, and best practices.

### Mobile Flutter App Support
- `GET /api/v1/projects/mobile` - Get lightweight project list for mobile
- `GET /api/v1/projects/mobile/{id}` - Get optimized project details for mobile
- `GET /api/v1/projects/mobile/dashboard` - Get dashboard data for mobile

See [Flutter API Support Documentation](./docs/FLUTTER_API_SUPPORT.md) for more details.

### Health Check
- `GET /health` - API health status

## Configuration

- **Local**: http://localhost:5001
- **Database**: PostgreSQL (local) / In-Memory (Docker)
- **Authentication**: JWT with role-based access

## Testing

```bash
# Health check
curl http://localhost:5001/health

# Test with comprehensive test files
# See tests/http/ directory for complete test collection
```
