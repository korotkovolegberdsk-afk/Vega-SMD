namespace Vega.StencilUI.Models;

/// <summary>
/// Manufacturer dimensions and Vega-SMD recommendations are kept separate.
/// Manufacturer values must never be replaced by inferred or scaled values.
/// </summary>
public sealed record Sot23ApertureSpecification(
    decimal TopApertureWidthMm,
    decimal TopApertureHeightMm,
    decimal LowerHalfPitchMm,
    decimal VerticalCenterDistanceMm,
    string ManufacturerSource,
    string? SizeRecommendation,
    string? ShapeRecommendation,
    string? StencilThicknessRecommendation);
