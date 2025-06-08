6. Image Handling Subsystem
The image handling subsystem is a critical component, facilitating the capture, upload, storage, and retrieval of images related to solar projects.

6.1. Image Upload Workflow
The workflow involves coordinated actions between the Flutter mobile application and the.NET backend API.

Flutter App:
The user captures a new image using the device camera or selects an existing image from the gallery, facilitated by the image_picker package.   
(Optional) The user may be provided with tools to add basic annotations or tags to the image.
The application automatically captures the current GPS coordinates (latitude and longitude) using the geolocator package, if location permissions are granted , and records the current timestamp.   
The application attempts to extract relevant EXIF data from the image file (especially DateTimeOriginal and GPS tags) using a package like native_exif.   
A multipart/form-data request is constructed. This request will include the image file itself and associated metadata such as ProjectID, TaskID (if applicable), GPS coordinates, capture timestamp, and any extracted EXIF data. The principle of constructing such a request is similar to that described for services like Cloudinary, where files and fields are added to a multipart request.   
The request is sent to the designated image upload endpoint on the.NET backend API.
If the device is offline, the image file and its metadata are queued in the local Drift database. The synchronization mechanism will handle uploading this queued data when connectivity is restored.   
.NET Backend API:

The API endpoint receives the IFormFile (the image) and the associated metadata from the multipart request.
The API validates the received metadata (e.g., ensuring ProjectID is valid) and the image file (e.g., checking file size limits, allowed content types).
The image file is streamed efficiently to the configured cloud object storage (AWS S3 or Azure Blob Storage).   
Upon successful upload to the cloud storage, the API saves a reference to the image (e.g., the unique key or path in the cloud storage) and all its metadata (including client-provided GPS, timestamp, EXIF data, and server-generated information like uploader ID and upload timestamp) into the ImageMetadata table in the PostgreSQL database.
A success response, potentially including the ID or URL of the stored image metadata record, is returned to the Flutter application.
6.2. Image Formats and Compression
Supported Formats: The system will primarily support common image formats such as JPEG (for photographic content) and PNG (for images requiring transparency or lossless quality).
Compression: To optimize storage space and reduce bandwidth consumption during uploads and downloads, image compression strategies will be considered. This might involve:
Client-Side Compression: The Flutter app could offer options to compress images before upload, balancing quality with file size.
Server-Side Compression/Optimization: After upload, the backend could perform further optimization or resizing if necessary, though this adds complexity. Initially, relying on appropriate client-side settings is preferred.
6.3. EXIF Data Handling
EXIF (Exchangeable image file format) data embedded in image files can provide valuable contextual information.

Capture (Flutter): The Flutter application will use the native_exif package  or a similar library to read EXIF data from images selected from the gallery or newly captured by the camera. Key EXIF tags to prioritize include DateTimeOriginal (for capture timestamp), GPS-related tags (GPSLatitude, GPSLongitude, GPSLatitudeRef, GPSLongitudeRef), Model (device model), and Orientation.   
Transmission: Selected and processed EXIF data will be transmitted as part of the multipart/form-data request when the image is uploaded to the backend.
Storage (.NET/PostgreSQL): The backend will store key EXIF fields in dedicated, indexed columns within the ImageMetadata table for efficient querying (e.g., CaptureTimestamp, GPSLatitude, GPSLongitude). Additionally, a more comprehensive set of extracted EXIF data can be stored in a JSONB column (e.g., FullEXIFPayload) for future analytical use or detailed auditing purposes.
Table 6.3.1: Key EXIF/Metadata Fields for Images

Field Name (DB)	Data Type (DB)	Source	Purpose
CaptureTimestamp	TIMESTAMP WITH TIME ZONE	EXIF DateTimeOriginal, or file system timestamp if EXIF is unavailable.	Auditing precisely when the photo was taken.
GPSLatitude	DOUBLE PRECISION	EXIF GPSLatitude (converted to decimal degrees).	Geotagging the image location.
GPSLongitude	DOUBLE PRECISION	EXIF GPSLongitude (converted to decimal degrees).	Geotagging the image location.
DeviceModel	VARCHAR(255)	EXIF Model (optional).	Information about the image capture device.
Orientation	SMALLINT	EXIF Orientation.	Ensuring correct image display orientation.
FullEXIFPayload	JSONB	All relevant extracted EXIF key-value pairs.	Comprehensive audit trail, future data mining.

Export to Sheets
6.4. Image Retrieval and Display Optimizations
Efficient retrieval and display of images are crucial for a good user experience in the mobile application.

Secure URLs: The backend API will provide endpoints that return secure, potentially time-limited URLs (e.g., pre-signed URLs for AWS S3 or SAS tokens for Azure Blob Storage) for accessing images directly from cloud storage. This prevents direct, unauthenticated access to image files.
Client-Side Caching: The Flutter application will implement caching strategies for frequently accessed images to reduce redundant downloads and improve loading times. Packages like cached_network_image can be used.
Lazy Loading: In list views or galleries displaying multiple images, lazy loading techniques will be employed so that images are only loaded when they are about to become visible on the screen.
Responsive Images (Consideration): For applications with significant bandwidth constraints or diverse client display capabilities, serving responsive images (i.e., different image sizes or resolutions tailored to the display context) might be considered. This could be achieved using features of the cloud storage provider or by integrating an image processing service, though this is a secondary optimization.
6.5. Security for Stored Images
Images stored in the cloud must be protected from unauthorized access.

Private Access: Cloud storage buckets or containers holding the project images will be configured with private access settings by default. Public access will be disabled.
Controlled Access via API: All image access by end-users will be brokered through the.NET backend API. The API will be responsible for authenticating and authorizing users before granting access to image resources, typically by generating secure, short-lived URLs as described above.