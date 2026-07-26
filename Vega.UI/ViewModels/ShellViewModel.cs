using Vega.Core.ProjectManagement;
using Vega.Models.Projects;

namespace Vega.UI.ViewModels;

public class ShellViewModel
{
    private readonly ProjectManager _projectManager;

    public ShellViewModel()
    {
        _projectManager = new ProjectManager();
    }

    public ProjectInfo? CurrentProject => _projectManager.CurrentProject;

    public bool IsProjectOpen => _projectManager.IsProjectOpen;

    public void NewProject(ProjectInfo project)
    {
        _projectManager.NewProject(project);
    }

    public void OpenProject()
    {
        _projectManager.OpenProject();
    }

    public void SaveProject()
    {
        _projectManager.SaveProject();
    }

    public void SaveProjectAs()
    {
        _projectManager.SaveProjectAs();
    }

    public void CloseProject()
    {
        _projectManager.CloseProject();
    }
}