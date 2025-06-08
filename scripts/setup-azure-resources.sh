# Azure CLI Setup Script for Local Development
# This script sets up Azure resources for the Solar Projects API

#!/bin/bash

# Configuration
RESOURCE_GROUP="rg-solar-projects-dev"
LOCATION="eastus"
ACR_NAME="solarprojacr$(date +%s)"
APP_NAME="solar-projects-api-dev"
DB_NAME="solar-projects-db-dev"
DB_ADMIN_USER="solaradmin"

echo "🚀 Setting up Azure resources for Solar Projects API"
echo "=================================================="

# Login to Azure (if not already logged in)
echo "📝 Checking Azure login status..."
if ! az account show &> /dev/null; then
    echo "Please login to Azure:"
    az login
fi

# Create Resource Group
echo "📁 Creating resource group: $RESOURCE_GROUP"
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION

# Create Container Registry
echo "📦 Creating Azure Container Registry: $ACR_NAME"
az acr create \
    --resource-group $RESOURCE_GROUP \
    --name $ACR_NAME \
    --sku Basic \
    --admin-enabled true

# Get ACR credentials
echo "🔑 Getting ACR credentials..."
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username --output tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value --output tsv)

# Create PostgreSQL Database
echo "🗄️  Creating PostgreSQL database: $DB_NAME"
read -s -p "Enter database admin password: " DB_PASSWORD
echo ""

az postgres flexible-server create \
    --resource-group $RESOURCE_GROUP \
    --name $DB_NAME \
    --location $LOCATION \
    --admin-user $DB_ADMIN_USER \
    --admin-password $DB_PASSWORD \
    --sku-name Standard_B1ms \
    --tier Burstable \
    --storage-size 32 \
    --version 16 \
    --public-access 0.0.0.0

# Create database
echo "📋 Creating application database..."
az postgres flexible-server db create \
    --resource-group $RESOURCE_GROUP \
    --server-name $DB_NAME \
    --database-name SolarProjectsDb

# Create App Service Plan
echo "🏗️  Creating App Service Plan..."
az appservice plan create \
    --resource-group $RESOURCE_GROUP \
    --name "${APP_NAME}-plan" \
    --location $LOCATION \
    --sku B1 \
    --is-linux

# Create App Service
echo "🌐 Creating App Service: $APP_NAME"
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan "${APP_NAME}-plan" \
    --name $APP_NAME \
    --deployment-container-image-name mcr.microsoft.com/dotnet/aspnet:9.0

# Configure App Service
echo "⚙️  Configuring App Service..."
az webapp config set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_NAME \
    --health-check-path "/health"

# Create Application Insights
echo "📊 Creating Application Insights..."
az monitor app-insights component create \
    --resource-group $RESOURCE_GROUP \
    --app "${APP_NAME}-insights" \
    --location $LOCATION \
    --kind web \
    --application-type web

# Output summary
echo ""
echo "✅ Azure resources created successfully!"
echo "======================================="
echo "Resource Group: $RESOURCE_GROUP"
echo "Container Registry: $ACR_NAME.azurecr.io"
echo "  Username: $ACR_USERNAME"
echo "  Password: $ACR_PASSWORD"
echo "Database Server: $DB_NAME.postgres.database.azure.com"
echo "  Database: SolarProjectsDb"
echo "  Admin User: $DB_ADMIN_USER"
echo "App Service: https://$APP_NAME.azurewebsites.net"
echo ""
echo "🔗 Useful links:"
echo "Azure Portal: https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP"
echo ""
echo "📝 Next steps:"
echo "1. Update your GitHub secrets with the ACR credentials"
echo "2. Update your connection string with the database details"
echo "3. Build and push your Docker image to ACR"
echo "4. Deploy your application using GitHub Actions"
