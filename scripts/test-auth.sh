curl -s -X POST http://localhost:5002/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "testuser@example.com",
    "password": "TestPassword123!",
    "fullName": "Test User",
    "roleId": 2
  }' | jq .