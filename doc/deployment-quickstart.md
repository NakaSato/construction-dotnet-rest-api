# Quick Start: Deploy Solar Projects API to Azure

This is a simplified guide to get your Solar Projects API deployed to Azure quickly.

## Prerequisites

- Azure subscription
- GitHub repository with the code
- Azure CLI installed locally

## Step 1: Setup Azure Resources

Run the automated setup script:

```bash
./scripts/setup-azure-resources.sh
```

This creates:
- Resource Group
- Azure Container Registry
- PostgreSQL Database
- App Service
- Application Insights

## Step 2: Configure GitHub Secrets

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

### Required Secrets:
```bash
# Azure credentials (from service principal)
AZURE_CREDENTIALS: '{"clientId":"...","clientSecret":"...","tenantId":"...","subscriptionId":"..."}'

# Azure resources
AZURE_RESOURCE_GROUP: 'rg-solar-projects-production'
AZURE_WEBAPP_NAME: 'solar-projects-api-production'
ACR_NAME: 'solarprojacr'

# Database
AZURE_DB_CONNECTION_STRING: 'Host=your-db.postgres.database.azure.com;Database=SolarProjectsDb;Username=solaradmin;Password=your-password;Port=5432;SSL Mode=Require;'
DB_ADMIN_PASSWORD: 'your-secure-password'

# JWT
JWT_SECRET_KEY: 'your-long-random-secret-key'
JWT_ISSUER: 'SolarProjectsAPI'
JWT_AUDIENCE: 'SolarProjectsClients'

# Cloudinary
CLOUDINARY_CLOUD_NAME: 'your-cloud-name'
CLOUDINARY_API_KEY: 'your-api-key'
CLOUDINARY_API_SECRET: 'your-api-secret'
```

## Step 3: Deploy

1. **Deploy Infrastructure** (first time only):
   - Go to GitHub Actions
   - Run "Infrastructure - Deploy Azure Resources"
   - Choose production environment

2. **Deploy Application**:
   - Push code to `main` branch
   - GitHub Actions will automatically build and deploy

## Step 4: Verify Deployment

Check these URLs:
- Health: `https://your-app-name.azurewebsites.net/health`
- Swagger: `https://your-app-name.azurewebsites.net/swagger`

## Available GitHub Actions Workflows

### Continuous Integration (ci.yml)
- Runs on every push/PR
- Builds, tests, and scans code
- Builds Docker image

### Continuous Deployment (deploy.yml)
- Runs on push to main
- Deploys to Azure App Service
- Updates configuration
- Runs health checks

### Infrastructure (infrastructure.yml)
- Manual trigger only
- Creates/updates Azure resources
- Use for initial setup

### Database Migration (database-migration.yml)
- Manual trigger only
- Runs EF Core migrations
- Use when database schema changes

### Security Scan (security-scan.yml)
- Runs weekly + on pushes
- CodeQL analysis
- Vulnerability scanning

### Performance Test (performance-test.yml)
- Manual trigger only
- Load testing with k6
- Performance benchmarking

## Monitoring and Troubleshooting

### View Logs
```bash
az webapp log tail --name your-app-name --resource-group your-rg
```

### Check App Status
```bash
az webapp show --name your-app-name --resource-group your-rg --query state
```

### Restart App
```bash
az webapp restart --name your-app-name --resource-group your-rg
```

## Cost Optimization

Current setup uses:
- **App Service**: Basic B1 (~$13/month)
- **PostgreSQL**: Burstable B1ms (~$12/month)
- **Container Registry**: Basic (~$5/month)
- **Application Insights**: Pay-as-you-go

**Total**: ~$30/month for production

For development, use smaller SKUs or auto-shutdown policies to reduce costs.

## Security Best Practices

✅ **Implemented**:
- HTTPS only
- Managed identities where possible
- Environment variables for secrets
- Regular security scanning

🔧 **Recommended**:
- Use Azure Key Vault for secrets
- Configure VNet integration
- Set up Azure Firewall
- Enable Azure Defender

## Next Steps

1. **Set up monitoring alerts** in Application Insights
2. **Configure backup policies** for the database
3. **Set up staging environment** for testing
4. **Implement blue-green deployment** for zero-downtime updates
5. **Add custom domain** and SSL certificate

## Support

If you encounter issues:
1. Check the GitHub Actions logs
2. Review Application Insights metrics
3. Check App Service logs
4. Verify all secrets are configured correctly

For detailed documentation, see [azure-deployment.md](azure-deployment.md)
