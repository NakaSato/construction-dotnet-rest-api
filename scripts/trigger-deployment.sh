#!/bin/bash

# Deployment Trigger Script
# Run this after configuring GitHub secrets to trigger the first deployment

echo "🚀 Triggering Azure Deployment"
echo "=============================="
echo ""

# Check if we're in the right directory
if [ ! -f "dotnet-rest-api.csproj" ]; then
    echo "❌ Error: Not in the project root directory"
    echo "Please run this script from the dotnet-rest-api project root"
    exit 1
fi

# Check git status
echo "📋 Checking git status..."
git_status=$(git status --porcelain)
if [ -n "$git_status" ]; then
    echo "⚠️  Warning: You have uncommitted changes:"
    git status --short
    echo ""
    read -p "Do you want to continue? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Deployment cancelled"
        exit 1
    fi
fi

# Check if we're on main branch
current_branch=$(git rev-parse --abbrev-ref HEAD)
if [ "$current_branch" != "main" ]; then
    echo "⚠️  Warning: You're not on the main branch (current: $current_branch)"
    read -p "Do you want to switch to main branch? (y/N): " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        git checkout main
        git pull origin main
    else
        echo "❌ Deployment cancelled"
        exit 1
    fi
fi

# Create a deployment trigger commit
echo "📝 Creating deployment trigger..."
deployment_time=$(date "+%Y-%m-%d %H:%M:%S")
echo "" >> README.md
echo "<!-- Deployment triggered: $deployment_time -->" >> README.md

# Commit and push
git add README.md
git commit -m "🚀 Trigger Azure deployment - $deployment_time

- Initial production deployment to Azure App Service
- Configured with PostgreSQL database and Application Insights
- CI/CD pipeline will build, test, and deploy automatically"

echo "🔄 Pushing to GitHub..."
git push origin main

echo ""
echo "✅ Deployment triggered successfully!"
echo ""
echo "📊 Monitor the deployment:"
echo "   GitHub Actions: https://github.com/NakaSato/dotnet-rest-api/actions"
echo "   CI Pipeline: https://github.com/NakaSato/dotnet-rest-api/actions/workflows/ci.yml"
echo "   Deploy Pipeline: https://github.com/NakaSato/dotnet-rest-api/actions/workflows/deploy.yml"
echo ""
echo "🌐 Expected deployment URL:"
echo "   Application: https://solar-projects-api-dev.azurewebsites.net"
echo "   Health Check: https://solar-projects-api-dev.azurewebsites.net/health"
echo "   Swagger UI: https://solar-projects-api-dev.azurewebsites.net/swagger"
echo ""
echo "⏱️  Deployment typically takes 5-10 minutes to complete."
echo "📧 You'll receive notifications if the deployment fails."
echo ""
echo "🎉 Happy deploying!"
