#!/bin/bash

# Production API User Registration Script
echo "👤 Production API User Registration"
echo "==================================="

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
PURPLE='\033[0;35m'
NC='\033[0m'

# Function to print colored output
print_header() {
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_fail() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${PURPLE}ℹ️  $1${NC}"
}

print_step() {
    echo -e "${BLUE}🔸 $1${NC}"
}

# Function to validate email format
validate_email() {
    local email="$1"
    if [[ "$email" =~ ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; then
        return 0
    else
        return 1
    fi
}

# Function to validate password strength
validate_password() {
    local password="$1"
    
    # Check length (at least 8 characters)
    if [[ ${#password} -lt 8 ]]; then
        echo "Password must be at least 8 characters long"
        return 1
    fi
    
    # Check for uppercase letter
    if [[ ! "$password" =~ [A-Z] ]]; then
        echo "Password must contain at least one uppercase letter"
        return 1
    fi
    
    # Check for lowercase letter
    if [[ ! "$password" =~ [a-z] ]]; then
        echo "Password must contain at least one lowercase letter"
        return 1
    fi
    
    # Check for digit
    if [[ ! "$password" =~ [0-9] ]]; then
        echo "Password must contain at least one digit"
        return 1
    fi
    
    # Check for special character
    if [[ ! "$password" =~ [@\$\!\%\*\?\&] ]]; then
        echo "Password must contain at least one special character (@\$!%*?&)"
        return 1
    fi
    
    return 0
}

# Function to validate username
validate_username() {
    local username="$1"
    
    # Check length (3-50 characters)
    if [[ ${#username} -lt 3 || ${#username} -gt 50 ]]; then
        echo "Username must be between 3 and 50 characters"
        return 1
    fi
    
    # Check format (letters, numbers, underscores only)
    if [[ ! "$username" =~ ^[a-zA-Z0-9_]+$ ]]; then
        echo "Username can only contain letters, numbers, and underscores"
        return 1
    fi
    
    return 0
}

# Function to register a user
register_user() {
    local username="$1"
    local email="$2"
    local password="$3"
    local full_name="$4"
    local role_id="$5"
    
    print_step "Registering user: $username ($email)"
    
    # Create registration payload
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
    
    # Make registration request
    local response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -H "User-Agent: ProductionRegistrationScript/1.0" \
        -d "$payload" \
        "$AUTH_ENDPOINT/register" \
        --max-time 30)
    
    local body=$(echo "$response" | sed '$d')
    local status_code=$(echo "$response" | tail -n1)
    
    case "$status_code" in
        200|201)
            print_success "User registered successfully!"
            
            # Try to extract user info from response
            if command -v jq &> /dev/null; then
                echo "$body" | jq -r '.data // .user // .' 2>/dev/null | head -10
            else
                echo "Response: $(echo "$body" | head -200)"
            fi
            
            return 0
            ;;
        400)
            print_fail "Registration failed - Bad Request"
            print_warning "Validation errors or duplicate user"
            
            if command -v jq &> /dev/null; then
                local error_msg=$(echo "$body" | jq -r '.message // .error // .' 2>/dev/null)
                if [[ "$error_msg" != "null" && -n "$error_msg" ]]; then
                    print_info "Error: $error_msg"
                fi
            else
                print_info "Response: $(echo "$body" | head -200)"
            fi
            
            return 1
            ;;
        500)
            print_fail "Registration failed - Server Error"
            print_warning "Internal server error (check server logs)"
            return 1
            ;;
        000)
            print_fail "Registration failed - Connection timeout"
            print_warning "Cannot reach the API server"
            return 1
            ;;
        *)
            print_fail "Registration failed - HTTP $status_code"
            print_info "Response: $(echo "$body" | head -200)"
            return 1
            ;;
    esac
}

# Function to test login after registration
test_login() {
    local username="$1"
    local password="$2"
    
    print_step "Testing login for user: $username"
    
    local login_payload=$(cat <<EOF
{
    "username": "$username",
    "password": "$password"
}
EOF
)
    
    local response=$(curl -s -w '\n%{http_code}' -X POST \
        -H "Content-Type: application/json" \
        -d "$login_payload" \
        "$AUTH_ENDPOINT/login" \
        --max-time 30)
    
    local body=$(echo "$response" | sed '$d')
    local status_code=$(echo "$response" | tail -n1)
    
    if [[ "$status_code" == "200" ]]; then
        print_success "Login test successful!"
        
        if command -v jq &> /dev/null; then
            local token=$(echo "$body" | jq -r '.data.token // .token // .' 2>/dev/null)
            if [[ "$token" != "null" && -n "$token" && ${#token} -gt 20 ]]; then
                print_info "JWT Token received ($(echo "$token" | head -c 20)...)"
                echo "$token" > "/tmp/api_token_$username.txt"
                print_info "Token saved to: /tmp/api_token_$username.txt"
            fi
        fi
        
        return 0
    else
        print_fail "Login test failed - HTTP $status_code"
        print_info "Response: $(echo "$body" | head -200)"
        return 1
    fi
}

# Function for interactive registration
interactive_registration() {
    print_header "🎯 Interactive User Registration"
    
    echo "Please provide the following information:"
    echo ""
    
    # Get username
    while true; do
        echo -n "Username (3-50 chars, letters/numbers/underscores only): "
        read -r username
        
        if validate_username "$username"; then
            break
        else
            print_warning "$(validate_username "$username" 2>&1)"
        fi
    done
    
    # Get email
    while true; do
        echo -n "Email address: "
        read -r email
        
        if validate_email "$email"; then
            break
        else
            print_warning "Please enter a valid email address"
        fi
    done
    
    # Get password
    while true; do
        echo -n "Password (min 8 chars, must include: uppercase, lowercase, digit, special char): "
        read -s password
        echo ""
        
        validation_result=$(validate_password "$password")
        if [[ $? -eq 0 ]]; then
            echo -n "Confirm password: "
            read -s password_confirm
            echo ""
            
            if [[ "$password" == "$password_confirm" ]]; then
                break
            else
                print_warning "Passwords do not match. Please try again."
            fi
        else
            print_warning "$validation_result"
        fi
    done
    
    # Get full name
    while true; do
        echo -n "Full name (2-100 characters): "
        read -r full_name
        
        if [[ ${#full_name} -ge 2 && ${#full_name} -le 100 ]]; then
            break
        else
            print_warning "Full name must be between 2 and 100 characters"
        fi
    done
    
    # Get role
    echo ""
    echo "Available roles:"
    echo "1. Employee (roleId: 1)"
    echo "2. ProjectManager (roleId: 2)" 
    echo "3. Administrator (roleId: 3)"
    echo ""
    
    while true; do
        echo -n "Select role (1-3): "
        read -r role_choice
        
        case "$role_choice" in
            1)
                role_id=1
                role_name="Employee"
                break
                ;;
            2)
                role_id=2
                role_name="ProjectManager"
                break
                ;;
            3)
                role_id=3
                role_name="Administrator"
                break
                ;;
            *)
                print_warning "Please select 1, 2, or 3"
                ;;
        esac
    done
    
    # Summary
    echo ""
    print_header "📋 Registration Summary"
    echo "Username: $username"
    echo "Email: $email"
    echo "Full Name: $full_name"
    echo "Role: $role_name (ID: $role_id)"
    echo ""
    
    echo -n "Proceed with registration? (y/N): "
    read -r confirm
    
    if [[ "$confirm" =~ ^[Yy]$ ]]; then
        echo ""
        register_user "$username" "$email" "$password" "$full_name" "$role_id"
        
        if [[ $? -eq 0 ]]; then
            echo ""
            echo -n "Test login with new credentials? (y/N): "
            read -r test_confirm
            
            if [[ "$test_confirm" =~ ^[Yy]$ ]]; then
                echo ""
                test_login "$username" "$password"
            fi
        fi
    else
        print_info "Registration cancelled"
    fi
}

# Function for batch registration with predefined users
batch_registration() {
    print_header "👥 Batch User Registration"
    
    print_info "Creating test users for production API..."
    echo ""
    
    # Define test users
    declare -a users=(
        "testuser1:test1@solarprojapi.com:TestPass123!:Test User One:1"
        "testuser2:test2@solarprojapi.com:TestPass123!:Test User Two:1"
        "manager1:manager1@solarprojapi.com:ManagerPass123!:Project Manager One:2"
        "admin1:admin1@solarprojapi.com:AdminPass123!:Administrator One:3"
    )
    
    local successful=0
    local failed=0
    
    for user_data in "${users[@]}"; do
        IFS=':' read -r username email password full_name role_id <<< "$user_data"
        
        echo ""
        register_user "$username" "$email" "$password" "$full_name" "$role_id"
        
        if [[ $? -eq 0 ]]; then
            ((successful++))
        else
            ((failed++))
        fi
        
        sleep 1  # Small delay between registrations
    done
    
    echo ""
    print_header "📊 Batch Registration Summary"
    echo "Successful registrations: $successful"
    echo "Failed registrations: $failed"
    
    if [[ $successful -gt 0 ]]; then
        echo ""
        print_info "Test credentials created:"
        echo "• testuser1 / TestPass123! (Employee)"
        echo "• testuser2 / TestPass123! (Employee)"
        echo "• manager1 / ManagerPass123! (ProjectManager)"
        echo "• admin1 / AdminPass123! (Administrator)"
    fi
}

# Function to test API availability
test_api_availability() {
    print_step "Testing API availability..."
    
    local health_response=$(curl -s -w '%{http_code}' -o /dev/null "$PROD_URL/health" --max-time 10)
    
    if [[ "$health_response" == "200" ]]; then
        print_success "API is available and healthy"
        return 0
    else
        print_fail "API is not available (HTTP $health_response)"
        print_warning "Cannot proceed with registration"
        return 1
    fi
}

# Function to show help
show_help() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Register users in the Solar Projects API production environment"
    echo ""
    echo "Options:"
    echo "  -i, --interactive     Interactive registration mode (default)"
    echo "  -b, --batch          Batch registration of predefined test users"
    echo "  -u, --username       Specify username for direct registration"
    echo "  -e, --email          Specify email for direct registration"
    echo "  -p, --password       Specify password for direct registration"
    echo "  -n, --name           Specify full name for direct registration"
    echo "  -r, --role           Specify role ID (1=Employee, 2=ProjectManager, 3=Administrator)"
    echo "  -t, --test-login     Test login after successful registration"
    echo "  -h, --help           Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                   # Interactive registration"
    echo "  $0 --batch           # Batch register test users"
    echo "  $0 -u john -e john@example.com -p SecurePass123! -n \"John Doe\" -r 1"
    echo ""
    echo "Password Requirements:"
    echo "  • At least 8 characters"
    echo "  • At least one uppercase letter"
    echo "  • At least one lowercase letter"
    echo "  • At least one digit"
    echo "  • At least one special character (@\$!%*?&)"
}

# Main execution
main() {
    echo -e "${BLUE}🌐 Production API: $PROD_URL${NC}"
    echo -e "${BLUE}⏰ Started: $(date)${NC}"
    echo ""
    
    # Test API availability first
    if ! test_api_availability; then
        exit 1
    fi
    
    # Check dependencies
    if ! command -v curl &> /dev/null; then
        print_fail "curl is required but not installed"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        print_warning "jq not found - JSON parsing will be limited"
    fi
    
    # Parse command line arguments
    local mode="interactive"
    local username=""
    local email=""
    local password=""
    local full_name=""
    local role_id=""
    local test_login_flag=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_help
                exit 0
                ;;
            -i|--interactive)
                mode="interactive"
                shift
                ;;
            -b|--batch)
                mode="batch"
                shift
                ;;
            -u|--username)
                username="$2"
                shift 2
                ;;
            -e|--email)
                email="$2"
                shift 2
                ;;
            -p|--password)
                password="$2"
                shift 2
                ;;
            -n|--name)
                full_name="$2"
                shift 2
                ;;
            -r|--role)
                role_id="$2"
                shift 2
                ;;
            -t|--test-login)
                test_login_flag=true
                shift
                ;;
            *)
                print_warning "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    # Execute based on mode
    case "$mode" in
        "interactive")
            interactive_registration
            ;;
        "batch")
            batch_registration
            ;;
        *)
            # Direct registration mode
            if [[ -n "$username" && -n "$email" && -n "$password" && -n "$full_name" && -n "$role_id" ]]; then
                echo ""
                register_user "$username" "$email" "$password" "$full_name" "$role_id"
                
                if [[ $? -eq 0 && "$test_login_flag" == true ]]; then
                    echo ""
                    test_login "$username" "$password"
                fi
            else
                print_warning "For direct registration, all parameters are required: -u -e -p -n -r"
                echo ""
                show_help
                exit 1
            fi
            ;;
    esac
}

# Check if script is being run directly
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
