# Solar Projects API

.NET 9.0 REST API for solar project management with real-time features and role-based access.

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
