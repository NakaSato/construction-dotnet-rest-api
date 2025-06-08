# Solar Projects REST API

This project is a comprehensive REST API built using .NET 9.0 for managing solar projects, tasks, and todo items. It provides complete CRUD operations with modern features including JWT authentication, file uploads, database integration, and Azure deployment capabilities.

## 🚀 Quick Start

### **Option 1: Local Development (Recommended)**
```bash
# Clone and navigate to the project
git clone <repository-url>
cd dotnet-rest-api

# Restore dependencies
dotnet restore

# Run the application
dotnet run --urls "http://localhost:5001"

# Visit http://localhost:5001 for Swagger documentation
```

### **Option 2: Docker Development**
```bash
# Start with Docker Compose (includes PostgreSQL)
docker-compose -f docker-compose.dev.yml up -d

# Application will be available at http://localhost:5001
```

## 📱 Current Status

✅ **Application Running**: http://localhost:5001  
✅ **Swagger UI**: http://localhost:5001 (Interactive API documentation)  
✅ **Health Check**: http://localhost:5001/health  
✅ **Todo API**: Full CRUD operations working  
✅ **PostgreSQL**: Available via Docker on port 5432  
✅ **Azure Deployment**: Infrastructure ready (see `DEPLOYMENT_READY.md`)  

## 🏗️ Project Structure

- **Controllers/**
  - `TodoController.cs`: Legacy Todo item management
  - `HealthController.cs`: Application health monitoring
  - `V1/`: Versioned API controllers (Auth, Projects, Tasks, Users, Images)
  
- **Models/**
  - `TodoItem.cs`: Todo data structure
  - Domain models for solar project management
  
- **Services/**
  - `TodoService.cs`: Todo business logic
  - Project, Task, User, Auth, and Image services
  
- **Data/**
  - `TodoContext.cs`: Database contexts (In-Memory + PostgreSQL)
  
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

## Usage

- **GET /todos**: Retrieve all Todo items.
- **GET /todos/{id}**: Retrieve a specific Todo item by ID.
- **POST /todos**: Create a new Todo item.
- **PUT /todos/{id}**: Update an existing Todo item.
- **DELETE /todos/{id}**: Delete a Todo item.

## Contributing

Feel free to submit issues or pull requests for improvements or bug fixes.