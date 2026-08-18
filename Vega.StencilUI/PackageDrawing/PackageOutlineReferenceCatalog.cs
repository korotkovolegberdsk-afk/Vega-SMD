using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.PackageDrawing;

/// <summary>Explicitly assigned outline references. A missing reference intentionally resolves to Generic.</summary>
public static class PackageOutlineReferenceCatalog
{
    public static PackageOutlineReference? Find(PackageDefinition package)
    {
        var name = (package.PackageName ?? string.Empty).Trim().ToUpperInvariant();
        var family = (package.PackageFamily ?? string.Empty).Trim().ToUpperInvariant();

        if (name is "C0805" or "R0603") return Ref("CHIP", "IEC 60115 / EIA package outline", "Chip termination component outline", "Rectangular", "Terminations", "E/W", "Two", false, false, false, false, false);
        if (family == "MELF") return Ref("MELF", "IEC 60115 cylindrical MELF outline", "MELF package outline", "Cylindrical", "End caps", "E/W", "Two", false, false, false, false, false);
        if (name == "SOT23") return Ref("SOT23", "JEDEC TO-236AB", "SOT-23 package outline", "Rectangular", "Gull-wing", "N/S", "2+1", false, false, false, false, true);
        if (name == "SOT25") return Ref("SOT25", "JEDEC MO-178", "SOT-23-5 package outline", "Rectangular", "Gull-wing", "N/S", "3+2", false, false, false, false, true);
        if (name == "SOT26") return Ref("SOT26", "JEDEC MO-178", "SOT-23-6 package outline", "Rectangular", "Gull-wing", "N/S", "3+3", false, false, false, false, true);
        if (name == "SOT323") return Ref("SOT323", "JEDEC MO-203 / SC-70", "SOT-323 / SC-70 package outline", "Rectangular", "Gull-wing", "N/S", "2+1", false, false, false, false, true);
        if (name == "SOT89") return Ref("SOT89", "JEDEC TO-243", "SOT-89 package outline", "Rectangular", "Gull-wing", "N/S", "Asymmetric", false, false, false, false, true);
        if (name == "SOT223") return Ref("SOT223", "JEDEC TO-261", "SOT-223 package outline", "Rectangular", "Gull-wing and tab", "N/S", "Lead row + tab", true, false, false, false, true);
        if (name == "SOD123") return Ref("SOD123", "JEDEC DO-219AB", "SOD-123 package outline", "Rectangular", "Terminations", "E/W", "Two", false, false, false, true, false);
        if (name is "SMA" or "SMB" or "SMC") return Ref(name, "JEDEC power diode outline", $"{name} power diode outline", "Rectangular", "Terminations", "E/W", "Two", false, false, false, true, false);
        if (name == "SO08P127W078" || name == "SO08") return Ref("SO8", "JEDEC MS-012", "SOIC-8 package outline", "Rectangular", "Gull-wing", "E/W", "4+4", false, false, false, false, true);
        if (name == "SSOP80P065W140") return Ref("SSOP8", "Approved seed 018 / package-specific outline", "SSOP-8 package outline", "Rectangular", "Gull-wing", "E/W", "4+4", false, false, false, false, true);
        if (name == "TSOP28P127W112V2") return Ref("TSOP28", "Approved seed 018 / package-specific outline", "TSOP-28 package outline", "Rectangular", "Gull-wing", "E/W", "14+14", false, false, false, false, true);
        if (name.StartsWith("TSSOP28", StringComparison.Ordinal)) return Ref("TSSOP28", "JEDEC MO-153", "TSSOP-28 package outline", "Rectangular", "Gull-wing", "E/W", "14+14", false, false, false, false, true);
        if (family is "SOP" or "SOIC" or "SSOP" or "TSSOP" or "TSOP" or "MSOP") return Ref("SOP", "Master Library leaded-IC outline", "Two-sided gull-wing IC outline", "Rectangular", "Gull-wing", "E/W", "Two rows", false, false, false, false, true);
        if (name == "QFP032P065W092" || name == "QFP32") return Ref("QFP32", "JEDEC MS-026", "QFP-32 package outline", "Square", "Gull-wing", "N/S/E/W", "8 per side", false, false, false, false, true);
        if (family is "QFP" or "LQFP" or "TQFP") return Ref("QFP", "Master Library leaded-IC outline", "Four-sided gull-wing IC outline", "Square", "Gull-wing", "N/S/E/W", "Four sides", false, false, false, false, true);
        if (family == "PLCC") return Ref("PLCC", "Master Library PLCC outline", "Four-sided J-lead IC outline", "Square", "J-lead", "N/S/E/W", "Four sides", false, false, false, false, true);
        if (name == "QFN032P050W500" || name.StartsWith("QFN032P050", StringComparison.Ordinal)) return Ref("QFN32", "JEDEC MO-220", "QFN-32 package outline", "Square", "Perimeter pads", "N/S/E/W", "8 per side", false, true, false, false, true);
        if (family == "DPAK") return Ref("DPAK", "JEDEC TO-252", "DPAK power package outline", "Rectangular", "Gull-wing and tab", "S", "Lead row + tab", true, false, false, false, true);
        if (family == "D2PAK") return Ref("D2PAK", "JEDEC TO-263", "D2PAK power package outline", "Rectangular", "Gull-wing and tab", "S", "Lead row + tab", true, false, false, false, true);
        if (name == "BGA064P080W800") return Ref("BGA64", "JEDEC BGA outline", "BGA-64 package outline", "Square", "Ball array", "Bottom", "8x8", false, false, true, false, true);
        if (name.StartsWith("ALC", StringComparison.Ordinal) && family == "ALUMINUM_CAP") return Ref("ALCAP", "Manufacturer SMD aluminum capacitor outline", "Aluminum capacitor package outline", "Circular", "Terminals", "Bottom", "Two", false, false, false, true, false);

        return null;
    }

    private static PackageOutlineReference Ref(string topology, string standard, string source, string bodyShape, string leadStyle, string leadSides, string distribution, bool tab, bool exposed, bool balls, bool polarity, bool pin1) =>
        new()
        {
            ReferencePackageName = topology,
            StandardReference = standard,
            SourceDocument = source,
            TopologyId = topology,
            BodyShape = bodyShape,
            LeadStyle = leadStyle,
            LeadSides = leadSides,
            LeadDistribution = distribution,
            HasTab = tab,
            HasExposedPad = exposed,
            HasBalls = balls,
            HasPolarityMarker = polarity,
            HasPin1Marker = pin1,
            Notes = "Vector drawing derived from the assigned package-outline reference; verify revision before production use."
        };
}

