using Vega.Models.MasterLibrary;

namespace Vega.PackageRecognition.Models;

public enum PackageRecognitionSource
{
    FootprintName,
    PartNumber,
    PnPComment,
    Geometry,
    Manual
}

public class PackageRecognitionResult
{
    public string RefDes { get; init; } = "";
    public PackageDefinition? DetectedPackage { get; init; }
    public string PackageFamily { get; init; } = "";
    public double Confidence { get; init; }
    public PackageRecognitionSource RecognitionSource { get; init; }
    public StencilTechnologyRule? MatchedRule { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}