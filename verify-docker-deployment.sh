#!/bin/bash

echo "🚀 Verifying Docker Deployment of Solar Projects API"
echo "=================================================="

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check container status
check_container_status() {
    local container_name=$1
    if docker-compose ps | grep -q "$container_name.*Up"; then
        echo -e "${GREEN}✅ $container_name is running${NC}"
        return 0
    else
        echo -e "${RED}❌ $container_name is not running${NC}"
        return 1
    fi
}

# Function to check HTTP endpoint
check_endpoint() {
    local url=$1
    local expected_code=$2
    local description=$3
    
    echo -n "Testing $description... "
    local response_code=$(curl -s -o /dev/null -w "%{http_code}" "$url")
    
    if [ "$response_code" = "$expected_code" ]; then
        echo -e "${GREEN}✅ HTTP $response_code${NC}"
        return 0
    else
        echo -e "${RED}❌ HTTP $response_code (expected $expected_code)${NC}"
        return 1
    fi
}

echo ""
echo "1. Checking Container Status"
echo "----------------------------"
check_container_status "solar-projects-db"
check_container_status "solar-projects-api"

echo ""
echo "2. Checking API Endpoints"
echo "-------------------------"
check_endpoint "http://localhost:5002/health" "200" "Health Check"
check_endpoint "http://localhost:5002/api/v1/projects" "401" "Projects API (Auth Required)"

echo ""
echo "3. Database Connection Test"
echo "---------------------------"
if docker-compose exec -T postgres pg_isready -U postgres -d SolarProjectsDb > /dev/null 2>&1; then
    echo -e "${GREEN}✅ PostgreSQL database is ready${NC}"
else
    echo -e "${RED}❌ PostgreSQL database is not ready${NC}"
fi

echo ""
echo "4. Checking Docker Images"
echo "-------------------------"
if docker images | grep -q "solar-projects-api"; then
    echo -e "${GREEN}✅ solar-projects-api image exists${NC}"
    docker images | grep "solar-projects-api" | head -1
else
    echo -e "${RED}❌ solar-projects-api image not found${NC}"
fi

echo ""
echo "5. Port Availability"
echo "--------------------"
if lsof -i :5002 > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Port 5002 is in use (API)${NC}"
else
    echo -e "${RED}❌ Port 5002 is not in use${NC}"
fi

if lsof -i :5432 > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Port 5432 is in use (PostgreSQL)${NC}"
else
    echo -e "${RED}❌ Port 5432 is not in use${NC}"
fi

echo ""
echo "6. Recent Container Logs"
echo "------------------------"
echo -e "${YELLOW}Latest API logs:${NC}"
docker-compose logs api --tail=3

echo ""
echo "📊 Deployment Summary"
echo "==================="
echo "🌐 API URL: http://localhost:5002"
echo "📚 Swagger UI: http://localhost:5002/swagger (if enabled in Production)"
echo "🗄️  Database: localhost:5432 (postgres/postgres)"
echo "📋 Health Check: http://localhost:5002/health"
echo ""
echo "🔐 Authentication Required:"
echo "   - Most endpoints require JWT authentication"
echo "   - Use /api/auth/login to get a token"
echo "   - RBAC roles: Administrator, ProjectManager, Planner, Technician"
echo ""
echo "🛠️  Docker Commands:"
echo "   - Stop: docker-compose down"
echo "   - Restart: docker-compose restart"
echo "   - Logs: docker-compose logs -f"
echo "   - Rebuild: docker-compose up --build -d"
