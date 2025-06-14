# 📱 Flutter API Integration Guide

## 🚀 Complete Solar Projects API Guide for Flutter

This comprehensive guide provides everything you need to integrate the Solar Projects REST API into your Flutter mobile application.

## 📦 Required Dependencies

Add these to your `pubspec.yaml`:

```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0                    # HTTP client
  dio: ^5.3.0                     # Advanced HTTP client (alternative)
  flutter_secure_storage: ^9.0.0 # Secure token storage
  image_picker: ^1.0.4            # Camera/gallery access
  geolocator: ^10.1.0            # GPS location
  cached_network_image: ^3.3.0   # Image caching
  connectivity_plus: ^5.0.1      # Network connectivity
  hive: ^2.2.3                   # Local database
  riverpod: ^2.4.0               # State management
  go_router: ^12.1.1             # Navigation
  
dev_dependencies:
  flutter_launcher_icons: ^0.13.1 # App icons
  flutter_native_splash: ^2.3.2   # Splash screen
```

## 🔐 Authentication Service

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  static const String baseUrl = 'http://localhost:5002/api/v1';
  static const storage = FlutterSecureStorage();

  // Login with username or email
  static Future<Map<String, dynamic>> login(String usernameOrEmail, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'username': usernameOrEmail,  // Can be username or email
          'password': password,
        }),
      );

      final data = jsonDecode(response.body);
      
      if (response.statusCode == 200 && data['success']) {
        // Store tokens securely
        await storage.write(key: 'jwt_token', value: data['data']['token']);
        await storage.write(key: 'refresh_token', value: data['data']['refreshToken']);
        await storage.write(key: 'user_data', value: jsonEncode(data['data']['user']));
        
        return {'success': true, 'user': data['data']['user']};
      } else {
        return {'success': false, 'message': data['message']};
      }
    } catch (e) {
      return {'success': false, 'message': 'Network error: $e'};
    }
  }

  // Get stored token
  static Future<String?> getToken() async {
    return await storage.read(key: 'jwt_token');
  }

  // Get stored user data
  static Future<Map<String, dynamic>?> getUserData() async {
    final userData = await storage.read(key: 'user_data');
    if (userData != null) {
      return jsonDecode(userData);
    }
    return null;
  }

  // Refresh token
  static Future<Map<String, dynamic>> refreshToken() async {
    try {
      final refreshToken = await storage.read(key: 'refresh_token');
      if (refreshToken == null) {
        return {'success': false, 'message': 'No refresh token available'};
      }

      final response = await http.post(
        Uri.parse('$baseUrl/auth/refresh'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode(refreshToken),
      );

      final data = jsonDecode(response.body);
      
      if (response.statusCode == 200 && data['success']) {
        await storage.write(key: 'jwt_token', value: data['data']);
        return {'success': true};
      } else {
        return {'success': false, 'message': data['message']};
      }
    } catch (e) {
      return {'success': false, 'message': 'Error refreshing token: $e'};
    }
  }

  // Logout
  static Future<void> logout() async {
    await storage.deleteAll();
  }

  // Check if user is authenticated
  static Future<bool> isAuthenticated() async {
    final token = await getToken();
    return token != null;
  }
}
```

## 📊 Daily Reports Service

```dart
class DailyReportsService {
  static const String baseUrl = 'http://localhost:5002/api/v1';

  // Get daily reports with pagination and filtering
  static Future<Map<String, dynamic>> getDailyReports({
    int pageNumber = 1,
    int pageSize = 10,
    String? projectId,
    String? reporterId,
    String? reportDate,
    String? reportDateFrom,
    String? reportDateTo,
  }) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      var queryParams = {
        'pageNumber': pageNumber.toString(),
        'pageSize': pageSize.toString(),
      };
      
      if (projectId != null) queryParams['projectId'] = projectId;
      if (reporterId != null) queryParams['reporterId'] = reporterId;
      if (reportDate != null) queryParams['reportDate'] = reportDate;
      if (reportDateFrom != null) queryParams['reportDateFrom'] = reportDateFrom;
      if (reportDateTo != null) queryParams['reportDateTo'] = reportDateTo;

      final uri = Uri.parse('$baseUrl/daily-reports').replace(queryParameters: queryParams);
      
      final response = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  // Create daily report
  static Future<Map<String, dynamic>> createDailyReport(Map<String, dynamic> reportData) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final response = await http.post(
        Uri.parse('$baseUrl/daily-reports'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(reportData),
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  // Get specific daily report
  static Future<Map<String, dynamic>> getDailyReport(String reportId) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final response = await http.get(
        Uri.parse('$baseUrl/daily-reports/$reportId'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  // Update daily report
  static Future<Map<String, dynamic>> updateDailyReport(String reportId, Map<String, dynamic> reportData) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final response = await http.put(
        Uri.parse('$baseUrl/daily-reports/$reportId'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(reportData),
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }
}
```

## 📷 Image Upload Service

```dart
import 'package:image_picker/image_picker.dart';
import 'dart:io';

class ImageService {
  static const String baseUrl = 'http://localhost:5002/api/v1';

  // Upload image with GPS and device info
  static Future<Map<String, dynamic>> uploadImage({
    required File imageFile,
    required String caption,
    required String category, // 'DailyReport', 'WorkProgress', 'Safety', 'Documentation'
    Map<String, double>? gpsCoordinates,
    Map<String, String>? deviceInfo,
  }) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      var request = http.MultipartRequest('POST', Uri.parse('$baseUrl/images/upload'));
      
      // Add authorization header
      request.headers['Authorization'] = 'Bearer $token';
      
      // Add image file
      request.files.add(await http.MultipartFile.fromPath('file', imageFile.path));
      
      // Add text fields
      request.fields['caption'] = caption;
      request.fields['category'] = category;
      
      if (gpsCoordinates != null) {
        request.fields['gpsCoordinates'] = jsonEncode(gpsCoordinates);
      }
      
      if (deviceInfo != null) {
        request.fields['deviceInfo'] = jsonEncode(deviceInfo);
      }

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);
      
      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Upload error: $e'};
    }
  }

  // Pick image from camera or gallery
  static Future<File?> pickImage({required ImageSource source}) async {
    try {
      final ImagePicker picker = ImagePicker();
      final XFile? image = await picker.pickImage(
        source: source,
        maxWidth: 1920,
        maxHeight: 1080,
        imageQuality: 85,
      );
      
      if (image != null) {
        return File(image.path);
      }
      return null;
    } catch (e) {
      print('Error picking image: $e');
      return null;
    }
  }

  // Get image metadata
  static Future<Map<String, dynamic>> getImageMetadata(String imageId) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final response = await http.get(
        Uri.parse('$baseUrl/images/$imageId/metadata'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }
}
```

## 🌍 Location & Device Services

```dart
import 'package:geolocator/geolocator.dart';
import 'dart:io';
import 'package:device_info_plus/device_info_plus.dart';

class LocationService {
  // Get current GPS coordinates
  static Future<Map<String, double>?> getCurrentLocation() async {
    try {
      // Check if location services are enabled
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        return null;
      }

      // Check permissions
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          return null;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        return null;
      }

      // Get position
      Position position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
        timeLimit: Duration(seconds: 10),
      );

      return {
        'latitude': position.latitude,
        'longitude': position.longitude,
        'accuracy': position.accuracy,
      };
    } catch (e) {
      print('Error getting location: $e');
      return null;
    }
  }

  // Get device information
  static Future<Map<String, String>> getDeviceInfo() async {
    DeviceInfoPlugin deviceInfo = DeviceInfoPlugin();
    
    try {
      if (Platform.isAndroid) {
        AndroidDeviceInfo androidInfo = await deviceInfo.androidInfo;
        return {
          'deviceType': 'Mobile',
          'operatingSystem': 'Android ${androidInfo.version.release}',
          'model': androidInfo.model,
          'manufacturer': androidInfo.manufacturer,
          'appVersion': '1.0.0', // Get from package_info_plus
        };
      } else if (Platform.isIOS) {
        IosDeviceInfo iosInfo = await deviceInfo.iosInfo;
        return {
          'deviceType': 'Mobile',
          'operatingSystem': 'iOS ${iosInfo.systemVersion}',
          'model': iosInfo.model,
          'manufacturer': 'Apple',
          'appVersion': '1.0.0', // Get from package_info_plus
        };
      }
    } catch (e) {
      print('Error getting device info: $e');
    }

    return {
      'deviceType': 'Mobile',
      'operatingSystem': Platform.isIOS ? 'iOS' : 'Android',
      'appVersion': '1.0.0',
    };
  }
}
```

## 📋 Projects & Tasks Service

```dart
class ProjectsService {
  static const String baseUrl = 'http://localhost:5002/api/v1';

  // Get all projects
  static Future<Map<String, dynamic>> getProjects({
    int pageNumber = 1,
    int pageSize = 10,
    String? searchTerm,
    String? status,
    String? projectManagerId,
  }) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      var queryParams = {
        'pageNumber': pageNumber.toString(),
        'pageSize': pageSize.toString(),
      };
      
      if (searchTerm != null) queryParams['searchTerm'] = searchTerm;
      if (status != null) queryParams['status'] = status;
      if (projectManagerId != null) queryParams['projectManagerId'] = projectManagerId;

      final uri = Uri.parse('$baseUrl/projects').replace(queryParameters: queryParams);
      
      final response = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  // Get project by ID
  static Future<Map<String, dynamic>> getProject(String projectId) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final response = await http.get(
        Uri.parse('$baseUrl/projects/$projectId'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }
}

class TasksService {
  static const String baseUrl = 'http://localhost:5002/api/v1';

  // Get my tasks (assigned to current user)
  static Future<Map<String, dynamic>> getMyTasks({
    String? status,
    String? priority,
    int pageSize = 20,
  }) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      var queryParams = {
        'pageSize': pageSize.toString(),
      };
      
      if (status != null) queryParams['status'] = status;
      if (priority != null) queryParams['priority'] = priority;

      final uri = Uri.parse('$baseUrl/tasks/my-tasks').replace(queryParameters: queryParams);
      
      final response = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  // Update task status
  static Future<Map<String, dynamic>> updateTaskStatus(
    String taskId,
    String status, {
    double? actualHours,
    String? notes,
  }) async {
    try {
      final token = await AuthService.getToken();
      if (token == null) return {'success': false, 'message': 'Not authenticated'};

      final requestBody = <String, dynamic>{
        'status': status,
      };
      
      if (actualHours != null) requestBody['actualHours'] = actualHours;
      if (notes != null) requestBody['notes'] = notes;

      final response = await http.patch(
        Uri.parse('$baseUrl/tasks/$taskId/status'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(requestBody),
      );

      return jsonDecode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }
}
```

## 🔄 API Response Models

```dart
class ApiResponse<T> {
  final bool success;
  final String message;
  final T? data;
  final List<String> errors;

  ApiResponse({
    required this.success,
    required this.message,
    this.data,
    this.errors = const [],
  });

  factory ApiResponse.fromJson(Map<String, dynamic> json, T Function(dynamic)? fromJsonT) {
    return ApiResponse<T>(
      success: json['success'] ?? false,
      message: json['message'] ?? '',
      data: json['data'] != null && fromJsonT != null ? fromJsonT(json['data']) : json['data'],
      errors: List<String>.from(json['errors'] ?? []),
    );
  }
}

class DailyReport {
  final String reportId;
  final String reportDate;
  final String projectId;
  final String projectName;
  final String reporterId;
  final String reporterName;
  final String workPerformed;
  final double hoursWorked;
  final String weatherCondition;
  final int? temperature;
  final bool safetyIncident;
  final String? safetyNotes;
  final String? equipmentUsed;
  final String? materialsUsed;
  final String? issuesEncountered;
  final String? nextDayPlans;
  final Map<String, double>? gpsCoordinates;
  final DateTime submittedAt;

  DailyReport({
    required this.reportId,
    required this.reportDate,
    required this.projectId,
    required this.projectName,
    required this.reporterId,
    required this.reporterName,
    required this.workPerformed,
    required this.hoursWorked,
    required this.weatherCondition,
    this.temperature,
    required this.safetyIncident,
    this.safetyNotes,
    this.equipmentUsed,
    this.materialsUsed,
    this.issuesEncountered,
    this.nextDayPlans,
    this.gpsCoordinates,
    required this.submittedAt,
  });

  factory DailyReport.fromJson(Map<String, dynamic> json) {
    return DailyReport(
      reportId: json['reportId'],
      reportDate: json['reportDate'],
      projectId: json['projectId'],
      projectName: json['projectName'],
      reporterId: json['reporterId'],
      reporterName: json['reporterName'],
      workPerformed: json['workPerformed'],
      hoursWorked: (json['hoursWorked'] as num).toDouble(),
      weatherCondition: json['weatherCondition'],
      temperature: json['temperature'],
      safetyIncident: json['safetyIncident'],
      safetyNotes: json['safetyNotes'],
      equipmentUsed: json['equipmentUsed'],
      materialsUsed: json['materialsUsed'],
      issuesEncountered: json['issuesEncountered'],
      nextDayPlans: json['nextDayPlans'],
      gpsCoordinates: json['gpsCoordinates'] != null 
          ? Map<String, double>.from(json['gpsCoordinates'])
          : null,
      submittedAt: DateTime.parse(json['submittedAt']),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'reportDate': reportDate,
      'projectId': projectId,
      'workPerformed': workPerformed,
      'hoursWorked': hoursWorked,
      'weatherCondition': weatherCondition,
      'temperature': temperature,
      'safetyIncident': safetyIncident,
      'safetyNotes': safetyNotes,
      'equipmentUsed': equipmentUsed,
      'materialsUsed': materialsUsed,
      'issuesEncountered': issuesEncountered,
      'nextDayPlans': nextDayPlans,
      'gpsCoordinates': gpsCoordinates,
    };
  }
}

class Project {
  final String projectId;
  final String name;
  final String description;
  final DateTime startDate;
  final DateTime endDate;
  final String status;
  final String projectManagerId;
  final String projectManagerName;
  final DateTime createdAt;
  final int? tasksCount;
  final int? completedTasksCount;
  final double? progressPercentage;

  Project({
    required this.projectId,
    required this.name,
    required this.description,
    required this.startDate,
    required this.endDate,
    required this.status,
    required this.projectManagerId,
    required this.projectManagerName,
    required this.createdAt,
    this.tasksCount,
    this.completedTasksCount,
    this.progressPercentage,
  });

  factory Project.fromJson(Map<String, dynamic> json) {
    return Project(
      projectId: json['projectId'],
      name: json['name'],
      description: json['description'],
      startDate: DateTime.parse(json['startDate']),
      endDate: DateTime.parse(json['endDate']),
      status: json['status'],
      projectManagerId: json['projectManagerId'],
      projectManagerName: json['projectManagerName'],
      createdAt: DateTime.parse(json['createdAt']),
      tasksCount: json['tasksCount'],
      completedTasksCount: json['completedTasksCount'],
      progressPercentage: json['progressPercentage']?.toDouble(),
    );
  }
}

class ProjectTask {
  final String taskId;
  final String title;
  final String description;
  final String status;
  final String priority;
  final String projectId;
  final String? projectName;
  final String? assignedTechnicianId;
  final String? assignedTechnicianName;
  final DateTime? dueDate;
  final double? estimatedHours;
  final double? actualHours;
  final int? completionPercentage;
  final DateTime createdAt;

  ProjectTask({
    required this.taskId,
    required this.title,
    required this.description,
    required this.status,
    required this.priority,
    required this.projectId,
    this.projectName,
    this.assignedTechnicianId,
    this.assignedTechnicianName,
    this.dueDate,
    this.estimatedHours,
    this.actualHours,
    this.completionPercentage,
    required this.createdAt,
  });

  factory ProjectTask.fromJson(Map<String, dynamic> json) {
    return ProjectTask(
      taskId: json['taskId'],
      title: json['title'],
      description: json['description'],
      status: json['status'],
      priority: json['priority'],
      projectId: json['projectId'],
      projectName: json['projectName'],
      assignedTechnicianId: json['assignedTechnicianId'],
      assignedTechnicianName: json['assignedTechnicianName'],
      dueDate: json['dueDate'] != null ? DateTime.parse(json['dueDate']) : null,
      estimatedHours: json['estimatedHours']?.toDouble(),
      actualHours: json['actualHours']?.toDouble(),
      completionPercentage: json['completionPercentage'],
      createdAt: DateTime.parse(json['createdAt']),
    );
  }
}
```

## 📱 Complete Flutter App Example

```dart
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';

class DailyReportScreen extends StatefulWidget {
  final String projectId;
  final String projectName;

  const DailyReportScreen({
    Key? key,
    required this.projectId,
    required this.projectName,
  }) : super(key: key);

  @override
  _DailyReportScreenState createState() => _DailyReportScreenState();
}

class _DailyReportScreenState extends State<DailyReportScreen> {
  final _formKey = GlobalKey<FormState>();
  final TextEditingController _workController = TextEditingController();
  final TextEditingController _hoursController = TextEditingController();
  final TextEditingController _equipmentController = TextEditingController();
  final TextEditingController _materialsController = TextEditingController();
  final TextEditingController _issuesController = TextEditingController();
  final TextEditingController _nextDayController = TextEditingController();
  final TextEditingController _safetyNotesController = TextEditingController();
  
  bool _isLoading = false;
  bool _safetyIncident = false;
  String _weatherCondition = 'Sunny';
  int _temperature = 75;
  List<File> _selectedImages = [];
  Map<String, double>? _currentLocation;

  final List<String> _weatherOptions = [
    'Sunny', 'Cloudy', 'Rainy', 'Windy', 'Overcast', 'Partly Cloudy'
  ];

  @override
  void initState() {
    super.initState();
    _getCurrentLocation();
  }

  Future<void> _getCurrentLocation() async {
    final location = await LocationService.getCurrentLocation();
    setState(() {
      _currentLocation = location;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Daily Report'),
        backgroundColor: Colors.orange,
        elevation: 0,
      ),
      body: Form(
        key: _formKey,
        child: SingleChildScrollView(
          padding: EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Project info card
              Card(
                elevation: 2,
                child: Padding(
                  padding: EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Project Information',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      SizedBox(height: 8),
                      Text('Project: ${widget.projectName}'),
                      Text('Date: ${DateTime.now().toString().split(' ')[0]}'),
                      if (_currentLocation != null)
                        Text('Location: ${_currentLocation!['latitude']!.toStringAsFixed(4)}, ${_currentLocation!['longitude']!.toStringAsFixed(4)}'),
                    ],
                  ),
                ),
              ),
              SizedBox(height: 16),

              // Work performed
              TextFormField(
                controller: _workController,
                maxLines: 4,
                decoration: InputDecoration(
                  labelText: 'Work Performed *',
                  hintText: 'Describe the work completed today...',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please describe the work performed';
                  }
                  return null;
                },
              ),
              SizedBox(height: 16),

              // Hours worked
              TextFormField(
                controller: _hoursController,
                keyboardType: TextInputType.numberWithOptions(decimal: true),
                decoration: InputDecoration(
                  labelText: 'Hours Worked *',
                  hintText: '8.0',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return 'Please enter hours worked';
                  }
                  final hours = double.tryParse(value);
                  if (hours == null || hours <= 0) {
                    return 'Please enter a valid number of hours';
                  }
                  return null;
                },
              ),
              SizedBox(height: 16),

              // Weather conditions
              Row(
                children: [
                  Expanded(
                    child: DropdownButtonFormField<String>(
                      value: _weatherCondition,
                      decoration: InputDecoration(
                        labelText: 'Weather Condition',
                        border: OutlineInputBorder(),
                      ),
                      items: _weatherOptions.map((weather) {
                        return DropdownMenuItem(
                          value: weather,
                          child: Text(weather),
                        );
                      }).toList(),
                      onChanged: (value) {
                        setState(() {
                          _weatherCondition = value!;
                        });
                      },
                    ),
                  ),
                  SizedBox(width: 16),
                  Expanded(
                    child: TextFormField(
                      initialValue: _temperature.toString(),
                      keyboardType: TextInputType.number,
                      decoration: InputDecoration(
                        labelText: 'Temperature (°F)',
                        border: OutlineInputBorder(),
                      ),
                      onChanged: (value) {
                        final temp = int.tryParse(value);
                        if (temp != null) {
                          _temperature = temp;
                        }
                      },
                    ),
                  ),
                ],
              ),
              SizedBox(height: 16),

              // Equipment used
              TextFormField(
                controller: _equipmentController,
                maxLines: 2,
                decoration: InputDecoration(
                  labelText: 'Equipment Used',
                  hintText: 'List tools and equipment used...',
                  border: OutlineInputBorder(),
                ),
              ),
              SizedBox(height: 16),

              // Materials used
              TextFormField(
                controller: _materialsController,
                maxLines: 2,
                decoration: InputDecoration(
                  labelText: 'Materials Used',
                  hintText: 'List materials and quantities...',
                  border: OutlineInputBorder(),
                ),
              ),
              SizedBox(height: 16),

              // Issues encountered
              TextFormField(
                controller: _issuesController,
                maxLines: 3,
                decoration: InputDecoration(
                  labelText: 'Issues Encountered',
                  hintText: 'Describe any problems or challenges...',
                  border: OutlineInputBorder(),
                ),
              ),
              SizedBox(height: 16),

              // Next day plans
              TextFormField(
                controller: _nextDayController,
                maxLines: 2,
                decoration: InputDecoration(
                  labelText: 'Next Day Plans',
                  hintText: 'What work is planned for tomorrow...',
                  border: OutlineInputBorder(),
                ),
              ),
              SizedBox(height: 16),

              // Safety incident
              CheckboxListTile(
                title: Text('Safety Incident Occurred'),
                subtitle: Text('Check if any safety incidents happened today'),
                value: _safetyIncident,
                onChanged: (value) {
                  setState(() {
                    _safetyIncident = value!;
                  });
                },
                controlAffinity: ListTileControlAffinity.leading,
              ),

              if (_safetyIncident) ...[
                SizedBox(height: 8),
                TextFormField(
                  controller: _safetyNotesController,
                  maxLines: 3,
                  decoration: InputDecoration(
                    labelText: 'Safety Notes *',
                    hintText: 'Describe the safety incident in detail...',
                    border: OutlineInputBorder(),
                  ),
                  validator: (value) {
                    if (_safetyIncident && (value == null || value.isEmpty)) {
                      return 'Please provide details about the safety incident';
                    }
                    return null;
                  },
                ),
              ],

              SizedBox(height: 16),

              // Photo section
              Text(
                'Photos',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              SizedBox(height: 8),
              Row(
                children: [
                  ElevatedButton.icon(
                    onPressed: () => _pickImage(ImageSource.camera),
                    icon: Icon(Icons.camera_alt),
                    label: Text('Camera'),
                    style: ElevatedButton.styleFrom(backgroundColor: Colors.blue),
                  ),
                  SizedBox(width: 8),
                  ElevatedButton.icon(
                    onPressed: () => _pickImage(ImageSource.gallery),
                    icon: Icon(Icons.photo_library),
                    label: Text('Gallery'),
                    style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
                  ),
                ],
              ),
              SizedBox(height: 16),

              // Selected images
              if (_selectedImages.isNotEmpty) ...[
                Text('Selected Photos (${_selectedImages.length})'),
                SizedBox(height: 8),
                Container(
                  height: 120,
                  child: ListView.builder(
                    scrollDirection: Axis.horizontal,
                    itemCount: _selectedImages.length,
                    itemBuilder: (context, index) {
                      return Padding(
                        padding: EdgeInsets.only(right: 8),
                        child: Stack(
                          children: [
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Image.file(
                                _selectedImages[index],
                                width: 120,
                                height: 120,
                                fit: BoxFit.cover,
                              ),
                            ),
                            Positioned(
                              top: 4,
                              right: 4,
                              child: GestureDetector(
                                onTap: () => _removeImage(index),
                                child: Container(
                                  decoration: BoxDecoration(
                                    color: Colors.red,
                                    shape: BoxShape.circle,
                                  ),
                                  child: Icon(
                                    Icons.close,
                                    color: Colors.white,
                                    size: 20,
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
                ),
                SizedBox(height: 16),
              ],

              // Submit button
              SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _submitReport,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.orange,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                  child: _isLoading
                      ? CircularProgressIndicator(color: Colors.white)
                      : Text(
                          'Submit Daily Report',
                          style: TextStyle(fontSize: 18, color: Colors.white),
                        ),
                ),
              ),
              SizedBox(height: 16),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _pickImage(ImageSource source) async {
    final image = await ImageService.pickImage(source: source);
    if (image != null) {
      setState(() {
        _selectedImages.add(image);
      });
    }
  }

  void _removeImage(int index) {
    setState(() {
      _selectedImages.removeAt(index);
    });
  }

  Future<void> _submitReport() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isLoading = true);

    try {
      // Get device info
      final deviceInfo = await LocationService.getDeviceInfo();
      
      // Create report data
      final reportData = {
        'reportDate': DateTime.now().toIso8601String().split('T')[0],
        'projectId': widget.projectId,
        'workPerformed': _workController.text,
        'hoursWorked': double.parse(_hoursController.text),
        'weatherCondition': _weatherCondition,
        'temperature': _temperature,
        'safetyIncident': _safetyIncident,
        'safetyNotes': _safetyIncident ? _safetyNotesController.text : null,
        'equipmentUsed': _equipmentController.text.isNotEmpty ? _equipmentController.text : null,
        'materialsUsed': _materialsController.text.isNotEmpty ? _materialsController.text : null,
        'issuesEncountered': _issuesController.text.isNotEmpty ? _issuesController.text : null,
        'nextDayPlans': _nextDayController.text.isNotEmpty ? _nextDayController.text : null,
        'gpsCoordinates': _currentLocation,
        'deviceInfo': deviceInfo,
      };

      // Submit report
      final result = await DailyReportsService.createDailyReport(reportData);
      
      if (result['success']) {
        // Upload images if any
        for (final image in _selectedImages) {
          await ImageService.uploadImage(
            imageFile: image,
            caption: 'Daily report photo - ${widget.projectName}',
            category: 'DailyReport',
            gpsCoordinates: _currentLocation,
            deviceInfo: deviceInfo,
          );
        }
        
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Daily report submitted successfully!'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context, true); // Return true to indicate success
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error: ${result['message']}'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Error submitting report: $e'),
          backgroundColor: Colors.red,
        ),
      );
    }

    setState(() => _isLoading = false);
  }

  @override
  void dispose() {
    _workController.dispose();
    _hoursController.dispose();
    _equipmentController.dispose();
    _materialsController.dispose();
    _issuesController.dispose();
    _nextDayController.dispose();
    _safetyNotesController.dispose();
    super.dispose();
  }
}
```

## 🎯 Test Accounts for Development

Use these test accounts in your Flutter app:

| Role | Username | Password | Email | Use Case |
|------|----------|----------|-------|----------|
| **Admin** | `test_admin` | `Admin123!` | `test_admin@example.com` | Full access testing |
| **Manager** | `test_manager` | `Manager123!` | `test_manager@example.com` | Project management |
| **User** | `test_user` | `User123!` | `test_user@example.com` | Field technician |
| **Viewer** | `test_viewer` | `Viewer123!` | `test_viewer@example.com` | Read-only access |

## 🔒 Security Best Practices

1. **Secure Storage**: Always use `flutter_secure_storage` for tokens
2. **Token Refresh**: Implement automatic token refresh logic
3. **Network Security**: Use HTTPS in production, validate certificates
4. **Data Validation**: Validate all user inputs before sending to API
5. **Error Handling**: Never expose sensitive error details to users
6. **Logout**: Clear all stored data on logout

## 📱 Production Configuration

### Android Permissions (android/app/src/main/AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
```

### iOS Permissions (ios/Runner/Info.plist)
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs camera access to take photos for daily reports</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs location access to tag photos with GPS coordinates</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs photo library access to attach images to reports</string>
```

This comprehensive guide provides everything you need to build a professional Flutter app that integrates seamlessly with the Solar Projects API! 🌞📱
