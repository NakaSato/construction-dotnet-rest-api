#!/bin/bash

# Azure Deployment Setup Script
# This script helps you get the Azure credentials and setup information needed for GitHub secrets

echo "🚀 Azure Deployment Setup for Solar Projects API"
echo "================================================"

echo ""
echo "📋 Step 1: Get Azure Service Principal Credentials"
echo "Run the following command to get your service principal credentials:"
echo ""
echo "az ad sp create-for-rbac --name 'solar-projects-api-deploy' --role contributor --scopes /subscriptions/ea02bfff-4d3f-4d0c-ac62-613e82d307b7/resourceGroups/rg-solar-projects-dev --json-auth"
echo ""
echo "This will output JSON credentials that you'll use for the AZURE_CREDENTIALS secret."
echo ""

echo "📋 Step 2: Get Database Password"
echo "Run the following command to get your database admin password:"
echo ""
echo "az postgres flexible-server show --resource-group rg-solar-projects-dev --name solar-db-1749364020 --query administratorLogin"
echo ""
echo "Note: You'll need the password you used when creating the database."
echo ""

echo "📋 Step 3: Get Container Registry Password"
echo "Run the following command to get your ACR password:"
echo ""
echo "az acr credential show --resource-group rg-solar-projects-dev --name solarprojacr1749359812 --query 'passwords[0].value' -o tsv"
echo ""

echo "📋 Step 4: GitHub Secrets Configuration"
echo "Go to: https://github.com/NakaSato/dotnet-rest-api/settings/secrets/actions"
echo ""
echo "Create the following secrets:"
echo ""

cat << 'EOF'
Secret Name: AZURE_CREDENTIALS
Secret Value: {JSON output from step 1}

Secret Name: AZURE_SUBSCRIPTION_ID
Secret Value: ea02bfff-4d3f-4d0c-ac62-613e82d307b7

Secret Name: AZURE_RESOURCE_GROUP
Secret Value: rg-solar-projects-dev

Secret Name: AZURE_WEBAPP_NAME
Secret Value: solar-projects-api-dev

Secret Name: ACR_NAME
Secret Value: solarprojacr1749359812

Secret Name: AZURE_DB_CONNECTION_STRING
Secret Value: Server=solar-db-1749364020.postgres.database.azure.com;Database=SolarProjectsDb;User Id=solaradmin;Password=YOUR_DB_PASSWORD;Port=5432;SSL Mode=Require;

Secret Name: JWT_SECRET_KEY
Secret Value: your-super-secret-jwt-key-with-at-least-32-characters-for-security-purposes

Secret Name: JWT_ISSUER
Secret Value: https://solar-projects-api-dev.azurewebsites.net

Secret Name: JWT_AUDIENCE
Secret Value: https://solar-projects-api-dev.azurewebsites.net
EOF

echo ""
echo "📋 Step 5: Optional Cloudinary Secrets (for image upload)"
echo "If you want to enable image upload functionality, also add:"
echo ""
echo "CLOUDINARY_CLOUD_NAME: your-cloudinary-cloud-name"
echo "CLOUDINARY_API_KEY: your-cloudinary-api-key"
echo "CLOUDINARY_API_SECRET: your-cloudinary-api-secret"
echo ""

echo "📋 Step 6: Deploy"
echo "After setting up all secrets, trigger deployment by:"
echo "1. Going to: https://github.com/NakaSato/dotnet-rest-api/actions"
echo "2. Select 'CD - Deploy to Azure'"
echo "3. Click 'Run workflow'"
echo "4. Select 'production' environment"
echo "5. Click 'Run workflow'"
echo ""

echo "📊 Step 7: Monitor Deployment"
echo "- Deployment Status: https://github.com/NakaSato/dotnet-rest-api/actions"
echo "- App URL: https://solar-projects-api-dev.azurewebsites.net"
echo "- Health Check: https://solar-projects-api-dev.azurewebsites.net/health"
echo "- API Docs: https://solar-projects-api-dev.azurewebsites.net/swagger"
echo ""

echo "✅ Setup guide complete!"
echo ""
echo "Need help? Check the setup-github-secrets.md file for detailed instructions."
