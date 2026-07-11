using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IImageService
{
    Task<ServiceResult<object>> GetImagesAsync();
    Task<ServiceResult<ImageMetadataDto>> UploadImageAsync(IFormFile file, ImageUploadRequest request, Guid uploadedByUserId);
    Task<ServiceResult<ImageMetadataDto>> GetImageMetadataAsync(Guid imageId);
    Task<ServiceResult<string>> GetImageUrlAsync(Guid imageId);
    Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, int pageNumber, int pageSize);
    Task<ServiceResult<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters);
    Task<ServiceResult<bool>> DeleteImageAsync(Guid imageId);
}

/// <summary>
/// Real implementation of <see cref="IImageService"/>. Replaces the former
/// StubImageService. Stores uploaded files on the local filesystem under
/// <c>uploads/images/{projectId}/</c> — exposed publicly through the <c>/files</c>
/// static mapping — and persists metadata to the Images table. The
/// <see cref="ImageMetadata.CloudStorageKey"/> holds the relative storage key;
/// the public URL is derived from it.
/// </summary>
public class ImageService : IImageService
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ImageService> _logger;

    public ImageService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<ImageService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<object>> GetImagesAsync()
    {
        var images = await BuildBaseQuery()
            .OrderByDescending(i => i.UploadTimestamp)
            .Take(100)
            .ToListAsync();
        return ServiceResult<object>.SuccessResult(ToDtos(images), "Images retrieved successfully");
    }

    public async Task<ServiceResult<ImageMetadataDto>> UploadImageAsync(IFormFile file, ImageUploadRequest request, Guid uploadedByUserId)
    {
        if (file == null || file.Length == 0)
            return ServiceResult<ImageMetadataDto>.ErrorResult("No file provided");
        if (file.Length > MaxFileSizeBytes)
            return ServiceResult<ImageMetadataDto>.ErrorResult($"File exceeds the {MaxFileSizeBytes / (1024 * 1024)}MB limit");

        if (!await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
            return ServiceResult<ImageMetadataDto>.ErrorResult("Project not found");
        if (request.TaskId.HasValue &&
            !await _context.ProjectTasks.AnyAsync(t => t.TaskId == request.TaskId.Value))
            return ServiceResult<ImageMetadataDto>.ErrorResult("Task not found");

        var relativeDir = Path.Combine("uploads", "images", request.ProjectId.ToString());
        var absoluteDir = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var safeName = Path.GetFileName(file.FileName);
        var storedName = $"{Guid.NewGuid()}_{safeName}";
        var absolutePath = Path.Combine(absoluteDir, storedName);

        await using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Storage key is relative to uploads/ so the /files static mapping serves it.
        var storageKey = $"images/{request.ProjectId}/{storedName}";

        var image = new ImageMetadata
        {
            ImageId = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            UploadedByUserId = uploadedByUserId,
            CloudStorageKey = storageKey,
            OriginalFileName = safeName,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSizeInBytes = file.Length,
            UploadTimestamp = DateTime.UtcNow,
            CaptureTimestamp = request.CaptureTimestamp,
            GPSLatitude = request.GPSLatitude,
            GPSLongitude = request.GPSLongitude,
            DeviceModel = request.DeviceModel,
            // EXIFData maps to a jsonb column; only store content that is valid JSON.
            EXIFData = IsJson(request.EXIFData) ? request.EXIFData : null
        };

        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var created = await BuildBaseQuery().FirstOrDefaultAsync(i => i.ImageId == image.ImageId) ?? image;
        return ServiceResult<ImageMetadataDto>.SuccessResult(ToDto(created), "Image uploaded successfully");
    }

    public async Task<ServiceResult<ImageMetadataDto>> GetImageMetadataAsync(Guid imageId)
    {
        var image = await BuildBaseQuery().FirstOrDefaultAsync(i => i.ImageId == imageId);
        if (image == null)
            return ServiceResult<ImageMetadataDto>.ErrorResult("Image not found");
        return ServiceResult<ImageMetadataDto>.SuccessResult(ToDto(image), "Image retrieved successfully");
    }

    public async Task<ServiceResult<string>> GetImageUrlAsync(Guid imageId)
    {
        var key = await _context.Images
            .Where(i => i.ImageId == imageId)
            .Select(i => i.CloudStorageKey)
            .FirstOrDefaultAsync();
        if (key == null)
            return ServiceResult<string>.ErrorResult("Image not found");
        return ServiceResult<string>.SuccessResult(BuildUrl(key), "Image URL retrieved successfully");
    }

    public async Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize is < 1 or > 100) pageSize = 10;

        var query = BuildBaseQuery().Where(i => i.ProjectId == projectId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.UploadTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<ImageMetadataDto>
        {
            Items = ToDtos(items),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return ServiceResult<PagedResult<ImageMetadataDto>>.SuccessResult(result, "Project images retrieved successfully");
    }

    public async Task<ServiceResult<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters)
    {
        var query = BuildBaseQuery().Where(i => i.ProjectId == projectId);

        if (parameters.TaskId.HasValue)
            query = query.Where(i => i.TaskId == parameters.TaskId.Value);
        if (parameters.UploadedById.HasValue)
            query = query.Where(i => i.UploadedByUserId == parameters.UploadedById.Value);
        if (!string.IsNullOrWhiteSpace(parameters.ContentType))
            query = query.Where(i => i.ContentType == parameters.ContentType);
        if (!string.IsNullOrWhiteSpace(parameters.DeviceModel))
            query = query.Where(i => i.DeviceModel == parameters.DeviceModel);
        if (parameters.UploadedAfter.HasValue)
            query = query.Where(i => i.UploadTimestamp >= parameters.UploadedAfter.Value);
        if (parameters.UploadedBefore.HasValue)
            query = query.Where(i => i.UploadTimestamp <= parameters.UploadedBefore.Value);
        if (parameters.CapturedAfter.HasValue)
            query = query.Where(i => i.CaptureTimestamp >= parameters.CapturedAfter.Value);
        if (parameters.CapturedBefore.HasValue)
            query = query.Where(i => i.CaptureTimestamp <= parameters.CapturedBefore.Value);
        if (parameters.MinFileSize.HasValue)
            query = query.Where(i => i.FileSizeInBytes >= parameters.MinFileSize.Value);
        if (parameters.MaxFileSize.HasValue)
            query = query.Where(i => i.FileSizeInBytes <= parameters.MaxFileSize.Value);

        query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<ImageMetadataDto>
        {
            Items = ToDtos(items),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };
        return ServiceResult<EnhancedPagedResult<ImageMetadataDto>>.SuccessResult(result, "Project images retrieved successfully");
    }

    public async Task<ServiceResult<bool>> DeleteImageAsync(Guid imageId)
    {
        var image = await _context.Images.FirstOrDefaultAsync(i => i.ImageId == imageId);
        if (image == null)
            return ServiceResult<bool>.ErrorResult("Image not found");

        // Best-effort removal of the backing file; a missing file must not block the
        // metadata delete.
        try
        {
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", image.CloudStorageKey.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absolutePath))
                File.Delete(absolutePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete backing file for image {ImageId}", imageId);
        }

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true, "Image deleted successfully");
    }

    // -------------------------------------------------------------- Helpers --

    private IQueryable<ImageMetadata> BuildBaseQuery()
        => _context.Images.Include(i => i.UploadedByUser).AsQueryable();

    private ImageMetadataDto ToDto(ImageMetadata image)
    {
        var dto = _mapper.Map<ImageMetadataDto>(image);
        dto.ImageUrl = BuildUrl(image.CloudStorageKey);
        return dto;
    }

    private List<ImageMetadataDto> ToDtos(IEnumerable<ImageMetadata> images)
        => images.Select(ToDto).ToList();

    // Static files map /files -> uploads/, so a storage key relative to uploads/
    // is served at /files/{key}.
    private static string BuildUrl(string storageKey) => $"/files/{storageKey}";

    private static bool IsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var trimmed = value.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }

    private static IQueryable<ImageMetadata> ApplySorting(IQueryable<ImageMetadata> query, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "filename" or "originalfilename" => descending ? query.OrderByDescending(i => i.OriginalFileName) : query.OrderBy(i => i.OriginalFileName),
            "filesize" or "filesizeinbytes" => descending ? query.OrderByDescending(i => i.FileSizeInBytes) : query.OrderBy(i => i.FileSizeInBytes),
            "capturetimestamp" => descending ? query.OrderByDescending(i => i.CaptureTimestamp) : query.OrderBy(i => i.CaptureTimestamp),
            "contenttype" => descending ? query.OrderByDescending(i => i.ContentType) : query.OrderBy(i => i.ContentType),
            "uploadtimestamp" => descending ? query.OrderByDescending(i => i.UploadTimestamp) : query.OrderBy(i => i.UploadTimestamp),
            _ => query.OrderByDescending(i => i.UploadTimestamp)
        };
    }
}
