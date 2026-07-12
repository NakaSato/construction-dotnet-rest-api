using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Image DTOs
public class ImageMetadataDto
{
    public Guid ImageId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public DateTime UploadTimestamp { get; set; }
    public DateTime? CaptureTimestamp { get; set; }
    public double? GPSLatitude { get; set; }
    public double? GPSLongitude { get; set; }
    public string? DeviceModel { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public UserDto UploadedBy { get; set; } = null!;
}

public class ImageUploadRequest
{
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime? CaptureTimestamp { get; set; }
    public double? GPSLatitude { get; set; }
    public double? GPSLongitude { get; set; }
    public string? DeviceModel { get; set; }
    public string? EXIFData { get; set; }
}


