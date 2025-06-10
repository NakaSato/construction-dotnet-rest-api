#!/bin/bash

# =============================================================================
# Calendar API Test Script
# =============================================================================
# This script tests the calendar API endpoints
# Usage: ./test-calendar-api.sh
# =============================================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# API Configuration
API_BASE="http://localhost:5002"
CONTENT_TYPE="Content-Type: application/json"

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
RATE_LIMIT_DELAY=3  # Delay between requests to avoid rate limiting

# Global variables
AUTH_TOKEN=""
USER_ID=""
CALENDAR_EVENT_ID=""

# =============================================================================
# Utility Functions
# =============================================================================

print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
}

print_test() {
    echo -e "${BLUE}🧪 Testing: $1${NC}"
    ((TOTAL_TESTS++))
}

print_success() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
    ((PASSED_TESTS++))
}

print_error() {
    echo -e "${RED}❌ FAIL: $1${NC}"
    ((FAILED_TESTS++))
}

print_warning() {
    echo -e "${YELLOW}⚠️  WARN: $1${NC}"
}

print_info() {
    echo -e "${PURPLE}ℹ️  INFO: $1${NC}"
}

# Add delay to avoid rate limiting
rate_limit_delay() {
    if [ "${SKIP_DELAYS:-false}" != "true" ]; then
        sleep $RATE_LIMIT_DELAY
    fi
}

check_success() {
    local response="$1"
    local test_name="$2"
    
    if echo "$response" | jq -e '.success == true' > /dev/null 2>&1; then
        print_success "$test_name"
        return 0
    else
        print_error "$test_name"
        echo -e "${RED}Response: $response${NC}"
        return 1
    fi
}

# =============================================================================
# Authentication
# =============================================================================

authenticate() {
    print_header "AUTHENTICATION"
    
    print_test "Authenticate User"
    response=$(curl -s -X POST "$API_BASE/api/v1/auth/login" \
        -H "$CONTENT_TYPE" \
        -d '{
            "username": "john_doe",
            "password": "SecurePassword123!"
        }')
    
    if check_success "$response" "User authentication"; then
        AUTH_TOKEN=$(echo "$response" | jq -r '.data.token')
        USER_ID=$(echo "$response" | jq -r '.data.user.userId')
        print_info "Authentication successful. User ID: $USER_ID"
        return 0
    else
        print_error "Authentication failed. Cannot proceed with calendar tests."
        return 1
    fi
}

# =============================================================================
# Calendar Tests
# =============================================================================

test_calendar_api() {
    print_header "CALENDAR API TESTS"
    
    if [ -z "$AUTH_TOKEN" ]; then
        print_error "No authentication token available. Cannot run calendar tests."
        return
    fi
    
    # Test 1: Create a calendar event
    print_test "Create Calendar Event"
    rate_limit_delay
    response=$(curl -s -X POST "$API_BASE/api/v1/calendar" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "title": "Test Meeting",
            "description": "Test calendar event for API testing",
            "startDateTime": "2025-06-15T09:00:00.000Z",
            "endDateTime": "2025-06-15T10:30:00.000Z",
            "eventType": 1,
            "status": 1,
            "priority": 2,
            "location": "Conference Room A",
            "isAllDay": false,
            "isRecurring": false,
            "notes": "Test notes",
            "reminderMinutes": 15,
            "color": "#2196F3",
            "isPrivate": false,
            "attendees": "test@example.com"
        }')
    
    if check_success "$response" "Create calendar event"; then
        CALENDAR_EVENT_ID=$(echo "$response" | jq -r '.data.eventId')
        print_info "Created calendar event ID: $CALENDAR_EVENT_ID"
    fi
    
    # Test 2: Get all calendar events
    print_test "Get All Calendar Events"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get all calendar events"
    
    # Test 3: Get calendar event by ID
    if [ -n "$CALENDAR_EVENT_ID" ] && [ "$CALENDAR_EVENT_ID" != "null" ]; then
        print_test "Get Calendar Event by ID"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get calendar event by ID"
        
        # Test 4: Update calendar event
        print_test "Update Calendar Event"
        rate_limit_delay
        response=$(curl -s -X PUT "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "$CONTENT_TYPE" \
            -H "Authorization: Bearer $AUTH_TOKEN" \
            -d '{
                "title": "Updated Test Meeting",
                "description": "Updated test calendar event",
                "startDateTime": "2025-06-15T10:00:00.000Z",
                "endDateTime": "2025-06-15T11:30:00.000Z",
                "status": 2,
                "priority": 3,
                "location": "Conference Room B",
                "notes": "Updated test notes",
                "reminderMinutes": 30,
                "color": "#FF9800"
            }')
        check_success "$response" "Update calendar event"
    fi
    
    # Test 5: Get upcoming events
    print_test "Get Upcoming Events"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar/upcoming?days=30" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get upcoming events"
    
    # Test 6: Check event conflicts
    print_test "Check Event Conflicts"
    rate_limit_delay
    response=$(curl -s -X POST "$API_BASE/api/v1/calendar/conflicts" \
        -H "$CONTENT_TYPE" \
        -H "Authorization: Bearer $AUTH_TOKEN" \
        -d '{
            "startDateTime": "2025-06-15T09:30:00.000Z",
            "endDateTime": "2025-06-15T11:00:00.000Z",
            "userId": "'$USER_ID'"
        }')
    check_success "$response" "Check event conflicts"
    
    # Test 7: Filter by date range  
    print_test "Get Events by Date Range"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?startDate=2025-06-01T00:00:00.000Z&endDate=2025-06-30T23:59:59.000Z" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by date range"
    
    # Test 8: Filter by event type (using enum value)
    print_test "Get Events by Type"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?eventType=1" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by type"
    
    # Test 9: Filter by status (using enum value)
    print_test "Get Events by Status"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar?status=1" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get events by status"
    
    # Test 10: Get events by user
    if [ -n "$USER_ID" ] && [ "$USER_ID" != "null" ]; then
        print_test "Get Events by User"
        rate_limit_delay
        response=$(curl -s "$API_BASE/api/v1/calendar/user/$USER_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Get events by user"
    fi
    
    # Test 11: Get recurring events (placeholder)
    print_test "Get Recurring Events"
    rate_limit_delay
    response=$(curl -s "$API_BASE/api/v1/calendar/recurring" \
        -H "Authorization: Bearer $AUTH_TOKEN")
    check_success "$response" "Get recurring events"
    
    # Test 12: Clean up - delete the test event
    if [ -n "$CALENDAR_EVENT_ID" ] && [ "$CALENDAR_EVENT_ID" != "null" ]; then
        print_test "Delete Calendar Event"
        rate_limit_delay
        response=$(curl -s -X DELETE "$API_BASE/api/v1/calendar/$CALENDAR_EVENT_ID" \
            -H "Authorization: Bearer $AUTH_TOKEN")
        check_success "$response" "Delete calendar event"
    fi
}

# =============================================================================
# Main Test Execution
# =============================================================================

main() {
    print_header "CALENDAR API TEST SUITE"
    echo -e "${CYAN}Testing Calendar API endpoints...${NC}"
    echo -e "${CYAN}API Base URL: $API_BASE${NC}"
    
    # Check if API is running
    print_test "Check API Health"
    health_response=$(curl -s "$API_BASE/health" 2>/dev/null)
    if [ $? -eq 0 ]; then
        print_success "API is running"
    else
        print_error "API is not running. Please start the API first."
        exit 1
    fi
    
    # Authenticate first
    if authenticate; then
        test_calendar_api
    fi
    
    # Print summary
    print_header "TEST SUMMARY"
    echo -e "${CYAN}Total Tests: $TOTAL_TESTS${NC}"
    echo -e "${GREEN}Passed: $PASSED_TESTS${NC}"
    echo -e "${RED}Failed: $FAILED_TESTS${NC}"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 All calendar tests passed!${NC}"
        exit 0
    else
        echo -e "${RED}❌ Some calendar tests failed.${NC}"
        exit 1
    fi
}

# Run the main function
main "$@"
