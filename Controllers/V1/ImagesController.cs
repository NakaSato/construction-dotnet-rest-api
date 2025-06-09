using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing images in solar projects
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/images")]
[Authorize]
public class ImagesController : BaseApiController
{
    private readonly IImageService _imageService;
    private readonly IQueryService _queryService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageService imageService, IQueryService queryService, ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads an image file
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="projectId">Project ID to associate with the image</param>
    /// <param name="taskId">Optional task ID to associate with the image</param>
    /// <param name="captureTimestamp">Optional timestamp when the image was captured</param>
    /// <param name="gpsLatitude">Optional GPS latitude</param>
    /// <param name="gpsLongitude">Optional GPS longitude</param>
    /// <param name="deviceModel">Optional device model</param>
    /// <param name="exifData">Optional EXIF data</param>
    /// <returns>Image metadata</returns>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<ImageMetadataDto>>> UploadImage(
        IFormFile file, 
        [FromForm] Guid projectId,
        [FromForm] Guid? taskId = null,
        [FromForm] DateTime? captureTimestamp = null,
        [FromForm] double? gpsLatitude = null,
        [FromForm] double? gpsLongitude = null,
        [FromForm] string? deviceModel = null,
        [FromForm] string? exifData = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return CreateErrorResponse("No file uploaded", 400);

            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var uploadedByUserId))
                return CreateErrorResponse("Invalid user token", 401);

            var uploadRequest = new ImageUploadRequest
            {
                ProjectId = projectId,
                TaskId = taskId,
                CaptureTimestamp = captureTimestamp,
                GPSLatitude = gpsLatitude,
                GPSLongitude = gpsLongitude,
                DeviceModel = deviceModel,
                EXIFData = exifData
            };

            var result = await _imageService.UploadImageAsync(file, uploadRequest, uploadedByUserId);
            
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Image uploaded successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "uploading image");
        }
    }

    /// <summary>
    /// Gets image metadata by ID
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>Image metadata</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ImageMetadataDto>>> GetImage(Guid id)
    {
        try
        {
            var result = await _imageService.GetImageMetadataAsync(id);

            if (!result.Success)
                return CreateNotFoundResponse(result.Message);

            return CreateSuccessResponse(result.Data!, "Image metadata retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving image {id}");
        }
    }

    /// <summary>
    /// Gets a direct URL to view/download the image
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>Direct image URL</returns>
    [HttpGet("{id:guid}/url")]
    public async Task<ActionResult<ApiResponse<string>>> GetImageUrl(Guid id)
    {
        try
        {
            var result = await _imageService.GetImageUrlAsync(id);

            if (!result.Success)
                return CreateNotFoundResponse(result.Message);

            return CreateSuccessResponse(result.Data!, "Image URL retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"getting image URL for {id}");
        }
    }

    /// <summary>
    /// Gets all images for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of project images</returns>
    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ImageMetadataDto>>>> GetProjectImages(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return validationResult;

            var result = await _imageService.GetProjectImagesAsync(projectId, pageNumber, pageSize);

            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Project images retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving project images for project {projectId}");
        }
    }

    /// <summary>
    /// Gets all images for a specific task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of task images</returns>
    [HttpGet("task/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ImageMetadataDto>>>> GetTaskImages(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return validationResult;

            var result = await _imageService.GetTaskImagesAsync(taskId, pageNumber, pageSize);

            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Task images retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving task images for task {taskId}");
        }
    }

    /// <summary>
    /// Deletes an image
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage(Guid id)
    {
        try
        {
            var result = await _imageService.DeleteImageAsync(id);

            if (!result.Success)
                return CreateNotFoundResponse(result.Message);

            return CreateSuccessResponse(new object(), "Image deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"deleting image {id}");
        }
    }

    /// <summary>
    /// Bulk upload multiple images
    /// </summary>
    /// <param name="files">List of image files to upload</param>
    /// <param name="projectId">Project ID to associate with all images</param>
    /// <param name="taskId">Optional task ID to associate with all images</param>
    /// <param name="captureTimestamp">Optional timestamp when the images were captured</param>
    /// <param name="gpsLatitude">Optional GPS latitude</param>
    /// <param name="gpsLongitude">Optional GPS longitude</param>
    /// <param name="deviceModel">Optional device model</param>
    /// <param name="exifData">Optional EXIF data</param>
    /// <returns>List of uploaded image metadata</returns>
    [HttpPost("bulk-upload")]
    public async Task<ActionResult<ApiResponse<object>>> BulkUploadImages(
        List<IFormFile> files,
        [FromForm] Guid projectId,
        [FromForm] Guid? taskId = null,
        [FromForm] DateTime? captureTimestamp = null,
        [FromForm] double? gpsLatitude = null,
        [FromForm] double? gpsLongitude = null,
        [FromForm] string? deviceModel = null,
        [FromForm] string? exifData = null)
    {
        try
        {
            if (files == null || files.Count == 0)
                return CreateErrorResponse("No files provided", 400);

            if (files.Count > 10)
                return CreateErrorResponse("Maximum 10 files allowed per bulk upload", 400);

            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var uploadedByUserId))
                return CreateErrorResponse("Invalid user token", 401);

            var request = new ImageUploadRequest
            {
                ProjectId = projectId,
                TaskId = taskId,
                CaptureTimestamp = captureTimestamp,
                GPSLatitude = gpsLatitude,
                GPSLongitude = gpsLongitude,
                DeviceModel = deviceModel,
                EXIFData = exifData
            };

            var results = new List<ImageMetadataDto>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var result = await _imageService.UploadImageAsync(file, request, uploadedByUserId);
                    if (result.Success && result.Data != null)
                    {
                        results.Add(result.Data);
                    }
                    else
                    {
                        errors.Add($"{file.FileName}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                    errors.Add($"{file.FileName}: Upload failed");
                }
            }

            if (errors.Any() && !results.Any())
                return CreateErrorResponse("All uploads failed", 400);

            var response = new
            {
                successful = results,
                errors = errors.Any() ? errors : null,
                summary = new
                {
                    total = files.Count,
                    successful = results.Count,
                    failed = errors.Count
                }
            };

            return CreateSuccessResponse((object)response, "Bulk upload completed");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "bulk upload");
        }
    }

    /// <summary>
    /// Gets all images for a specific project with advanced querying capabilities
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="parameters">Advanced query parameters</param>
    /// <returns>Enhanced paginated list of project images with metadata</returns>
    [HttpGet("project/{projectId:guid}/advanced")]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<ImageMetadataDto>>>> GetProjectImagesAdvanced(
        Guid projectId,
        [FromQuery] ImageQueryParameters parameters)
    {
        try
        {
            // Validate pagination parameters
            if (parameters.PageNumber < 1)
                return CreateErrorResponse("Page number must be greater than 0", 400);

            if (parameters.PageSize < 1 || parameters.PageSize > 100)
                return CreateErrorResponse("Page size must be between 1 and 100", 400);

            // Set the project ID in parameters
            parameters.ProjectId = projectId;

            // Parse filters from query string if not already populated
            if (!parameters.Filters.Any() && Request.Query.Any())
            {
                parameters.Filters = ParseFiltersFromQuery(Request.Query);
            }

            var result = await _imageService.GetProjectImagesAsync(projectId, parameters);

            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Project images retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving project images with advanced query for project {projectId}");
        }
    }

    /// <summary>
    /// Gets project images with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("project/{projectId:guid}/rich")]
    public async Task<ActionResult<ApiResponseWithPagination<ImageMetadataDto>>> GetProjectImagesWithRichPagination(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? taskId = null,
        [FromQuery] string? contentType = null,
        [FromQuery] string? deviceModel = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "desc")
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(page, pageSize);
            if (validationResult != null)
                return validationResult;

            // Create query parameters
            var parameters = new ImageQueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                ProjectId = projectId,
                TaskId = taskId,
                ContentType = contentType,
                DeviceModel = deviceModel,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Get data using existing service
            var serviceResult = await _imageService.GetProjectImagesAsync(projectId, parameters);
            if (!serviceResult.Success)
                return CreateErrorResponse(serviceResult.Message, 400);

            // Build base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (taskId.HasValue) queryParams.Add("taskId", taskId.Value.ToString());
            if (!string.IsNullOrEmpty(contentType)) queryParams.Add("contentType", contentType);
            if (!string.IsNullOrEmpty(deviceModel)) queryParams.Add("deviceModel", deviceModel);
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add("sortBy", sortBy);
            if (!string.IsNullOrEmpty(sortOrder)) queryParams.Add("sortOrder", sortOrder);

            // Create rich paginated response using QueryService
            var response = _queryService.CreateRichPaginatedResponse(
                serviceResult.Data!.Items,
                serviceResult.Data.TotalCount,
                page,
                pageSize,
                baseUrl,
                queryParams,
                "Project images retrieved successfully"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving project images with rich pagination for project {projectId}");
        }
    }

    private List<FilterParameter> ParseFiltersFromQuery(IQueryCollection query)
    {
        var filters = new List<FilterParameter>();
        
        foreach (var kvp in query)
        {
            if (kvp.Key.StartsWith("filter."))
            {
                var parts = kvp.Key.Split('.');
                if (parts.Length >= 3)
                {
                    var field = parts[1];
                    var op = parts[2];
                    var value = kvp.Value.FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        filters.Add(new FilterParameter
                        {
                            Field = field,
                            Operator = op,
                            Value = value
                        });
                    }
                }
            }
        }
        
        return filters;
    }
}
