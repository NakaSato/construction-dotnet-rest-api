# 📁 Image Upload & File Management

**Base URL**: `/api/v1/uploads`

**🔒 Authentication Required**  
**🎯 Status**: ✅ Available

The Image Upload module provides secure file and image handling for the Solar Project Management system, supporting documentation, progress photos, technical diagrams, and compliance materials.

## ⚡ Authorization & Access Control

| Role | Upload Files | View Files | Delete Files | Manage Storage | Admin Functions |
|------|--------------|------------|--------------|----------------|-----------------|
| **Admin** | ✅ All Types | ✅ All Files | ✅ All Files | ✅ Full Access | ✅ System Config |
| **Manager** | ✅ All Types | ✅ Project Files | ✅ Project Files | ✅ Project Storage | ✅ Quotas |
| **Supervisor** | ✅ Limited Types | ✅ Assigned Files | ✅ Own Files | ❌ No | ❌ No |
| **User** | ✅ Reports Only | ✅ Assigned Files | ✅ Own Files | ❌ No | ❌ No |

## 📋 Supported File Types

### 🖼️ Images
- **Formats**: JPEG, PNG, GIF, WEBP, TIFF
- **Max Size**: 10 MB per file
- **Use Cases**: Progress photos, equipment images, site documentation

### 📄 Documents  
- **Formats**: PDF, DOC, DOCX, XLS, XLSX, TXT
- **Max Size**: 25 MB per file
- **Use Cases**: Technical specifications, contracts, reports

### 🎥 Videos
- **Formats**: MP4, AVI, MOV, WEBM
- **Max Size**: 100 MB per file  
- **Use Cases**: Training videos, progress documentation

### 📐 Technical Files
- **Formats**: DWG, DXF, CAD, SKP (SketchUp)
- **Max Size**: 50 MB per file
- **Use Cases**: Technical drawings, 3D models

## 📤 Upload Single File

**POST** `/api/v1/uploads/single`

Upload a single file with metadata.

**Authorization**: All authenticated users (with role-based restrictions)

**Content-Type**: `multipart/form-data`

**Form Fields**:
- `file` (file) - The file to upload
- `projectId` (string, optional) - Associated project ID
- `category` (string) - File category ("progress", "technical", "documentation", "compliance")
- `description` (string, optional) - File description
- `tags` (string, optional) - Comma-separated tags
- `isPublic` (boolean, optional) - Make file publicly accessible (default: false)

**Example Request**:
```bash
curl -X POST "http://localhost:5002/api/v1/uploads/single" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@progress_photo.jpg" \
  -F "projectId=8f83b2a1-c4e5-4d67-9abc-123456789def" \
  -F "category=progress" \
  -F "description=Solar panel installation progress - Day 5" \
  -F "tags=solar,installation,rooftop,progress"
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "fileId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "fileName": "progress_photo.jpg",
    "originalFileName": "IMG_20240620_143022.jpg",
    "fileSize": 2457600,
    "mimeType": "image/jpeg",
    "category": "progress",
    "description": "Solar panel installation progress - Day 5",
    "tags": ["solar", "installation", "rooftop", "progress"],
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "uploadedBy": {
      "userId": "user123",
      "fullName": "John Smith",
      "role": "Technician"
    },
    "uploadedAt": "2024-06-20T14:30:22Z",
    "isPublic": false,
    "urls": {
      "original": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
      "thumbnail": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/thumbnail",
      "preview": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/preview"
    },
    "metadata": {
      "dimensions": {
        "width": 1920,
        "height": 1080
      },
      "location": {
        "latitude": 13.7563,
        "longitude": 100.5018
      },
      "camera": {
        "make": "Apple",
        "model": "iPhone 12",
        "timestamp": "2024-06-20T14:30:22Z"
      }
    }
  }
}
```

## 📤 Upload Multiple Files

**POST** `/api/v1/uploads/multiple`

Upload multiple files in a single request.

**Authorization**: All authenticated users (with role-based restrictions)

**Content-Type**: `multipart/form-data`

**Form Fields**:
- `files` (file[]) - Array of files to upload
- `projectId` (string, optional) - Associated project ID
- `category` (string) - File category for all files
- `descriptions` (string[], optional) - Array of descriptions matching file order
- `tags` (string, optional) - Common tags for all files

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Files uploaded successfully",
  "data": {
    "uploadBatchId": "batch_123456789",
    "totalFiles": 3,
    "successfulUploads": 3,
    "failedUploads": 0,
    "files": [
      {
        "fileId": "file1_guid",
        "fileName": "panel_installation_1.jpg",
        "fileSize": 2457600,
        "status": "success"
      },
      {
        "fileId": "file2_guid", 
        "fileName": "panel_installation_2.jpg",
        "fileSize": 2134567,
        "status": "success"
      },
      {
        "fileId": "file3_guid",
        "fileName": "technical_spec.pdf",
        "fileSize": 1567890,
        "status": "success"
      }
    ],
    "uploadedAt": "2024-06-20T14:35:00Z"
  }
}
```

## 📋 Get File List

**GET** `/api/v1/uploads`

Retrieve a paginated list of uploaded files with advanced filtering.

**Query Parameters**:
- `projectId` (guid): Filter by project
- `category` (string): Filter by category
- `fileType` (string): Filter by file type ("image", "document", "video", "technical")
- `uploadedBy` (guid): Filter by uploader user ID
- `tags` (string): Comma-separated tags to filter by
- `dateFrom` (datetime): Filter files uploaded after this date
- `dateTo` (datetime): Filter files uploaded before this date
- `search` (string): Search in file names and descriptions
- `sortBy` (string): Sort field ("fileName", "uploadedAt", "fileSize")
- `sortOrder` (string): Sort direction ("asc", "desc")
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Files retrieved successfully",
  "data": {
    "items": [
      {
        "fileId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "fileName": "progress_photo.jpg",
        "originalFileName": "IMG_20240620_143022.jpg",
        "fileSize": 2457600,
        "mimeType": "image/jpeg",
        "category": "progress",
        "description": "Solar panel installation progress - Day 5",
        "tags": ["solar", "installation", "rooftop", "progress"],
        "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
        "projectName": "Building A Solar Installation",
        "uploadedBy": {
          "userId": "user123",
          "fullName": "John Smith",
          "role": "Technician"
        },
        "uploadedAt": "2024-06-20T14:30:22Z",
        "isPublic": false,
        "downloadCount": 15,
        "urls": {
          "thumbnail": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/thumbnail",
          "preview": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/preview"
        }
      }
    ],
    "summary": {
      "totalFiles": 145,
      "totalSize": 1250000000,
      "categoryBreakdown": {
        "progress": 89,
        "technical": 34,
        "documentation": 15,
        "compliance": 7
      },
      "typeBreakdown": {
        "image": 112,
        "document": 28,
        "video": 3,
        "technical": 2
      }
    },
    "pagination": {
      "totalCount": 145,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 8
    }
  }
}
```

## 📁 Get File Details

**GET** `/api/v1/uploads/{id}`

Get detailed information about a specific file.

**Path Parameters**:
- `id` (guid) - File ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "File details retrieved successfully",
  "data": {
    "fileId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "fileName": "progress_photo.jpg",
    "originalFileName": "IMG_20240620_143022.jpg",
    "fileSize": 2457600,
    "fileSizeFormatted": "2.35 MB",
    "mimeType": "image/jpeg",
    "category": "progress",
    "description": "Solar panel installation progress - Day 5",
    "tags": ["solar", "installation", "rooftop", "progress"],
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "projectName": "Building A Solar Installation",
    "uploadedBy": {
      "userId": "user123",
      "fullName": "John Smith",
      "role": "Technician",
      "email": "john.smith@company.com"
    },
    "uploadedAt": "2024-06-20T14:30:22Z",
    "lastAccessedAt": "2024-06-20T16:45:30Z",
    "downloadCount": 15,
    "isPublic": false,
    "urls": {
      "original": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
      "thumbnail": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/thumbnail",
      "preview": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/preview",
      "download": "/api/v1/uploads/files/6e729d9a-b2fc-4d54-8e79-81d77bd248d3/download"
    },
    "metadata": {
      "dimensions": {
        "width": 1920,
        "height": 1080,
        "aspectRatio": "16:9"
      },
      "location": {
        "latitude": 13.7563,
        "longitude": 100.5018,
        "address": "Bangkok, Thailand"
      },
      "camera": {
        "make": "Apple",
        "model": "iPhone 12",
        "timestamp": "2024-06-20T14:30:22Z",
        "settings": {
          "iso": 100,
          "aperture": "f/1.6",
          "shutterSpeed": "1/120"
        }
      }
    },
    "analysis": {
      "imageAnalysis": {
        "objectsDetected": ["solar panel", "rooftop", "mounting system"],
        "qualityScore": 95,
        "brightness": 78,
        "contrast": 85,
        "sharpness": 92
      },
      "complianceCheck": {
        "safetyEquipmentVisible": true,
        "properInstallation": true,
        "qualityStandards": "passed"
      }
    },
    "accessHistory": [
      {
        "userId": "mgr001",
        "fullName": "Sarah Davis",
        "action": "download",
        "timestamp": "2024-06-20T16:45:30Z"
      }
    ]
  }
}
```

## 📥 Download File

**GET** `/api/v1/uploads/files/{id}`

Download the original file.

**Path Parameters**:
- `id` (guid) - File ID

**Query Parameters**:
- `download` (bool): Force download instead of inline display (default: false)
- `version` (string): File version ("original", "thumbnail", "preview")

**Success Response (200)**:
- Returns the file content with appropriate headers
- `Content-Type`: Set to file's MIME type
- `Content-Disposition`: Set based on download parameter
- `Content-Length`: File size in bytes

## 🖼️ Get Image Thumbnail

**GET** `/api/v1/uploads/files/{id}/thumbnail`

Get a thumbnail version of an image file.

**Path Parameters**:
- `id` (guid) - File ID

**Query Parameters**:
- `size` (string): Thumbnail size ("small", "medium", "large") - default: "medium"
- `format` (string): Output format ("jpeg", "png", "webp") - default: "jpeg"

**Thumbnail Sizes**:
- **Small**: 150x150px
- **Medium**: 300x300px  
- **Large**: 600x600px

## ✏️ Update File Metadata

**PATCH** `/api/v1/uploads/{id}`

Update file metadata and properties.

**Authorization**: File owner, Manager, Admin

**Path Parameters**:
- `id` (guid) - File ID

**Request Body**:
```json
{
  "description": "Updated description for the file",
  "tags": ["new", "updated", "tags"],
  "category": "documentation",
  "isPublic": true
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "File metadata updated successfully",
  "data": {
    "fileId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "updatedFields": ["description", "tags", "category", "isPublic"],
    "updatedAt": "2024-06-20T17:00:00Z"
  }
}
```

## 🗑️ Delete File

**DELETE** `/api/v1/uploads/{id}`

Delete a file permanently.

**Authorization**: File owner, Manager (project files), Admin (all files)

**Path Parameters**:
- `id` (guid) - File ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "File deleted successfully",
  "data": {
    "fileId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
    "fileName": "progress_photo.jpg",
    "deletedAt": "2024-06-20T17:15:00Z",
    "freedSpace": 2457600
  }
}
```

## 📊 Upload Analytics

**GET** `/api/v1/uploads/analytics`

Get comprehensive upload and storage analytics.

**Query Parameters**:
- `projectId` (guid): Filter by specific project
- `period` (string): Analytics period ("7d", "30d", "90d", "1y")
- `groupBy` (string): Group results by ("day", "week", "month")

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Upload analytics retrieved successfully",
  "data": {
    "period": {
      "startDate": "2024-05-21T00:00:00Z",
      "endDate": "2024-06-20T23:59:59Z",
      "duration": "30 days"
    },
    "summary": {
      "totalFiles": 1250,
      "totalSize": 15750000000,
      "totalSizeFormatted": "14.67 GB",
      "averageFileSize": 12600000,
      "uploadsThisPeriod": 145,
      "downloadsThisPeriod": 892,
      "uniqueUploaders": 25
    },
    "breakdown": {
      "byCategory": {
        "progress": {
          "count": 89,
          "size": 890000000,
          "percentage": 61.4
        },
        "technical": {
          "count": 34,
          "size": 340000000,
          "percentage": 23.4
        },
        "documentation": {
          "count": 15,
          "size": 150000000,
          "percentage": 10.3
        },
        "compliance": {
          "count": 7,
          "size": 70000000,
          "percentage": 4.8
        }
      },
      "byFileType": {
        "image": {
          "count": 112,
          "size": 1120000000,
          "averageSize": 10000000
        },
        "document": {
          "count": 28,
          "size": 280000000,
          "averageSize": 10000000
        },
        "video": {
          "count": 3,
          "size": 300000000,
          "averageSize": 100000000
        },
        "technical": {
          "count": 2,
          "size": 50000000,
          "averageSize": 25000000
        }
      },
      "byProject": [
        {
          "projectId": "proj001",
          "projectName": "Building A Solar",
          "fileCount": 45,
          "totalSize": 450000000,
          "lastUpload": "2024-06-20T14:30:00Z"
        }
      ]
    },
    "trends": {
      "dailyUploads": [
        {
          "date": "2024-06-20",
          "uploads": 8,
          "size": 80000000
        }
      ],
      "topUploaders": [
        {
          "userId": "user123",
          "fullName": "John Smith",
          "uploadCount": 25,
          "totalSize": 250000000
        }
      ],
      "popularCategories": [
        {
          "category": "progress",
          "uploadCount": 89,
          "downloadCount": 445
        }
      ]
    },
    "storageInfo": {
      "quotaUsed": 15750000000,
      "quotaTotal": 50000000000,
      "quotaUsedPercentage": 31.5,
      "storageByProject": [
        {
          "projectId": "proj001",
          "usage": 4500000000,
          "percentage": 28.6
        }
      ]
    }
  }
}
```

## 🔍 Search Files

**GET** `/api/v1/uploads/search`

Advanced file search with full-text search capabilities.

**Query Parameters**:
- `q` (string): Search query
- `filters` (object): Advanced filter criteria
- `sortBy` (string): Sort field
- `sortOrder` (string): Sort direction
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Search results retrieved successfully",
  "data": {
    "query": "solar panel installation",
    "results": [
      {
        "fileId": "file123",
        "fileName": "solar_panel_installation_guide.pdf",
        "relevanceScore": 95.2,
        "matchedFields": ["fileName", "description", "tags"],
        "highlights": [
          "Solar panel <mark>installation</mark> procedures",
          "Rooftop <mark>solar</mark> mounting systems"
        ]
      }
    ],
    "facets": {
      "categories": {
        "progress": 45,
        "technical": 23,
        "documentation": 12
      },
      "fileTypes": {
        "image": 67,
        "document": 13
      },
      "projects": {
        "Building A Solar": 34,
        "Warehouse Installation": 28
      }
    },
    "suggestions": ["solar panels", "installation guide", "rooftop mounting"],
    "totalResults": 80,
    "searchTime": 0.045
  }
}
```

## 🛡️ Security Features

### 🔐 Access Control
- **Role-based permissions** for upload, view, and delete operations
- **Project-based access control** - users can only access files for assigned projects
- **File ownership** - users have special permissions for their own uploads

### 🔍 Content Scanning
- **Virus scanning** - All uploaded files are scanned for malware
- **Content validation** - File headers are checked to prevent executable uploads
- **Size restrictions** - Configurable file size limits by role and file type

### 📝 Audit Trail
- **Upload tracking** - Complete log of who uploaded what and when
- **Access logging** - Track file downloads and views
- **Modification history** - Log all metadata changes

### 🛡️ Data Protection
- **Encryption at rest** - Files are encrypted in storage
- **Secure URLs** - Temporary signed URLs for file access
- **Backup integration** - Files are included in system backups

## ⚙️ Configuration

### 📁 Storage Settings
```json
{
  "storage": {
    "provider": "local",
    "path": "/app/uploads",
    "maxFileSize": 104857600,
    "allowedTypes": ["image/*", "application/pdf", "video/mp4"],
    "quarantineEnabled": true,
    "virusScanningEnabled": true
  },
  "thumbnails": {
    "enabled": true,
    "sizes": ["150x150", "300x300", "600x600"],
    "format": "jpeg",
    "quality": 85
  },
  "quota": {
    "perProject": 10737418240,
    "perUser": 1073741824,
    "systemTotal": 107374182400
  }
}
```

## ⚠️ Error Codes

| Code | Message | Description | Solution |
|------|---------|-------------|----------|
| **UL001** | File too large | File exceeds size limit | Reduce file size or contact admin |
| **UL002** | Invalid file type | File type not allowed | Use supported file formats |
| **UL003** | Upload quota exceeded | Storage quota limit reached | Delete old files or request increase |
| **UL004** | File not found | Requested file doesn't exist | Check file ID and permissions |
| **UL005** | Access denied | Insufficient permissions | Check user role and project access |
| **UL006** | Virus detected | File failed security scan | Upload clean file |
| **UL007** | Upload failed | General upload error | Check network and retry |
| **UL008** | Invalid metadata | Metadata validation failed | Check required fields |
| **UL009** | File corrupted | File appears to be corrupted | Re-upload file |
| **UL010** | Storage unavailable | Storage system error | Contact administrator |

## 📋 Summary

### Key Features
- **Multi-format Support**: Images, documents, videos, and technical files
- **Advanced Security**: Virus scanning, content validation, and access control
- **Intelligent Processing**: Automatic thumbnail generation and metadata extraction
- **Search & Organization**: Full-text search, tagging, and categorization
- **Analytics & Reporting**: Comprehensive usage and storage analytics
- **API Integration**: RESTful endpoints for all file operations

### Use Cases
- **Progress Documentation**: Upload photos and videos of installation progress
- **Technical Documentation**: Store technical drawings, specifications, and manuals
- **Compliance Materials**: Maintain compliance documentation and certificates
- **Training Resources**: Share training videos and instructional materials
- **Quality Assurance**: Document quality checks and inspection results

### Best Practices
- Use descriptive file names and add detailed descriptions
- Tag files appropriately for easy searching and organization
- Regularly review and clean up unnecessary files to manage storage
- Use appropriate categories to improve organization and access control
- Upload high-quality images for better documentation and analysis
- Follow security guidelines and avoid uploading sensitive personal information
