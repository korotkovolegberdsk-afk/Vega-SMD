using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class PackageDefinitionService
{
    private readonly PackageDefinitionRepository _repository;
    private readonly PackageProcessProfileRepository _processProfileRepository;
    private readonly EquipmentAliasRepository _equipmentAliasRepository;
    private readonly PackageGeometryRepository _geometryRepository;

    public PackageDefinitionService()
    {
        _repository = new PackageDefinitionRepository();
        _processProfileRepository = new PackageProcessProfileRepository();
        _equipmentAliasRepository = new EquipmentAliasRepository();
        _geometryRepository = new PackageGeometryRepository();
    }

    public List<PackageCategory> GetCategories() => _repository.GetCategories();

    public List<PackageFamily> GetFamilies(int categoryId)
    {
        return categoryId <= 0
            ? new List<PackageFamily>()
            : _repository.GetFamilies(categoryId);
    }

    public PackageProcessProfile? GetProcessProfile(int packageId)
    {
        return _processProfileRepository.GetByPackageId(packageId);
    }

    public List<EquipmentAlias> GetEquipmentAliases(int packageId)
    {
        return packageId <= 0
            ? new List<EquipmentAlias>()
            : _equipmentAliasRepository.GetByPackageId(packageId);
    }
    public PackageGeometry? GetGeometry(int packageId)
    {
        return packageId <= 0
            ? null
            : _geometryRepository.GetByPackageId(packageId);
    }

    public void AddGeometry(PackageGeometry geometry)
    {
        ValidateGeometry(geometry);
        _geometryRepository.Add(geometry);
    }

    public void UpdateGeometry(PackageGeometry geometry)
    {
        if (geometry.Id <= 0)
        {
            throw new ArgumentException("Id геометрии должен быть указан.");
        }

        ValidateGeometry(geometry);
        _geometryRepository.Update(geometry);
    }

    public void AddEquipmentAlias(EquipmentAlias alias)
    {
        ValidateEquipmentAlias(alias);
        _equipmentAliasRepository.Add(alias);
    }

    public void UpdateEquipmentAlias(EquipmentAlias alias)
    {
        if (alias.Id <= 0)
        {
            throw new ArgumentException("Id алиаса должен быть указан.");
        }

        ValidateEquipmentAlias(alias);
        _equipmentAliasRepository.Update(alias);
    }

    public void DeleteEquipmentAlias(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id алиаса должен быть указан.");
        }

        _equipmentAliasRepository.Delete(id);
    }

    public void Save(PackageDefinition package, PackageProcessProfile processProfile)
    {
        Validate(package);

        if (package.Id <= 0)
        {
            _repository.Add(package);
            package.Id = _repository.GetAll()
                .Single(x => x.PackageName == package.PackageName)
                .Id;
        }
        else
        {
            _repository.Update(package);
        }

        processProfile.PackageId = package.Id;
        _processProfileRepository.Upsert(processProfile);
    }


    public void Save(
        PackageDefinition package,
        PackageProcessProfile processProfile,
        PackageGeometry geometry)
    {
        Save(package, processProfile);

        geometry.PackageId = package.Id;

        if (geometry.Id == 0)
        {
            _geometryRepository.Add(geometry);
        }
        else
        {
            _geometryRepository.Update(geometry);
        }
    }


    public List<PackageDefinition> GetAll() => _repository.GetAll();

    public PackageDefinition? GetById(int id) => _repository.GetById(id);

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
    private static void ValidateGeometry(PackageGeometry geometry)
    {
        if (geometry.PackageId <= 0)
        {
            throw new ArgumentException("PackageId геометрии должен быть указан.");
        }

        if (geometry.LeadCount < 0)
        {
            throw new ArgumentException("Количество выводов не может быть отрицательным.");
        }
    }
    private static void ValidateEquipmentAlias(EquipmentAlias alias)
    {
        if (alias.PackageId <= 0)
        {
            throw new ArgumentException("PackageId алиаса должен быть указан.");
        }

        if (string.IsNullOrWhiteSpace(alias.Vendor))
        {
            throw new ArgumentException("Укажите производителя оборудования.");
        }

        if (string.IsNullOrWhiteSpace(alias.Alias))
        {
            throw new ArgumentException("Укажите альтернативное имя корпуса.");
        }
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