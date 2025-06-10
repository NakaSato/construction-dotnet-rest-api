# Solar Projects REST API with Calendar Management

This project is a comprehensive REST API built using .NET 9.0 for managing solar projects, tasks, daily reports, work requests, and calendar events. It provides complete CRUD operations with modern features including JWT authentication, file uploads, advanced caching, rate limiting, and comprehensive calendar management.

## 🚀 Quick Start

### **Local Development (Recommended)**
```bash
# Clone and navigate to the project
git clone <repository-url>
cd dotnet-rest-api

# Restore dependencies
dotnet restore

# Run the application
dotnet run --urls "http://localhost:5002"

# Visit http://localhost:5002 for Swagger documentation
```

### **Docker Development**
```bash
# Start with Docker Compose (includes PostgreSQL)
docker-compose -f docker-compose.dev.yml up -d

# Application will be available at http://localhost:5002
```

## 📱 Current Status

✅ **Application Running**: http://localhost:5002  
✅ **Swagger UI**: http://localhost:5002 (Interactive API documentation)  
✅ **Health Check**: http://localhost:5002/health  
✅ **Calendar API**: Complete calendar management with conflict detection  
✅ **Daily Reports API**: Comprehensive field reporting workflow  
✅ **Work Requests API**: Change orders and additional work tracking  
✅ **Advanced Features**: Caching, rate limiting, HATEOAS support  
✅ **Database**: Entity Framework Core with PostgreSQL  
✅ **Azure Deployment**: Infrastructure ready (see `DEPLOYMENT_READY.md`)  

## 🎯 Key Features

### **📅 Calendar Management**
- **Event Scheduling**: Create, update, delete calendar events
- **Conflict Detection**: Smart scheduling conflict detection
- **Event Types**: Meeting, Deadline, Installation, Maintenance, Training, Other
- **Priority Management**: Low, Medium, High, Critical priorities
- **Project Integration**: Link events to projects and tasks
- **User Assignment**: Assign events to team members
- **Upcoming Events**: Filter events by date range
- **Recurring Events**: Framework for future recurring event support

### **📊 Daily Reports**
- **Field Reporting**: Comprehensive daily work reports
- **Workflow Management**: Draft → Submitted → Approved/Rejected
- **Work Progress**: Track work items, personnel, materials, equipment
- **Photo Integration**: Associate images with daily reports
- **HATEOAS Support**: Hypermedia-driven API navigation

### **🔧 Work Requests**
- **Change Orders**: Manage additional work requests
- **Priority Tracking**: Low, Medium, High, Critical priorities
- **Task Management**: Break down work requests into tasks
- **Comment System**: Collaborative commenting on work requests
- **Status Tracking**: New, InProgress, Completed, Cancelled states

### **⚡ Performance & Reliability**
- **Caching**: 5-minute cache duration on frequently accessed endpoints
- **Rate Limiting**: Configurable rate limiting for API protection
- **Health Monitoring**: Comprehensive health checks with system metrics
- **Error Handling**: Consistent error response format with validation
- **Authentication**: JWT-based security with role-based access control

## 🏗️ Project Structure

- **Controllers/**
  - `HealthController.cs`: Application health monitoring
  - `V1/CalendarController.cs`: Calendar event management
  - `V1/DailyReportsController.cs`: Field reporting system
  - `V1/WorkRequestsController.cs`: Work request management
  - `V1/`: Additional versioned API controllers (Auth, Projects, Tasks, Users, Images)
  
- **Models/**
  - `CalendarEvent.cs`: Calendar event domain model
  - `DailyReport.cs`: Daily report with associated entities
  - `WorkRequest.cs`: Work request management
  - Additional domain models for solar project management
  
- **Services/**
  - `CalendarService.cs`: Calendar business logic
  - `DailyReportService.cs`: Report workflow management
  - `WorkRequestService.cs`: Work request operations
  - Additional services for projects, tasks, users, auth, and images
  
- **Data/**
  - `ApplicationDbContext.cs`: Main database context (PostgreSQL)
  
- **Migrations/**
  - Entity Framework Core database migrations

## 🔧 Development Commands

### **Running the Application**
```bash
# Standard run
dotnet run --urls "http://localhost:5001"

# Watch mode (auto-reload on changes)
dotnet watch run --urls "http://localhost:5001"

# Build project
dotnet build

# Run tests
dotnet test
```

### **Database Management**
```bash
# Create migration
dotnet ef migrations add YourMigrationName --context ApplicationDbContext

# Update database
dotnet ef database update --context ApplicationDbContext

# Drop database
dotnet ef database drop --context ApplicationDbContext
```

### **Docker Commands**
```bash
# Start development environment
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop services
docker-compose -f docker-compose.dev.yml down
```

## 🚀 API Endpoints

### **Solar Projects Management API**
- **Authentication**: `/api/v1/auth` - Login, register, JWT token management
- **Projects**: `/api/v1/projects` - Project CRUD operations
- **Tasks**: `/api/v1/projects/{projectId}/tasks` - Task management within projects
- **Users**: `/api/v1/users` - User management and profiles
- **Images**: `/api/v1/images` - File upload and metadata management
- **Daily Reports**: `/api/v1/daily-reports` - Construction daily reporting
- **Work Requests**: `/api/v1/work-requests` - Work request management

### **System Health**
- **Health Check**: `/health` - Application health status

## Contributing

Feel free to submit issues or pull requests for improvements or bug fixes.
<!-- Deployment triggered: 2025-06-08 14:26:14 -->
