# 🗄️ Database Schema Migration Guide for Real-Time Features

**📅 Version**: 2.0 → 2.1  
**🔄 Migration Date**: January 15, 2025  
**🎯 Purpose**: Add comprehensive real-time notification and SignalR support  

## 🚀 Migration Overview

This migration adds comprehensive real-time features to the Solar Project Management database schema, including SignalR support, geographic intelligence for Thailand water authority projects, and enhanced project tracking capabilities.

## 📋 Pre-Migration Checklist

- [ ] **Backup Database**: Create full backup before running migration
- [ ] **Stop Application**: Ensure no active connections during migration
- [ ] **Check Disk Space**: Verify sufficient space for new tables and indexes
- [ ] **Review Dependencies**: Ensure PostgreSQL version supports all features
- [ ] **Test Environment**: Run migration on test environment first

## 🔧 Migration Steps

### Step 1: Enhanced Projects Table

```sql
-- Add new columns to existing Projects table
ALTER TABLE "Projects" 
ADD COLUMN "LocationAccuracy" DECIMAL(8,2),
ADD COLUMN "LocationUpdatedAt" TIMESTAMP WITH TIME ZONE,
ADD COLUMN "LocationUpdatedBy" VARCHAR(255),
ADD COLUMN "CompletionPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK ("CompletionPercentage" BETWEEN 0 AND 100),
ADD COLUMN "LastStatusChange" TIMESTAMP WITH TIME ZONE,
ADD COLUMN "LastLocationUpdate" TIMESTAMP WITH TIME ZONE,
ADD COLUMN "LastActivityAt" TIMESTAMP WITH TIME ZONE,
ADD COLUMN "RealTimeEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN "Version" INTEGER NOT NULL DEFAULT 1,
ADD COLUMN "Province" VARCHAR(100),
ADD COLUMN "Region" VARCHAR(50),
ADD COLUMN "FacilityType" VARCHAR(100),
ADD COLUMN "WaterAuthority" VARCHAR(255);

-- Set default values for existing equipment columns
UPDATE "Projects" SET "Inverter125kw" = 0 WHERE "Inverter125kw" IS NULL;
UPDATE "Projects" SET "Inverter80kw" = 0 WHERE "Inverter80kw" IS NULL;
UPDATE "Projects" SET "Inverter60kw" = 0 WHERE "Inverter60kw" IS NULL;
UPDATE "Projects" SET "Inverter40kw" = 0 WHERE "Inverter40kw" IS NULL;

ALTER TABLE "Projects" 
ALTER COLUMN "Inverter125kw" SET DEFAULT 0,
ALTER COLUMN "Inverter80kw" SET DEFAULT 0,
ALTER COLUMN "Inverter60kw" SET DEFAULT 0,
ALTER COLUMN "Inverter40kw" SET DEFAULT 0;

-- Make ProjectManagerId nullable for development flexibility
ALTER TABLE "Projects" 
DROP CONSTRAINT "FK_Projects_Users_ProjectManagerId",
ADD CONSTRAINT "FK_Projects_Users_ProjectManagerId" 
    FOREIGN KEY ("ProjectManagerId") REFERENCES "Users" ("UserId") ON DELETE SET NULL;

-- Add new foreign key for CreatedById
ALTER TABLE "Projects" 
ADD CONSTRAINT "FK_Projects_Users_CreatedById" 
    FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL;
```

### Step 2: Real-Time Notification Tables

```sql
-- Create SignalR and real-time notification tables
CREATE TABLE "UserConnections" (
    "ConnectionId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "SignalRConnectionId" VARCHAR(255) NOT NULL,
    "UserAgent" VARCHAR(500),
    "IpAddress" VARCHAR(45),
    "ConnectedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastHeartbeat" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisconnectedAt" TIMESTAMP WITH TIME ZONE,
    "SessionData" JSONB,
    
    CONSTRAINT "FK_UserConnections_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "RealTimeNotifications" (
    "NotificationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "GroupName" VARCHAR(255),
    "EventType" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Message" VARCHAR(2000) NOT NULL,
    "Data" JSONB NOT NULL DEFAULT '{}',
    "Priority" INTEGER NOT NULL DEFAULT 1,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "CreatedBy" VARCHAR(255),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "SentAt" TIMESTAMP WITH TIME ZONE,
    "DeliveredAt" TIMESTAMP WITH TIME ZONE,
    "ReadAt" TIMESTAMP WITH TIME ZONE,
    "ErrorMessage" VARCHAR(1000),
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "MaxRetries" INTEGER NOT NULL DEFAULT 3,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_RealTimeNotifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "SignalRGroups" (
    "GroupId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "GroupName" VARCHAR(255) NOT NULL,
    "GroupType" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "EntityId" UUID,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE TABLE "SignalRGroupMemberships" (
    "MembershipId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "GroupId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ConnectionId" UUID,
    "JoinedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LeftAt" TIMESTAMP WITH TIME ZONE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "MembershipData" JSONB,
    
    CONSTRAINT "FK_SignalRGroupMemberships_SignalRGroups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "SignalRGroups" ("GroupId") ON DELETE CASCADE,
    CONSTRAINT "FK_SignalRGroupMemberships_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_SignalRGroupMemberships_UserConnections_ConnectionId" FOREIGN KEY ("ConnectionId") REFERENCES "UserConnections" ("ConnectionId") ON DELETE SET NULL
);
```

### Step 3: Project Tracking Tables

```sql
-- Create enhanced project tracking tables
CREATE TABLE "ProjectStatusHistory" (
    "StatusHistoryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "OldStatus" INTEGER NOT NULL,
    "NewStatus" INTEGER NOT NULL,
    "ChangedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ChangedByUserId" UUID,
    "ChangedByUserName" VARCHAR(255),
    "ChangeReason" VARCHAR(1000),
    "CompletionPercentage" DECIMAL(5,2),
    "ActualEndDate" TIMESTAMP WITH TIME ZONE,
    "EstimatedEndDate" TIMESTAMP WITH TIME ZONE,
    "Notes" VARCHAR(2000),
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotificationRecipients" JSONB NOT NULL DEFAULT '[]',
    
    CONSTRAINT "FK_ProjectStatusHistory_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectStatusHistory_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

CREATE TABLE "ProjectLocationHistory" (
    "LocationHistoryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "OldLatitude" DECIMAL(10,7),
    "OldLongitude" DECIMAL(10,7),
    "NewLatitude" DECIMAL(10,7),
    "NewLongitude" DECIMAL(10,7),
    "OldAddress" VARCHAR(500),
    "NewAddress" VARCHAR(500),
    "ChangedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ChangedByUserId" UUID,
    "ChangedByUserName" VARCHAR(255),
    "ChangeMethod" VARCHAR(100) NOT NULL,
    "AccuracyMeters" DECIMAL(8,2),
    "DataSource" VARCHAR(255),
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "GeofenceAlerts" JSONB NOT NULL DEFAULT '[]',
    
    CONSTRAINT "FK_ProjectLocationHistory_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectLocationHistory_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

CREATE TABLE "ProjectActivityFeed" (
    "ActivityId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "ActivityType" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "PerformedByUserId" UUID,
    "PerformedByUserName" VARCHAR(255),
    "ActivityData" JSONB NOT NULL DEFAULT '{}',
    "Severity" INTEGER NOT NULL DEFAULT 1,
    "IsVisible" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "AggregatedCount" INTEGER NOT NULL DEFAULT 1,
    "LastAggregatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_ProjectActivityFeed_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectActivityFeed_Users_PerformedByUserId" FOREIGN KEY ("PerformedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);
```

### Step 4: Geographic Intelligence Tables

```sql
-- Create Thailand-specific geographic tables
CREATE TABLE "ProjectRegions" (
    "RegionId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RegionName" VARCHAR(100) NOT NULL,
    "RegionDisplayName" VARCHAR(255) NOT NULL,
    "Provinces" JSONB NOT NULL DEFAULT '[]',
    "BoundingBox" JSONB,
    "Description" VARCHAR(500),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE "WaterFacilityTypes" (
    "FacilityTypeId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "FacilityTypeName" VARCHAR(100) NOT NULL,
    "FacilityDisplayName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(500),
    "Authority" VARCHAR(255),
    "SignalRGroupName" VARCHAR(255),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE "ProjectRegionAssignments" (
    "AssignmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "RegionId" UUID NOT NULL,
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "AssignmentMethod" VARCHAR(100) NOT NULL,
    "Confidence" DECIMAL(3,2),
    "Notes" VARCHAR(500),
    
    CONSTRAINT "FK_ProjectRegionAssignments_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectRegionAssignments_ProjectRegions_RegionId" FOREIGN KEY ("RegionId") REFERENCES "ProjectRegions" ("RegionId") ON DELETE CASCADE
);

CREATE TABLE "ProjectFacilityAssignments" (
    "AssignmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "FacilityTypeId" UUID NOT NULL,
    "FacilityName" VARCHAR(255),
    "FacilityCode" VARCHAR(100),
    "Authority" VARCHAR(255),
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "AssignmentMethod" VARCHAR(100) NOT NULL,
    "Confidence" DECIMAL(3,2),
    "VerifiedAt" TIMESTAMP WITH TIME ZONE,
    "VerifiedByUserId" UUID,
    "Notes" VARCHAR(500),
    
    CONSTRAINT "FK_ProjectFacilityAssignments_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectFacilityAssignments_WaterFacilityTypes_FacilityTypeId" FOREIGN KEY ("FacilityTypeId") REFERENCES "WaterFacilityTypes" ("FacilityTypeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectFacilityAssignments_Users_VerifiedByUserId" FOREIGN KEY ("VerifiedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);
```

### Step 5: Performance and Monitoring Tables

```sql
-- Create performance optimization and monitoring tables
CREATE TABLE "DashboardStatsCache" (
    "CacheId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CacheKey" VARCHAR(255) NOT NULL,
    "CacheType" VARCHAR(100) NOT NULL,
    "EntityId" UUID,
    "Statistics" JSONB NOT NULL DEFAULT '{}',
    "LastCalculated" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Version" INTEGER NOT NULL DEFAULT 1,
    "IsValid" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE "RealTimeEventsLog" (
    "EventLogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "EventType" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "UserId" UUID,
    "UserName" VARCHAR(255),
    "EventData" JSONB NOT NULL DEFAULT '{}',
    "Recipients" JSONB NOT NULL DEFAULT '[]',
    "BroadcastGroups" JSONB NOT NULL DEFAULT '[]',
    "DeliveryStatus" INTEGER NOT NULL DEFAULT 0,
    "ProcessingTimeMs" INTEGER,
    "ErrorDetails" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ProcessedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_RealTimeEventsLog_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);
```

### Step 6: Enhanced Indexes

```sql
-- Add enhanced indexes for real-time performance
CREATE INDEX "IX_Projects_Region" ON "Projects" ("Region");
CREATE INDEX "IX_Projects_Province" ON "Projects" ("Province");
CREATE INDEX "IX_Projects_FacilityType" ON "Projects" ("FacilityType");
CREATE INDEX "IX_Projects_LastActivityAt" ON "Projects" ("LastActivityAt");
CREATE INDEX "IX_Projects_CompletionPercentage" ON "Projects" ("CompletionPercentage");
CREATE INDEX "IX_Projects_Location" ON "Projects" ("Latitude", "Longitude");
CREATE INDEX "IX_Projects_RealTimeEnabled" ON "Projects" ("RealTimeEnabled");

-- Real-time notification indexes
CREATE INDEX "IX_UserConnections_UserId" ON "UserConnections" ("UserId");
CREATE INDEX "IX_UserConnections_SignalRConnectionId" ON "UserConnections" ("SignalRConnectionId");
CREATE INDEX "IX_UserConnections_IsActive" ON "UserConnections" ("IsActive");
CREATE INDEX "IX_UserConnections_LastHeartbeat" ON "UserConnections" ("LastHeartbeat");

CREATE INDEX "IX_RealTimeNotifications_UserId" ON "RealTimeNotifications" ("UserId");
CREATE INDEX "IX_RealTimeNotifications_Status" ON "RealTimeNotifications" ("Status");
CREATE INDEX "IX_RealTimeNotifications_EventType" ON "RealTimeNotifications" ("EventType");
CREATE INDEX "IX_RealTimeNotifications_EntityType_EntityId" ON "RealTimeNotifications" ("EntityType", "EntityId");
CREATE INDEX "IX_RealTimeNotifications_CreatedAt" ON "RealTimeNotifications" ("CreatedAt");
CREATE INDEX "IX_RealTimeNotifications_Priority" ON "RealTimeNotifications" ("Priority");

-- Additional indexes for tracking tables
CREATE INDEX "IX_ProjectStatusHistory_ProjectId" ON "ProjectStatusHistory" ("ProjectId");
CREATE INDEX "IX_ProjectStatusHistory_ChangedAt" ON "ProjectStatusHistory" ("ChangedAt");
CREATE INDEX "IX_ProjectLocationHistory_ProjectId" ON "ProjectLocationHistory" ("ProjectId");
CREATE INDEX "IX_ProjectActivityFeed_ProjectId" ON "ProjectActivityFeed" ("ProjectId");
CREATE INDEX "IX_ProjectActivityFeed_CreatedAt" ON "ProjectActivityFeed" ("CreatedAt");
```

### Step 7: Seed Data for Real-Time Features

```sql
-- Insert Thailand geographic regions
INSERT INTO "ProjectRegions" ("RegionId", "RegionName", "RegionDisplayName", "Provinces", "Description", "IsActive") VALUES 
('11111111-1111-1111-1111-111111111111', 'northern', 'Northern Thailand', '["เชียงใหม่", "เชียงราย", "ลำปาง", "ลำพูน", "พะเยา", "แพร่", "น่าน"]', 'Northern region of Thailand', TRUE),
('22222222-2222-2222-2222-222222222222', 'western', 'Western Thailand', '["ตาก"]', 'Western region of Thailand', TRUE),
('33333333-3333-3333-3333-333333333333', 'central', 'Central Thailand', '["กรุงเทพมหานคร", "พิจิตร", "พิษณุโลก", "สุโขทัย", "อุตรดิตถ์"]', 'Central region including Bangkok', TRUE),
('44444444-4444-4444-4444-444444444444', 'unknown', 'Unknown Region', '[]', 'Default region for unidentified locations', TRUE);

-- Insert water facility types
INSERT INTO "WaterFacilityTypes" ("FacilityTypeId", "FacilityTypeName", "FacilityDisplayName", "Description", "Authority", "SignalRGroupName", "IsActive") VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'water_treatment', 'Water Treatment Plant', 'Water treatment facilities', 'Provincial Waterworks Authority', 'facility_water_treatment', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'pumping_station', 'Water Pumping Station', 'Water pumping stations', 'Provincial Waterworks Authority', 'facility_pumping_station', TRUE),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'reservoir', 'Water Reservoir', 'Water storage facilities', 'Provincial Waterworks Authority', 'facility_reservoir', TRUE),
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'distribution_center', 'Distribution Center', 'Water distribution centers', 'Metropolitan Waterworks Authority', 'facility_distribution', TRUE),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'solar_installation', 'Solar Installation Site', 'Solar panel installation sites', 'Solar Project Management', 'facility_solar_installation', TRUE);

-- Insert default SignalR groups
INSERT INTO "SignalRGroups" ("GroupId", "GroupName", "GroupType", "Description", "IsActive") VALUES 
('f1111111-1111-1111-1111-111111111111', 'all_users', 'global', 'Global system announcements', TRUE),
('f2222222-2222-2222-2222-222222222222', 'role_admin', 'role', 'Administrative users', TRUE),
('f3333333-3333-3333-3333-333333333333', 'role_manager', 'role', 'Project managers', TRUE),
('f4444444-4444-4444-4444-444444444444', 'region_northern', 'region', 'Northern Thailand updates', TRUE),
('f5555555-5555-5555-5555-555555555555', 'region_western', 'region', 'Western Thailand updates', TRUE),
('f6666666-6666-6666-6666-666666666666', 'region_central', 'region', 'Central Thailand updates', TRUE),
('f7777777-7777-7777-7777-777777777777', 'facility_water_treatment', 'facility', 'Water treatment updates', TRUE),
('f8888888-8888-8888-8888-888888888888', 'facility_solar_installation', 'facility', 'Solar installation updates', TRUE),
('f9999999-9999-9999-9999-999999999999', 'map_viewers', 'special', 'Real-time map updates', TRUE);
```

## 🧪 Post-Migration Validation

### 1. Data Integrity Checks

```sql
-- Verify table creation
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN (
    'UserConnections', 'RealTimeNotifications', 'SignalRGroups', 
    'ProjectStatusHistory', 'ProjectLocationHistory', 'ProjectActivityFeed',
    'ProjectRegions', 'WaterFacilityTypes', 'DashboardStatsCache'
);

-- Check new columns in Projects table
SELECT column_name, data_type, is_nullable, column_default 
FROM information_schema.columns 
WHERE table_name = 'Projects' 
AND column_name IN (
    'CompletionPercentage', 'RealTimeEnabled', 'Region', 'Province', 
    'FacilityType', 'LocationAccuracy', 'Version'
);

-- Verify seed data
SELECT COUNT(*) as region_count FROM "ProjectRegions";
SELECT COUNT(*) as facility_count FROM "WaterFacilityTypes";
SELECT COUNT(*) as group_count FROM "SignalRGroups";
```

### 2. Index Verification

```sql
-- Check new indexes
SELECT indexname, tablename FROM pg_indexes 
WHERE tablename IN (
    'Projects', 'UserConnections', 'RealTimeNotifications', 
    'ProjectStatusHistory', 'ProjectLocationHistory'
) 
AND indexname LIKE 'IX_%'
ORDER BY tablename, indexname;
```

### 3. Constraint Verification

```sql
-- Check foreign key constraints
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
AND tc.table_name IN (
    'UserConnections', 'RealTimeNotifications', 'SignalRGroupMemberships',
    'ProjectStatusHistory', 'ProjectLocationHistory', 'ProjectActivityFeed'
);
```

## ✅ Migration Completion Checklist

- [ ] **All Tables Created**: Verify all new tables exist with correct structure
- [ ] **Indexes Applied**: Confirm all performance indexes are in place
- [ ] **Constraints Active**: Check all foreign key relationships work correctly
- [ ] **Seed Data Loaded**: Verify regional and facility data is properly inserted
- [ ] **Application Compatibility**: Test that existing application functionality works
- [ ] **Real-Time Features**: Verify SignalR integration works with new schema
- [ ] **Performance Test**: Run basic performance tests on enhanced queries
- [ ] **Backup Updated**: Create post-migration backup for rollback if needed

## 🔄 Rollback Plan

If issues occur, rollback steps:

1. **Stop Application**: Ensure no active connections
2. **Restore Backup**: Restore pre-migration database backup
3. **Verify Rollback**: Confirm original functionality works
4. **Investigate Issues**: Analyze migration problems
5. **Fix and Retry**: Address issues and re-run migration

## 📊 Performance Impact

Expected impact:
- **Storage**: +15-20% increase due to new tracking tables
- **Read Performance**: Improved with enhanced indexes
- **Write Performance**: Slight overhead for real-time features
- **Memory Usage**: +10-15% for SignalR connection tracking

## 🚀 Next Steps

After successful migration:
1. **Update Application**: Deploy updated application code with real-time features
2. **Configure SignalR**: Ensure SignalR hub is properly configured
3. **Test Real-Time**: Validate real-time notifications work correctly
4. **Monitor Performance**: Watch database performance with new features
5. **User Training**: Train users on new real-time collaboration features

---

**⚠️ Important**: Always test this migration on a development environment before running on production!
