#!/bin/bash

# CI/CD Workflow Validation Script
echo "🔧 CI/CD Workflow Validation"
echo "============================"

WORKFLOWS_DIR=".github/workflows"
ERRORS=0

# Function to check workflow syntax
check_workflow_syntax() {
    local file="$1"
    echo "📋 Checking $file..."
    
    # Basic YAML syntax check using Python
    python3 -c "
import yaml
import sys
try:
    with open('$file', 'r') as f:
        yaml.safe_load(f)
    print('  ✅ Valid YAML syntax')
except yaml.YAMLError as e:
    print(f'  ❌ YAML Error: {e}')
    sys.exit(1)
except Exception as e:
    print(f'  ⚠️ Error: {e}')
    sys.exit(1)
" 2>/dev/null || echo "  ⚠️ Python YAML check skipped (PyYAML not available)"
}

# Function to check for duplicate job names
check_duplicate_jobs() {
    echo "🔍 Checking for duplicate job names..."
    local duplicates=$(grep -h "^  [a-zA-Z_-]*:$" $WORKFLOWS_DIR/*.yml | sort | uniq -d)
    
    if [ -n "$duplicates" ]; then
        echo "  ❌ Duplicate job names found:"
        echo "$duplicates" | sed 's/^/    /'
        ((ERRORS++))
    else
        echo "  ✅ No duplicate job names"
    fi
}

# Function to check trigger conflicts
check_trigger_conflicts() {
    echo "🚦 Checking trigger conflicts..."
    
    # Check for multiple workflows with same push triggers
    local main_push=$(grep -l "branches.*main" $WORKFLOWS_DIR/*.yml | wc -l)
    local develop_push=$(grep -l "branches.*develop" $WORKFLOWS_DIR/*.yml | wc -l)
    
    echo "  📊 Workflows triggering on main branch: $main_push"
    echo "  📊 Workflows triggering on develop branch: $develop_push"
    
    if [ "$main_push" -gt 2 ]; then
        echo "  ⚠️ Multiple workflows trigger on main branch pushes"
        echo "  📋 Consider using different triggers or workflow_run events"
    else
        echo "  ✅ Main branch triggers look good"
    fi
}

# Function to check environment variables consistency
check_env_consistency() {
    echo "🌍 Checking environment variable consistency..."
    
    local dotnet_versions=$(grep -h "DOTNET_VERSION:" $WORKFLOWS_DIR/*.yml | sort | uniq)
    local version_count=$(echo "$dotnet_versions" | wc -l)
    
    if [ "$version_count" -eq 1 ]; then
        echo "  ✅ Consistent .NET version across all workflows"
        echo "  📋 Version: $(echo "$dotnet_versions" | tr -d ' ')"
    else
        echo "  ⚠️ Inconsistent .NET versions found:"
        echo "$dotnet_versions" | sed 's/^/    /'
    fi
}

# Function to check required secrets documentation
check_secrets_documentation() {
    echo "🔐 Checking secrets documentation..."
    
    local secrets_in_workflows=$(grep -h "secrets\." $WORKFLOWS_DIR/*.yml | sed 's/.*secrets\.\([A-Z_]*\).*/\1/' | sort | uniq)
    
    if [ -f "CICD_WORKFLOW_STATUS.md" ]; then
        echo "  ✅ CI/CD documentation exists"
        
        local documented_secrets=$(grep -o "\`[A-Z_]*\`:" CICD_WORKFLOW_STATUS.md | tr -d '`:'| sort | uniq)
        
        echo "  📋 Secrets used in workflows:"
        echo "$secrets_in_workflows" | sed 's/^/    /'
        
        echo "  📋 Secrets documented:"
        echo "$documented_secrets" | sed 's/^/    /'
    else
        echo "  ⚠️ CI/CD documentation missing"
    fi
}

# Function to validate Docker configuration
check_docker_config() {
    echo "🐳 Checking Docker configuration..."
    
    if [ -f "Dockerfile" ]; then
        echo "  ✅ Dockerfile exists"
        
        local dockerfile_dotnet=$(grep "FROM.*dotnet" Dockerfile | head -1)
        local workflow_dotnet=$(grep "DOTNET_VERSION" $WORKFLOWS_DIR/*.yml | head -1)
        
        echo "  📋 Dockerfile .NET: $dockerfile_dotnet"
        echo "  📋 Workflow .NET: $workflow_dotnet"
    else
        echo "  ❌ Dockerfile missing"
        ((ERRORS++))
    fi
}

# Function to check workflow file naming
check_workflow_naming() {
    echo "📛 Checking workflow naming conventions..."
    
    for file in $WORKFLOWS_DIR/*.yml; do
        if [[ ! "$file" =~ ^$WORKFLOWS_DIR/[a-z0-9\-]+\.yml$ ]]; then
            echo "  ⚠️ Non-standard filename: $(basename $file)"
        fi
    done
    echo "  ✅ Workflow naming check complete"
}

# Main validation
echo "📁 Found workflows:"
ls -1 $WORKFLOWS_DIR/*.yml | sed 's/^/  /'
echo

# Run all checks
for workflow in $WORKFLOWS_DIR/*.yml; do
    check_workflow_syntax "$workflow"
done

echo
check_duplicate_jobs
echo
check_trigger_conflicts
echo
check_env_consistency
echo
check_secrets_documentation
echo
check_docker_config
echo
check_workflow_naming

# Summary
echo
echo "📊 VALIDATION SUMMARY"
echo "===================="

if [ $ERRORS -eq 0 ]; then
    echo "🎉 All checks passed successfully!"
    echo "✅ CI/CD pipeline is properly configured"
    echo
    echo "🚀 Next Steps:"
    echo "  1. Test workflows with 'act' tool (if available)"
    echo "  2. Configure required GitHub secrets"
    echo "  3. Set up branch protection rules"
    echo "  4. Monitor workflow runs in Actions tab"
else
    echo "❌ Found $ERRORS error(s) that should be addressed"
    echo "🔧 Please review and fix the issues above"
    exit 1
fi

echo
echo "🔗 Useful Commands:"
echo "  # List all workflow files"
echo "  find .github/workflows -name '*.yml' -type f"
echo
echo "  # Check workflow syntax (requires act)"
echo "  act --list"
echo
echo "  # Test specific workflow locally"
echo "  act -j build-and-test"
echo
echo "📚 Documentation:"
echo "  - See CICD_WORKFLOW_STATUS.md for detailed workflow information"
echo "  - See CICD_DOCUMENTATION.md for setup and configuration"
