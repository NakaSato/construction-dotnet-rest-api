#!/bin/bash

# Solar Projects Database Access Script
# Connects to PostgreSQL database running in Docker and displays project data

# Database connection details
CONTAINER_NAME="solar-projects-db"
DB_NAME="SolarProjectsDb"
DB_USER="postgres"

echo "=========================================="
echo "Solar Projects Database Access"
echo "=========================================="
echo ""

echo "1. Checking Docker container..."
echo "Container: $CONTAINER_NAME"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo ""

# Check if container is running
if ! docker ps | grep -q $CONTAINER_NAME; then
    echo "❌ Container '$CONTAINER_NAME' is not running."
    echo "Please start the container with: docker-compose up -d postgres"
    exit 1
fi

echo "✅ Container is running!"
echo ""

# Function to execute SQL query using docker exec
execute_sql() {
    local query="$1"
    local description="$2"
    echo "$description"
    echo "Executing: $query"
    echo "----------------------------------------"
    docker exec -it $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME -c "$query" 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "❌ Failed to execute query."
        return 1
    fi
    echo ""
    return 0
}

# Test database connection
echo "Testing database connection..."
if ! execute_sql "SELECT version();" "Database Version:"; then
    echo "❌ Cannot connect to database in container."
    exit 1
fi

echo "✅ Database connection successful!"
echo ""

# Show all tables
execute_sql "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;" "2. Available tables in the database:"

# Show Projects data
execute_sql "SELECT \"ProjectId\", \"ProjectName\", \"Address\", \"ClientInfo\", \"Status\", \"StartDate\", \"EstimatedEndDate\", \"CreatedAt\" FROM \"Projects\" ORDER BY \"CreatedAt\" DESC LIMIT 10;" "3. Projects data:"

# Show Project Tasks data
execute_sql "SELECT \"TaskId\", \"TaskName\", \"Description\", \"Status\", \"Priority\", \"ProjectId\", \"AssignedUserId\", \"CreatedAt\" FROM \"ProjectTasks\" ORDER BY \"CreatedAt\" DESC LIMIT 10;" "4. Project Tasks data:"

# Show Users data (without passwords)
execute_sql "SELECT \"UserId\", \"Username\", \"Email\", \"FullName\", \"RoleId\", \"IsActive\", \"CreatedAt\" FROM \"Users\" ORDER BY \"CreatedAt\" DESC LIMIT 10;" "5. Users data:"

# Show Daily Reports data
execute_sql "SELECT \"ReportId\", \"Date\", \"ProjectId\", \"UserId\", \"HoursWorked\", \"TasksCompleted\", \"CreatedAt\" FROM \"DailyReports\" ORDER BY \"Date\" DESC LIMIT 10;" "6. Daily Reports data:"

# Show Work Requests data
execute_sql "SELECT \"RequestId\", \"Title\", \"Description\", \"Status\", \"Priority\", \"ProjectId\", \"RequestedById\", \"CreatedAt\" FROM \"WorkRequests\" ORDER BY \"CreatedAt\" DESC LIMIT 10;" "7. Work Requests data:"

# Show Calendar Events data
execute_sql "SELECT \"EventId\", \"Title\", \"Description\", \"StartDateTime\", \"EndDateTime\", \"ProjectId\", \"UserId\", \"CreatedAt\" FROM \"CalendarEvents\" ORDER BY \"StartDateTime\" DESC LIMIT 10;" "8. Calendar Events data:"

# Show Roles data
execute_sql "SELECT \"RoleId\", \"RoleName\" FROM \"Roles\" ORDER BY \"RoleId\";" "9. Roles data:"

# Summary statistics
execute_sql "
SELECT 
    'Projects' as table_name, COUNT(*) as record_count FROM \"Projects\"
UNION ALL
SELECT 
    'ProjectTasks' as table_name, COUNT(*) as record_count FROM \"ProjectTasks\"
UNION ALL
SELECT 
    'Users' as table_name, COUNT(*) as record_count FROM \"Users\"
UNION ALL
SELECT 
    'DailyReports' as table_name, COUNT(*) as record_count FROM \"DailyReports\"
UNION ALL
SELECT 
    'WorkRequests' as table_name, COUNT(*) as record_count FROM \"WorkRequests\"
UNION ALL
SELECT 
    'CalendarEvents' as table_name, COUNT(*) as record_count FROM \"CalendarEvents\"
UNION ALL
SELECT 
    'Roles' as table_name, COUNT(*) as record_count FROM \"Roles\"
ORDER BY record_count DESC;
" "10. Database summary statistics:"

echo "=========================================="
echo "Database access complete!"
echo ""
echo "For more specific queries, you can use:"
echo "docker exec -it $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME"
echo ""
echo "Example custom queries:"
echo "SELECT * FROM \"Projects\" WHERE \"Status\" = 'Active';"
echo "SELECT p.\"ProjectName\", COUNT(t.\"TaskId\") as task_count FROM \"Projects\" p LEFT JOIN \"ProjectTasks\" t ON p.\"ProjectId\" = t.\"ProjectId\" GROUP BY p.\"ProjectId\", p.\"ProjectName\";"
echo "SELECT u.\"Username\", COUNT(dr.\"ReportId\") as report_count FROM \"Users\" u LEFT JOIN \"DailyReports\" dr ON u.\"UserId\" = dr.\"UserId\" GROUP BY u.\"UserId\", u.\"Username\";"
echo "=========================================="
