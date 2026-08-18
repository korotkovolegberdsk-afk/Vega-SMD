using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public sealed class PackageManufacturerDrawingGeometryValidationResult
{
    public List<string> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;
}

public sealed class PackageManufacturerDrawingGeometryService
{
    private readonly PackageManufacturerDrawingGeometryRepository _repository = new();

    public PackageManufacturerDrawingGeometry? GetVerifiedGeometry(PackageDefinition package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var geometry = _repository.GetCurrentVerified(package.Id);
        return geometry is not null && ValidateForRendering(package, geometry).IsValid ? geometry : null;
    }

    public bool HasVerifiedDrawing(PackageDefinition package) => GetVerifiedGeometry(package) is not null;

    public PackageManufacturerDrawingGeometryValidationResult ValidateForRendering(PackageDefinition package, PackageManufacturerDrawingGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(package); ArgumentNullException.ThrowIfNull(geometry);
        var result = new PackageManufacturerDrawingGeometryValidationResult();
        if (geometry.PackageDefinitionId != package.Id) result.Errors.Add("PackageDefinitionId does not match.");
        if (!string.Equals(geometry.VerificationStatus, "Verified", StringComparison.OrdinalIgnoreCase)) result.Errors.Add("Geometry is not verified.");
        if (!geometry.IsCurrent) result.Errors.Add("Geometry is not current.");
        var projections = geometry.Projections.Where(p => p.IsAvailable).ToList();
        var top = projections.FirstOrDefault(p => Is(p.ProjectionType, "Top"));
        if (top is null) result.Errors.Add("Verified Top projection is required.");
        var sideDeclared = geometry.Projections.Any(p => Is(p.ProjectionType, "Side"));
        if (sideDeclared && projections.All(p => !Is(p.ProjectionType, "Side"))) result.Errors.Add("Declared Side projection is unavailable.");
        foreach (var projection in projections)
        {
            if (!Finite(projection.OriginX) || !Finite(projection.OriginY)) result.Errors.Add($"Projection {projection.ProjectionType} has non-finite coordinates.");
        }
        foreach (var lead in geometry.Leads)
        {
            if (lead.LeadNumber <= 0) result.Errors.Add("LeadNumber must be greater than zero.");
            if (!projections.Any(p => Is(p.ProjectionType, lead.ProjectionType))) result.Errors.Add($"Lead {lead.LeadNumber} references a missing projection.");
            if (!Finite(lead.RootX) || !Finite(lead.RootY) || !Finite(lead.ContactX) || !Finite(lead.ContactY)) result.Errors.Add($"Lead {lead.LeadNumber} has non-finite coordinates.");
            if (lead.Width < 0 || lead.Thickness < 0 || lead.Pitch < 0) result.Errors.Add($"Lead {lead.LeadNumber} has negative dimensions.");
            if (string.IsNullOrWhiteSpace(lead.ProfileGeometry)) result.Errors.Add($"Lead {lead.LeadNumber} has no verified profile geometry.");
        }
        foreach (var duplicate in geometry.Leads.GroupBy(x => x.ProjectionType, StringComparer.OrdinalIgnoreCase).SelectMany(x => x.GroupBy(y => y.LeadNumber).Where(y => y.Count() > 1))) result.Errors.Add($"Duplicate LeadNumber {duplicate.Key} in projection.");
        foreach (var dimension in geometry.Dimensions)
        {
            if (dimension.NominalValue is { } n && (!Finite(n) || n < 0) || dimension.MinValue is { } min && (!Finite(min) || min < 0) || dimension.MaxValue is { } max && (!Finite(max) || max < 0)) result.Errors.Add($"Dimension {dimension.DimensionKey} has invalid values.");
            if (dimension.MinValue is { } lo && dimension.MaxValue is { } hi && lo > hi) result.Errors.Add($"Dimension {dimension.DimensionKey} has an invalid tolerance range.");
        }
        foreach (var marker in geometry.Pin1Markers)
        {
            if (!projections.Any(p => Is(p.ProjectionType, marker.ProjectionType))) result.Errors.Add($"Pin1 marker references a missing projection.");
            if (!Finite(marker.PositionX) || !Finite(marker.PositionY) || marker.Width < 0 || marker.Height < 0) result.Errors.Add("Pin1 marker has invalid geometry.");
        }
        return result;
    }

    private static bool Is(string left, string right) => string.Equals(left?.Trim(), right, StringComparison.OrdinalIgnoreCase);
    private static bool Finite(double value) => double.IsFinite(value);
}
