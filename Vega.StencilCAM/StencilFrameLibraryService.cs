using Vega.StencilCAM.Data;
using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilFrameLibraryService
{
    private readonly StencilFrameRepository _repository;

    public StencilFrameLibraryService(StencilFrameRepository? repository = null)
    {
        _repository = repository ?? new StencilFrameRepository();
    }

    public StencilFrame? GetDefaultFrame() => _repository.GetDefaultFrame();
    public List<StencilFrame> GetFrames(bool activeOnly = true) => activeOnly ? _repository.GetActiveFrames() : _repository.GetAll();

    public StencilFrame? SelectFrame(int id) => id <= 0 ? null : _repository.GetById(id);

    public void SetDefaultFrame(int id) => _repository.SetDefault(id);

    public int Add(StencilFrame frame) => _repository.Add(frame);
    public void Update(StencilFrame frame) => _repository.Update(frame);

    public StencilProjectFrame SaveProjectFrame(int projectId, int? frameId = null)
    {
        if (projectId <= 0) throw new ArgumentOutOfRangeException(nameof(projectId));
        var frame = frameId.HasValue ? SelectFrame(frameId.Value) : GetDefaultFrame();
        if (frame is null || !frame.IsActive) throw new InvalidOperationException("An active stencil frame must be selected.");
        var projectFrame = new StencilProjectFrame
        {
            ProjectId = projectId, FrameId = frame.Id, FrameName = frame.Name, AssignedDate = DateTime.UtcNow
        };
        _repository.SaveProjectFrame(projectFrame);
        return projectFrame;
    }

    public StencilProjectFrame? GetProjectFrame(int projectId) => _repository.GetProjectFrame(projectId);
}
