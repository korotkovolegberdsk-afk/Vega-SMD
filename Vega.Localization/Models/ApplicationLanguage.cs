namespace Vega.Localization.Models;

public class ApplicationLanguage
{
    public int Id { get; init; }
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public string NativeName { get; init; } = "";
    public string DisplayName => $"{NativeName} ({Code})";
}

public class ApplicationSettings
{
    public string Language { get; set; } = "";
    public string Theme { get; set; } = "Light";
    public string LastProjectPath { get; set; } = "";
}