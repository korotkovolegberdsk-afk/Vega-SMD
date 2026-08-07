using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class PackageDefinitionService
{
    private readonly PackageDefinitionRepository _repository;

    public PackageDefinitionService()
    {
        _repository = new PackageDefinitionRepository();
    }

    public List<PackageDefinition> GetAll()
    {
        return _repository.GetAll();
    }

    public PackageDefinition? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public void Add(PackageDefinition package)
    {
        Validate(package);

        _repository.Add(package);
    }

    public void Update(PackageDefinition package)
    {
        if (package.Id <= 0)
        {
            throw new ArgumentException("Id корпуса должен быть указан");
        }

        Validate(package);

        _repository.Update(package);
    }

    public void Deactivate(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id корпуса должен быть указан");
        }

        _repository.SetActive(id, false);
    }

    private static void Validate(PackageDefinition package)
    {
        if (string.IsNullOrWhiteSpace(package.PackageName))
        {
            throw new ArgumentException("PackageName не может быть пустым");
        }

        if (string.IsNullOrWhiteSpace(package.DisplayName))
        {
            throw new ArgumentException("DisplayName не может быть пустым");
        }

        if (package.CategoryId <= 0)
        {
            throw new ArgumentException("CategoryId должен быть указан");
        }

        if (package.FamilyId <= 0)
        {
            throw new ArgumentException("FamilyId должен быть указан");
        }
    }
}
