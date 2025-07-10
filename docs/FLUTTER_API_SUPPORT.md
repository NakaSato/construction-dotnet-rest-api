# Flutter Mobile App API Support

The Solar Projects API now includes optimized endpoints specifically for Flutter mobile apps. These endpoints are designed to be lightweight, efficient, and mobile-friendly.

## Mobile-Specific Endpoints

### Projects

#### Get Mobile-Optimized Project List
```bash
curl http://localhost:5001/api/v1/projects/mobile \
  -H "Authorization: Bearer {your_token}" \
  -H "Accept: application/json"
```

Query parameters:
- `limit` - Maximum number of projects to return (default: 20)
- `status` - Filter by project status (e.g., "Active", "Completed")

#### Get Mobile Project Details
```bash
curl http://localhost:5001/api/v1/projects/mobile/{project_id} \
  -H "Authorization: Bearer {your_token}" \
  -H "Accept: application/json"
```

#### Get Mobile Dashboard Data
```bash
curl http://localhost:5001/api/v1/projects/mobile/dashboard \
  -H "Authorization: Bearer {your_token}" \
  -H "Accept: application/json"
```

Query parameters:
- `userId` - Optional user ID to filter dashboard for specific user

## Flutter Integration

### CORS Support

The API includes CORS support specifically configured for Flutter apps, allowing:
- Cross-origin requests
- Custom headers
- All HTTP methods
- Exposed pagination headers

### Response Format

All mobile endpoints return a consistent JSON format:

```json
{
  "success": true,
  "message": "Operation successful message",
  "data": {
    // The actual payload varies by endpoint
  }
}
```

### Optimizations

Mobile endpoints are optimized for Flutter apps with:

1. **Reduced payload size** - Only essential fields are included
2. **Short-term caching** - 5-minute cache to reduce API calls
3. **Efficient data structures** - Formatted for easy rendering in Flutter UI components
4. **Appropriate pagination** - Default limit of 20 items per request
5. **Thumbnail support** - Image URLs are included where relevant

## Setup for Flutter Developers

When implementing a Flutter app using this API:

1. Store the authentication token securely using `flutter_secure_storage`
2. Implement a simple API client using `http` or `dio` packages
3. For real-time updates, connect to the notification hub using SignalR
4. Consider implementing offline support with local SQLite database

### Example Flutter HTTP Client Usage

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class SolarApiClient {
  final String baseUrl = 'http://localhost:5001/api/v1';
  final String token;
  
  SolarApiClient(this.token);
  
  Future<Map<String, dynamic>> getMobileProjects() async {
    final response = await http.get(
      Uri.parse('$baseUrl/projects/mobile'),
      headers: {
        'Authorization': 'Bearer $token',
        'Accept': 'application/json',
      },
    );
    
    if (response.statusCode == 200) {
      return json.decode(response.body);
    } else {
      throw Exception('Failed to load projects');
    }
  }
}
```
