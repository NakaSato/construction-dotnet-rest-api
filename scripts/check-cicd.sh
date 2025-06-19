#!/bin/bash

# CI/CD Health Check Script
echo "🔍 CI/CD Pipeline Health Check"
echo "=============================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Track issues
ISSUES_FOUND=0

echo -e "${BLUE}📋 Checking workflow files...${NC}"

# Check if workflow files exist
WORKFLOWS=(
    ".github/workflows/main-pipeline.yml"
    ".github/workflows/ci.yml"
    ".github/workflows/ci-simple.yml"
    ".github/workflows/deploy.yml"
    ".github/workflows/codeql.yml"
)

for workflow in "${WORKFLOWS[@]}"; do
    if [ -f "$workflow" ]; then
        echo -e "  ✅ $workflow"
    else
        echo -e "  ❌ $workflow ${RED}(missing)${NC}"
        ((ISSUES_FOUND++))
    fi
done

echo ""
echo -e "${BLUE}🔧 Checking .NET configuration...${NC}"

# Check .NET version consistency
DOTNET_VERSION=$(dotnet --version)
echo -e "  📦 Local .NET version: ${GREEN}$DOTNET_VERSION${NC}"

# Check workflow .NET versions
echo -e "  📋 Workflow .NET versions:"
grep -r "DOTNET_VERSION" .github/workflows/ | sed 's/.*DOTNET_VERSION: /  - /' | sort -u

echo ""
echo -e "${BLUE}🐳 Checking Docker configuration...${NC}"

# Check Dockerfile
if [ -f "Dockerfile" ]; then
    echo -e "  ✅ Dockerfile exists"
    
    # Check if Dockerfile uses correct .NET version
    DOCKERFILE_DOTNET=$(grep -o "mcr.microsoft.com/dotnet/.*:.*" Dockerfile | head -1)
    if [ ! -z "$DOCKERFILE_DOTNET" ]; then
        echo -e "  📦 Dockerfile .NET image: ${GREEN}$DOCKERFILE_DOTNET${NC}"
    fi
else
    echo -e "  ❌ Dockerfile ${RED}(missing)${NC}"
    ((ISSUES_FOUND++))
fi

echo ""
echo -e "${BLUE}🗄️ Checking database configuration...${NC}"

# Check for Entity Framework
if command -v dotnet ef &> /dev/null; then
    echo -e "  ✅ EF Core tools installed"
else
    echo -e "  ⚠️  EF Core tools not installed ${YELLOW}(install with: dotnet tool install --global dotnet-ef)${NC}"
fi

# Check migrations
if [ -d "Migrations" ]; then
    MIGRATION_COUNT=$(find Migrations -name "*.cs" -not -name "*.Designer.cs" | wc -l)
    echo -e "  📊 Migrations found: ${GREEN}$MIGRATION_COUNT${NC}"
else
    echo -e "  ⚠️  No Migrations directory found"
fi

echo ""
echo -e "${BLUE}🧪 Checking test configuration...${NC}"

# Check for test projects
TEST_PROJECTS=$(find . -name "*.Tests.csproj" -o -name "*Test*.csproj" | wc -l)
if [ $TEST_PROJECTS -gt 0 ]; then
    echo -e "  ✅ Test projects found: ${GREEN}$TEST_PROJECTS${NC}"
else
    echo -e "  ⚠️  No test projects found ${YELLOW}(consider adding unit tests)${NC}"
fi

echo ""
echo -e "${BLUE}🔐 Checking security configuration...${NC}"

# Check for secrets documentation
if [ -f "README.md" ]; then
    if grep -q "secrets\|AZURE_CREDENTIALS" README.md; then
        echo -e "  ✅ Secrets documentation found in README"
    else
        echo -e "  ⚠️  No secrets documentation in README"
    fi
fi

# Check for environment-specific appsettings
APPSETTINGS_COUNT=$(find . -name "appsettings.*.json" | wc -l)
echo -e "  📁 Environment configs: ${GREEN}$APPSETTINGS_COUNT${NC}"

echo ""
echo -e "${BLUE}📝 Checking project files...${NC}"

# Check project file
if [ -f "dotnet-rest-api.csproj" ]; then
    echo -e "  ✅ Project file exists"
    
    # Check target framework
    TARGET_FRAMEWORK=$(grep -o "<TargetFramework>.*</TargetFramework>" dotnet-rest-api.csproj | sed 's/<[^>]*>//g')
    if [ ! -z "$TARGET_FRAMEWORK" ]; then
        echo -e "  🎯 Target framework: ${GREEN}$TARGET_FRAMEWORK${NC}"
    fi
else
    echo -e "  ❌ Project file missing"
    ((ISSUES_FOUND++))
fi

echo ""
echo -e "${BLUE}🚀 Checking build status...${NC}"

# Try to build the project
echo "  🔨 Testing build..."
if dotnet build --configuration Release --verbosity quiet > /dev/null 2>&1; then
    echo -e "  ✅ Build successful"
else
    echo -e "  ❌ Build failed ${RED}(run 'dotnet build' for details)${NC}"
    ((ISSUES_FOUND++))
fi

echo ""
echo "=============================="

if [ $ISSUES_FOUND -eq 0 ]; then
    echo -e "${GREEN}🎉 CI/CD pipeline is healthy!${NC}"
    echo "   All checks passed successfully."
else
    echo -e "${RED}⚠️  Found $ISSUES_FOUND issue(s) in CI/CD configuration${NC}"
    echo "   Please review the issues above."
fi

echo ""
echo -e "${BLUE}💡 Recommendations:${NC}"
echo "   1. Test workflows locally with 'act' tool"
echo "   2. Set up required GitHub secrets (AZURE_CREDENTIALS, etc.)"
echo "   3. Monitor workflow runs in GitHub Actions tab"
echo "   4. Consider adding integration tests"
echo "   5. Set up branch protection rules"

exit $ISSUES_FOUND
