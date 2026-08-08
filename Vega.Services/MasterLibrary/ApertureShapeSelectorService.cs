using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class ApertureShapeSelectorService
{
    public ApertureShapeType SelectShape(PackageDefinition package, ComponentDefinition component, StencilRecommendationRule rule)
    {
        var strategy = new ApertureStrategySelectorService().SelectStrategy(component, package, new ProcessCondition());
        return SelectShape(package, component, rule, strategy);
    }

    public ApertureShapeType SelectShape(
        PackageDefinition package,
        ComponentDefinition component,
        StencilRecommendationRule rule,
        ApertureStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(rule);

        var packageText = string.Join(' ', package.PackageName, package.DisplayName,
            package.IPCName, package.JEDECName, package.Description);
        if ((strategy is ApertureStrategy.ThermalPad or ApertureStrategy.VoidReduction)
            && Contains(packageText, "QFN") && package.ThermalPadCount > 0)
            return ApertureShapeType.Array;
        if (strategy == ApertureStrategy.BGARelease || Contains(packageText, "BGA"))
            return ApertureShapeType.Round;
        if (Contains(packageText, "MELF") || Contains(component.ComponentType, "MELF"))
            return ApertureShapeType.MELF;
        if (strategy == ApertureStrategy.AntiSolderBall)
            return ApertureShapeType.Snubnose;
        if (strategy == ApertureStrategy.FinePitch
            && (Contains(packageText, "QFP") || package.Pitch is > 0 and <= 0.5))
            return ApertureShapeType.HomePlate;
        if (TryParseShape(rule.PreferredShape, out var preferredShape))
            return preferredShape;
        if (TryParseShape(rule.AlternativeShape, out var alternativeShape))
            return alternativeShape;

        return rule.ApertureShape switch
        {
            ApertureShape.Circle => ApertureShapeType.Round,
            ApertureShape.Square => ApertureShapeType.Square,
            ApertureShape.RoundedRectangle => ApertureShapeType.Oblong,
            _ => ApertureShapeType.Rectangle
        };
    }

    public static bool TrySelectTechnologyShape(StencilTechnologyRule rule, out ApertureShapeType shape)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return TryParseShape(rule.PreferredShape, out shape)
            || TryParseShape(rule.AlternativeShape, out shape);
    }

    private static bool Contains(string value, string expected) => value.Contains(expected, StringComparison.OrdinalIgnoreCase);

    private static bool TryParseShape(string value, out ApertureShapeType shape)
    {
        if (value.Equals("WindowPane", StringComparison.OrdinalIgnoreCase))
        {
            shape = ApertureShapeType.Array;
            return true;
        }

        return Enum.TryParse(value, true, out shape);
    }
}


