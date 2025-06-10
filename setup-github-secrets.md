# GitHub Secrets Setup for Azure Deployment

To deploy your .NET REST API to Azure, you need to configure the following GitHub secrets in your repository.

## 🔧 How to Set GitHub Secrets

1. Go to your GitHub repository
2. Click on **Settings** tab
3. Navigate to **Secrets and variables** → **Actions**
4. Click **New repository secret** for each secret below

## 📋 Required GitHub Secrets

### 1. AZURE_CREDENTIALS
```json
{
  "clientId": "YOUR_CLIENT_ID",
  "clientSecret": "YOUR_CLIENT_SECRET", 
  "subscriptionId": "ea02bfff-4d3f-4d0c-ac62-613e82d307b7",
  "tenantId": "0ecb7c82-1b84-4b36-adef-2081b5c1125b"
}
```
*Note: Get the actual clientId and clientSecret from your Azure service principal*

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
*Note: Replace YOUR_DB_PASSWORD with the actual database password*

### 7. JWT_SECRET_KEY
```
your-super-secret-jwt-key-with-at-least-32-characters-for-security
```

### 8. JWT_ISSUER
```
https://solar-projects-api-dev.azurewebsites.net
```

### 9. JWT_AUDIENCE
```
https://solar-projects-api-dev.azurewebsites.net
```

### 10. CLOUDINARY_CLOUD_NAME (Optional)
```
your-cloudinary-cloud-name
```

### 11. CLOUDINARY_API_KEY (Optional)
```
your-cloudinary-api-key
```

### 12. CLOUDINARY_API_SECRET (Optional)
```
your-cloudinary-api-secret
```

## 🚀 Deployment Steps

Once all secrets are configured:

1. **Manual Deployment**: Go to Actions tab → Select "CD - Deploy to Azure" → Run workflow
2. **Automatic Deployment**: Push changes to main branch to trigger deployment

## 📊 Monitoring Deployment

- **GitHub Actions**: Monitor deployment progress in the Actions tab
- **Azure Portal**: Check App Service logs and metrics
- **Health Check**: https://solar-projects-api-dev.azurewebsites.net/health
- **API Docs**: https://solar-projects-api-dev.azurewebsites.net/swagger

## 🔍 Troubleshooting

If deployment fails:
1. Check GitHub Actions logs for specific errors
2. Verify all secrets are correctly set
3. Check Azure App Service logs in Azure Portal
4. Ensure database connection string is correct
5. Verify container registry credentials

## 📁 Key Files

- **Workflow**: `.github/workflows/deploy.yml`
- **Docker**: `Dockerfile`
- **Config**: `appsettings.json`, `appsettings.Production.json`
- **Startup**: `Program.cs`
