# Azure Deployment Guide

This guide explains how to deploy the Solar Projects API to Azure using GitHub Actions.

## Prerequisites

1. **Azure Account**: Active Azure subscription
2. **GitHub Repository**: Code hosted on GitHub
3. **Azure CLI**: For local setup and testing

## Setup Steps

### 1. Create Azure Service Principal

```bash
# Login to Azure
az login

# Create service principal for GitHub Actions
az ad sp create-for-rbac \
  --name "github-actions-solar-projects" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID \
  --sdk-auth
```

Save the JSON output as a GitHub secret named `AZURE_CREDENTIALS`.

### 2. Required GitHub Secrets

Add the following secrets to your GitHub repository:

#### Azure Configuration
- `AZURE_CREDENTIALS` - Service principal JSON from step 1
- `AZURE_RESOURCE_GROUP` - Name of your Azure resource group (e.g., "rg-solar-projects-production")
- `AZURE_WEBAPP_NAME` - Name of your Azure App Service (e.g., "solar-projects-api-production")
- `ACR_NAME` - Azure Container Registry name (e.g., "solarprojacr")

#### Database Configuration
- `AZURE_DB_CONNECTION_STRING` - PostgreSQL connection string
- `DB_ADMIN_PASSWORD` - Database administrator password

#### Application Configuration
- `JWT_SECRET_KEY` - JWT signing key (generate a secure random string)
- `JWT_ISSUER` - JWT issuer (e.g., "SolarProjectsAPI")
- `JWT_AUDIENCE` - JWT audience (e.g., "SolarProjectsClients")

#### Cloud Storage Configuration
- `CLOUDINARY_CLOUD_NAME` - Cloudinary cloud name
- `CLOUDINARY_API_KEY` - Cloudinary API key
- `CLOUDINARY_API_SECRET` - Cloudinary API secret

### 3. Deploy Infrastructure

Run the Infrastructure workflow manually from GitHub Actions:

1. Go to your repository → Actions
2. Select "Infrastructure - Deploy Azure Resources"
3. Click "Run workflow"
4. Choose environment (staging/production)

This will create:
- Resource Group
- Azure Container Registry
- PostgreSQL Flexible Server
- App Service Plan
- App Service
- Application Insights

### 4. Deploy Application

After infrastructure is ready:

1. Push code to `main` branch
2. CI workflow will run automatically
3. CD workflow will deploy to Azure

## Azure Resources Created

### App Service
- **Name**: `solar-projects-api-{environment}`
- **SKU**: B1 (Basic)
- **Platform**: Linux containers
- **Health Check**: `/health`

### PostgreSQL Database
- **Name**: `solar-projects-db-{environment}`
- **SKU**: Standard_B1ms (Burstable)
- **Version**: PostgreSQL 16
- **Storage**: 32GB

### Container Registry
- **Name**: `solarprojacr{environment}`
- **SKU**: Basic
- **Admin**: Enabled

### Application Insights
- **Name**: `solar-projects-insights-{environment}`
- **Type**: Web application

## Monitoring and Logging

### Health Checks
- **Basic**: `GET /health`
- **Detailed**: `GET /health/detailed`

### Application Insights
Monitor application performance, errors, and usage through Azure Application Insights.

### Log Streaming
View real-time logs:
```bash
az webapp log tail \
  --resource-group rg-solar-projects-production \
  --name solar-projects-api-production
```

## Scaling

### Vertical Scaling
Change App Service Plan SKU:
```bash
az appservice plan update \
  --resource-group rg-solar-projects-production \
  --name solar-projects-plan-production \
  --sku S1
```

### Horizontal Scaling
Scale out instances:
```bash
az webapp update \
  --resource-group rg-solar-projects-production \
  --name solar-projects-api-production \
  --set instanceCount=2
```

## Security Best Practices

1. **Network Security**
   - Configure VNet integration
   - Use private endpoints for database
   - Enable Web Application Firewall

2. **Identity and Access**
   - Use managed identities
   - Configure RBAC properly
   - Rotate secrets regularly

3. **Data Protection**
   - Enable SSL/TLS
   - Configure backup policies
   - Use Azure Key Vault for secrets

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Check firewall rules
   - Verify connection string
   - Ensure database is running

2. **Container Startup Issues**
   - Check application logs
   - Verify environment variables
   - Test Docker image locally

3. **Performance Issues**
   - Monitor Application Insights
   - Check resource utilization
   - Consider scaling up/out

### Useful Commands

```bash
# View app service logs
az webapp log download \
  --resource-group rg-solar-projects-production \
  --name solar-projects-api-production

# Restart app service
az webapp restart \
  --resource-group rg-solar-projects-production \
  --name solar-projects-api-production

# Check app service status
az webapp show \
  --resource-group rg-solar-projects-production \
  --name solar-projects-api-production \
  --query state
```

## Cost Optimization

1. **Right-sizing Resources**
   - Monitor usage patterns
   - Adjust SKUs based on demand
   - Use auto-scaling

2. **Development/Staging**
   - Use smaller SKUs for non-production
   - Implement auto-shutdown for dev environments
   - Share resources where possible

3. **Monitoring**
   - Set up cost alerts
   - Review Azure Advisor recommendations
   - Use Azure Cost Management
