# Solar Projects API Documentation for Flutter

## Overview
Complete API documentation for the Solar Projects .NET REST API, designed for Flutter mobile app integration. This API uses JWT authentication with role-based access control (RBAC).

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
**Access**: All authenticated users

```dart
// Daily Report Model
class DailyReportDto {
  final String dailyReportId;
  final String projectId;
  final DateTime reportDate;
  final String? weatherConditions;
  final int? crewSize;
  final double? hoursWorked;
  final String? workCompleted;
  final String? issuesEncountered;
  final String? materialUsed;
  final String status;
  final UserDto? submittedByUser;

  DailyReportDto.fromJson(Map<String, dynamic> json)
    : dailyReportId = json['dailyReportId'],
      projectId = json['projectId'],
      reportDate = DateTime.parse(json['reportDate']),
      weatherConditions = json['weatherConditions'],
      crewSize = json['crewSize'],
      hoursWorked = json['hoursWorked']?.toDouble(),
      workCompleted = json['workCompleted'],
      issuesEncountered = json['issuesEncountered'],
      materialUsed = json['materialUsed'],
      status = json['status'],
      submittedByUser = json['submittedByUser'] != null 
          ? UserDto.fromJson(json['submittedByUser']) : null;
}

// Query Parameters
class DailyReportQueryParameters {
  final int pageNumber;
  final int pageSize;
  final String? projectId;
  final DateTime? reportDateAfter;
  final DateTime? reportDateBefore;
  final String? status;

  DailyReportQueryParameters({
    this.pageNumber = 1,
    this.pageSize = 10,
    this.projectId,
    this.reportDateAfter,
    this.reportDateBefore,
    this.status,
  });

  String toQueryString() {
    final params = <String, String>{
      'pageNumber': pageNumber.toString(),
      'pageSize': pageSize.toString(),
    };
    
    if (projectId != null) params['projectId'] = projectId!;
    if (reportDateAfter != null) params['reportDateAfter'] = reportDateAfter!.toIso8601String();
    if (reportDateBefore != null) params['reportDateBefore'] = reportDateBefore!.toIso8601String();
    if (status != null) params['status'] = status!;
    
    return params.entries.map((e) => '${e.key}=${Uri.encodeComponent(e.value)}').join('&');
  }
}

// Flutter Usage
Future<List<DailyReportDto>> getDailyReports({DailyReportQueryParameters? params}) async {
  try {
    String url = '${ApiConfig.apiBaseUrl}/daily-reports';
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
          .map((report) => DailyReportDto.fromJson(report))
          .toList();
    }
    return [];
  } catch (e) {
    print('Get daily reports error: $e');
    return [];
  }
}
```

### 12. Create Daily Report
**Endpoint**: `POST /api/v1/daily-reports`  
**Access**: Administrator, ProjectManager, Technician

```dart
// Request Model
class CreateDailyReportRequest {
  final String projectId;
  final DateTime reportDate;
  final String? weatherConditions;
  final int? crewSize;
  final double? hoursWorked;
  final String? workCompleted;
  final String? issuesEncountered;
  final String? materialUsed;
  final String submittedById;

  CreateDailyReportRequest({
    required this.projectId,
    required this.reportDate,
    this.weatherConditions,
    this.crewSize,
    this.hoursWorked,
    this.workCompleted,
    this.issuesEncountered,
    this.materialUsed,
    required this.submittedById,
  });

  Map<String, dynamic> toJson() => {
    'projectId': projectId,
    'reportDate': reportDate.toIso8601String(),
    'weatherConditions': weatherConditions,
    'crewSize': crewSize,
    'hoursWorked': hoursWorked,
    'workCompleted': workCompleted,
    'issuesEncountered': issuesEncountered,
    'materialUsed': materialUsed,
    'submittedById': submittedById,
  };
}

// Flutter Usage
Future<DailyReportDto?> createDailyReport(CreateDailyReportRequest request) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return DailyReportDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Create daily report error: $e');
    return null;
  }
}
```

### 13. Update Daily Report
**Endpoint**: `PUT /api/v1/daily-reports/{id}`  
**Access**: Administrator, ProjectManager, Technician

```dart
// Request Model (similar to Create but with optional fields)
class UpdateDailyReportRequest {
  final String? weatherConditions;
  final int? crewSize;
  final double? hoursWorked;
  final String? workCompleted;
  final String? issuesEncountered;
  final String? materialUsed;

  UpdateDailyReportRequest({
    this.weatherConditions,
    this.crewSize,
    this.hoursWorked,
    this.workCompleted,
    this.issuesEncountered,
    this.materialUsed,
  });

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (weatherConditions != null) map['weatherConditions'] = weatherConditions;
    if (crewSize != null) map['crewSize'] = crewSize;
    if (hoursWorked != null) map['hoursWorked'] = hoursWorked;
    if (workCompleted != null) map['workCompleted'] = workCompleted;
    if (issuesEncountered != null) map['issuesEncountered'] = issuesEncountered;
    if (materialUsed != null) map['materialUsed'] = materialUsed;
    return map;
  }
}

// Flutter Usage
Future<DailyReportDto?> updateDailyReport(String reportId, UpdateDailyReportRequest request) async {
  try {
    final response = await http.put(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$reportId'),
      headers: ApiClient.headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return DailyReportDto.fromJson(data['data']);
    }
    return null;
  } catch (e) {
    print('Update daily report error: $e');
    return null;
  }
}
```

### 14. Approve Daily Report
**Endpoint**: `POST /api/v1/daily-reports/{id}/approve`  
**Access**: Administrator, ProjectManager

```dart
Future<bool> approveDailyReport(String reportId) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$reportId/approve'),
      headers: ApiClient.headers,
    );

    return response.statusCode == 200;
  } catch (e) {
    print('Approve daily report error: $e');
    return false;
  }
}
```

### 15. Reject Daily Report
**Endpoint**: `POST /api/v1/daily-reports/{id}/reject`  
**Access**: Administrator, ProjectManager

```dart
Future<bool> rejectDailyReport(String reportId, String rejectionReason) async {
  try {
    final response = await http.post(
      Uri.parse('${ApiConfig.apiBaseUrl}/daily-reports/$reportId/reject'),
      headers: ApiClient.headers,
      body: jsonEncode({'rejectionReason': rejectionReason}),
    );

    return response.statusCode == 200;
  } catch (e) {
    print('Reject daily report error: $e');
    return false;
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

## Error Handling

### Standard Error Response
```dart
class ApiError {
  final String type;
  final String title;
  final int status;
  final String detail;
  final String? instance;
  final String? traceId;
  final List<ErrorDetail> errors;

  ApiError.fromJson(Map<String, dynamic> json)
    : type = json['type'],
      title = json['title'],
      status = json['status'],
      detail = json['detail'],
      instance = json['instance'],
      traceId = json['traceId'],
      errors = (json['errors'] as List?)
          ?.map((error) => ErrorDetail.fromJson(error))
          .toList() ?? [];
}

class ErrorDetail {
  final String code;
  final String message;
  final List<dynamic> details;

  ErrorDetail.fromJson(Map<String, dynamic> json)
    : code = json['code'],
      message = json['message'],
      details = json['details'] ?? [];
}
```

### Global Error Handler
```dart
class ApiErrorHandler {
  static String getErrorMessage(http.Response response) {
    try {
      final data = jsonDecode(response.body);
      
      if (data['message'] != null) {
        return data['message'];
      }
      
      if (data['errors'] != null && data['errors'].isNotEmpty) {
        return data['errors'].first;
      }
      
      return 'An unexpected error occurred';
    } catch (e) {
      return 'Network error occurred';
    }
  }
  
  static void handleError(http.Response response) {
    final message = getErrorMessage(response);
    
    switch (response.statusCode) {
      case 400:
        throw BadRequestException(message);
      case 401:
        throw UnauthorizedException(message);
      case 403:
        throw ForbiddenException(message);
      case 404:
        throw NotFoundException(message);
      case 429:
        throw RateLimitException(message);
      case 500:
        throw ServerException(message);
      default:
        throw ApiException(message);
    }
  }
}

// Custom Exceptions
class ApiException implements Exception {
  final String message;
  ApiException(this.message);
}

class BadRequestException extends ApiException {
  BadRequestException(String message) : super(message);
}

class UnauthorizedException extends ApiException {
  UnauthorizedException(String message) : super(message);
}

class ForbiddenException extends ApiException {
  ForbiddenException(String message) : super(message);
}

class NotFoundException extends ApiException {
  NotFoundException(String message) : super(message);
}

class RateLimitException extends ApiException {
  RateLimitException(String message) : super(message);
}

class ServerException extends ApiException {
  ServerException(String message) : super(message);
}
```

## Flutter Dependencies

Add these dependencies to your `pubspec.yaml`:

```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  shared_preferences: ^2.2.2  # For token storage
  provider: ^6.1.1  # For state management
  json_annotation: ^4.8.1

dev_dependencies:
  flutter_test:
    sdk: flutter
  build_runner: ^2.4.7
  json_serializable: ^6.7.1
```

## Usage Example

### Complete Flutter Integration Example
```dart
// main.dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

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
      ],
      child: MaterialApp(
        title: 'Solar Projects',
        home: LoginScreen(),
      ),
    );
  }
}

// auth_provider.dart
class AuthProvider extends ChangeNotifier {
  UserDto? _user;
  String? _token;

  UserDto? get user => _user;
  bool get isAuthenticated => _token != null;

  Future<bool> login(String username, String password) async {
    try {
      final loginResponse = await login(username, password);
      if (loginResponse != null) {
        _user = loginResponse.user;
        _token = loginResponse.token;
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      print('Login error: $e');
      return false;
    }
  }

  void logout() {
    _user = null;
    _token = null;
    ApiClient.clearAuthToken();
    notifyListeners();
  }
}

// project_provider.dart
class ProjectProvider extends ChangeNotifier {
  List<ProjectDto> _projects = [];
  bool _isLoading = false;

  List<ProjectDto> get projects => _projects;
  bool get isLoading => _isLoading;

  Future<void> loadProjects() async {
    _isLoading = true;
    notifyListeners();

    try {
      _projects = await getProjects();
    } catch (e) {
      print('Load projects error: $e');
    }

    _isLoading = false;
    notifyListeners();
  }

  Future<bool> createProject(CreateProjectRequest request) async {
    try {
      final project = await createProject(request);
      if (project != null) {
        _projects.add(project);
        notifyListeners();
        return true;
      }
      return false;
    } catch (e) {
      print('Create project error: $e');
      return false;
    }
  }
}
```

This documentation provides complete Flutter integration for all Solar Projects API endpoints with proper RBAC implementation, error handling, and real-world usage examples.
