using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Infrastructure;

public interface ICalendarService
{
    Task<ServiceResult<object>> GetCalendarEventsAsync();
    Task<ServiceResult<object>> CreateCalendarEventAsync(object request);
}

public interface IImageService
{
    Task<ServiceResult<object>> GetImagesAsync();
    Task<ServiceResult<object>> UploadImageAsync(object request);
}

public interface IResourceService
{
    Task<ServiceResult<object>> GetResourcesAsync();
    Task<ServiceResult<object>> CreateResourceAsync(object request);
}

/// <summary>
/// Stub implementation of ICalendarService for development purposes
/// </summary>
public class StubCalendarService : ICalendarService
{
    public async Task<ServiceResult<object>> GetCalendarEventsAsync()
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("CalendarService not implemented yet");
    }

    public async Task<ServiceResult<object>> CreateCalendarEventAsync(object request)
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("CalendarService not implemented yet");
    }
}

/// <summary>
/// Stub implementation of IImageService for development purposes
/// </summary>
public class StubImageService : IImageService
{
    public async Task<ServiceResult<object>> GetImagesAsync()
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("ImageService not implemented yet");
    }

    public async Task<ServiceResult<object>> UploadImageAsync(object request)
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("ImageService not implemented yet");
    }
}

/// <summary>
/// Stub implementation of IResourceService for development purposes
/// </summary>
public class StubResourceService : IResourceService
{
    public async Task<ServiceResult<object>> GetResourcesAsync()
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("ResourceService not implemented yet");
    }

    public async Task<ServiceResult<object>> CreateResourceAsync(object request)
    {
        await Task.CompletedTask;
        return ServiceResult<object>.ErrorResult("ResourceService not implemented yet");
    }
}
