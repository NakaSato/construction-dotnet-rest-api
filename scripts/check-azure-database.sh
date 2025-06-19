#!/bin/bash

# Azure Database Connectivity and Health Check Script
echo "🗃️  Azure Database Check for Solar Projects API"
echo "=============================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
HEALTH_ENDPOINT="$PROD_URL/health"
DEBUG_ENDPOINT="$PROD_URL/debug"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Test results
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0
WARNING_CHECKS=0

# Function to print colored output
print_section() {
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}🔍 $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_check() {
    echo -e "${BLUE}🧪 Checking: $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
    ((PASSED_CHECKS++))
}

print_fail() {
    echo -e "${RED}❌ FAIL: $1${NC}"
    ((FAILED_CHECKS++))
}

print_warning() {
    echo -e "${YELLOW}⚠️  WARN: $1${NC}"
    ((WARNING_CHECKS++))
}

print_info() {
    echo -e "${PURPLE}ℹ️  INFO: $1${NC}"
}

count_check() {
    ((TOTAL_CHECKS++))
}

# Function to check database connectivity through API
check_database_via_api() {
    print_section "Database Connectivity via API"
    
    # Check if the API can connect to database through health endpoint
    count_check
    print_check "Health endpoint database connectivity"
    
    local health_response=$(curl -s "$HEALTH_ENDPOINT" 2>/dev/null)
    local health_status=$(curl -s -w '%{http_code}' -o /dev/null "$HEALTH_ENDPOINT" 2>/dev/null)
    
    if [[ "$health_status" == "200" ]]; then
        print_success "Health endpoint responding (API is running)"
        
        # Check if health response includes database status
        if echo "$health_response" | grep -q -i "database\|db\|postgres"; then
            print_info "Health response includes database information"
        else
            print_info "Health response: $health_response"
        fi
    else
        print_fail "Health endpoint not responding (status: $health_status)"
        return 1
    fi
}

# Function to test database-dependent endpoints
test_database_endpoints() {
    print_section "Database-Dependent Endpoint Tests"
    
    # Test endpoints that require database operations
    local endpoints=(
        "$API_BASE/projects:Projects data"
        "$API_BASE/users:Users data"
        "$API_BASE/daily-reports:Daily reports data"
        "$API_BASE/work-requests:Work requests data"
        "$API_BASE/master-plans:Master plans data"
        "$API_BASE/tasks:Tasks data"
    )
    
    for endpoint_info in "${endpoints[@]}"; do
        local endpoint=$(echo "$endpoint_info" | cut -d':' -f1)
        local description=$(echo "$endpoint_info" | cut -d':' -f2)
        
        count_check
        print_check "$description endpoint"
        
        local response=$(curl -s -w '%{http_code}' -o /dev/null "$endpoint" 2>/dev/null)
        
        case "$response" in
            200)
                print_success "$description - Database query successful"
                ;;
            401)
                print_success "$description - Endpoint protected (auth required) - Database likely OK"
                ;;
            500)
                print_fail "$description - Server error (possible database issue)"
                ;;
            502|503|504)
                print_fail "$description - Service unavailable (possible database connection issue)"
                ;;
            404)
                print_warning "$description - Endpoint not found (check routing)"
                ;;
            *)
                print_warning "$description - Unexpected response: $response"
                ;;
        esac
    done
}

# Function to check Entity Framework migrations status
check_ef_migrations() {
    print_section "Entity Framework Migrations Status"
    
    count_check
    print_check "Database migration status"
    
    # Try to access a debug endpoint that might show migration info
    local debug_response=$(curl -s "$DEBUG_ENDPOINT/database" 2>/dev/null)
    local debug_status=$(curl -s -w '%{http_code}' -o /dev/null "$DEBUG_ENDPOINT/database" 2>/dev/null)
    
    if [[ "$debug_status" == "200" ]]; then
        print_success "Debug endpoint accessible - checking migration status"
        print_info "Debug response: $debug_response"
    else
        print_warning "Debug endpoint not available (status: $debug_status)"
        print_info "This is normal in production environments"
    fi
    
    # Alternative: Check if we can perform basic database operations
    count_check
    print_check "Basic database operations test"
    
    # Test the public test endpoint which might hit the database
    local test_response=$(curl -s "$API_BASE/projects/test" 2>/dev/null)
    local test_status=$(curl -s -w '%{http_code}' -o /dev/null "$API_BASE/projects/test" 2>/dev/null)
    
    if [[ "$test_status" == "200" ]]; then
        print_success "Test endpoint working - basic API functionality OK"
        if echo "$test_response" | grep -q -i "working"; then
            print_info "API reports it's working correctly"
        fi
    else
        print_fail "Test endpoint not responding (status: $test_status)"
    fi
}

# Function to analyze Azure deployment configuration
check_azure_configuration() {
    print_section "Azure Deployment Configuration Analysis"
    
    print_info "Analyzing Azure deployment setup..."
    
    # Check if we can determine the environment
    count_check
    print_check "Environment detection"
    
    local health_response=$(curl -s "$HEALTH_ENDPOINT" 2>/dev/null)
    if echo "$health_response" | grep -q -i "environment"; then
        local environment=$(echo "$health_response" | jq -r '.environment' 2>/dev/null || echo "Unknown")
        print_success "Environment detected: $environment"
    else
        print_warning "Environment information not available in health response"
    fi
    
    # Check application insights connectivity (indicates Azure services)
    count_check
    print_check "Azure services integration"
    
    if echo "$health_response" | grep -q -i "version\|timestamp"; then
        print_success "Application appears to be properly configured with health checks"
    else
        print_warning "Limited health check information available"
    fi
}

# Function to test database performance
test_database_performance() {
    print_section "Database Performance Test"
    
    count_check
    print_check "Database response time"
    
    # Test response time of database-dependent endpoint
    local start_time=$(date +%s.%N)
    local response=$(curl -s -w '%{http_code}' -o /dev/null "$API_BASE/projects" 2>/dev/null)
    local end_time=$(date +%s.%N)
    
    if command -v bc &> /dev/null; then
        local duration=$(echo "$end_time - $start_time" | bc)
        local duration_ms=$(echo "$duration * 1000" | bc | cut -d'.' -f1)
        
        if [[ "$response" == "401" ]] || [[ "$response" == "200" ]]; then
            if (( $(echo "$duration < 2.0" | bc -l) )); then
                print_success "Database response time good: ${duration_ms}ms (<2000ms)"
            elif (( $(echo "$duration < 5.0" | bc -l) )); then
                print_warning "Database response time acceptable: ${duration_ms}ms (2000-5000ms)"
            else
                print_fail "Database response time slow: ${duration_ms}ms (>5000ms)"
            fi
        else
            print_warning "Cannot test performance - endpoint returned: $response"
        fi
    else
        print_warning "bc calculator not available - cannot measure precise timing"
    fi
}

# Function to check Azure database configuration hints
check_azure_database_hints() {
    print_section "Azure Database Configuration Hints"
    
    print_info "Checking for PostgreSQL Azure Database configuration..."
    
    # Check if application is using PostgreSQL (based on our configuration)
    count_check
    print_check "PostgreSQL compatibility"
    
    # Look for PostgreSQL-specific behavior in responses
    local test_response=$(curl -s "$API_BASE/projects/test" 2>/dev/null)
    
    if [[ -n "$test_response" ]]; then
        print_success "API responding - database driver appears functional"
        
        # Check if response includes any PostgreSQL-specific elements
        if echo "$test_response" | grep -q -i "postgres\|npgsql"; then
            print_info "PostgreSQL-specific elements detected in response"
        fi
    else
        print_fail "No response from test endpoint"
    fi
    
    # Provide information about expected Azure database setup
    print_info "Expected Azure Database Configuration:"
    echo "   • Database Type: PostgreSQL Flexible Server"
    echo "   • Database Name: SolarProjectsDb"
    echo "   • Admin User: solaradmin"
    echo "   • SSL Mode: Required"
    echo "   • Port: 5432"
}

# Function to provide troubleshooting guidance
provide_troubleshooting_guidance() {
    print_section "Database Troubleshooting Guidance"
    
    print_info "Common database issues and solutions:"
    echo ""
    echo "📋 Connection Issues:"
    echo "   • Check Azure PostgreSQL server firewall rules"
    echo "   • Verify connection string in Azure App Service configuration"
    echo "   • Ensure SSL Mode is set to 'Require'"
    echo ""
    echo "🔧 Performance Issues:"
    echo "   • Check Azure PostgreSQL performance metrics"
    echo "   • Review connection pooling settings"
    echo "   • Monitor Azure PostgreSQL DTU/CPU usage"
    echo ""
    echo "🗃️  Migration Issues:"
    echo "   • Verify Entity Framework migrations have been applied"
    echo "   • Check if database schema exists"
    echo "   • Review application logs for migration errors"
    echo ""
    echo "🔍 Debugging Commands:"
    echo "   # Check Azure PostgreSQL server status"
    echo "   az postgres flexible-server show --name solar-projects-db-staging --resource-group solar-projects-rg"
    echo ""
    echo "   # Check Azure App Service logs"
    echo "   az webapp log tail --name solar-projects-api --resource-group solar-projects-rg"
    echo ""
    echo "   # Check Azure App Service configuration"
    echo "   az webapp config appsettings list --name solar-projects-api --resource-group solar-projects-rg"
}

# Function to generate comprehensive report
generate_database_report() {
    print_section "Database Health Report"
    
    echo -e "${CYAN}📊 Database Check Summary${NC}"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo -e "Total Checks: ${BLUE}$TOTAL_CHECKS${NC}"
    echo -e "Passed: ${GREEN}$PASSED_CHECKS${NC}"
    echo -e "Failed: ${RED}$FAILED_CHECKS${NC}"
    echo -e "Warnings: ${YELLOW}$WARNING_CHECKS${NC}"
    
    local success_rate=0
    if [[ $TOTAL_CHECKS -gt 0 ]]; then
        success_rate=$(echo "scale=1; $PASSED_CHECKS * 100 / $TOTAL_CHECKS" | bc -l 2>/dev/null || echo "N/A")
    fi
    
    echo -e "Success Rate: ${GREEN}${success_rate}%${NC}"
    echo ""
    
    # Determine overall database health
    if [[ $FAILED_CHECKS -eq 0 ]]; then
        echo -e "${GREEN}🎉 Database appears to be healthy and properly connected!${NC}"
        echo -e "${GREEN}   ✓ API is responding correctly${NC}"
        echo -e "${GREEN}   ✓ Database operations appear functional${NC}"
        echo -e "${GREEN}   ✓ No critical connectivity issues detected${NC}"
    elif [[ $FAILED_CHECKS -le 2 ]]; then
        echo -e "${YELLOW}⚠️  Database appears mostly functional with some potential issues.${NC}"
        echo -e "${YELLOW}   • Review failed checks above${NC}"
        echo -e "${YELLOW}   • Monitor performance and connectivity${NC}"
        echo -e "${YELLOW}   • Consider reviewing Azure database metrics${NC}"
    else
        echo -e "${RED}❌ Multiple database issues detected!${NC}"
        echo -e "${RED}   • Database connectivity may be compromised${NC}"
        echo -e "${RED}   • Check Azure PostgreSQL server status${NC}"
        echo -e "${RED}   • Review application logs for errors${NC}"
        echo -e "${RED}   • Verify connection string configuration${NC}"
    fi
    
    echo ""
    echo -e "${CYAN}🔗 Useful Resources:${NC}"
    echo "• Azure Portal: https://portal.azure.com"
    echo "• Application Logs: Azure Portal → App Services → solar-projects-api → Log stream"
    echo "• Database Metrics: Azure Portal → PostgreSQL → solar-projects-db-staging → Metrics"
    echo "• Connection Test: $API_BASE/projects/test"
    
    # Save results to file
    local timestamp=$(date +%Y%m%d-%H%M%S)
    echo "💾 Saving database check results to database-check-$timestamp.txt"
    {
        echo "Azure Database Health Check Results - $(date)"
        echo "============================================="
        echo "Total Checks: $TOTAL_CHECKS"
        echo "Passed: $PASSED_CHECKS"
        echo "Failed: $FAILED_CHECKS"
        echo "Warnings: $WARNING_CHECKS"
        echo "Success Rate: $success_rate%"
        echo ""
        echo "Production API: $PROD_URL"
        echo "Health Endpoint: $HEALTH_ENDPOINT"
        echo "Test Endpoint: $API_BASE/projects/test"
    } > "database-check-$timestamp.txt"
}

# Main execution
main() {
    echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
    echo -e "${BLUE}⏰ Database Check Started: $(date)${NC}"
    echo ""
    
    # Check dependencies
    if ! command -v curl &> /dev/null; then
        echo -e "${RED}❌ curl is required but not installed${NC}"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        print_warning "'jq' not found - JSON parsing will be limited"
    fi
    
    if ! command -v bc &> /dev/null; then
        print_warning "'bc' calculator not found - timing calculations will be limited"
    fi
    
    # Run all database checks
    check_database_via_api
    test_database_endpoints
    check_ef_migrations
    check_azure_configuration
    test_database_performance
    check_azure_database_hints
    provide_troubleshooting_guidance
    
    # Generate final report
    generate_database_report
    
    # Exit with appropriate code
    if [[ $FAILED_CHECKS -eq 0 ]]; then
        exit 0
    elif [[ $FAILED_CHECKS -le 2 ]]; then
        exit 1
    else
        exit 2
    fi
}

# Run the database check
main "$@"
