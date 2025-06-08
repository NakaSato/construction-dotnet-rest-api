using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services;

namespace dotnet_rest_api.Services;

public class ImageService : IImageService
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IQueryService _queryService;
    private readonly ILogger<ImageService> _logger;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

    public ImageService(
        ApplicationDbContext context, 
        ICloudStorageService cloudStorageService,
        IQueryService queryService,
        ILogger<ImageService> logger)
    {
        _context = context;
        _cloudStorageService = cloudStorageService;
        _queryService = queryService;
        _logger = logger;
    }

    public async Task<ApiResponse<ImageMetadataDto>> UploadImageAsync(
        IFormFile file, 
        ImageUploadRequest request, 
        Guid uploadedByUserId)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<ImageMetadataDto>
                {
                    Success = false,
                    Message = "No file provided"
                };
            }

            if (file.Length > _maxFileSize)
            {
                return new ApiResponse<ImageMetadataDto>
                {
                    Success = false,
                    Message = $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB"
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return new ApiResponse<ImageMetadataDto>
                {
                    Success = false,
                    Message = $"File type {extension} is not allowed"
                };
            }

            // Validate references exist - ProjectId is required (not nullable)
            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId);
            if (!projectExists)
            {
                return new ApiResponse<ImageMetadataDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            // TaskId is optional
            if (request.TaskId.HasValue)
            {
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.TaskId == request.TaskId.Value);
                if (!taskExists)
                {
                    return new ApiResponse<ImageMetadataDto>
                    {
                        Success = false,
                        Message = "Task not found"
                    };
                }
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Upload to cloud storage
            string cloudUrl;
            using (var stream = file.OpenReadStream())
            {
                cloudUrl = await _cloudStorageService.UploadFileAsync(stream, uniqueFileName, file.ContentType);
            }

            // Create image metadata
            var imageMetadata = new ImageMetadata
            {
                ImageId = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                TaskId = request.TaskId,
                UploadedByUserId = uploadedByUserId,
                CloudStorageKey = uniqueFileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeInBytes = file.Length,
                UploadTimestamp = DateTime.UtcNow,
                CaptureTimestamp = request.CaptureTimestamp,
                GPSLatitude = request.GPSLatitude,
                GPSLongitude = request.GPSLongitude,
                DeviceModel = request.DeviceModel,
                EXIFData = request.EXIFData
            };

            _context.ImageMetadata.Add(imageMetadata);
            await _context.SaveChangesAsync();

            var imageDto = await MapToImageMetadataDto(imageMetadata);
            return new ApiResponse<ImageMetadataDto>
            {
                Success = true,
                Data = imageDto,
                Message = "Image uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return new ApiResponse<ImageMetadataDto>
            {
                Success = false,
                Message = "An error occurred while uploading the image"
            };
        }
    }

    public async Task<ApiResponse<ImageMetadataDto>> GetImageMetadataAsync(Guid imageId)
    {
        try
        {
            var image = await _context.ImageMetadata
                .Include(i => i.Project)
                .Include(i => i.Task)
                .Include(i => i.UploadedByUser)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(i => i.ImageId == imageId);

            if (image == null)
            {
                return new ApiResponse<ImageMetadataDto>
                {
                    Success = false,
                    Message = "Image not found"
                };
            }

            var imageDto = await MapToImageMetadataDto(image);
            return new ApiResponse<ImageMetadataDto>
            {
                Success = true,
                Data = imageDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image metadata {ImageId}", imageId);
            return new ApiResponse<ImageMetadataDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the image metadata"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(
        Guid projectId, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        try
        {
            var query = _context.ImageMetadata
                .Include(i => i.Project)
                .Include(i => i.Task)
                .Include(i => i.UploadedByUser)
                .ThenInclude(u => u.Role)
                .Where(i => i.ProjectId == projectId)
                .OrderByDescending(i => i.UploadTimestamp);

            var totalCount = await query.CountAsync();
            var images = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var imageDtos = new List<ImageMetadataDto>();
            foreach (var image in images)
            {
                imageDtos.Add(await MapToImageMetadataDto(image));
            }

            var result = new PagedResult<ImageMetadataDto>
            {
                Items = imageDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<ImageMetadataDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project images {ProjectId}", projectId);
            return new ApiResponse<PagedResult<ImageMetadataDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving project images"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<ImageMetadataDto>>> GetTaskImagesAsync(
        Guid taskId, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        try
        {
            var query = _context.ImageMetadata
                .Include(i => i.Project)
                .Include(i => i.Task)
                .Include(i => i.UploadedByUser)
                .ThenInclude(u => u.Role)
                .Where(i => i.TaskId == taskId)
                .OrderByDescending(i => i.UploadTimestamp);

            var totalCount = await query.CountAsync();
            var images = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var imageDtos = new List<ImageMetadataDto>();
            foreach (var image in images)
            {
                imageDtos.Add(await MapToImageMetadataDto(image));
            }

            var result = new PagedResult<ImageMetadataDto>
            {
                Items = imageDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<ImageMetadataDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task images {TaskId}", taskId);
            return new ApiResponse<PagedResult<ImageMetadataDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving task images"
            };
        }
    }

    public async Task<ApiResponse<string>> GetImageUrlAsync(Guid imageId)
    {
        try
        {
            var image = await _context.ImageMetadata.FindAsync(imageId);
            if (image == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Image not found"
                };
            }

            var url = await _cloudStorageService.GetFileUrlAsync(image.CloudStorageKey, TimeSpan.FromHours(1));
            return new ApiResponse<string>
            {
                Success = true,
                Data = url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image URL {ImageId}", imageId);
            return new ApiResponse<string>
            {
                Success = false,
                Message = "An error occurred while getting the image URL"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteImageAsync(Guid imageId)
    {
        try
        {
            var image = await _context.ImageMetadata.FindAsync(imageId);
            if (image == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Image not found"
                };
            }

            // Delete from cloud storage
            await _cloudStorageService.DeleteFileAsync(image.CloudStorageKey);

            // Delete metadata from database
            _context.ImageMetadata.Remove(image);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Image deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the image"
            };
        }
    }

    // Advanced querying method
    public async Task<ApiResponse<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters)
    {
        try
        {
            var baseQuery = _context.ImageMetadata
                .Include(i => i.UploadedByUser)
                .ThenInclude(u => u.Role)
                .Include(i => i.Project)
                .Include(i => i.Task)
                .Where(i => i.ProjectId == projectId)
                .AsQueryable();

            // Apply entity-specific filters first
            var filteredQuery = ApplyImageFilters(baseQuery, parameters);

            // Use the generic query service for advanced filtering, sorting, and pagination
            var result = await _queryService.ExecuteQueryAsync(filteredQuery, parameters);

            // Convert entities to DTOs
            var dtoItems = new List<ImageMetadataDto>();
            foreach (var item in result.Items)
            {
                dtoItems.Add(await MapToImageMetadataDto(item));
            }
            
            // Apply field selection if requested
            var finalItems = string.IsNullOrEmpty(parameters.Fields) 
                ? dtoItems.Cast<object>().ToList()
                : _queryService.ApplyFieldSelection(dtoItems, parameters.Fields);

            var enhancedResult = new EnhancedPagedResult<ImageMetadataDto>
            {
                Items = string.IsNullOrEmpty(parameters.Fields) 
                    ? dtoItems 
                    : finalItems.Cast<ImageMetadataDto>().ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder,
                RequestedFields = string.IsNullOrEmpty(parameters.Fields) 
                    ? new List<string>() 
                    : parameters.Fields.Split(',').Select(f => f.Trim()).ToList(),
                Metadata = result.Metadata
            };

            return new ApiResponse<EnhancedPagedResult<ImageMetadataDto>>
            {
                Success = true,
                Data = enhancedResult
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EnhancedPagedResult<ImageMetadataDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving images",
                Errors = new List<string> { ex.Message }
            };
        }
    }
    
    private IQueryable<ImageMetadata> ApplyImageFilters(IQueryable<ImageMetadata> query, ImageQueryParameters parameters)
    {
        if (parameters.TaskId.HasValue)
        {
            query = query.Where(i => i.TaskId == parameters.TaskId.Value);
        }
        
        if (parameters.UploadedById.HasValue)
        {
            query = query.Where(i => i.UploadedByUserId == parameters.UploadedById.Value);
        }
        
        if (parameters.UploadedAfter.HasValue)
        {
            query = query.Where(i => i.UploadTimestamp >= parameters.UploadedAfter.Value);
        }
        
        if (parameters.UploadedBefore.HasValue)
        {
            query = query.Where(i => i.UploadTimestamp <= parameters.UploadedBefore.Value);
        }
        
        if (parameters.CapturedAfter.HasValue)
        {
            query = query.Where(i => i.CaptureTimestamp >= parameters.CapturedAfter.Value);
        }
        
        if (parameters.CapturedBefore.HasValue)
        {
            query = query.Where(i => i.CaptureTimestamp <= parameters.CapturedBefore.Value);
        }
        
        if (!string.IsNullOrEmpty(parameters.ContentType))
        {
            query = query.Where(i => i.ContentType.Contains(parameters.ContentType));
        }
        
        if (!string.IsNullOrEmpty(parameters.DeviceModel))
        {
            query = query.Where(i => i.DeviceModel != null && i.DeviceModel.Contains(parameters.DeviceModel));
        }
        
        if (parameters.MinFileSize.HasValue)
        {
            query = query.Where(i => i.FileSizeInBytes >= parameters.MinFileSize.Value);
        }
        
        if (parameters.MaxFileSize.HasValue)
        {
            query = query.Where(i => i.FileSizeInBytes <= parameters.MaxFileSize.Value);
        }
        
        return query;
    }

    private async Task<ImageMetadataDto> MapToImageMetadataDto(ImageMetadata image)
    {
        return new ImageMetadataDto
        {
            ImageId = image.ImageId,
            ProjectId = image.ProjectId,
            TaskId = image.TaskId,
            OriginalFileName = image.OriginalFileName,
            ContentType = image.ContentType,
            FileSizeInBytes = image.FileSizeInBytes,
            UploadTimestamp = image.UploadTimestamp,
            CaptureTimestamp = image.CaptureTimestamp,
            GPSLatitude = image.GPSLatitude,
            GPSLongitude = image.GPSLongitude,
            DeviceModel = image.DeviceModel,
            ImageUrl = await _cloudStorageService.GetFileUrlAsync(image.CloudStorageKey, TimeSpan.FromHours(1)),
            UploadedBy = image.UploadedByUser != null ? new UserDto
            {
                UserId = image.UploadedByUser.UserId,
                Username = image.UploadedByUser.Username,
                Email = image.UploadedByUser.Email,
                FullName = image.UploadedByUser.FullName,
                RoleName = image.UploadedByUser.Role?.RoleName ?? "Unknown",
                IsActive = image.UploadedByUser.IsActive
            } : new UserDto()
        };
    }
}
