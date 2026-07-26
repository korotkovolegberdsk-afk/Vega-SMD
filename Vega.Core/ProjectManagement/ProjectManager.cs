using Vega.Models.Projects;

namespace Vega.Core.ProjectManagement;

public class ProjectManager
{
    public ProjectInfo? CurrentProject { get; private set; }

    public bool IsProjectOpen => CurrentProject != null;

    public ProjectManager()
    {
    }

    public void NewProject(ProjectInfo project)
    {
        CurrentProject = project;
    }

    public void OpenProject()
    {
    }

    public void SaveProject()
    {
    }

    public void SaveProjectAs()
    {
    }

    public void CloseProject()
    {
        CurrentProject = null;
    }
}