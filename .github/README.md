# GitHub Actions CI/CD for Solar Projects API

This directory contains GitHub Actions workflows for continuous integration and deployment to Azure.

## Workflows Overview

### 📋 Available Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| **ci.yml** | Push/PR to main/develop | Build, test, security scan |
| **deploy.yml** | Push to main | Deploy to Azure App Service |
| **infrastructure.yml** | Manual | Create/update Azure resources |
| **database-migration.yml** | Manual | Run database migrations |
| **security-scan.yml** | Weekly + Push | Security scanning |
| **performance-test.yml** | Manual | Load testing |

### 🚀 Quick Setup

1. **Fork/Clone** this repository
2. **Set up Azure resources** using `./scripts/setup-azure-resources.sh`
3. **Configure GitHub Secrets** (see list below)
4. **Push to main branch** to trigger deployment

### 🔐 Required GitHub Secrets

```bash
# Azure Configuration
AZURE_CREDENTIALS           # Service principal JSON
AZURE_RESOURCE_GROUP         # Azure resource group name
AZURE_WEBAPP_NAME           # App Service name
ACR_NAME                    # Container Registry name

# Database
AZURE_DB_CONNECTION_STRING  # PostgreSQL connection string
DB_ADMIN_PASSWORD           # Database password

# Application
JWT_SECRET_KEY              # JWT signing key
JWT_ISSUER                  # JWT issuer
JWT_AUDIENCE                # JWT audience

# Cloud Storage
CLOUDINARY_CLOUD_NAME       # Cloudinary cloud name
CLOUDINARY_API_KEY          # Cloudinary API key
CLOUDINARY_API_SECRET       # Cloudinary secret
```

### 📊 Workflow Details

#### CI Workflow (ci.yml)
- **Triggers**: Push/PR to main/develop branches
- **Steps**:
  - Checkout code
  - Setup .NET 9.0
  - Restore dependencies
  - Build application
  - Run unit tests
  - Security scan with CodeQL
  - Build Docker image
  - Upload test coverage

#### Deploy Workflow (deploy.yml)
- **Triggers**: Push to main branch, manual
- **Steps**:
  - Build and test application
  - Build Docker image
  - Push to Azure Container Registry
  - Deploy to Azure App Service
  - Update configuration
  - Health checks
  - Smoke tests

#### Infrastructure Workflow (infrastructure.yml)
- **Triggers**: Manual only
- **Steps**:
  - Create resource group
  - Deploy ARM template
  - Set up Container Registry
  - Configure PostgreSQL
  - Create App Service
  - Set up monitoring

### 🛡️ Security Features

- **CodeQL scanning** for code vulnerabilities
- **Container scanning** with Trivy
- **Dependency scanning** for vulnerable packages
- **Secret scanning** (GitHub native)
- **HTTPS enforcement** in production

### 📈 Monitoring & Observability

- **Application Insights** integration
- **Health checks** on deployment
- **Performance testing** with k6
- **Log streaming** from Azure
- **Automated alerts** on failures

### 🔧 Development Workflow

1. **Feature Development**:
   ```bash
   git checkout -b feature/new-feature
   git commit -am "Add new feature"
   git push origin feature/new-feature
   ```

2. **Create Pull Request**:
   - CI workflow runs automatically
   - Code review and approval required
   - Merge to main triggers deployment

3. **Production Deployment**:
   - Automatic on merge to main
   - Manual deployment available
   - Rollback capability

### 🧪 Testing Locally

Run the local test script before pushing:

```bash
./scripts/test-local.sh
```

This checks:
- ✅ .NET SDK installation
- ✅ Docker availability
- ✅ Application builds
- ✅ Unit tests pass
- ✅ Docker image builds
- ✅ Configuration files
- ✅ GitHub Actions workflows

### 🏗️ Infrastructure as Code

Azure resources are defined in:
- `azure/arm-template.json` - ARM template
- `scripts/setup-azure-resources.sh` - Automated setup
- GitHub Actions workflows use Azure CLI

### 📚 Documentation

- [Deployment Quick Start](../doc/deployment-quickstart.md)
- [Azure Deployment Guide](../doc/azure-deployment.md)
- [API Documentation](../doc/api.md)

### 🆘 Troubleshooting

#### Common Issues:

1. **Build Failures**:
   - Check .NET version compatibility
   - Verify NuGet package references
   - Review build logs in GitHub Actions

2. **Deployment Failures**:
   - Verify all GitHub secrets are set
   - Check Azure resource availability
   - Review App Service logs

3. **Health Check Failures**:
   - Verify database connectivity
   - Check application configuration
   - Review container logs

#### Debug Commands:

```bash
# View deployment logs
az webapp log tail --name your-app --resource-group your-rg

# Check app status
az webapp show --name your-app --resource-group your-rg

# Restart application
az webapp restart --name your-app --resource-group your-rg
```

### 🎯 Best Practices

1. **Environment Separation**:
   - Use different resource groups for staging/production
   - Separate GitHub environments
   - Different configuration values

2. **Security**:
   - Rotate secrets regularly
   - Use managed identities where possible
   - Monitor security alerts

3. **Performance**:
   - Monitor Application Insights metrics
   - Run performance tests regularly
   - Optimize Docker images

4. **Cost Management**:
   - Use appropriate SKUs for each environment
   - Monitor Azure costs
   - Implement auto-shutdown for dev environments

### 📞 Support

For issues or questions:
1. Check the troubleshooting section above
2. Review GitHub Actions logs
3. Check Azure portal for resource status
4. Create an issue in this repository
