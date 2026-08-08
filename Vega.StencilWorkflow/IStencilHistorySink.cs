using Vega.StencilWorkflow.Models;

namespace Vega.StencilWorkflow;

public interface IStencilHistorySink
{
    void RecordGenerated(StencilManufacturingProject project);
}