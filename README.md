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
