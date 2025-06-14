#!/bin/bash

# Admin API Test Results Analyzer
# Analyzes the test results and provides a comprehensive summary

echo "🔍 ADMIN API TEST RESULTS ANALYSIS"
echo "=================================="

# Find the latest test log
LATEST_LOG=$(ls -t test-results/admin_test_*.log 2>/dev/null | head -1)

if [ -z "$LATEST_LOG" ]; then
    echo "❌ No test log files found. Please run ./test-admin-endpoints.sh first."
    exit 1
fi

echo "📁 Analyzing: $LATEST_LOG"
echo ""

# Extract test results
TOTAL_TESTS=$(grep -c "📡 Testing:" "$LATEST_LOG")
SUCCESS_TESTS=$(grep -c "✅ SUCCESS" "$LATEST_LOG")
WARNING_TESTS=$(grep -c "⚠️" "$LATEST_LOG")
ERROR_TESTS=$(grep -c "❌ ERROR" "$LATEST_LOG")

echo "📊 OVERALL STATISTICS"
echo "=================="
echo "Total API endpoints tested: $TOTAL_TESTS"
echo "✅ Successful: $SUCCESS_TESTS"
echo "⚠️  Warnings (4xx errors): $WARNING_TESTS"
echo "❌ Errors (5xx/network): $ERROR_TESTS"
echo ""

# Calculate success rate
if [ $TOTAL_TESTS -gt 0 ]; then
    SUCCESS_RATE=$((SUCCESS_TESTS * 100 / TOTAL_TESTS))
    echo "🎯 Success Rate: $SUCCESS_RATE%"
else
    echo "🎯 Success Rate: 0%"
fi
echo ""

# Working endpoints
echo "✅ WORKING ENDPOINTS"
echo "==================="
grep -B 2 "✅ SUCCESS" "$LATEST_LOG" | grep "📡 Testing:" | sed 's/📡 Testing: /✅ /' | sort
echo ""

# Endpoints with warnings
echo "⚠️  ENDPOINTS WITH WARNINGS"
echo "=========================="
grep -B 2 "⚠️" "$LATEST_LOG" | grep "📡 Testing:" | sed 's/📡 Testing: /⚠️  /' | sort
echo ""

# Failed endpoints
echo "❌ FAILED ENDPOINTS"
echo "=================="
grep -B 2 "❌ ERROR" "$LATEST_LOG" | grep "📡 Testing:" | sed 's/📡 Testing: /❌ /' | sort
echo ""

# Analyze specific issues
echo "🔍 ISSUE ANALYSIS"
echo "================"

# Check for rate limiting
RATE_LIMITED=$(grep -c "Status: 429" "$LATEST_LOG")
if [ $RATE_LIMITED -gt 0 ]; then
    echo "⚠️  Rate Limiting Detected: $RATE_LIMITED requests were rate limited"
    echo "   💡 Recommendation: Add delays between requests or increase rate limits"
fi

# Check for validation errors
VALIDATION_ERRORS=$(grep -c "Status: 400" "$LATEST_LOG")
if [ $VALIDATION_ERRORS -gt 0 ]; then
    echo "⚠️  Validation Errors: $VALIDATION_ERRORS requests had validation issues"
    echo "   💡 Recommendation: Check request body formats and required fields"
fi

# Check for missing endpoints
NOT_FOUND=$(grep -c "Status: 404" "$LATEST_LOG")
if [ $NOT_FOUND -gt 0 ]; then
    echo "⚠️  Missing Endpoints: $NOT_FOUND endpoints returned 404"
    echo "   💡 Recommendation: Verify endpoint implementations or URLs"
fi

echo ""

# Specific recommendations
echo "💡 RECOMMENDATIONS"
echo "=================="

# Check refresh token issue
if grep -q "refresh.*400" "$LATEST_LOG"; then
    echo "🔧 Fix refresh token endpoint:"
    echo "   - Check if endpoint expects string or object"
    echo "   - Current: {\"refreshToken\": \"token\"}"
    echo "   - Try: \"token_string\""
fi

# Check project creation issue
if grep -q "Create New Project.*400" "$LATEST_LOG"; then
    echo "🔧 Fix project creation:"
    echo "   - Add required 'Address' field to project data"
    echo "   - Address must be 5-50 characters"
fi

# Check image upload issue
if grep -q "Upload Test Image.*000" "$LATEST_LOG"; then
    echo "🔧 Fix image upload:"
    echo "   - Verify /api/v1/images/upload endpoint exists"
    echo "   - Check multipart/form-data handling"
fi

echo ""

# Admin-specific analysis
echo "🔐 ADMIN-SPECIFIC FEATURES"
echo "=========================="

# User management
if grep -q "Get All Users.*SUCCESS" "$LATEST_LOG"; then
    echo "✅ User management endpoint working"
else
    echo "❌ User management endpoint issues"
fi

# Full CRUD access
CRUD_SUCCESS=0
if grep -q "Get All Projects.*SUCCESS" "$LATEST_LOG"; then CRUD_SUCCESS=$((CRUD_SUCCESS + 1)); fi
if grep -q "Get All Tasks.*SUCCESS" "$LATEST_LOG"; then CRUD_SUCCESS=$((CRUD_SUCCESS + 1)); fi
if grep -q "Get All Daily Reports.*SUCCESS" "$LATEST_LOG"; then CRUD_SUCCESS=$((CRUD_SUCCESS + 1)); fi
if grep -q "Get All Work Requests.*SUCCESS" "$LATEST_LOG"; then CRUD_SUCCESS=$((CRUD_SUCCESS + 1)); fi

echo "✅ CRUD access verified for $CRUD_SUCCESS/4 main entities"

echo ""

echo "📋 NEXT STEPS"
echo "============="
echo "1. Fix validation issues in project creation and refresh token"
echo "2. Implement or verify image upload endpoint"
echo "3. Consider increasing rate limits for testing"
echo "4. Re-run tests after fixes with: ./test-admin-endpoints.sh"
echo ""

echo "📖 For detailed error messages, review: $LATEST_LOG"
