namespace Vega.ProcessLearning.Models;

public enum ProcessDefectType
{
    SolderBall,
    SolderBridge,
    InsufficientSolder,
    ExcessSolder,
    Void,
    Tombstone,
    OpenJoint,
    ComponentShift,
    Other
}

public enum ProcessDefectSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum ProcessExperienceResult
{
    Improved,
    NoChange,
    Worse
}

public class ProcessDefectRecord
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int RevisionId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string ComponentRef { get; set; } = "";
    public int PackageId { get; set; }
    public int? DefectDefinitionId { get; set; }
    public int? ReflowProfileId { get; set; }
    public int? ProductionLotId { get; set; }
    public ProcessDefectType DefectType { get; set; }
    public ProcessDefectSeverity Severity { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = "";
}

public class ProcessExperienceRecord
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int? DefectDefinitionId { get; set; }
    public int? ReflowProfileId { get; set; }
    public ProcessDefectType DefectType { get; set; }
    public string PreviousStrategy { get; set; } = "";
    public string NewStrategy { get; set; } = "";
    public string BeforeParameters { get; set; } = "";
    public string AfterParameters { get; set; } = "";
    public ProcessExperienceResult Result { get; set; }
    public double Confidence { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class ProcessLearningRecommendation
{
    public string Package { get; set; } = "";
    public ProcessDefectType Defect { get; set; }
    public string RecommendedStrategy { get; set; } = "";
    public string Reason { get; set; } = "";
    public double Confidence { get; set; }
}

public class ProcessLearningReport
{
    public string Package { get; set; } = "";
    public IReadOnlyList<ProcessDefectRecord> Defects { get; set; } = Array.Empty<ProcessDefectRecord>();
    public IReadOnlyList<ProcessExperienceRecord> PreviousDecisions { get; set; } = Array.Empty<ProcessExperienceRecord>();
    public IReadOnlyList<ProcessExperienceRecord> ImprovedDecisions { get; set; } = Array.Empty<ProcessExperienceRecord>();
    public double Confidence { get; set; }
}