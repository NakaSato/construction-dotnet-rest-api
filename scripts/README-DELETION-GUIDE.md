# 🗑️ Solar Projects Deletion Guide

This guide provides multiple methods to delete projects from the Solar Projects API, ranging from safe testing to complete deletion.

## ⚠️ **IMPORTANT WARNINGS**
- **ALL DELETION OPERATIONS ARE IRREVERSIBLE**
- **ALWAYS BACKUP YOUR DATA BEFORE DELETING**
- **TEST WITH SMALL NUMBERS FIRST**
- **VERIFY RESULTS AFTER DELETION**

## 📁 Available Scripts

### 1. Safe Test Deletion (`delete-test-projects.sh`)
**Purpose**: Delete only the first 5 projects for testing
```bash
./scripts/delete-test-projects.sh
```
- ✅ Safe for testing
- ✅ Shows confirmation before deletion
- ✅ Provides detailed progress
- ✅ Limited to 5 projects

### 2. Complete Deletion (`delete-all-projects.sh`)
**Purpose**: Delete ALL projects with safety confirmations
```bash
./scripts/delete-all-projects.sh
```
- ⚠️ **DANGEROUS**: Deletes ALL projects
- ✅ Multiple confirmation prompts
- ✅ Detailed progress and summary
- ✅ Verification after deletion

### 3. Skip Confirmations (DANGEROUS!)
```bash
./scripts/delete-all-projects.sh --confirm
```
- 🚨 **EXTREMELY DANGEROUS**: No confirmation prompts
- ⚠️ Use only in automated environments
- ❌ No safety nets

## 🔗 Quick Curl Commands

### Prerequisites
```bash
# Install jq if not available
brew install jq

# Set API URL (optional)
API_URL="http://localhost:5001"
```

### Get JWT Token
```bash
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token')
```

### List All Project IDs
```bash
curl -s -H "Authorization: Bearer $TOKEN" \
  http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId'
```

### Delete Single Project
```bash
PROJECT_ID="your-project-id-here"
curl -X DELETE http://localhost:5001/api/v1/projects/$PROJECT_ID \
  -H "Authorization: Bearer $TOKEN"
```

### Delete First N Projects (Safe Testing)
```bash
# Delete first 3 projects only
N=3
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token') && \
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId' \
  | head -n $N \
  | while read id; do 
      echo "Deleting $id..."
      curl -s -X DELETE http://localhost:5001/api/v1/projects/$id \
        -H "Authorization: Bearer $TOKEN"
    done && echo "Deleted first $N projects"
```

### Delete ALL Projects (One-liner - DANGEROUS!)
```bash
# ⚠️ WARNING: This deletes ALL projects permanently!
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token') && \
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId' \
  | while read id; do 
      echo "Deleting $id..."
      curl -s -X DELETE http://localhost:5001/api/v1/projects/$id \
        -H "Authorization: Bearer $TOKEN"
    done && echo "ALL PROJECTS DELETED"
```

## 🔍 Verification Commands

### Count Remaining Projects
```bash
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq '.data.totalCount'
```

### List Project Names and IDs
```bash
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[] | "\(.projectId): \(.projectName)"'
```

### Verify Empty Database
```bash
./scripts/verify-projects.sh
```

## 📊 Usage Examples

### Example 1: Safe Testing Workflow
```bash
# 1. Check current count
./scripts/verify-projects.sh

# 2. Delete 5 projects safely
./scripts/delete-test-projects.sh

# 3. Verify deletion
./scripts/verify-projects.sh
```

### Example 2: Complete Deletion Workflow
```bash
# 1. Backup data (if needed)
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects > projects_backup.json

# 2. Delete all projects with confirmations
./scripts/delete-all-projects.sh

# 3. Verify complete deletion
./scripts/verify-projects.sh
```

### Example 3: Quick Testing with Curl
```bash
# Delete just 2 projects for testing
N=2
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}' \
  | jq -r '.data.token')

curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5001/api/v1/projects \
  | jq -r '.data.items[].projectId' \
  | head -n $N \
  | xargs -I {} curl -X DELETE http://localhost:5001/api/v1/projects/{} \
    -H "Authorization: Bearer $TOKEN"
```

## 🛠️ Troubleshooting

### Authentication Issues
- Ensure API server is running on localhost:5001
- Check admin credentials: `admin@example.com` / `Admin123!`
- Verify JWT token is being extracted correctly

### API Response Issues
- Projects are in `.data.items[]` array
- Project ID field is `projectId` (not `id`)
- HTTP status codes 200, 204, and 404 indicate successful deletion

### Script Permission Issues
```bash
chmod +x scripts/delete-*.sh
```

## 📝 Script Features

All provided scripts include:
- ✅ **Robust error handling**
- ✅ **Multiple API response format support**
- ✅ **Colorized progress output**
- ✅ **Rate limiting to avoid API overload**
- ✅ **Comprehensive deletion summary**
- ✅ **Post-deletion verification**

## 🚀 Quick Reference

| Goal | Command | Safety |
|------|---------|---------|
| Delete 5 projects | `./scripts/delete-test-projects.sh` | ✅ Safe |
| Delete all (with prompts) | `./scripts/delete-all-projects.sh` | ⚠️ Confirmed |
| Delete all (no prompts) | `./scripts/delete-all-projects.sh --confirm` | 🚨 Dangerous |
| Verify projects | `./scripts/verify-projects.sh` | ✅ Read-only |

Remember: **When in doubt, start with the test script!**
