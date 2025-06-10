# 🚀 Azure Deployment - Ready to Deploy!

## ✅ Azure Resources Status
All Azure resources are created and ready:
- ✅ Resource Group: `rg-solar-projects-dev`
- ✅ Container Registry: `solarprojacr1749359812.azurecr.io`
- ✅ PostgreSQL Database: `solar-db-1749364020.postgres.database.azure.com`
- ✅ App Service: `solar-projects-api-dev.azurewebsites.net`
- ✅ Application Insights configured

## 🔑 GitHub Secrets Configuration

Go to: **https://github.com/NakaSato/dotnet-rest-api/settings/secrets/actions**

Click **"New repository secret"** and add each of the following:

### 1. AZURE_CREDENTIALS
```json
{
  "clientId": "82273b42-0323-4d98-8284-55ecc60bf6f8",
  "clientSecret": "[USE_ACTUAL_CLIENT_SECRET_FROM_TERMINAL_OUTPUT]",
  "subscriptionId": "ea02bfff-4d3f-4d0c-ac62-613e82d307b7",
  "tenantId": "0ecb7c82-1b84-4b36-adef-2081b5c1125b",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```
💡 **Get the actual clientSecret by running**: `./azure-setup.sh`

### 2. AZURE_SUBSCRIPTION_ID
```
ea02bfff-4d3f-4d0c-ac62-613e82d307b7
```

### 3. AZURE_RESOURCE_GROUP
```
rg-solar-projects-dev
```

### 4. AZURE_WEBAPP_NAME
```
solar-projects-api-dev
```

### 5. ACR_NAME
```
solarprojacr1749359812
```

### 6. AZURE_DB_CONNECTION_STRING
```
Server=solar-db-1749364020.postgres.database.azure.com;Database=SolarProjectsDb;User Id=solaradmin;Password=YOUR_DB_PASSWORD;Port=5432;SSL Mode=Require;
```
⚠️ **Note**: Replace `YOUR_DB_PASSWORD` with the actual database password you used when creating the PostgreSQL server.

### 7. JWT_SECRET_KEY
```
SolarProjects2024-SuperSecretKey-ForJWT-Authentication-Security-32Plus-Characters!
```

### 8. JWT_ISSUER
```
https://solar-projects-api-dev.azurewebsites.net
```

### 9. JWT_AUDIENCE
```
https://solar-projects-api-dev.azurewebsites.net
```

## 🚀 Deploy to Azure

Once all secrets are configured:

### Option 1: Manual Deployment (Recommended)
1. Go to: **https://github.com/NakaSato/dotnet-rest-api/actions**
2. Click on **"CD - Deploy to Azure"** workflow
3. Click **"Run workflow"** button
4. Select **"production"** environment
5. Click **"Run workflow"**

### Option 2: Automatic Deployment
Push any changes to the `main` branch to trigger automatic deployment.

## 📊 Monitor Deployment

### GitHub Actions
- **Workflow Status**: https://github.com/NakaSato/dotnet-rest-api/actions
- **Build Logs**: Available in the workflow run details

### Azure Endpoints
- **App URL**: https://solar-projects-api-dev.azurewebsites.net
- **Health Check**: https://solar-projects-api-dev.azurewebsites.net/health
- **API Documentation**: https://solar-projects-api-dev.azurewebsites.net/swagger
- **OpenAPI Spec**: https://solar-projects-api-dev.azurewebsites.net/swagger/v1/swagger.json

### Azure Portal
- **App Service**: https://portal.azure.com/#resource/subscriptions/ea02bfff-4d3f-4d0c-ac62-613e82d307b7/resourceGroups/rg-solar-projects-dev/providers/Microsoft.Web/sites/solar-projects-api-dev
- **Container Registry**: https://portal.azure.com/#resource/subscriptions/ea02bfff-4d3f-4d0c-ac62-613e82d307b7/resourceGroups/rg-solar-projects-dev/providers/Microsoft.ContainerRegistry/registries/solarprojacr1749359812
- **Database**: https://portal.azure.com/#resource/subscriptions/ea02bfff-4d3f-4d0c-ac62-613e82d307b7/resourceGroups/rg-solar-projects-dev/providers/Microsoft.DBforPostgreSQL/flexibleServers/solar-db-1749364020

## 🔧 Deployment Process

The CI/CD pipeline will:
1. ✅ Build the .NET application
2. ✅ Run tests
3. ✅ Build Docker image
4. ✅ Push to Azure Container Registry
5. ✅ Deploy to Azure App Service
6. ✅ Update configuration
7. ✅ Restart the app
8. ✅ Run health checks
9. ✅ Execute smoke tests

## 🎯 Expected Results

After successful deployment:
- **API Available**: https://solar-projects-api-dev.azurewebsites.net
- **Swagger UI**: Interactive API documentation
- **Authentication**: JWT-based auth system
- **Database**: PostgreSQL with Entity Framework migrations applied
- **Features**: Complete solar project management API with calendar functionality

## 🆘 Troubleshooting

If deployment fails:

1. **Check GitHub Secrets**: Ensure all secrets are correctly configured
2. **View Logs**: Check GitHub Actions workflow logs for specific errors
3. **Azure Logs**: Check App Service logs in Azure Portal
4. **Database Connection**: Verify the database password in the connection string
5. **Container Registry**: Ensure ACR credentials are correct

## 📝 Database Password Recovery

If you don't remember the database password, you can reset it:
```bash
az postgres flexible-server update --resource-group rg-solar-projects-dev --name solar-db-1749364020 --admin-password "NewPassword123!"
```

## 🔄 Re-deployment

To redeploy after configuration changes:
1. Update the necessary GitHub secrets
2. Trigger the workflow again
3. Monitor the deployment progress

---

**Next Step**: Configure the GitHub secrets and run the deployment workflow!
