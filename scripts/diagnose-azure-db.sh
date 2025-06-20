#!/bin/bash

# Azure Database Connection Troubleshooting Script
echo "🔧 Azure Database Connection Troubleshooting"
echo "============================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
RESOURCE_GROUP="solar-projects-rg"
APP_NAME="solar-projects-api"
DB_SERVER="solar-projects-db-staging"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Diagnostic Time: $(date)${NC}"
echo ""

# Function to test API response
test_api_response() {
    local endpoint="$1"
    local description="$2"
    
    echo -e "${CYAN}Testing: $description${NC}"
    echo "Endpoint: $endpoint"
    
    local response=$(curl -s "$endpoint" 2>/dev/null)
    local status_code=$(curl -s -w '%{http_code}' -o /dev/null "$endpoint" 2>/dev/null)
    
    echo "Status Code: $status_code"
    echo "Response: $response"
    
    if echo "$response" | grep -q "Name or service not known"; then
        echo -e "${RED}❌ DNS/Database connection issue detected!${NC}"
        return 1
    elif [[ "$status_code" == "200" ]]; then
        echo -e "${GREEN}✅ Working properly${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠️  Other issue (status: $status_code)${NC}"
        return 2
    fi
    
    echo ""
}

echo "🧪 Testing API Endpoints for Database Connection Issues:"
echo "========================================================="

# Test endpoints that don't require database
echo ""
echo -e "${BLUE}Non-Database Endpoints:${NC}"
test_api_response "$PROD_URL/health" "Health Check"
echo ""

# Test endpoints that require database
echo -e "${BLUE}Database-Dependent Endpoints:${NC}"
test_api_response "$PROD_URL/api/v1/projects/test" "Test Endpoint"
echo ""

# Test authentication endpoints
echo -e "${BLUE}Authentication Endpoints:${NC}"
echo "Testing Login endpoint with invalid credentials..."
response=$(curl -s -X POST -H "Content-Type: application/json" -d '{"username":"test","password":"test"}' "$PROD_URL/api/v1/auth/login")
echo "Login Response: $response"

if echo "$response" | grep -q "Name or service not known"; then
    echo -e "${RED}❌ Database connection issue in authentication!${NC}"
    database_issue=true
else
    echo -e "${GREEN}✅ Authentication endpoint responding (may still reject credentials)${NC}"
    database_issue=false
fi

echo ""
echo "Testing Registration endpoint..."
response=$(curl -s -X POST -H "Content-Type: application/json" -d '{"username":"test","email":"test@test.com","password":"Test123!","fullName":"Test","roleId":1}' "$PROD_URL/api/v1/auth/register")
echo "Registration Response: $response"

if echo "$response" | grep -q "Name or service not known"; then
    echo -e "${RED}❌ Database connection issue in registration!${NC}"
    database_issue=true
else
    echo -e "${GREEN}✅ Registration endpoint responding (may still have validation errors)${NC}"
fi

echo ""
echo "📊 Diagnosis Summary:"
echo "===================="

if [[ "$database_issue" == "true" ]]; then
    echo -e "${RED}❌ CRITICAL: Database connection issues detected${NC}"
    echo ""
    echo -e "${YELLOW}Possible Causes:${NC}"
    echo "1. PostgreSQL server is down or unreachable"
    echo "2. Incorrect database hostname in connection string"
    echo "3. Network/firewall issues between App Service and PostgreSQL"
    echo "4. PostgreSQL server configuration issues"
    echo "5. Connection string format issues"
    echo ""
    echo -e "${YELLOW}Recommended Actions:${NC}"
    echo ""
    echo "1. Check PostgreSQL server status:"
    echo "   az postgres flexible-server show --name $DB_SERVER --resource-group $RESOURCE_GROUP"
    echo ""
    echo "2. Check App Service connection strings:"
    echo "   az webapp config connection-string list --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo ""
    echo "3. Check App Service logs for detailed errors:"
    echo "   az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo ""
    echo "4. Test database connectivity from App Service:"
    echo "   az webapp ssh --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo "   # Then inside the SSH session:"
    echo "   # ping $DB_SERVER.postgres.database.azure.com"
    echo ""
    echo "5. Verify PostgreSQL server firewall rules:"
    echo "   az postgres flexible-server firewall-rule list --name $DB_SERVER --resource-group $RESOURCE_GROUP"
    echo ""
    echo "6. Check if PostgreSQL server allows Azure services:"
    echo "   # Should have a rule allowing Azure services (0.0.0.0-0.0.0.0)"
    echo ""
    echo -e "${CYAN}💡 Quick Fix Attempts:${NC}"
    echo ""
    echo "If you have Azure CLI configured, try these commands:"
    echo ""
    echo "# 1. Restart the PostgreSQL server"
    echo "az postgres flexible-server restart --name $DB_SERVER --resource-group $RESOURCE_GROUP"
    echo ""
    echo "# 2. Restart the App Service"
    echo "az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP"
    echo ""
    echo "# 3. Add/update firewall rule for Azure services"
    echo "az postgres flexible-server firewall-rule create \\"
    echo "  --name $DB_SERVER \\"
    echo "  --resource-group $RESOURCE_GROUP \\"
    echo "  --rule-name 'AllowAzureServices' \\"
    echo "  --start-ip-address 0.0.0.0 \\"
    echo "  --end-ip-address 0.0.0.0"
    
else
    echo -e "${GREEN}✅ Database connection appears to be working${NC}"
    echo ""
    echo "The API endpoints are responding normally."
    echo "Any registration/login failures are likely due to:"
    echo "• Invalid credentials"
    echo "• Password complexity requirements"
    echo "• Duplicate user records"
    echo "• Application logic errors (not database connectivity)"
fi

echo ""
echo "🔧 Manual Verification Commands:"
echo "================================"
echo ""
echo "# Check Azure resource status:"
echo "az group show --name $RESOURCE_GROUP"
echo ""
echo "# Check PostgreSQL server details:"
echo "az postgres flexible-server show --name $DB_SERVER --resource-group $RESOURCE_GROUP"
echo ""
echo "# Check App Service status:"
echo "az webapp show --name $APP_NAME --resource-group $RESOURCE_GROUP --query '{name:name,state:state,defaultHostName:defaultHostName}'"
echo ""
echo "# Monitor App Service logs in real-time:"
echo "az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"

exit 0
