#!/bin/bash

# Simple Production API User Registration Script
# This script provides an easy way to register users in the production API

# Configuration
PROD_URL="https://solar-projects-api.azurewebsites.net"
API_BASE="$PROD_URL/api/v1"
AUTH_ENDPOINT="$API_BASE/auth"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Banner
echo -e "${CYAN}╔══════════════════════════════════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                                🚀 Solar Projects API                                        ║${NC}"
echo -e "${CYAN}║                                User Registration Tool                                        ║${NC}"
echo -e "${CYAN}╚══════════════════════════════════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}📍 Production API: $PROD_URL${NC}"
echo -e "${BLUE}⏰ Registration Time: $(date)${NC}"
echo ""

# Function to validate email format
validate_email() {
    local email="$1"
    if [[ "$email" =~ ^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$ ]]; then
        return 0
    else
        return 1
    fi
}

# Function to validate password strength
validate_password() {
    local password="$1"
    
    if [[ ${#password} -lt 8 ]]; then
        echo -e "${RED}❌ Password must be at least 8 characters long${NC}"
        return 1
    fi
    
    if [[ ${#password} -gt 100 ]]; then
        echo -e "${RED}❌ Password cannot exceed 100 characters${NC}"
        return 1
    fi
    
    # Check for uppercase letter
    if [[ ! "$password" =~ [A-Z] ]]; then
        echo -e "${RED}❌ Password must contain at least one uppercase letter${NC}"
        return 1
    fi
    
    # Check for lowercase letter
    if [[ ! "$password" =~ [a-z] ]]; then
        echo -e "${RED}❌ Password must contain at least one lowercase letter${NC}"
        return 1
    fi
    
    # Check for digit
    if [[ ! "$password" =~ [0-9] ]]; then
        echo -e "${RED}❌ Password must contain at least one digit${NC}"
        return 1
    fi
    
    # Check for special character
    if [[ ! "$password" =~ [^a-zA-Z0-9] ]]; then
        echo -e "${RED}❌ Password must contain at least one special character${NC}"
        return 1
    fi
    
    return 0
}

# Function to test API connectivity
test_api_connectivity() {
    echo -e "${BLUE}🔍 Testing API connectivity...${NC}"
    
    local health_response=$(curl -s -w '%{http_code}' -o /dev/null "$PROD_URL/health" --max-time 10)
    
    if [[ "$health_response" == "200" ]]; then
        echo -e "${GREEN}✅ API is reachable${NC}"
        return 0
    else
        echo -e "${RED}❌ API connectivity issue (status: $health_response)${NC}"
        echo -e "${YELLOW}⚠️  Please check your internet connection and try again${NC}"
        return 1
    fi
}

# Function to get available roles
get_roles() {
    echo -e "${BLUE}🔍 Fetching available roles...${NC}"
    
    local roles_response=$(curl -s "$API_BASE/roles" --max-time 10)
    local status_code=$(curl -s -w '%{http_code}' -o /dev/null "$API_BASE/roles" --max-time 10)
    
    if [[ "$status_code" == "200" ]]; then
        echo -e "${GREEN}✅ Roles retrieved successfully${NC}"
        echo ""
        echo "Available roles:"
        echo "$roles_response" | python3 -m json.tool 2>/dev/null || echo "$roles_response"
        echo ""
    else
        echo -e "${YELLOW}⚠️  Could not fetch roles (status: $status_code)${NC}"
        echo "Using default role mapping:"
        echo "1 = Employee, 2 = Manager, 3 = Admin"
        echo ""
    fi
}

# Function to register a user
register_user() {
    local username="$1"
    local email="$2"
    local password="$3"
    local full_name="$4"
    local role_id="${5:-1}"  # Default to Employee role (1)
    
    echo -e "${BLUE}📝 Registering user...${NC}"
    echo "• Username: $username"
    echo "• Email: $email"
    echo "• Full Name: $full_name"
    echo "• Role ID: $role_id"
    echo ""
    
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
    
    echo -e "${BLUE}🚀 Sending registration request...${NC}"
    
    local response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$AUTH_ENDPOINT/register" \
        --max-time 20)
    
    local status_code=$(curl -s -w '%{http_code}' -o /dev/null -X POST \
        -H "Content-Type: application/json" \
        -d "$payload" \
        "$AUTH_ENDPOINT/register" \
        --max-time 20)
    
    echo "Status Code: $status_code"
    echo "Response: $response"
    echo ""
    
    # Check if response indicates success regardless of status code
    local success_check=$(echo "$response" | grep -o '"success"[[:space:]]*:[[:space:]]*true' || echo "")
    
    if [[ -n "$success_check" ]]; then
        echo -e "${GREEN}🎉 Registration successful!${NC}"
        echo -e "${GREEN}✅ User '$username' has been created${NC}"
        echo ""
        echo "Next steps:"
        echo "• You can now login with these credentials"
        echo "• Test the login at: $AUTH_ENDPOINT/login"
        echo "• Access protected endpoints with the JWT token"
        return 0
    fi
    
    case "$status_code" in
        200|201)
            echo -e "${GREEN}🎉 Registration successful!${NC}"
            echo -e "${GREEN}✅ User '$username' has been created${NC}"
            echo ""
            echo "Next steps:"
            echo "• You can now login with these credentials"
            echo "• Test the login at: $AUTH_ENDPOINT/login"
            echo "• Access protected endpoints with the JWT token"
            return 0
            ;;
        400)
            echo -e "${RED}❌ Registration failed - Bad Request${NC}"
            echo -e "${YELLOW}⚠️  Possible issues:${NC}"
            echo "• Username or email already exists"
            echo "• Invalid email format"
            echo "• Password doesn't meet requirements"
            echo "• Missing required fields"
            echo ""
            echo "Server response: $response"
            return 1
            ;;
        409)
            echo -e "${RED}❌ Registration failed - User already exists${NC}"
            echo -e "${YELLOW}⚠️  The username or email is already registered${NC}"
            return 1
            ;;
        500)
            echo -e "${RED}❌ Registration failed - Server Error${NC}"
            echo -e "${YELLOW}⚠️  This could be a database or server issue${NC}"
            echo "Server response: $response"
            return 1
            ;;
        000)
            echo -e "${RED}❌ Registration failed - Connection timeout${NC}"
            echo -e "${YELLOW}⚠️  Please check your internet connection${NC}"
            return 1
            ;;
        *)
            echo -e "${RED}❌ Registration failed - Unexpected response ($status_code)${NC}"
            echo "Server response: $response"
            return 1
            ;;
    esac
}

# Function to collect user information interactively
interactive_registration() {
    echo -e "${CYAN}📋 Interactive User Registration${NC}"
    echo "================================="
    echo ""
    
    # Username
    while true; do
        echo -n "Enter username: "
        read -r username
        
        if [[ -z "$username" ]]; then
            echo -e "${RED}❌ Username cannot be empty${NC}"
            continue
        fi
        
        if [[ ${#username} -lt 3 ]]; then
            echo -e "${RED}❌ Username must be at least 3 characters${NC}"
            continue
        fi
        
        break
    done
    
    # Email
    while true; do
        echo -n "Enter email: "
        read -r email
        
        if [[ -z "$email" ]]; then
            echo -e "${RED}❌ Email cannot be empty${NC}"
            continue
        fi
        
        if ! validate_email "$email"; then
            echo -e "${RED}❌ Invalid email format${NC}"
            continue
        fi
        
        break
    done
    
    # Full Name
    while true; do
        echo -n "Enter full name: "
        read -r full_name
        
        if [[ -z "$full_name" ]]; then
            echo -e "${RED}❌ Full name cannot be empty${NC}"
            continue
        fi
        
        break
    done
    
    # Password
    echo ""
    echo -e "${YELLOW}Password Requirements:${NC}"
    echo "• At least 8 characters long"
    echo "• At least one uppercase letter (A-Z)"
    echo "• At least one lowercase letter (a-z)"
    echo "• At least one digit (0-9)"
    echo "• At least one special character (!@#$%^&*)"
    echo ""
    while true; do
        echo -n "Enter password: "
        read -s password
        echo ""
        
        if [[ -z "$password" ]]; then
            echo -e "${RED}❌ Password cannot be empty${NC}"
            continue
        fi
        
        if ! validate_password "$password"; then
            continue
        fi
        
        echo -n "Confirm password: "
        read -s password_confirm
        echo ""
        
        if [[ "$password" != "$password_confirm" ]]; then
            echo -e "${RED}❌ Passwords do not match${NC}"
            continue
        fi
        
        break
    done
    
    # Role ID
    echo ""
    echo "Role selection:"
    echo "1 = Employee (default)"
    echo "2 = Manager"
    echo "3 = Admin"
    echo ""
    echo -n "Enter role ID [1-3] (default: 1): "
    read -r role_id
    
    if [[ -z "$role_id" ]]; then
        role_id=1
    fi
    
    if [[ ! "$role_id" =~ ^[1-3]$ ]]; then
        echo -e "${YELLOW}⚠️  Invalid role ID, using default (1 = Employee)${NC}"
        role_id=1
    fi
    
    echo ""
    echo -e "${CYAN}📋 Registration Summary${NC}"
    echo "======================="
    echo "Username: $username"
    echo "Email: $email"
    echo "Full Name: $full_name"
    echo "Role ID: $role_id"
    echo ""
    echo -n "Proceed with registration? [y/N]: "
    read -r confirm
    
    if [[ "$confirm" =~ ^[Yy]$ ]]; then
        register_user "$username" "$email" "$password" "$full_name" "$role_id"
    else
        echo -e "${YELLOW}⚠️  Registration cancelled${NC}"
        return 1
    fi
}

# Main script logic
main() {
    # Check for command line arguments
    if [[ $# -eq 0 ]]; then
        # Interactive mode
        if ! test_api_connectivity; then
            exit 1
        fi
        
        get_roles
        interactive_registration
        
    elif [[ $# -eq 4 || $# -eq 5 ]]; then
        # Command line mode
        local username="$1"
        local email="$2"
        local password="$3"
        local full_name="$4"
        local role_id="${5:-1}"
        
        echo -e "${CYAN}📋 Command Line Registration${NC}"
        echo "============================="
        echo ""
        
        # Validate inputs
        if [[ -z "$username" || -z "$email" || -z "$password" || -z "$full_name" ]]; then
            echo -e "${RED}❌ All parameters are required${NC}"
            echo "Usage: $0 <username> <email> <password> <full_name> [role_id]"
            exit 1
        fi
        
        if ! validate_email "$email"; then
            echo -e "${RED}❌ Invalid email format${NC}"
            exit 1
        fi
        
        if ! validate_password "$password"; then
            exit 1
        fi
        
        if ! test_api_connectivity; then
            exit 1
        fi
        
        register_user "$username" "$email" "$password" "$full_name" "$role_id"
        
    else
        # Show usage
        echo -e "${YELLOW}Usage:${NC}"
        echo "  $0                                    # Interactive mode"
        echo "  $0 <username> <email> <password> <full_name> [role_id]   # Command line mode"
        echo ""
        echo -e "${YELLOW}Examples:${NC}"
        echo "  $0"
        echo "  $0 'john.doe' 'john@example.com' 'password123' 'John Doe' 1"
        echo ""
        echo -e "${YELLOW}Role IDs:${NC}"
        echo "  1 = Employee (default)"
        echo "  2 = Manager"
        echo "  3 = Admin"
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    echo -e "${RED}❌ curl is required but not installed${NC}"
    exit 1
fi

# Run main function
main "$@"
