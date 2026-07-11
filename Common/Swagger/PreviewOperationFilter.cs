using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace dotnet_rest_api.Common.Swagger;

/// <summary>
/// Flags any operation whose controller or action carries <see cref="PreviewAttribute"/> as
/// deprecated in Swagger and prepends a warning to its description, so clients know the
/// endpoint is stub-backed and returns fake/empty data (Phase 5 — stub reconciliation).
/// </summary>
public class PreviewOperationFilter : IOperationFilter
{
    private const string Warning =
        "⚠️ **PREVIEW / STUB** — this endpoint is not yet implemented and returns fake or empty data. Do not rely on it.";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo is null)
            return;

        var preview =
            (context.MethodInfo.GetCustomAttributes(typeof(PreviewAttribute), true).FirstOrDefault()
             ?? context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(PreviewAttribute), true).FirstOrDefault())
            as PreviewAttribute;

        if (preview is null)
            return;

        operation.Deprecated = true;

        var note = string.IsNullOrWhiteSpace(preview.Note) ? "" : $" ({preview.Note})";
        operation.Description = $"{Warning}{note}\n\n{operation.Description}".TrimEnd();
    }
}
