using dotnet_rest_api.Services;

namespace dotnet_rest_api.Services;

/// <summary>
/// Cloud storage service implementation
/// This is a basic file system implementation for development.
/// In production, replace with actual cloud storage providers like AWS S3, Azure Blob Storage, etc.
/// </summary>
public class CloudStorageService : ICloudStorageService
{
    private readonly ILogger<CloudStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public CloudStorageService(ILogger<CloudStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // For development, use local file storage
        _uploadPath = _configuration.GetValue<string>("FileStorage:UploadPath") ?? "uploads";
        _baseUrl = _configuration.GetValue<string>("FileStorage:BaseUrl") ?? "http://localhost:5001/files";
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var filePath = Path.Combine(_uploadPath, fileName);
            
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // Return the URL where the file can be accessed
            return $"{_baseUrl}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GetFileUrlAsync(string key, TimeSpan? expiration = null)
    {
        try
        {
            // For local file storage, return direct URL
            // In production with cloud storage, you might generate signed URLs here
            var filePath = Path.Combine(_uploadPath, key);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {key} not found");
            }

            return await Task.FromResult($"{_baseUrl}/{key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL for {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var filePath = Path.Combine(_uploadPath, key);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted file {Key}", key);
            }

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Key}", key);
            return false;
        }
    }
}

/// <summary>
/// AWS S3 implementation example (uncomment and configure for production use)
/// </summary>
/*
public class S3CloudStorageService : ICloudStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3CloudStorageService> _logger;

    public S3CloudStorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3CloudStorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration.GetValue<string>("AWS:S3:BucketName");
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(request);
            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GetFileUrlAsync(string key, TimeSpan? expiration = null)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(1)),
                Verb = HttpVerb.GET
            };

            return await Task.FromResult(_s3Client.GetPreSignedURL(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pre-signed URL for S3 object: {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {Key}", key);
            return false;
        }
    }
}
*/
