namespace Vega.Models.MasterLibrary;

/// <summary>Alternative customer, CAD, IPC or equipment name for a single package definition.</summary>
public sealed class PackageAlias
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string AliasType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}