#!/bin/bash

# Setup GitHub Secrets for Solar Projects API
# This script shows you what secrets need to be set in your GitHub repository
# IMPORTANT: The actual secret values are stored securely and not included in this repository

echo "🔐 GitHub Secrets Setup for Solar Projects API"
echo "=============================================="
echo ""
echo "Please set the following secrets in your GitHub repository:"
echo "Go to: Repository Settings > Secrets and variables > Actions"
echo ""

echo "📋 Required GitHub Secrets:"
echo "=========================="
echo ""

echo "1. AZURE_CREDENTIALS"
echo "   Description: Service Principal JSON for Azure authentication"
echo "   Note: Get actual value from local setup script or Azure administrator"
echo ""

echo "2. AZURE_SUBSCRIPTION_ID"
echo "   Value: ea02bfff-4d3f-4d0c-ac62-613e82d307b7"
echo ""

echo "3. AZURE_RESOURCE_GROUP"
echo "   Value: rg-solar-projects-dev"
echo ""

echo "4. AZURE_APP_NAME"
echo "   Value: solar-projects-api-dev"
echo ""

echo "5. ACR_LOGIN_SERVER"
echo "   Value: solarprojacr1749359812.azurecr.io"
echo ""

echo "6. ACR_USERNAME"
echo "   Value: solarprojacr1749359812"
echo ""

echo "7. ACR_PASSWORD"
echo "   Description: Azure Container Registry password"
echo "   Note: Get from Azure Portal > Container Registry > Access keys"
echo ""

echo "8. DATABASE_CONNECTION_STRING"
echo "   Description: PostgreSQL connection string"
echo "   Format: Server=solar-db-1749364020.postgres.database.azure.com;Database=SolarProjectsDb;User Id=solaradmin;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;"
echo ""

echo "9. APPLICATIONINSIGHTS_CONNECTION_STRING"
echo "   Value: InstrumentationKey=6694e2bb-efd8-4b50-9d2b-ad9a80177827;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=bf1318a1-dacb-45ff-9ffc-18d70800e36a"
echo ""

echo "🔗 Quick Setup URLs:"
echo "   GitHub Secrets: https://github.com/YOUR-USERNAME/YOUR-REPO/settings/secrets/actions"
echo "   Azure Portal: https://portal.azure.com/#@/resource/subscriptions/ea02bfff-4d3f-4d0c-ac62-613e82d307b7/resourceGroups/rg-solar-projects-dev"
echo ""

echo "⚠️  SECURITY NOTE:"
echo "   The actual secret values are stored securely and not included in this repository."
echo "   Contact the Azure administrator or check local setup files for actual values."
echo ""

echo "✅ After setting all secrets, push to main branch to trigger deployment!"
