# Error Handling

Error handling approach and common error codes for the Solar Projects API.

## API Response Format

All API responses follow a consistent format:

```json
{
  "success": true/false,
  "message": "Human-readable message",
  "data": { ... } or null,
  "errors": []
}
```

## HTTP Status Codes

| Code | Status | Meaning | Common Causes |
|------|--------|---------|--------------|
| 200 | OK | Successful request | GET, PUT, PATCH operations completed |
| 201 | Created | Resource created | POST operations completed |
| 400 | Bad Request | Invalid input | Missing fields, validation errors |
| 401 | Unauthorized | Authentication error | Missing/invalid token |
| 403 | Forbidden | Permission error | User lacks required role |
| 404 | Not Found | Resource not found | Invalid ID, wrong endpoint |
| 409 | Conflict | Resource conflict | Duplicate entry, concurrency issue |
| 422 | Unprocessable Entity | Validation error | Business rule violations |
| 429 | Too Many Requests | Rate limited | Exceeded API call limits |
| 500 | Server Error | Internal error | Unexpected server-side errors |

## Error Response Format

Error responses provide detailed information about what went wrong:

```json
{
  "success": false,
  "message": "Unable to create project",
  "data": null,
  "errors": [
    "Project name is required",
    "Total capacity must be greater than 0",
    "Start date cannot be in the past"
  ]
}
```

### Validation Errors (400)

```json
{
  "success": false,
  "message": "Validation failed",
  "data": {
    "validationErrors": {
      "projectName": ["The project name field is required"],
      "totalCapacityKw": ["Total capacity must be greater than 0"],
      "startDate": ["Start date cannot be in the past"]
    }
  },
  "errors": ["Multiple validation errors occurred"]
}
```

### 🚫 Permission Errors (403)

```json
{
  "success": false,
  "message": "Access denied",
  "data": {
    "requiredRole": "Admin",
    "userRole": "User",
    "operation": "Delete project"
  },
  "errors": ["User does not have permission to perform this operation"]
}
```

### 🔍 Not Found Errors (404)

```json
{
  "success": false,
  "message": "Resource not found",
  "data": {
    "resourceType": "Project",
    "resourceId": "123e4567-e89b-12d3-a456-426614174000"
  },
  "errors": ["The requested project could not be found"]
}
```

## 🔢 Error Code System

The API uses domain-specific error codes to provide more details about errors:

### Authentication Errors (AUTH)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **AUTH001** | Invalid credentials | 401 |
| **AUTH002** | Token expired | 401 |
| **AUTH003** | Invalid token format | 401 |
| **AUTH004** | Insufficient permissions | 403 |
| **AUTH005** | Weak password | 400 |
| **AUTH006** | Username/email exists | 409 |

### Project Errors (PRJ)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **PRJ001** | Project not found | 404 |
| **PRJ002** | Insufficient permissions | 403 |
| **PRJ003** | Invalid project data | 400 |
| **PRJ004** | Cannot delete active project | 422 |
| **PRJ005** | Project manager not found | 400 |
| **PRJ006** | Invalid status transition | 422 |
| **PRJ007** | Duplicate project name | 409 |

### Master Plan Errors (MP)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **MP001** | Master plan not found | 404 |
| **MP002** | Project already has a master plan | 409 |
| **MP003** | Invalid project ID | 400 |
| **MP004** | Insufficient permissions | 403 |
| **MP005** | Cannot delete plan with active tasks | 422 |
| **MP006** | Invalid status transition | 422 |

### Task Errors (TSK)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **TSK001** | Task not found | 404 |
| **TSK002** | Insufficient permissions | 403 |
| **TSK003** | Invalid task data | 400 |
| **TSK004** | Invalid status transition | 422 |
| **TSK005** | Assigned user not found | 400 |
| **TSK006** | Project not found | 400 |

### Daily Report Errors (DR)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **DR001** | Report not found | 404 |
| **DR002** | Insufficient permissions | 403 |
| **DR003** | Invalid report data | 400 |
| **DR004** | Cannot update after 24h | 422 |
| **DR005** | Project not found | 400 |
| **DR006** | File upload failed | 400 |

### Work Request Errors (WR)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **WR001** | Work request not found | 404 |
| **WR002** | Insufficient permissions | 403 |
| **WR003** | Invalid request data | 400 |
| **WR004** | Invalid status transition | 422 |
| **WR005** | Cannot modify submitted request | 422 |
| **WR006** | Cannot approve own request | 422 |
| **WR007** | Missing approval information | 400 |

### Calendar Event Errors (CAL)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **CAL001** | Event not found | 404 |
| **CAL002** | Insufficient permissions | 403 |
| **CAL003** | Invalid event data | 400 |
| **CAL004** | Date conflict | 409 |
| **CAL005** | Resource unavailable | 422 |

### System Errors (SYS)

| Code | Description | HTTP Status |
|------|-------------|------------|
| **SYS001** | Database error | 500 |
| **SYS002** | External service error | 502 |
| **SYS003** | Configuration error | 500 |
| **SYS004** | File system error | 500 |
| **SYS005** | Rate limit exceeded | 429 |

## 📋 Error Handling Best Practices

### Client-Side Error Handling

```dart
// Flutter example of proper error handling
Future<List<Project>> getProjects() async {
  try {
    final response = await _apiClient.get('/projects');
    
    if (response['success']) {
      return (response['data']['items'] as List)
        .map((item) => Project.fromJson(item))
        .toList();
    } else {
      // Handle anticipated errors
      final errors = response['errors'] as List<String>;
      throw ApiException(
        message: response['message'] ?? 'Failed to get projects',
        errors: errors,
      );
    }
  } on ApiException {
    // Re-throw API exceptions
    rethrow;
  } catch (e) {
    // Handle unexpected errors
    throw ApiException(
      message: 'An unexpected error occurred',
      errors: ['Could not complete request: ${e.toString()}'],
    );
  }
}
```

### Handling Specific Error Types

```dart
// Handle specific error codes
void handleProjectOperation(Function operation) async {
  try {
    await operation();
  } on ApiException catch (e) {
    if (e.errorCode == 'PRJ001') {
      // Handle project not found
      showNotFoundDialog();
    } else if (e.errorCode == 'PRJ004') {
      // Handle cannot delete active project
      showActiveProjectDialog();
    } else if (e.statusCode == 403) {
      // Handle permission errors
      showPermissionErrorDialog();
    } else {
      // Handle other API errors
      showErrorDialog(e.message);
    }
  } catch (e) {
    // Handle unexpected errors
    showErrorDialog('An unexpected error occurred');
    logError(e);
  }
}
```

## 🔄 Retry Strategies

For transient errors (network issues, timeouts, 5xx errors), implement retries with exponential backoff:

```dart
Future<T> retryOperation<T>(Future<T> Function() operation) async {
  int attempts = 0;
  const maxAttempts = 3;
  
  while (attempts < maxAttempts) {
    try {
      return await operation();
    } catch (e) {
      attempts++;
      
      if (attempts >= maxAttempts) rethrow;
      
      // Only retry on transient errors like network or 5xx
      if (e is ApiException && e.statusCode < 500) rethrow;
      
      // Exponential backoff
      final waitTime = Duration(milliseconds: 200 * pow(2, attempts));
      await Future.delayed(waitTime);
    }
  }
  
  throw Exception('This should never happen');
}
```

## 📱 Error UI Guidelines

- **Toast Messages**: For minor errors that don't interrupt workflow
- **Error Dialogs**: For errors requiring user attention
- **Error Pages**: For critical errors preventing operation
- **Inline Validation**: For form field validation errors
- **Status Indicators**: For ongoing operation status

---
*Last Updated: July 4, 2025*
