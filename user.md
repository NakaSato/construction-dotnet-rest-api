Username	Email	Password	Role
test_admin	test_admin@example.com	Admin123!	Admin
test_manager	test_manager@example.com	Manager123!	Manager
test_user	test_user@example.com	User123!	User
test_viewer	test_viewer@example.com	Viewer123!	Viewer


# Production Database Connection Examples (REPLACE WITH YOUR ACTUAL VALUES)
# Format: postgresql://username:password@host:port/database?sslmode=require
# Example: postgresql://myuser:mypass@myhost.com:25060/mydb?sslmode=require

# .NET Connection String Format:
# CONNECTIONSTRINGS__DEFAULT="Host=your-host;Port=25060;Database=your-db;Username=your-user;Password=your-pass;SslMode=Require"


{
  "username": "admin",
  "email": "admin@example.com",
  "password": "Admin123!",
  "fullName": "TEST",
  "roleId": 1
}


/api/v1/projects (CreateProjectRequest)
ProjectName (string, required)
Address (string, required)
ClientInfo (string, optional)
StartDate (string, required, ISO 8601)
EstimatedEndDate (string, optional, ISO 8601)
ProjectManagerId (GUID, required)
Team (string, optional)
ConnectionType (string, optional)
ConnectionNotes (string, optional)
TotalCapacityKw (decimal, optional)
PvModuleCount (int, optional)
EquipmentDetails (object, optional: see below)
Inverter125kw (int)
Inverter80kw (int)
Inverter60kw (int)
Inverter40kw (int)
/api/v1/users (CreateUserRequest)
Username (string, required)
Email (string, required)
Password (string, required)
FullName (string, required)
RoleId (int, required)
/api/v1/tasks (CreateTaskRequest)
Title (string, required)
Description (string, optional)
DueDate (string, optional, ISO 8601)
AssignedTechnicianId (GUID, optional)
/api/v1/daily-reports (CreateDailyReportRequest)
ProjectId (GUID, required)
ReportDate (string, required, ISO 8601)
GeneralNotes (string, optional)
WeatherCondition (string, optional)
Temperature (double, optional)
TemperatureHigh (double, optional)
/api/v1/work-requests (CreateWorkRequestRequest)
ProjectId (GUID, required)
Title (string, required)
Description (string, required)
Type (string, required, e.g. "Other")
Priority (string, required, e.g. "Medium")
/api/v1/master-plans (CreateMasterPlanRequest)
ProjectId (GUID, required)
Name (string, required)
Description (string, optional)
PlannedStartDate (string, required, ISO 8601)
/api/v1/calendar (CalendarEvent)
Title (string, required)
StartDateTime (string, required, ISO 8601)
EndDateTime (string, required, ISO 8601)
EventType (int, required)
Status (int, required)
Priority (int, required)
Location (string, optional)
IsAllDay (bool, optional)