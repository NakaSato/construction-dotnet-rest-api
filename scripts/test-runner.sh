#!/bin/bash

# Production API Test Runner
# Interactive menu to run different API tests
echo "🧪 Production API Test Runner"
echo "============================="

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
RED='\033[0;31m'
NC='\033[0m'

# Check if we're in the right directory
if [[ ! -d "scripts" ]]; then
    echo -e "${RED}❌ Error: Please run this script from the project root directory${NC}"
    echo "   Expected: /path/to/dotnet-rest-api/"
    echo "   Current:  $(pwd)"
    exit 1
fi

# Production API URL
PROD_URL="https://solar-projects-api.azurewebsites.net"

echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Current Time: $(date)${NC}"
echo ""

# Function to check if script exists and is executable
check_script() {
    local script_path="$1"
    if [[ ! -f "$script_path" ]]; then
        echo -e "${RED}❌ Script not found: $script_path${NC}"
        return 1
    fi
    
    if [[ ! -x "$script_path" ]]; then
        echo -e "${YELLOW}⚠️  Making script executable: $script_path${NC}"
        chmod +x "$script_path"
    fi
    
    return 0
}

# Function to run a test script
run_test() {
    local script_path="$1"
    local script_name="$2"
    
    echo -e "${CYAN}🚀 Running: $script_name${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    
    if check_script "$script_path"; then
        ./"$script_path"
        local exit_code=$?
        
        echo ""
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        
        if [[ $exit_code -eq 0 ]]; then
            echo -e "${GREEN}✅ $script_name completed successfully${NC}"
        else
            echo -e "${YELLOW}⚠️  $script_name completed with issues (exit code: $exit_code)${NC}"
        fi
        
        echo ""
        read -p "Press Enter to continue..."
        return $exit_code
    else
        echo -e "${RED}❌ Cannot run $script_name - script not available${NC}"
        echo ""
        read -p "Press Enter to continue..."
        return 1
    fi
}

# Function to show the main menu
show_menu() {
    clear
    echo "🧪 Production API Test Runner"
    echo "============================="
    echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
    echo ""
    echo "Available Tests:"
    echo ""
    echo -e "${GREEN}1)${NC} Quick Health Check ${YELLOW}(⚡ Fast - 30 seconds)${NC}"
    echo "   Basic validation of API health and core functionality"
    echo ""
    echo -e "${GREEN}2)${NC} Enhanced Comprehensive Test ${YELLOW}(🔬 Detailed - 2-3 minutes)${NC}"
    echo "   Complete API testing with performance, security, and error handling"
    echo ""
    echo -e "${GREEN}3)${NC} Authentication Testing ${YELLOW}(🔐 Requires credentials)${NC}"
    echo "   Test authenticated endpoints with JWT tokens"
    echo ""
    echo -e "${GREEN}4)${NC} Database Connectivity Check ${YELLOW}(🗃️  Azure Database)${NC}"
    echo "   Test Azure PostgreSQL database connectivity and health"
    echo ""
    echo -e "${GREEN}5)${NC} View API Documentation ${YELLOW}(📚 Information)${NC}"
    echo "   Open production API testing documentation"
    echo ""
    echo -e "${GREEN}6)${NC} Check API Status ${YELLOW}(🌐 Browser)${NC}"
    echo "   Open production API in browser"
    echo ""
    echo -e "${GREEN}q)${NC} Quit"
    echo ""
    echo -n "Select an option [1-6, q]: "
}

# Function to open documentation
view_documentation() {
    local doc_file="docs/PRODUCTION_API_TESTING.md"
    
    echo -e "${CYAN}📚 Production API Testing Documentation${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    if [[ -f "$doc_file" ]]; then
        echo "Opening documentation file: $doc_file"
        
        # Try to open with different viewers
        if command -v code &> /dev/null; then
            echo "Opening in VS Code..."
            code "$doc_file"
        elif command -v cat &> /dev/null; then
            echo ""
            echo "Documentation content:"
            echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
            head -50 "$doc_file"
            echo ""
            echo "... (showing first 50 lines, open file to see complete documentation)"
        else
            echo "Documentation file available at: $doc_file"
        fi
    else
        echo -e "${RED}❌ Documentation file not found: $doc_file${NC}"
    fi
    
    echo ""
    read -p "Press Enter to continue..."
}

# Function to open API in browser
open_api_browser() {
    echo -e "${CYAN}🌐 Opening Production API in Browser${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "Available URLs:"
    echo "• Production API: $PROD_URL"
    echo "• Health Check: $PROD_URL/health"
    echo "• Test Endpoint: $PROD_URL/api/v1/projects/test"
    echo "• Swagger UI: $PROD_URL/swagger"
    echo ""
    
    # Try to open in browser
    local url_to_open="$PROD_URL/health"
    
    if command -v open &> /dev/null; then
        echo "Opening health check URL in default browser..."
        open "$url_to_open"
    elif command -v xdg-open &> /dev/null; then
        echo "Opening health check URL in default browser..."
        xdg-open "$url_to_open"
    else
        echo "Please manually open one of the URLs above in your browser."
    fi
    
    echo ""
    read -p "Press Enter to continue..."
}

# Function to get authentication credentials
get_auth_credentials() {
    echo -e "${CYAN}🔐 Authentication Testing Setup${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "This test requires valid production credentials."
    echo -e "${YELLOW}⚠️  Default test credentials will likely fail in production.${NC}"
    echo ""
    echo "Options:"
    echo "1) Use default test credentials (will likely fail)"
    echo "2) Provide custom credentials"
    echo "3) Cancel"
    echo ""
    echo -n "Select option [1-3]: "
    
    read -r auth_choice
    
    case $auth_choice in
        1)
            echo "Using default test credentials..."
            run_test "scripts/test-auth-endpoints.sh" "Authentication Testing (Default Credentials)"
            ;;
        2)
            echo ""
            echo -n "Enter username/email: "
            read -r username
            echo -n "Enter password: "
            read -s password
            echo ""
            echo ""
            echo "Running authentication tests with provided credentials..."
            run_test "scripts/test-auth-endpoints.sh \"$username\" \"$password\"" "Authentication Testing (Custom Credentials)"
            ;;
        3)
            echo "Cancelled authentication testing."
            echo ""
            read -p "Press Enter to continue..."
            ;;
        *)
            echo -e "${RED}Invalid option. Cancelling authentication testing.${NC}"
            echo ""
            read -p "Press Enter to continue..."
            ;;
    esac
}

# Main loop
main() {
    while true; do
        show_menu
        read -r choice
        
        case $choice in
            1)
                run_test "scripts/quick-health-check.sh" "Quick Health Check"
                ;;
            2)
                run_test "scripts/test-production-api-enhanced.sh" "Enhanced Comprehensive Test"
                ;;
            3)
                get_auth_credentials
                ;;
            4)
                run_test "scripts/test-db-connectivity.sh" "Database Connectivity Check"
                ;;
            5)
                view_documentation
                ;;
            6)
                open_api_browser
                ;;
            q|Q)
                echo ""
                echo -e "${GREEN}👋 Thank you for using the Production API Test Runner!${NC}"
                echo ""
                exit 0
                ;;
            *)
                echo ""
                echo -e "${RED}❌ Invalid option. Please select 1-6 or 'q' to quit.${NC}"
                echo ""
                read -p "Press Enter to continue..."
                ;;
        esac
    done
}

# Check dependencies
echo "🔍 Checking dependencies..."

if ! command -v curl &> /dev/null; then
    echo -e "${RED}❌ curl is required but not installed.${NC}"
    exit 1
fi

if ! command -v bc &> /dev/null; then
    echo -e "${YELLOW}⚠️  bc (calculator) not found. Some calculations may not work.${NC}"
fi

echo -e "${GREEN}✅ Dependencies check complete${NC}"
echo ""

# Run main menu
main
