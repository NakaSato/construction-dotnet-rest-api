# 🔗 Project-Task Relationship in Solar Projects API

## Overview

The Solar Projects API implements a **One-to-Many relationship** between Projects and Tasks, where:
- **One Project** can have **many Tasks**
- **Each Task** belongs to **exactly one Project**

## 📊 Database Relationship Diagram

```
┌─────────────────────────────────────┐
│               PROJECT               │
├─────────────────────────────────────┤
│ + ProjectId (PK, Guid)              │
│ + ProjectName (string)              │
│ + Address (string)                  │
│ + ClientInfo (string)               │
│ + Status (ProjectStatus)            │
│ + StartDate (DateTime)              │
│ + EstimatedEndDate (DateTime?)      │
│ + ProjectManagerId (FK, Guid)       │
│ + CreatedAt (DateTime)              │
│ + Team (string?)                    │
│ + ConnectionType (string?)          │
│ + TotalCapacityKw (decimal?)        │
│ + PvModuleCount (int?)              │
│ ...                                 │
└─────────────────────────────────────┘
               │ 1
               │
               │ Has Many
               │
               │ M
┌─────────────────────────────────────┐
│            PROJECT_TASK             │
├─────────────────────────────────────┤
│ + TaskId (PK, Guid)                 │
│ + ProjectId (FK, Guid) ←────────────│
│ + PhaseId (FK, Guid?)               │
│ + Title (string)                    │
│ + Description (string)              │
│ + Status (TaskStatus)               │
│ + Priority (TaskPriority)           │
│ + DueDate (DateTime?)               │
│ + AssignedTechnicianId (FK, Guid?)  │
│ + CompletionDate (DateTime?)        │
│ + EstimatedHours (decimal)          │
│ + ActualHours (decimal)             │
│ + CompletionPercentage (decimal)    │
│ + WeightInPhase (decimal)           │
│ + Dependencies (string?)            │
│ + CreatedAt (DateTime)              │
└─────────────────────────────────────┘
```

## 🏗️ Entity Framework Relationship

### In Project.cs
```csharp
public class Project
{
    [Key]
    public Guid ProjectId { get; set; }
    
    // ... other properties ...
    
    // Navigation property - One Project has Many Tasks
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
```

### In ProjectTask.cs
```csharp
public class ProjectTask
{
    [Key]
    public Guid TaskId { get; set; }
    
    // Foreign Key - Each Task belongs to One Project
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    // ... other properties ...
    
    // Navigation property - Each Task belongs to One Project
    public virtual Project Project { get; set; } = null!;
}
```

## 🌐 API Relationship Implementation

### 1. **Creating Tasks for Projects**

Tasks are created with a **required Project ID**:

```http
POST /api/v1/tasks?projectId={project-guid}
Content-Type: application/json

{
    "title": "Install Solar Panels",
    "description": "Install 50 solar panels on roof section A",
    "dueDate": "2025-08-15T10:00:00Z",
    "assignedTechnicianId": "user-guid-here"
}
```

### 2. **Task Response includes Project Information**

```json
{
    "taskId": "task-guid",
    "projectId": "project-guid",
    "projectName": "PWA-107: สำนักงานประปาเขต 9",
    "title": "Install Solar Panels",
    "description": "Install 50 solar panels on roof section A",
    "status": "NotStarted",
    "dueDate": "2025-08-15T10:00:00Z",
    "assignedTechnician": null,
    "completionDate": null,
    "createdAt": "2025-07-04T13:34:39.192778Z"
}
```

### 3. **Filtering Tasks by Project**

```http
GET /api/v1/tasks?projectId={project-guid}
```

## 📋 Business Logic Relationship

### **Project Level** (High-level solar installation project)
- **Example:** "PWA-107: สำนักงานประปาเขต 9"
- **Scope:** Complete solar installation for a water treatment facility
- **Duration:** Weeks to months
- **Includes:** Site preparation, equipment installation, testing, commissioning

### **Task Level** (Specific work items within a project)
- **Examples:**
  - "Site Survey and Measurement"
  - "Install Solar Panel Mounting System"
  - "Connect Electrical Wiring"
  - "Install Inverters"
  - "System Testing and Commissioning"
- **Scope:** Specific actionable work items
- **Duration:** Hours to days
- **Assignable:** To specific technicians

## 🔄 Workflow Hierarchy

```
🏭 SOLAR PROJECT
├── 📋 Task 1: Site Survey
├── 📋 Task 2: Equipment Delivery
├── 📋 Task 3: Install Mounting System
├── 📋 Task 4: Install Solar Panels
├── 📋 Task 5: Electrical Connections
├── 📋 Task 6: Install Inverters
├── 📋 Task 7: System Testing
└── 📋 Task 8: Final Commissioning
```

## 🎯 Key Relationship Features

### **1. Mandatory Association**
- ✅ Every Task **must** belong to a Project
- ✅ Tasks cannot exist without a parent Project
- ✅ ProjectId is required when creating tasks

### **2. Project Context in Tasks**
- ✅ Task responses include `projectId` and `projectName`
- ✅ Tasks inherit project context (location, client, etc.)
- ✅ Tasks can be filtered by project

### **3. Project Progress Tracking**
- ✅ Project completion depends on task completion
- ✅ Tasks have completion percentages and status
- ✅ Project status reflects overall task progress

### **4. Access Control Inheritance**
- ✅ Task permissions often inherit from project permissions
- ✅ Project managers can manage all project tasks
- ✅ Technicians can only work on assigned tasks

## 🔧 API Usage Examples with Scripts

### **Create a Task for a Project**
```bash
# Get a project ID first
PROJECT_ID="1c91433d-fc43-40d9-a49b-e9f2d2a9e922"

# Create a task
./scripts/quick-tasks.sh create $PROJECT_ID
```

### **List All Tasks for a Project**
```bash
./scripts/quick-tasks.sh list-project $PROJECT_ID
```

### **View Task with Project Context**
```bash
./scripts/quick-tasks.sh view task-id-here
# Returns task info including projectId and projectName
```

## 🎯 Practical Relationship Examples

### **Real-World Solar Project: "PWA-107: สำนักงานประปาเขต 9"**

**Project Details:**
- Location: สำนักงานประปาเขต 9
- Capacity: 125.4 kW
- Panels: 220 units
- Client: กปภ.เขต 9

**Associated Tasks:**
1. **Site Preparation Task**
   - Title: "Prepare roof surface for panel installation"
   - Assigned to: Site preparation team
   - Duration: 2 days

2. **Panel Installation Task**
   - Title: "Install 220 solar panels"
   - Assigned to: Installation technicians
   - Duration: 5 days

3. **Electrical Connection Task**
   - Title: "Connect DC wiring and install inverters"
   - Assigned to: Electrical technicians
   - Duration: 3 days

4. **Testing & Commissioning Task**
   - Title: "System testing and grid connection"
   - Assigned to: Testing specialists
   - Duration: 2 days

## 📊 Database Query Examples

### **Get All Tasks for a Project**
```sql
SELECT t.*, p.ProjectName 
FROM ProjectTask t
JOIN Project p ON t.ProjectId = p.ProjectId
WHERE t.ProjectId = @projectId
```

### **Get Project with Task Count**
```sql
SELECT p.*, COUNT(t.TaskId) as TaskCount
FROM Project p
LEFT JOIN ProjectTask t ON p.ProjectId = t.ProjectId
GROUP BY p.ProjectId
```

### **Get Project Progress Based on Tasks**
```sql
SELECT p.ProjectName,
       COUNT(t.TaskId) as TotalTasks,
       COUNT(CASE WHEN t.Status = 'Completed' THEN 1 END) as CompletedTasks,
       AVG(t.CompletionPercentage) as OverallProgress
FROM Project p
LEFT JOIN ProjectTask t ON p.ProjectId = t.ProjectId
GROUP BY p.ProjectId, p.ProjectName
```

---

## 🎯 Summary

The **Project-Task relationship** in the Solar Projects API is a fundamental **One-to-Many** relationship that enables:

1. **Hierarchical Work Organization** - Breaking down large solar projects into manageable tasks
2. **Resource Assignment** - Assigning specific technicians to specific tasks
3. **Progress Tracking** - Monitoring project completion through task completion
4. **Access Control** - Managing permissions at both project and task levels
5. **Reporting** - Generating reports on project progress, resource utilization, and timelines

This relationship structure supports the complete lifecycle of solar installation projects, from initial planning through final commissioning, with detailed tracking and management capabilities at both the strategic (project) and operational (task) levels.
