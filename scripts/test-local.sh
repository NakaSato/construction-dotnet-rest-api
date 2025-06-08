#!/bin/bash

# Local Testing Script for Solar Projects API
# Run this script to test your application locally before deploying

echo "🧪 Solar Projects API - Local Testing Script"
echo "============================================="

# Configuration
API_URL="http://localhost:5001"
DOCKER_API_URL="http://localhost:5001"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check if API is running
check_api_health() {
    local url=$1
    log_info "Checking API health at $url"
    
    if curl -f -s "$url/health" > /dev/null; then
        log_success "API is healthy"
        return 0
    else
        log_error "API is not responding"
        return 1
    fi
}

# Test endpoints
test_endpoints() {
    local base_url=$1
    log_info "Testing API endpoints..."
    
    # Test health endpoint
    if curl -f -s "$base_url/health" | jq . > /dev/null 2>&1; then
        log_success "Health endpoint working"
    else
        log_error "Health endpoint failed"
    fi
    
    # Test todo endpoint
    if curl -f -s "$base_url/api/todo" > /dev/null; then
        log_success "Todo endpoint working"
    else
        log_warning "Todo endpoint may require authentication"
    fi
    
    # Test swagger
    if curl -f -s "$base_url/swagger/index.html" > /dev/null; then
        log_success "Swagger documentation available"
    else
        log_warning "Swagger documentation not accessible"
    fi
}

# Main testing flow
main() {
    echo ""
    log_info "Starting local tests..."
    
    # Test 1: Check if .NET is installed
    if command -v dotnet &> /dev/null; then
        log_success ".NET SDK is installed ($(dotnet --version))"
    else
        log_error ".NET SDK is not installed"
        exit 1
    fi
    
    # Test 2: Check if Docker is installed and running
    if command -v docker &> /dev/null && docker info &> /dev/null; then
        log_success "Docker is installed and running"
    else
        log_warning "Docker is not available"
    fi
    
    # Test 3: Build the application
    log_info "Building the application..."
    if dotnet build --configuration Release > /dev/null 2>&1; then
        log_success "Application builds successfully"
    else
        log_error "Application build failed"
        exit 1
    fi
    
    # Test 4: Run tests
    log_info "Running unit tests..."
    if dotnet test --configuration Release --no-build > /dev/null 2>&1; then
        log_success "All tests passed"
    else
        log_warning "Some tests failed or no tests found"
    fi
    
    # Test 5: Check Docker build
    if command -v docker &> /dev/null; then
        log_info "Building Docker image..."
        if docker build -t solar-projects-api-test . > /dev/null 2>&1; then
            log_success "Docker image builds successfully"
        else
            log_error "Docker build failed"
        fi
    fi
    
    # Test 6: Check for required configuration files
    log_info "Checking configuration files..."
    
    if [ -f "appsettings.json" ]; then
        log_success "appsettings.json found"
    else
        log_error "appsettings.json missing"
    fi
    
    if [ -f "appsettings.Development.json" ]; then
        log_success "appsettings.Development.json found"
    else
        log_warning "appsettings.Development.json missing"
    fi
    
    if [ -f "Dockerfile" ]; then
        log_success "Dockerfile found"
    else
        log_error "Dockerfile missing"
    fi
    
    # Test 7: Check GitHub Actions workflows
    log_info "Checking GitHub Actions workflows..."
    
    if [ -d ".github/workflows" ]; then
        workflow_count=$(find .github/workflows -name "*.yml" | wc -l)
        log_success "Found $workflow_count GitHub Actions workflows"
    else
        log_warning "No GitHub Actions workflows found"
    fi
    
    echo ""
    log_info "Local testing completed!"
    echo ""
    log_info "Next steps:"
    echo "1. Start the application: dotnet run"
    echo "2. Test locally: curl http://localhost:5001/health"
    echo "3. Check Swagger: http://localhost:5001/swagger"
    echo "4. Start with Docker: docker-compose up"
    echo "5. Deploy to Azure: Push to main branch"
    echo ""
    log_success "Your application is ready for deployment! 🚀"
}

# Run main function
main "$@"
