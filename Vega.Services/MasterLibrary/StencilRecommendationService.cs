using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class StencilRecommendationService
{
    private readonly StencilRecommendationRepository _repository;

    public StencilRecommendationService(
        StencilRecommendationRepository? repository = null)
    {
        _repository = repository ?? new StencilRecommendationRepository();
    }

    public List<StencilRecommendationRule> GetRulesByPackageFamily(
        string packageFamily)
    {
        return string.IsNullOrWhiteSpace(packageFamily)
            ? new List<StencilRecommendationRule>()
            : _repository.GetRulesByPackageFamily(packageFamily);
    }

    public StencilRecommendationRule? GetRuleForPackage(
        int packageId,
        string componentType = "")
    {
        return packageId <= 0
            ? null
            : _repository.GetRuleForPackage(packageId, componentType);
    }
}
