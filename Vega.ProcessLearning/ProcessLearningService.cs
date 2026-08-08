using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;

namespace Vega.ProcessLearning;

public class ProcessLearningService : IProcessLearningProvider
{
    private readonly ProcessLearningRepository _repository;
    private readonly PackageDefinitionRepository _packages;

    public ProcessLearningService(ProcessLearningRepository? repository = null, PackageDefinitionRepository? packages = null)
    {
        _repository = repository ?? new ProcessLearningRepository();
        _packages = packages ?? new PackageDefinitionRepository();
    }

    public int RegisterDefect(ProcessDefectRecord defect) => _repository.AddDefect(defect);

    public ProcessLearningReport AnalyzeHistory(int packageId)
    {
        var experiences = _repository.GetExperience(packageId);
        return new ProcessLearningReport
        {
            Package = _packages.GetById(packageId)?.PackageName ?? packageId.ToString(),
            Defects = _repository.GetDefectsByPackage(packageId),
            PreviousDecisions = experiences,
            ImprovedDecisions = experiences.Where(item => item.Result == ProcessExperienceResult.Improved).ToArray(),
            Confidence = experiences.Where(item => item.Result == ProcessExperienceResult.Improved).Select(item => item.Confidence).DefaultIfEmpty(0).Max()
        };
    }

    public ProcessLearningRecommendation? SuggestImprovement(int packageId, ProcessDefectType defectType)
    {
        var experience = _repository.GetBestExperience(packageId, defectType);
        if (experience is null) return null;
        return new ProcessLearningRecommendation
        {
            Package = _packages.GetById(packageId)?.PackageName ?? packageId.ToString(),
            Defect = defectType,
            RecommendedStrategy = experience.NewStrategy,
            Reason = "Validated production history: " + experience.PreviousStrategy + " → " + experience.NewStrategy + ".",
            Confidence = experience.Confidence
        };
    }

    public ProcessLearningRecommendation CreateExperienceRule(ProcessExperienceRecord experience)
    {
        ArgumentNullException.ThrowIfNull(experience);
        _repository.AddExperience(experience);
        return SuggestImprovement(experience.PackageId, experience.DefectType)
            ?? new ProcessLearningRecommendation
            {
                Package = _packages.GetById(experience.PackageId)?.PackageName ?? experience.PackageId.ToString(),
                Defect = experience.DefectType,
                RecommendedStrategy = experience.NewStrategy,
                Reason = "Production experience was recorded.",
                Confidence = experience.Confidence
            };
    }

    public IReadOnlyList<ProcessExperienceInsight> GetInsights(int packageId, IReadOnlyCollection<StencilDefectType> defects)
    {
        return defects.Select(ToProcessDefectType).Distinct()
            .Select(defect => _repository.GetBestExperience(packageId, defect))
            .Where(experience => experience is not null)
            .Select(experience => new ProcessExperienceInsight
            {
                PackageId = experience!.PackageId, DefectType = experience.DefectType.ToString(), RecommendedStrategy = experience.NewStrategy,
                Parameters = experience.AfterParameters, Confidence = experience.Confidence,
                Reason = "Validated production history: " + experience.PreviousStrategy + " → " + experience.NewStrategy + "."
            })
            .ToArray();
    }

    private static ProcessDefectType ToProcessDefectType(StencilDefectType defect) => defect switch
    {
        StencilDefectType.SolderBall => ProcessDefectType.SolderBall,
        StencilDefectType.Bridging => ProcessDefectType.SolderBridge,
        StencilDefectType.InsufficientSolder => ProcessDefectType.InsufficientSolder,
        StencilDefectType.ExcessSolder => ProcessDefectType.ExcessSolder,
        StencilDefectType.Void => ProcessDefectType.Void,
        StencilDefectType.Tombstone => ProcessDefectType.Tombstone,
        StencilDefectType.OpenJoint => ProcessDefectType.OpenJoint,
        _ => ProcessDefectType.Other
    };
}