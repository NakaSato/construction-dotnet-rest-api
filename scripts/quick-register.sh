#!/bin/bash

# Quick Production API User Registration
echo "⚡ Quick User Registration for Production API"
echo "============================================="

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
AUTH_ENDPOINT="$API_BASE/auth"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Quick registration function
quick_register() {
    local username="$1"
    local email="$2"
    local password="$3"
    local full_name="$4"
    local role_id="${5:-1}"  # Default to Employee role
    
    echo -e "${BLUE}📝 Registering: $username ($email)${NC}"
    
    local payload=$(cat <<EOF
{
    "username": "$username",
    "email": "$email", 
    "password": "$password",
    "fullName": "$full_name",
    "roleId": $role_id
}
EOF
)
    
    local response=$(curl -s -w '%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$AUTH_ENDPOINT/register" \
        --max-time 20)
    
    local status_code=$(echo "$response" | tail -c 4)
    
    if [[ "$status_code" == "200" || "$status_code" == "201" ]]; then
        echo -e "${GREEN}✅ Registration successful!${NC}"
        return 0
    else
        echo -e "${RED}❌ Registration failed (HTTP $status_code)${NC}"
        return 1
    fi
}

# Quick login test
quick_login() {
    local username="$1"
    local password="$2"
    
    echo -e "${BLUE}🔐 Testing login for: $username${NC}"
    
    local payload=$(cat <<EOF
{
    "username": "$username",
    "password": "$password"
}
EOF
)
    
    local response=$(curl -s -w '%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$AUTH_ENDPOINT/login" \
        --max-time 20)
    
    local status_code=$(echo "$response" | tail -c 4)
    
    if [[ "$status_code" == "200" ]]; then
        echo -e "${GREEN}✅ Login successful!${NC}"
        return 0
    else
        echo -e "${RED}❌ Login failed (HTTP $status_code)${NC}"
        return 1
    fi
}

# Show usage
show_usage() {
    echo "Usage: $0 [username] [email] [password] [full_name] [role_id]"
    echo ""
    echo "Quick registration for production API"
    echo ""
    echo "Parameters:"
    echo "  username   - Username (3-50 chars, alphanumeric + underscore)"
    echo "  email      - Valid email address"
    echo "  password   - Strong password (8+ chars with uppercase, lowercase, digit, special char)"
    echo "  full_name  - Full name (2-100 characters)"
    echo "  role_id    - Role ID: 1=Employee, 2=ProjectManager, 3=Administrator (optional, default=1)"
    echo ""
    echo "Examples:"
    echo "  $0 testuser test@example.com 'TestPass123!' 'Test User'"
    echo "  $0 manager1 manager@example.com 'ManagerPass123!' 'Project Manager' 2"
    echo ""
    echo "For interactive registration, use: ./scripts/register-production-user.sh"
}

# Main execution
if [[ $# -lt 4 ]]; then
    show_usage
    exit 1
fi

# Get parameters
username="$1"
email="$2"
password="$3"
full_name="$4"
role_id="${5:-1}"

echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Time: $(date)${NC}"
echo ""

# Test API availability
echo -n "Testing API availability... "
health_check=$(curl -s -w '%{http_code}' -o /dev/null "$PROD_URL/health" --max-time 10)

if [[ "$health_check" == "200" ]]; then
    echo -e "${GREEN}✅ API available${NC}"
else
    echo -e "${RED}❌ API unavailable (HTTP $health_check)${NC}"
    exit 1
fi

echo ""

# Register user
if quick_register "$username" "$email" "$password" "$full_name" "$role_id"; then
    echo ""
    echo -e "${YELLOW}🧪 Testing login with new credentials...${NC}"
    
    if quick_login "$username" "$password"; then
        echo ""
        echo -e "${GREEN}🎉 Registration and login test completed successfully!${NC}"
        echo ""
        echo "Credentials:"
        echo "• Username: $username"
        echo "• Email: $email"
        echo "• Role ID: $role_id"
        echo ""
        echo "You can now use these credentials to:"
        echo "• Test authenticated endpoints"
        echo "• Access the API with proper authentication"
        echo "• Use in authentication testing scripts"
    else
        echo ""
        echo -e "${YELLOW}⚠️  Registration succeeded but login test failed${NC}"
        echo "The user was created but there may be an issue with authentication"
    fi
else
    echo ""
    echo -e "${RED}❌ Registration failed${NC}"
    echo ""
    echo "Common reasons:"
    echo "• Username or email already exists"
    echo "• Password doesn't meet requirements"
    echo "• Invalid role ID"
    echo "• Database connection issues"
    echo ""
    echo "For detailed error messages, use: ./scripts/register-production-user.sh --interactive"
    exit 1
fi
