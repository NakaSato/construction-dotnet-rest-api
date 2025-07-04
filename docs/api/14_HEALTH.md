# 🏥 Health Monitoring & System Status

**Base URL**: `/api/v1/health`

**🔒 Authentication**: Some endpoints public, others require authentication  
**🎯 Status**: ✅ Available

The Health Monitoring module provides comprehensive system health checks, performance metrics, and operational status monitoring for the Solar Project Management API.

## ⚡ Authorization & Access Control

| Role | Basic Health | Detailed Metrics | System Logs | Config Status | Admin Functions |
|------|--------------|------------------|-------------|---------------|-----------------|
| **Public** | ✅ Basic Check | ❌ No | ❌ No | ❌ No | ❌ No |
| **User** | ✅ Basic Check | ❌ No | ❌ No | ❌ No | ❌ No |
| **Manager** | ✅ Basic Check | ✅ Limited | ❌ No | ✅ Read Only | ❌ No |
| **Admin** | ✅ Full Access | ✅ Full Access | ✅ Full Access | ✅ Full Access | ✅ All Functions |

## 🔍 Basic Health Check

**GET** `/api/v1/health`

Get basic system health status. This endpoint is public and requires no authentication.

**Success Response (200)**:
```json
{
  "status": "Healthy",
  "timestamp": "2024-06-20T15:30:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "uptime": "15d 8h 42m 15s",
  "services": {
    "api": "Healthy",
    "database": "Healthy",
    "redis": "Healthy",
    "fileStorage": "Healthy"
  },
  "quick_stats": {
    "activeUsers": 47,
    "totalProjects": 97,
    "todayReports": 23,
    "apiCallsToday": 8742
  }
}
```

**Degraded Response (200)**:
```json
{
  "status": "Degraded",
  "timestamp": "2024-06-20T15:30:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "uptime": "15d 8h 42m 15s",
  "services": {
    "api": "Healthy",
    "database": "Healthy",
    "redis": "Degraded",
    "fileStorage": "Healthy"
  },
  "issues": [
    {
      "service": "redis",
      "severity": "Warning",
      "message": "High memory usage detected",
      "timestamp": "2024-06-20T15:25:00Z"
    }
  ],
  "quick_stats": {
    "activeUsers": 47,
    "totalProjects": 97,
    "todayReports": 23,
    "apiCallsToday": 8742
  }
}
```

**Unhealthy Response (503)**:
```json
{
  "status": "Unhealthy",
  "timestamp": "2024-06-20T15:30:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "uptime": "15d 8h 42m 15s",
  "services": {
    "api": "Healthy",
    "database": "Unhealthy",
    "redis": "Healthy",
    "fileStorage": "Healthy"
  },
  "critical_issues": [
    {
      "service": "database",
      "severity": "Critical",
      "message": "Database connection failed",
      "timestamp": "2024-06-20T15:28:00Z",
      "impact": "All data operations affected"
    }
  ]
}
```

## 📊 Detailed Health Status

**GET** `/api/v1/health/detailed`

Get comprehensive system health information including performance metrics.

**Authorization**: Manager, Admin

**Query Parameters**:
- `includeMetrics` (bool): Include performance metrics (default: true)
- `includeLogs` (bool): Include recent error logs (default: false, Admin only)
- `timeWindow` (string): Metrics time window ("1h", "24h", "7d") - default: "1h"

**Success Response (200)**:
```json
{
  "status": "Healthy",
  "timestamp": "2024-06-20T15:30:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "uptime": "15d 8h 42m 15s",
  "server": {
    "hostname": "solar-api-prod-01",
    "platform": ".NET 9.0",
    "processorCount": 8,
    "workingSet": "256 MB",
    "gcMemory": "145 MB"
  },
  "services": {
    "api": {
      "status": "Healthy",
      "responseTime": "45ms",
      "lastCheck": "2024-06-20T15:30:00Z",
      "details": {
        "requestsPerSecond": 12.5,
        "averageResponseTime": "87ms",
        "errorRate": 0.02
      }
    },
    "database": {
      "status": "Healthy",
      "responseTime": "12ms",
      "lastCheck": "2024-06-20T15:30:00Z",
      "details": {
        "connectionPoolSize": 50,
        "activeConnections": 12,
        "queryPerformance": "Good",
        "diskUsage": "45%"
      }
    },
    "redis": {
      "status": "Healthy",
      "responseTime": "3ms",
      "lastCheck": "2024-06-20T15:30:00Z",
      "details": {
        "memoryUsage": "67%",
        "connectedClients": 8,
        "cacheHitRate": 94.2,
        "keyCount": 15420
      }
    },
    "fileStorage": {
      "status": "Healthy",
      "responseTime": "25ms",
      "lastCheck": "2024-06-20T15:30:00Z",
      "details": {
        "diskUsage": "32%",
        "totalFiles": 1250,
        "totalSize": "14.67 GB",
        "availableSpace": "156 GB"
      }
    }
  },
  "performance": {
    "requestMetrics": {
      "total": 8742,
      "successful": 8724,
      "failed": 18,
      "averageResponseTime": "87ms",
      "p95ResponseTime": "245ms",
      "p99ResponseTime": "850ms",
      "throughput": 12.5
    },
    "resourceUtilization": {
      "cpu": 23.5,
      "memory": 67.8,
      "disk": 32.1,
      "network": 12.3
    },
    "databaseMetrics": {
      "queryCount": 15420,
      "averageQueryTime": "12ms",
      "slowQueries": 3,
      "connectionPoolUtilization": 24.0
    }
  },
  "businessMetrics": {
    "activeUsers": 47,
    "totalProjects": 97,
    "todayReports": 23,
    "completedTasks": 156,
    "pendingApprovals": 12,
    "systemLoad": "Normal"
  },
  "alerts": [
    {
      "id": "alert001",
      "severity": "Info",
      "message": "High API usage detected",
      "timestamp": "2024-06-20T15:25:00Z",
      "service": "api",
      "resolved": false
    }
  ]
}
```

## 📈 Performance Metrics

**GET** `/api/v1/health/metrics`

Get detailed performance metrics and statistics.

**Authorization**: Manager, Admin

**Query Parameters**:
- `metric` (string): Specific metric to retrieve ("response_time", "throughput", "error_rate", "all")
- `timeWindow` (string): Time window for metrics ("1h", "6h", "24h", "7d") - default: "1h"
- `granularity` (string): Data granularity ("minute", "hour", "day") - default: "minute"

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Performance metrics retrieved successfully",
  "data": {
    "timeWindow": {
      "start": "2024-06-20T14:30:00Z",
      "end": "2024-06-20T15:30:00Z",
      "duration": "1 hour"
    },
    "summary": {
      "totalRequests": 723,
      "successfulRequests": 720,
      "failedRequests": 3,
      "averageResponseTime": 87,
      "maxResponseTime": 1245,
      "minResponseTime": 12,
      "throughput": 12.05,
      "errorRate": 0.41
    },
    "timeSeries": {
      "responseTime": [
        {
          "timestamp": "2024-06-20T15:00:00Z",
          "average": 85,
          "p50": 65,
          "p95": 180,
          "p99": 450
        },
        {
          "timestamp": "2024-06-20T15:01:00Z",
          "average": 92,
          "p50": 68,
          "p95": 195,
          "p99": 520
        }
      ],
      "throughput": [
        {
          "timestamp": "2024-06-20T15:00:00Z",
          "requestsPerSecond": 11.2
        },
        {
          "timestamp": "2024-06-20T15:01:00Z",
          "requestsPerSecond": 13.8
        }
      ],
      "errorRate": [
        {
          "timestamp": "2024-06-20T15:00:00Z",
          "percentage": 0.2
        },
        {
          "timestamp": "2024-06-20T15:01:00Z",
          "percentage": 0.0
        }
      ]
    },
    "endpoints": {
      "mostUsed": [
        {
          "endpoint": "GET /api/v1/projects",
          "requests": 145,
          "averageResponseTime": 65,
          "errorRate": 0.0
        },
        {
          "endpoint": "POST /api/v1/daily-reports",
          "requests": 89,
          "averageResponseTime": 125,
          "errorRate": 1.1
        }
      ],
      "slowest": [
        {
          "endpoint": "GET /api/v1/master-plans/analytics",
          "averageResponseTime": 850,
          "requests": 23,
          "errorRate": 0.0
        }
      ]
    },
    "errors": {
      "byType": {
        "validation": 2,
        "timeout": 1,
        "notFound": 0,
        "serverError": 0
      },
      "byEndpoint": {
        "POST /api/v1/daily-reports": 2,
        "GET /api/v1/users/profile": 1
      }
    }
  }
}
```

## 🎯 Service Dependencies

**GET** `/api/v1/health/dependencies`

Check the health of all external service dependencies.

**Authorization**: Manager, Admin

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Dependency health check completed",
  "data": {
    "overall": "Healthy",
    "timestamp": "2024-06-20T15:30:00Z",
    "dependencies": {
      "postgresql": {
        "status": "Healthy",
        "responseTime": 12,
        "lastCheck": "2024-06-20T15:30:00Z",
        "version": "14.8",
        "details": {
          "maxConnections": 100,
          "activeConnections": 12,
          "databaseSize": "2.4 GB",
          "lastBackup": "2024-06-20T02:00:00Z"
        }
      },
      "redis": {
        "status": "Healthy",
        "responseTime": 3,
        "lastCheck": "2024-06-20T15:30:00Z",
        "version": "6.2.6",
        "details": {
          "memoryUsage": "145 MB",
          "maxMemory": "512 MB",
          "keyCount": 15420,
          "uptime": "15d 8h 42m"
        }
      },
      "signalr": {
        "status": "Healthy",
        "responseTime": 8,
        "lastCheck": "2024-06-20T15:30:00Z",
        "details": {
          "connectedClients": 47,
          "activeGroups": 12,
          "messagesPerSecond": 2.3
        }
      },
      "fileSystem": {
        "status": "Healthy",
        "responseTime": 5,
        "lastCheck": "2024-06-20T15:30:00Z",
        "details": {
          "diskUsage": "32%",
          "availableSpace": "156 GB",
          "inodeUsage": "15%",
          "readWriteTest": "Passed"
        }
      },
      "emailService": {
        "status": "Healthy",
        "responseTime": 145,
        "lastCheck": "2024-06-20T15:30:00Z",
        "details": {
          "provider": "SendGrid",
          "dailyQuota": 10000,
          "usedToday": 234,
          "deliveryRate": "99.2%"
        }
      }
    },
    "criticalDependencies": ["postgresql", "redis"],
    "optionalDependencies": ["emailService"],
    "failureImpact": {
      "postgresql": "Complete system unavailability",
      "redis": "Reduced performance, no caching",
      "signalr": "No real-time notifications",
      "emailService": "No email notifications"
    }
  }
}
```

## 🔧 System Configuration

**GET** `/api/v1/health/config`

Get current system configuration and settings.

**Authorization**: Admin only

**Success Response (200)**:
```json
{
  "success": true,
  "message": "System configuration retrieved",
  "data": {
    "environment": "Production",
    "version": "1.0.0",
    "buildNumber": "20240620.1",
    "deploymentDate": "2024-06-20T08:00:00Z",
    "configuration": {
      "database": {
        "provider": "PostgreSQL",
        "connectionPoolSize": 50,
        "commandTimeout": 30,
        "enableLogging": false
      },
      "redis": {
        "enabled": true,
        "maxMemory": "512MB",
        "defaultTtl": 3600
      },
      "authentication": {
        "jwtExpiration": 24,
        "refreshTokenExpiration": 168,
        "requireEmailConfirmation": true
      },
      "rateLimiting": {
        "enabled": true,
        "requestsPerMinute": 100,
        "burstLimit": 10
      },
      "fileUpload": {
        "maxFileSize": "10MB",
        "allowedTypes": ["image/*", "application/pdf"],
        "storageProvider": "Local"
      },
      "features": {
        "realTimeNotifications": true,
        "analytics": true,
        "advancedSearch": true,
        "fileProcessing": true
      }
    },
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Production",
      "TZ": "UTC",
      "LOG_LEVEL": "Information"
    },
    "resourceLimits": {
      "maxMemory": "1GB",
      "maxCpuPercent": 80,
      "maxDiskUsage": "80%"
    }
  }
}
```

## 📊 System Statistics

**GET** `/api/v1/health/stats`

Get comprehensive system usage statistics.

**Authorization**: Manager, Admin

**Query Parameters**:
- `period` (string): Statistics period ("today", "week", "month", "year")
- `includeHistorical` (bool): Include historical data (default: false)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "System statistics retrieved successfully",
  "data": {
    "period": {
      "start": "2024-06-20T00:00:00Z",
      "end": "2024-06-20T23:59:59Z",
      "description": "Today"
    },
    "usage": {
      "apiCalls": {
        "total": 8742,
        "successful": 8724,
        "failed": 18,
        "authenticated": 8234,
        "anonymous": 508
      },
      "users": {
        "totalRegistered": 156,
        "activeToday": 47,
        "newRegistrations": 3,
        "loginSessions": 89
      },
      "projects": {
        "total": 97,
        "active": 68,
        "completed": 23,
        "created": 2,
        "updated": 34
      },
      "reports": {
        "dailyReports": 23,
        "weeklyPlans": 5,
        "masterPlans": 2,
        "workRequests": 8
      },
      "files": {
        "uploaded": 12,
        "downloaded": 89,
        "totalSize": "145 MB",
        "storageUsed": "14.67 GB"
      }
    },
    "performance": {
      "averageResponseTime": 87,
      "p95ResponseTime": 245,
      "maxResponseTime": 1245,
      "uptime": 99.97,
      "errorRate": 0.21
    },
    "resources": {
      "cpu": {
        "average": 23.5,
        "peak": 67.8,
        "low": 8.2
      },
      "memory": {
        "average": 67.8,
        "peak": 89.3,
        "low": 45.2
      },
      "storage": {
        "used": 32.1,
        "available": 67.9,
        "growth": "+2.3GB this month"
      }
    },
    "trends": {
      "userGrowth": "+12.5%",
      "apiUsageGrowth": "+8.9%",
      "performanceChange": "-5ms avg response time",
      "storageGrowth": "+15.2% this month"
    }
  }
}
```

## 🚨 Health Alerts

**GET** `/api/v1/health/alerts`

Get current system alerts and warnings.

**Authorization**: Manager, Admin

**Query Parameters**:
- `severity` (string): Filter by severity ("info", "warning", "error", "critical")
- `status` (string): Filter by status ("active", "resolved", "acknowledged")
- `service` (string): Filter by service name

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Health alerts retrieved successfully",
  "data": {
    "summary": {
      "total": 5,
      "critical": 0,
      "error": 1,
      "warning": 2,
      "info": 2,
      "active": 3,
      "resolved": 2
    },
    "alerts": [
      {
        "id": "alert001",
        "severity": "Warning",
        "status": "Active",
        "service": "redis",
        "title": "High Memory Usage",
        "message": "Redis memory usage is at 85% of available memory",
        "timestamp": "2024-06-20T15:25:00Z",
        "details": {
          "currentUsage": "435 MB",
          "maxMemory": "512 MB",
          "threshold": "80%"
        },
        "impact": "Performance may be degraded",
        "recommendedAction": "Clear unused cache keys or increase memory limit",
        "acknowledgedBy": null,
        "resolvedAt": null
      },
      {
        "id": "alert002",
        "severity": "Info",
        "status": "Active",
        "service": "api",
        "title": "High API Usage",
        "message": "API request rate is higher than normal",
        "timestamp": "2024-06-20T15:20:00Z",
        "details": {
          "currentRate": 15.2,
          "normalRange": "8-12",
          "threshold": 15.0
        },
        "impact": "No immediate impact",
        "recommendedAction": "Monitor for sustained high usage",
        "acknowledgedBy": {
          "userId": "admin001",
          "fullName": "System Admin",
          "acknowledgedAt": "2024-06-20T15:22:00Z"
        },
        "resolvedAt": null
      }
    ],
    "recentResolved": [
      {
        "id": "alert003",
        "severity": "Warning",
        "status": "Resolved",
        "service": "database",
        "title": "Slow Query Detected",
        "message": "Database query exceeded performance threshold",
        "timestamp": "2024-06-20T14:45:00Z",
        "resolvedAt": "2024-06-20T14:50:00Z",
        "resolution": "Query optimized and index added"
      }
    ]
  }
}
```

## 🔧 Diagnostic Tools

**POST** `/api/v1/health/diagnostics`

Run diagnostic tests on system components.

**Authorization**: Admin only

**Request Body**:
```json
{
  "tests": ["database", "redis", "filesystem", "network"],
  "includePerformanceTest": true,
  "testDuration": 30
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Diagnostic tests completed",
  "data": {
    "testRun": {
      "id": "diag_20240620_153000",
      "startTime": "2024-06-20T15:30:00Z",
      "endTime": "2024-06-20T15:30:45Z",
      "duration": 45,
      "testsRun": 4
    },
    "results": {
      "database": {
        "status": "Passed",
        "duration": 12,
        "tests": {
          "connectivity": "Passed",
          "queryPerformance": "Passed",
          "indexHealth": "Passed",
          "tableLocks": "Passed"
        },
        "metrics": {
          "connectionTime": "8ms",
          "queryTime": "12ms",
          "concurrentConnections": 15
        }
      },
      "redis": {
        "status": "Warning",
        "duration": 8,
        "tests": {
          "connectivity": "Passed",
          "memoryHealth": "Warning",
          "keyExpiration": "Passed",
          "persistence": "Passed"
        },
        "metrics": {
          "responseTime": "3ms",
          "memoryUsage": "85%",
          "hitRate": "94.2%"
        },
        "warnings": ["Memory usage approaching limit"]
      },
      "filesystem": {
        "status": "Passed",
        "duration": 15,
        "tests": {
          "diskSpace": "Passed",
          "readWrite": "Passed",
          "permissions": "Passed",
          "performance": "Passed"
        },
        "metrics": {
          "readSpeed": "125 MB/s",
          "writeSpeed": "89 MB/s",
          "diskUsage": "32%"
        }
      },
      "network": {
        "status": "Passed",
        "duration": 10,
        "tests": {
          "externalConnectivity": "Passed",
          "dnsResolution": "Passed",
          "bandwidth": "Passed",
          "latency": "Passed"
        },
        "metrics": {
          "averageLatency": "45ms",
          "bandwidth": "100 Mbps",
          "packetLoss": "0%"
        }
      }
    },
    "performanceTest": {
      "duration": 30,
      "requestsSent": 300,
      "successfulRequests": 299,
      "failedRequests": 1,
      "averageResponseTime": 89,
      "maxResponseTime": 456,
      "throughput": 9.97
    },
    "recommendations": [
      "Consider increasing Redis memory limit",
      "Monitor database connection pool usage",
      "System performance is within acceptable ranges"
    ]
  }
}
```

## ⚠️ Error Codes

| Code | Message | Description | Solution |
|------|---------|-------------|----------|
| **HM001** | Service unavailable | System component is not responding | Check service status and restart if needed |
| **HM002** | Health check timeout | Health check took too long to complete | Investigate system performance |
| **HM003** | Insufficient permissions | User lacks permission for health data | Check user role and permissions |
| **HM004** | Invalid metric request | Requested metric doesn't exist | Check available metrics |
| **HM005** | Diagnostic failed | Diagnostic test failed to complete | Check system logs for details |
| **HM006** | Configuration error | System configuration is invalid | Review configuration settings |
| **HM007** | Resource limit exceeded | System resource usage too high | Scale resources or optimize usage |
| **HM008** | Database health check failed | Database is not responding properly | Check database connection and status |
| **HM009** | Cache service degraded | Redis/cache service performance issues | Check cache service status |
| **HM010** | Storage health check failed | File storage system issues | Check disk space and permissions |

## 📋 Summary

### Key Features
- **Comprehensive Monitoring**: Multi-level health checks from basic to detailed diagnostics
- **Real-time Metrics**: Live performance data and system statistics
- **Proactive Alerting**: Automated alerts for system issues and performance degradation
- **Dependency Tracking**: Monitor all external service dependencies
- **Diagnostic Tools**: Built-in tools for troubleshooting system issues
- **Performance Analytics**: Historical trends and performance optimization insights

### Monitoring Levels
1. **Basic Health**: Public endpoint for basic system status
2. **Detailed Status**: Comprehensive health information for administrators
3. **Performance Metrics**: Real-time and historical performance data
4. **Dependency Health**: External service dependency monitoring
5. **System Diagnostics**: Advanced diagnostic and testing tools

### Use Cases
- **Operations Monitoring**: Monitor system health and performance in production
- **Troubleshooting**: Diagnose and resolve system issues quickly
- **Capacity Planning**: Analyze usage trends for resource planning
- **SLA Monitoring**: Track system uptime and performance against SLAs
- **Incident Response**: Rapid identification and response to system issues

### Best Practices
- Monitor health endpoints regularly using external monitoring tools
- Set up automated alerts for critical system metrics
- Use performance data to optimize system configuration
- Review diagnostic results to identify potential issues before they become critical
- Maintain historical health data for trend analysis and capacity planning
- Document incident response procedures based on health check results
