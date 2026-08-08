namespace Vega.StencilWorkflow.Models;

public enum StencilWorkflowStatus
{
    Created,
    InputLoaded,
    Analyzed,
    Corrected,
    PlacedOnFrame,
    PreviewReady,
    Generated,
    Error
}