#!/bin/bash

# Solar Projects API - Docker Deployment Script
# This script builds and runs the .NET REST API in Docker with in-memory database

set -e

echo "🔨 Building Docker image..."
docker build -t dotnet-rest-api:latest .

echo "🧹 Cleaning up any existing container..."
docker stop dotnet-api 2>/dev/null || true
docker rm dotnet-api 2>/dev/null || true

echo "🚀 Starting API container with in-memory database..."
docker run -d \
  --name dotnet-api \
  -p 8080:8080 \
  -e USE_IN_MEMORY_DB=true \
  dotnet-rest-api:latest

echo "⏳ Waiting for container to start..."
sleep 3

echo "📋 Container logs:"
docker logs dotnet-api

echo ""
echo "✅ Deployment complete!"
echo "🌐 API is running at: http://localhost:8080"
echo "🏥 Health check: http://localhost:8080/health"
echo "🐛 Debug info: http://localhost:8080/api/debug/database"
echo ""
echo "To stop the API: docker stop dotnet-api && docker rm dotnet-api"
