using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Interface for document management service
/// </summary>
public interface IDocumentService
{
    Task<ServiceResult<EnhancedPagedResult<DocumentDto>>> GetDocumentsAsync(DocumentQueryParameters parameters);
    Task<ServiceResult<DocumentDto>> GetDocumentByIdAsync(Guid documentId);
    Task<ServiceResult<DocumentDto>> CreateDocumentAsync(CreateDocumentRequest request);
    Task<ServiceResult<DocumentDto>> UpdateDocumentAsync(Guid documentId, UpdateDocumentRequest request);
    Task<ServiceResult<bool>> DeleteDocumentAsync(Guid documentId);
}

/// <summary>
/// Stub implementation of document service for development
/// </summary>
public class StubDocumentService : IDocumentService
{
    public Task<ServiceResult<EnhancedPagedResult<DocumentDto>>> GetDocumentsAsync(DocumentQueryParameters parameters)
    {
        var result = new EnhancedPagedResult<DocumentDto>
        {
            Items = new List<DocumentDto>(),
            TotalCount = 0,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
        return Task.FromResult(ServiceResult<EnhancedPagedResult<DocumentDto>>.SuccessResult(result));
    }

    public Task<ServiceResult<DocumentDto>> GetDocumentByIdAsync(Guid documentId)
    {
        return Task.FromResult(ServiceResult<DocumentDto>.ErrorResult("Document not found"));
    }

    public Task<ServiceResult<DocumentDto>> CreateDocumentAsync(CreateDocumentRequest request)
    {
        var doc = new DocumentDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            ProjectId = request.ProjectId,
            Tags = request.Tags,
            Status = DocumentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
        };
        return Task.FromResult(ServiceResult<DocumentDto>.SuccessResult(doc));
    }

    public Task<ServiceResult<DocumentDto>> UpdateDocumentAsync(Guid documentId, UpdateDocumentRequest request)
    {
        return Task.FromResult(ServiceResult<DocumentDto>.ErrorResult("Document not found"));
    }

    public Task<ServiceResult<bool>> DeleteDocumentAsync(Guid documentId)
    {
        return Task.FromResult(ServiceResult<bool>.ErrorResult("Document not found"));
    }
}
