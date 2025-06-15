# Solar Projects API Documentation for Flutter

## Overview
Complete API documentation for the Solar Projects .NET REST API, designed for Flutter mobile app integration. This API uses JWT authentication with role-based access control (RBAC) and implements Clean Architecture principles.

## Features
- Complete CRUD operations for Projects, Tasks, and Daily Reports
- Role-based access control (Administrator, ProjectManager, User)
- Advanced querying with filtering, sorting, and pagination
- Rich HATEOAS pagination support
- Intelligent caching strategies
- Robust error handling

## Base Configuration

### API Base URL
```dart
class ApiConfig {
  static const String baseUrl = 'http://localhost:5002';
  static const String apiVersion = 'v1';
  static const String apiBaseUrl = '$baseUrl/api/$apiVersion';
}
```

### Flutter HTTP Client Setup
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class ApiClient {
  static String? _authToken;
  
  static Map<String, String> get headers => {
    'Content-Type': 'application/json',
    if (_authToken != null) 'Authorization': 'Bearer $_authToken',
  };

  static void setAuthToken(String token) {
    _authToken = token;
  }

  static void clearAuthToken() {
    _authToken = null;
  }
}
```

## Authentication

### 1. User Login
**Endpoint**: `POST /api/auth/login`  
**Access**: Public

```dart
// Request Model
class LoginRequest {
  final String username;
  final String password;

  LoginRequest({required this.username, required this.password});

  Map<String, dynamic> toJson() => {
    'username': username,
    'password': password,
  };
}

// Response Model
class LoginResponse {
  final String token;
  final String refreshToken;
  final DateTime expiresAt;
  final UserDto user;

  LoginResponse.fromJson(Map<String, dynamic> json)
    : token = json['token'],
      refreshToken = json['refreshToken'],
      expiresAt = DateTime.parse(json['expiresAt']),
      user = UserDto.fromJson(json['user']);
}

// Flutter Usage
Future<LoginResponse?> login(String username, String password) async {
  final request = LoginRequest(username: username, password: password);
  
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/auth/login'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 200) {
      final loginResponse = LoginResponse.fromJson(jsonDecode(response.body));
      ApiClient.setAuthToken(loginResponse.token);
      return loginResponse;
    }
    return null;
  } catch (e) {
    print('Login error: $e');
    return null;
  }
}
```

### 2. User Registration
**Endpoint**: `POST /api/auth/register`  
**Access**: Public

```dart
// Request Model
class RegisterRequest {
  final String username;
  final String email;
  final String password;
  final String fullName;
  final String role; // "Administrator", "ProjectManager", "Planner", "Technician"

  RegisterRequest({
    required this.username,
    required this.email, 
    required this.password,
    required this.fullName,
    required this.role,
  });

  Map<String, dynamic> toJson() => {
    'username': username,
    'email': email,
    'password': password,
    'fullName': fullName,
    'role': role,
  };
}

// Flutter Usage
Future<bool> register(RegisterRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/auth/register'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );
    
    return response.statusCode == 201;
  } catch (e) {
    print('Registration error: $e');
    return false;
  }
}
```

### 3. Refresh Token
**Endpoint**: `POST /api/auth/refresh`  
**Access**: Requires valid refresh token

```dart
Future<String?> refreshToken(String refreshToken) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/auth/refresh'),
      headers: ApiClient.headers,
      body: jsonEncode({'refreshToken': refreshToken}),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final newToken = data['token'];
      ApiClient.setAuthToken(newToken);
      return newToken;
    }
    return null;
  } catch (e) {
    print('Token refresh error: $e');
    return null;
  }
}
```

## Projects Management

### 4. Get All Projects
**Endpoint**: `GET /api/v1/projects`  
**Access**: All authenticated users

```dart
// Response Model
class ProjectDto {
  final String projectId;
  final String projectName;
  final String status;
  final String? clientInfo;
  final DateTime startDate;
  final DateTime? estimatedEndDate;
  final DateTime? actualEndDate;
  final String? address;
  final UserDto? projectManager;
  final List<TaskDto> tasks;

  ProjectDto.fromJson(Map<String, dynamic> json)
    : projectId = json['projectId'],
      projectName = json['projectName'],
      status = json['status'],
      clientInfo = json['clientInfo'],
      startDate = DateTime.parse(json['startDate']),
      estimatedEndDate = json['estimatedEndDate'] != null 
          ? DateTime.parse(json['estimatedEndDate']) : null,
      actualEndDate = json['actualEndDate'] != null 
          ? DateTime.parse(json['actualEndDate']) : null,
      address = json['address'],
      projectManager = json['projectManager'] != null 
          ? UserDto.fromJson(json['projectManager']) : null,
      tasks = (json['tasks'] as List?)
          ?.map((task) => TaskDto.fromJson(task))
          .toList() ?? [];
}

// Query Parameters
class ProjectQueryParameters {
  final int pageNumber;
  final int pageSize;
  final String? projectName;
  final String? status;
  final String? clientInfo;
  final DateTime? startDateAfter;
  final DateTime? startDateBefore;

  ProjectQueryParameters({
    this.pageNumber = 1,
    this.pageSize = 10,
    this.projectName,
    this.status,
    this.clientInfo,
    this.startDateAfter,
    this.startDateBefore,
  });

  String toQueryString() {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectName != null) params['projectName'] = projectName!;
    if (status != null) params['status'] = status!;
    if (clientInfo != null) params['clientInfo'] = clientInfo!;
    if (startDateAfter != null) params['startDateAfter'] = startDateAfter!.toIso8601String();
    if (startDateBefore != null) params['startDateBefore'] = startDateBefore!.toIso8601String();
    
    return params.entries.map((e) => '${e.key}=${Uri.encodeComponent(e.value)}').join('&');
  }
}

// Flutter Usage
Future<List<ProjectDto>> getProjects({ProjectQueryParameters? params}) async {
  try {
    String url = '${ApiConfig.apiBaseUrl}/projects';
    if (params != null) {
      url += '?${params.toQueryString()}';
    }

    final response = await http.get(
      Uri.parse(url),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['data']['items'] as List)
          .map((project) => ProjectDto.fromJson(project))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get projects error: $e');
    return [];
  }
}
```

### 5. Get Project by ID
**Endpoint**: `GET /api/v1/projects/{id}`  
**Access**: All authenticated users

```dart
Future<ProjectDto?> getProject(String projectId) async {
  try {
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/projects/$projectId'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return ProjectDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Get project error: $e');
    return null;
  }
}
```

### 6. Create Project
**Endpoint**: `POST /api/v1/projects`  
**Access**: Administrator, ProjectManager

```dart
// Request Model
class CreateProjectRequest {
  final String projectName;
  final String status;
  final String? clientInfo;
  final DateTime startDate;
  final DateTime? estimatedEndDate;
  final String? address;
  final String? projectManagerId;

  CreateProjectRequest({
    required this.projectName,
    required this.status,
    this.clientInfo,
    required this.startDate,
    this.estimatedEndDate,
    this.address,
    this.projectManagerId,
  });

  Map<String, dynamic> toJson() => {
    'projectName': projectName,
    'status': status,
    'clientInfo': clientInfo,
    'startDate': startDate.toIso8601String(),
    'estimatedEndDate': estimatedEndDate?.toIso8601String(),
    'address': address,
    'projectManagerId': projectManagerId,
  };
}

// Flutter Usage
Future<ProjectDto?> createProject(CreateProjectRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/projects'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return ProjectDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create project error: $e');
    return null;
  }
}
```

### 7. Update Project
**Endpoint**: `PUT /api/v1/projects/{id}`  
**Access**: Administrator, ProjectManager

```dart
// Request Model (same as CreateProjectRequest but optional fields)
class UpdateProjectRequest {
  final String? projectName;
  final String? status;
  final String? clientInfo;
  final DateTime? startDate;
  final DateTime? estimatedEndDate;
  final String? address;
  final String? projectManagerId;

  UpdateProjectRequest({
    this.projectName,
    this.status,
    this.clientInfo,
    this.startDate,
    this.estimatedEndDate,
    this.address,
    this.projectManagerId,
  });

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (projectName != null) map['projectName'] = projectName;
    if (status != null) map['status'] = status;
    if (clientInfo != null) map['clientInfo'] = clientInfo;
    if (startDate != null) map['startDate'] = startDate!.toIso8601String();
    if (estimatedEndDate != null) map['estimatedEndDate'] = estimatedEndDate!.toIso8601String();
    if (address != null) map['address'] = address;
    if (projectManagerId != null) map['projectManagerId'] = projectManagerId;
    return map;
  }
}

// Flutter Usage
Future<ProjectDto?> updateProject(String projectId, UpdateProjectRequest request) async {
  try {
    final response = await http.put(
      Uri.parse('${ApiConfig.apiBaseUrl}/projects/$projectId'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return ProjectDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Update project error: $e');
    return null;
  }
}
```

### 8. Delete Project
**Endpoint**: `DELETE /api/v1/projects/{id}`  
**Access**: Administrator only

```dart
Future<bool> deleteProject(String projectId) async {
  try {
    final response = await http.delete(
      Uri.parse('${ApiConfig.apiBaseUrl}/projects/$projectId'),
      headers: ApiClient.headers,
    );

    return response.statusCode == 200;
  } catch (e) {
    print('Delete project error: $e');
    return false;
  }
}
```

## Task Management

### 9. Get All Tasks
**Endpoint**: `GET /api/v1/tasks`  
**Access**: All authenticated users

```dart
// Task Model
class TaskDto {
  final String taskId;
  final String title;
  final String? description;
  final String status;
  final DateTime? dueDate;
  final DateTime? completionDate;
  final String projectId;
  final String? assignedTechnicianId;

  TaskDto.fromJson(Map<String, dynamic> json)
    : taskId = json['taskId'],
      title = json['title'],
      description = json['description'],
      status = json['status'],
      dueDate = json['dueDate'] != null ? DateTime.parse(json['dueDate']) : null,
      completionDate = json['completionDate'] != null ? DateTime.parse(json['completionDate']) : null,
      projectId = json['projectId'],
      assignedTechnicianId = json['assignedTechnicianId'];
}

// Flutter Usage
Future<List<TaskDto>> getTasks({
  int pageNumber = 1,
  int pageSize = 10,
  String? projectId,
  String? assigneeId,
}) async {
  try {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectId != null) params['projectId'] = projectId;
    if (assigneeId != null) params['assigneeId'] = assigneeId;
    
    final queryString = params.entries.map((e) => '${e.key}=${e.value}').join('&');
    
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/tasks?$queryString'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['items'] as List)
          .map((task) => TaskDto.fromJson(task))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get tasks error: $e');
    return [];
  }
}
```

### 10. Create Task
**Endpoint**: `POST /api/v1/tasks/project/{projectId}`  
**Access**: Administrator, ProjectManager

```dart
// Request Model
class CreateTaskRequest {
  final String title;
  final String? description;
  final DateTime? dueDate;
  final String? assignedTechnicianId;

  CreateTaskRequest({
    required this.title,
    this.description,
    this.dueDate,
    this.assignedTechnicianId,
  });

  Map<String, dynamic> toJson() => {
    'title': title,
    'description': description,
    'dueDate': dueDate?.toIso8601String(),
    'assignedTechnicianId': assignedTechnicianId,
  };
}

// Flutter Usage
Future<TaskDto?> createTask(String projectId, CreateTaskRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/tasks/project/$projectId'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return TaskDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create task error: $e');
    return null;
  }
}
```

## Daily Reports

### 11. Get Daily Reports
**Endpoint**: `GET /api/v1/daily-reports`  
**Access**: Authenticated Users

```dart
Future<List<DailyReport>> getDailyReports({
  int pageNumber = 1,
  int pageSize = 10,
  String? projectId,
  DateTime? startDate,
  DateTime? endDate,
}) async {
  final queryParams = {
    'pageNumber': pageNumber.toString(),
    'pageSize': pageSize.toString(),
    if (projectId != null) 'projectId': projectId,
    if (startDate != null) 'startDate': startDate.toIso8601String(),
    if (endDate != null) 'endDate': endDate.toIso8601String(),
  };

  final queryString = Uri(queryParameters: queryParams).query;
  
  try {
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports?$queryString'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['data']['items'] as List)
          .map((item) => DailyReport.fromJson(item))
          .toList();
    }
    throw Exception('Failed to fetch daily reports');
  } catch (e) {
    print('Error fetching daily reports: $e');
    rethrow;
  }
}
```

### 12. Get Daily Report by ID
**Endpoint**: `GET /api/v1/daily-reports/{id}`  
**Access**: Authenticated Users

```dart
Future<DailyReport> getDailyReport(String id) async {
  try {
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$id'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return DailyReport.fromJson(data['data']);
    }
    throw Exception('Failed to fetch daily report');
  } catch (e) {
    print('Error fetching daily report: $e');
    rethrow;
  }
}
```

### 13. Create Daily Report
**Endpoint**: `POST /api/v1/daily-reports`  
**Access**: Project Managers, Site Supervisors

```dart
class CreateDailyReportRequest {
  final String projectId;
  final String summary;
  final List<String> activities;
  final WeatherData weatherData;
  final List<String> attachments;

  Map<String, dynamic> toJson() => {
    'projectId': projectId,
    'summary': summary,
    'activities': activities,
    'weatherData': weatherData.toJson(),
    'attachments': attachments,
  };
}

Future<DailyReport> createDailyReport(CreateDailyReportRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return DailyReport.fromJson(data['data']);
    }
    throw Exception('Failed to create daily report');
  } catch (e) {
    print('Error creating daily report: $e');
    rethrow;
  }
}
```

### 14. Update Daily Report
**Endpoint**: `PUT /api/v1/daily-reports/{id}`  
**Access**: Project Managers, Site Supervisors (Original Creator)

```dart
class UpdateDailyReportRequest {
  final String summary;
  final List<String> activities;
  final WeatherData weatherData;
  final List<String> attachments;

  Map<String, dynamic> toJson() => {
    'summary': summary,
    'activities': activities,
    'weatherData': weatherData.toJson(),
    'attachments': attachments,
  };
}

Future<DailyReport> updateDailyReport(
  String id,
  UpdateDailyReportRequest request,
) async {
  try {
    final response = await http.put(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$id'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return DailyReport.fromJson(data['data']);
    }
    throw Exception('Failed to update daily report');
  } catch (e) {
    print('Error updating daily report: $e');
    rethrow;
  }
}
```

### 15. Delete Daily Report
**Endpoint**: `DELETE /api/v1/daily-reports/{id}`  
**Access**: Project Managers, Administrators

```dart
Future<bool> deleteDailyReport(String id) async {
  try {
    final response = await http.delete(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$id'),
      headers: ApiClient.headers,
    );

    return response.statusCode == 204;
  } catch (e) {
    print('Error deleting daily report: $e');
    rethrow;
  }
}
```

## Weekly Work Requests

### 16. Get Weekly Work Requests
**Endpoint**: `GET /api/v1/weekly-requests`  
**Access**: All authenticated users

```dart
// Weekly Work Request Model
class WeeklyWorkRequestDto {
  final String weeklyRequestId;
  final String projectId;
  final DateTime weekStartDate;
  final String? plannedActivities;
  final String? resourceRequirements;
  final String? anticipatedChallenges;
  final String status;
  final UserDto? submittedByUser;
  final UserDto? approvedByUser;

  WeeklyWorkRequestDto.fromJson(Map<String, dynamic> json)
    : weeklyRequestId = json['weeklyRequestId'],
      projectId = json['projectId'],
      weekStartDate = DateTime.parse(json['weekStartDate']),
      plannedActivities = json['plannedActivities'],
      resourceRequirements = json['resourceRequirements'],
      anticipatedChallenges = json['anticipatedChallenges'],
      status = json['status'],
      submittedByUser = json['submittedByUser'] != null 
          ? UserDto.fromJson(json['submittedByUser']) : null,
      approvedByUser = json['approvedByUser'] != null 
          ? UserDto.fromJson(json['approvedByUser']) : null;
}

// Flutter Usage
Future<List<WeeklyWorkRequestDto>> getWeeklyWorkRequests({
  int pageNumber = 1,
  int pageSize = 10,
  String? projectId,
  String? status,
}) async {
  try {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectId != null) params['projectId'] = projectId;
    if (status != null) params['status'] = status;
    
    final queryString = params.entries.map((e) => '${e.key}=${e.value}').join('&');
    
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-requests?$queryString'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['data']['items'] as List)
          .map((request) => WeeklyWorkRequestDto.fromJson(request))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get weekly work requests error: $e');
    return [];
  }
}
```

### 17. Create Weekly Work Request
**Endpoint**: `POST /api/v1/weekly-requests`  
**Access**: Administrator, ProjectManager, Planner

```dart
// Request Model
class CreateWeeklyWorkRequestDto {
  final String projectId;
  final DateTime weekStartDate;
  final String? plannedActivities;
  final String? resourceRequirements;
  final String? anticipatedChallenges;
  final String submittedById;

  CreateWeeklyWorkRequestDto({
    required this.projectId,
    required this.weekStartDate,
    this.plannedActivities,
    this.resourceRequirements,
    this.anticipatedChallenges,
    required this.submittedById,
  });

  Map<String, dynamic> toJson() => {
    'projectId': projectId,
    'weekStartDate': weekStartDate.toIso8601String(),
    'plannedActivities': plannedActivities,
    'resourceRequirements': resourceRequirements,
    'anticipatedChallenges': anticipatedChallenges,
    'submittedById': submittedById,
  };
}

// Flutter Usage
Future<WeeklyWorkRequestDto?> createWeeklyWorkRequest(CreateWeeklyWorkRequestDto request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-requests'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return WeeklyWorkRequestDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create weekly work request error: $e');
    return null;
  }
}
```

### 18. Submit Weekly Work Request
**Endpoint**: `POST /api/v1/weekly-requests/{id}/submit`  
**Access**: Administrator, ProjectManager, Planner

```dart
Future<bool> submitWeeklyWorkRequest(String requestId) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-requests/$requestId/submit'),
      headers: ApiClient.headers,
    );

    return response.statusCode == 200;
  } catch (e) {
    print('Submit weekly work request error: $e');
    return false;
  }
}
```

### 19. Approve Weekly Work Request
**Endpoint**: `POST /api/v1/weekly-requests/{id}/approve`  
**Access**: Administrator, ProjectManager

```dart
Future<bool> approveWeeklyWorkRequest(String requestId) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-requests/$requestId/approve'),
      headers: ApiClient.headers,
    );

    return response.statusCode == 200;
  } catch (e) {
    print('Approve weekly work request error: $e');
    return false;
  }
}
```

## Weekly Reports

### 20. Get Weekly Reports
**Endpoint**: `GET /api/v1/weekly-reports`  
**Access**: All authenticated users

```dart
// Weekly Report Model
class WeeklyReportDto {
  final String weeklyReportId;
  final String projectId;
  final DateTime weekStartDate;
  final String? summaryOfProgress;
  final double? totalManHours;
  final int? panelsInstalled;
  final int? safetyIncidents;
  final String? delaysReported;
  final List<String> majorAccomplishments;
  final List<WeeklyIssueDto> majorIssues;
  final String? lookahead;
  final String status;
  final UserDto? submittedByUser;

  WeeklyReportDto.fromJson(Map<String, dynamic> json)
    : weeklyReportId = json['weeklyReportId'],
      projectId = json['projectId'],
      weekStartDate = DateTime.parse(json['weekStartDate']),
      summaryOfProgress = json['summaryOfProgress'],
      totalManHours = json['totalManHours']?.toDouble(),
      panelsInstalled = json['panelsInstalled'],
      safetyIncidents = json['safetyIncidents'],
      delaysReported = json['delaysReported'],
      majorAccomplishments = (json['majorAccomplishments'] as List?)
          ?.map((item) => item.toString())
          .toList() ?? [],
      majorIssues = (json['majorIssues'] as List?)
          ?.map((issue) => WeeklyIssueDto.fromJson(issue))
          .toList() ?? [],
      lookahead = json['lookahead'],
      status = json['status'],
      submittedByUser = json['submittedByUser'] != null 
          ? UserDto.fromJson(json['submittedByUser']) : null;
}

class WeeklyIssueDto {
  final String description;
  final String severity;
  final String? resolution;

  WeeklyIssueDto.fromJson(Map<String, dynamic> json)
    : description = json['description'],
      severity = json['severity'],
      resolution = json['resolution'];

  Map<String, dynamic> toJson() => {
    'description': description,
    'severity': severity,
    'resolution': resolution,
  };
}

// Flutter Usage
Future<List<WeeklyReportDto>> getWeeklyReports({
  int pageNumber = 1,
  int pageSize = 10,
  String? projectId,
  String? status,
}) async {
  try {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectId != null) params['projectId'] = projectId;
    if (status != null) params['status'] = status;
    
    final queryString = params.entries.map((e) => '${e.key}=${e.value}').join('&');
    
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-reports?$queryString'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['data']['items'] as List)
          .map((report) => WeeklyReportDto.fromJson(report))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get weekly reports error: $e');
    return [];
  }
}
```

### 21. Create Weekly Report
**Endpoint**: `POST /api/v1/weekly-reports`  
**Access**: Administrator, ProjectManager (Note: Planner cannot create reports)

```dart
// Request Model
class CreateWeeklyReportDto {
  final String projectId;
  final DateTime weekStartDate;
  final String? summaryOfProgress;
  final double? totalManHours;
  final int? panelsInstalled;
  final int? safetyIncidents;
  final String? delaysReported;
  final List<String>? majorAccomplishments;
  final List<WeeklyIssueDto>? majorIssues;
  final String? lookahead;
  final String submittedById;
  final double? completionPercentage;

  CreateWeeklyReportDto({
    required this.projectId,
    required this.weekStartDate,
    this.summaryOfProgress,
    this.totalManHours,
    this.panelsInstalled,
    this.safetyIncidents,
    this.delaysReported,
    this.majorAccomplishments,
    this.majorIssues,
    this.lookahead,
    required this.submittedById,
    this.completionPercentage,
  });

  Map<String, dynamic> toJson() => {
    'projectId': projectId,
    'weekStartDate': weekStartDate.toIso8601String(),
    'summaryOfProgress': summaryOfProgress,
    'totalManHours': totalManHours,
    'panelsInstalled': panelsInstalled,
    'safetyIncidents': safetyIncidents,
    'delaysReported': delaysReported,
    'majorAccomplishments': majorAccomplishments,
    'majorIssues': majorIssues?.map((issue) => issue.toJson()).toList(),
    'lookahead': lookahead,
    'submittedById': submittedById,
    'completionPercentage': completionPercentage,
  };
}

// Flutter Usage
Future<WeeklyReportDto?> createWeeklyReport(CreateWeeklyReportDto request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/weekly-reports'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return WeeklyReportDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create weekly report error: $e');
    return null;
  }
}
```

## Work Requests (General)

### 22. Get Work Requests
**Endpoint**: `GET /api/v1/work-requests`  
**Access**: All authenticated users

```dart
// Work Request Model
class WorkRequestDto {
  final String workRequestId;
  final String title;
  final String? description;
  final String status;
  final String priority;
  final String projectId;
  final String? assignedUserId;
  final DateTime createdAt;
  final DateTime? dueDate;

  WorkRequestDto.fromJson(Map<String, dynamic> json)
    : workRequestId = json['workRequestId'],
      title = json['title'],
      description = json['description'],
      status = json['status'],
      priority = json['priority'],
      projectId = json['projectId'],
      assignedUserId = json['assignedUserId'],
      createdAt = DateTime.parse(json['createdAt']),
      dueDate = json['dueDate'] != null ? DateTime.parse(json['dueDate']) : null;
}

// Flutter Usage
Future<List<WorkRequestDto>> getWorkRequests({
  int pageNumber = 1,
  int pageSize = 10,
  String? projectId,
  String? status,
  String? priority,
}) async {
  try {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectId != null) params['projectId'] = projectId;
    if (status != null) params['status'] = status;
    if (priority != null) params['priority'] = priority;
    
    final queryString = params.entries.map((e) => '${e.key}=${e.value}').join('&');
    
    final response = await http.get(
      Uri.parse('${ApiConfig.apiBaseUrl}/work-requests?$queryString'),
      headers: ApiClient.headers,
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return (data['data']['items'] as List)
          .map((request) => WorkRequestDto.fromJson(request))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get work requests error: $e');
    return [];
  }
}
```

### 23. Create Work Request
**Endpoint**: `POST /api/v1/work-requests`  
**Access**: Administrator, ProjectManager, Planner

```dart
// Request Model
class CreateWorkRequestRequest {
  final String title;
  final String? description;
  final String priority; // "Low", "Medium", "High", "Critical"
  final String projectId;
  final String? assignedUserId;
  final DateTime? dueDate;

  CreateWorkRequestRequest({
    required this.title,
    this.description,
    required this.priority,
    required this.projectId,
    this.assignedUserId,
    this.dueDate,
  });

  Map<String, dynamic> toJson() => {
    'title': title,
    'description': description,
    'priority': priority,
    'projectId': projectId,
    'assignedUserId': assignedUserId,
    'dueDate': dueDate?.toIso8601String(),
  };
}

// Flutter Usage
Future<WorkRequestDto?> createWorkRequest(CreateWorkRequestRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/work-requests'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return WorkRequestDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create work request error: $e');
    return null;
  }
}
```

## Health & Utility

### 24. Health Check
**Endpoint**: `GET /health`  
**Access**: Public

```dart
class HealthResponse {
  final String status;
  final DateTime timestamp;
  final String version;
  final String environment;

  HealthResponse.fromJson(Map<String, dynamic> json)
    : status = json['status'],
      timestamp = DateTime.parse(json['timestamp']),
      version = json['version'],
      environment = json['environment'];
}

// Flutter Usage
Future<HealthResponse?> checkHealth() async {
  try {
    final response = await http.get(
      Uri.parse('${ApiConfig.baseUrl}/health'),
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      return HealthResponse.fromJson(jsonDecode(response.body));
    }
    return null;
  } catch (e) {
    print('Health check error: $e');
    return null;
  }
}
```

## Common Models

### User Model
```dart
class UserDto {
  final String userId;
  final String username;
  final String email;
  final String fullName;
  final bool isActive;
  final String roleId;
  final RoleDto? role;

  UserDto.fromJson(Map<String, dynamic> json)
    : userId = json['userId'],
      username = json['username'],
      email = json['email'],
      fullName = json['fullName'],
      isActive = json['isActive'],
      roleId = json['roleId'],
      role = json['role'] != null ? RoleDto.fromJson(json['role']) : null;
}

class RoleDto {
  final String roleId;
  final String roleName;

  RoleDto.fromJson(Map<String, dynamic> json)
    : roleId = json['roleId'],
      roleName = json['roleName'];
}
```

### API Response Models
```dart
class ApiResponse<T> {
  final bool success;
  final String message;
  final T? data;
  final List<String> errors;

  ApiResponse.fromJson(Map<String, dynamic> json, T Function(dynamic) fromJsonT)
    : success = json['success'],
      message = json['message'],
      data = json['data'] != null ? fromJsonT(json['data']) : null,
      errors = (json['errors'] as List?)?.map((e) => e.toString()).toList() ?? [];
}

class PagedResult<T> {
  final List<T> items;
  final int totalCount;
  final int pageNumber;
  final int pageSize;
  final int totalPages;

  PagedResult.fromJson(Map<String, dynamic> json, T Function(dynamic) fromJsonT)
    : items = (json['items'] as List).map((item) => fromJsonT(item)).toList(),
      totalCount = json['totalCount'],
      pageNumber = json['pageNumber'],
      pageSize = json['pageSize'],
      totalPages = json['totalPages'];
}
```

## Error Handling & Best Practices

### API Response Wrapper

```dart
class ApiResponse<T> {
  final bool success;
  final String message;
  final T? data;
  final List<String>? errors;

  ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJson,
  )   : success = json['success'],
        message = json['message'],
        errors = json['errors'] != null 
            ? List<String>.from(json['errors'])
            : null,
        data = json['data'] != null 
            ? fromJson(json['data'])
            : null;
}

// For list responses
class ApiListResponse<T> {
  final bool success;
  final String message;
  final List<T> data;
  final List<String>? errors;

  ApiListResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJson,
  )   : success = json['success'],
        message = json['message'],
        errors = json['errors'] != null 
            ? List<String>.from(json['errors'])
            : null,
        data = (json['data'] as List)
            .map((item) => fromJson(item))
            .toList();
}
```

### Error Handling Service

```dart
class ApiException implements Exception {
  final String message;
  final int? statusCode;
  final List<String>? errors;

  ApiException(this.message, {this.statusCode, this.errors});

  @override
  String toString() => message;
}

class ApiErrorHandler {
  static void handleError(http.Response response) {
    switch (response.statusCode) {
      case 400:
        throw ApiException(
          'Bad Request',
          statusCode: 400,
          errors: _parseErrors(response),
        );
      case 401:
        throw ApiException(
          'Unauthorized - Please login again',
          statusCode: 401,
        );
      case 403:
        throw ApiException(
          'Forbidden - You don\'t have permission',
          statusCode: 403,
        );
      case 404:
        throw ApiException(
          'Not Found',
          statusCode: 404,
        );
      case 500:
        throw ApiException(
          'Internal Server Error',
          statusCode: 500,
        );
      default:
        throw ApiException(
          'Something went wrong',
          statusCode: response.statusCode,
        );
    }
  }

  static List<String>? _parseErrors(http.Response response) {
    try {
      final data = jsonDecode(response.body);
      if (data['errors'] != null) {
        return List<String>.from(data['errors']);
      }
      return null;
    } catch (_) {
      return null;
    }
  }
}
```

### Implementing Retry Logic

```dart
Future<T> retryRequest<T>(
  Future<T> Function() request, {
  int maxAttempts = 3,
  Duration delay = const Duration(seconds: 1),
}) async {
  int attempts = 0;
  while (true) {
    try {
      attempts++;
      return await request();
    } catch (e) {
      if (attempts >= maxAttempts) rethrow;
      if (e is ApiException && e.statusCode == 401) rethrow; // Don't retry auth errors
      
      await Future.delayed(delay * attempts);
    }
  }
}

// Usage Example
Future<List<Project>> getProjectsWithRetry() async {
  return retryRequest(
    () => getProjects(),
    maxAttempts: 3,
    delay: Duration(seconds: 2),
  );
}
```

### Caching Implementation

```dart
class ApiCache {
  static final _cache = <String, _CacheEntry>{};
  
  static T? get<T>(String key) {
    final entry = _cache[key];
    if (entry == null) return null;
    
    if (entry.isExpired) {
      _cache.remove(key);
      return null;
    }
    
    return entry.value as T;
  }
  
  static void set<T>(String key, T value, Duration ttl) {
    _cache[key] = _CacheEntry(
      value: value,
      expiresAt: DateTime.now().add(ttl),
    );
  }
  
  static void remove(String key) {
    _cache.remove(key);
  }
  
  static void clear() {
    _cache.clear();
  }
}

class _CacheEntry {
  final dynamic value;
  final DateTime expiresAt;
  
  _CacheEntry({required this.value, required this.expiresAt});
  
  bool get isExpired => DateTime.now().isAfter(expiresAt);
}

// Usage Example
Future<Project> getProjectWithCache(String id) async {
  final cacheKey = 'project_$id';
  final cached = ApiCache.get<Project>(cacheKey);
  if (cached != null) return cached;
  
  final project = await getProject(id);
  ApiCache.set(cacheKey, project, Duration(minutes: 15));
  return project;
}
```

### Best Practices

1. **Token Management**
```dart
class AuthManager {
  static String? _token;
  static String? _refreshToken;
  static DateTime? _expiresAt;
  
  static bool get isLoggedIn => 
    _token != null && _expiresAt != null && 
    DateTime.now().isBefore(_expiresAt!);
  
  static Future<void> refreshTokenIfNeeded() async {
    if (!isLoggedIn && _refreshToken != null) {
      // Implement token refresh logic
    }
  }
  
  static Future<void> logout() async {
    _token = null;
    _refreshToken = null;
    _expiresAt = null;
    ApiCache.clear();
    // Implement additional cleanup
  }
}
```

2. **API Service Base Class**
```dart
abstract class BaseApiService {
  final String baseUrl;
  
  BaseApiService(this.baseUrl);
  
  Future<T> get<T>(
    String endpoint,
    T Function(Map<String, dynamic>) fromJson, {
    Map<String, String>? queryParams,
  }) async {
    final uri = Uri.parse('$baseUrl$endpoint')
        .replace(queryParameters: queryParams);
    
    try {
      final response = await http.get(uri, headers: ApiClient.headers);
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return fromJson(data);
      }
      ApiErrorHandler.handleError(response);
      throw ApiException('Unknown error');
    } catch (e) {
      if (e is ApiException) rethrow;
      throw ApiException('Network error: $e');
    }
  }
  
  // Implement post, put, delete similarly
}
```

3. **Connectivity Handling**
```dart
class ConnectivityService {
  static final _connectivity = Connectivity();
  
  static Future<bool> checkConnectivity() async {
    final result = await _connectivity.checkConnectivity();
    return result != ConnectivityResult.none;
  }
  
  static Stream<ConnectivityResult> get onConnectivityChanged =>
    _connectivity.onConnectivityChanged;
}

// Usage in API calls
Future<T> withConnectivity<T>(Future<T> Function() apiCall) async {
  if (!await ConnectivityService.checkConnectivity()) {
    throw ApiException('No internet connection');
  }
  return apiCall();
}
```

4. **Request Queue Management**
```dart
class RequestQueue {
  static final _queue = Queue<Future Function()>();
  static bool _processing = false;
  
  static Future<void> add(Future Function() request) async {
    _queue.add(request);
    if (!_processing) {
      _processing = true;
      await _processQueue();
    }
  }
  
  static Future<void> _processQueue() async {
    while (_queue.isNotEmpty) {
      final request = _queue.removeFirst();
      try {
        await request();
      } catch (e) {
        print('Error processing queued request: $e');
      }
    }
    _processing = false;
  }
}
```

5. **Loading State Management**
```dart
class LoadingState<T> {
  final T? data;
  final bool isLoading;
  final String? error;
  
  const LoadingState({
    this.data,
    this.isLoading = false,
    this.error,
  });
  
  LoadingState<T> copyWith({
    T? data,
    bool? isLoading,
    String? error,
  }) {
    return LoadingState(
      data: data ?? this.data,
      isLoading: isLoading ?? this.isLoading,
      error: error ?? this.error,
    );
  }
}

// Usage with StateNotifier
class ProjectsNotifier extends StateNotifier<LoadingState<List<Project>>> {
  final ProjectsRepository _repository;
  
  ProjectsNotifier(this._repository) : super(LoadingState());
  
  Future<void> loadProjects() async {
    state = state.copyWith(isLoading: true);
    try {
      final projects = await _repository.getProjects();
      state = state.copyWith(
        data: projects,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(
        error: e.toString(),
        isLoading: false,
      );
    }
  }
}
```
