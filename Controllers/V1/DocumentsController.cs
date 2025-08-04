using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Services.Users;
using dotnet_rest_api.Services.Tasks;
using dotnet_rest_api.Services.Projects;
using dotnet_rest_api.Services.MasterPlans;
using dotnet_rest_api.Services.WBS;
using dotnet_rest_api.Services.Infrastructure;
using dotnet_rest_api.Attributes;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Top-level documents controller following canonical URI structure
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documents")]
[Authorize]
public class DocumentsController : BaseApiController
{
    // private readonly IDocumentService _documentService;
    private readonly IQueryService _queryService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        // IDocumentService documentService,
        IQueryService queryService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents with filtering (top-level collection)
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<DocumentDto>>>> GetDocuments([FromQuery] DocumentQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetDocuments", parameters);

            // Apply dynamic filters from query string
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _documentService.GetDocumentsAsync(parameters);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<DocumentDto>>(_logger, ex, "retrieving documents");
        }
    }

    /// <summary>
    /// Get a specific document by ID (canonical resource)
    /// </summary>
    [HttpGet("{documentId:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GetDocument(Guid documentId)
    {
        try
        {
            LogControllerAction(_logger, "GetDocument", new { documentId });

            var result = await _documentService.GetDocumentByIdAsync(documentId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DocumentDto>(_logger, ex, "retrieving document");
        }
    }

    /// <summary>
    /// Create a new document
    /// Available to: Administrator, ProjectManager, Technician
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Technician")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateDocument", request);

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<DocumentDto> { Success = false, Message = "Invalid input data" });

            var result = await _documentService.CreateDocumentAsync(request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DocumentDto>(_logger, ex, "creating document");
        }
    }

    /// <summary>
    /// Update a document (canonical resource)
    /// Available to: Administrator, ProjectManager, Document Creator
    /// </summary>
    [HttpPatch("{documentId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager,Technician")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> UpdateDocument(Guid documentId, [FromBody] UpdateDocumentRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdateDocument", new { documentId, request });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<DocumentDto> { Success = false, Message = "Invalid input data" });

            var result = await _documentService.UpdateDocumentAsync(documentId, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DocumentDto>(_logger, ex, "updating document");
        }
    }

    /// <summary>
    /// Delete a document (canonical resource)
    /// Available to: Administrator, Document Creator
    /// </summary>
    [HttpDelete("{documentId:guid}")]
    [Authorize(Roles = "Administrator")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDocument(Guid documentId)
    {
        try
        {
            LogControllerAction(_logger, "DeleteDocument", new { documentId });

            var result = await _documentService.DeleteDocumentAsync(documentId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "deleting document");
        }
    }
}
