# User Registration API Documentation

## Overview
The Solar Projects REST API provides a user registration endpoint that allows new users to create accounts in the system.

## Endpoint
**POST** `/api/v1/auth/register`

## Description
Creates a new user account in the system. This is a public endpoint that doesn't require authentication.

## Request Body
```json
{
  "username": "string (required, 3-50 chars, alphanumeric + underscore only)",
  "email": "string (required, valid email, max 255 chars)",
  "password": "string (required, 8-100 chars, must contain uppercase, lowercase, digit, special char)",
  "fullName": "string (required, 2-100 chars)",
  "roleId": "integer (required, positive number)"
}
```

## Role IDs
- `1` - Admin
- `2` - Manager  
- `3` - User (default)

## Success Response
**Status Code:** `200 OK`
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "8d457b09-6659-47a4-98a4-18838ccd6f71",
    "username": "testuser123",
    "email": "testuser@example.com", 
    "fullName": "Test User",
    "roleName": "User",
    "isActive": true
  },
  "errors": [],
  "error": null
}
```

## Error Responses

### Validation Errors
**Status Code:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Please provide a valid email address"],
    "RoleId": ["Role ID must be a positive number"],
    "FullName": ["Full name is required"],
    "Password": ["Password must contain at least one uppercase letter..."],
    "Username": ["Username must be between 3 and 50 characters"]
  }
}
```

### Duplicate User
**Status Code:** `200 OK` (but success = false)
```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "errors": ["Username or email already exists"],
  "error": null
}
```

## Example Usage

### cURL
```bash
curl -X POST "http://localhost:5001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser123",
    "email": "newuser@example.com",
    "password": "SecurePass123!",
    "fullName": "New User",
    "roleId": 3
  }'
```

### JavaScript (Fetch)
```javascript
const registerUser = async (userData) => {
  try {
    const response = await fetch('http://localhost:5001/api/v1/auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData)
    });

    const result = await response.json();
    
    if (result.success) {
      console.log('User registered successfully:', result.data);
      return result.data;
    } else {
      console.error('Registration failed:', result.errors);
      return null;
    }
  } catch (error) {
    console.error('Error during registration:', error);
    return null;
  }
};

// Usage
registerUser({
  username: "newuser123",
  email: "newuser@example.com", 
  password: "SecurePass123!",
  fullName: "New User",
  roleId: 3
});
```

### Python (requests)
```python
import requests

def register_user(user_data):
    url = "http://localhost:5001/api/v1/auth/register"
    headers = {"Content-Type": "application/json"}
    
    response = requests.post(url, json=user_data, headers=headers)
    result = response.json()
    
    if result.get("success"):
        print("User registered successfully:", result["data"])
        return result["data"]
    else:
        print("Registration failed:", result.get("errors", []))
        return None

# Usage
user_data = {
    "username": "newuser123",
    "email": "newuser@example.com",
    "password": "SecurePass123!",
    "fullName": "New User", 
    "roleId": 3
}

register_user(user_data)
```

## Password Requirements
- Minimum 8 characters, maximum 100 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z) 
- At least one digit (0-9)
- At least one special character (@$!%*?&)

## Username Requirements
- 3-50 characters
- Only letters, numbers, and underscores allowed
- Must be unique

## Email Requirements
- Valid email format
- Maximum 255 characters
- Must be unique

## Security Features
- Passwords are hashed using BCrypt before storage
- Input validation on all fields
- Duplicate username/email prevention
- SQL injection protection through Entity Framework

## Notes
- The registration endpoint is public and doesn't require authentication
- User accounts are automatically activated upon creation (`isActive: true`)
- The `CreatedAt` timestamp is automatically set to the current UTC time
- Invalid roleId values default to role 3 (User)
