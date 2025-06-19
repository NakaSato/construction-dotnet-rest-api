# Production API Testing Documentation

This directory contains comprehensive testing scripts for the Solar Projects API deployed on Azure. These scripts validate functionality, performance, security, and authentication of the production API.

## 🚀 Available Testing Scripts

### 1. Enhanced Comprehensive Testing
**File**: `test-production-api-enhanced.sh`
**Purpose**: Complete API testing suite with detailed validation
**Usage**: `./scripts/test-production-api-enhanced.sh`

**Features**:
- ✅ Core endpoint validation
- ✅ API documentation testing (Swagger)
- ✅ Authentication validation (401 responses)
- ✅ Security testing (HTTP methods, CORS)
- ✅ Rate limiting validation
- ✅ Performance analysis (response times)
- ✅ Error handling validation
- ✅ Comprehensive reporting

**Output**: Generates detailed test report with colored output and saves results to timestamped file.

### 2. Quick Health Check
**File**: `quick-health-check.sh`
**Purpose**: Fast validation of API health and core functionality
**Usage**: `./scripts/quick-health-check.sh`

**Features**:
- ⚡ Fast execution (< 30 seconds)
- ✅ Health endpoint validation
- ✅ Test endpoint validation
- ✅ Authentication check
- ✅ 404 error handling
- ✅ Basic performance check
- ✅ API response details (JSON formatted)

**Use Case**: Quick verification during deployments or CI/CD pipelines.

### 3. Authentication Testing
**File**: `test-auth-endpoints.sh`
**Purpose**: Test authenticated endpoints with JWT tokens
**Usage**: 
```bash
./scripts/test-auth-endpoints.sh                           # Default credentials
./scripts/test-auth-endpoints.sh user@example.com password # Custom credentials
```

**Features**:
- 🔐 JWT token acquisition
- ✅ All authenticated endpoint testing
- ✅ Token security validation
- ✅ Invalid token handling
- ✅ Authorization header validation

**Note**: Requires valid production credentials for full testing.

## 🌐 Production API Information

### Base URLs
- **Production API**: https://solar-projects-api.azurewebsites.net
- **Health Check**: https://solar-projects-api.azurewebsites.net/health
- **API Base**: https://solar-projects-api.azurewebsites.net/api/v1
- **Swagger UI**: https://solar-projects-api.azurewebsites.net/swagger

### Available Endpoints

#### Public Endpoints (No Authentication)
- `GET /health` - API health status
- `GET /api/v1/projects/test` - Public test endpoint

#### Authenticated Endpoints (Require JWT Token)
- `GET /api/v1/projects` - List all projects
- `GET /api/v1/projects/legacy` - Legacy projects list
- `GET /api/v1/projects/{id}` - Get project by ID
- `POST /api/v1/projects` - Create new project
- `PUT /api/v1/projects/{id}` - Update project
- `DELETE /api/v1/projects/{id}` - Delete project
- `GET /api/v1/users` - List users
- `GET /api/v1/daily-reports` - List daily reports
- `GET /api/v1/work-requests` - List work requests
- `GET /api/v1/master-plans` - List master plans
- `GET /api/v1/calendar/events` - Calendar events
- `GET /api/v1/tasks` - List tasks
- `GET /api/v1/documents` - List documents
- `GET /api/v1/resources` - List resources

## 📊 Test Result Interpretation

### Success Indicators
- ✅ **Health endpoint returns 200** - API is running
- ✅ **Test endpoint returns 200** - Core functionality working
- ✅ **Protected endpoints return 401** - Security is active
- ✅ **Response times < 2 seconds** - Good performance
- ✅ **Proper error codes (404, 405)** - Error handling working

### Warning Signs
- ⚠️ **Response times 2-5 seconds** - Acceptable but monitor
- ⚠️ **Some documentation endpoints failing** - May need configuration
- ⚠️ **Rate limiting not triggered** - May have high limits

### Failure Indicators
- ❌ **Health endpoint fails** - API is down or misconfigured
- ❌ **Multiple 500 errors** - Server-side issues
- ❌ **Response times > 5 seconds** - Performance problems
- ❌ **Security checks failing** - Authentication issues

## 🔧 Troubleshooting Guide

### Common Issues and Solutions

#### 1. Swagger UI Not Accessible (404)
**Problem**: https://solar-projects-api.azurewebsites.net/swagger returns 404
**Solutions**:
- Check if Swagger is enabled in production environment
- Verify `appsettings.Production.json` configuration
- Try alternative paths: `/swagger/index.html`, `/api-docs`

#### 2. Authentication Failures
**Problem**: All authenticated endpoints return 401
**Solutions**:
- Verify JWT configuration in production
- Check authentication service implementation
- Ensure valid production credentials
- Test authentication endpoint: `POST /api/v1/auth/login`

#### 3. Slow Response Times
**Problem**: API responses are consistently slow (>3 seconds)
**Solutions**:
- Check Azure App Service plan (scale up if needed)
- Monitor database performance
- Review application logs in Azure portal
- Consider implementing caching

#### 4. Rate Limiting Issues
**Problem**: Unexpected 429 responses or rate limiting not working
**Solutions**:
- Review rate limiting configuration in `appsettings.json`
- Check middleware configuration
- Verify Redis cache if using distributed rate limiting

## 🚀 CI/CD Integration

### Automated Testing in Pipelines

Add to your CI/CD pipeline:

```yaml
# Azure DevOps Pipeline Example
- task: Bash@3
  displayName: 'Quick Health Check'
  inputs:
    targetType: 'inline'
    script: |
      chmod +x scripts/quick-health-check.sh
      ./scripts/quick-health-check.sh

- task: Bash@3
  displayName: 'Comprehensive API Test'
  inputs:
    targetType: 'inline'
    script: |
      chmod +x scripts/test-production-api-enhanced.sh
      ./scripts/test-production-api-enhanced.sh
  condition: succeededOrFailed()
```

### GitHub Actions Example

```yaml
- name: Production API Health Check
  run: |
    chmod +x scripts/quick-health-check.sh
    ./scripts/quick-health-check.sh

- name: Comprehensive API Testing
  run: |
    chmod +x scripts/test-production-api-enhanced.sh
    ./scripts/test-production-api-enhanced.sh
  if: always()
```

## 📈 Performance Baselines

### Expected Response Times
- **Health Check**: < 1 second
- **Test Endpoint**: < 2 seconds
- **Authenticated Endpoints**: < 3 seconds
- **List Endpoints**: < 5 seconds

### Performance Optimization Tips
1. **Enable Response Caching**: Implement `[ResponseCache]` attributes
2. **Database Optimization**: Use proper indexing and query optimization
3. **Azure Configuration**: Use appropriate App Service plan
4. **Connection Pooling**: Optimize Entity Framework configuration
5. **CDN Usage**: For static content and API responses

## 🔍 Monitoring and Alerting

### Recommended Monitoring
1. **Application Insights**: Enable for detailed telemetry
2. **Health Checks**: Monitor `/health` endpoint continuously
3. **Response Time Alerts**: Alert if average response time > 3s
4. **Error Rate Monitoring**: Alert if error rate > 5%
5. **Availability Monitoring**: External monitoring of key endpoints

### Key Metrics to Track
- API availability (uptime percentage)
- Average response time
- Error rate by endpoint
- Request volume
- Authentication success/failure rate

## 🔧 Script Customization

### Adding New Tests
To add custom tests to the enhanced script:

1. **Create test function**:
```bash
test_custom_feature() {
    print_section "Custom Feature Tests"
    test_endpoint "Custom Endpoint" "$API_BASE/custom" "200"
}
```

2. **Add to main execution**:
```bash
test_custom_feature
```

### Environment-Specific Configuration
Create environment-specific scripts:

```bash
# Copy base script
cp test-production-api-enhanced.sh test-staging-api.sh

# Modify URL configuration
PROD_URL="https://staging-solar-projects-api.azurewebsites.net"
```

## 📞 Support and Troubleshooting

### Log Locations
- **Application Logs**: Azure Portal → App Service → Log stream
- **Test Results**: Local `test-results-YYYYMMDD-HHMMSS.txt` files
- **CI/CD Logs**: Build pipeline logs in Azure DevOps or GitHub Actions

### Getting Help
1. **Check Azure Portal**: Application Insights and App Service logs
2. **Run Diagnostic Tests**: Use comprehensive testing script
3. **Verify Configuration**: Check `appsettings.Production.json`
4. **Database Connectivity**: Verify connection strings and database status

### Emergency Procedures
If production API is down:
1. Run quick health check to confirm issue
2. Check Azure service status
3. Review recent deployments
4. Check application logs in Azure Portal
5. Verify database connectivity
6. Consider rollback if recent deployment caused issue

---

*Last Updated: June 2025*
*For questions or issues, refer to the project documentation or contact the development team.*
