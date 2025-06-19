#!/bin/bash

# Azure Database Direct Connectivity Test
echo "🔗 Azure Database Direct Connectivity Test"
echo "=========================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Function to test a specific endpoint
test_db_endpoint() {
    local endpoint="$1"
    local description="$2"
    
    echo -n "Testing $description... "
    
    local response=$(curl -s -w '%{http_code}' -o /dev/null "$endpoint" --max-time 15)
    
    case "$response" in
        200)
            echo -e "${GREEN}✅ SUCCESS (200)${NC} - Database accessible"
            return 0
            ;;
        401)
            echo -e "${GREEN}✅ PROTECTED (401)${NC} - Database accessible, auth required"
            return 0
            ;;
        500)
            echo -e "${RED}❌ ERROR (500)${NC} - Database connection issue"
            return 1
            ;;
        502|503|504)
            echo -e "${RED}❌ UNAVAILABLE ($response)${NC} - Service/Database down"
            return 1
            ;;
        404)
            echo -e "${YELLOW}⚠️  NOT FOUND (404)${NC} - Endpoint doesn't exist"
            return 2
            ;;
        000)
            echo -e "${RED}❌ NO RESPONSE${NC} - Connection timeout or DNS issue"
            return 1
            ;;
        *)
            echo -e "${YELLOW}⚠️  UNEXPECTED ($response)${NC}"
            return 2
            ;;
    esac
}

# Function to get detailed response for analysis
get_detailed_response() {
    local endpoint="$1"
    echo "Getting detailed response from: $endpoint"
    echo "─────────────────────────────────────────────────────────────"
    curl -s "$endpoint" | head -200
    echo ""
    echo "─────────────────────────────────────────────────────────────"
}

echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Test Time: $(date)${NC}"
echo ""

# Test core endpoints that would require database connectivity
echo "🧪 Testing Database-Connected Endpoints:"
echo "========================================="

# Test results tracking
total=0
success=0
database_issues=0

# Health endpoint (minimal database dependency)
test_db_endpoint "$PROD_URL/health" "Health Check"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); fi

# Test endpoint (should work if API is functional)
test_db_endpoint "$API_BASE/projects/test" "Test Endpoint"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); fi

# Database-dependent endpoints
echo ""
echo "🗃️  Testing Database-Dependent Operations:"
echo "=========================================="

test_db_endpoint "$API_BASE/projects" "Projects List"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); elif [[ $result -eq 1 ]]; then ((database_issues++)); fi

test_db_endpoint "$API_BASE/users" "Users List" 
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); elif [[ $result -eq 1 ]]; then ((database_issues++)); fi

test_db_endpoint "$API_BASE/daily-reports" "Daily Reports"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); elif [[ $result -eq 1 ]]; then ((database_issues++)); fi

test_db_endpoint "$API_BASE/master-plans" "Master Plans"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); elif [[ $result -eq 1 ]]; then ((database_issues++)); fi

test_db_endpoint "$API_BASE/tasks" "Tasks"
result=$?; ((total++)); if [[ $result -eq 0 ]]; then ((success++)); elif [[ $result -eq 1 ]]; then ((database_issues++)); fi

# Summary
echo ""
echo "📊 Database Connectivity Summary:"
echo "=================================="
echo "Total Tests: $total"
echo "Successful: $success"
echo "Database Issues: $database_issues"

if [[ $database_issues -eq 0 ]]; then
    echo -e "${GREEN}🎉 Database connectivity appears healthy!${NC}"
    
    # Get sample responses to verify data flow
    echo ""
    echo "📋 Sample API Responses:"
    echo "========================="
    get_detailed_response "$PROD_URL/health"
    get_detailed_response "$API_BASE/projects/test"
    
elif [[ $database_issues -lt 3 ]]; then
    echo -e "${YELLOW}⚠️  Some database connectivity issues detected.${NC}"
else
    echo -e "${RED}❌ Multiple database connectivity issues!${NC}"
fi

# Azure-specific checks
echo ""
echo "☁️  Azure-Specific Database Checks:"
echo "==================================="

echo -n "Checking SSL/HTTPS connectivity... "
if curl -s -I "$PROD_URL" | grep -q "200 OK"; then
    echo -e "${GREEN}✅ HTTPS working${NC}"
else
    echo -e "${RED}❌ HTTPS issues${NC}"
fi

echo -n "Checking response headers for Azure indicators... "
headers=$(curl -s -I "$PROD_URL")
if echo "$headers" | grep -q -i "azure\|microsoft"; then
    echo -e "${GREEN}✅ Azure hosting confirmed${NC}"
else
    echo -e "${YELLOW}⚠️  Azure headers not detected${NC}"
fi

# Recommendations
echo ""
echo "💡 Next Steps for Database Issues:"
echo "=================================="
echo "1. Check Azure Portal → PostgreSQL server status"
echo "2. Verify connection string in App Service settings"
echo "3. Check firewall rules allow App Service connections"
echo "4. Review App Service logs: az webapp log tail --name solar-projects-api --resource-group solar-projects-rg"
echo "5. Test connection string manually with psql or pgAdmin"

# Azure CLI commands for troubleshooting
echo ""
echo "🔧 Azure CLI Troubleshooting Commands:"
echo "====================================="
echo "# Check PostgreSQL server"
echo "az postgres flexible-server show --name solar-projects-db-staging --resource-group solar-projects-rg"
echo ""
echo "# Check App Service configuration"
echo "az webapp config connection-string list --name solar-projects-api --resource-group solar-projects-rg"
echo ""
echo "# Check App Service logs"
echo "az webapp log tail --name solar-projects-api --resource-group solar-projects-rg"
echo ""
echo "# Test database connectivity from App Service"
echo "az webapp ssh --name solar-projects-api --resource-group solar-projects-rg"

exit $database_issues
