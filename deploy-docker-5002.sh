#!/bin/bash

# Docker Deployment Script for Solar Projects API on Port 5002
echo "🚀 Building and deploying Solar Projects API to Docker on port 5002..."

# Stop and remove existing containers
echo "📋 Cleaning up existing containers..."
docker-compose down
docker container prune -f

# Build and start the application
echo "🔨 Building Docker image..."
docker-compose build --no-cache

echo "🚀 Starting application on port 5002..."
docker-compose up -d

# Wait for services to start
echo "⏳ Waiting for services to start..."
sleep 15

# Check container status
echo "📊 Container Status:"
docker-compose ps

# Test the application
echo "🧪 Testing application endpoints..."

# Health check
echo "Testing health endpoint..."
if curl -f -s "http://localhost:5002/health" > /dev/null; then
    echo "✅ Health check passed"
    curl -s "http://localhost:5002/health" | jq '.' || curl -s "http://localhost:5002/health"
else
    echo "❌ Health check failed"
fi

echo ""

# Swagger UI
echo "Testing Swagger UI..."
if curl -f -s "http://localhost:5002/swagger" > /dev/null; then
    echo "✅ Swagger UI is accessible"
else
    echo "❌ Swagger UI is not accessible"
fi

echo ""

# API endpoints
echo "Testing API endpoints..."
if curl -f -s "http://localhost:5002/api/v1/projects" > /dev/null; then
    echo "✅ Projects API is working"
else
    echo "❌ Projects API is not working"
fi

echo ""
echo "🎉 Deployment complete!"
echo ""
echo "📋 Application URLs:"
echo "🌐 API Base URL: http://localhost:5002"
echo "🔍 Health Check: http://localhost:5002/health"
echo "📚 Swagger UI: http://localhost:5002/swagger"
echo "📋 API Docs: http://localhost:5002/swagger/v1/swagger.json"
echo ""
echo "📊 Database URLs:"
echo "🗄️  Database: localhost:5432 (postgres/postgres)"
echo "🔧 PgAdmin: http://localhost:8080 (admin@solarprojects.com/admin)"
echo ""
echo "🔧 Useful commands:"
echo "📋 View logs: docker-compose logs -f api"
echo "🛑 Stop: docker-compose down"
echo "🔄 Restart: docker-compose restart api"
echo ""

# Show container logs
echo "📋 Recent API logs:"
docker-compose logs --tail=20 api
