namespace Vega.Models.MasterLibrary;

public sealed class PackageValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];
}

public sealed class PackageDeleteResult
{
    public bool CanDelete { get; init; }
    public string Reason { get; init; } = string.Empty;
}