# GitHub Actions Secrets Setup Script
# Run this script to set up all required GitHub secrets for Azure deployment

# Azure Service Principal (run this first)
echo "Creating Azure Service Principal..."
az ad sp create-for-rbac \
  --name "github-actions-solar-projects" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID \
  --sdk-auth

echo ""
echo "📋 GitHub Secrets to Configure:"
echo "================================"

echo ""
echo "🔐 Azure Configuration:"
echo "AZURE_CREDENTIALS           - Output from the service principal command above"
echo "AZURE_RESOURCE_GROUP         - rg-solar-projects-production"
echo "ACR_USERNAME                 - From Azure Container Registry"
echo "ACR_PASSWORD                 - From Azure Container Registry"

echo ""
echo "🗄️  Database Configuration:"
echo "AZURE_DB_CONNECTION_STRING   - PostgreSQL connection string"
echo "DB_ADMIN_PASSWORD            - Strong password for database admin"

echo ""
echo "🔑 JWT Configuration:"
echo "JWT_SECRET_KEY               - $(openssl rand -base64 64)"
echo "JWT_ISSUER                   - SolarProjectsAPI"
echo "JWT_AUDIENCE                 - SolarProjectsClients"

echo ""
echo "☁️  Cloudinary Configuration:"
echo "CLOUDINARY_CLOUD_NAME        - Your Cloudinary cloud name"
echo "CLOUDINARY_API_KEY           - Your Cloudinary API key"
echo "CLOUDINARY_API_SECRET        - Your Cloudinary API secret"

echo ""
echo "🎯 Example Commands to Set Secrets:"
echo "gh secret set AZURE_CREDENTIALS --body 'SERVICE_PRINCIPAL_JSON'"
echo "gh secret set JWT_SECRET_KEY --body '$(openssl rand -base64 64)'"
echo "gh secret set AZURE_RESOURCE_GROUP --body 'rg-solar-projects-production'"

echo ""
echo "⚠️  Remember to:"
echo "1. Replace YOUR_SUBSCRIPTION_ID with your actual Azure subscription ID"
echo "2. Create the secrets in your GitHub repository settings"
echo "3. Test the deployment in staging environment first"
