# Solar Projects API - Project Deletion Commands

This file contains curl commands for deleting projects from the Solar Projects API.

## Prerequisites

1. API server running on localhost:5001
2. Valid admin credentials (admin@example.com / Admin123!)
3. `jq` installed for JSON parsing

## Quick Commands

### 1. Get JWT Token
```bash
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token // .token // .accessToken')

echo "Token: $TOKEN"
```

### 2. List All Projects (Get IDs)
```bash
curl -s -H "Authorization: Bearer $TOKEN" \
  http://localhost:5001/api/v1/projects \
  | jq '.data.items[].projectId // .data.items[].id // .data.projects[].projectId // .data.projects[].id // .data[].projectId // .data[].id // .[].projectId // .[].id'
```

### 3. Delete a Single Project
```bash
PROJECT_ID="your-project-id-here"
curl -X DELETE http://localhost:5001/api/v1/projects/$PROJECT_ID \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Delete All Projects (One-liner)
```bash
# ⚠️ WARNING: This deletes ALL projects - use with extreme caution!
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token // .token // .accessToken') && \
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId // .data.items[].id // .data.projects[].projectId // .data.projects[].id // .data[].projectId // .data[].id // .[].projectId // .[].id' \
  | xargs -I {} curl -X DELETE http://localhost:5001/api/v1/projects/{} \
    -H "Authorization: Bearer $TOKEN" \
    && echo "All projects deleted"
```

### 5. Delete First N Projects (Safer)
```bash
# Delete first 5 projects only
N=5
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token // .token // .accessToken') && \
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId // .data.items[].id // .data.projects[].projectId // .data.projects[].id // .data[].projectId // .data[].id // .[].projectId // .[].id' \
  | head -n $N \
  | xargs -I {} curl -X DELETE http://localhost:5001/api/v1/projects/{} \
    -H "Authorization: Bearer $TOKEN"
```

## Verification Commands

### Count Projects
```bash
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq '.data.totalCount // (.data | length) // length'
```

### List Project Names and IDs
```bash
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[] // .data.projects[] // .data[] // .[] | "\(.projectId // .id): \(.projectName // .name)"'
```

## Script Usage

Instead of using curl commands directly, you can use the provided scripts:

### Safe Testing (Delete First 5 Projects)
```bash
./scripts/delete-test-projects.sh
```

### Delete All Projects (With Safety Confirmations)
```bash
./scripts/delete-all-projects.sh
```

### Delete All Projects (Skip Confirmations - DANGEROUS!)
```bash
./scripts/delete-all-projects.sh --confirm
```

### Verify Projects After Deletion
```bash
./scripts/verify-projects.sh
```

## Notes

- All deletion operations are **IRREVERSIBLE**
- The API may return 404 for already deleted projects (this is normal)
- HTTP status codes 200, 204, and 404 are considered successful deletions
- Rate limiting is built into the scripts to avoid overwhelming the API
- Always verify the API response structure if commands fail unexpectedly
