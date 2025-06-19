#!/bin/bash

# Azure Deployment Script for Solar Projects API
echo "🚀 Azure Deployment Script"
echo "=========================="

# Configuration
RESOURCE_GROUP="solar-projects-rg"
LOCATION="East US"
APP_NAME="solar-projects-api"
APP_SERVICE_PLAN="solar-projects-plan"
SKU="B1"  # Basic tier
ENVIRONMENT="production"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check if Azure CLI is installed
check_azure_cli() {
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed"
        echo "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    print_status "Azure CLI is installed"
}

# Login to Azure
azure_login() {
    echo "🔐 Checking Azure login status..."
    if ! az account show &> /dev/null; then
        print_warning "Not logged in to Azure"
        echo "Please login to Azure:"
        az login
    else
        print_status "Already logged in to Azure"
        echo "Current subscription: $(az account show --query name -o tsv)"
    fi
}

# Create or verify resource group
create_resource_group() {
    echo "📦 Creating/verifying resource group..."
    if az group show --name "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Resource group '$RESOURCE_GROUP' already exists"
    else
        print_warning "Creating resource group '$RESOURCE_GROUP'"
        az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
        print_status "Resource group created"
    fi
}

# Create App Service Plan
create_app_service_plan() {
    echo "🏗️ Creating/verifying App Service Plan..."
    if az appservice plan show --name "$APP_SERVICE_PLAN" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "App Service Plan '$APP_SERVICE_PLAN' already exists"
    else
        print_warning "Creating App Service Plan '$APP_SERVICE_PLAN'"
        az appservice plan create \
            --name "$APP_SERVICE_PLAN" \
            --resource-group "$RESOURCE_GROUP" \
            --sku "$SKU" \
            --is-linux
        print_status "App Service Plan created"
    fi
}

# Create Web App
create_web_app() {
    echo "🌐 Creating/verifying Web App..."
    if az webapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        print_status "Web App '$APP_NAME' already exists"
    else
        print_warning "Creating Web App '$APP_NAME'"
        az webapp create \
            --name "$APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --plan "$APP_SERVICE_PLAN" \
            --runtime "DOTNETCORE:9.0"
        print_status "Web App created"
    fi
}

# Configure Web App settings
configure_web_app() {
    echo "⚙️ Configuring Web App settings..."
    
    # Set application settings
    az webapp config appsettings set \
        --name "$APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --settings \
            ASPNETCORE_ENVIRONMENT="Production" \
            WEBSITES_ENABLE_APP_SERVICE_STORAGE="false" \
            ASPNETCORE_URLS="http://+:8080"
    
    print_status "Web App settings configured"
}

# Build and publish the application
build_application() {
    echo "🔨 Building application..."
    
    # Clean previous builds
    dotnet clean
    
    # Restore dependencies
    dotnet restore
    
    # Build in Release mode
    dotnet build --configuration Release --no-restore
    
    # Publish the application
    dotnet publish --configuration Release --output ./publish --no-build
    
    print_status "Application built and published"
}

# Deploy to Azure
deploy_application() {
    echo "🚀 Deploying application to Azure..."
    
    # Create deployment package
    cd publish
    zip -r ../deployment.zip .
    cd ..
    
    # Deploy using Azure CLI
    az webapp deployment source config-zip \
        --name "$APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --src deployment.zip
    
    print_status "Application deployed to Azure"
    
    # Clean up deployment files
    rm -f deployment.zip
    rm -rf publish
}

# Verify deployment
verify_deployment() {
    echo "🔍 Verifying deployment..."
    
    APP_URL="https://${APP_NAME}.azurewebsites.net"
    echo "Application URL: $APP_URL"
    
    # Wait for deployment to complete
    print_warning "Waiting 30 seconds for deployment to complete..."
    sleep 30
    
    # Test health endpoint
    for i in {1..5}; do
        if curl -f "${APP_URL}/health" &> /dev/null; then
            print_status "Health check passed! Application is running."
            echo "🌐 Application URL: $APP_URL"
            echo "🔍 Health endpoint: $APP_URL/health"
            echo "📖 API documentation: $APP_URL/swagger"
            return 0
        else
            print_warning "Health check attempt $i/5 failed, retrying in 10 seconds..."
            sleep 10
        fi
    done
    
    print_error "Health check failed. Please check the application logs."
    echo "View logs with: az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
}

# Main deployment process
main() {
    echo "Starting deployment process..."
    echo "Resource Group: $RESOURCE_GROUP"
    echo "App Name: $APP_NAME"
    echo "Location: $LOCATION"
    echo ""
    
    check_azure_cli
    azure_login
    create_resource_group
    create_app_service_plan
    create_web_app
    configure_web_app
    build_application
    deploy_application
    verify_deployment
    
    echo ""
    echo "🎉 Deployment completed!"
    echo ""
    echo "📋 Next steps:"
    echo "1. Configure database connection string if needed"
    echo "2. Set up custom domain (optional)"
    echo "3. Configure SSL certificate"
    echo "4. Set up monitoring and alerts"
    echo ""
    echo "🔧 Useful commands:"
    echo "  # View application logs"
    echo "  az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo ""
    echo "  # Restart the application"
    echo "  az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo ""
    echo "  # Update application settings"
    echo "  az webapp config appsettings set --name $APP_NAME --resource-group $RESOURCE_GROUP --settings KEY=VALUE"
}

# Run the deployment
main "$@"
