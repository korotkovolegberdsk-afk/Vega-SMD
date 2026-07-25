namespace Vega.Models.Project;

/// <summary>
/// Основная информация о проекте SMT.
/// Это центральный объект всей программы Vega-SMD.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Уникальный идентификатор проекта.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название изделия.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Заказчик.
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Версия проекта.
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.Now;

    /// <summary>
    /// Автор проекта.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Путь к папке проекта.
    /// </summary>
    public string ProjectFolder { get; set; } = string.Empty;

    /// <summary>
    /// Файл BOM.
    /// </summary>
    public string BomFile { get; set; } = string.Empty;

    /// <summary>
    /// Файл Pick&Place.
    /// </summary>
    public string PickPlaceFile { get; set; } = string.Empty;

    /// <summary>
    /// Файл Gerber Top.
    /// </summary>
    public string TopGerber { get; set; } = string.Empty;

    /// <summary>
    /// Файл Gerber Bottom.
    /// </summary>
    public string BottomGerber { get; set; } = string.Empty;

    /// <summary>
    /// Файл YGX.
    /// </summary>
    public string YgxFile { get; set; } = string.Empty;

    /// <summary>
    /// Файл MMD.
    /// </summary>
    public string MmdFile { get; set; } = string.Empty;

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}