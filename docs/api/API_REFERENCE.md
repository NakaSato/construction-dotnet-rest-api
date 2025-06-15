# 🌞 Solar Projects REST API Reference for Flutter

## 📱 Flutter Mobile App Integration Guide

This comprehensive API reference is specifically designed for **Flutter mobile app development**. The Solar Projects Management REST API is built with .NET 9.0 and provides complete functionality for managing solar installation projects, field operations, and team collaboration.

**Base URLs**:
- 🏠 **Local Development**: `http://localhost:5002`
- 🌐 **Production**: `https://solar-projects-api-dev.azurewebsites.net`
- 📱 **Mobile Testing**: `http://10.0.2.2:5002` (Android Emulator)

**API Version**: `v1`  
**Authentication**: JWT Bearer Token (except public endpoints)  
**Content-Type**: `application/json`  
**Date Format**: ISO 8601 (`2025-06-14T10:30:00Z`)

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

- [ ] ✅ Test health endpoint: `GET /health`
- [ ] 🔐 Implement authentication: `POST /api/v1/auth/login`
- [ ] 👤 Get user profile after login
- [ ] 📊 Fetch daily reports: `GET /api/v1/daily-reports`
- [ ] 📱 Test image upload: `POST /api/v1/images/upload`
- [ ] 🔄 Implement token refresh logic
- [ ] ⚠️ Handle error responses and rate limiting
- [ ] 💾 Set up local storage for offline support

---

## 📖 Complete API Reference

### 🔗 Quick Navigation
- [🔐 Authentication & User Management](#-authentication--user-management)
- [❤️ Health & System Status](#️-health--system-status)  
- [👥 User Management](#-user-management)
- [📋 Project Management](#-project-management)
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

**Role IDs & Permissions**:
| ID | Role | Permissions | Mobile Use Case |
|---|---|---|---|
| `1` | **Admin** | Full system access | Management app |
| `2` | **Manager** | Project oversight | Supervisor app |
| `3` | **User** | Field operations | Technician app |
| `4` | **Viewer** | Read-only access | Client/Reporting app |

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

## 🖼️ Image Upload

**🔒 Authentication Required**

Supports multiple image formats (JPEG, PNG, GIF, WebP) with automatic GPS metadata extraction and device information capture.

### 📷 Upload Image
**POST** `/api/v1/images/upload`

**Content-Type**: `multipart/form-data`

**Form Data**:
- `file` (File): Image file to upload
- `category` (string): Image category (optional)
- `description` (string): Image description (optional)

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Image uploaded successfully",
  "data": {
    "id": "abc123-def456-ghi789",
    "fileName": "installation_progress_001.jpg",
    "originalFileName": "IMG_20250614_153000.jpg",
    "filePath": "/uploads/2025/06/14/abc123-def456-ghi789.jpg",
    "fileSize": 2048576,
    "mimeType": "image/jpeg",
    "uploadedAt": "2025-06-14T15:30:00Z",
    "metadata": {
      "width": 4032,
      "height": 3024,
      "camera": "iPhone 14 Pro",
      "gpsLocation": {
        "latitude": 37.7749,
        "longitude": -122.4194
      },
      "timestamp": "2025-06-14T15:30:00Z"
    }
  },
  "errors": []
}
```

**Flutter Example - Image Upload with GPS**:
```dart
class ImageUploadService {
  static Future<String?> uploadImage(XFile imageFile, {String? category, String? description}) async {
    try {
      // Get current location
      Position? position;
      try {
        position = await Geolocator.getCurrentPosition();
      } catch (e) {
        print('Could not get GPS location: $e');
      }
      
      var request = http.MultipartRequest(
        'POST',
        Uri.parse('${ApiClient.apiBase}/images/upload'),
      );
      
      // Add headers
      final headers = await ApiClient.getAuthHeaders();
      request.headers.addAll(headers);
      
      // Add file
      request.files.add(await http.MultipartFile.fromPath(
        'file',
        imageFile.path,
        contentType: MediaType('image', 'jpeg'),
      ));
      
      // Add metadata
      if (category != null) {
        request.fields['category'] = category;
      }
      if (description != null) {
        request.fields['description'] = description;
      }
      
      // Add GPS data if available
      if (position != null) {
        request.fields['latitude'] = position.latitude.toString();
        request.fields['longitude'] = position.longitude.toString();
      }
      
      // Add device info
      final deviceInfo = await _getDeviceInfo();
      request.fields['deviceInfo'] = json.encode(deviceInfo);
      
      final response = await request.send();
      final responseData = await response.stream.bytesToString();
      final data = json.decode(responseData);
      
      if (response.statusCode == 201 && data['success']) {
        return data['data']['id'];
      } else {
        throw Exception(data['message'] ?? 'Upload failed');
      }
    } catch (e) {
      print('Error uploading image: $e');
      return null;
    }
  }
  
  static Future<Map<String, dynamic>> _getDeviceInfo() async {
    final deviceInfo = DeviceInfoPlugin();
    
    if (Platform.isAndroid) {
      final androidInfo = await deviceInfo.androidInfo;
      return {
        'platform': 'Android',
        'model': androidInfo.model,
        'manufacturer': androidInfo.manufacturer,
        'version': androidInfo.version.release,
      };
    } else if (Platform.isIOS) {
      final iosInfo = await deviceInfo.iosInfo;
      return {
        'platform': 'iOS',
        'model': iosInfo.model,
        'systemName': iosInfo.systemName,
        'systemVersion': iosInfo.systemVersion,
      };
    }
    
    return {'platform': 'Unknown'};
  }
}

// Usage in camera capture
class CameraScreen extends StatefulWidget {
  @override
  _CameraScreenState createState() => _CameraScreenState();
}

class _CameraScreenState extends State<CameraScreen> {
  final ImagePicker _picker = ImagePicker();
  
  Future<void> _captureImage() async {
    try {
      final XFile? image = await _picker.pickImage(
        source: ImageSource.camera,
        imageQuality: 85, // Compress to reduce file size
      );
      
      if (image != null) {
        // Show loading indicator
        showDialog(
          context: context,
          barrierDismissible: false,
          builder: (context) => AlertDialog(
            content: Row(
              children: [
                CircularProgressIndicator(),
                SizedBox(width: 16),
                Text('Uploading image...'),
              ],
            ),
          ),
        );
        
        // Upload image
        final imageId = await ImageUploadService.uploadImage(
          image,
          category: 'daily_report',
          description: 'Installation progress photo',
        );
        
        Navigator.pop(context); // Close loading dialog
        
        if (imageId != null) {
          // Image uploaded successfully
          Navigator.pop(context, imageId);
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Failed to upload image')),
          );
        }
      }
    } catch (e) {
      Navigator.pop(context); // Close loading dialog if open
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error capturing image: $e')),
      );
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Capture Photo')),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.camera_alt, size: 100, color: Colors.grey),
            SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: _captureImage,
              icon: Icon(Icons.camera),
              label: Text('Take Photo'),
            ),
            SizedBox(height: 16),
            ElevatedButton.icon(
              onPressed: () async {
                final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
                if (image != null) {
                  final imageId = await ImageUploadService.uploadImage(image);
                  if (imageId != null) {
                    Navigator.pop(context, imageId);
                  }
                }
              },
              icon: Icon(Icons.photo_library),
              label: Text('Choose from Gallery'),
            ),
          ],
        ),
      ),
    );
  }
}
```

### 🖼️ Get Image Metadata
**GET** `/api/v1/images/{imageId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Image metadata retrieved successfully",
  "data": {
    "id": "abc123-def456-ghi789",
    "fileName": "installation_progress_001.jpg",
    "originalFileName": "IMG_20250614_153000.jpg",
    "filePath": "/uploads/2025/06/14/abc123-def456-ghi789.jpg",
    "fileSize": 2048576,
    "mimeType": "image/jpeg",
    "uploadedAt": "2025-06-14T15:30:00Z",
    "metadata": {
      "width": 4032,
      "height": 3024,
      "camera": "iPhone 14 Pro",
      "gpsLocation": {
        "latitude": 37.7749,
        "longitude": -122.4194
      },
      "timestamp": "2025-06-14T15:30:00Z"
    }
  },
  "errors": []
}
```

### 🗑️ Delete Image
**DELETE** `/api/v1/images/{imageId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Image deleted successfully",
  "data": null,
  "errors": []
}
```

**Flutter Example - Image Display with Caching**:
```dart
class CachedNetworkImageWidget extends StatelessWidget {
  final String imageId;
  final double? width;
  final double? height;
  final BoxFit fit;
  
  const CachedNetworkImageWidget({
    required this.imageId,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
  });
  
  @override
  Widget build(BuildContext context) {
    return CachedNetworkImage(
      imageUrl: '${ApiClient.apiBase}/images/$imageId',
      httpHeaders: await ApiClient.getAuthHeaders(),
      width: width,
      height: height,
      fit: fit,
      placeholder: (context, url) => Container(
        width: width,
        height: height,
        color: Colors.grey[300],
        child: Center(child: CircularProgressIndicator()),
      ),
      errorWidget: (context, url, error) => Container(
        width: width,
        height: height,
        color: Colors.grey[300],
        child: Icon(Icons.error, color: Colors.red),
      ),
    );
  }
}
```

---
## 📋 Project Management

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (create/edit), All users (view)

### 📊 Get All Projects
**GET** `/api/v1/projects`

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status ("Active", "Completed", "OnHold", "Cancelled")
- `search` (string): Search in project name or description

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "projects": [
      {
        "id": "456e7890-e89b-12d3-a456-426614174001",
        "name": "Solar Installation Project Alpha",
        "description": "Residential solar panel installation for 50-home subdivision",
        "status": "Active",
        "startDate": "2025-06-01",
        "endDate": "2025-08-15",
        "totalTasks": 12,
        "completedTasks": 8,
        "progressPercentage": 66.7,
        "location": "Sunnydale Subdivision, CA",
        "budget": 250000.00,
        "createdAt": "2025-05-15T10:00:00Z",
        "updatedAt": "2025-06-14T16:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 25,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 3,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

**Flutter Example - Projects Dashboard**:
```dart
class ProjectsDashboard extends StatefulWidget {
  @override
  _ProjectsDashboardState createState() => _ProjectsDashboardState();
}

class _ProjectsDashboardState extends State<ProjectsDashboard> {
  List<Project> projects = [];
  bool isLoading = true;
  String selectedStatus = 'All';
  
  @override
  void initState() {
    super.initState();
    _loadProjects();
  }
  
  Future<void> _loadProjects() async {
    setState(() => isLoading = true);
    
    try {
      String endpoint = '/projects?pageSize=50';
      if (selectedStatus != 'All') {
        endpoint += '&status=$selectedStatus';
      }
      
      final response = await ApiClient.get(endpoint);
      
      if (response['success']) {
        setState(() {
          projects = (response['data']['projects'] as List)
              .map((json) => Project.fromJson(json))
              .toList();
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading projects: $e')),
      );
    } finally {
      setState(() => isLoading = false);
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Projects'),
        actions: [
          PopupMenuButton<String>(
            initialValue: selectedStatus,
            onSelected: (status) {
              setState(() => selectedStatus = status);
              _loadProjects();
            },
            itemBuilder: (context) => [
              PopupMenuItem(value: 'All', child: Text('All Projects')),
              PopupMenuItem(value: 'Active', child: Text('Active')),
              PopupMenuItem(value: 'Completed', child: Text('Completed')),
              PopupMenuItem(value: 'OnHold', child: Text('On Hold')),
              PopupMenuItem(value: 'Cancelled', child: Text('Cancelled')),
            ],
          ),
        ],
      ),
      body: isLoading
          ? Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadProjects,
              child: GridView.builder(
                padding: EdgeInsets.all(16),
                gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  crossAxisSpacing: 16,
                  mainAxisSpacing: 16,
                  childAspectRatio: 0.8,
                ),
                itemCount: projects.length,
                itemBuilder: (context, index) {
                  final project = projects[index];
                  return ProjectCard(
                    project: project,
                    onTap: () => Navigator.pushNamed(
                      context,
                      '/project-details',
                      arguments: project.id,
                    ),
                  );
                },
              ),
            ),
      floatingActionButton: _canCreateProject()
          ? FloatingActionButton(
              onPressed: () => Navigator.pushNamed(context, '/create-project'),
              child: Icon(Icons.add),
            )
          : null,
    );
  }
  
  bool _canCreateProject() {
    // Check user role from auth provider
    return ['Admin', 'Manager'].contains(context.read<AuthProvider>().user?.roleName);
  }
}

class ProjectCard extends StatelessWidget {
  final Project project;
  final VoidCallback onTap;
  
  const ProjectCard({required this.project, required this.onTap});
  
  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 4,
      child: InkWell(
        onTap: onTap,
        child: Padding(
          padding: EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      project.name,
                      style: Theme.of(context).textTheme.titleMedium,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  _StatusChip(status: project.status),
                ],
              ),
              Spacer(),
              Text(
                'Progress',
                style: Theme.of(context).textTheme.bodySmall,
              ),
              SizedBox(height: 4),
              LinearProgressIndicator(
                value: project.progressPercentage / 100,
                backgroundColor: Colors.grey[300],
                valueColor: AlwaysStoppedAnimation<Color>(_getProgressColor(project.progressPercentage)),
              ),
              SizedBox(height: 4),
              Text(
                '${project.completedTasks}/${project.totalTasks} tasks • ${project.progressPercentage.toStringAsFixed(1)}%',
                style: Theme.of(context).textTheme.bodySmall,
              ),
              SizedBox(height: 8),
              Row(
                children: [
                  Icon(Icons.location_on, size: 16, color: Colors.grey),
                  SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      project.location,
                      style: Theme.of(context).textTheme.bodySmall,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
  
  Color _getProgressColor(double progress) {
    if (progress >= 80) return Colors.green;
    if (progress >= 50) return Colors.orange;
    return Colors.red;
  }
}

class _StatusChip extends StatelessWidget {
  final String status;
  
  const _StatusChip({required this.status});
  
  @override
  Widget build(BuildContext context) {
    Color color;
    switch (status) {
      case 'Active':
        color = Colors.green;
        break;
      case 'Completed':
        color = Colors.blue;
        break;
      case 'OnHold':
        color = Colors.orange;
        break;
      case 'Cancelled':
        color = Colors.red;
        break;
      default:
        color = Colors.grey;
    }
    
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Text(
        status,
        style: TextStyle(
          color: color,
          fontSize: 12,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }
}
```

### 🔍 Get Project by ID
**GET** `/api/v1/projects/{projectId}`

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project retrieved successfully",
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "name": "Solar Installation Project Alpha",
    "description": "Residential solar panel installation for 50-home subdivision with 3MW total capacity",
    "status": "Active",
    "startDate": "2025-06-01",
    "endDate": "2025-08-15",
    "location": "Sunnydale Subdivision, California",
    "budget": 250000.00,
    "actualCost": 180000.00,
    "totalTasks": 12,
    "completedTasks": 8,
    "progressPercentage": 66.7,
    "tasks": [
      {
        "id": "task123",
        "title": "Site Preparation",
        "status": "Completed",
        "dueDate": "2025-06-10"
      }
    ],
    "recentReports": [
      {
        "id": "report123",
        "reportDate": "2025-06-14",
        "userName": "John Tech",
        "hoursWorked": 8.5
      }
    ],
    "createdAt": "2025-05-15T10:00:00Z",
    "updatedAt": "2025-06-14T16:30:00Z"
  },
  "errors": []
}
```

### 📝 Create Project
**POST** `/api/v1/projects`  
**🎯 Role Required**: Administrator, ProjectManager

**Request Body**:
```json
{
  "projectName": "New Solar Installation Project",
  "address": "456 Oak Ave, Another City, State 67890",
  "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-09-30T00:00:00Z",
  "projectManagerId": "123e4567-e89b-12d3-a456-426614174000",
  "team": "E2",
  "connectionType": "LV",
  "connectionNotes": "เสาหม้อแปลงไม่มีพื้นที่ในการทำ Zero Export แรงสูง",
  "totalCapacityKw": 171.0,
  "pvModuleCount": 300,
  "equipmentDetails": {
    "inverter125kw": 1,
    "inverter80kw": 0,
    "inverter60kw": 1,
    "inverter40kw": 0
  },
  "ftsValue": 6,
  "revenueValue": 1,
  "pqmValue": 0,
  "locationCoordinates": {
    "latitude": 14.72746,
    "longitude": 102.16276
  }
}
```

**Success Response (201 Created)**: (Same structure as Get Project by ID)

### ✏️ Update Project
**PUT** `/api/v1/projects/{projectId}`  
**🎯 Role Required**: Admin, Manager

### 🗑️ Delete Project
**DELETE** `/api/v1/projects/{projectId}`  
**🎯 Role Required**: Admin only

---

## ✅ Task Management

**🔒 Authentication Required**

### 📋 Get All Tasks
**GET** `/api/v1/tasks`

**Query Parameters**:
- `projectId` (Guid): Filter tasks by project
- `assignedToUserId` (Guid): Filter tasks by assigned user
- `status` (string): Filter by status ("Pending", "InProgress", "Completed", "Cancelled")
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
        "projectName": "Solar Installation Project Alpha",
        "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
        "assignedToUserName": "John Technician",
        "estimatedHours": 16.0,
        "actualHours": 12.5,
        "progressPercentage": 75.0,
        "createdAt": "2025-06-01T10:00:00Z",
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

### 🔍 Get Task by ID
**GET** `/api/v1/tasks/{id}`

Get detailed information about a specific task.

**Path Parameters**:
- `id` (Guid): Task ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Install Solar Panels - Section A",
    "description": "Install 24 solar panels on the south-facing roof section with proper mounting and electrical connections",
    "status": "InProgress",
    "priority": "High",
    "dueDate": "2025-06-20T00:00:00Z",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
    "assignedToUserName": "John Technician",
    "estimatedHours": 16.0,
    "actualHours": 12.5,
    "progressPercentage": 75.0,
    "createdAt": "2025-06-01T10:00:00Z",
    "updatedAt": "2025-06-14T14:30:00Z",
    "createdByUserId": "456e7890-e89b-12d3-a456-426614174001",
    "createdByUserName": "Project Manager"
  },
  "errors": []
}
```

### 📋 Get Project Tasks
**GET** `/api/v1/tasks/project/{projectId}`

Get all tasks for a specific project.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project tasks retrieved successfully",
  "data": {
    "items": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Install Solar Panels - Section A",
        "status": "InProgress",
        "priority": "High",
        "dueDate": "2025-06-20T00:00:00Z",
        "assignedToUserName": "John Technician",
        "progressPercentage": 75.0
      }
    ],
    "totalCount": 8,
    "pageNumber": 1,
    "pageSize": 50
  },
  "errors": []
}
```

### ➕ Create Task
**POST** `/api/v1/tasks/project/{projectId}`

**🔒 Requires**: Admin, Manager, or User role

Create a new task for a specific project.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Request Body**:
```json
{
  "title": "Install Electrical Panel",
  "description": "Install and configure the main electrical panel for solar system integration",
  "priority": "High",
  "dueDate": "2025-06-25T00:00:00Z",
  "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
  "estimatedHours": 8.0,
  "status": "Pending"
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Task created successfully",
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174003",
    "title": "Install Electrical Panel",
    "description": "Install and configure the main electrical panel for solar system integration",
    "status": "Pending",
    "priority": "High",
    "dueDate": "2025-06-25T00:00:00Z",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174002",
    "estimatedHours": 8.0,
    "progressPercentage": 0.0,
    "createdAt": "2025-06-15T10:00:00Z"
  },
  "errors": []
}
```

### ✏️ Update Task
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

#### 📝 Get Approval History
**GET** `/api/v1/work-requests/{id}/approval-history`

Get the complete approval history with audit trail for a work request.

**Path Parameters**:
- `id` (Guid): Work request ID

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "",
  "data": {
    "items": [
      {
        "approvalId": "8b727c28-103a-4091-930a-02e29101090f",
        "workRequestId": "63d02702-0c28-48a1-ab68-fc633ae7d9f8",
        "workRequestTitle": "Solar Panel Maintenance Request",
        "approverId": "fa5d2bd9-d15a-419a-aa46-e1c11f001deb",
        "approverName": "Test Administrator",
        "action": "AdminApproved",
        "level": "Admin",
        "previousStatus": "PendingAdminApproval",
        "newStatus": "Approved",
        "comments": "Final approval granted. Maintenance authorized to proceed.",
        "rejectionReason": "",
        "createdAt": "2025-06-14T21:14:28.340096Z",
        "processedAt": "2025-06-14T21:14:28.340096Z",
        "isActive": true
      }
    ],
    "totalCount": 2,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "errors": []
}
```

#### 📋 Get Pending Approvals
**GET** `/api/v1/work-requests/pending-approval`

**🔒 Requires**: Manager or Admin role

Get work requests pending approval for the current user's role level.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Pending approvals retrieved successfully",
  "data": {
    "items": [
      {
        "workRequestId": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Solar Panel Maintenance Request",
        "description": "Routine maintenance required for solar panels",
        "estimatedCost": 1500.00,
        "priority": "Medium",
        "submittedDate": "2025-06-14T21:12:31.306587Z",
        "submittedByName": "Test User",
        "projectName": "Manager Emergency Repair",
        "daysPending": 0
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "errors": []
}
```

#### 🔺 Escalate Work Request
**POST** `/api/v1/work-requests/{id}/escalate`

**🔒 Requires**: Manager or Admin role

Escalate a work request to the next approval level.

**Path Parameters**:
- `id` (Guid): Work request ID

**Request Body**:
```json
{
  "reason": "Requires executive approval due to budget exceeding department limits",
  "comments": "This request exceeds our standard approval threshold"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Work request escalated successfully",
  "data": true,
  "errors": []
}
```

#### 📊 Get Approval Statistics
**GET** `/api/v1/work-requests/approval-statistics`

**🔒 Requires**: Admin role

Get approval workflow statistics and metrics.

**Query Parameters**:
- `startDate` (DateTime): Filter start date
- `endDate` (DateTime): Filter end date

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Approval statistics retrieved successfully",
  "data": {
    "totalSubmitted": 45,
    "totalApproved": 38,
    "totalRejected": 7,
    "averageApprovalTime": "2.5 days",
    "pendingCount": 12,
    "approvalRate": 84.4,
    "byStatus": {
      "Pending": 12,
      "ManagerApproved": 8,
      "AdminApproved": 38,
      "Rejected": 7
    },
    "byPriority": {
      "High": 15,
      "Medium": 20,
      "Low": 10
    }
  },
  "errors": []
}
```

#### 🚀 Bulk Approval
**POST** `/api/v1/work-requests/bulk-approval`

**🔒 Requires**: Admin role

Process multiple work request approvals in a single operation.

**Request Body**:
```json
{
  "workRequestIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "456e7890-e89b-12d3-a456-426614174001"
  ],
  "action": "Approve", // "Approve" or "Reject"
  "comments": "Bulk approval for routine maintenance requests",
  "rejectionReason": "" // Required if action is "Reject"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Bulk approval processed successfully",
  "data": {
    "processedCount": 2,
    "successCount": 2,
    "failedCount": 0,
    "results": [
      {
        "workRequestId": "123e4567-e89b-12d3-a456-426614174000",
        "success": true,
        "message": "Approved successfully"
      }
    ]
  },
  "errors": []
}
```

#### 📧 Send Approval Reminders
**POST** `/api/v1/work-requests/send-approval-reminders`

**🔒 Requires**: Admin role

Send email reminders for pending approvals.

**Request Body**:
```json
{
  "daysPending": 3, // Send reminders for requests pending more than 3 days
  "includeEscalation": true
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Approval reminders sent successfully",
  "data": {
    "remindersSent": 5,
    "escalationsSent": 2
  },
  "errors": []
}
```

### 📱 Flutter Implementation Example - Approval Workflow

```dart
class ApprovalWorkflowService {
  static const String baseUrl = 'http://localhost:5002/api/v1/work-requests';
  
  // Submit work request for approval
  static Future<bool> submitForApproval(String workRequestId, String comments) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/$workRequestId/submit-approval'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${await TokenManager.getToken()}',
        },
        body: json.encode({
          'comments': comments,
        }),
      );
      
      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        return data['success'] == true;
      }
      return false;
    } catch (e) {
      print('Error submitting for approval: $e');
      return false;
    }
  }
  
  // Process approval (approve/reject)
  static Future<bool> processApproval({
    required String workRequestId,
    required String action, // "Approve" or "Reject"
    required String comments,
    String? rejectionReason,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/$workRequestId/process-approval'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${await TokenManager.getToken()}',
        },
        body: json.encode({
          'action': action,
          'comments': comments,
          if (rejectionReason != null) 'rejectionReason': rejectionReason,
        }),
      );
      
      return response.statusCode == 200;
    } catch (e) {
      print('Error processing approval: $e');
      return false;
    }
  }
  
  // Get approval status
  static Future<Map<String, dynamic>?> getApprovalStatus(String workRequestId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/$workRequestId/approval-status'),
        headers: {
          'Authorization': 'Bearer ${await TokenManager.getToken()}',
        },
      );
      
      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        return data['data'];
      }
      return null;
    } catch (e) {
      print('Error getting approval status: $e');
      return null;
    }
  }
  
  // Get pending approvals
  static Future<List<Map<String, dynamic>>> getPendingApprovals({
    int pageNumber = 1,
    int pageSize = 10,
  }) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/pending-approval?pageNumber=$pageNumber&pageSize=$pageSize'),
        headers: {
          'Authorization': 'Bearer ${await TokenManager.getToken()}',
        },
      );
      
      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        return List<Map<String, dynamic>>.from(data['data']['items']);
      }
      return [];
    } catch (e) {
      print('Error getting pending approvals: $e');
      return [];
    }
  }
  
  // Get work request by ID
  static Future<Map<String, dynamic>?> getWorkRequest(String requestId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/$requestId'),
        headers: {
          'Authorization': 'Bearer ${await TokenManager.getToken()}',
        },
      );
      
      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        return data['data'];
      }
      return null;
    } catch (e) {
      print('Error getting work request: $e');
      return null;
    }
  }
}

// Example Widget for Approval Actions
class ApprovalActionsWidget extends StatelessWidget {
  final String workRequestId;
  final VoidCallback onApprovalChanged;
  
  const ApprovalActionsWidget({
    Key? key,
    required this.workRequestId,
    required this.onApprovalChanged,
  }) : super(key: key);
  
  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: [
        ElevatedButton.icon(
          onPressed: () => _showApprovalDialog(context, 'Approve'),
          icon: Icon(Icons.check_circle, color: Colors.green),
          label: Text('Approve'),
          style: ElevatedButton.styleFrom(backgroundColor: Colors.green[50]),
        ),
        ElevatedButton.icon(
          onPressed: () => _showApprovalDialog(context, 'Reject'),
          icon: Icon(Icons.cancel, color: Colors.red),
          label: Text('Reject'),
          style: ElevatedButton.styleFrom(backgroundColor: Colors.red[50]),
        ),
      ],
    );
  }
  
  void _showApprovalDialog(BuildContext context, String action) {
    final commentsController = TextEditingController();
    final rejectionController = TextEditingController();
    
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('$action Work Request'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: commentsController,
              decoration: InputDecoration(
                labelText: 'Comments',
                hintText: 'Enter your comments...',
              ),
              maxLines: 3,
            ),
            if (action == 'Reject') ...[
              SizedBox(height: 16),
              TextField(
                controller: rejectionController,
                decoration: InputDecoration(
                  labelText: 'Rejection Reason',
                  hintText: 'Specify the reason for rejection...',
                ),
                maxLines: 2,
              ),
            ],
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () async {
              final success = await ApprovalWorkflowService.processApproval(
                workRequestId: workRequestId,
                action: action,
                comments: commentsController.text,
                rejectionReason: action == 'Reject' ? rejectionController.text : null,
              );
              
              Navigator.pop(context);
              
              if (success) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Work request ${action.toLowerCase()}d successfully')),
                );
                onApprovalChanged();
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Failed to $action work request')),
                );
              }
            },
            child: Text(action),
          ),
        ],
      ),
    );
  }
}
```

---

## 📅 Weekly Planning and Reporting

**🔒 Authentication Required**

This section provides endpoints for managing higher-level weekly work requests and summary reports, which complement the granular daily logs.

### 📋 Weekly Work Requests

Weekly Work Requests outline the key objectives, tasks, and resource forecasts for an upcoming week.

**Data Model: WeeklyWorkRequest**
```json
{
  "weeklyRequestId": "guid-for-weekly-request-1",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "weekStartDate": "2025-06-16T00:00:00Z",
  "status": "Approved", // Draft, Submitted, Approved, InProgress, Completed
  "overallGoals": "Complete installation on Roof Section B and begin electrical rough-in for Section C.",
  "keyTasks": [
    "Install 50 panels on Section B",
    "Run conduit for Section C electrical",
    "Complete safety inspections"
  ],
  "resourceForecast": {
    "personnel": "Requires 2 electrical teams and 1 installation team.",
    "majorEquipment": "Crane needed for Monday morning lift.",
    "criticalMaterials": "Ensure all inverters for Section C are on-site."
  },
  "requestedBy": { "userId": "guid-user-pm", "fullName": "Jane Project Manager" },
  "approvedBy": { "userId": "guid-user-admin", "fullName": "Admin User" },
  "createdAt": "2025-06-13T14:00:00Z"
}
```

#### 📝 Create Weekly Work Request
**POST** `/api/v1/projects/{projectId}/weekly-requests`

**🎯 Required Role**: Administrator, ProjectManager

**Request Body:**
```json
{
  "weekStartDate": "2025-06-23T00:00:00Z",
  "overallGoals": "Finalize all electrical work and prepare for final inspection.",
  "keyTasks": [
    "Complete all electrical connections",
    "Conduct pre-commissioning checks",
    "Prepare documentation for inspection"
  ],
  "resourceForecast": {
    "personnel": "Full electrical team required all week.",
    "majorEquipment": "Scissor lift for final checks.",
    "criticalMaterials": "All wiring labels and safety placards."
  }
}
```

**Success Response (201 Created)**: Returns the full WeeklyWorkRequest object

#### 📋 Get Weekly Work Requests for a Project
**GET** `/api/v1/projects/{projectId}/weekly-requests`

**Query Parameters:**
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10)
- `status` (string, optional): Filter by status (e.g., "Approved")

**Success Response (200 OK)**: Returns a paginated list of WeeklyWorkRequest objects

#### 🔍 Get Weekly Work Request by ID
**GET** `/api/v1/weekly-requests/{requestId}`

**Success Response (200 OK)**: Returns a single WeeklyWorkRequest object

#### ✏️ Update Weekly Work Request
**PUT** `/api/v1/weekly-requests/{requestId}`

**🎯 Required Role**: Administrator, ProjectManager (Policy: Can only edit if status is "Draft")

**Success Response (200 OK)**: Returns the updated WeeklyWorkRequest object

#### 📤 Submit/Approve a Weekly Work Request
**POST** `/api/v1/weekly-requests/{requestId}/submit` (Changes status from "Draft" to "Submitted")

**POST** `/api/v1/weekly-requests/{requestId}/approve` (Changes status from "Submitted" to "Approved")

**Success Response (200 OK)**: Returns the updated WeeklyWorkRequest object with the new status

---

### 📊 Weekly Reports

Weekly Reports provide a summary of the week's progress, aggregating information from daily logs and comparing it against the weekly request.

**Data Model: WeeklyReport**
```json
{
  "weeklyReportId": "guid-for-weekly-report-1",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "weekStartDate": "2025-06-16T00:00:00Z",
  "status": "Submitted", // Draft, Submitted, Approved
  "summaryOfProgress": "Successfully completed installation on Roof Section B. Electrical rough-in for Section C is 50% complete. Progress is on track with the weekly request.",
  "aggregatedMetrics": {
    "totalManHours": 450,
    "panelsInstalled": 150,
    "safetyIncidents": 0,
    "delaysReported": 1
  },
  "majorAccomplishments": [
    "Completed Section B installation ahead of schedule",
    "Zero safety incidents this week",
    "Successfully coordinated crane operations"
  ],
  "majorIssues": [
    { 
      "issueId": "guid-issue-1", 
      "description": "Material delivery delay on Wednesday caused a 4-hour work stoppage for one team." 
    }
  ],
  "lookahead": "Next week's focus will be completing Section C electrical and starting inverter commissioning.",
  "submittedBy": { "userId": "guid-user-pm", "fullName": "Jane Project Manager" },
  "createdAt": "2025-06-20T16:00:00Z"
}
```

#### 📝 Create Weekly Report
**POST** `/api/v1/projects/{projectId}/weekly-reports`

**🎯 Required Role**: Administrator, ProjectManager

**Request Body:**
```json
{
  "weekStartDate": "2025-06-16T00:00:00Z",
  // Optional: If summary fields are provided, they will be used.
  // If not, the server can attempt to auto-generate a draft by
  // aggregating daily logs from the specified week.
  "summaryOfProgress": "Initial draft summary...",
  "lookahead": "Planning for next week..."
}
```

**Success Response (201 Created)**: Returns the full WeeklyReport object, potentially auto-populated

#### 📋 Get Weekly Reports for a Project
**GET** `/api/v1/projects/{projectId}/weekly-reports`

**Query Parameters:**
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10)

**Success Response (200 OK)**: Returns a paginated list of WeeklyReport objects

#### 🔍 Get Weekly Report by ID
**GET** `/api/v1/weekly-reports/{reportId}`

**Success Response (200 OK)**: Returns a single WeeklyReport object

#### ✏️ Update Weekly Report
**PUT** `/api/v1/weekly-reports/{reportId}`

**🎯 Required Role**: Administrator, ProjectManager (Policy: Can only edit if status is "Draft")

**Success Response (200 OK)**: Returns the updated WeeklyReport object

#### 📤 Submit/Approve a Weekly Report
**POST** `/api/v1/weekly-reports/{reportId}/submit` (Changes status from "Draft" to "Submitted")

**POST** `/api/v1/weekly-reports/{reportId}/approve` (Changes status from "Submitted" to "Approved")

**Success Response (200 OK)**: Returns the updated WeeklyReport object with the new status

**Flutter Example - Weekly Planning Dashboard:**
```dart
class WeeklyPlanningPage extends StatefulWidget {
  final String projectId;
  
  const WeeklyPlanningPage({Key? key, required this.projectId}) : super(key: key);
  
  @override
  _WeeklyPlanningPageState createState() => _WeeklyPlanningPageState();
}

class _WeeklyPlanningPageState extends State<WeeklyPlanningPage> {
  List<WeeklyWorkRequest> weeklyRequests = [];
  List<WeeklyReport> weeklyReports = [];
  bool isLoading = true;
  
  @override
  void initState() {
    super.initState();
    _loadWeeklyData();
  }
  
  Future<void> _loadWeeklyData() async {
    setState(() => isLoading = true);
    
    try {
      // Load weekly requests
      final requestsResponse = await ApiClient.get(
        '/projects/${widget.projectId}/weekly-requests'
      );
      
      // Load weekly reports
      final reportsResponse = await ApiClient.get(
        '/projects/${widget.projectId}/weekly-reports'
      );
      
      if (requestsResponse['success'] && reportsResponse['success']) {
        setState(() {
          weeklyRequests = (requestsResponse['data']['items'] as List)
              .map((json) => WeeklyWorkRequest.fromJson(json))
              .toList();
          weeklyReports = (reportsResponse['data']['items'] as List)
              .map((json) => WeeklyReport.fromJson(json))
              .toList();
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading weekly data: $e')),
      );
    } finally {
      setState(() => isLoading = false);
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Weekly Planning'),
        actions: [
          IconButton(
            icon: Icon(Icons.add),
            onPressed: () => _showCreateWeeklyRequestDialog(),
          ),
        ],
      ),
      body: isLoading
          ? Center(child: CircularProgressIndicator())
          : DefaultTabController(
              length: 2,
              child: Column(
                children: [
                  TabBar(
                    tabs: [
                      Tab(text: 'Work Requests', icon: Icon(Icons.assignment)),
                      Tab(text: 'Reports', icon: Icon(Icons.analytics)),
                    ],
                  ),
                  Expanded(
                    child: TabBarView(
                      children: [
                        _buildWeeklyRequestsList(),
                        _buildWeeklyReportsList(),
                      ],
                    ),
                  ),
                ],
              ),
            ),
    );
  }
  
  Widget _buildWeeklyRequestsList() {
    return RefreshIndicator(
      onRefresh: _loadWeeklyData,
      child: ListView.builder(
        padding: EdgeInsets.all(16),
        itemCount: weeklyRequests.length,
        itemBuilder: (context, index) {
          final request = weeklyRequests[index];
          return Card(
            child: ListTile(
              title: Text('Week of ${_formatDate(request.weekStartDate)}'),
              subtitle: Text(request.overallGoals),
              trailing: Chip(
                label: Text(request.status),
                backgroundColor: _getStatusColor(request.status),
              ),
              onTap: () => _showWeeklyRequestDetails(request),
            ),
          );
        },
      ),
    );
  }
  
  Widget _buildWeeklyReportsList() {
    return RefreshIndicator(
      onRefresh: _loadWeeklyData,
      child: ListView.builder(
        padding: EdgeInsets.all(16),
        itemCount: weeklyReports.length,
        itemBuilder: (context, index) {
          final report = weeklyReports[index];
          return Card(
            child: ListTile(
              title: Text('Report: Week of ${_formatDate(report.weekStartDate)}'),
              subtitle: Text(report.summaryOfProgress),
              trailing: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Chip(
                    label: Text(report.status),
                    backgroundColor: _getStatusColor(report.status),
                  ),
                  Text('${report.aggregatedMetrics.totalManHours}h'),
                ],
              ),
              onTap: () => _showWeeklyReportDetails(report),
            ),
          );
        },
      ),
    );
  }
  
  Color _getStatusColor(String status) {
    switch (status.toLowerCase()) {
      case 'draft': return Colors.grey;
      case 'submitted': return Colors.orange;
      case 'approved': return Colors.green;
      case 'inprogress': return Colors.blue;
      case 'completed': return Colors.teal;
      default: return Colors.grey;
    }
  }
  
  String _formatDate(String dateString) {
    final date = DateTime.parse(dateString);
    return '${date.day}/${date.month}/${date.year}';
  }
  
  void _showWeeklyRequestDetails(WeeklyWorkRequest request) {
    // Navigate to detailed weekly request view
  }
  
  void _showWeeklyReportDetails(WeeklyReport report) {
    // Navigate to detailed weekly report view
  }
  
  void _showCreateWeeklyRequestDialog() {
    // Show dialog to create new weekly request
  }
}
```
