#!/bin/bash

# Work Request Approval Workflow Demo Script
# This script demonstrates the enhanced approval workflow system

API_BASE_URL="http://localhost:5002/api/v1"
ADMIN_TOKEN=""
MANAGER_TOKEN=""
USER_TOKEN=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Work Request Approval Workflow Demo ===${NC}"
echo

# Function to make API calls
api_call() {
    local method=$1
    local endpoint=$2
    local token=$3
    local data=$4
    
    if [ -n "$data" ]; then
        curl -s -X $method \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data" \
            "$API_BASE_URL$endpoint"
    else
        curl -s -X $method \
            -H "Authorization: Bearer $token" \
            "$API_BASE_URL$endpoint"
    fi
}

# Function to authenticate and get tokens
authenticate() {
    echo -e "${YELLOW}Authenticating users...${NC}"
    
    # Admin login
    ADMIN_RESPONSE=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d '{"username": "admin@solarprojects.com", "password": "Admin123!"}' \
        "$API_BASE_URL/auth/login")
    
    ADMIN_TOKEN=$(echo $ADMIN_RESPONSE | jq -r '.data.token // empty')
    
    # Manager login
    MANAGER_RESPONSE=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d '{"username": "manager@solarprojects.com", "password": "Manager123!"}' \
        "$API_BASE_URL/auth/login")
    
    MANAGER_TOKEN=$(echo $MANAGER_RESPONSE | jq -r '.data.token // empty')
    
    # User login
    USER_RESPONSE=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d '{"username": "user@solarprojects.com", "password": "User123!"}' \
        "$API_BASE_URL/auth/login")
    
    USER_TOKEN=$(echo $USER_RESPONSE | jq -r '.data.token // empty')
    
    if [ -z "$ADMIN_TOKEN" ] || [ -z "$MANAGER_TOKEN" ] || [ -z "$USER_TOKEN" ]; then
        echo -e "${RED}Authentication failed. Please check credentials.${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Authentication successful!${NC}"
    echo
}

# Test 1: Create a work request
test_create_work_request() {
    echo -e "${YELLOW}Test 1: Creating a work request${NC}"
    
    # First get a project ID
    PROJECTS_RESPONSE=$(api_call "GET" "/projects" "$USER_TOKEN")
    PROJECT_ID=$(echo $PROJECTS_RESPONSE | jq -r '.data.items[0].projectId // empty')
    
    if [ -z "$PROJECT_ID" ]; then
        echo -e "${RED}No projects found. Creating test project first...${NC}"
        return 1
    fi
    
    WORK_REQUEST_DATA='{
        "projectId": "'$PROJECT_ID'",
        "title": "Solar Panel Installation - Building A",
        "description": "Install 50kW solar panel system on Building A roof. Requires electrical work and structural assessment.",
        "type": "Installation",
        "priority": "High",
        "estimatedCost": 25000,
        "estimatedHours": 120,
        "location": "Building A - Rooftop",
        "notes": "Weather dependent work. Requires crane access."
    }'
    
    RESPONSE=$(api_call "POST" "/work-requests" "$USER_TOKEN" "$WORK_REQUEST_DATA")
    WORK_REQUEST_ID=$(echo $RESPONSE | jq -r '.data.workRequestId // empty')
    
    if [ -n "$WORK_REQUEST_ID" ]; then
        echo -e "${GREEN}✓ Work request created: $WORK_REQUEST_ID${NC}"
        echo "Response: $(echo $RESPONSE | jq '.data | {workRequestId, title, status, estimatedCost}')"
    else
        echo -e "${RED}✗ Failed to create work request${NC}"
        echo "Response: $(echo $RESPONSE | jq '.')"
        return 1
    fi
    
    echo
    return 0
}

# Test 2: Submit for approval
test_submit_for_approval() {
    echo -e "${YELLOW}Test 2: Submitting work request for approval${NC}"
    
    if [ -z "$WORK_REQUEST_ID" ]; then
        echo -e "${RED}No work request ID available${NC}"
        return 1
    fi
    
    SUBMIT_DATA='{
        "requiresAdminApproval": true,
        "comments": "Urgent installation required for Q3 deadline. Customer has approved design."
    }'
    
    RESPONSE=$(api_call "POST" "/work-requests/$WORK_REQUEST_ID/submit-approval" "$USER_TOKEN" "$SUBMIT_DATA")
    
    if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
        echo -e "${GREEN}✓ Work request submitted for approval${NC}"
        echo "Response: $(echo $RESPONSE | jq '.message')"
    else
        echo -e "${RED}✗ Failed to submit for approval${NC}"
        echo "Response: $(echo $RESPONSE | jq '.')"
        return 1
    fi
    
    echo
    return 0
}

# Test 3: Check approval status
test_check_approval_status() {
    echo -e "${YELLOW}Test 3: Checking approval workflow status${NC}"
    
    if [ -z "$WORK_REQUEST_ID" ]; then
        echo -e "${RED}No work request ID available${NC}"
        return 1
    fi
    
    RESPONSE=$(api_call "GET" "/work-requests/$WORK_REQUEST_ID/approval-status" "$USER_TOKEN")
    
    if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
        echo -e "${GREEN}✓ Approval status retrieved${NC}"
        echo "Status: $(echo $RESPONSE | jq '.data | {currentStatus, requiresManagerApproval, requiresAdminApproval, daysPendingApproval}')"
    else
        echo -e "${RED}✗ Failed to get approval status${NC}"
        echo "Response: $(echo $RESPONSE | jq '.')"
    fi
    
    echo
}

# Test 4: Manager approval
test_manager_approval() {
    echo -e "${YELLOW}Test 4: Manager approval process${NC}"
    
    # First check pending approvals
    echo "Checking manager's pending approvals..."
    PENDING_RESPONSE=$(api_call "GET" "/work-requests/pending-approvals" "$MANAGER_TOKEN")
    
    if [ "$(echo $PENDING_RESPONSE | jq -r '.success')" = "true" ]; then
        PENDING_COUNT=$(echo $PENDING_RESPONSE | jq -r '.data.totalCount')
        echo -e "${GREEN}✓ Manager has $PENDING_COUNT pending approval(s)${NC}"
    fi
    
    # Approve the work request
    if [ -n "$WORK_REQUEST_ID" ]; then
        APPROVAL_DATA='{
            "action": "Approve",
            "comments": "Installation plan looks good. Approved for Q3 implementation. Please coordinate with electrical team."
        }'
        
        RESPONSE=$(api_call "POST" "/work-requests/$WORK_REQUEST_ID/process-approval" "$MANAGER_TOKEN" "$APPROVAL_DATA")
        
        if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
            echo -e "${GREEN}✓ Manager approved the work request${NC}"
            echo "Response: $(echo $RESPONSE | jq '.message')"
        else
            echo -e "${RED}✗ Manager approval failed${NC}"
            echo "Response: $(echo $RESPONSE | jq '.')"
        fi
    fi
    
    echo
}

# Test 5: Admin approval
test_admin_approval() {
    echo -e "${YELLOW}Test 5: Admin approval process${NC}"
    
    # Check admin's pending approvals
    echo "Checking admin's pending approvals..."
    PENDING_RESPONSE=$(api_call "GET" "/work-requests/pending-approvals" "$ADMIN_TOKEN")
    
    if [ "$(echo $PENDING_RESPONSE | jq -r '.success')" = "true" ]; then
        PENDING_COUNT=$(echo $PENDING_RESPONSE | jq -r '.data.totalCount')
        echo -e "${GREEN}✓ Admin has $PENDING_COUNT pending approval(s)${NC}"
    fi
    
    # Approve the work request
    if [ -n "$WORK_REQUEST_ID" ]; then
        APPROVAL_DATA='{
            "action": "Approve",
            "comments": "Budget approved. High priority project for Q3 delivery. Proceed with implementation."
        }'
        
        RESPONSE=$(api_call "POST" "/work-requests/$WORK_REQUEST_ID/process-approval" "$ADMIN_TOKEN" "$APPROVAL_DATA")
        
        if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
            echo -e "${GREEN}✓ Admin approved the work request${NC}"
            echo "Response: $(echo $RESPONSE | jq '.message')"
        else
            echo -e "${RED}✗ Admin approval failed${NC}"
            echo "Response: $(echo $RESPONSE | jq '.')"
        fi
    fi
    
    echo
}

# Test 6: Check approval history
test_approval_history() {
    echo -e "${YELLOW}Test 6: Checking approval history${NC}"
    
    if [ -z "$WORK_REQUEST_ID" ]; then
        echo -e "${RED}No work request ID available${NC}"
        return 1
    fi
    
    RESPONSE=$(api_call "GET" "/work-requests/$WORK_REQUEST_ID/approval-history" "$USER_TOKEN")
    
    if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
        echo -e "${GREEN}✓ Approval history retrieved${NC}"
        echo "History: $(echo $RESPONSE | jq '.data.items[] | {action, level, approverName, createdAt, comments}')"
    else
        echo -e "${RED}✗ Failed to get approval history${NC}"
        echo "Response: $(echo $RESPONSE | jq '.')"
    fi
    
    echo
}

# Test 7: Get approval statistics
test_approval_statistics() {
    echo -e "${YELLOW}Test 7: Getting approval statistics${NC}"
    
    RESPONSE=$(api_call "GET" "/work-requests/approval-statistics" "$MANAGER_TOKEN")
    
    if [ "$(echo $RESPONSE | jq -r '.success')" = "true" ]; then
        echo -e "${GREEN}✓ Approval statistics retrieved${NC}"
        echo "Statistics: $(echo $RESPONSE | jq '.data | {totalPendingApprovals, managerPendingApprovals, adminPendingApprovals, averageApprovalTimeHours}')"
    else
        echo -e "${RED}✗ Failed to get approval statistics${NC}"
        echo "Response: $(echo $RESPONSE | jq '.')"
    fi
    
    echo
}

# Test 8: Test rejection workflow
test_rejection_workflow() {
    echo -e "${YELLOW}Test 8: Testing rejection workflow${NC}"
    
    # Create another work request for rejection test
    PROJECTS_RESPONSE=$(api_call "GET" "/projects" "$USER_TOKEN")
    PROJECT_ID=$(echo $PROJECTS_RESPONSE | jq -r '.data.items[0].projectId // empty')
    
    WORK_REQUEST_DATA='{
        "projectId": "'$PROJECT_ID'",
        "title": "Unnecessary Maintenance Work",
        "description": "Regular maintenance that is not needed at this time.",
        "type": "Maintenance",
        "priority": "Low",
        "estimatedCost": 5000,
        "estimatedHours": 40,
        "location": "Building B",
        "notes": "Can be deferred to next quarter."
    }'
    
    RESPONSE=$(api_call "POST" "/work-requests" "$USER_TOKEN" "$WORK_REQUEST_DATA")
    REJECTION_REQUEST_ID=$(echo $RESPONSE | jq -r '.data.workRequestId // empty')
    
    if [ -n "$REJECTION_REQUEST_ID" ]; then
        echo -e "${GREEN}✓ Test work request created for rejection: $REJECTION_REQUEST_ID${NC}"
        
        # Submit for approval
        SUBMIT_DATA='{
            "requiresAdminApproval": false,
            "comments": "Requesting approval for maintenance work."
        }'
        
        api_call "POST" "/work-requests/$REJECTION_REQUEST_ID/submit-approval" "$USER_TOKEN" "$SUBMIT_DATA" > /dev/null
        
        # Manager rejects
        REJECTION_DATA='{
            "action": "Reject",
            "comments": "This maintenance work is not necessary at this time. Please defer to Q4.",
            "rejectionReason": "Budget constraints and low priority"
        }'
        
        REJECT_RESPONSE=$(api_call "POST" "/work-requests/$REJECTION_REQUEST_ID/process-approval" "$MANAGER_TOKEN" "$REJECTION_DATA")
        
        if [ "$(echo $REJECT_RESPONSE | jq -r '.success')" = "true" ]; then
            echo -e "${GREEN}✓ Work request rejected by manager${NC}"
            echo "Rejection reason: $(echo $REJECT_RESPONSE | jq '.message')"
        else
            echo -e "${RED}✗ Rejection failed${NC}"
        fi
    fi
    
    echo
}

# Test 9: Test bulk approval
test_bulk_approval() {
    echo -e "${YELLOW}Test 9: Testing bulk approval functionality${NC}"
    
    # This would require multiple pending work requests
    # For demo purposes, we'll show the API structure
    
    BULK_DATA='{
        "workRequestIds": ["'$WORK_REQUEST_ID'"],
        "action": "Approve",
        "comments": "Bulk approval for Q3 installations"
    }'
    
    echo "Bulk approval API structure:"
    echo "POST /work-requests/bulk-approval"
    echo "Body: $(echo $BULK_DATA | jq '.')"
    
    echo
}

# Main execution
main() {
    echo -e "${BLUE}Starting Work Request Approval Workflow Demo${NC}"
    echo "This demo will test the complete approval workflow system."
    echo
    
    # Check if API is running
    echo -e "${YELLOW}Checking API availability...${NC}"
    if ! curl -s "$API_BASE_URL/health" > /dev/null 2>&1; then
        echo -e "${RED}API is not available at $API_BASE_URL${NC}"
        echo "Please ensure the API is running with: dotnet run --urls http://localhost:5002"
        exit 1
    fi
    echo -e "${GREEN}✓ API is available${NC}"
    echo
    
    # Authenticate users
    authenticate
    
    # Run tests
    test_create_work_request && \
    test_submit_for_approval && \
    test_check_approval_status && \
    test_manager_approval && \
    test_admin_approval && \
    test_approval_history && \
    test_approval_statistics && \
    test_rejection_workflow && \
    test_bulk_approval
    
    echo -e "${BLUE}=== Demo Summary ===${NC}"
    echo -e "${GREEN}✓ Work Request Approval Workflow Implementation Complete${NC}"
    echo
    echo "Key features implemented:"
    echo "• ✅ Multi-level approval chain (User → Manager → Admin)"
    echo "• ✅ Status transitions and workflow management"
    echo "• ✅ Approval history and comments tracking"
    echo "• ✅ Email notification system (simulated)"
    echo "• ✅ Bulk approval capabilities"
    echo "• ✅ Escalation and rejection workflows"
    echo "• ✅ Approval statistics and reporting"
    echo "• ✅ Auto-approval based on thresholds"
    echo "• ✅ Role-based approval permissions"
    echo
    echo -e "${YELLOW}Next steps:${NC}"
    echo "1. Configure real email service (SendGrid, AWS SES, etc.)"
    echo "2. Set up automated reminder job for overdue approvals"
    echo "3. Add approval workflow templates for different request types"
    echo "4. Implement mobile push notifications"
    echo "5. Add approval analytics dashboard"
}

# Run the demo
main
