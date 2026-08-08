using System.Text.RegularExpressions;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.PackageRecognition.Data;
using Vega.PackageRecognition.Models;

namespace Vega.PackageRecognition;

public class PackageRecognitionService
{
    private readonly PackageRecognitionRuleRepository _recognitionRules;
    private readonly StencilTechnologyRuleRepository _technologyRules;
    private readonly PackageDefinitionRepository _packages;

    public PackageRecognitionService(
        PackageRecognitionRuleRepository? recognitionRules = null,
        StencilTechnologyRuleRepository? technologyRules = null,
        PackageDefinitionRepository? packages = null)
    {
        _recognitionRules = recognitionRules ?? new PackageRecognitionRuleRepository();
        _technologyRules = technologyRules ?? new StencilTechnologyRuleRepository();
        _packages = packages ?? new PackageDefinitionRepository();
    }

    public PackageRecognitionResult Recognize(PackageRecognitionInput input, PackageDefinition? manualOverride = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (manualOverride is not null) return Result(input, manualOverride, PackageRecognitionSource.Manual, 1, null);
        return RecognizeByFootprint(input)
            ?? RecognizeByPnPComment(input)
            ?? RecognizeByPartNumber(input)
            ?? RecognizeByGeometry(input)
            ?? Unknown(input);
    }

    public PackageRecognitionResult? RecognizeByFootprint(PackageRecognitionInput input) => RecognizePattern(input, input.FootprintName, PackageRecognitionSource.FootprintName, 0.98);
    public PackageRecognitionResult? RecognizeByPnPComment(PackageRecognitionInput input) => RecognizePattern(input, input.Comment, PackageRecognitionSource.PnPComment, 0.93);
    public PackageRecognitionResult? RecognizeByPartNumber(PackageRecognitionInput input) => RecognizePattern(input, input.ManufacturerPartNumber, PackageRecognitionSource.PartNumber, 0.82);

    public PackageRecognitionResult? RecognizeByGeometry(PackageRecognitionInput input)
    {
        var packageName = DetectGeometryPackage(input);
        if (packageName is null) return null;
        var package = _packages.GetAll().FirstOrDefault(item => item.PackageName.Equals(packageName, StringComparison.OrdinalIgnoreCase));
        return package is null ? null : Result(input, package, PackageRecognitionSource.Geometry, 0.65, "Geometry-based recognition must be reviewed.");
    }

    private PackageRecognitionResult? RecognizePattern(PackageRecognitionInput input, string value, PackageRecognitionSource source, double confidence)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var rule = _recognitionRules.GetAll().FirstOrDefault(rule => Matches(rule, value));
        if (rule is null) return null;
        var package = _recognitionRules.GetPackage(rule.PackageId);
        return package is null ? null : Result(input, package, source, confidence, null);
    }

    private PackageRecognitionResult Result(PackageRecognitionInput input, PackageDefinition package, PackageRecognitionSource source, double confidence, string? warning)
    {
        var technologyRule = _technologyRules.GetByPackage(package.PackageName).FirstOrDefault();
        var family = string.IsNullOrWhiteSpace(package.PackageFamily) ? InferFamily(package.PackageName) : package.PackageFamily;
        return new PackageRecognitionResult
        {
            RefDes = input.RefDes, DetectedPackage = package, PackageFamily = family, Confidence = confidence,
            RecognitionSource = source, MatchedRule = technologyRule,
            Warnings = string.IsNullOrWhiteSpace(warning) ? Array.Empty<string>() : [warning]
        };
    }

    private static bool Matches(PackageRecognitionRule rule, string value) => rule.MatchType switch
    {
        PackageRecognitionMatchType.Exact => value.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase),
        PackageRecognitionMatchType.Contains => value.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
        PackageRecognitionMatchType.Regex => Regex.IsMatch(value, rule.Pattern, RegexOptions.IgnoreCase),
        _ => false
    };

    private static string? DetectGeometryPackage(PackageRecognitionInput input)
    {
        if (input.PadCount == 2)
        {
            if (input.PadPitch <= 0.55) return "R0201";
            if (input.PadPitch <= 0.75) return "R0402";
            if (input.PadPitch <= 1.15) return "R0603";
            if (input.PadPitch <= 1.65) return "R0805";
            return "R1206";
        }
        if (input.PadCount >= 48) return input.PadPitch <= 0.5 ? "QFP" : "QFN";
        if (input.PadCount is >= 8 and <= 16) return input.PadPitch >= 1.0 ? "SO08" : "TSSOP";
        return null;
    }

    private static string InferFamily(string packageName)
    {
        if (packageName.StartsWith("R") || packageName.StartsWith("C") || packageName.StartsWith("L")) return "CHIP";
        if (packageName.StartsWith("SO")) return "SOIC";
        if (packageName.StartsWith("QFN")) return "QFN";
        if (packageName.StartsWith("QFP")) return "QFP";
        return "";
    }

    private static PackageRecognitionResult Unknown(PackageRecognitionInput input) => new()
    {
        RefDes = input.RefDes, Confidence = 0, RecognitionSource = PackageRecognitionSource.Manual,
        Warnings = ["Package was not recognized. Select a package manually."]
    };
}