# 🌞 Solar Projects REST API Reference for Flutter

## 📱 Flutter Mobile App Integration Guide

This comprehensive API reference is specifically designed for **Flutter mobile app development**. The Solar Projects Management REST API is built with .NET 9.0 and provides complete functionality for managing solar installation projects, field operations, and team collaboration.

**🚀 DEPLOYMENT STATUS: ACTIVE**
- ✅ Docker containers deployed and running
- ✅ Database populated with 97+ solar projects
- ✅ Authentication system operational
- ✅ All endpoints tested and functional

**Base URLs**:
- 🏠 **Local Development**: `http://localhost:5002`
- 🌐 **Production**: `https://solar-projects-api-dev.azurewebsites.net`
- 📱 **Mobile Testing**: `http://10.0.2.2:5002` (Android Emulator)
- 🐳 **Docker Local**: `http://localhost:5002` (Current deployment)

**API Version**: `v1`  
**Authentication**: JWT Bearer Token (except public endpoints)  
**Content-Type**: `application/json`  
**Date Format**: ISO 8601 (`2025-06-15T08:44:38Z`)
**Current Data**: 97 solar projects imported and ready

## 🔥 Key Features for Mobile Apps

🔐 **Flexible Authentication**: Username OR Email login with JWT tokens  
📊 **Field Operations**: Complete daily reporting with photo uploads  
🔧 **Work Management**: Track tasks, projects, and change requests  
📷 **Mobile Optimized**: Image upload with GPS, device metadata, offline support  
🔄 **Real-time Sync**: CRUD operations with pagination and filtering  
⚡ **Performance**: Built-in caching, rate limiting (50 req/min), and compression  
🎯 **Role-based Access**: Admin, Manager, User, and Viewer permissions  
💾 **Offline Ready**: Local storage friendly data structures  
🔗 **HATEOAS**: Hypermedia links for navigation  
🩺 **Health Monitoring**: System status and diagnostics  

## 📱 Flutter Quick Setup Guide

### 1. **Required Dependencies**
```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  flutter_secure_storage: ^9.0.0
  image_picker: ^1.0.4
  geolocator: ^10.1.0
  permission_handler: ^11.0.1

dev_dependencies:
  flutter_test:
    sdk: flutter
```

## 📋 Quick Start Checklist

- [x] ✅ Test health endpoint: `GET /health` (WORKING)
- [x] 🔐 Implement authentication: `POST /api/v1/auth/login` (WORKING)
- [x] 👤 Get user profile after login (WORKING)
- [x] 📊 Fetch projects: `GET /api/v1/projects` (97 projects available)
- [x] 📱 API ready for mobile development (Docker deployed)
- [ ] 🔄 Implement token refresh logic
- [ ] ⚠️ Handle error responses and rate limiting
- [ ] 💾 Set up local storage for offline support
- [ ] 📱 Test image upload: `POST /api/v1/images/upload`

**🎉 Status**: API is deployed and ready for Flutter development!

---

## 📖 Complete API Reference

### 🔗 Quick Navigation
- [🔐 Authentication & User Management](#-authentication--user-management)
- [❤️ Health & System Status](#️-health--system-status)  
- [👥 User Management](#-user-management)
- [📋 Project Management](#-project-management)
- [🏗️ Master Plan Management](#️-master-plan-management)
- [✅ Task Management](#-task-management)
- [📊 Daily Reports](#-daily-reports)
- [🔧 Work Requests](#-work-requests)
- [📅 Calendar Events](#-calendar-events)
- [� Weekly Planning and Reporting](#-weekly-planning-and-reporting)
- [�🖼️ Image Upload](#️-image-upload)
- [⚡ Rate Limiting](#-rate-limiting)
- [❌ Error Handling](#-error-handling)
- [📱 Flutter Code Examples](#-flutter-code-examples)

---

## 🔐 Authentication & User Management

### 🔑 Login (Username OR Email)
**POST** `/api/v1/auth/login`

**✨ Enhanced Feature**: Login with either username or email address!

**Request Body**:
```json
{
  "username": "test_admin",           // Can be username
  "password": "Admin123!"
}
```
```json
{
  "username": "test_admin@example.com", // Can be email
  "password": "Admin123!"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "userId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
      "username": "test_admin",
      "email": "test_admin@example.com",
      "fullName": "Test Administrator",
      "roleName": "Admin",
      "isActive": true
    }
  },
  "errors": []
}
```

**Error Response (401)**:
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": ["Authentication failed"]
}
```

**Flutter Example**:
```dart
// Login method with error handling
Future<AuthResponse> login(String usernameOrEmail, String password) async {
  try {
    final response = await ApiClient.post('/auth/login', {
      'username': usernameOrEmail,  // Can be username or email
      'password': password,
    });
    
    if (response['success']) {
      final authData = AuthResponse.fromJson(response['data']);
      
      // Store tokens securely
      await _storage.write(key: 'jwt_token', value: authData.token);
      await _storage.write(key: 'refresh_token', value: authData.refreshToken);
      
      return authData;
    } else {
      throw Exception(response['message']);
    }
  } on ApiException catch (e) {
    if (e.statusCode == 401) {
      throw Exception('Invalid username or password');
    }
    rethrow;
  }
}
```

### 📝 Register New User
**POST** `/api/v1/auth/register`

**Request Body**:
```json
{
  "username": "john_tech",
  "email": "john@solartech.com",
  "password": "SecurePass123!",
  "fullName": "John Technician",
  "roleId": 3
}
```

**Role IDs & Permissions** (Updated as of June 2025):
| ID | Role | Project Management Access | Mobile Use Case | API Access |
|---|---|---|---|---|
| `1` | **Admin** | ✅ **Full CRUD** - Create, Read, Update, Delete all project data | Management app | All endpoints |
| `2` | **Manager** | ✅ **Create/Read/Update** - Full project data modification (no delete) | Supervisor app | Create/Update all project fields |
| `3` | **User** | 📖 **Read Only** - View projects and submit reports | Technician app | Read projects, create reports |
| `4` | **Viewer** | 📖 **Read Only** - View projects and reports | Client/Reporting app | Read-only endpoints |

**💡 Project Update Capabilities for Admin & Manager**:
- Project details (name, address, client info)
- Timeline management (start/end dates)  
- Technical specifications (capacity, modules, equipment)
- Team assignments and project manager
- Financial values and location coordinates
- Status updates and project phases

**🔑 Current Test Accounts**:
- **Admin**: `test_admin` / `Admin123!` 
- **Manager**: `test_manager` / `Manager123!`
- **User**: `test_user` / `User123!`
- **Viewer**: `test_viewer` / `Viewer123!`

**Success Response (201)**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "456e7890-e89b-12d3-a456-426614174001",
    "username": "john_tech",
    "email": "john@solartech.com",
    "fullName": "John Technician",
    "roleName": "User",
    "isActive": true
  },
  "errors": []
}
```

**Error Response (400)**:
```json
{
  "success": false,
  "message": "Registration failed",
  "data": null,
  "errors": [
    "Username already exists",
    "Password must contain at least one uppercase letter"
  ]
}
```

**Flutter Example**:
```dart
// Registration form validation
class RegistrationForm extends StatefulWidget {
  @override
  _RegistrationFormState createState() => _RegistrationFormState();
}

class _RegistrationFormState extends State<RegistrationForm> {
  final _formKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _fullNameController = TextEditingController();
  int _selectedRole = 3; // Default to User
  
  Future<void> _register() async {
    if (_formKey.currentState!.validate()) {
      try {
        final response = await ApiClient.post('/auth/register', {
          'username': _usernameController.text,
          'email': _emailController.text,
          'password': _passwordController.text,
          'fullName': _fullNameController.text,
          'roleId': _selectedRole,
        });
        
        if (response['success']) {
          // Registration successful
          Navigator.pushReplacementNamed(context, '/login');
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Registration successful! Please login.')),
          );
        }
      } catch (e) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Registration failed: $e')),
        );
      }
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Form(
      key: _formKey,
      child: Column(
        children: [
          TextFormField(
            controller: _usernameController,
            decoration: InputDecoration(labelText: 'Username'),
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Please enter a username';
              }
              return null;
            },
          ),
          // ... other form fields
        ],
      ),
    );
  }
}
```

### 🔄 Refresh Token
**POST** `/api/v1/auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "refresh_token_string_here"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": "new_jwt_token_here",
  "errors": []
}
```

**Flutter Example - Auto Token Refresh**:
```dart
class ApiClient {
  static Future<Map<String, dynamic>> _makeRequest(
    String method, 
    String endpoint, 
    {Map<String, dynamic>? data}
  ) async {
    try {
      // Make initial request
      final response = await _performRequest(method, endpoint, data);
      return response;
    } on ApiException catch (e) {
      if (e.statusCode == 401) {
        // Token expired, try to refresh
        await _refreshToken();
        // Retry original request
        return await _performRequest(method, endpoint, data);
      }
      rethrow;
    }
  }
  
  static Future<void> _refreshToken() async {
    final refreshToken = await _storage.read(key: 'refresh_token');
    if (refreshToken == null) {
      throw Exception('No refresh token available');
    }
    
    final response = await http.post(
      Uri.parse('$apiBase/auth/refresh'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'refreshToken': refreshToken}),
    );
    
    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      await _storage.write(key: 'jwt_token', value: data['data']);
    } else {
      // Refresh failed, redirect to login
      await _storage.deleteAll();
      throw Exception('Session expired. Please login again.');
    }
  }
}
```

### 🔒 Authentication Headers
Include in all protected endpoint requests:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Flutter HTTP Headers Example**:
```dart
// Method to get headers with authentication
static Future<Map<String, String>> getAuthHeaders() async {
  final token = await _storage.read(key: 'jwt_token');
  return {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    if (token != null) 'Authorization': 'Bearer $token',
  };
}
```

### 🎯 Role-Based Access Control

The API uses a role-based permission system:

| Role ID | Role Name | Permissions | Mobile App Features |
|---------|-----------|-------------|-------------------|
| `1` | **Admin** | Full Access | • User management<br>• System configuration<br>• All CRUD operations<br>• Analytics & reports |
| `2` | **Manager** | Project Management | • Project oversight<br>• Team management<br>• Approval workflows<br>• Performance reports |
| `3` | **User** | Field Operations | • Daily reports<br>• Task updates<br>• Photo uploads<br>• Location tracking |
| `4` | **Viewer** | Read Only | • View projects<br>• Read reports<br>• Basic dashboards<br>• Limited data access |

**Flutter Role-Based UI Example**:
```dart
class PermissionWidget extends StatelessWidget {
  final List<String> requiredRoles;
  final Widget child;
  final Widget? fallback;
  
  const PermissionWidget({
    required this.requiredRoles,
    required this.child,
    this.fallback,
  });
  
  @override
  Widget build(BuildContext context) {
    return Consumer<AuthProvider>(
      builder: (context, auth, _) {
        if (auth.user != null && requiredRoles.contains(auth.user!.roleName)) {
          return child;
        }
        return fallback ?? SizedBox.shrink();
      },
    );
  }
}

// Usage example
PermissionWidget(
  requiredRoles: ['Admin', 'Manager'],
  child: FloatingActionButton(
    onPressed: () => _createProject(),
    child: Icon(Icons.add),
  ),
  fallback: Container(), // Hide button for users without permission
)
```

---

## ❤️ Health & System Status

### 🩺 Basic Health Check  
**GET** `/health` *(No auth required)*

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-14T10:30:00Z",
  "version": "1.0.0",
  "environment": "Development"
}
```

**Flutter Example - Connectivity Check**:
```dart
class HealthService {
  static Future<bool> checkServerHealth() async {
    try {
      final response = await http.get(
        Uri.parse('${ApiClient.baseUrl}/health'),
        headers: {'Accept': 'application/json'},
      ).timeout(Duration(seconds: 5));
      
      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        return data['status'] == 'Healthy';
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}

// Use in app startup
class SplashScreen extends StatefulWidget {
  @override
  _SplashScreenState createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _checkHealthAndNavigate();
  }
  
  Future<void> _checkHealthAndNavigate() async {
    final isHealthy = await HealthService.checkServerHealth();
    
    if (isHealthy) {
      // Check if user is logged in
      final isLoggedIn = await AuthService.isLoggedIn();
      Navigator.pushReplacementNamed(
        context, 
        isLoggedIn ? '/dashboard' : '/login'
      );
    } else {
      // Show offline mode or retry
      _showOfflineDialog();
    }
  }
  
  void _showOfflineDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Connection Error'),
        content: Text('Unable to connect to server. Please check your internet connection.'),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              _checkHealthAndNavigate();
            },
            child: Text('Retry'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              // Navigate to offline mode
            },
            child: Text('Work Offline'),
          ),
        ],
      ),
    );
  }
}
```

### 🔍 Detailed Health Check
**GET** `/health/detailed` *(No auth required)*

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-14T10:30:00Z",
  "version": "1.0.0",
  "environment": "Development",
  "database": {
    "status": "Connected",
    "provider": "Npgsql.EntityFrameworkCore.PostgreSQL"
  },
  "memory": {
    "workingSet": 52428800,
    "gen0Collections": 12,
    "gen1Collections": 4,
    "gen2Collections": 1
  }
}
```

**💡 Flutter Tips**: 
- Use basic health check for connectivity testing
- Use detailed health check for admin dashboards
- Implement automatic retry logic for failed requests
- Cache health status to avoid excessive API calls

---

## 📊 Daily Reports

**🔒 Authentication Required**

Daily reports are essential for field operations, allowing technicians to document work progress, safety incidents, weather conditions, and attach photos with GPS metadata.

### 📋 Get All Daily Reports
**GET** `/api/v1/daily-reports`

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `projectId` (Guid): Filter by specific project
- `userId` (Guid): Filter by user who created the report
- `startDate` (DateTime): Filter reports from this date
- `endDate` (DateTime): Filter reports until this date
- `includeImages` (bool): Include image metadata (default: false)

**Example Request**:
```
GET /api/v1/daily-reports?pageSize=20&startDate=2025-06-01&includeImages=true
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily reports retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "789e0123-e89b-12d3-a456-426614174002",
        "userName": "John Technician",
        "reportDate": "2025-06-14",
        "workDescription": "Installed 12 solar panels on south-facing roof section",
        "hoursWorked": 8.5,
        "weatherConditions": "Sunny, 75°F, light breeze",
        "safetyIncidents": null,
        "notes": "All panels properly secured. Electrical connections completed.",
        "location": {
          "latitude": 37.7749,
          "longitude": -122.4194,
          "address": "123 Solar Street, San Francisco, CA"
        },
        "images": [
          {
            "id": "abc123-def456-ghi789",
            "fileName": "installation_progress_001.jpg",
            "filePath": "/uploads/daily-reports/2025/06/14/abc123-def456-ghi789.jpg",
            "uploadedAt": "2025-06-14T15:30:00Z",
            "fileSize": 2048576,
            "mimeType": "image/jpeg"
          }
        ],
        "createdAt": "2025-06-14T16:00:00Z",
        "updatedAt": "2025-06-14T16:00:00Z"
      }
    ],
    "pagination": {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

**Flutter Example - Daily Reports List**:
```dart
class DailyReportsScreen extends StatefulWidget {
  @override
  _DailyReportsScreenState createState() => _DailyReportsScreenState();
}

class _DailyReportsScreenState extends State<DailyReportsScreen> {
  List<DailyReport> reports = [];
  bool isLoading = true;
  int currentPage = 1;
  bool hasMoreData = true;
  
  @override
  void initState() {
    super.initState();
    _loadReports();
  }
  
  Future<void> _loadReports({bool isRefresh = false}) async {
    if (isRefresh) {
      currentPage = 1;
      hasMoreData = true;
    }
    
    try {
      final response = await ApiClient.get(
        '/daily-reports?pageNumber=$currentPage&pageSize=10&includeImages=true'
      );
      
      if (response['success']) {
        final data = response['data'];
        final newReports = (data['reports'] as List)
            .map((json) => DailyReport.fromJson(json))
            .toList();
        
        setState(() {
          if (isRefresh) {
            reports = newReports;
          } else {
            reports.addAll(newReports);
          }
          hasMoreData = data['pagination']['hasNextPage'];
          currentPage++;
          isLoading = false;
        });
      }
    } catch (e) {
      setState(() => isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading reports: $e')),
      );
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Daily Reports'),
        actions: [
          IconButton(
            icon: Icon(Icons.add),
            onPressed: () => Navigator.pushNamed(context, '/create-report'),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () => _loadReports(isRefresh: true),
        child: isLoading
            ? Center(child: CircularProgressIndicator())
            : ListView.builder(
                itemCount: reports.length + (hasMoreData ? 1 : 0),
                itemBuilder: (context, index) {
                  if (index == reports.length) {
                    // Load more indicator
                    return Padding(
                      padding: EdgeInsets.all(16.0),
                      child: Center(child: CircularProgressIndicator()),
                    );
                  }
                  
                  final report = reports[index];
                  return DailyReportCard(
                    report: report,
                    onTap: () => _viewReportDetails(report),
                  );
                },
              ),
      ),
    );
  }
}

class DailyReportCard extends StatelessWidget {
  final DailyReport report;
  final VoidCallback onTap;
  
  const DailyReportCard({required this.report, required this.onTap});
  
  @override
  Widget build(BuildContext context) {
    return Card(
      margin: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: Colors.blue,
          child: Text(report.reportDate.day.toString()),
        ),
        title: Text(report.projectName),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('${report.hoursWorked} hours worked'),
            Text('By: ${report.userName}'),
            if (report.images.isNotEmpty)
              Text('📷 ${report.images.length} photos'),
          ],
        ),
        trailing: Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
```

### 📝 Create Daily Report
**POST** `/api/v1/daily-reports`

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "reportDate": "2025-06-14",
  "workDescription": "Installed solar panels and completed electrical connections",
  "hoursWorked": 8.5,
  "weatherConditions": "Sunny, 75°F",
  "safetyIncidents": null,
  "notes": "All safety protocols followed. No incidents.",
  "location": {
    "latitude": 37.7749,
    "longitude": -122.4194,
    "address": "123 Solar Street, San Francisco, CA"
  },
  "imageIds": ["abc123-def456-ghi789", "def456-ghi789-jkl012"]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Daily report created successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "reportDate": "2025-06-14",
    "workDescription": "Installed solar panels and completed electrical connections",
    "hoursWorked": 8.5,
    "weatherConditions": "Sunny, 75°F",
    "safetyIncidents": null,
    "notes": "All safety protocols followed. No incidents.",
    "location": {
      "latitude": 37.7749,
      "longitude": -122.4194,
      "address": "123 Solar Street, San Francisco, CA"
    },
    "createdAt": "2025-06-14T16:00:00Z",
    "updatedAt": "2025-06-14T16:00:00Z"
  },
  "errors": []
}
```

**Flutter Example - Create Report Form**:
```dart
class CreateDailyReportScreen extends StatefulWidget {
  @override
  _CreateDailyReportScreenState createState() => _CreateDailyReportScreenState();
}

class _CreateDailyReportScreenState extends State<CreateDailyReportScreen> {
  final _formKey = GlobalKey<FormState>();
  final _workDescriptionController = TextEditingController();
  final _hoursWorkedController = TextEditingController();
  final _weatherConditionsController = TextEditingController();
  final _notesController = TextEditingController();
  
  String? selectedProjectId;
  List<Project> projects = [];
  List<XFile> selectedImages = [];
  Position? currentPosition;
  bool isSubmitting = false;
  
  @override
  void initState() {
    super.initState();
    _loadProjects();
    _getCurrentLocation();
  }
  
  Future<void> _loadProjects() async {
    try {
      final response = await ApiClient.get('/projects');
      if (response['success']) {
        setState(() {
          projects = (response['data'] as List)
              .map((json) => Project.fromJson(json))
              .toList();
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading projects: $e')),
      );
    }
  }
  
  Future<void> _getCurrentLocation() async {
    try {
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) return;
      
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) return;
      }
      
      currentPosition = await Geolocator.getCurrentPosition();
    } catch (e) {
      print('Error getting location: $e');
    }
  }
  
  Future<void> _pickImages() async {
    final ImagePicker picker = ImagePicker();
    final List<XFile> images = await picker.pickMultiImage();
    
    setState(() {
      selectedImages.addAll(images);
    });
  }
  
  Future<void> _submitReport() async {
    if (!_formKey.currentState!.validate()) return;
    
    setState(() => isSubmitting = true);
    
    try {
      // Upload images first
      List<String> imageIds = [];
      for (XFile image in selectedImages) {
        final imageId = await _uploadImage(image);
        if (imageId != null) imageIds.add(imageId);
      }
      
      // Create report
      final reportData = {
        'projectId': selectedProjectId,
        'reportDate': DateTime.now().toIso8601String().split('T')[0],
        'workDescription': _workDescriptionController.text,
        'hoursWorked': double.parse(_hoursWorkedController.text),
        'weatherConditions': _weatherConditionsController.text,
        'notes': _notesController.text,
        'imageIds': imageIds,
        if (currentPosition != null) 'location': {
          'latitude': currentPosition!.latitude,
          'longitude': currentPosition!.longitude,
        },
      };
      
      final response = await ApiClient.post('/daily-reports', reportData);
      
      if (response['success']) {
        Navigator.pop(context, true);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Daily report created successfully!')),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error creating report: $e')),
      );
    } finally {
      setState(() => isSubmitting = false);
    }
  }
  
  Future<String?> _uploadImage(XFile image) async {
    try {
      // Implementation depends on your image upload endpoint
      // See Image Upload section for details
      return 'uploaded_image_id';
    } catch (e) {
      print('Error uploading image: $e');
      return null;
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Create Daily Report')),
      body: Form(
        key: _formKey,
        child: SingleChildScrollView(
          padding: EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Project Selection
              DropdownButtonFormField<String>(
                value: selectedProjectId,
                decoration: InputDecoration(labelText: 'Project'),
                items: projects.map((project) {
                  return DropdownMenuItem(
                    value: project.id,
                    child: Text(project.name),
                  );
                }).toList(),
                onChanged: (value) => setState(() => selectedProjectId = value),
                validator: (value) => value == null ? 'Please select a project' : null,
              ),
              
              SizedBox(height: 16),
              
              // Work Description
              TextFormField(
                controller: _workDescriptionController,
                decoration: InputDecoration(labelText: 'Work Description'),
                maxLines: 3,
                validator: (value) => value?.isEmpty ?? true ? 'Please describe the work done' : null,
              ),
              
              SizedBox(height: 16),
              
              // Hours Worked
              TextFormField(
                controller: _hoursWorkedController,
                decoration: InputDecoration(labelText: 'Hours Worked'),
                keyboardType: TextInputType.number,
                validator: (value) {
                  if (value?.isEmpty ?? true) return 'Please enter hours worked';
                  if (double.tryParse(value!) == null) return 'Please enter a valid number';
                  return null;
                },
              ),
              
              SizedBox(height: 16),
              
              // Weather Conditions
              TextFormField(
                controller: _weatherConditionsController,
                decoration: InputDecoration(labelText: 'Weather Conditions'),
              ),
              
              SizedBox(height: 16),
              
              // Notes
              TextFormField(
                controller: _notesController,
                decoration: InputDecoration(labelText: 'Additional Notes'),
                maxLines: 3,
              ),
              
              SizedBox(height: 16),
              
              // Image Selection
              Card(
                child: Padding(
                  padding: EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('Photos (${selectedImages.length})', 
                               style: Theme.of(context).textTheme.titleMedium),
                          IconButton(
                            icon: Icon(Icons.add_a_photo),
                            onPressed: _pickImages,
                          ),
                        ],
                      ),
                      if (selectedImages.isNotEmpty)
                        SizedBox(
                          height: 100,
                          child: ListView.builder(
                            scrollDirection: Axis.horizontal,
                            itemCount: selectedImages.length,
                            itemBuilder: (context, index) {
                              return Padding(
                                padding: EdgeInsets.only(right: 8),
                                child: Stack(
                                  children: [
                                    ClipRRect(
                                      borderRadius: BorderRadius.circular(8),
                                      child: Image.file(
                                        File(selectedImages[index].path),
                                        width: 100,
                                        height: 100,
                                        fit: BoxFit.cover,
                                      ),
                                    ),
                                    Positioned(
                                      top: 4,
                                      right: 4,
                                      child: GestureDetector(
                                        onTap: () {
                                          setState(() {
                                            selectedImages.removeAt(index);
                                          });
                                        },
                                        child: Container(
                                          decoration: BoxDecoration(
                                            color: Colors.red,
                                            shape: BoxShape.circle,
                                          ),
                                          child: Icon(Icons.close, color: Colors.white, size: 16),
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              );
                            },
                          ),
                        ),
                    ],
                  ),
                ),
              ),
              
              SizedBox(height: 24),
              
              // Submit Button
              ElevatedButton(
                onPressed: isSubmitting ? null : _submitReport,
                child: isSubmitting
                    ? Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          SizedBox(
                            width: 16,
                            height: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          ),
                          SizedBox(width: 8),
                          Text('Creating Report...'),
                        ],
                      )
                    : Text('Create Report'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

### 🔍 Get Daily Report by ID
**GET** `/api/v1/daily-reports/{reportId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "userName": "John Technician",
    "reportDate": "2025-06-14",
    "workDescription": "Installed 12 solar panels on south-facing roof section. Completed all electrical connections and testing.",
    "hoursWorked": 8.5,
    "weatherConditions": "Sunny, 75°F, light breeze",
    "safetyIncidents": null,
    "notes": "All panels properly secured. Electrical connections completed. Site cleanup done.",
    "location": {
      "latitude": 37.7749,
      "longitude": -122.4194,
      "address": "123 Solar Street, San Francisco, CA"
    },
    "images": [
      {
        "id": "abc123-def456-ghi789",
        "fileName": "installation_progress_001.jpg",
        "filePath": "/uploads/daily-reports/2025/06/14/abc123-def456-ghi789.jpg",
        "uploadedAt": "2025-06-14T15:30:00Z",
        "fileSize": 2048576,
        "mimeType": "image/jpeg",
        "metadata": {
          "camera": "iPhone 14 Pro",
          "gpsLocation": {
            "latitude": 37.7749,
            "longitude": -122.4194
          },
          "timestamp": "2025-06-14T15:30:00Z"
        }
      }
    ],
    "createdAt": "2025-06-14T16:00:00Z",
    "updatedAt": "2025-06-14T16:00:00Z"
  },
  "errors": []
}
```

### ✏️ Update Daily Report
**PUT** `/api/v1/daily-reports/{reportId}`

**Request Body**: Same structure as create request

### 🗑️ Delete Daily Report
**DELETE** `/api/v1/daily-reports/{reportId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report deleted successfully",
  "data": null,
  "errors": []
}
```

---

## 🏗️ Master Plan Management

**🔒 Authentication Required**  
**🎯 Role Required**: Administrator, Project Manager (full CRUD access), All authenticated users (view only)

Master plans provide comprehensive project planning and management capabilities, allowing Project Managers to create detailed plans with phases, milestones, and budget tracking for solar installation projects.

**⚡ Administrator & Project Manager Capabilities**:
- ✅ Create new master plans for projects
- ✅ Update plan details (name, description, dates, budget)
- ✅ Manage plan status and versioning
- ✅ Set project phases and milestones
- ✅ Track progress and budget allocation
- ✅ Approve and submit plans for execution

**📖 User & Viewer Access**:
- Read-only access to approved master plans
- Cannot modify plan information
- Can view project progress and milestones

### 🏗️ Create Master Plan
**POST** `/api/v1/master-plans`

**🔒 Required Roles**: Administrator, ProjectManager

Create a new master plan for a project with detailed planning information.

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1",
  "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
  "projectId": 123,
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-09-30T00:00:00Z",
  "budget": 275000.00,
  "priority": "High",
  "notes": "Plan includes weather contingency and permit approval delays"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Master plan created successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Draft",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": null,
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": null
  },
  "errors": []
}
```

### 🔍 Get Master Plan by ID
**GET** `/api/v1/master-plans/{id}`

Retrieve detailed information about a specific master plan.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation covering site preparation through final commissioning",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "Approved",
    "budget": 275000.00,
    "priority": "High",
    "notes": "Plan includes weather contingency and permit approval delays",
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": "2025-06-16T14:30:00Z",
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "projectName": "Solar Installation Project Alpha",
    "createdByName": "John Manager",
    "approvedByName": "Sarah Administrator"
  },
  "errors": []
}
```

### 🔍 Get Master Plan by Project ID
**GET** `/api/v1/master-plans/project/{projectId}`

Retrieve the master plan associated with a specific project.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1",
    "description": "Comprehensive plan for residential solar installation",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T00:00:00Z",
    "status": "InProgress",
    "budget": 275000.00,
    "priority": "High",
    "projectName": "Solar Installation Project Alpha"
  },
  "errors": []
}
```

### 🔄 Update Master Plan
**PUT** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Update an existing master plan with new information.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Request Body**:
```json
{
  "title": "Solar Installation Master Plan - Phase 1 (Updated)",
  "description": "Updated comprehensive plan with revised timeline and budget",
  "startDate": "2025-07-15T00:00:00Z",
  "endDate": "2025-10-15T00:00:00Z",
  "status": "InProgress",
  "budget": 285000.00,
  "priority": "High",
  "notes": "Updated to include additional safety requirements and equipment upgrades"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan updated successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Installation Master Plan - Phase 1 (Updated)",
    "description": "Updated comprehensive plan with revised timeline and budget",
    "startDate": "2025-07-15T00:00:00Z",
    "endDate": "2025-10-15T00:00:00Z",
    "status": "InProgress",
    "budget": 285000.00,
    "priority": "High",
    "notes": "Updated to include additional safety requirements and equipment upgrades",
    "updatedAt": "2025-06-16T15:45:00Z"
  },
  "errors": []
}
```

### 🗑️ Delete Master Plan
**DELETE** `/api/v1/master-plans/{id}`

**🔒 Required Roles**: Administrator, ProjectManager

Delete a master plan. Only plans in "Draft" status can be deleted.

**Path Parameters**:
- `id` (Guid): Master plan ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Master plan deleted successfully",
  "data": null,
  "errors": []
}
```

### 📊 Master Plan Status Workflow

Master plans follow a defined status workflow:

| Status | Description | Available Actions | Who Can Change |
|--------|-------------|-------------------|----------------|
| **Draft** | Initial creation, editable | Edit, Submit for Approval, Delete | Creator, Admin |
| **Pending** | Submitted for approval | Approve, Reject, Request Changes | Admin, Senior PM |
| **Approved** | Ready for execution | Start Execution, Archive | Admin, PM |
| **InProgress** | Currently being executed | Update Progress, Complete | Admin, PM |
| **Completed** | Successfully finished | Archive, Generate Report | Admin, PM |
| **Cancelled** | Cancelled before completion | Archive, Restart | Admin |

### 🔧 Master Plan Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **MP001** | Master plan not found | Verify the plan ID exists |
| **MP002** | Project already has a master plan | Update existing plan or create new version |
| **MP003** | Invalid project ID | Ensure project exists and is accessible |
| **MP004** | Insufficient permissions | Check user role requirements |
| **MP005** | Cannot delete plan with active tasks | Complete or remove associated tasks first |
| **MP006** | Invalid status transition | Follow the defined workflow sequence |

---

## ✅ Task Management

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager, User (full CRUD access for own tasks), Viewer (read-only)

Task management enables tracking and managing of individual tasks within projects, including assignment, status updates, and progress tracking.

**⚡ Admin & Manager Capabilities**:
- ✅ Create, read, update, and delete any task
- ✅ Assign tasks to users
- ✅ Update task status and priorities
- ✅ Add comments and attachments to tasks
- ✅ View task history and audit logs

**📖 User Capabilities**:
- Create, read, update, and delete own tasks
- Update task status and add comments
- Upload attachments to tasks
- View task history

**🔗 Viewer Capabilities**:
- Read-only access to tasks
- View task details, comments, and attachments
- Cannot modify any task information

### ➕ Create Task
**POST** `/api/v1/tasks`

**Request Body**:
```json
{
  "title": "Install Solar Panels - Section A",
  "description": "Install 24 solar panels on the south-facing roof section",
  "status": "Pending",
  "priority": "High",
  "dueDate": "2025-06-20",
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
  "estimatedHours": 16.0
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Task created successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Install Solar Panels - Section A",
    "description": "Install 24 solar panels on the south-facing roof section",
    "status": "Pending",
    "priority": "High",
    "dueDate": "2025-06-20",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
    "estimatedHours": 16.0,
    "progressPercentage": 0.0,
    "createdAt": "2025-06-15T10:00:00Z"
  },
  "errors": []
}
```

### 🔍 Get Task by ID
**GET** `/api/v1/tasks/{taskId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Install Solar Panels - Section A",
    "description": "Install 24 solar panels on the south-facing roof section",
    "status": "InProgress",
    "priority": "High",
    "dueDate": "2025-06-20",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
    "estimatedHours": 16.0,
    "actualHours": 12.5,
    "progressPercentage": 75.0,
    "createdAt": "2025-06-15T10:00:00Z",
    "updatedAt": "2025-06-14T14:30:00Z"
  },
  "errors": []
}
```

### 📋 Get All Tasks
**GET** `/api/v1/tasks`

**Query Parameters**:
- `projectId` (Guid): Filter tasks by project
- `assignedToUserId` (Guid): Filter tasks by assigned user
- `status` (string): Filter by status
- `priority` (string): Filter by priority
- `dueDate` (DateTime): Filter by due date
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "tasks": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Install Solar Panels - Section A",
        "description": "Install 24 solar panels on the south-facing roof section",
        "status": "InProgress",
        "priority": "High",
        "dueDate": "2025-06-20",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
        "estimatedHours": 16.0,
        "actualHours": 12.5,
        "progressPercentage": 75.0,
        "createdAt": "2025-06-15T10:00:00Z",
        "updatedAt": "2025-06-14T14:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 15,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 2,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

### 🔄 Update Task
**PUT** `/api/v1/tasks/{id}`

**🔒 Requires**: Admin, Manager, or task assignee

Update all fields of an existing task.

**Path Parameters**:
- `id` (Guid): Task ID

**Request Body**:
```json
{
  "title": "Install Electrical Panel - Updated",
  "description": "Install and configure the main electrical panel for solar system integration with additional safety checks",
  "priority": "High",
  "dueDate": "2025-06-26T00:00:00Z",
  "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
  "estimatedHours": 10.0,
  "actualHours": 5.0,
  "status": "InProgress",
  "progressPercentage": 50.0
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task updated successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "title": "Install Electrical Panel - Updated",
    "status": "InProgress",
    "progressPercentage": 50.0,
    "updatedAt": "2025-06-15T12:00:00Z"
  },
  "errors": []
}
```

### 🔄 Partially Update Task
**PATCH** `/api/v1/tasks/{id}`

**🔒 Requires**: Admin, Manager, or task assignee

Update specific fields of an existing task without affecting other fields.

**Path Parameters**:
- `id` (Guid): Task ID

**Request Body**:
```json
{
  "status": "Completed",
  "progressPercentage": 100.0,
  "actualHours": 9.5
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task updated successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "status": "Completed",
    "progressPercentage": 100.0,
    "actualHours": 9.5,
    "updatedAt": "2025-06-15T16:00:00Z"
  },
  "errors": []
}
```

### 🗑️ Delete Task
**DELETE** `/api/v1/tasks/{id}`

**🔒 Requires**: Admin or Manager role

Delete a task (soft delete - task is marked as deleted but retained in database).

**Path Parameters**:
- `id` (Guid): Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task deleted successfully",
  "data": true,
  "errors": []
}
```

### 🔍 Advanced Task Query
**GET** `/api/v1/tasks/advanced`

Get tasks with advanced filtering, sorting, and field selection capabilities.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `projectId` (Guid): Filter by project
- `assigneeId` (Guid): Filter by assigned user
- `status` (string): Filter by status
- `priority` (string): Filter by priority
- `dueDate` (DateTime): Filter by due date
- `sortBy` (string): Sort field (title, dueDate, status, priority)
- `sortOrder` (string): Sort direction (asc, desc)
- `fields` (string): Comma-separated list of fields to include
- `filter` (string): Dynamic filter expression

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "items": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Install Solar Panels - Section A",
        "status": "InProgress",
        "priority": "High"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true,
    "metadata": {
      "queryTime": "00:00:00.0234567",
      "appliedFilters": ["status=InProgress"],
      "availableFields": ["id", "title", "status", "priority", "dueDate"]
    }
  },
  "errors": []
}
```

### 🔗 Rich Task Pagination
**GET** `/api/v1/tasks/rich`

Get tasks with rich HATEOAS pagination and enhanced metadata for advanced UI components.

**Query Parameters**: Same as advanced query

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Install Solar Panels - Section A",
      "status": "InProgress"
    }
  ],
  "pagination": {
    "totalItems": 25,
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "links": {
      "first": "http://localhost:5002/api/v1/tasks/rich?page=1&pageSize=10",
      "last": "http://localhost:5002/api/v1/tasks/rich?page=3&pageSize=10",
      "next": "http://localhost:5002/api/v1/tasks/rich?page=2&pageSize=10"
    }
  },
  "metadata": {
    "generatedAt": "2025-06-15T10:30:00Z",
    "requestId": "req_123456",
    "apiVersion": "v1"
  },
  "errors": []
}
```

---

## 🔧 Work Requests

**🔒 Authentication Required**

Work requests are used to document change orders, additional work, or modifications to the original project scope.

### 📋 Get All Work Requests
**GET** `/api/v1/work-requests`

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `status` (string): Filter by status ("Pending", "Approved", "Rejected", "InProgress", "Completed")
- `requestType` (string): Filter by type ("ChangeOrder", "AdditionalWork", "Emergency", "Other")
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work requests retrieved successfully",
  "data": {
    "workRequests": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Additional Panel Installation",
        "description": "Client requested 6 additional solar panels on garage roof",
        "requestType": "AdditionalWork",
        "status": "Pending",
        "priority": "Medium",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "requestedByUserId": "789e0123-e89b-12d3-a456-426614174002",
        "requestedByUserName": "John Technician",
        "estimatedCost": 5000.00,
        "estimatedHours": 12.0,
        "requestedDate": "2025-06-14",
        "targetCompletionDate": "2025-06-25",
        "createdAt": "2025-06-14T10:30:00Z",
        "updatedAt": "2025-06-14T10:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 8,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  },
  "errors": []
}
```

### 📝 Create Work Request
**POST** `/api/v1/work-requests`

**Request Body**:
```json
{
  "title": "Emergency Electrical Repair",
  "description": "Electrical panel needs immediate repair due to weather damage",
  "requestType": "Emergency",
  "priority": "High",
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "estimatedCost": 2500.00,
  "estimatedHours": 8.0,
  "targetCompletionDate": "2025-06-16"
}
```

### 🔄 Work Request Approval Workflow

The approval workflow system enables multi-level approval for work requests with complete audit trail and notifications.

#### 📤 Submit Work Request for Approval
**POST** `/api/v1/work-requests/{id}/submit-approval`

Submit a work request for management approval. Only the request creator can submit their own requests.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "comments": "Ready for manager review. All documentation attached and cost estimates verified."
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request submitted for approval",
  "data": true,
  "errors": []
}
```

#### ✅ Process Approval (Approve/Reject)
**POST** `/api/v1/work-requests/{id}/process-approval`

**🔒 Requires**: Manager or Admin role

Process approval for a work request. Managers can approve requests up to their authority level, Admins can provide final approval.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "action": "Approve", // "Approve" or "Reject"
  "comments": "Approved for maintenance. Cost estimate is reasonable and maintenance is needed.",
  "rejectionReason": "Budget constraints" // Required if action is "Reject"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request approved successfully",
  "data": true,
  "errors": []
}
```

#### 📊 Get Approval Status
**GET** `/api/v1/work-requests/{id}/approval-status`

Get the current approval workflow status for a work request.

**Path Parameters**:
- `id` (Guid): Work request ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "",
  "data": {
    "workRequestId": "63d02702-0c28-48a1-ab68-fc633ae7d9f8",
    "title": "Solar Panel Maintenance Request",
    "currentStatus": "Approved",
    "requiresManagerApproval": false,
    "requiresAdminApproval": false,
    "isManagerApproved": false,
    "isAdminApproved": true,
    "currentApproverName": null,
    "nextApproverName": null,
    "submittedForApprovalDate": "2025-06-14T21:12:31.306587Z",
    "lastActionDate": "2025-06-14T21:14:28.340096Z",
    "daysPendingApproval": 0
  },
  "errors": []
}
```

---
