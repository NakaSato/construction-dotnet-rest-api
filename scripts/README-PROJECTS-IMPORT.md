# 🚀 Solar Projects Bulk Creation Scripts

This directory contains scripts for bulk creating solar projects from JSON data using the REST API.

## 📁 Scripts Overview

### Main Scripts

#### `create-projects-from-json.sh`
**Purpose**: Creates projects from a JSON file using the REST API  
**Usage**: `./create-projects-from-json.sh [json_file] [api_base_url]`  
**Features**:
- ✅ JWT authentication with admin credentials
- ✅ Comprehensive error handling and logging
- ✅ Progress tracking with colorized output
- ✅ Automatic response format detection
- ✅ Rate limiting with 0.5s delays between requests
- ✅ Detailed success/failure reporting

#### `test-create-projects.sh`
**Purpose**: Test script that creates only the first 3 projects  
**Usage**: `./test-create-projects.sh`  
**Features**:
- ✅ Safe testing with limited scope
- ✅ Automatic test file creation and cleanup
- ✅ Preview of projects to be created

#### `verify-projects.sh`
**Purpose**: Verifies existing projects in the API  
**Usage**: `./verify-projects.sh [api_base_url]`  
**Features**:
- ✅ Lists all projects with names and IDs
- ✅ Project count and status summary
- ✅ Equipment and capacity totals
- ✅ Grouped statistics by status

## 📊 Data Files

### `projects.json`
- **Contains**: 25 Thai solar installation projects
- **Format**: Array of project objects with complete solar project data
- **Fields**: Project names, locations, equipment details, capacities, timelines
- **Encoding**: UTF-8 with Thai language support

## 🔧 Prerequisites

### Required Tools
```bash
# Install jq for JSON processing
brew install jq  # macOS
apt-get install jq  # Ubuntu/Debian
```

### Required Services
- ✅ .NET API server running on `http://localhost:5001`
- ✅ Valid admin credentials: `admin@example.com` / `Admin123!`
- ✅ Database with proper migrations applied

## 📋 Usage Examples

### Create All Projects
```bash
# Create all projects from projects.json
./create-projects-from-json.sh projects.json

# Create projects from custom file
./create-projects-from-json.sh my-projects.json

# Use custom API URL
./create-projects-from-json.sh projects.json http://localhost:8080
```

### Test with Limited Data
```bash
# Test with first 3 projects only
./test-create-projects.sh

# Create custom test subset
jq '.[0:5]' projects.json > test-subset.json
./create-projects-from-json.sh test-subset.json
```

### Verify Results
```bash
# Check all projects in the system
./verify-projects.sh

# Check with custom API URL
./verify-projects.sh http://localhost:8080
```

## 🎯 JSON Data Format

The JSON file should contain an array of project objects with this structure:

```json
[
  {
    "projectId": "107",
    "projectName": "สำนักงานประปาเขต 9",
    "address": null,
    "clientInfo": "107 กปภ.เขต 9",
    "status": "Planning",
    "startDate": "2025-06-09T00:00:00Z",
    "estimatedEndDate": "2025-08-08T00:00:00Z",
    "actualEndDate": null,
    "team": "Solar Team Alpha",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง",
    "totalCapacityKw": 125.4,
    "pvModuleCount": 220,
    "equipmentDetails": {
      "inverter125kw": 1,
      "inverter80kw": 0,
      "inverter60kw": 4,
      "inverter40kw": 1
    },
    "ftsValue": 0,
    "revenueValue": 0,
    "pqmValue": 0,
    "locationCoordinates": {
      "latitude": 0.0,
      "longitude": 0.0
    },
    "createdAt": "2025-07-05T17:15:00Z"
  }
]
```

## ⚙️ Configuration

### API Endpoints
- **Login**: `POST /api/v1/auth/login`
- **Projects**: `POST /api/v1/projects`

### Authentication
- **Method**: JWT Bearer Token
- **Default Credentials**: `admin@example.com` / `Admin123!`
- **Roles Required**: Admin or Manager for project creation

### Field Mapping
The script automatically maps JSON fields to API request format:
- `projectManagerId` → `null` (avoiding ID conflicts)
- `address` → `"Address not specified"` (if null)
- All equipment and location data preserved

## 🔍 Troubleshooting

### Common Issues

#### "Project manager not found"
**Cause**: ProjectManagerId in JSON doesn't exist in database  
**Solution**: Script sets `projectManagerId: null` to avoid this

#### "Invalid JSON syntax"
**Cause**: Malformed JSON file  
**Solution**: Validate JSON with `jq empty filename.json`

#### "Login failed"
**Cause**: API not running or wrong credentials  
**Solution**: 
- Check API is running: `curl http://localhost:5001/api/v1/projects`
- Verify credentials in script

#### "No response from API"
**Cause**: Network or server issues  
**Solution**: Check API logs and network connectivity

### Response Format Handling
The script handles multiple API response formats:
- ✅ Standard ApiResponse: `{"success": true, "data": {...}}`
- ✅ Result format: `{"result": {}, "value": null}`
- ✅ Error responses with proper error messages

## 📈 Success Metrics

### Completed Execution (July 5, 2025)
- ✅ **Total Projects Created**: 25 Thai solar projects
- ✅ **Success Rate**: 100% (25/25 projects)
- ✅ **Data Integrity**: All equipment, capacity, and timeline data preserved
- ✅ **Performance**: ~0.5s per project with rate limiting
- ✅ **Error Handling**: Robust error detection and reporting

### Project Coverage
- **Regions**: Northern Thailand (Chiang Mai, Lamphun, Lampang, etc.)
- **Capacity Range**: 94.62 kW - 475.38 kW
- **Total Capacity**: 4,600+ kW across all projects
- **Connection Types**: MV (Medium Voltage) and LV (Low Voltage)
- **Equipment**: 125kW, 80kW, 60kW, and 40kW inverters

## 🔄 Integration

These scripts integrate with:
- ✅ **JWT Authentication System**: Role-based access control
- ✅ **Project Management API**: CRUD operations with validation
- ✅ **Database Migrations**: EF Core with PostgreSQL
- ✅ **Equipment Tracking**: Detailed inverter and capacity data
- ✅ **Timeline Management**: Start/end dates with proper formatting

---

**Last Updated**: July 5, 2025  
**Status**: Production Ready ✅  
**Total Projects**: 60 (25 from JSON import + existing)  
**Next Steps**: Ready for daily reports and task management integration
