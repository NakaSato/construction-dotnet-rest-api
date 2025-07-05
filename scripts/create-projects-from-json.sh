#!/bin/bash

# =================================================================================================
# Create Projects from JSON Data Script
# =================================================================================================
# This script reads projects.json and creates projects via the REST API
# Requires: jq, curl
# Usage: ./create-projects-from-json.sh [json_file] [api_base_url]
# =================================================================================================

set -e  # Exit on any error

# Configuration
JSON_FILE="${1:-projects.json}"
API_BASE_URL="${2:-http://localhost:5001}"
API_ENDPOINT="$API_BASE_URL/api/v1/projects"
LOGIN_ENDPOINT="$API_BASE_URL/api/v1/auth/login"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

log_success() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] ✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] ⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ❌ $1${NC}"
}

# Check dependencies
check_dependencies() {
    log "Checking dependencies..."
    
    if ! command -v jq &> /dev/null; then
        log_error "jq is required but not installed. Please install jq (brew install jq)"
        exit 1
    fi
    
    if ! command -v curl &> /dev/null; then
        log_error "curl is required but not installed."
        exit 1
    fi
    
    log_success "Dependencies check passed"
}

# Check if JSON file exists
check_json_file() {
    if [[ ! -f "$JSON_FILE" ]]; then
        log_error "JSON file '$JSON_FILE' not found!"
        echo "Usage: $0 [json_file] [api_base_url]"
        echo "Example: $0 projects.json http://localhost:5001"
        exit 1
    fi
    
    # Validate JSON syntax
    if ! jq empty "$JSON_FILE" 2>/dev/null; then
        log_error "Invalid JSON syntax in '$JSON_FILE'"
        exit 1
    fi
    
    log_success "JSON file '$JSON_FILE' found and valid"
}

# Login and get JWT token
get_jwt_token() {
    log "Logging in to get JWT token..."
    
    local response
    response=$(curl -s -X POST "$LOGIN_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "username": "admin@example.com",
            "password": "Admin123!"
        }')
    
    if [[ -z "$response" ]]; then
        log_error "No response from login endpoint"
        exit 1
    fi
    
    # Check if response contains success field
    local success
    success=$(echo "$response" | jq -r '.success // false')
    
    if [[ "$success" != "true" ]]; then
        log_error "Login failed"
        echo "Response: $response"
        exit 1
    fi
    
    # Extract token from response
    JWT_TOKEN=$(echo "$response" | jq -r '.data.token // empty')
    
    if [[ -z "$JWT_TOKEN" || "$JWT_TOKEN" == "null" ]]; then
        log_error "Failed to extract JWT token from login response"
        echo "Response: $response"
        exit 1
    fi
    
    log_success "Successfully obtained JWT token"
}

# Convert JSON project to API request format
convert_project_to_request() {
    local project_json="$1"
    
    # Extract and convert the project data to match CreateProjectRequest format
    local request_json
    request_json=$(echo "$project_json" | jq '{
        projectName: .projectName,
        address: (.address // "Address not specified"),
        clientInfo: (.clientInfo // ""),
        startDate: .startDate,
        estimatedEndDate: .estimatedEndDate,
        projectManagerId: null,
        team: (.team // null),
        connectionType: (.connectionType // null),
        connectionNotes: (.connectionNotes // null),
        totalCapacityKw: (.totalCapacityKw // null),
        pvModuleCount: (.pvModuleCount // null),
        equipmentDetails: (.equipmentDetails // null),
        ftsValue: (.ftsValue // null),
        revenueValue: (.revenueValue // null),
        pqmValue: (.pqmValue // null),
        locationCoordinates: (.locationCoordinates // null)
    }')
    
    echo "$request_json"
}

# Create a single project
create_project() {
    local project_json="$1"
    local project_id
    local project_name
    
    project_id=$(echo "$project_json" | jq -r '.projectId // "unknown"')
    project_name=$(echo "$project_json" | jq -r '.projectName // "unknown"')
    
    log "Creating project: $project_name (ID: $project_id)"
    
    # Convert to API request format
    local request_json
    request_json=$(convert_project_to_request "$project_json")
    
    # Make API call
    local response
    response=$(curl -s -X POST "$API_ENDPOINT" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $JWT_TOKEN" \
        -d "$request_json")
    
    if [[ -z "$response" ]]; then
        log_error "No response from API for project: $project_name"
        return 1
    fi
    
    # Check for different response formats
    local success
    local has_success_field
    has_success_field=$(echo "$response" | jq 'has("success")')
    
    if [[ "$has_success_field" == "true" ]]; then
        # Standard ApiResponse format
        success=$(echo "$response" | jq -r '.success // false')
        if [[ "$success" == "true" ]]; then
            local created_id
            created_id=$(echo "$response" | jq -r '.data.id // "unknown"')
            log_success "Created project: $project_name (API ID: $created_id)"
            return 0
        else
            local error_message
            error_message=$(echo "$response" | jq -r '.message // "Unknown error"')
            log_error "Failed to create project: $project_name - $error_message"
            echo "Response: $response" | jq . 2>/dev/null || echo "$response"
            return 1
        fi
    else
        # Check if we got a result/value format (which usually means success)
        local has_result
        has_result=$(echo "$response" | jq 'has("result")')
        if [[ "$has_result" == "true" ]]; then
            log_success "Created project: $project_name (API responded with result format)"
            return 0
        else
            log_error "Unexpected response format for project: $project_name"
            echo "Response: $response" | jq . 2>/dev/null || echo "$response"
            return 1
        fi
    fi
}

# Main function
main() {
    echo "=============================================="
    echo "🚀 Solar Projects Bulk Creation Script"
    echo "=============================================="
    echo "JSON File: $JSON_FILE"
    echo "API URL: $API_BASE_URL"
    echo "=============================================="
    
    # Pre-flight checks
    check_dependencies
    check_json_file
    
    # Authentication
    get_jwt_token
    
    # Get project count
    local total_projects
    total_projects=$(jq '. | length' "$JSON_FILE")
    log "Found $total_projects projects to create"
    
    # Counters
    local success_count=0
    local error_count=0
    local current=0
    
    # Process each project
    while IFS= read -r project; do
        ((current++))
        echo ""
        log "Processing project $current/$total_projects"
        
        if create_project "$project"; then
            ((success_count++))
        else
            ((error_count++))
        fi
        
        # Add small delay to avoid overwhelming the API
        sleep 0.5
        
    done < <(jq -c '.[]' "$JSON_FILE")
    
    # Summary
    echo ""
    echo "=============================================="
    echo "📊 BULK CREATION SUMMARY"
    echo "=============================================="
    log_success "Successfully created: $success_count projects"
    if [[ $error_count -gt 0 ]]; then
        log_error "Failed to create: $error_count projects"
    fi
    echo "Total processed: $total_projects projects"
    echo "=============================================="
    
    # Exit with appropriate code
    if [[ $error_count -eq 0 ]]; then
        log_success "All projects created successfully! 🎉"
        exit 0
    else
        log_warning "Some projects failed to create. Check the errors above."
        exit 1
    fi
}

# Show usage if help requested
if [[ "$1" == "-h" || "$1" == "--help" ]]; then
    echo "Solar Projects Bulk Creation Script"
    echo ""
    echo "Usage: $0 [json_file] [api_base_url]"
    echo ""
    echo "Arguments:"
    echo "  json_file     Path to JSON file containing projects (default: projects.json)"
    echo "  api_base_url  Base URL of the API (default: http://localhost:5001)"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Use defaults"
    echo "  $0 my-projects.json                   # Custom JSON file"
    echo "  $0 projects.json http://localhost:8080  # Custom API URL"
    echo ""
    echo "Requirements:"
    echo "  - jq (JSON processor)"
    echo "  - curl (HTTP client)"
    echo "  - Running API server with admin credentials"
    echo ""
    echo "JSON Format:"
    echo "  The JSON file should contain an array of project objects"
    echo "  matching the format from the API export."
    exit 0
fi

# Run main function
main "$@"
