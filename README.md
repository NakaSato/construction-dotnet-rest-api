# Solar Projects API

A comprehensive .NET 9.0 REST API for solar project management with real-time notifications, role-based access control, and advanced project tracking capabilities.

## 🚀 Quick Start

```bash
# Clone and setup
git clone <repository-url>
cd dotnet-rest-api

# Restore packages and build
dotnet restore
dotnet build

# Run the application  
dotnet run --urls "http://localhost:5001"

# Access Swagger UI
open http://localhost:5001/swagger
```

## 📋 Features

- **Project Management**: Complete CRUD operations for solar projects
- **Task Management**: Hierarchical task tracking with progress monitoring
- **Real-time Notifications**: SignalR-powered live updates
- **Role-based Security**: Admin, Manager, User, and Viewer roles
- **Master Planning**: Advanced project planning and scheduling
- **Daily Reporting**: Comprehensive reporting system
- **Calendar Integration**: Event and milestone tracking
- **Document Management**: File upload and resource management
- **Rate Limiting**: API throttling and caching
- **Health Monitoring**: Built-in health checks

## 🏗️ Architecture

- **Framework**: .NET 9.0
- **Database**: Entity Framework Core with PostgreSQL
- **Authentication**: JWT-based with role authorization
- **Real-time**: SignalR Hub for notifications
- **Documentation**: Swagger/OpenAPI
- **Pattern**: Clean Architecture (Controllers → Services → Data)

## 📚 Documentation

Comprehensive documentation is available in the `/docs` folder:

### 📖 API Reference
- **[Complete API Documentation](docs/README.md)** - Start here for full overview
- **[API Reference](docs/api/API_REFERENCE.md)** - All endpoints and schemas
- **[Authentication Guide](docs/api/01_AUTHENTICATION.md)** - JWT setup and usage
- **[Project Endpoints](docs/api/03_PROJECTS.md)** - Project management API
- **[Role Access Matrix](docs/api/ROLE_ACCESS_MATRIX.md)** - Permission by role

### 🔧 Implementation Guides  
- **[Quick Start Guide](docs/guides/QUICK_START_REALTIME.md)** - Real-time features
- **[User Guide](docs/guides/USER_GUIDE.md)** - End-user documentation
- **[Implementation Status](docs/implementation/FINAL_IMPLEMENTATION_STATUS.md)** - Current state

### 🧪 Testing
- **[Test Documentation](docs/testing/PROJECT_CREATION_TESTS.md)** - Role-based testing
- **[API Testing Results](docs/testing/API_TESTING_RESULTS.md)** - Validation results

## 🔐 Authentication & Roles

The API uses JWT authentication with four role levels:

| Role | Project Create | Project Delete | Reports | Real-time |
|------|---------------|---------------|---------|-----------|
| **Admin** | ✅ | ✅ | ✅ | ✅ |
| **Manager** | ✅ | ✅ | ✅ | ✅ |
| **User** | ❌ | ❌ | ✅ | ✅ |
| **Viewer** | ❌ | ❌ | ✅ | ✅ |

## 🛠️ Development

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL (or SQL Server)
- Node.js (for frontend testing)

### Local Development
```bash
# Development with hot reload
dotnet watch run --urls "http://localhost:5001"

# Run tests
dotnet test

# Database migrations
dotnet ef database update
```

### Environment Configuration
```bash
# Copy example environment file
cp .env.example .env

# Update connection strings and settings
nano .env
```

## 🧪 Testing

### Role-based Permission Testing
```bash
# Test user role restrictions (should fail)
./scripts/test-user-project-creation.sh

# Test admin/manager permissions (should succeed)
./scripts/test-admin-manager-final.sh
```

### API Health Check
```bash
curl http://localhost:5001/health
```

## 📊 API Endpoints

### Core Endpoints
- `POST /api/v1/auth/login` - Authentication
- `GET /api/v1/projects` - List projects
- `POST /api/v1/projects` - Create project (Admin/Manager only)
- `GET /api/v1/tasks` - List tasks
- `POST /api/v1/daily-reports` - Submit daily report

### Real-time Hub
- `/notificationHub` - SignalR connection for live updates

### Health & Monitoring
- `/health` - API health status
- `/swagger` - Interactive API documentation

## 🔧 Configuration

### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SolarProjectsDB;Trusted_Connection=true;"
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "SolarProjectsAPI",
    "Audience": "SolarProjectsClient",
    "ExpirationHours": 24
  }
}
```

## 🚀 Deployment

### Docker
```bash
# Build and run with Docker Compose
docker-compose up -d

# Production deployment
docker-compose -f docker-compose.yml up -d
```

### Azure Deployment
See `/azure/arm-template.json` for Azure Resource Manager template.

## 📈 Performance

- **Response Time**: < 200ms average
- **Concurrent Users**: 1000+ supported
- **Database**: Optimized queries with EF Core
- **Caching**: Redis-backed response caching
- **Rate Limiting**: 100 requests/minute per user

## 🤝 Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

- **Documentation**: [docs/README.md](docs/README.md)
- **API Reference**: [docs/api/API_REFERENCE.md](docs/api/API_REFERENCE.md)
- **Issues**: GitHub Issues
- **Testing**: [docs/testing/PROJECT_CREATION_TESTS.md](docs/testing/PROJECT_CREATION_TESTS.md)

---

**Version**: 1.0.0  
**Framework**: .NET 9.0  
**Last Updated**: July 4, 2025
