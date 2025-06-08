using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing images in solar projects
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/images")]
[Authorize]
public class ImagesController : ControllerBase
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
    public async Task<ActionResult<ImageMetadataDto>> UploadImage(
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
                return BadRequest("No file uploaded");

            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var uploadedByUserId))
                return Unauthorized("Invalid user token");

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
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetImage), new { id = result.Data!.ImageId }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets image metadata by ID
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>Image metadata</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ImageMetadataDto>> GetImage(Guid id)
    {
        try
        {
            var result = await _imageService.GetImageMetadataAsync(id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image {ImageId}", id);
            return StatusCode(500, "An error occurred while retrieving the image");
        }
    }

    /// <summary>
    /// Gets a direct URL to view/download the image
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>Direct image URL</returns>
    [HttpGet("{id:guid}/url")]
    public async Task<ActionResult<string>> GetImageUrl(Guid id)
    {
        try
        {
            var result = await _imageService.GetImageUrlAsync(id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(new { url = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image URL {ImageId}", id);
            return StatusCode(500, "An error occurred while getting the image URL");
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
    public async Task<ActionResult<PagedResult<ImageMetadataDto>>> GetProjectImages(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100");

            var result = await _imageService.GetProjectImagesAsync(projectId, pageNumber, pageSize);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project images {ProjectId}", projectId);
            return StatusCode(500, "An error occurred while retrieving project images");
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
    public async Task<ActionResult<PagedResult<ImageMetadataDto>>> GetTaskImages(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100");

            var result = await _imageService.GetTaskImagesAsync(taskId, pageNumber, pageSize);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task images {TaskId}", taskId);
            return StatusCode(500, "An error occurred while retrieving task images");
        }
    }

    /// <summary>
    /// Deletes an image
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        try
        {
            var result = await _imageService.DeleteImageAsync(id);

            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", id);
            return StatusCode(500, "An error occurred while deleting the image");
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
    public async Task<ActionResult<IEnumerable<ImageMetadataDto>>> BulkUploadImages(
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
                return BadRequest("No files provided");

            if (files.Count > 10)
                return BadRequest("Maximum 10 files allowed per bulk upload");

            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var uploadedByUserId))
                return Unauthorized("Invalid user token");

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
                return BadRequest(new { message = "All uploads failed", errors });

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

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk upload");
            return StatusCode(500, "An error occurred during bulk upload");
        }
    }

    /// <summary>
    /// Gets all images for a specific project with advanced querying capabilities
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="parameters">Advanced query parameters</param>
    /// <returns>Enhanced paginated list of project images with metadata</returns>
    [HttpGet("project/{projectId:guid}/advanced")]
    public async Task<ActionResult<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAdvanced(
        Guid projectId,
        [FromQuery] ImageQueryParameters parameters)
    {
        try
        {
            // Validate pagination parameters
            if (parameters.PageNumber < 1)
                return BadRequest("Page number must be greater than 0");

            if (parameters.PageSize < 1 || parameters.PageSize > 100)
                return BadRequest("Page size must be between 1 and 100");

            // Set the project ID in parameters
            parameters.ProjectId = projectId;

            // Parse filters from query string if not already populated
            if (!parameters.Filters.Any() && Request.Query.Any())
            {
                parameters.Filters = ParseFiltersFromQuery(Request.Query);
            }

            var result = await _imageService.GetProjectImagesAsync(projectId, parameters);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project images with advanced query {ProjectId}", projectId);
            return StatusCode(500, "An error occurred while retrieving project images");
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
            // Validate pagination parameters
            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

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
                return BadRequest(serviceResult.Message);

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
            _logger.LogError(ex, "Error retrieving project images with rich pagination for project {ProjectId}", projectId);
            return StatusCode(500, new ApiResponseWithPagination<ImageMetadataDto>
            {
                Success = false,
                Message = "An error occurred while retrieving project images",
                Errors = new List<string> { ex.Message }
            });
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
