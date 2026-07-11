namespace dotnet_rest_api.Common;

/// <summary>
/// Marks a controller or action as stub-backed / preview: it is documented in Swagger
/// but returns fake or empty data because its service has no real implementation yet.
/// The <see cref="dotnet_rest_api.Common.Swagger.PreviewOperationFilter"/> renders these
/// operations as deprecated in Swagger with a warning note so clients know the data is not real.
/// Remove the attribute once the backing service is implemented (Phase 5).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PreviewAttribute : Attribute
{
    /// <summary>Optional note appended to the Swagger warning (e.g. "schema pending migration").</summary>
    public string? Note { get; }

    public PreviewAttribute(string? note = null) => Note = note;
}
