#!/bin/bash

# Azure Cloud Shell Database Migration Script
# Run this script from Azure Cloud Shell (https://shell.azure.com) 

echo "🔧 Azure Database Migration Setup"
echo "=================================="

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
RESOURCE_GROUP="solar-projects-rg"
DB_SERVER="solar-projects-db"
DB_NAME="SolarProjectsDb"
DB_USER="dbadmin"
DB_PASSWORD="SolarDB123!"
CONNECTION_STRING="Host=${DB_SERVER}.postgres.database.azure.com;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Port=5432;SSL Mode=Require;"

echo -e "${BLUE}📊 Database Configuration:${NC}"
echo "Server: ${DB_SERVER}.postgres.database.azure.com"
echo "Database: ${DB_NAME}"
echo "User: ${DB_USER}"
echo ""

# Step 1: Verify Azure resources
echo -e "${BLUE}1. Verifying Azure Resources...${NC}"

echo "Checking resource group..."
if az group show --name $RESOURCE_GROUP > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Resource group exists${NC}"
else
    echo -e "${RED}❌ Resource group not found${NC}"
    exit 1
fi

echo "Checking PostgreSQL server..."
if az postgres flexible-server show --name $DB_SERVER --resource-group $RESOURCE_GROUP > /dev/null 2>&1; then
    echo -e "${GREEN}✅ PostgreSQL server exists and is accessible${NC}"
else
    echo -e "${RED}❌ PostgreSQL server not found or not accessible${NC}"
    exit 1
fi

echo "Checking database..."
if az postgres flexible-server db show --database-name $DB_NAME --server-name $DB_SERVER --resource-group $RESOURCE_GROUP > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Database exists${NC}"
else
    echo -e "${RED}❌ Database not found${NC}"
    exit 1
fi

# Step 2: Test database connectivity
echo ""
echo -e "${BLUE}2. Testing Database Connectivity...${NC}"

# Use psql to test connection
if command -v psql > /dev/null 2>&1; then
    echo "Testing connection with psql..."
    if PGPASSWORD="$DB_PASSWORD" psql -h "${DB_SERVER}.postgres.database.azure.com" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Database connection successful${NC}"
    else
        echo -e "${YELLOW}⚠️  psql connection failed, but this might be normal in Cloud Shell${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  psql not available, skipping direct connection test${NC}"
fi

# Step 3: Clone repository if needed
echo ""
echo -e "${BLUE}3. Setting Up Project...${NC}"

if [ ! -d "dotnet-rest-api" ]; then
    echo "Repository not found. Please clone your repository first:"
    echo ""
    echo -e "${YELLOW}git clone https://github.com/your-username/dotnet-rest-api.git${NC}"
    echo "cd dotnet-rest-api"
    echo "chmod +x scripts/azure-migrate.sh"
    echo "./scripts/azure-migrate.sh"
    echo ""
    echo "Or manually run the migration:"
    echo ""
    echo -e "${YELLOW}export ConnectionStrings__DefaultConnection=\"$CONNECTION_STRING\"${NC}"
    echo -e "${YELLOW}dotnet ef database update${NC}"
    exit 0
fi

cd dotnet-rest-api

# Step 4: Install .NET SDK if needed
echo ""
echo -e "${BLUE}4. Checking .NET SDK...${NC}"

if command -v dotnet > /dev/null 2>&1; then
    echo -e "${GREEN}✅ .NET SDK is available${NC}"
    dotnet --version
else
    echo -e "${YELLOW}⚠️  .NET SDK not found. Installing...${NC}"
    
    # Install .NET SDK in Azure Cloud Shell
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
    export PATH="$PATH:$HOME/.dotnet"
    
    if command -v dotnet > /dev/null 2>&1; then
        echo -e "${GREEN}✅ .NET SDK installed successfully${NC}"
    else
        echo -e "${RED}❌ Failed to install .NET SDK${NC}"
        exit 1
    fi
fi

# Step 5: Set connection string and run migrations
echo ""
echo -e "${BLUE}5. Running Database Migrations...${NC}"

export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"

echo "Connection string set. Running migrations..."

if dotnet ef database update; then
    echo ""
    echo -e "${GREEN}🎉 Database migrations completed successfully!${NC}"
    echo ""
    echo -e "${BLUE}Next steps:${NC}"
    echo "1. Test user registration:"
    echo "   curl -X POST -H \"Content-Type: application/json\" \\"
    echo "     -d '{\"username\":\"testuser\",\"email\":\"test@example.com\",\"password\":\"Password123!\",\"fullName\":\"Test User\",\"roleId\":1}' \\"
    echo "     https://solar-projects-api.azurewebsites.net/api/v1/auth/register"
    echo ""
    echo "2. Verify with your local scripts:"
    echo "   ./scripts/register-user.sh testuser test@example.com 'Password123!' 'Test User' 1"
    
else
    echo ""
    echo -e "${RED}❌ Database migration failed${NC}"
    echo ""
    echo -e "${YELLOW}Troubleshooting options:${NC}"
    echo "1. Check if Entity Framework tools are installed:"
    echo "   dotnet tool install --global dotnet-ef"
    echo ""
    echo "2. Verify connection string:"
    echo "   echo \$ConnectionStrings__DefaultConnection"
    echo ""
    echo "3. Test database connection manually:"
    echo "   PGPASSWORD=\"$DB_PASSWORD\" psql -h \"${DB_SERVER}.postgres.database.azure.com\" -U \"$DB_USER\" -d \"$DB_NAME\""
    echo ""
    echo "4. Check firewall rules:"
    echo "   az postgres flexible-server firewall-rule list --name $DB_SERVER --resource-group $RESOURCE_GROUP"
    
    exit 1
fi

echo ""
echo -e "${GREEN}✅ Migration script completed${NC}"
