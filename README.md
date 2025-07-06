# Solar Projects API

.NET 9.0 REST API for solar project managemen## 👥 User Management

### Test Admin Account
- **Username**: `test_admin`
- **Password**: `Admin123!`
- **Role**: Admin

### Create New Admin User
```bash
# Quick method - use automated script
./scripts/create_admin_user.sh

# Manual method - see docs/CURL_COMMANDS.md
curl -X POST "http://localhost:5001/api/v1/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🧪 Testing

```bash
# Health check
curl http://localhost:5001/health

# Test authentication  
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test_admin","password":"Admin123!"}'
```

## 📚 Documentation

- **[User Management Guide](docs/USER_MANAGEMENT_GUIDE.md)** - Authentication & user creation
- **[Real-Time Features](docs/api/00_REAL_TIME_LIVE_UPDATES.md)** - SignalR implementation
- **[Curl Commands](docs/CURL_COMMANDS.md)** - Quick API reference
- **[Database Schema](docs/schema.sql)** - Complete database structure-time features and role-based access.

## 🚀 Quick Start

```bash
# Run locally
dotnet run --urls "http://localhost:5001"

# Run with Docker
./scripts/deploy-docker.sh

# API Documentation
http://localhost:5001
```

## � Tech Stack

- **.NET 9.0** with Entity Framework Core
- **PostgreSQL** (local) / **In-Memory** (Docker)
- **JWT Authentication** with role-based access
- **SignalR** for real-time notifications
- **Swagger/OpenAPI** documentation

## � Roles & Permissions

| Role | Create Projects | Delete Projects | View Reports |
|------|----------------|----------------|--------------|
| **Admin** | ✅ | ✅ | ✅ |
| **Manager** | ✅ | ✅ | ✅ |
| **User** | ❌ | ❌ | ✅ |
| **Viewer** | ❌ | ❌ | ✅ |

## � API Endpoints

### Authentication
```bash
POST /api/v1/auth/login
```

### Core Resources
```bash
GET /api/v1/projects         # List projects
POST /api/v1/projects        # Create project (Admin/Manager)
GET /api/v1/tasks           # List tasks
POST /api/v1/daily-reports  # Submit report
```

### Health & Docs
```bash
GET /health                 # API status
GET /                      # Swagger UI
```

## ⚙️ Configuration

### Local Development
- **Port**: http://localhost:5001
- **Database**: PostgreSQL
- **Environment**: Development

### Docker Deployment
- **Port**: http://localhost:8080
- **Database**: In-Memory
- **Environment**: Docker

## � Testing

```bash
# Health check
curl http://localhost:5001/health

# Test authentication
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```
