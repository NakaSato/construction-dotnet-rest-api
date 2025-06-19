#!/bin/bash

# Project Cleanup Script
echo "🧹 Starting Project Cleanup..."
echo "=============================="

# Track what we're cleaning
DELETED_COUNT=0

# Function to safely remove files/directories
safe_remove() {
    if [ -e "$1" ]; then
        echo "🗑️  Removing: $1"
        rm -rf "$1"
        ((DELETED_COUNT++))
    fi
}

# 1. Remove API test result logs
echo ""
echo "📋 Cleaning up API test logs..."
for file in api-test-results-*.log; do
    if [ -f "$file" ]; then
        safe_remove "$file"
    fi
done

# 2. Remove Python fix scripts (temporary development tools)
echo ""
echo "🐍 Cleaning up temporary Python scripts..."
safe_remove "bulk_fix_controllers.py"
safe_remove "fix_controllers.py"
safe_remove "fix_conversion_errors.py"
safe_remove "fix_final_errors.py"
safe_remove "fix_malformed_constructors.py"
safe_remove "fix_remaining_syntax.py"
safe_remove "fix_return_type_issues.py"
safe_remove "fix_return_types.py"
safe_remove "fix_specific_errors.py"
safe_remove "fix_syntax_errors.py"
safe_remove "fix_to_api_response.py"
safe_remove "import_projects.py"
safe_remove "restore_work_requests.py"

# 3. Remove old test logs from test-results directory
echo ""
echo "📊 Cleaning up old test results..."
if [ -d "test-results" ]; then
    find test-results -name "*.log" -type f -exec rm -f {} \;
    echo "🗑️  Removed old test logs from test-results/"
    ((DELETED_COUNT++))
fi

# 4. Remove temporary/legacy files
echo ""
echo "📄 Cleaning up temporary and legacy files..."
safe_remove "phase 2.csv"
safe_remove "phase 2.xlsx"
safe_remove "project.json"
safe_remove "user.md"

# 5. Remove redundant test scripts (keep the main ones)
echo ""
echo "🧪 Cleaning up redundant test scripts..."
safe_remove "api-testing-summary.sh"
safe_remove "approval-workflow-demo.sh"
safe_remove "demo-approval-workflow.sh"
safe_remove "monitor-rate-limits.sh"
safe_remove "quick-api-status.sh"
safe_remove "test-delete-rate-limit.sh"
safe_remove "test-rate-limit-debug.sh"
safe_remove "test-rate-limit.sh"
safe_remove "test-solar-project.sh"
safe_remove "verify-docker-deployment.sh"

# 6. Clean up build artifacts and temporary directories
echo ""
echo "🔧 Cleaning up build artifacts..."
safe_remove "bin/"
safe_remove "obj/"

# 7. Remove Python virtual environment (if not needed)
echo ""
echo "🐍 Cleaning up Python virtual environment..."
safe_remove ".venv/"

# 8. Clean up temp directory
echo ""
echo "📁 Cleaning up temp directory..."
if [ -d "temp" ]; then
    rm -rf temp/*
    echo "🗑️  Cleaned temp directory contents"
    ((DELETED_COUNT++))
fi

# 9. Clean up uploads directory (optional - be careful!)
echo ""
read -p "🖼️  Do you want to clean the uploads directory? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if [ -d "uploads" ]; then
        rm -rf uploads/*
        echo "🗑️  Cleaned uploads directory contents"
        ((DELETED_COUNT++))
    fi
else
    echo "⏭️  Skipping uploads directory cleanup"
fi

echo ""
echo "✅ Cleanup Complete!"
echo "📊 Summary:"
echo "   - Files/directories removed: $DELETED_COUNT"
echo "   - Project structure preserved"
echo "   - Core functionality maintained"

echo ""
echo "🎯 Recommended next steps:"
echo "   1. Run 'dotnet clean' to clean build artifacts"
echo "   2. Run 'dotnet restore' to restore packages"
echo "   3. Run 'dotnet build' to verify everything still works"
echo "   4. Commit cleaned up project to git"
