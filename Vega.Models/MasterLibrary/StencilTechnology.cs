namespace Vega.Models.MasterLibrary;

public enum ApertureStrategy
{
    StandardPasteRelease,
    AntiTombstone,
    AntiSolderBall,
    FinePitch,
    ThermalPad,
    HighVolume,
    VoidReduction,
    BGARelease
}

public enum StencilDefectType
{
    Tombstone,
    SolderBall,
    SolderBead,
    Bridging,
    InsufficientSolder,
    ExcessSolder,
    Void,
    OpenJoint
}

public class ProcessCondition
{
    public IReadOnlyCollection<StencilDefectType> DefectRisks { get; init; }
        = Array.Empty<StencilDefectType>();
    public bool IsFinePitch { get; init; }
    public bool HasThermalPad { get; init; }
    public bool RequiresVoidReduction { get; init; }
    public bool IsHighVolume { get; init; }
    public bool IsBga { get; init; }
}
