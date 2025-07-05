#!/bin/bash

# =============================================================================
# 🚀 SIMPLE CSV PROJECT IMPORTER
# =============================================================================
# A simplified version for importing Thai projects from CSV
# =============================================================================

BASE_URL="http://localhost:5001"
CSV_FILE="project.csv"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Counters
SUCCESS_COUNT=0
FAIL_COUNT=0

print_step() { echo -e "${BLUE}➤ $1${NC}"; }
print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_error() { echo -e "${RED}✗ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠ $1${NC}"; }

# Get admin token
get_admin_token() {
    print_step "Getting admin authentication token..."
    
    local response=$(curl -s -H "Content-Type: application/json" \
        -d '{"username": "test_admin", "password": "Admin123!"}' \
        "$BASE_URL/api/v1/auth/login")
    
    local token=$(echo "$response" | jq -r '.data.token' 2>/dev/null)
    
    if [ -z "$token" ] || [ "$token" = "null" ]; then
        print_error "Failed to get admin token"
        exit 1
    fi
    
    print_success "Admin token obtained"
    echo "$token"
}

# Convert M/D/Y to ISO date
convert_date() {
    local date_str="$1"
    
    if [[ "$date_str" =~ ^([0-9]+)/([0-9]+)/([0-9]{4})$ ]]; then
        local month="${BASH_REMATCH[1]}"
        local day="${BASH_REMATCH[2]}"
        local year="${BASH_REMATCH[3]}"
        
        # Remove leading zeros to avoid octal interpretation
        month=$((10#$month))
        day=$((10#$day))
        
        # Format with leading zeros
        month=$(printf "%02d" "$month")
        day=$(printf "%02d" "$day")
        
        echo "${year}-${month}-${day}T00:00:00Z"
    else
        echo "2025-07-04T00:00:00Z"  # Default date
    fi
}

# Create a single project
create_project() {
    local token="$1"
    local csv_line="$2"
    local line_number="$3"
    
    # Parse CSV line using a more robust method
    local IFS=','
    read -ra FIELDS <<< "$csv_line"
    
    # Extract key fields
    local project_id="${FIELDS[0]}"
    local location="${FIELDS[1]}"
    local branch="${FIELDS[2]}"
    local capacity="${FIELDS[3]:-100.0}"
    local panel_count="${FIELDS[4]:-100}"
    local connection_type="${FIELDS[6]:-MV}"
    local inv125="${FIELDS[7]:-0}"
    local inv80="${FIELDS[8]:-0}"
    local inv60="${FIELDS[9]:-0}"
    local inv40="${FIELDS[10]:-0}"
    local start_date="${FIELDS[18]}"
    local due_date="${FIELDS[19]}"
    
    # Skip if missing essential data
    if [ -z "$project_id" ] || [ -z "$location" ]; then
        print_warning "Line $line_number: Missing essential data, skipping"
        return 1
    fi
    
    # Clean and prepare data
    local project_name="PWA-${project_id}: ${location}"
    local start_iso=$(convert_date "$start_date")
    local end_iso=$(convert_date "$due_date")
    
    # Create project JSON (simplified for development)
    local project_json=$(cat <<EOF
{
    "projectName": "$project_name",
    "address": "$location, Thailand",
    "clientInfo": "การประปาส่วนภูมิภาค - $branch",
    "startDate": "$start_iso",
    "estimatedEndDate": "$end_iso"
}
EOF
)
    
    print_step "Creating: $project_name"
    
    # Make API call (no authentication needed for development)
    local response=$(curl -s -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -d "$project_json" \
        "$BASE_URL/api/v1/projects")
    
    local http_code="${response: -3}"
    local body="${response%???}"
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        print_success "Created: $project_name"
        ((SUCCESS_COUNT++))
        return 0
    else
        print_error "Failed: $project_name (HTTP $http_code)"
        if [ -n "$body" ]; then
            echo "  Response: $(echo "$body" | jq '.' 2>/dev/null || echo "$body")"
        fi
        ((FAIL_COUNT++))
        return 1
    fi
}

# Main function
main() {
    echo "🏗️ CSV Project Importer"
    echo "========================"
    echo "CSV File: $CSV_FILE"
    echo "API URL: $BASE_URL"
    echo ""
    
    # Check if CSV exists
    if [ ! -f "$CSV_FILE" ]; then
        print_error "CSV file '$CSV_FILE' not found!"
        exit 1
    fi
    
    # Get admin token (commented out for development)
    # local admin_token=$(get_admin_token)
    echo ""
    
    # Process CSV file
    print_step "Processing CSV file..."
    echo ""
    
    local line_num=0
    while IFS= read -r line; do
        ((line_num++))
        
        # Skip header
        if [ $line_num -eq 1 ]; then
            continue
        fi
        
        # Skip empty lines
        if [ -z "$(echo "$line" | tr -d '[:space:]')" ]; then
            continue
        fi
        
        # Create project (no authentication needed for development)
        create_project "" "$line" "$line_num"
        
        # Brief pause
        sleep 0.3
        
    done < "$CSV_FILE"
    
    # Summary
    echo ""
    echo "📊 Import Summary:"
    echo "• Successful: $SUCCESS_COUNT"
    echo "• Failed: $FAIL_COUNT"
    echo "• Total processed: $((SUCCESS_COUNT + FAIL_COUNT))"
    
    if [ $SUCCESS_COUNT -gt 0 ]; then
        echo ""
        print_success "Import completed! Check projects at: $BASE_URL/api/v1/projects"
        print_success "Swagger UI: $BASE_URL/swagger"
    fi
    
    # Exit with appropriate code
    if [ $FAIL_COUNT -eq 0 ]; then
        exit 0
    else
        exit 1
    fi
}

# Run if executed directly
if [ "${BASH_SOURCE[0]}" = "${0}" ]; then
    main "$@"
fi
