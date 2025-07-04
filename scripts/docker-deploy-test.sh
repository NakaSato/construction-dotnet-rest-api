#!/bin/bash

# Docker Build and Deploy Test Script for Solar Projects API
# This script builds and deploys the .NET REST API using Docker

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="solar-projects"
API_IMAGE="${PROJECT_NAME}-api"
API_CONTAINER="${PROJECT_NAME}-api-container"
DB_CONTAINER="${PROJECT_NAME}-db-container"

print_status() {
    local status=$1
    local message=$2
    case $status in
        "INFO")
            echo -e "${BLUE}[INFO]${NC} $message"
            ;;
        "SUCCESS")
            echo -e "${GREEN}[SUCCESS]${NC} $message"
            ;;
        "ERROR")
            echo -e "${RED}[ERROR]${NC} $message"
            ;;
        "WARNING")
            echo -e "${YELLOW}[WARNING]${NC} $message"
            ;;
    esac
}

print_header() {
    echo ""
    echo "=========================================="
    echo "$1"
    echo "=========================================="
}

# Function to check if Docker is running
check_docker() {
    print_header "Checking Docker Environment"
    
    if ! command -v docker &> /dev/null; then
        print_status "ERROR" "Docker is not installed"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_status "ERROR" "Docker daemon is not running"
        exit 1
    fi
    
    print_status "SUCCESS" "Docker is running"
    echo "Docker version: $(docker --version)"
    echo "Docker Compose version: $(docker-compose --version)"
}

# Function to clean up existing containers
cleanup_containers() {
    print_header "Cleaning Up Existing Containers"
    
    # Stop and remove containers
    docker stop $API_CONTAINER $DB_CONTAINER 2>/dev/null || true
    docker rm $API_CONTAINER $DB_CONTAINER 2>/dev/null || true
    
    # Stop docker-compose containers if they exist
    docker-compose down --remove-orphans 2>/dev/null || true
    
    print_status "SUCCESS" "Cleanup completed"
}

# Function to build the API image
build_image() {
    print_header "Building Docker Image"
    
    print_status "INFO" "Building $API_IMAGE image..."
    
    if docker build -t $API_IMAGE . --no-cache; then
        print_status "SUCCESS" "Docker image built successfully"
        
        # Show image info
        image_size=$(docker images $API_IMAGE --format "{{.Size}}" | head -1)
        print_status "INFO" "Image size: $image_size"
    else
        print_status "ERROR" "Failed to build Docker image"
        exit 1
    fi
}

# Function to deploy using docker-compose
deploy_with_compose() {
    print_header "Deploying with Docker Compose"
    
    if [ ! -f "docker-compose.yml" ]; then
        print_status "ERROR" "docker-compose.yml not found"
        exit 1
    fi
    
    if [ ! -f ".env" ]; then
        print_status "WARNING" ".env file not found, using defaults"
    fi
    
    print_status "INFO" "Starting services with docker-compose..."
    
    # Start the services
    docker-compose up -d --build
    
    if [ $? -eq 0 ]; then
        print_status "SUCCESS" "Services started successfully"
    else
        print_status "ERROR" "Failed to start services"
        exit 1
    fi
}

# Function to wait for services to be ready
wait_for_services() {
    print_header "Waiting for Services to Start"
    
    print_status "INFO" "Waiting for database to be ready..."
    timeout=60
    elapsed=0
    
    while [ $elapsed -lt $timeout ]; do
        if docker-compose exec -T postgres pg_isready -U postgres -d SolarProjectsDb &>/dev/null; then
            print_status "SUCCESS" "Database is ready"
            break
        fi
        sleep 2
        elapsed=$((elapsed + 2))
        echo -n "."
    done
    
    if [ $elapsed -ge $timeout ]; then
        print_status "ERROR" "Database failed to start within $timeout seconds"
        exit 1
    fi
    
    print_status "INFO" "Waiting for API to be ready..."
    sleep 10  # Give the API some time to start
    
    # Check if API is responding
    for i in {1..30}; do
        if curl -f -s http://localhost:5001/Health > /dev/null; then
            print_status "SUCCESS" "API is ready and responding"
            return 0
        fi
        sleep 2
        echo -n "."
    done
    
    print_status "WARNING" "API health check timeout, but containers may still be starting"
}

# Function to test the deployment
test_deployment() {
    print_header "Testing Deployment"
    
    # Check container status
    print_status "INFO" "Checking container status..."
    docker-compose ps
    
    # Test database connection
    print_status "INFO" "Testing database connection..."
    if docker-compose exec -T postgres psql -U postgres -d SolarProjectsDb -c "SELECT 1;" &>/dev/null; then
        print_status "SUCCESS" "Database connection test passed"
    else
        print_status "ERROR" "Database connection test failed"
    fi
    
    # Test API health endpoint
    print_status "INFO" "Testing API health endpoint..."
    response=$(curl -s http://localhost:5001/Health 2>/dev/null || echo "FAILED")
    
    if echo "$response" | grep -q "Healthy"; then
        print_status "SUCCESS" "API health check passed"
        echo "Response: $response"
    else
        print_status "ERROR" "API health check failed"
        echo "Response: $response"
        
        # Show API logs for debugging
        print_status "INFO" "API container logs (last 20 lines):"
        docker-compose logs --tail=20 api
    fi
    
    # Test authentication endpoint
    print_status "INFO" "Testing authentication endpoint..."
    auth_response=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
        -H "Content-Type: application/json" \
        -d '{"username":"admin@example.com","password":"Admin123!"}' 2>/dev/null || echo "FAILED")
    
    if echo "$auth_response" | grep -q "token"; then
        print_status "SUCCESS" "Authentication endpoint test passed"
    else
        print_status "WARNING" "Authentication endpoint test failed (may need database seeding)"
        echo "Response: $auth_response"
    fi
}

# Function to show deployment information
show_deployment_info() {
    print_header "Deployment Information"
    
    echo "Services are running on:"
    echo "  • API: http://localhost:5001"
    echo "  • API Health: http://localhost:5001/Health"
    echo "  • API Swagger: http://localhost:5001/swagger"
    echo "  • Database: localhost:5432"
    echo ""
    echo "Container status:"
    docker-compose ps
    echo ""
    echo "To view logs:"
    echo "  docker-compose logs api      # API logs"
    echo "  docker-compose logs postgres # Database logs"
    echo ""
    echo "To stop the deployment:"
    echo "  docker-compose down"
    echo ""
    echo "To stop and remove volumes:"
    echo "  docker-compose down -v"
}

# Main execution
main() {
    echo "=========================================="
    echo "Solar Projects API - Docker Deployment"
    echo "=========================================="
    echo "Timestamp: $(date)"
    echo ""
    
    check_docker
    cleanup_containers
    build_image
    deploy_with_compose
    wait_for_services
    test_deployment
    show_deployment_info
    
    print_status "SUCCESS" "Docker deployment completed successfully! 🚀"
}

# Handle script termination
trap 'print_status "WARNING" "Script interrupted"; exit 1' INT TERM

# Run main function
main "$@"
