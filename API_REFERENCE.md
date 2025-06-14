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

### 2. **API Client Configuration**
```dart
// lib/services/api_client.dart
import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiClient {
  static const String baseUrl = 'http://localhost:5002';
  static const String apiVersion = 'v1';
  static String get apiBase => '$baseUrl/api/$apiVersion';
  
  static const _storage = FlutterSecureStorage();
  
  // Get headers with auth token
  static Future<Map<String, String>> _getHeaders() async {
    final token = await _storage.read(key: 'jwt_token');
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }
  
  // Generic GET request
  static Future<Map<String, dynamic>> get(String endpoint) async {
    final response = await http.get(
      Uri.parse('$apiBase$endpoint'),
      headers: await _getHeaders(),
    );
    return _handleResponse(response);
  }
  
  // Generic POST request
  static Future<Map<String, dynamic>> post(String endpoint, Map<String, dynamic> data) async {
    final response = await http.post(
      Uri.parse('$apiBase$endpoint'),
      headers: await _getHeaders(),
      body: json.encode(data),
    );
    return _handleResponse(response);
  }
  
  // Response handler
  static Map<String, dynamic> _handleResponse(http.Response response) {
    final data = json.decode(response.body) as Map<String, dynamic>;
    
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return data;
    } else {
      throw ApiException(
        message: data['message'] ?? 'Unknown error',
        statusCode: response.statusCode,
        errors: List<String>.from(data['errors'] ?? []),
      );
    }
  }
}

class ApiException implements Exception {
  final String message;
  final int statusCode;
  final List<String> errors;
  
  ApiException({required this.message, required this.statusCode, required this.errors});
}
```

### 3. **Authentication Service**
```dart
// lib/services/auth_service.dart
class AuthService {
  static const _storage = FlutterSecureStorage();
  
  static Future<AuthResponse> login(String username, String password) async {
    final response = await ApiClient.post('/auth/login', {
      'username': username,  // Can be username or email
      'password': password,
    });
    
    final authData = AuthResponse.fromJson(response['data']);
    
    // Store tokens securely
    await _storage.write(key: 'jwt_token', value: authData.token);
    await _storage.write(key: 'refresh_token', value: authData.refreshToken);
    
    return authData;
  }
  
  static Future<void> logout() async {
    await _storage.delete(key: 'jwt_token');
    await _storage.delete(key: 'refresh_token');
  }
  
  static Future<bool> isLoggedIn() async {
    final token = await _storage.read(key: 'jwt_token');
    return token != null;
  }
}
```

### 4. **Data Models**
```dart
// lib/models/auth_response.dart
class AuthResponse {
  final String token;
  final String refreshToken;
  final User user;
  
  AuthResponse({required this.token, required this.refreshToken, required this.user});
  
  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      token: json['token'],
      refreshToken: json['refreshToken'],
      user: User.fromJson(json['user']),
    );
  }
}

class User {
  final String userId;
  final String username;
  final String email;
  final String fullName;
  final String roleName;
  final bool isActive;
  
  User({
    required this.userId,
    required this.username,
    required this.email,
    required this.fullName,
    required this.roleName,
    required this.isActive,
  });
  
  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      userId: json['userId'],
      username: json['username'],
      email: json['email'],
      fullName: json['fullName'],
      roleName: json['roleName'],
      isActive: json['isActive'],
    );
  }
}
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
- [🖼️ Image Upload](#️-image-upload)
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
**🎯 Role Required**: Admin, Manager

**Request Body**:
```json
{
  "name": "Solar Installation Project Beta",
  "description": "Commercial solar installation for office complex",
  "startDate": "2025-07-01",
  "endDate": "2025-09-30",
  "location": "Business District, San Francisco, CA",
  "budget": 500000.00
}
```

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

**Flutter Example - Task List**:
```dart
class TaskListScreen extends StatefulWidget {
  final String? projectId;
  
  const TaskListScreen({this.projectId});
  
  @override
  _TaskListScreenState createState() => _TaskListScreenState();
}

class _TaskListScreenState extends State<TaskListScreen> {
  List<ProjectTask> tasks = [];
  bool isLoading = true;
  String selectedStatus = 'All';
  
  @override
  void initState() {
    super.initState();
    _loadTasks();
  }
  
  Future<void> _loadTasks() async {
    setState(() => isLoading = true);
    
    try {
      String endpoint = '/tasks?pageSize=50';
      if (widget.projectId != null) {
        endpoint += '&projectId=${widget.projectId}';
      }
      if (selectedStatus != 'All') {
        endpoint += '&status=$selectedStatus';
      }
      
      final response = await ApiClient.get(endpoint);
      
      if (response['success']) {
        setState(() {
          tasks = (response['data']['tasks'] as List)
              .map((json) => ProjectTask.fromJson(json))
              .toList();
        });
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error loading tasks: $e')),
      );
    } finally {
      setState(() => isLoading = false);
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.projectId != null ? 'Project Tasks' : 'All Tasks'),
        actions: [
          PopupMenuButton<String>(
            initialValue: selectedStatus,
            onSelected: (status) {
              setState(() => selectedStatus = status);
              _loadTasks();
            },
            itemBuilder: (context) => [
              PopupMenuItem(value: 'All', child: Text('All Tasks')),
              PopupMenuItem(value: 'Pending', child: Text('Pending')),
              PopupMenuItem(value: 'InProgress', child: Text('In Progress')),
              PopupMenuItem(value: 'Completed', child: Text('Completed')),
            ],
          ),
        ],
      ),
      body: isLoading
          ? Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadTasks,
              child: tasks.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.task_alt, size: 64, color: Colors.grey),
                          SizedBox(height: 16),
                          Text('No tasks found', style: Theme.of(context).textTheme.titleMedium),
                        ],
                      ),
                    )
                  : ListView.builder(
                      padding: EdgeInsets.all(16),
                      itemCount: tasks.length,
                      itemBuilder: (context, index) {
                        final task = tasks[index];
                        return TaskCard(
                          task: task,
                          onTap: () => Navigator.pushNamed(
                            context,
                            '/task-details',
                            arguments: task.id,
                          ),
                          onStatusUpdate: (newStatus) => _updateTaskStatus(task.id, newStatus),
                        );
                      },
                    ),
            ),
    );
  }
  
  Future<void> _updateTaskStatus(String taskId, String newStatus) async {
    try {
      final response = await ApiClient.put('/tasks/$taskId', {
        'status': newStatus,
      });
      
      if (response['success']) {
        _loadTasks(); // Refresh the list
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Task status updated')),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error updating task: $e')),
      );
    }
  }
}

class TaskCard extends StatelessWidget {
  final ProjectTask task;
  final VoidCallback onTap;
  final Function(String) onStatusUpdate;
  
  const TaskCard({
    required this.task,
    required this.onTap,
    required this.onStatusUpdate,
  });
  
  @override
  Widget build(BuildContext context) {
    return Card(
      margin: EdgeInsets.only(bottom: 16),
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
                      task.title,
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  _PriorityChip(priority: task.priority),
                ],
              ),
              SizedBox(height: 8),
              Text(
                task.description,
                style: Theme.of(context).textTheme.bodyMedium,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              SizedBox(height: 12),
              Row(
                children: [
                  Icon(Icons.person, size: 16, color: Colors.grey),
                  SizedBox(width: 4),
                  Text(
                    task.assignedToUserName,
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                  Spacer(),
                  Icon(Icons.schedule, size: 16, color: Colors.grey),
                  SizedBox(width: 4),
                  Text(
                    'Due: ${_formatDate(task.dueDate)}',
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                ],
              ),
              SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Progress: ${task.progressPercentage.toStringAsFixed(0)}%',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                        SizedBox(height: 4),
                        LinearProgressIndicator(
                          value: task.progressPercentage / 100,
                          backgroundColor: Colors.grey[300],
                        ),
                      ],
                    ),
                  ),
                  SizedBox(width: 16),
                  DropdownButton<String>(
                    value: task.status,
                    items: ['Pending', 'InProgress', 'Completed'].map((status) {
                      return DropdownMenuItem(
                        value: status,
                        child: Text(status),
                      );
                    }).toList(),
                    onChanged: (newStatus) {
                      if (newStatus != null && newStatus != task.status) {
                        onStatusUpdate(newStatus);
                      }
                    },
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
  
  String _formatDate(DateTime date) {
    return '${date.month}/${date.day}/${date.year}';
  }
}

class _PriorityChip extends StatelessWidget {
  final String priority;
  
  const _PriorityChip({required this.priority});
  
  @override
  Widget build(BuildContext context) {
    Color color;
    switch (priority) {
      case 'High':
        color = Colors.red;
        break;
      case 'Medium':
        color = Colors.orange;
        break;
      case 'Low':
        color = Colors.green;
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
        priority,
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

---

## ⚡ Rate Limiting

The API implements rate limiting to ensure fair usage:

- **Rate Limit**: 50 requests per minute per IP address
- **Headers Included**: 
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Requests remaining in current window
  - `X-RateLimit-Reset`: Time when the rate limit resets

**Rate Limit Exceeded (429)**:
```json
{
  "success": false,
  "message": "Rate limit exceeded. Try again later.",
  "data": null,
  "errors": ["Too many requests"]
}
```

**Flutter Example - Rate Limit Handling**:
```dart
class ApiClient {
  static int remainingRequests = 50;
  static DateTime? resetTime;
  
  static Future<Map<String, dynamic>> _handleRateLimit(http.Response response) async {
    // Update rate limit info from headers
    remainingRequests = int.tryParse(response.headers['x-ratelimit-remaining'] ?? '50') ?? 50;
    
    if (response.statusCode == 429) {
      final resetHeader = response.headers['x-ratelimit-reset'];
      if (resetHeader != null) {
        resetTime = DateTime.fromMillisecondsSinceEpoch(int.parse(resetHeader) * 1000);
        final waitTime = resetTime!.difference(DateTime.now());
        
        throw RateLimitException(
          message: 'Rate limit exceeded',
          retryAfter: waitTime,
          remainingRequests: remainingRequests,
        );
      }
    }
    
    return json.decode(response.body);
  }
}

class RateLimitException implements Exception {
  final String message;
  final Duration retryAfter;
  final int remainingRequests;
  
  RateLimitException({
    required this.message,
    required this.retryAfter,
    required this.remainingRequests,
  });
}

// Usage with automatic retry
Future<Map<String, dynamic>> makeRequestWithRetry(String endpoint) async {
  try {
    return await ApiClient.get(endpoint);
  } on RateLimitException catch (e) {
    // Show user-friendly message
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Rate limit exceeded. Retrying in ${e.retryAfter.inSeconds} seconds...'),
        duration: e.retryAfter,
      ),
    );
    
    // Wait and retry
    await Future.delayed(e.retryAfter);
    return await ApiClient.get(endpoint);
  }
}
```

---

## ❌ Error Handling

### Standard Error Response Format
All error responses follow this consistent format:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error message 1", "Detailed error message 2"]
}
```

### Common HTTP Status Codes

| Status Code | Description | Common Causes |
|-------------|-------------|---------------|
| `400` | Bad Request | Invalid request format, missing required fields |
| `401` | Unauthorized | Missing or invalid authentication token |
| `403` | Forbidden | Insufficient permissions for requested action |
| `404` | Not Found | Resource doesn't exist or user lacks access |
| `409` | Conflict | Resource already exists or data conflict |
| `422` | Unprocessable Entity | Validation errors in request data |
| `429` | Too Many Requests | Rate limit exceeded |
| `500` | Internal Server Error | Unexpected server error |

### Flutter Error Handling Best Practices

```dart
class ApiErrorHandler {
  static void handleError(BuildContext context, dynamic error) {
    String message = 'An unexpected error occurred';
    String? action;
    
    if (error is ApiException) {
      switch (error.statusCode) {
        case 400:
          message = 'Invalid request. Please check your input.';
          break;
        case 401:
          message = 'Session expired. Please login again.';
          action = 'Login';
          break;
        case 403:
          message = 'You don\'t have permission to perform this action.';
          break;
        case 404:
          message = 'The requested item was not found.';
          break;
        case 409:
          message = 'This item already exists.';
          break;
        case 422:
          message = error.errors.isNotEmpty 
              ? error.errors.join('\n') 
              : 'Please check your input.';
          break;
        case 429:
          message = 'Too many requests. Please try again later.';
          break;
        case 500:
          message = 'Server error. Please try again later.';
          break;
        default:
          message = error.message;
      }
    } else if (error is SocketException) {
      message = 'No internet connection. Please check your network.';
      action = 'Retry';
    } else if (error is TimeoutException) {
      message = 'Request timed out. Please try again.';
      action = 'Retry';
    }
    
    _showErrorDialog(context, message, action);
  }
  
  static void _showErrorDialog(BuildContext context, String message, String? action) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Error'),
        content: Text(message),
        actions: [
          if (action != null)
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                if (action == 'Login') {
                  Navigator.pushReplacementNamed(context, '/login');
                }
                // Handle other actions...
              },
              child: Text(action),
            ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text('OK'),
          ),
        ],
      ),
    );
  }
}

// Usage in your API calls
try {
  final response = await ApiClient.post('/daily-reports', reportData);
  // Handle success
} catch (error) {
  ApiErrorHandler.handleError(context, error);
}
```

### Validation Error Examples

**Validation Error Response (422)**:
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Project name is required",
    "Budget must be greater than 0",
    "End date must be after start date"
  ]
}
```

**Flutter Validation Error Handling**:
```dart
class ValidationErrorWidget extends StatelessWidget {
  final List<String> errors;
  
  const ValidationErrorWidget({required this.errors});
  
  @override
  Widget build(BuildContext context) {
    if (errors.isEmpty) return SizedBox.shrink();
    
    return Container(
      margin: EdgeInsets.symmetric(vertical: 8),
      padding: EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.red.withOpacity(0.1),
        border: Border.all(color: Colors.red.withOpacity(0.3)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.error, color: Colors.red, size: 20),
              SizedBox(width: 8),
              Text(
                'Please fix the following errors:',
                style: TextStyle(
                  color: Colors.red,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
          SizedBox(height: 8),
          ...errors.map((error) => Padding(
                padding: EdgeInsets.only(left: 24, bottom: 4),
                child: Text(
                  '• $error',
                  style: TextStyle(color: Colors.red),
                ),
              )),
        ],
      ),
    );
  }
}
```

---

## 📱 Complete Flutter App Example

Here's a complete example of a Flutter app structure for the Solar Projects API:

```dart
// main.dart
void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => ProjectProvider()),
        ChangeNotifierProvider(create: (_) => DailyReportProvider()),
      ],
      child: MaterialApp(
        title: 'Solar Projects Manager',
        theme: ThemeData(
          primarySwatch: Colors.orange,
          appBarTheme: AppBarTheme(
            backgroundColor: Colors.orange,
            foregroundColor: Colors.white,
          ),
        ),
        initialRoute: '/',
        routes: {
          '/': (context) => SplashScreen(),
          '/login': (context) => LoginScreen(),
          '/dashboard': (context) => DashboardScreen(),
          '/projects': (context) => ProjectsScreen(),
          '/daily-reports': (context) => DailyReportsScreen(),
          '/create-report': (context) => CreateDailyReportScreen(),
        },
      ),
    );
  }
}

// auth_provider.dart
class AuthProvider extends ChangeNotifier {
  User? _user;
  bool _isLoading = false;
  
  User? get user => _user;
  bool get isLoading => _isLoading;
  bool get isLoggedIn => _user != null;
  
  Future<void> login(String username, String password) async {
    _isLoading = true;
    notifyListeners();
    
    try {
      final authResponse = await AuthService.login(username, password);
      _user = authResponse.user;
      notifyListeners();
    } catch (e) {
      _isLoading = false;
      notifyListeners();
      rethrow;
    }
    
    _isLoading = false;
    notifyListeners();
  }
  
  Future<void> logout() async {
    await AuthService.logout();
    _user = null;
    notifyListeners();
  }
  
  Future<void> checkAuthStatus() async {
    final isLoggedIn = await AuthService.isLoggedIn();
    if (isLoggedIn) {
      // Get user info from stored token or API
      _user = await AuthService.getCurrentUser();
      notifyListeners();
    }
  }
}
```

This comprehensive API reference provides Flutter developers with everything needed to integrate with the Solar Projects REST API, including complete code examples, error handling, and best practices for mobile app development.

---

## 🎯 Summary

The Solar Projects REST API is designed specifically for mobile app development with Flutter, providing:

✅ **Flexible Authentication** with username/email login  
✅ **Comprehensive CRUD Operations** for all entities  
✅ **Mobile-Optimized Features** like GPS integration and image upload  
✅ **Real-time Data** with efficient pagination and filtering  
✅ **Robust Error Handling** with consistent response formats  
✅ **Performance Features** like caching and rate limiting  
✅ **Complete Flutter Examples** for all major functionality  

For additional support or feature requests, consult the API documentation or contact the development team.
