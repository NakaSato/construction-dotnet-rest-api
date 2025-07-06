# API Test Files

HTTP test files for the Solar Projects .NET REST API, organized by feature.

## Test Files

```
tests/http/
├── auth.http                 # Authentication & Registration
├── users.http               # User Management
├── projects.http            # Project Management  
├── tasks.http               # Task Management
├── daily-reports.http       # Daily Reports
├── work-requests.http       # Work Request Workflow
├── master-plans.http        # Master Plans & Phases
├── wbs.http                 # Work Breakdown Structure
├── calendar.http            # Calendar Events & Scheduling
├── dashboard.http           # Dashboard & Analytics
├── notifications.http       # Notifications & Messaging
└── utilities.http           # Health Check, Debug & Utilities
```

## Quick Start

1. **Start the server**:
   ```bash
   dotnet run --urls "http://localhost:5001"
   ```

2. **Get authentication tokens**:
   - Open `auth.http`
   - Run registration test to create a user
   - Run login test to get JWT token
   - Copy token to other test files

3. **Update variables** in each test file:
   - Replace `YOUR_JWT_TOKEN_HERE` with actual JWT tokens
   - Replace ID placeholders with actual GUIDs

## Usage

### VS Code REST Client
1. Install "REST Client" extension
2. Open any `.http` file
3. Click "Send Request" above HTTP requests
4. View responses in split panel

### cURL Command Line
```bash
curl -X POST "http://localhost:5001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "email": "test@example.com", "password": "SecurePass123!", "fullName": "Test User", "roleId": 3}'
```

## Common Variables

Update these in each test file:
```http
@baseUrl = http://localhost:5001
@contentType = application/json
@authToken = YOUR_JWT_TOKEN_HERE
@adminToken = YOUR_ADMIN_JWT_TOKEN_HERE
@projectId = YOUR_PROJECT_ID_HERE
@userId = YOUR_USER_ID_HERE
```

## Test Order

1. **auth.http** - Get authentication tokens
2. **users.http** - Create test users
3. **projects.http** - Create test projects
4. **Other files** - Run remaining tests

## Troubleshooting

- **401 Unauthorized**: Update expired JWT tokens
- **404 Not Found**: Check server is running on port 5001
- **400 Bad Request**: Verify request body format
- **Variable not found**: Update placeholder variables with actual values
