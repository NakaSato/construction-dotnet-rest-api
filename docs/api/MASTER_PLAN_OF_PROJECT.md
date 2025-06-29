Architectural Blueprint for a Solar Project Management System: Business Logic and.NET Implementation
This report provides a comprehensive architectural design for a project management application tailored specifically to the "Solar Rooftop Power Generation Management Project 1." It translates the project's master schedule, implicit operational needs, and compliance requirements into a robust, scalable, and maintainable solution using the.NET ecosystem. We will deconstruct the project's core dynamics into formal business logic, define a multi-layered system architecture, and specify the technical implementation for the backend, frontend, and an integrated document management system.

Section 1: Deconstructing Project Dynamics into Business Logic
This section translates the abstract project plan into a concrete set of rules and calculations that will govern the software's behavior. We will define the algorithms for progress tracking and the logic for managing task interdependencies, forming the foundational business requirements for the system.

1.1. Analysis of the Master Project Schedule
The foundation of the application's business logic is derived directly from the Master Project Schedule for the Doi Chom Thong Water Production Plant project. This schedule establishes the core parameters and constraints that the software must enforce and monitor. The project is structured into four main phases, each assigned a specific weight that signifies its contribution to the overall project completion. These phases are: Planning & Permitting (15% weight), Procurement & Logistics (10% weight), Construction & Installation (65% weight), and Testing & Handover (10% weight). Each of these phases is further decomposed into a series of detailed sub-activities, complete with specified durations and planned start and end dates. These values are not merely informational data points; they represent the authoritative business rules that define the project's timeline, measure its progress, and ultimately determine its success. The system must be architected to treat these weights, dates, and durations as foundational constraints for all subsequent calculations and logic.

1.2. Advanced Progress Tracking and Weighted Calculations
To provide an accurate and meaningful representation of project status, the system must implement a sophisticated, multi-level progress tracking mechanism based on weighted calculations. This approach ensures that the completion of more critical or effort-intensive tasks contributes proportionally more to the overall progress metric. The logic for these calculations will be hierarchical, rolling up from the most granular tasks to the top-level project status.   

The implementation logic is defined as follows:

Sub-Activity Completion: The progress of a leaf-level task, such as "Kick-off & Scope Verification" (ID 1.1), will be a value between 0 and 1 (representing 0% to 100%) updated manually by the responsible team members. This is the primary data input for the progress calculation engine.

Phase Completion: The completion percentage for a main phase is not a simple average of its sub-activities. It must be a weighted average, reflecting that sub-activities have different durations and thus contribute differently to the phase's completion. If explicit weights are not assigned to sub-activities, their duration serves as a reliable proxy for effort. The formula for phase completion is therefore a weighted average based on duration :   


$$ \text{PhaseCompletion} = \frac{\sum_{i=1}^{n} (\text{SubActivityCompletion}_i \times \text{SubActivityDuration}i)}{\sum{i=1}^{n} \text{SubActivityDuration}_i} $$
Where 'i' represents each sub-activity within the phase.

Overall Project Completion: The total project progress is the sum of each phase's calculated completion percentage multiplied by its predefined master weight. This calculation provides a high-level, accurate snapshot of the project's status against its plan. The formula is:   


$$ \text{OverallProjectCompletion} = \sum_{p=1}^{m} (\text{PhaseCompletion}p \times \text{PhaseWeight}p) $$
For this specific project, this translates to:
$$ \text{OverallProjectCompletion} = (\text{Completion}{\text{Phase1}} \times 0.15) + (\text{Completion}{\text{Phase2}} \times 0.10) + (\text{Completion}{\text{Phase3}} \times 0.65) + (\text{Completion}{\text{Phase4}} \times 0.10) $$

A critical architectural consideration emerges from this structure. The weighting system is inherently hierarchical, and a robust system must not be hardcoded to only two levels of nesting (Phase and Sub-Activity). Future project needs may require breaking down a complex sub-activity like "Work on Clear Water Tank Roof 1" (ID 3.1) into even smaller, weighted tasks. Therefore, the business logic for calculating progress should be implemented as a recursive function. This function would be capable of calculating the weighted completion of any parent task by iterating through its direct children, multiplying their individual completion by their relative weights, and summing the results. This design choice ensures the architecture is flexible and resilient to future changes in project granularity without requiring a significant rewrite of the core logic.

1.3. Modeling Task Dependencies and the Critical Path
A project schedule is more than a list of tasks; it is a network of interconnected activities. The system must accurately model these relationships to forecast timelines and identify risks. The provided schedule implies numerous dependencies through its sequencing of start and end dates. For instance, the main "Construction & Installation" phase (Phase 3) cannot logically begin until key permits from Phase 1 are secured and equipment from Phase 2 has been procured.

The system will support the four standard types of task dependencies to provide project managers with maximum flexibility :   

Finish-to-Start (FS): Task B cannot start until Task A is finished. This is the most common dependency and applies to sequential activities like completing "Engineering Design Approval by PWA" (1.3) before submitting the "Building Modification Permit" (1.4).

Start-to-Start (SS): Task B cannot start until Task A has started. This could model relationships between parallel construction efforts, such as ensuring work on the carport roof (3.4) does not begin until work on the main roofs (3.1, 3.2) is underway.

Finish-to-Finish (FF): Task B cannot finish until Task A is finished. This is useful for tasks that must conclude simultaneously, such as "Complete DC and AC Electrical System Interconnection" (3.7) and the final "Pre-Commissioning Test" (4.1).

Start-to-Finish (SF): Task B cannot finish until Task A has started. This is the least common type but will be supported for completeness.

Using these defined dependencies and the duration of each task, the system will dynamically calculate the project's critical path. The critical path is the longest sequence of dependent tasks, and its length determines the shortest possible duration for the entire project. Any delay to a task on this path will directly delay the project's final completion date. This calculation is a core function of the business logic layer, providing project managers with an essential tool for risk management and resource allocation.   

A deeper analysis of the project schedule reveals a second, implicit layer of dependencies. The plan shows four major construction activities (3.1, 3.2, 3.3, 3.4) running in parallel for extended periods. It is highly unlikely that the main contractor has four distinct, fully equipped installation crews. This situation creates Resource Dependencies, which arise when multiple tasks compete for the same limited resource, such as a specialized crew or a piece of heavy machinery. A project plan that ignores these constraints will be unrealistically optimistic. For example, a delay in finishing work on "Clear Water Tank Roof 2" (3.2) could prevent the installation crew from moving to the "Carport Roof" (3.4), a dependency not captured by a simple FS or SS link. To address this, the system's architecture must be extended beyond simple logical dependencies. It must include the ability to define and manage a pool of project resources and assign them to specific activities. The business logic must then be capable of identifying and flagging scheduling conflicts where a single resource is assigned to multiple tasks simultaneously. This makes the project simulation more realistic and provides a crucial, practical tool for the project manager.   

Section 2: System Architecture and Data Model Design
This section defines the software's blueprint. We will create a comprehensive data model to represent all project entities and propose a modern, layered.NET architecture that ensures the system is robust, scalable, and easy to maintain.

2.1. Conceptual and Logical Data Model
The process of data modeling is essential for creating a blueprint of the information system, ensuring that the data structure aligns with business requirements. This process begins with identifying the core business concepts and progresses to a detailed logical model that defines entities, attributes, and their relationships.   

The core entities required for the Solar Project Management System are:

Project: The top-level entity representing the entire initiative.

Phase: The four main project phases with their associated weights.

Activity: The detailed sub-activities within each phase.

TaskDependency: A link table to model the relationships between activities.

Resource: A new entity to model the project's human and material resources, a requirement derived from the analysis of concurrent construction tasks.

ActivityResource: A many-to-many join table linking activities to the resources they require.

Document: The core entity for the integrated Document Management System.

User: Represents system users and their roles for access control.

Notification: Stores messages for the real-time notification system.

To eliminate ambiguity and provide a clear specification for developers, these entities and their attributes are detailed in an Entity-Relationship Dictionary, which serves as a textual companion to a visual ERD diagram.   

Table 2.1: Entity-Relationship Dictionary

Entity

Attribute

Data Type

Key

Description

Project

ProjectId

int

PK

Unique identifier for the project.

ProjectName

nvarchar(255)

Official name of the project.

ProjectOwner

nvarchar(255)

The client organization (e.g., PWA Phayao).

MainContractor

nvarchar(255)

The primary contractor (e.g., PEA).

PlannedStartDate

datetime2

The official start date of the project.

PlannedEndDate

datetime2

The official end date of the project.

Phase

PhaseId

int

PK

Unique identifier for a phase.

ProjectId

int

FK

Links to the parent Project.

PhaseName

nvarchar(100)

Name of the phase (e.g., Planning & Permitting).

Weight

decimal(5, 4)

The weight of the phase (e.g., 0.15 for 15%).

StartDate

datetime2

Planned start date of the phase.

EndDate

datetime2

Planned end date of the phase.

Activity

ActivityId

int

PK

Unique identifier for an activity.

PhaseId

int

FK

Links to the parent Phase.

ActivityName

nvarchar(255)

Name of the sub-activity (e.g., Detailed Survey).

Duration

int

Duration of the activity in days.

StartDate

datetime2

Planned start date of the activity.

EndDate

datetime2

Planned end date of the activity.

PercentComplete

decimal(5, 4)

The completion status from 0.0 to 1.0.

TaskDependency

DependencyId

int

PK

Unique identifier for the dependency link.

PredecessorId

int

FK

Foreign key to the preceding Activity.

SuccessorId

int

FK

Foreign key to the succeeding Activity.

DependencyType

int

Enum: 1=FS, 2=SS, 3=FF, 4=SF.

LagTime

int

Lag time in days (can be negative for lead time).

Resource

ResourceId

int

PK

Unique identifier for a resource.

ResourceName

nvarchar(100)

Name of the resource (e.g., Installation Crew A).

ResourceType

nvarchar(50)

Type of resource (e.g., Team, Equipment).

Document

DocumentId

int

PK

Unique identifier for a document.

ActivityId

int

FK

Links the document to a specific activity.

DocumentName

nvarchar(255)

User-friendly name of the document.

FileName

nvarchar(255)

The actual stored file name.

Version

int

The version number of the document.

UploadDate

datetime2

Timestamp of when the document was uploaded.

UploaderId

int

FK

Foreign key to the User who uploaded it.

2.2. Physical Data Model and Implementation with Entity Framework Core
The logical data model will be translated into a physical database schema using Entity Framework (EF) Core, a modern Object-Relational Mapper (ORM) for the.NET platform. EF Core bridges the gap between the object-oriented C# code and the relational database (e.g., SQL Server), allowing developers to work with data using C# classes and LINQ queries.   

The implementation will follow these standard steps:

Create Plain Old C# Object (POCO) Classes: For each entity defined in the dictionary above, a corresponding C# class will be created. For example, public class Project { public int ProjectId { get; set; }... }.

Create a DbContext: An AppDbContext class will be created that inherits from Microsoft.EntityFrameworkCore.DbContext. This class acts as the session with the database and will contain a DbSet<T> property for each entity (e.g., public DbSet<Project> Projects { get; set; }).   

Configure Relationships: While EF Core can infer many relationships by convention, the Fluent API will be used within the OnModelCreating method of the DbContext for explicit configuration. This is especially important for defining the many-to-many relationship between Activity and Resource (via the ActivityResource join table) and setting up precise cascade delete behaviors.

Database Migrations: The dotnet-ef command-line tool will be used to manage the database schema. The dotnet ef migrations add <MigrationName> command will scaffold a new migration based on changes to the model, and dotnet ef database update will apply those changes to the target database. This approach provides a version-controlled history of the database schema, which is crucial for team development and deployment.   

2.3. A Modern N-Tier.NET Architecture
To ensure the system is scalable, testable, and maintainable, a decoupled, N-Tier architecture will be employed, specifically following the principles of Clean Architecture or Onion Architecture. This architectural style separates concerns into distinct layers, preventing the creation of a tightly-coupled monolith where business logic, data access, and UI code are intertwined.   

The proposed architecture consists of the following layers:

Core/Domain Layer: This is the innermost layer and the heart of the application. It contains the C# entity classes (POCOs) and any core business logic that is completely independent of technology concerns like databases or web frameworks.

Application Layer: This layer orchestrates the application's use cases. It contains application-specific business logic and defines interfaces for any external dependencies, such as repositories (IProjectRepository) or notification services (INotificationService). It depends on the Core/Domain layer but knows nothing about the outer layers.

Infrastructure Layer: This layer provides the concrete implementations for the interfaces defined in the Application layer. It contains the EF Core AppDbContext, repository classes that use EF Core to query the database, and clients for any external services (e.g., an SMTP client for sending emails). This layer depends on the Application layer.

Presentation Layer: This is the outermost layer, which for this project will be an ASP.NET Core Web API. It contains the API Controllers that handle incoming HTTP requests. Its role is to validate input, call the appropriate methods in the Application layer, and map the results to Data Transfer Objects (DTOs) to be sent back to the client.

Adopting this decoupled architecture is a strategic decision that provides immense long-term value. The project's 540-day duration means that business requirements or underlying technologies could evolve. For example, if a decision were made to migrate the database from SQL Server to a different platform, this architecture contains the change entirely within the Infrastructure layer. A new set of repository implementations would be created, but the Application and Domain layers—which house the complex and valuable business logic—would remain completely unchanged. This isolation makes the system highly adaptable and significantly reduces the cost and risk of future modifications.   

Section 3: Backend Implementation: API, Real-Time Services, and Automation
This section details the construction of the server-side components. We will design a RESTful API for core operations, a real-time layer for live monitoring, and a background service for automated tasks, all built on ASP.NET Core best practices.

3.1. Designing a RESTful API for Project Operations
The primary interface for the frontend application and any potential third-party integrations will be a well-defined RESTful API. The API design will adhere to industry best practices, using nouns for resource URIs (e.g., /api/projects), standard HTTP verbs for actions (GET, POST, PUT, DELETE), and appropriate HTTP status codes to communicate outcomes. To handle potentially large datasets, all collection-based endpoints will support filtering, sorting, and pagination.   

A formal API specification provides a clear contract for frontend and backend developers, enabling parallel workstreams and reducing integration friction.

Table 3.1: RESTful API Endpoint Specification

Method

URI

Description

Sample Success Response

GET

/api/projects/{id}

Retrieves the full details of a single project, including its nested phases and activities.

200 OK with a JSON object representing the project.

GET

/api/projects/{id}/gantt

Retrieves project data specifically formatted for consumption by the Blazor Gantt chart component.

200 OK with a hierarchical JSON structure.

GET

/api/projects/{id}/progress

Calculates and returns the current weighted completion percentage for the project and each of its phases.

200 OK with JSON: {"overallCompletion": 0.45,...}

PUT

/api/activities/{id}/progress

Updates the completion percentage of a single activity. Request body: {"percentComplete": 0.75}.

200 OK with the updated activity object.

POST

/api/dependencies

Creates a new dependency link between two activities. Request body: {"predecessorId": 5, "successorId": 6, "type": 1}.

201 Created with the new dependency object.

DELETE

/api/dependencies/{id}

Deletes a dependency link.

204 No Content.

GET

/api/activities/{id}/documents

Lists all documents associated with a specific activity.

200 OK with an array of document metadata objects.

POST

/api/activities/{id}/documents

Uploads a new document for an activity. This will be a multipart/form-data request.

201 Created with the new document's metadata.

3.2. Live Project Monitoring with SignalR
To provide a dynamic and responsive user experience, the system will incorporate a real-time communication layer using ASP.NET Core SignalR. SignalR enables bi-directional communication, allowing the server to push updates to connected clients instantly, which is ideal for live dashboards and notifications. This eliminates the need for users to manually refresh the page to see the latest project status.   

The implementation will be designed as follows:

Create ProjectHub.cs: A central communication hub class will be created that inherits from Microsoft.AspNetCore.SignalR.Hub. This hub will manage client connections.   

Server-to-Client Communication: The hub's primary role is to push data to clients. It will define methods that can be called from the server to be executed on the client, such as ReceiveProgressUpdate, TaskAdded, or DependencyChanged.

Integration with Application Services: When a business operation that affects the project state occurs (e.g., a user updates an activity's progress via the API), the corresponding application service will perform the database update. After successfully saving the changes, it will use an injected IHubContext<ProjectHub> to broadcast the change to all relevant clients. For example: await _hubContext.Clients.All.SendAsync("ReceiveProgressUpdate", activityId, newProgress);.   

Client-Side Integration: The Blazor frontend application will use the SignalR client library to establish a persistent connection to the /projectHub endpoint. It will then register event handlers to listen for messages from the server (e.g., connection.on("ReceiveProgressUpdate",...)). When a message is received, the client-side code will update the Gantt chart's data source in memory, causing the UI to re-render with the new information automatically.   

3.3. Automated Notifications and Reporting via Background Services
Many project management tasks, such as sending deadline reminders or generating daily reports, are not tied to a specific user request and should run automatically in the background. ASP.NET Core provides the IHostedService interface for implementing such long-running background tasks. This architecture will utilize two types of hosted services to handle automation.   

Timed Service (DeadlineNotifierService): This service will be responsible for periodically checking the project schedule for upcoming events.

It will inherit from the BackgroundService base class, which provides a convenient ExecuteAsync method for long-running logic.   

The ExecuteAsync method will contain a continuous loop with a delay (e.g., await Task.Delay(TimeSpan.FromHours(24), stoppingToken)), causing it to run once per day.

Inside the loop, it will use IServiceScopeFactory to create a new dependency injection scope. This is a critical step, as IHostedService is a singleton, but it needs to consume scoped services like the AppDbContext.   

Within this scope, it will query the database for activities with deadlines approaching in the next week or tasks that are overdue. For each finding, it will create a notification object and pass it to a shared queue for processing.

Queued Service (NotificationDispatcherService): This service will manage the actual dispatching of notifications, decoupling this potentially slow I/O-bound operation from the main application threads.

This service will be based on the queued background task pattern, using a System.Threading.Channels.Channel<T> or a ConcurrentQueue with a SemaphoreSlim to create a thread-safe, in-memory message queue.   

It will expose a public method (EnqueueNotification) that other parts of the application (like the API controllers or the DeadlineNotifierService) can call to add a notification to the queue.

Its ExecuteAsync method will continuously dequeue items and process them one by one. Processing will involve sending an email, writing a record to the Notification table in the database, and pushing a real-time alert via SignalR.

This combination of a queued service and SignalR creates a highly responsive and robust notification system. When a user action triggers multiple notifications, the API controller can quickly add them to the queue and return a response to the user almost instantly, adhering to the best practice of completing long-running tasks outside of the HTTP request. The background service then reliably handles the delivery, and SignalR provides immediate visual feedback in the user's interface, confirming that their action has been processed and notifications have been sent.   

Section 4: Frontend Implementation: An Interactive Project Command Center
This section focuses on the user-facing part of the application. We will select the best UI component for visualizing the project and design a dashboard that serves as the project's interactive control panel.

4.1. Comparative Analysis of Blazor Gantt Chart Components
The centerpiece of the user interface will be a powerful Gantt chart component. The choice of this component is a significant architectural decision, as it will dictate the user's ability to interact with and manage the project plan. A comparative analysis of leading third-party Blazor Gantt components is necessary to make an informed, evidence-based recommendation. Key vendors in this space include Syncfusion, Telerik, and others. The evaluation criteria are derived directly from the project's business requirements: hierarchical data binding, dependency visualization, interactive editing, performance with large datasets, and support for advanced features like critical path analysis and baselining.   

Table 4.1: Blazor Gantt Component Feature Comparison

Feature

Syncfusion Blazor Gantt

Telerik UI for Blazor Gantt

Assessment for Project

Data Binding

Supports flat and hierarchical data sources. Excellent support for remote data via DataManager.   

Supports flat and hierarchical data. ItemsField for hierarchy.   

Both are strong. Syncfusion's DataManager offers slightly more streamlined remote data handling.

Task Dependencies

Full support for FS, SS, FF, SF relationships. Interactive creation via drag-and-drop.   

Full support for all dependency types, rendered in the timeline.   

Both components fully meet the requirement.

Interactive Editing

In-cell, dialog, and taskbar editing modes. CRUD operations are configurable.   

Supports Inline, Popup, and InCell editing modes.   

Both offer comprehensive editing capabilities.

Critical Path Analysis

Built-in feature to automatically calculate and highlight the critical path on the timeline.   

Not explicitly listed as a built-in, automated feature in the overview documentation. May require custom implementation.

Syncfusion has a distinct advantage here, as this is a core project requirement.

Baselines

Built-in support for displaying baseline bars to compare planned vs. actual schedules.   

Not explicitly mentioned as a standard feature in the overview documentation.

Syncfusion has a clear advantage, providing a key feature for project deviation tracking.

Performance

Features virtual scrolling to handle large numbers of tasks without performance degradation.   

Standard scrolling is available. Performance with very large datasets is not highlighted as a key feature.

Syncfusion's explicit mention of virtualization is a significant plus for future scalability.

Mobile/Touch Support

Highly responsive layout and touch-friendly gestures are emphasized as key features.   

General responsiveness is supported as part of the Telerik UI suite.

Both are suitable, but Syncfusion places a stronger emphasis on this in its marketing.

Recommendation:
Based on the comprehensive feature comparison, the Syncfusion Blazor Gantt Chart is the recommended component for this project. Its out-of-the-box support for critical path calculation and baseline visualization directly addresses advanced requirements identified in the project analysis. These features, combined with its strong performance claims regarding virtualization, make it the most suitable choice for building a feature-rich and scalable project management command center.   

4.2. Designing the Project Management Dashboard
The main user interface will be a single, comprehensive dashboard designed to serve as the project's command center. It will be built as a Blazor WebAssembly application, providing a rich, desktop-like experience in the browser.

The UI design blueprint is as follows:

Main View: The dashboard will be dominated by the selected Syncfusion Gantt Chart component, providing a full-screen, interactive view of the entire project plan.   

Data Loading: Upon initialization, the dashboard will make an asynchronous call to the GET /api/projects/{id}/gantt endpoint. The returned hierarchical data will be bound to the Gantt component, populating the tree list and timeline views.

Real-Time Integration: The Blazor component will establish and maintain a SignalR connection to the ProjectHub on the server. It will register client-side handlers for server-pushed events like ReceiveProgressUpdate. When such an event is received, the handler will locate the corresponding task in the Gantt chart's local data source and update its properties. Blazor's rendering engine will then automatically update the UI to reflect the change, providing a live, dynamic view of project progress without requiring a full page refresh.

Interactivity: The dashboard will empower users with a range of interactive capabilities:

Navigation: Users can expand and collapse phases in the tree list to drill down into sub-activities.

Detailed View: Clicking on any task or document icon will open a side panel or modal dialog displaying detailed information, including a list of associated documents.

Progress Updates: Users can update a task's completion percentage directly within the Gantt grid. This action will trigger a PUT request to the /api/activities/{id}/progress endpoint. Upon a successful response, the local data will be updated, and a SignalR message will be broadcast to other connected clients.

Dependency Management: Users can visually create new task dependencies by dragging from the end of one taskbar to the start of another. This interaction will trigger a POST request to the /api/dependencies endpoint to persist the new relationship.

Section 5: Integrated Construction Document Management System (DMS)
This section details the architecture for a secure, integrated module for managing all project-related documentation, a critical requirement for a complex construction project.

5.1. Architecting an Embedded DMS
A robust Document Management System (DMS) is not an optional add-on but a core component of successful construction project management. It provides the necessary control and organization for all electronic documents and images, from permits and blueprints to contracts and reports. The system will feature an embedded DMS with a dedicated architecture to handle these requirements securely and efficiently.   

The DMS architecture will be composed of:

Expanded Data Model: The Document entity will be enhanced to include VersionNumber, FileHash (for integrity checks), MimeType, and StoragePath. A separate DocumentHistory entity will be created to serve as an immutable audit log, recording every significant action (e.g., upload, view, download, check-in, check-out) performed on a document, along with the user and timestamp.   

Decoupled Storage Strategy: Storing large binary files (like PDFs or CAD drawings) directly in the primary relational database is highly inefficient and can severely degrade performance. Therefore, a decoupled storage strategy will be used. The actual files will be stored in a dedicated, secure, and scalable object storage service, such as Azure Blob Storage or a local secured file server. The application's SQL database will only store the document's metadata and a reference or path to its location in the blob storage.

Dedicated API Endpoints: A set of specialized API endpoints will be created to manage the document lifecycle:

POST /api/activities/{id}/documents/upload: This endpoint will handle file uploads. The backend will receive the file, generate a unique, secure filename, upload the binary data to the designated blob storage container, and then create a new Document record in the database with the associated metadata and storage path.

GET /api/documents/{id}/download: This endpoint will retrieve a document. It will first look up the document's metadata in the database, then generate a secure, time-limited access URL to the file in blob storage, and finally redirect the user to that URL for the download.

POST /api/documents/{id}/checkout: This endpoint implements a locking mechanism. It will set a flag on the document record, indicating that it is being edited. This prevents other users from making conflicting changes simultaneously.   

POST /api/documents/{id}/checkin: This endpoint is used to upload a new version of a checked-out document. It will upload the new file, create a new Document record with an incremented version number, link it to the previous version, and release the lock.

5.2. Essential Features for Construction Project Compliance
For a construction project, a DMS must provide specific features to ensure regulatory compliance, enhance collaboration, and reduce risk. The system will implement the following essential features tailored for the construction industry.   

Centralized Document Repository: The system will act as the single source of truth for all project documentation. The UI will provide a dedicated "Documents" section where users can browse a structured, folder-like view of all files. Additionally, documents will be directly accessible from their associated tasks within the Gantt chart, providing contextual access to information.   

Strict Version Control: This is a non-negotiable feature for construction. Outdated plans or specifications can lead to costly rework. When a user accesses a document, the UI will clearly display the current version number and provide access to its complete history. Users with appropriate permissions can view, compare, or roll back to previous versions, ensuring everyone is working from the correct information.   

Role-Based Access Control (RBAC): Security and confidentiality are paramount. The system will implement granular, role-based access controls for all document operations. For example, an "Engineer" role might have permission to upload and version drawings, while a "Subcontractor" role may only have view and download permissions. Approval workflows will also be governed by RBAC, ensuring that only authorized individuals, like a "Project Manager," can approve a submitted RFI or permit application.   

Comprehensive Audit Trails: To ensure accountability and simplify compliance audits, the system will provide a transparent and unalterable audit trail for every document. The UI will allow authorized users to view the    

DocumentHistory for any file, showing a complete, timestamped log of who viewed, downloaded, modified, or approved it.

Mobile Accessibility: Construction projects happen in the field, not just in the office. The Blazor frontend will be designed with a fully responsive layout, ensuring that teams on the construction site can use tablets or mobile phones to access the latest blueprints, submit daily reports with photo attachments, or review safety documents in real-time. This capability is critical for bridging the gap between the office and the field and is a key feature of modern construction DMS.   

Conclusion and Strategic Recommendations
This report has laid out a comprehensive architectural blueprint for the Solar Rooftop Power Generation Management System. By translating the master schedule into concrete business logic and designing a modern.NET solution, we have defined a path toward a powerful, scalable, and maintainable application. The proposed architecture, built on clean design principles, leverages EF Core for data persistence, a RESTful API for operations, SignalR for real-time updates, and Blazor for a rich, interactive frontend. Crucially, it incorporates a purpose-built Document Management System to meet the stringent compliance and operational needs of a large-scale construction project.

Strategic Recommendations
Prioritize a Phased Development Approach: The project should be developed in logical phases to deliver value incrementally. The recommended sequence is:

Phase I: Core data model, backend API, and user authentication (Sections 2 & 3).

Phase II: The interactive Gantt chart dashboard and progress update functionality (Section 4).

Phase III: The integrated Document Management System (Section 5).

Phase IV: The automated notification and background services.

Emphasize Security from Day One: Security, particularly the Role-Based Access Control (RBAC) model, should be designed and implemented from the very beginning of the development cycle. It must be integrated deeply into the API and DMS logic, not treated as an afterthought.

Invest in Automated Testing: The complexity of the business logic for weighted progress calculation and critical path analysis necessitates a robust automated testing strategy. The decoupled clean architecture is highly conducive to testing. A comprehensive suite of unit tests for the Application and Core layers, along with integration tests for the API endpoints, is critical to ensure correctness and prevent regressions.

Plan for Scalability: While the initial deployment targets a single project, the underlying architecture is designed with scalability in mind. For production deployment, using managed cloud services like Azure SQL for the database and the Azure SignalR Service will offload connection management and allow the system to scale horizontally to support multiple concurrent projects in the future with minimal architectural changes.