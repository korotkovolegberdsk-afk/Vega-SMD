using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class PackageDefinitionService
{
    private readonly PackageDefinitionRepository _repository;
    private readonly PackageProcessProfileRepository _processProfileRepository;
    private readonly EquipmentAliasRepository _equipmentAliasRepository;
    private readonly PackageGeometryRepository _geometryRepository;
    private readonly PackageFootprintRepository _footprintRepository;

    public PackageDefinitionService()
    {
        _repository = new PackageDefinitionRepository();
        _processProfileRepository = new PackageProcessProfileRepository();
        _equipmentAliasRepository = new EquipmentAliasRepository();
        _geometryRepository = new PackageGeometryRepository();
        _footprintRepository = new PackageFootprintRepository();
    }

    public List<PackageCategory> GetCategories() => _repository.GetCategories();

    public List<PackageFamily> GetFamilies(int categoryId) => categoryId <= 0
        ? new List<PackageFamily>()
        : _repository.GetFamilies(categoryId);

    public PackageProcessProfile? GetProcessProfile(int packageId) =>
        packageId <= 0 ? null : _processProfileRepository.GetByPackageId(packageId);

    public List<EquipmentAlias> GetEquipmentAliases(int packageId) => packageId <= 0
        ? new List<EquipmentAlias>()
        : _equipmentAliasRepository.GetByPackageId(packageId);

    public PackageGeometry? GetGeometry(int packageId) =>
        packageId <= 0 ? null : _geometryRepository.GetByPackageId(packageId);

    public PackageFootprint? GetFootprint(int packageId) =>
        packageId <= 0 ? null : _footprintRepository.GetByPackageId(packageId);

    public void AddEquipmentAlias(EquipmentAlias alias)
    {
        ValidateEquipmentAlias(alias);
        _equipmentAliasRepository.Add(alias);
    }

    public void UpdateEquipmentAlias(EquipmentAlias alias)
    {
        if (alias.Id <= 0) throw new ArgumentException("Id алиаса должен быть указан.");
        ValidateEquipmentAlias(alias);
        _equipmentAliasRepository.Update(alias);
    }

    public void DeleteEquipmentAlias(int id)
    {
        if (id <= 0) throw new ArgumentException("Id алиаса должен быть указан.");
        _equipmentAliasRepository.Delete(id);
    }

    public void AddGeometry(PackageGeometry geometry)
    {
        ValidateGeometry(geometry);
        _geometryRepository.Add(geometry);
    }

    public void UpdateGeometry(PackageGeometry geometry)
    {
        if (geometry.Id <= 0) throw new ArgumentException("Id геометрии должен быть указан.");
        ValidateGeometry(geometry);
        _geometryRepository.Update(geometry);
    }

    public void AddFootprint(PackageFootprint footprint)
    {
        ValidateFootprint(footprint);
        _footprintRepository.Add(footprint);
    }

    public void UpdateFootprint(PackageFootprint footprint)
    {
        if (footprint.Id <= 0) throw new ArgumentException("Id посадочного места должен быть указан.");
        ValidateFootprint(footprint);
        _footprintRepository.Update(footprint);
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
        SaveGeometry(package.Id, geometry);
    }

    public void Save(
        PackageDefinition package,
        PackageProcessProfile processProfile,
        PackageGeometry geometry,
        PackageFootprint footprint)
    {
        Save(package, processProfile, geometry);
        SaveFootprint(package.Id, footprint);
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
        if (package.Id <= 0) throw new ArgumentException("Id корпуса должен быть указан");
        Validate(package);
        _repository.Update(package);
    }

    public void Deactivate(int id)
    {
        if (id <= 0) throw new ArgumentException("Id корпуса должен быть указан");
        _repository.SetActive(id, false);
    }

    private void SaveGeometry(int packageId, PackageGeometry geometry)
    {
        geometry.PackageId = packageId;
        if (geometry.Id == 0) _geometryRepository.Add(geometry);
        else _geometryRepository.Update(geometry);
    }

    private void SaveFootprint(int packageId, PackageFootprint footprint)
    {
        footprint.PackageId = packageId;
        if (footprint.Id == 0) _footprintRepository.Add(footprint);
        else _footprintRepository.Update(footprint);
    }

    private static void ValidateGeometry(PackageGeometry geometry)
    {
        if (geometry.PackageId <= 0) throw new ArgumentException("PackageId геометрии должен быть указан.");
        if (geometry.LeadCount < 0) throw new ArgumentException("Количество выводов не может быть отрицательным.");
    }

    private static void ValidateFootprint(PackageFootprint footprint)
    {
        if (footprint.PackageId <= 0) throw new ArgumentException("PackageId посадочного места должен быть указан.");
        if (footprint.PadCount < 0 || footprint.RowCount < 0 || footprint.ColumnCount < 0)
            throw new ArgumentException("Количество площадок и рядов не может быть отрицательным.");
    }

    private static void ValidateEquipmentAlias(EquipmentAlias alias)
    {
        if (alias.PackageId <= 0) throw new ArgumentException("PackageId алиаса должен быть указан.");
        if (string.IsNullOrWhiteSpace(alias.Vendor)) throw new ArgumentException("Укажите производителя оборудования.");
        if (string.IsNullOrWhiteSpace(alias.Alias)) throw new ArgumentException("Укажите альтернативное имя корпуса.");
    }

    private static void Validate(PackageDefinition package)
    {
        if (string.IsNullOrWhiteSpace(package.PackageName)) throw new ArgumentException("PackageName не может быть пустым");
        if (string.IsNullOrWhiteSpace(package.DisplayName)) throw new ArgumentException("DisplayName не может быть пустым");
        if (package.CategoryId <= 0) throw new ArgumentException("CategoryId должен быть указан");
        if (package.FamilyId <= 0) throw new ArgumentException("FamilyId должен быть указан");
    }
}