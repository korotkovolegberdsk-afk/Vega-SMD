using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.PackageDrawing;

public sealed record PackageDimensionDefinition(string Key, string DisplayName, string SourceProperty, string Unit = "mm");

public sealed record YamahaParameterDefinition(string Key, string Name, string SourceProperty, string Unit = "mm", bool IsCount = false, bool IsRecognitionParameter = false, bool IsOptional = true);

public sealed record PackageDrawingTemplate(string Name, string AlignmentGroup, string AlignmentType, IReadOnlyList<PackageDimensionDefinition> Dimensions, IReadOnlyList<YamahaParameterDefinition>? YamahaParameters = null, PackageTopologyType TopologyType = PackageTopologyType.Generic, bool HasTab = false, bool HasExposedPad = false, bool HasBalls = false, bool HasTerminations = false)
{
    public IReadOnlyList<YamahaParameterDefinition> Parameters => YamahaParameters ?? Dimensions.Select(d => new YamahaParameterDefinition(d.Key, d.DisplayName, d.SourceProperty, d.Unit, d.Unit.Length == 0)).ToArray();
}

public static class PackageDrawingTemplateCatalog
{
    public static PackageDrawingTemplate Resolve(PackageDefinition package)
    {
        var family = (package.PackageFamily ?? string.Empty).Trim().ToUpperInvariant();
        if (family == "DPAK") return Dpak;
        if (family == "D2PAK") return D2Pak;
        if (family == "D3PAK") return D3Pak;
        if (family == "SOT" && package.PackageName.StartsWith("SOT223", StringComparison.OrdinalIgnoreCase)) return Sot223;
        if (family == "SOT") return Sot;
        if (family is "BGA" or "FBGA" or "CSP" or "WLCSP" or "LGA") return Bga;
        if (family is "QFN" or "DFN" or "SON") return Qfn;
        if (family is "QFP" or "LQFP" or "TQFP" or "PQFP") return Qfp;
        if (family is "CHIP" or "MELF") return Chip;
        return Sop;
    }

    public static PackageDimensionDefinition? Find(PackageDefinition package, string sourceProperty) => Resolve(package).Dimensions.FirstOrDefault(d => d.SourceProperty == sourceProperty);

    private static readonly PackageDrawingTemplate Dpak = new("DPAK", "Power", "TO-252",
    [
        new("D", "Body Length", "BodyLength"), new("E", "Body Width", "BodyWidth"), new("A", "Height", "Height"),
        new("N", "Lead Count", "LeadCount", ""), new("e", "Pitch", "Pitch"), new("L", "Lead Length", "LeadLength"), new("b", "Lead Width", "LeadWidth")
    ]);
    private static readonly PackageDrawingTemplate D2Pak = Dpak with { Name = "D2PAK", AlignmentType = "TO-263" };
    private static readonly PackageDrawingTemplate D3Pak = Dpak with { Name = "D3PAK", AlignmentType = "TO-268" };
    private static readonly PackageDrawingTemplate Sot = new("SOT", "Mini-mold", "SOT",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height"),
        new("F", "Lead Count", "LeadCount", ""), new("H", "Lead Pitch NS", "Pitch"), new("J", "Lead Width", "LeadWidth"), new("K", "ReflectLL", "LeadLength")
    ]);
    private static readonly PackageDrawingTemplate Sot223 = new("SOT-223", "Power", "SOT-223",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height"),
        new("F", "Lead Count", "LeadCount", ""), new("G", "Lead Pitch", "Pitch"), new("H", "Lead Width", "LeadWidth"), new("I", "ReflectLL", "LeadLength")
    ]);
    private static readonly PackageDrawingTemplate Sop = new("SOP", "SOP", "Two-sided",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height"),
        new("F", "Leads / side", "LeadCount", ""), new("G", "Lead Pitch", "Pitch"), new("H", "Lead Width", "LeadWidth"), new("I", "ReflectLL", "LeadLength")
    ]);
    private static readonly PackageDrawingTemplate Qfp = Sop with { Name = "QFP", AlignmentGroup = "QFP", AlignmentType = "Four-sided" };
    private static readonly PackageDrawingTemplate Qfn = new("QFN", "QFN", "Perimeter pads",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height"),
        new("F", "Pads / side", "PadCount", ""), new("G", "Pad Pitch", "Pitch"), new("H", "Pad Width", "LeadWidth"),
        new("D2", "Exposed Pad Length", "ThermalPadLength"), new("E2", "Exposed Pad Width", "ThermalPadWidth")
    ]);
    private static readonly PackageDrawingTemplate Chip = new("CHIP", "Chip", "Chip",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height")
    ]);
    private static readonly PackageDrawingTemplate Bga = new("BGA", "BGA", "Ball array",
    [
        new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height"),
        new("G", "Ball Count", "PadCount", ""), new("J", "Ball Pitch", "BallPitch"), new("L", "Ball Diameter", "BallDiameter")
    ]);
}

public static class PackageDrawingTemplateResolver
{
    public static PackageOutlineReference? GetReference(PackageDefinition package) => PackageOutlineReferenceCatalog.Find(package);

    public static PackageDrawingTemplate Resolve(PackageDefinition package)
    {
        var reference = GetReference(package);
        if (reference is null)
            return new PackageDrawingTemplate("Generic", "Generic", "Reference drawing not assigned",
            [new("A", "Body Size X", "BodyLength"), new("B", "Body Size Y", "BodyWidth"), new("C", "Body Size Z", "Height")],
            TopologyType: PackageTopologyType.Generic);

        var baseTemplate = PackageDrawingTemplateCatalog.Resolve(package);
        var topology = reference.TopologyId switch
        {
            "CHIP" => PackageTopologyType.Chip,
            "MELF" => PackageTopologyType.Melf,
            "SOT23" or "SOT25" or "SOT26" or "SOT323" or "SOT89" => PackageTopologyType.MiniMold,
            "SOT223" => PackageTopologyType.Sot223,
            "SOD123" => PackageTopologyType.Sod,
            "SMA" or "SMB" or "SMC" => PackageTopologyType.PowerDiode,
            "SO8" or "SSOP8" or "TSSOP28" or "TSOP28" or "SOP" => PackageTopologyType.Sop,
            "QFP32" or "QFP" => PackageTopologyType.Qfp,
            "PLCC" => PackageTopologyType.Plcc,
            "QFN32" => PackageTopologyType.Qfn,
            "DPAK" or "D2PAK" => PackageTopologyType.PowerPackage,
            "BGA64" => PackageTopologyType.Bga,
            "ALCAP" => PackageTopologyType.AluminumCap,
            _ => PackageTopologyType.Generic
        };
        return baseTemplate with
        {
            Name = reference.TopologyId,
            AlignmentType = reference.LeadStyle,
            TopologyType = topology,
            HasTab = reference.HasTab,
            HasExposedPad = reference.HasExposedPad,
            HasBalls = reference.HasBalls,
            HasTerminations = topology is PackageTopologyType.Chip or PackageTopologyType.Sod or PackageTopologyType.PowerDiode
        };
    }
}

