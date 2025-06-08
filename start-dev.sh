#!/bin/bash

# Local development startup script
# This script ensures the application runs with proper Development environment settings

echo "🚀 Starting Solar Projects API in Development mode..."
echo "=================================================="

# Set environment to Development
export ASPNETCORE_ENVIRONMENT=Development

# Check if PostgreSQL is running (optional)
if command -v pg_isready >/dev/null 2>&1; then
    if pg_isready -h localhost -p 5432 >/dev/null 2>&1; then
        echo "✅ PostgreSQL is running on localhost:5432"
    else
        echo "⚠️  PostgreSQL might not be running. Starting Docker Compose..."
        docker-compose -f docker-compose.dev.yml up -d postgres
    fi
fi

# Show environment info
echo "🔧 Environment: $ASPNETCORE_ENVIRONMENT"
echo "🌐 URL: http://localhost:5001"
echo "📚 Swagger: http://localhost:5001/swagger"
echo "🏥 Health: http://localhost:5001/health"
echo ""

# Start the application
echo "🎯 Starting application..."
dotnet run --urls "http://localhost:5001"
