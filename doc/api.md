NET Backend API Requirements
The.NET backend API will serve as the central nervous system of the application, handling business logic, data persistence, and secure communication with the Flutter mobile client. It will be built using.NET 8 (ASP.NET Core).

API Architecture
RESTful API Design: The API will adhere to REST (Representational State Transfer) principles, utilizing standard HTTP methods (GET, POST, PUT, DELETE), status codes, and resource-based URLs for interactions.

.NET 8 Minimal APIs or Controllers: A hybrid approach may be adopted. Minimal APIs are suitable for straightforward, lightweight endpoints, particularly for simple CRUD operations, due to their "simplified routing and fewer boilerplate codes". Traditional controllers offer more structure and are preferable for complex endpoints requiring more elaborate logic, dependency injection patterns, or advanced features not as easily managed with the minimal approach.

Asynchronous Programming: The async/await pattern will be used extensively for all I/O-bound operations (e.g., database access, cloud storage interactions, external service calls) to ensure non-blocking behavior, improve responsiveness, and enhance server throughput.

Core Modules and Services
The backend will be organized into logical modules or services, each responsible for a distinct functional area.

Table 4.2.1: Core.NET Backend API Modules/Services

Module/Service	Description	Key Endpoints (Examples)
User Management & Authentication	Handles user registration (if applicable), login, profile management, role assignments, and JWT generation/validation.	/auth/login, /auth/register, /users/me
Project Management	Manages CRUD operations for projects, including project details, status updates, and assignment to users.	/projects, /projects/{id}
Task Management	Manages CRUD operations for tasks, including assignment to technicians, status updates, and linking to projects.	/projects/{projectId}/tasks, /tasks/{id}
Image Handling Service	Manages image uploads from clients, metadata processing, secure storage in the cloud, and URL generation for access.	/projects/{projectId}/images, /images/{id}/metadata
Notification Service (Optional)	(If SignalR is implemented) Manages the sending of real-time notifications to connected clients.	(Internal service, interacts with SignalR hub infrastructure)

Authentication and Authorization
Robust authentication and authorization mechanisms are critical for securing the application and its data.

JWT-based Authentication:
The API will implement JWT bearer token authentication using the Microsoft.AspNetCore.Authentication.JwtBearer NuGet package. Upon successful login, the API will issue a JWT to the client. This token must be included in the Authorization header of subsequent requests.   
JWTs will contain standard claims (e.g., iss, aud, exp, sub for user ID) and custom claims (e.g., user roles).
The API will rigorously validate incoming JWTs, checking the signature, issuer, audience, and expiration to ensure authenticity and prevent tampering.   
Role-Based Access Control (RBAC):
User roles (e.g., "Administrator," "ProjectManager," "FieldTechnician") will be defined to represent different levels of access and permissions within the system.
API endpoints will be secured using authorization attributes (e.g., ``) to restrict access to resources and operations based on the authenticated user's role(s).   

Image Upload Processing and Storage
The backend will handle the reception, processing, and storage of images uploaded from the Flutter application.

Receiving Files: API endpoints (e.g., /api/v1/images/upload) will be designed to accept image files as IFormFile objects, which is a standard way to handle file uploads in ASP.NET Core.   
Temporary Storage/Buffering: Incoming file streams will be handled efficiently to minimize memory footprint, potentially streaming directly to the cloud storage service.
Metadata Extraction: The API may perform additional server-side extraction or validation of EXIF data if necessary, complementing the data sent by the client.
Cloud Storage Integration:
Processed images will be uploaded to a dedicated bucket in a cloud object storage service, such as AWS S3 (using AWSSDK.S3 and TransferUtility ) or Azure Blob Storage (using Azure.Storage.Blobs ).   
A reference to the cloud-stored image (e.g., its URL or a unique key) will be stored in the application's PostgreSQL database, along with all relevant metadata (project ID, task ID, uploader ID, timestamp, GPS coordinates, etc.).
Cloud service credentials (access keys, secret keys) and configuration details (bucket/container names, regions) will be stored securely using the.NET configuration system (e.g., appsettings.json for local development, environment variables, or managed identity services in cloud environments).   

API Versioning Strategy
To allow the API to evolve over time without disrupting existing client applications, a clear versioning strategy is essential.   

Recommendation: URI Path Versioning. This strategy involves including the version number directly in the URI path (e.g., /api/v1/projects, /api/v2/projects).
Rationale: URI path versioning is generally considered easy to implement and understand, and the version is clearly visible and explicit in the URL, which aids discoverability and debugging. While it can lead to some redundancy in URIs if many versions are maintained, its explicitness is often favored for clarity in API contracts.   
The Microsoft.AspNetCore.Mvc.Versioning NuGet package can be used to implement API versioning in ASP.NET Core.   
Comprehensive documentation must be provided for each API version, detailing changes, new features, deprecated functionalities, and clear migration paths for clients.   

Real-time Features (If required)
If real-time notifications or data updates are a requirement (e.g., instant notification of new task assignments or project status changes), ASP.NET Core SignalR will be integrated.

SignalR Integration: SignalR enables bi-directional real-time communication between the server and connected clients.   
Hub Definition: SignalR Hubs (e.g., ProjectUpdatesHub, NotificationHub) will be created on the server. These hubs act as high-level pipelines that manage client connections, groups, and message broadcasting. For example, a hub method like public async Task SendMessage(string user, string message) { await Clients.All.SendAsync("ReceiveMessage", user, message); } can be called by clients to send messages or by the server to push updates to all connected clients.   
Security: SignalR hubs will be secured using the same JWT authentication mechanism employed for REST API endpoints. The SignalR endpoint mapping can be configured to require authorization: app.MapHub<ChatHub>("/chathub").RequireAuthorization();.   

Background Tasks (If applicable)
For operations that need to run in the background without direct user interaction,.NET Hosted Services will be utilized.

Hosted Services provide a way to implement long-running background tasks or scheduled jobs within an ASP.NET Core application.   
Potential use cases include periodic data cleanup routines, generation of complex reports, batch processing of notifications, or automated maintenance tasks.

API Endpoint Specification (Summary)
A detailed API specification will be maintained using the Swagger/OpenAPI standard, generated from the.NET backend code. This section provides a high-level summary of key resources and their primary actions. API versioning (e.g., /v1/) will be used in the path.

Authentication Endpoints (/api/v1/auth)

POST /login: Authenticates a user and returns a JWT.
POST /register: (Access controlled by Administrator) Registers a new user.
POST /refresh-token: Obtains a new JWT using a refresh token.
Project Endpoints (/api/v1/projects)

GET /: Retrieves a list of projects accessible to the authenticated user. Supports pagination and filtering.
POST /: Creates a new project.
GET /{projectId}: Retrieves details of a specific project.
PUT /{projectId}: Updates details of a specific project.
DELETE /{projectId}: Deletes a specific project (soft delete preferred).
Task Endpoints

GET /api/v1/projects/{projectId}/tasks: Retrieves tasks for a specific project. Supports pagination and filtering.
POST /api/v1/projects/{projectId}/tasks: Creates a new task for a specific project.
GET /api/v1/tasks/{taskId}: Retrieves details of a specific task.
PUT /api/v1/tasks/{taskId}: Updates details or status of a specific task.
DELETE /api/v1/tasks/{taskId}: Deletes a specific task (soft delete preferred).
Image Endpoints (/api/v1/images)

POST /upload: Uploads an image. Expects multipart/form-data containing the image file and metadata fields (e.g., projectId, taskId (optional), gpsLatitude, gpsLongitude, captureTimestamp, exifJsonPayload).
GET /project/{projectId}: Retrieves metadata for all images associated with a specific project.
GET /task/{taskId}: Retrieves metadata for all images associated with a specific task.
GET /{imageId}/metadata: Retrieves metadata for a specific image.
GET /{imageId}/download-url: Returns a secure, time-limited URL to download/view the actual image file from cloud storage.
User Management Endpoints (Admin only) (/api/v1/users)

GET /: Retrieves a list of all users.
POST /: Creates a new user (distinct from self-registration if that's restricted).
GET /{userId}: Retrieves details for a specific user.
PUT /{userId}: Updates details for a specific user.
PUT /{userId}/role: Assigns or changes the role of a specific user.
Table 9.0.1: API Endpoint Summary

Resource	HTTP Method	Example URI Path	Brief Description
Authentication	POST	/api/v1/auth/login	User login; returns JWT upon success.
Projects	GET	/api/v1/projects	List all projects accessible to the user.
Projects	POST	/api/v1/projects	Create a new project.
Tasks	GET	/api/v1/projects/{projectId}/tasks	List tasks for a given project.
Tasks	PUT	/api/v1/tasks/{taskId}	Update an existing task (e.g., status, assignment).
Images	POST	/api/v1/images/upload	Upload an image file with associated metadata.
Images	GET	/api/v1/images/{imageId}/download-url	Get a secure URL to view/download an image file.
User Management	GET	/api/v1/users	(Admin) List all users in the system.

Export to Sheets
The design of the image upload endpoint (POST /api/v1/images/upload) is crucial for efficient mobile synchronization and correct data association, especially considering potential offline scenarios. A mobile user might capture several photos related to a specific task within a project. When uploading, the Flutter application must transmit the relevant projectId and, optionally, the taskId along with each image file. The API endpoint will accept these identifiers as part of the multipart/form-data request, allowing the backend to immediately establish the correct associations in the ImageMetadata table, linking the image to its parent Project and Task entities.
For data captured offline, particularly new tasks and their associated images, a careful workflow is needed. If a task is created offline, it won't yet have a server-generated ID. The mobile application might need to first synchronize the new task, obtain its server-assigned ID upon successful creation, and then subsequently synchronize the images, now associating them with this new task ID. While more complex alternatives like batch uploads handling entity creation and association in a single transaction or using client-generated UUIDs for temporary linkage exist, the simpler, sequential approach (sync parent entities first, then child entities) is often more robust for initial implementation, provided the UI handles the intermediate states gracefully. The API contract must clearly define how these project and task associations are made during image uploads to support both online and offline-originated data.