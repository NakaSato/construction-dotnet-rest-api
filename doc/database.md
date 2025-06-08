Data Management Requirements
This section details the requirements for data storage, modeling, and management within the application.

5.1. Database System
Recommendation: PostgreSQL.
Rationale: PostgreSQL is a powerful, open-source object-relational database system known for its reliability, feature robustness, and performance. Community discussions and technical comparisons often highlight its strong standards compliance, stability, and extensive feature set, which includes advanced data types like JSONB and excellent geospatial support through the PostGIS extension. The Npgsql driver provides excellent Entity Framework Core support, making it a good fit for.NET applications. Given the requirement to store GPS coordinates with images, PostgreSQL's native geospatial capabilities could prove highly beneficial for future analytical features or map-based visualizations.   
ORM: Entity Framework Core (EF Core).
Rationale: EF Core is a modern, cross-platform Object-Relational Mapper (ORM) for.NET. It simplifies data access by allowing developers to work with domain-specific objects and properties, handles the translation of LINQ queries into SQL, and provides robust mechanisms for database schema migrations.

Data Models / Entity Definitions (Key entities)
The following table outlines the key data entities and their core attributes. This serves as a foundation for the database schema design.

Table 5.2.1: Key Data Entities and Core Attributes

Entity	Core Attributes	Relationships	Notes
User	UserID (PK, GUID/int), Username (unique), PasswordHash, Email (unique), RoleID (FK), FullName, IsActive	One User (as Manager) to Many Projects. Many Users (Technicians) assigned to Many Tasks.	
Project	ProjectID (PK, GUID/int), ProjectName, Address, ClientInfo, Status (e.g., Planning, InProgress, Completed), StartDate, EstimatedEndDate, ActualEndDate, ProjectManagerID (FK to User)	One Project to Many Tasks. One Project to Many ImageMetadata records.	
Task	TaskID (PK, GUID/int), ProjectID (FK), Title, Description, Status (e.g., Pending, InProgress, Completed, Blocked), DueDate, AssignedTechnicianID (FK to User, nullable), CompletionDate	One Task to Many ImageMetadata records (nullable).	
ImageMetadata	ImageID (PK, GUID/int), ProjectID (FK), TaskID (FK, nullable), UploadedByUserID (FK to User), CloudStorageKey (unique, for S3/Blob key), OriginalFileName, ContentType, FileSizeInBytes, UploadTimestamp, CaptureTimestamp (from EXIF), GPSLatitude (nullable), GPSLongitude (nullable), EXIFData (JSONB, nullable)	Many ImageMetadata to One Project. Many ImageMetadata to One Task (optional). Many ImageMetadata to One User (uploader).	GPS and EXIF data enhance auditability and context. CloudStorageKey is used instead of full URL for flexibility.
Role	RoleID (PK, int), RoleName (unique, e.g., "Administrator", "ProjectManager", "FieldTechnician")	One Role to Many Users.

The EXIF data associated with images can be extensive and varied. Storing this information flexibly is advantageous. The native_exif package in Flutter allows for the extraction of numerous EXIF attributes. The backend system needs a way to persist this potentially diverse set of key-value pairs. Defining fixed columns in the ImageMetadata table for every conceivable EXIF tag would be impractical and would lead to a rigid schema. PostgreSQL's JSONB data type, noted as a significant strength , is perfectly suited for storing semi-structured data like EXIF attributes. This allows the system to store all relevant EXIF data without predefining a fixed schema for each tag, while still enabling efficient querying within these JSONB fields if necessary for advanced analysis or specific data retrieval scenarios. Consequently, the ImageMetadata entity will include an EXIFData field of type JSONB to store the raw or processed EXIF key-value pairs.

Data Persistence and Retrieval Logic
Data access logic will be encapsulated within repository patterns or service layers. This abstraction promotes separation of concerns, making the codebase more modular and testable. EF Core will be used to interact with the PostgreSQL database, with LINQ queries being the primary method for data retrieval and manipulation. Queries will be optimized for performance, especially for frequently accessed data, list views, and search functionalities, through appropriate indexing strategies and efficient query construction.