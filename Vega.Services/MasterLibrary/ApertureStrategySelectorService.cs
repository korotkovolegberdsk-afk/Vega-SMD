using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class ApertureStrategySelectorService
{
    public ApertureStrategy SelectStrategy(
        ComponentDefinition component,
        PackageDefinition package,
        ProcessCondition processCondition)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(processCondition);

        var packageText = string.Join(' ', package.PackageName, package.DisplayName,
            package.IPCName, package.JEDECName, package.Description);

        if (processCondition.RequiresVoidReduction
            || processCondition.DefectRisks.Contains(StencilDefectType.Void))
        {
            return ApertureStrategy.VoidReduction;
        }

        if (processCondition.HasThermalPad
            || (Contains(packageText, "QFN") && package.ThermalPadCount > 0))
        {
            return ApertureStrategy.ThermalPad;
        }

        if (processCondition.DefectRisks.Contains(StencilDefectType.SolderBall)
            || processCondition.DefectRisks.Contains(StencilDefectType.SolderBead))
        {
            return ApertureStrategy.AntiSolderBall;
        }

        if (processCondition.DefectRisks.Contains(StencilDefectType.Tombstone))
        {
            return ApertureStrategy.AntiTombstone;
        }

        if (processCondition.IsBga || Contains(packageText, "BGA"))
        {
            return ApertureStrategy.BGARelease;
        }

        if (processCondition.IsFinePitch || package.Pitch is > 0 and <= 0.5)
        {
            return ApertureStrategy.FinePitch;
        }

        return processCondition.IsHighVolume
            ? ApertureStrategy.HighVolume
            : ApertureStrategy.StandardPasteRelease;
    }

    public StencilTechnologyRule? SelectTechnologyRule(
        ComponentDefinition component,
        PackageDefinition package,
        ProcessCondition processCondition,
        StencilTechnologyRuleService? technologyRuleService = null)
    {
        var strategy = SelectStrategy(component, package, processCondition);
        return (technologyRuleService ?? new StencilTechnologyRuleService()).GetRule(package, strategy);
    }

    private static bool Contains(string value, string expected) =>
        value.Contains(expected, StringComparison.OrdinalIgnoreCase);
}

