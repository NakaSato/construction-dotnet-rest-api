# Authentication & User Management

Authentication endpoints, user registration, and role-based access control for the Solar Project Management API.

## Authentication Overview

All API endpoints require JWT authentication:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Security Features
- JWT token authentication
- Refresh token rotation
- Role-based access control (RBAC)
- Password security policies
- Session management
- Audit logging

## User Roles & Permissions

| Role ID | Role Name | Project Access | Description |
|---------|-----------|----------------|-------------|
| `1` | Admin | Full CRUD + Delete | Complete system access |
| `2` | Manager | Full CRUD | Project management |
| `3` | User | Read + Own Reports | Field technician access |
| `4` | Viewer | Read Only | Client/reporting access |

### Permission Matrix

| Action | Admin | Manager | User | Viewer |
|--------|-------|---------|------|--------|
| Projects | Full CRUD + Delete | Full CRUD | Read Only | Read Only |
| Master Plans | Full CRUD + Delete | Full CRUD | Read Only | Read Only |
| Tasks | Full CRUD + Delete | Full CRUD | Read + Own CRUD | Read Only |
| Daily Reports | Full CRUD + Delete | Full CRUD + Approve | Own CRUD | Read Only |
| Work Requests | Full CRUD + Delete | Full CRUD + Approve | Own CRUD | Read Only |
| Users | Full CRUD | Read + Team Management | Read Own Profile | Read Own Profile |

### Test Accounts

| Username | Email | Password | Role |
|----------|-------|----------|------|
| `test_admin` | `test_admin@example.com` | `Admin123!` | Admin |
| `test_manager` | `test_manager@example.com` | `Manager123!` | Manager |
| `test_user` | `test_user@example.com` | `User123!` | User |
| `test_viewer` | `test_viewer@example.com` | `Viewer123!` | Viewer |

## Login

**POST** `/api/v1/auth/login`

### Request

```json
// Option 1: Login with username
{
  "username": "test_admin",
  "password": "Admin123!"
}
```

```json
// Option 2: Login with email
{
  "username": "test_admin@example.com",
  "password": "Admin123!"
}
```

### Success Response (200)

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

### Error Response (401)

```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": ["Authentication failed"]
}
```

## Register New User

**POST** `/api/v1/auth/register`

### Request Body

```json
{
  "username": "john_tech",
  "email": "john@solartech.com",
  "password": "SecurePass123!",
  "fullName": "John Technician",
  "roleId": 3
}
```

### Success Response (201)

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

### Error Response (400)

```json
{
  "success": false,
  "message": "Registration failed",
  "data": null,
  "errors": [
    "Username already exists",
    "Password must contain at least one uppercase letter",
    "Email format is invalid"
  ]
}
```

## Token Refresh
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
              if (value.length < 3) {
                return 'Username must be at least 3 characters';
              }
              return null;
            },
          ),
          TextFormField(
            controller: _emailController,
            decoration: InputDecoration(labelText: 'Email'),
            keyboardType: TextInputType.emailAddress,
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Please enter an email';
              }
              if (!RegExp(r'^[^@]+@[^@]+\.[^@]+').hasMatch(value)) {
                return 'Please enter a valid email';
              }
              return null;
            },
          ),
          TextFormField(
            controller: _passwordController,
            decoration: InputDecoration(labelText: 'Password'),
            obscureText: true,
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Please enter a password';
              }
              if (value.length < 8) {
                return 'Password must be at least 8 characters';
              }
              if (!RegExp(r'^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]').hasMatch(value)) {
                return 'Password must contain uppercase, lowercase, number and special character';
              }
              return null;
            },
          ),
          TextFormField(
            controller: _fullNameController,
            decoration: InputDecoration(labelText: 'Full Name'),
            validator: (value) {
              if (value == null || value.isEmpty) {
                return 'Please enter your full name';
              }
              return null;
            },
          ),
          DropdownButtonFormField<int>(
            value: _selectedRole,
            decoration: InputDecoration(labelText: 'Role'),
            items: [
              DropdownMenuItem(value: 3, child: Text('User (Technician)')),
              DropdownMenuItem(value: 4, child: Text('Viewer (Client)')),
            ],
            onChanged: (value) {
              setState(() {
                _selectedRole = value!;
              });
            },
          ),
          ElevatedButton(
            onPressed: _register,
            child: Text('Register'),
          ),
        ],
      ),
    );
  }
}
```

## 🔄 Token Refresh

**POST** `/api/v1/auth/refresh`

Refresh an expired JWT token using a refresh token.

### Request Body

```json
{
  "refreshToken": "your_refresh_token_here"
}
```

### Success Response (200)

```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "token": "new_jwt_token_here",
    "refreshToken": "new_refresh_token_here"
  },
  "errors": []
}
```

## 🚪 Logout

**POST** `/api/v1/auth/logout`

**Headers Required**:
```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Invalidate the current session and tokens.

### Success Response (200)

```json
{
  "success": true,
  "message": "Logout successful",
  "data": null,
  "errors": []
}
```

## 🔒 Security Best Practices

### 1. Token Storage
- **Mobile Apps**: Use secure storage (Keychain/Keystore)
- **Web Apps**: Use httpOnly cookies or secure localStorage
- **Never** store tokens in plain text

### 2. Token Handling
Implement secure token storage using appropriate methods for your platform.

### 3. Automatic Token Refresh
Implement automatic token refresh when tokens expire.
      return await _makeRequest(endpoint, data, token);
    }
  }
  
  static Future<void> _refreshToken() async {
    final refreshToken = await TokenManager.getRefreshToken();
    final response = await post('/auth/refresh', {
      'refreshToken': refreshToken
    });
    
    if (response['success']) {
      await TokenManager.saveTokens(
        response['data']['token'],
        response['data']['refreshToken']
      );
    } else {
      // Refresh failed, redirect to login
      await TokenManager.clearTokens();
      throw AuthenticationException('Please login again');
    }
  }
}
```

## ❌ Common Authentication Errors

| Error Code | Status | Description | Solution |
|------------|--------|-------------|----------|
| AUTH001 | 401 | Invalid credentials | Check username/email and password |
| AUTH002 | 401 | Token expired | Use refresh token or login again |
| AUTH003 | 401 | Invalid token format | Ensure Bearer token format |
| AUTH004 | 403 | Insufficient permissions | Check user role requirements |
| AUTH005 | 400 | Weak password | Follow password requirements |
| AUTH006 | 409 | Username/email exists | Choose different username or email |
| AUTH007 | 429 | Too many login attempts | Wait before retrying |
| AUTH008 | 400 | Invalid refresh token | Login again to get new tokens |

## Advanced Security Features

### Multi-Factor Authentication
```http
POST /api/v1/auth/enable-mfa
```

### Password Policy
```http
GET /api/v1/auth/password-policy
```

### Session Management
```http
GET /api/v1/auth/active-sessions
DELETE /api/v1/auth/sessions/{sessionId}
```

### Account Recovery
```http
POST /api/v1/auth/forgot-password
POST /api/v1/auth/reset-password
```

---
*Last Updated: January 2025*
