# CI/CD Pipeline Documentation

## 🚀 Overview

This project uses GitHub Actions for Continuous Integration and Continuous Deployment (CI/CD). The pipeline is designed to ensure code quality, run comprehensive tests, build Docker images, and deploy to Azure.

## 📋 Pipeline Structure

### Main Workflows

1. **Main Pipeline** (`main-pipeline.yml`) - Complete CI/CD workflow
2. **CI Build & Test** (`ci.yml`) - Comprehensive testing with database
3. **Simple CI** (`ci-simple.yml`) - Fast build and test for feature branches
4. **Deploy** (`deploy.yml`) - Azure deployment workflow
5. **CodeQL** (`codeql.yml`) - Security analysis
6. **Database Migration** (`database-migration.yml`) - Database updates

## 🔧 Fixed Issues

### Previous Issues Resolved:
- ✅ Updated all GitHub Actions to latest versions
- ✅ Fixed .NET version consistency across all workflows
- ✅ Improved database connection handling
- ✅ Enhanced error handling and logging
- ✅ Added proper Docker testing with network isolation
- ✅ Fixed PostgreSQL health checks
- ✅ Added comprehensive test result uploads
- ✅ Improved cache strategies for better performance

### Key Improvements:
- **Better Error Handling**: All workflows now include comprehensive error handling
- **Database Testing**: Proper PostgreSQL setup with health checks
- **Docker Integration**: Complete Docker build and test pipeline
- **Version Management**: Consistent .NET 9.0.x across all environments
- **Security**: Enhanced CodeQL configuration and security scanning

## 🏗️ Workflow Details

### 1. Main Pipeline (`main-pipeline.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main`

**Jobs:**
1. **Test Job**
   - Sets up PostgreSQL service
   - Builds and tests the application
   - Runs database migrations
   - Uploads test results and coverage

2. **Docker Job**
   - Builds Docker image
   - Tests image locally with database
   - Pushes to GitHub Container Registry (main branch only)

3. **Deploy Job** (main branch only)
   - Deploys to Azure Web App
   - Verifies deployment health

### 2. CI Build & Test (`ci.yml`)

**Enhanced Features:**
- PostgreSQL service with proper health checks
- Environment variable management
- Comprehensive testing with coverage
- Docker build and test with isolated network
- Artifact uploads for debugging

### 3. Simple CI (`ci-simple.yml`)

**Use Case:** Fast feedback for feature branches
- Basic build and test
- Minimal database setup
- Quick Docker validation

## 🔐 Required Secrets

Set these secrets in your GitHub repository:

```bash
# Azure Deployment
AZURE_CREDENTIALS='{
  "clientId": "<client-id>",
  "clientSecret": "<client-secret>", 
  "subscriptionId": "<subscription-id>",
  "tenantId": "<tenant-id>"
}'

# Optional: Enhanced reporting
CODECOV_TOKEN=<your-codecov-token>
```

## 🗄️ Database Configuration

### PostgreSQL Service
- **Image**: `postgres:16`
- **Test Database**: `SolarProjectsDb_Test`
- **Credentials**: `testuser/testpassword`
- **Health Checks**: Configured with retries

### Migration Handling
```bash
# Automatic in CI/CD
dotnet ef database update --verbose

# Manual migration creation
dotnet ef migrations add <MigrationName>
```

## 🐳 Docker Configuration

### Multi-stage Build
- **Base**: `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Build**: `mcr.microsoft.com/dotnet/sdk:9.0`
- **Port**: 8080
- **Environment**: Configurable via environment variables

### Testing Strategy
1. Build image with GitHub SHA tag
2. Create isolated test network
3. Start PostgreSQL container
4. Start application container
5. Run health checks
6. Clean up resources

## 📊 Monitoring & Quality

### Code Coverage
- **Tool**: XPlat Code Coverage
- **Upload**: Codecov integration
- **Reports**: Available in GitHub Actions artifacts

### Security Scanning
- **CodeQL Analysis**: Automated security scanning
- **Configuration**: `.github/codeql/codeql-config.yml`
- **Scope**: Controllers, Services, Models, DTOs, Data

### Performance Testing
- **Load Testing**: Available via `performance-test.yml`
- **Metrics**: Response times, throughput, error rates

## 🚀 Deployment Process

### Azure Web App Deployment
1. **Build**: Application compiled and published
2. **Test**: Unit and integration tests pass
3. **Security**: Security scans complete
4. **Deploy**: Package deployed to Azure
5. **Verify**: Health checks confirm deployment

### Environment Configuration
- **Development**: Local development with Docker
- **Staging**: Azure staging slot (optional)
- **Production**: Azure production environment

## 🔍 Troubleshooting

### Common Issues & Solutions

#### 1. Build Failures
```bash
# Check .NET version compatibility
dotnet --version
# Ensure NuGet packages are up to date
dotnet restore --force
```

#### 2. Database Connection Issues
```bash
# Verify connection string format
ConnectionStrings__DefaultConnection="Host=localhost;Database=TestDb;Username=user;Password=pass"
# Check PostgreSQL service health
pg_isready -h localhost -p 5432
```

#### 3. Docker Build Problems
```bash
# Test Docker build locally
docker build -t test-image .
# Check container logs
docker logs <container-name>
```

#### 4. Azure Deployment Issues
```bash
# Verify Azure credentials
az login --service-principal -u <client-id> -p <client-secret> --tenant <tenant-id>
# Check resource group and app service
az webapp list --resource-group <rg-name>
```

## 📈 Performance Optimizations

### Build Optimizations
- **NuGet Caching**: Reduces restore time by ~60%
- **Docker Layer Caching**: GitHub Actions cache integration
- **Parallel Jobs**: Independent job execution
- **Conditional Execution**: Skip unnecessary steps

### Resource Usage
- **Runner**: Ubuntu Latest (cost-effective)
- **Timeouts**: Configured to prevent hanging jobs
- **Cleanup**: Automatic resource cleanup after tests

## 🎯 Best Practices

### Branch Strategy
- **Main**: Production-ready code, triggers full deployment
- **Develop**: Integration branch, triggers comprehensive testing
- **Feature**: Feature branches, triggers fast CI

### Pull Request Workflow
1. Feature branch triggers Simple CI
2. PR to main triggers full CI pipeline
3. Manual review required before merge
4. Main branch triggers deployment

### Security Guidelines
- Never commit secrets to repository
- Use GitHub secrets for sensitive data
- Regular security scanning enabled
- Dependency vulnerability checks

## 📚 Additional Resources

### Workflow Files Location
```
.github/
├── workflows/
│   ├── main-pipeline.yml      # Complete CI/CD
│   ├── ci.yml                 # Comprehensive testing
│   ├── ci-simple.yml          # Fast feedback
│   ├── deploy.yml             # Azure deployment
│   ├── codeql.yml             # Security analysis
│   └── database-migration.yml # DB migrations
└── codeql/
    └── codeql-config.yml       # Security scan config
```

### Scripts
- `scripts/check-cicd.sh` - Pipeline health check
- `scripts/testing/` - Test automation scripts

### Documentation
- This file - Complete CI/CD documentation
- `README.md` - Project overview and setup
- `doc/ci-cd.md` - Detailed technical documentation

---

## 🎉 Summary

The CI/CD pipeline is now optimized for:
- ✅ **Reliability**: Robust error handling and retries
- ✅ **Speed**: Efficient caching and parallel execution
- ✅ **Security**: Comprehensive scanning and secret management
- ✅ **Quality**: Automated testing and code coverage
- ✅ **Deployment**: Automated Azure deployment with verification

The pipeline supports modern development workflows with fast feedback loops and comprehensive quality gates.
