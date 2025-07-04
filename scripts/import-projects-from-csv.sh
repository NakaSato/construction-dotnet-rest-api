#!/bin/bash

# =============================================================================
# 🏗️ PROJECT CSV IMPORT SCRIPT
# =============================================================================
# Imports projects from project.csv file into the Solar Projects API
# Supports Thai language project names and locations
# =============================================================================

set -e  # Exit on any error

# Configuration
BASE_URL="http://localhost:5001"
CSV_FILE="project.csv"
SCRIPT_DIR="$(dirname "$0")"
LOG_DIR="$SCRIPT_DIR/test-logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="$LOG_DIR/csv_import_$TIMESTAMP.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Global variables
ADMIN_TOKEN=""
TOTAL_PROJECTS=0
SUCCESSFUL_IMPORTS=0
FAILED_IMPORTS=0
SKIPPED_IMPORTS=0

# Valid project manager ID (from database)
VALID_MANAGER_ID="cface76b-1457-44a1-89fa-6b4ccc2f5f66"

# Admin credentials
ADMIN_USERNAME="test_admin"
ADMIN_PASSWORD="Admin123!"

# Utility functions
log_message() {
    local level="$1"
    local message="$2"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo "[$timestamp] [$level] $message" | tee -a "$LOG_FILE"
}

print_header() {
    echo -e "${CYAN}=================================================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}=================================================================${NC}"
}

print_step() {
    echo -e "${BLUE}➤ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${PURPLE}ℹ $1${NC}"
}

check_prerequisites() {
    print_step "Checking prerequisites..."
    
    # Check if CSV file exists
    if [ ! -f "$CSV_FILE" ]; then
        print_error "CSV file '$CSV_FILE' not found!"
        exit 1
    fi
    
    # Check if jq is available
    if ! command -v jq >/dev/null 2>&1; then
        print_error "jq is required but not installed. Please install jq."
        exit 1
    fi
    
    # Check API health
    local health_response=$(curl -s "$BASE_URL/health" 2>/dev/null || echo "")
    if [[ "$health_response" != *"Healthy"* ]]; then
        print_error "API is not responding at $BASE_URL"
        print_error "Please ensure the API is running: dotnet run --urls \"http://localhost:5001\""
        exit 1
    fi
    
    print_success "All prerequisites met"
}

login_admin() {
    print_step "Logging in as Admin..."
    
    local login_response=$(curl -s -H "Content-Type: application/json" \
        -d "{\"username\": \"$ADMIN_USERNAME\", \"password\": \"$ADMIN_PASSWORD\"}" \
        "$BASE_URL/api/v1/auth/login" 2>/dev/null || echo "")
    
    ADMIN_TOKEN=$(echo "$login_response" | jq -r '.data.token' 2>/dev/null)
    
    if [ -z "$ADMIN_TOKEN" ] || [ "$ADMIN_TOKEN" = "null" ]; then
        print_error "Admin login failed"
        print_error "Response: $login_response"
        exit 1
    fi
    
    print_success "Admin login successful"
    log_message "INFO" "Admin authenticated successfully"
}

convert_date() {
    local input_date="$1"
    
    # Convert M/D/Y format to ISO 8601 format
    if [[ "$input_date" =~ ^([0-9]{1,2})/([0-9]{1,2})/([0-9]{4})$ ]]; then
        local month="${BASH_REMATCH[1]}"
        local day="${BASH_REMATCH[2]}"
        local year="${BASH_REMATCH[3]}"
        
        # Remove leading zeros to avoid octal interpretation, then pad
        month=$((10#$month))
        day=$((10#$day))
        month=$(printf "%02d" "$month")
        day=$(printf "%02d" "$day")
        
        echo "${year}-${month}-${day}T00:00:00Z"
    else
        # Default to current date if parsing fails
        echo "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    fi
}

parse_equipment_details() {
    local inv125="$1"
    local inv80="$2"
    local inv60="$3"
    local inv40="$4"
    
    # Convert empty values to 0
    inv125="${inv125:-0}"
    inv80="${inv80:-0}"
    inv60="${inv60:-0}"
    inv40="${inv40:-0}"
    
    # Create equipment details JSON
    cat <<EOF
{
    "inverter125kw": $inv125,
    "inverter80kw": $inv80,
    "inverter60kw": $inv60,
    "inverter40kw": $inv40
}
EOF
}

create_project_from_csv_row() {
    local row="$1"
    local line_num="$2"
    
    # Parse CSV row (handle Thai characters properly)
    local IFS=','
    local fields=($row)
    
    # Extract fields from CSV
    local project_id="${fields[0]}"
    local location="${fields[1]}"
    local branch="${fields[2]}"
    local capacity="${fields[3]}"
    local panel_count="${fields[4]}"
    local team="${fields[5]}"
    local connection_type="${fields[6]}"
    local inv125="${fields[7]}"
    local inv80="${fields[8]}"
    local inv60="${fields[9]}"
    local inv40="${fields[10]}"
    local fts="${fields[11]}"
    local revenue="${fields[12]}"
    local start_date="${fields[18]}"
    local due_date="${fields[19]}"
    local duration="${fields[20]}"
    
    # Skip if essential fields are missing
    if [ -z "$project_id" ] || [ -z "$location" ] || [ -z "$branch" ]; then
        print_warning "Skipping line $line_num: Missing essential fields"
        ((SKIPPED_IMPORTS++))
        return 1
    fi
    
    # Convert dates
    local start_date_iso=$(convert_date "$start_date")
    local end_date_iso=$(convert_date "$due_date")
    
    # Parse equipment details
    local equipment_json=$(parse_equipment_details "$inv125" "$inv80" "$inv60" "$inv40")
    
    # Create project name combining ID and location
    local project_name="PWA-${project_id}: ${location}"
    
    # Create client info with branch details
    local client_info="การประปาส่วนภูมิภาค - ${branch}"
    
    # Set default values for missing fields
    local capacity_clean="${capacity:-100.0}"
    local panel_count_clean="${panel_count:-100}"
    local fts_clean="${fts:-1000000}"
    local revenue_clean="${revenue:-1200000}"
    local team_clean="${team:-Solar Team}"
    local connection_type_clean="${connection_type:-MV}"
    
    print_step "Creating project: $project_name"
    
    # Create project JSON payload
    local project_data=$(cat <<EOF
{
    "projectName": "$project_name",
    "address": "$location, Thailand",
    "clientInfo": "$client_info",
    "startDate": "$start_date_iso",
    "estimatedEndDate": "$end_date_iso",
    "projectManagerId": "$VALID_MANAGER_ID",
    "team": "$team_clean",
    "connectionType": "$connection_type_clean",
    "connectionNotes": "Solar installation project for water treatment facility",
    "totalCapacityKw": $capacity_clean,
    "pvModuleCount": $panel_count_clean,
    "equipmentDetails": $equipment_json,
    "ftsValue": $fts_clean,
    "revenueValue": $revenue_clean,
    "pqmValue": 100000,
    "locationCoordinates": {
        "latitude": 13.7563,
        "longitude": 100.5018
    }
}
EOF
)
    
    # Create project via API
    local response=$(curl -s -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $ADMIN_TOKEN" \
        -d "$project_data" \
        "$BASE_URL/api/v1/projects" \
        2>/dev/null || echo "000")
    
    local http_code="${response: -3}"
    local response_body="${response%???}"
    
    if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
        print_success "Created: $project_name"
        log_message "INFO" "Project created successfully: $project_name"
        ((SUCCESSFUL_IMPORTS++))
        return 0
    else
        print_error "Failed to create: $project_name (HTTP $http_code)"
        print_error "Response: $response_body"
        log_message "ERROR" "Project creation failed: $project_name - $response_body"
        ((FAILED_IMPORTS++))
        return 1
    fi
}

import_projects_from_csv() {
    print_step "Reading CSV file: $CSV_FILE"
    
    local line_num=0
    
    # Read CSV file line by line, skipping header
    while IFS= read -r line; do
        ((line_num++))
        
        # Skip header row
        if [ $line_num -eq 1 ]; then
            print_info "Skipping header row"
            continue
        fi
        
        # Skip empty lines
        if [ -z "$(echo "$line" | tr -d '[:space:]')" ]; then
            continue
        fi
        
        ((TOTAL_PROJECTS++))
        
        print_info "Processing line $line_num..."
        
        # Create project from CSV row
        create_project_from_csv_row "$line" "$line_num"
        
        # Small delay to avoid overwhelming the API
        sleep 0.5
        
    done < "$CSV_FILE"
}

generate_summary_report() {
    print_header "📊 IMPORT SUMMARY REPORT"
    
    echo "📁 Source File: $CSV_FILE"
    echo "🕐 Import Time: $(date)"
    echo "📊 Statistics:"
    echo "   • Total projects in CSV: $TOTAL_PROJECTS"
    echo "   • Successfully imported: $SUCCESSFUL_IMPORTS"
    echo "   • Failed imports: $FAILED_IMPORTS"
    echo "   • Skipped imports: $SKIPPED_IMPORTS"
    echo ""
    
    local success_rate=0
    if [ $TOTAL_PROJECTS -gt 0 ]; then
        success_rate=$((SUCCESSFUL_IMPORTS * 100 / TOTAL_PROJECTS))
    fi
    
    echo "📈 Success Rate: $success_rate%"
    echo ""
    
    if [ $SUCCESSFUL_IMPORTS -gt 0 ]; then
        print_success "✅ Import completed with $SUCCESSFUL_IMPORTS successful projects"
    fi
    
    if [ $FAILED_IMPORTS -gt 0 ]; then
        print_error "❌ $FAILED_IMPORTS projects failed to import"
        echo "Check log file for details: $LOG_FILE"
    fi
    
    if [ $SKIPPED_IMPORTS -gt 0 ]; then
        print_warning "⚠️ $SKIPPED_IMPORTS projects were skipped"
    fi
    
    echo ""
    echo "📖 Next Steps:"
    echo "• View imported projects: GET $BASE_URL/api/v1/projects"
    echo "• Check Swagger UI: $BASE_URL/swagger"
    echo "• Review logs: $LOG_FILE"
    
    # Create summary in log file
    log_message "SUMMARY" "Import completed: $SUCCESSFUL_IMPORTS/$TOTAL_PROJECTS successful"
}

verify_imports() {
    print_step "Verifying imported projects..."
    
    local response=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
        "$BASE_URL/api/v1/projects" 2>/dev/null || echo "")
    
    local project_count=$(echo "$response" | jq '.data.totalCount // .totalCount // 0' 2>/dev/null)
    
    if [ "$project_count" -gt 0 ]; then
        print_success "Found $project_count projects in database"
        
        # Show a few examples
        echo ""
        print_info "Recent projects:"
        echo "$response" | jq -r '.data.items[:3][] | "• \(.projectName) - \(.status)"' 2>/dev/null || echo "• Unable to parse project details"
    else
        print_warning "No projects found in database"
    fi
}

main() {
    # Create log directory
    mkdir -p "$LOG_DIR"
    
    # Initialize log file
    echo "CSV Project Import - $(date)" > "$LOG_FILE"
    echo "============================================" >> "$LOG_FILE"
    
    print_header "🏗️ CSV PROJECT IMPORT TOOL"
    
    echo "Importing solar projects from CSV file into the API"
    echo "CSV File: $CSV_FILE"
    echo "API URL: $BASE_URL"
    echo "Log File: $LOG_FILE"
    echo ""
    
    # Main import process
    check_prerequisites
    echo ""
    
    login_admin
    echo ""
    
    import_projects_from_csv
    echo ""
    
    verify_imports
    echo ""
    
    generate_summary_report
    
    # Return appropriate exit code
    if [ $FAILED_IMPORTS -eq 0 ]; then
        exit 0
    else
        exit 1
    fi
}

# Check if script is being run directly
if [ "${BASH_SOURCE[0]}" = "${0}" ]; then
    main "$@"
fi
