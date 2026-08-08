using Vega.Data.MasterLibrary.Repository;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class StencilTechnologyRuleService
{
    private readonly StencilTechnologyRuleRepository _repository;

    public StencilTechnologyRuleService(StencilTechnologyRuleRepository? repository = null)
    {
        _repository = repository ?? new StencilTechnologyRuleRepository();
    }

    public StencilTechnologyRule? GetRule(PackageDefinition package, ApertureStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(package);
        var strategyName = strategy.ToString();
        return _repository.GetByPackage(package.PackageName)
            .OrderBy(rule => package.PackageName.Contains(rule.PackageFamily, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(rule => rule.TechnologyGoal.Equals(strategyName, StringComparison.OrdinalIgnoreCase) ? 0
                : string.IsNullOrEmpty(rule.TechnologyGoal) ? 1 : 2)
            .ThenByDescending(rule => rule.Priority)
            .FirstOrDefault();
    }

    public ApertureShapeType? GetPreferredShape(PackageDefinition package, ApertureStrategy strategy)
    {
        var rule = GetRule(package, strategy);
        return rule is not null && ApertureShapeSelectorService.TrySelectTechnologyShape(rule, out var shape)
            ? shape
            : null;
    }
}
