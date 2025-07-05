#!/bin/bash

# Real-Time Updates Testing Script
# Tests SignalR WebSocket connections and API real-time broadcasting

API_BASE="http://localhost:5001"
JWT_TOKEN=""

echo "🧪 Real-Time Updates Testing Script"
echo "=================================="
echo ""

# Function to check if server is running
check_server() {
    echo "🔍 Checking if API server is running..."
    if curl -s "$API_BASE/health" > /dev/null; then
        echo "✅ API server is running at $API_BASE"
        return 0
    else
        echo "❌ API server is not running at $API_BASE"
        echo "💡 Start the server with: dotnet run --urls \"http://localhost:5001\""
        return 1
    fi
}

# Function to test SignalR hub endpoint
test_signalr_hub() {
    echo ""
    echo "🔌 Testing SignalR hub endpoint..."
    
    RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE/notificationHub")
    
    if [ "$RESPONSE" = "404" ]; then
        echo "❌ SignalR hub not found at /notificationHub"
        echo "💡 Make sure SignalR is properly configured in Program.cs"
    elif [ "$RESPONSE" = "400" ] || [ "$RESPONSE" = "200" ]; then
        echo "✅ SignalR hub is accessible (HTTP $RESPONSE)"
    else
        echo "⚠️  SignalR hub returned HTTP $RESPONSE"
    fi
}

# Function to test API endpoint with real-time broadcasting
test_api_endpoint() {
    local endpoint=$1
    local method=$2
    local payload=$3
    local description=$4
    
    echo ""
    echo "🧪 Testing $description..."
    echo "   Endpoint: $method $endpoint"
    
    if [ -z "$JWT_TOKEN" ]; then
        echo "⚠️  No JWT token provided - testing without authentication"
        HEADERS="Content-Type: application/json"
    else
        HEADERS="Content-Type: application/json
Authorization: Bearer $JWT_TOKEN"
    fi
    
    RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" \
        -X "$method" \
        -H "$HEADERS" \
        -d "$payload" \
        "$API_BASE$endpoint")
    
    HTTP_STATUS=$(echo $RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    BODY=$(echo $RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')
    
    if [ "$HTTP_STATUS" -ge 200 ] && [ "$HTTP_STATUS" -lt 300 ]; then
        echo "✅ Success (HTTP $HTTP_STATUS)"
        echo "💡 This should have triggered real-time updates to connected SignalR clients"
    elif [ "$HTTP_STATUS" = "401" ]; then
        echo "🔒 Authentication required (HTTP $HTTP_STATUS)"
        echo "💡 Provide JWT token for authenticated testing"
    else
        echo "❌ Failed (HTTP $HTTP_STATUS)"
        echo "   Response: $BODY"
    fi
}

# Function to get JWT token (for authenticated testing)
get_jwt_token() {
    echo ""
    echo "🔐 Getting JWT token for authenticated testing..."
    
    LOGIN_PAYLOAD='{
        "username": "admin@example.com",
        "password": "Admin123!"
    }'
    
    RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" \
        -X POST \
        -H "Content-Type: application/json" \
        -d "$LOGIN_PAYLOAD" \
        "$API_BASE/api/v1/auth/login")
    
    HTTP_STATUS=$(echo $RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    BODY=$(echo $RESPONSE | sed -e 's/HTTPSTATUS\:.*//g')
    
    if [ "$HTTP_STATUS" = "200" ]; then
        JWT_TOKEN=$(echo $BODY | jq -r '.data.token' 2>/dev/null)
        if [ "$JWT_TOKEN" != "null" ] && [ -n "$JWT_TOKEN" ]; then
            echo "✅ JWT token obtained successfully"
            echo "🎫 Token: ${JWT_TOKEN:0:50}..."
        else
            echo "❌ Could not extract JWT token from response"
        fi
    else
        echo "❌ Login failed (HTTP $HTTP_STATUS)"
        echo "💡 Using default admin credentials - adjust if needed"
    fi
}

# Function to test all major endpoints
test_all_endpoints() {
    echo ""
    echo "🚀 Testing all API endpoints for real-time updates..."
    echo "======================================================"
    
    # Projects API
    PROJECT_PAYLOAD='{
        "projectName": "Real-Time Test Project",
        "address": "123 Test Street, Test City",
        "clientInfo": "Test Client for Real-Time Updates"
    }'
    test_api_endpoint "/api/v1/projects" "POST" "$PROJECT_PAYLOAD" "Projects API (Create)"
    
    # Tasks API
    TASK_PAYLOAD='{
        "title": "Real-Time Test Task",
        "description": "Testing real-time task updates",
        "priority": "Medium"
    }'
    test_api_endpoint "/api/v1/tasks" "POST" "$TASK_PAYLOAD" "Tasks API (Create)"
    
    # Daily Reports API
    REPORT_PAYLOAD='{
        "reportDate": "'$(date +%Y-%m-%d)'",
        "weatherConditions": "Sunny",
        "workCompleted": "Testing real-time updates",
        "hoursWorked": 8
    }'
    test_api_endpoint "/api/v1/dailyreports" "POST" "$REPORT_PAYLOAD" "Daily Reports API (Create)"
    
    # Work Requests API
    WORKREQUEST_PAYLOAD='{
        "title": "Real-Time Test Work Request",
        "description": "Testing real-time work request updates",
        "priority": "High"
    }'
    test_api_endpoint "/api/v1/workrequests" "POST" "$WORKREQUEST_PAYLOAD" "Work Requests API (Create)"
    
    # Notifications API (Test SignalR)
    NOTIFICATION_PAYLOAD='"Real-time test notification from automated script"'
    test_api_endpoint "/api/v1/notifications/test-signalr" "POST" "$NOTIFICATION_PAYLOAD" "Notifications API (Test SignalR)"
}

# Function to show interactive test dashboard
show_dashboard_info() {
    echo ""
    echo "🎨 Interactive Test Dashboard Available!"
    echo "========================================"
    echo "📁 Location: ./realtime-test-dashboard.html"
    echo "🌐 Usage: Open in browser for visual real-time testing"
    echo ""
    echo "Features:"
    echo "  ✅ Visual SignalR connection status"
    echo "  ✅ Live events feed with real-time updates"
    echo "  ✅ Interactive API testing buttons"
    echo "  ✅ Connection statistics and monitoring"
    echo "  ✅ Multi-tab testing for collaboration simulation"
    echo ""
    echo "💡 To use:"
    echo "   1. Open realtime-test-dashboard.html in your browser"
    echo "   2. Click 'Connect to SignalR'"
    echo "   3. Use test buttons or run this script to see live updates"
}

# Main execution
main() {
    # Check if server is running
    if ! check_server; then
        exit 1
    fi
    
    # Test SignalR hub
    test_signalr_hub
    
    # Show options
    echo ""
    echo "🎯 Testing Options:"
    echo "1. Quick test (no authentication)"
    echo "2. Full test with authentication"
    echo "3. Show interactive dashboard info"
    echo ""
    read -p "Choose option (1-3) or press Enter for option 2: " choice
    
    case $choice in
        1)
            echo "🏃 Running quick test without authentication..."
            test_all_endpoints
            ;;
        3)
            show_dashboard_info
            ;;
        *)
            echo "🔐 Running full test with authentication..."
            get_jwt_token
            test_all_endpoints
            ;;
    esac
    
    echo ""
    echo "✅ Testing completed!"
    echo ""
    echo "🔗 Next Steps:"
    echo "   1. Open realtime-test-dashboard.html in browser for visual testing"
    echo "   2. Open multiple browser tabs to test multi-user scenarios"
    echo "   3. Check the API logs for SignalR connection and broadcast messages"
    echo "   4. Monitor network tab in browser dev tools for WebSocket activity"
    echo ""
    echo "📚 Documentation: docs/api/00_REAL_TIME_LIVE_UPDATES.md"
}

# Run main function
main
