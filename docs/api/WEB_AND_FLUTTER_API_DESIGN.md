# 🌐📱 Web App & Flutter Mobile API Design Guide

## 🎯 Overview

This document provides a comprehensive design guide for supporting both **Web Applications** and **Flutter Mobile Apps** with your Solar Project Management API. The API is architected to provide optimal performance, usability, and developer experience across platforms.

## 🏗️ API Architecture Principles

### 1. **Platform-Agnostic Design**
- **Single Codebase**: One API serves both web and mobile clients
- **Consistent Data Models**: Same DTOs and response formats across platforms
- **Uniform Authentication**: JWT-based auth works for web and mobile
- **RESTful Standards**: HTTP verbs, status codes, and resource naming

### 2. **Mobile-First Optimizations**
- **Lightweight Responses**: Minimal payload sizes
- **Batch Operations**: Reduce API calls with bulk endpoints
- **Offline Support**: Data structures optimized for local storage
- **Progressive Loading**: Pagination and lazy loading support

### 3. **Web Application Enhancements**
- **Rich Pagination**: HATEOAS links for advanced web components
- **Advanced Filtering**: Complex query capabilities
- **Real-time Updates**: SignalR integration for live dashboards
- **Bulk Operations**: Admin interfaces with multi-select actions

## 📊 Core API Features

### ✅ **Authentication & Security**
```csharp
// Flexible login - username OR email
POST /api/v1/auth/login
{
  "username": "admin@example.com",  // Can be username or email
  "password": "Admin123!"
}

// JWT token with role-based access
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

// Role-based permissions
- Admin: Full CRUD access
- Manager: Project management + reporting
- User: Field operations + daily reports
- Viewer: Read-only access
```

### ✅ **Standardized Response Format**
```csharp
// All endpoints return consistent format
{
  "success": true,
  "message": "Success",
  "data": {
    // Response payload
  },
  "errors": [],
  "pagination": {
    "totalCount": 97,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### ✅ **Advanced Querying**
```csharp
// Base query parameters for all list endpoints
public class BaseQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;  // Max 100
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public string? Search { get; set; }
    public string? Fields { get; set; }  // Field selection
}

// Example usage
GET /api/v1/projects?pageSize=20&sortBy=startDate&sortOrder=desc&fields=projectId,projectName,status
```

## 🌐 Web Application Support

### **Rich Dashboard APIs**
```csharp
// Enhanced pagination with HATEOAS
GET /api/v1/projects/rich
{
  "data": [...],
  "pagination": {
    "totalItems": 97,
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "links": {
      "first": "/api/v1/projects/rich?page=1",
      "last": "/api/v1/projects/rich?page=10",
      "next": "/api/v1/projects/rich?page=2",
      "previous": null
    }
  }
}
```

### **Advanced Filtering**
```csharp
// Complex queries for admin interfaces
GET /api/v1/projects/advanced?filter=status:Active&search=Solar&sortBy=startDate
GET /api/v1/users/advanced?role=Manager&isActive=true&fields=userId,fullName,email
```

### **Bulk Operations**
```csharp
// Multi-select operations for web admin
PATCH /api/v1/projects/bulk-status
{
  "projectIds": ["id1", "id2", "id3"],
  "status": "Active"
}

DELETE /api/v1/users/bulk
{
  "userIds": ["id1", "id2", "id3"]
}
```

### **Real-time Updates**
```csharp
// SignalR Hub for live dashboard updates
public class NotificationHub : Hub
{
    // Project status changes
    await Clients.Group("ProjectManagers").SendAsync("ProjectStatusChanged", data);
    
    // Daily report submissions
    await Clients.Group("Admins").SendAsync("NewDailyReport", data);
    
    // Task assignments
    await Clients.User(userId).SendAsync("TaskAssigned", task);
}
```

## 📱 Flutter Mobile App Support

### **Mobile-Optimized Endpoints**
```dart
// Lightweight responses for mobile
GET /api/v1/projects/mobile?fields=projectId,projectName,status,location
{
  "data": [
    {
      "projectId": "123",
      "projectName": "Solar Alpha",
      "status": "Active",
      "location": {
        "latitude": 37.7749,
        "longitude": -122.4194
      }
    }
  ]
}
```

### **Image Upload with Metadata**
```dart
// Mobile-optimized image upload
POST /api/v1/images/upload
Content-Type: multipart/form-data

{
  "file": <image_data>,
  "metadata": {
    "location": {
      "latitude": 37.7749,
      "longitude": -122.4194
    },
    "deviceInfo": {
      "device": "iPhone 13",
      "os": "iOS 15.0"
    },
    "timestamp": "2025-07-08T10:30:00Z"
  }
}
```

### **Offline-First Data Structures**
```dart
// Optimized for SQLite storage
class Project {
  final String projectId;
  final String projectName;
  final String status;
  final DateTime? lastModified;
  final bool isDeleted;
  final Map<String, dynamic> location;
  
  // Sync metadata
  final bool isSynced;
  final DateTime? lastSyncAt;
}
```

### **Batch Sync Operations**
```dart
// Efficient sync for mobile
POST /api/v1/sync/daily-reports
{
  "reports": [
    {
      "localId": "temp_1",
      "projectId": "123",
      "data": {...},
      "pendingImages": ["local_img_1", "local_img_2"]
    }
  ]
}
```

## 🔧 Implementation Strategies

### **1. Controller Design Patterns**

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProjectsController : BaseApiController
{
    // Standard endpoint for both web and mobile
    [HttpGet]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<ProjectDto>>>> GetProjects(
        [FromQuery] ProjectQueryParameters parameters)
    {
        // Supports filtering, sorting, pagination, field selection
    }

    // Mobile-optimized endpoint
    [HttpGet("mobile")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectMobileDto>>>> GetProjectsMobile(
        [FromQuery] MobileQueryParameters parameters)
    {
        // Lightweight DTOs, essential fields only
    }

    // Web admin endpoint with rich pagination
    [HttpGet("rich")]
    [MediumCache]
    public async Task<ActionResult<ApiResponseWithPagination<ProjectDto>>> GetProjectsWithRichPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // HATEOAS links, metadata, navigation helpers
    }
}
```

### **2. DTO Optimization**

```csharp
// Standard DTO - Full data
public class ProjectDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Description { get; set; }
    public LocationDto? Location { get; set; }
    public UserDto? Manager { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
}

// Mobile DTO - Essential data only
public class ProjectMobileDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public DateTime? LastModified { get; set; }
}

// List DTO - Summary for listings
public class ProjectSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public decimal ProgressPercentage { get; set; }
}
```

### **3. Caching Strategy**

```csharp
// Attribute-based caching
[ShortCache] // 5 minutes - frequently changing data
[MediumCache] // 15 minutes - moderate change frequency
[LongCache] // 60 minutes - static/reference data

// Implementation
public class CacheAttribute : Attribute
{
    public int DurationMinutes { get; set; }
    public string? CacheKey { get; set; }
    public bool VaryByUser { get; set; } = false;
    public bool VaryByRole { get; set; } = false;
}
```

### **4. Rate Limiting**

```csharp
// Tiered rate limiting
[RateLimit(50, 60)] // 50 requests per minute - default
[RateLimit(100, 60)] // 100 requests per minute - authenticated
[RateLimit(200, 60)] // 200 requests per minute - admin

// Per-endpoint limits
[HttpPost]
[RateLimit(10, 60)] // 10 file uploads per minute
public async Task<IActionResult> UploadImage(IFormFile file)
```

## 📱 Flutter Integration Examples

### **1. API Client Setup**

```dart
class ApiClient {
  static const String baseUrl = 'https://solar-projects-api-dev.azurewebsites.net';
  static const String localUrl = 'http://localhost:5001';
  
  // Auto token management
  static Future<Map<String, String>> _getHeaders() async {
    final token = await SecureStorage.getToken();
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }
  
  // Auto retry with token refresh
  static Future<Map<String, dynamic>> request(
    String method,
    String endpoint, {
    Map<String, dynamic>? data,
    bool requiresAuth = true,
  }) async {
    try {
      final response = await _makeRequest(method, endpoint, data);
      return _handleResponse(response);
    } on UnauthorizedException {
      if (requiresAuth) {
        await _refreshToken();
        final response = await _makeRequest(method, endpoint, data);
        return _handleResponse(response);
      }
      rethrow;
    }
  }
}
```

### **2. Model Classes**

```dart
class Project {
  final String projectId;
  final String projectName;
  final String status;
  final Location? location;
  final DateTime? lastModified;
  
  Project({
    required this.projectId,
    required this.projectName,
    required this.status,
    this.location,
    this.lastModified,
  });
  
  factory Project.fromJson(Map<String, dynamic> json) {
    return Project(
      projectId: json['projectId'],
      projectName: json['projectName'],
      status: json['status'],
      location: json['location'] != null 
        ? Location.fromJson(json['location']) 
        : null,
      lastModified: json['lastModified'] != null
        ? DateTime.parse(json['lastModified'])
        : null,
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'projectId': projectId,
      'projectName': projectName,
      'status': status,
      'location': location?.toJson(),
      'lastModified': lastModified?.toIso8601String(),
    };
  }
}
```

### **3. Repository Pattern**

```dart
class ProjectRepository {
  final ApiClient _apiClient;
  final DatabaseHelper _db;
  
  ProjectRepository(this._apiClient, this._db);
  
  // Get projects with offline support
  Future<List<Project>> getProjects({
    int page = 1,
    int pageSize = 20,
    bool forceRefresh = false,
  }) async {
    try {
      if (forceRefresh || await _shouldRefresh()) {
        final response = await _apiClient.get(
          '/projects/mobile',
          params: {'page': page, 'pageSize': pageSize},
        );
        
        final projects = (response['data'] as List)
          .map((json) => Project.fromJson(json))
          .toList();
        
        // Cache locally
        await _db.cacheProjects(projects);
        return projects;
      } else {
        // Return cached data
        return await _db.getProjects(page, pageSize);
      }
    } catch (e) {
      // Fallback to cache on network error
      return await _db.getProjects(page, pageSize);
    }
  }
}
```

### **4. State Management (Provider/Bloc)**

```dart
class ProjectProvider extends ChangeNotifier {
  final ProjectRepository _repository;
  
  List<Project> _projects = [];
  bool _isLoading = false;
  String? _error;
  
  List<Project> get projects => _projects;
  bool get isLoading => _isLoading;
  String? get error => _error;
  
  Future<void> loadProjects({bool refresh = false}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();
    
    try {
      _projects = await _repository.getProjects(forceRefresh: refresh);
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
  
  Future<void> createProject(Project project) async {
    try {
      final newProject = await _repository.createProject(project);
      _projects.insert(0, newProject);
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      notifyListeners();
    }
  }
}
```

## 🌐 Web Application Examples

### **1. React/Vue.js API Service**

```javascript
class ApiService {
  constructor() {
    this.baseURL = 'https://solar-projects-api-dev.azurewebsites.net/api/v1';
    this.token = localStorage.getItem('jwt_token');
  }
  
  async request(method, endpoint, data = null) {
    const config = {
      method,
      headers: {
        'Content-Type': 'application/json',
        ...(this.token && { 'Authorization': `Bearer ${this.token}` }),
      },
      ...(data && { body: JSON.stringify(data) }),
    };
    
    try {
      const response = await fetch(`${this.baseURL}${endpoint}`, config);
      const result = await response.json();
      
      if (!response.ok) {
        throw new Error(result.message || 'API request failed');
      }
      
      return result;
    } catch (error) {
      if (error.status === 401) {
        // Handle token expiry
        await this.refreshToken();
        return this.request(method, endpoint, data);
      }
      throw error;
    }
  }
  
  // Rich pagination for data tables
  async getProjectsWithPagination(params) {
    const queryString = new URLSearchParams(params).toString();
    return this.request('GET', `/projects/rich?${queryString}`);
  }
}
```

### **2. Data Table Component**

```javascript
// React DataTable component
function ProjectsDataTable() {
  const [projects, setProjects] = useState([]);
  const [pagination, setPagination] = useState({});
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 25,
    sortBy: 'startDate',
    sortOrder: 'desc',
    status: '',
    search: '',
  });
  
  useEffect(() => {
    loadProjects();
  }, [filters]);
  
  const loadProjects = async () => {
    setLoading(true);
    try {
      const response = await apiService.getProjectsWithPagination(filters);
      setProjects(response.data);
      setPagination(response.pagination);
    } catch (error) {
      console.error('Failed to load projects:', error);
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <div className="projects-table">
      <TableFilters 
        filters={filters} 
        onFiltersChange={setFilters} 
      />
      
      <DataTable
        columns={columns}
        data={projects}
        loading={loading}
        pagination={pagination}
        onPageChange={(page) => setFilters(prev => ({ ...prev, page }))}
        onSortChange={(sortBy, sortOrder) => 
          setFilters(prev => ({ ...prev, sortBy, sortOrder }))
        }
      />
    </div>
  );
}
```

## 🚀 Performance Optimizations

### **1. Response Compression**
```csharp
// Automatic compression for large responses
[HttpGet]
[ResponseCompression]
public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjects()
```

### **2. Field Selection**
```csharp
// Reduce payload size
GET /api/v1/projects?fields=projectId,projectName,status
```

### **3. Conditional Requests**
```csharp
// ETags for cache validation
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
{
    var project = await _service.GetProjectAsync(id);
    var etag = GenerateETag(project);
    
    if (Request.Headers.IfNoneMatch == etag)
        return StatusCode(304); // Not Modified
    
    Response.Headers.ETag = etag;
    return Ok(project);
}
```

### **4. Background Processing**
```csharp
// Async operations for heavy tasks
[HttpPost]
public async Task<ActionResult<ApiResponse<string>>> ImportProjects(IFormFile file)
{
    var jobId = await _backgroundService.QueueImportJob(file);
    return Accepted(new { jobId, status = "Processing" });
}

[HttpGet("import-status/{jobId}")]
public async Task<ActionResult<ApiResponse<ImportStatus>>> GetImportStatus(string jobId)
{
    var status = await _backgroundService.GetJobStatus(jobId);
    return Ok(status);
}
```

## 🔒 Security Considerations

### **1. Role-Based Access Control**
```csharp
// Granular permissions
[HttpPost]
[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> CreateProject(CreateProjectRequest request)

[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteProject(Guid id)
```

### **2. Input Validation**
```csharp
public class CreateProjectRequest
{
    [Required]
    [StringLength(100)]
    public string ProjectName { get; set; }
    
    [Required]
    [EmailAddress]
    public string ManagerEmail { get; set; }
    
    [Range(1000, 10000000)]
    public decimal? Budget { get; set; }
}
```

### **3. Audit Logging**
```csharp
[HttpPut("{id}")]
[AuditLog] // Custom attribute
public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequest request)
```

## 📊 Monitoring & Analytics

### **1. Health Checks**
```csharp
// Comprehensive health monitoring
GET /health/detailed
{
  "status": "Healthy",
  "database": "Connected",
  "cache": "Available",
  "storage": "Accessible",
  "memory": "Normal",
  "dependencies": {
    "signalr": "Connected",
    "background_service": "Running"
  }
}
```

### **2. API Metrics**
```csharp
// Built-in metrics
- Request/response times
- Error rates by endpoint
- Authentication failures
- Rate limit violations
- Cache hit/miss ratios
```

### **3. Usage Analytics**
```csharp
// Track API usage patterns
- Most used endpoints
- Peak usage times
- Geographic distribution
- Device/platform breakdown
- Feature adoption rates
```

## 🎯 Best Practices Summary

### **✅ DO:**
- Use consistent response formats across all endpoints
- Implement proper error handling with meaningful messages
- Provide both lightweight mobile and rich web endpoints
- Use caching strategically based on data volatility
- Implement rate limiting to prevent abuse
- Support offline scenarios for mobile apps
- Use proper HTTP status codes
- Implement comprehensive logging and monitoring

### **❌ DON'T:**
- Return different response structures for similar endpoints
- Expose sensitive data in error messages
- Ignore pagination for large datasets
- Cache user-specific data globally
- Allow unlimited requests without rate limiting
- Force mobile apps to use heavy web endpoints
- Use HTTP 200 for error responses
- Skip input validation and sanitization

## 🚀 Next Steps

1. **Implement mobile-specific endpoints** with lightweight DTOs
2. **Add batch operation support** for admin interfaces
3. **Enhance real-time capabilities** with SignalR
4. **Optimize query performance** with proper indexing
5. **Add comprehensive API documentation** with examples
6. **Implement API versioning** for future compatibility
7. **Add monitoring dashboards** for operational insights
8. **Create SDK packages** for common platforms

---

**🎉 Your Solar Projects API is already well-architected for both web and mobile applications! This guide provides the roadmap for optimizing and extending it further.**
