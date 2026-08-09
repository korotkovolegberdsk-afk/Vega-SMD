using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class PackageDefinitionService
{
    private readonly PackageDefinitionRepository _repository;
    private readonly PackageProcessProfileRepository _processProfileRepository;
    private readonly EquipmentAliasRepository _equipmentAliasRepository;
    private readonly PackageAliasRepository _packageAliasRepository;
    private readonly PackageGeometryRepository _geometryRepository;
    private readonly PackageFootprintRepository _footprintRepository;

    public PackageDefinitionService()
    {
        _repository = new PackageDefinitionRepository();
        _processProfileRepository = new PackageProcessProfileRepository();
        _equipmentAliasRepository = new EquipmentAliasRepository();
        _packageAliasRepository = new PackageAliasRepository();
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

    public PackageValidationResult ValidatePackage(PackageDefinition package)
    {
        var result = new PackageValidationResult(); package.PackageName = package.PackageName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(package.PackageName)) result.Errors.Add("PackageName is required.");
        if (package.Id == 0 && !string.IsNullOrWhiteSpace(package.PackageName) && !_repository.IsPackageNameUnique(package.PackageName)) result.Errors.Add("PackageName already exists.");
        if (package.Id > 0 && !_repository.IsPackageNameUnique(package.PackageName, package.Id)) result.Errors.Add("PackageName already exists.");
        var families = _repository.GetAll().Select(p => p.PackageFamily).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase); if (!families.Contains(package.PackageFamily, StringComparer.OrdinalIgnoreCase)) result.Errors.Add("PackageFamily does not exist.");
        var types = new[]{"Resistor","Capacitor","Inductor","Diode","Transistor","MOSFET","IC","Connector","LED","Crystal","Oscillator","Transformer","Relay","Other"}; if (!types.Contains(package.ComponentType, StringComparer.OrdinalIgnoreCase)) result.Errors.Add("Unknown ComponentType.");
        if (new[]{package.Length,package.Width,package.Height,package.BodyLength,package.BodyWidth,package.Pitch,package.LeadLength,package.LeadWidth,package.ThermalPadLength,package.ThermalPadWidth,package.BallDiameter,package.BallPitch}.Any(x=>x<0) || package.LeadCount<0 || package.PadCount<0) result.Errors.Add("Numeric values must be non-negative."); return result;
    }
    public PackageDefinition CreatePackage(PackageDefinition package) { var validation=ValidatePackage(package); if(!validation.IsValid) throw new ArgumentException(string.Join(" ",validation.Errors)); return _repository.CreatePackage(package); }
    public PackageDefinition ClonePackage(int id) { var source=_repository.GetById(id) ?? throw new ArgumentException("Package not found."); return _repository.ClonePackage(source); }
    public PackageDeleteResult DeletePackage(int id) => _repository.DeletePackage(id);
    public bool IsPackageNameUnique(string name,int excludeId=0) => !string.IsNullOrWhiteSpace(name)&&_repository.IsPackageNameUnique(name,excludeId);
    public PackageDefinition? GetPackageByName(string name) => _repository.GetPackageByName(name);
    public PackageAlias CreateAlias(PackageAlias alias) { alias.Alias=alias.Alias?.Trim() ?? string.Empty; if(alias.PackageId<=0||string.IsNullOrWhiteSpace(alias.Alias))throw new ArgumentException("Alias is required."); return _packageAliasRepository.CreateAlias(alias); }
    public void DeleteAlias(int id) => _packageAliasRepository.DeleteAlias(id);    public List<PackageAlias> GetAliases(int packageId) => packageId <= 0 ? [] : _packageAliasRepository.GetByPackageId(packageId);
    public List<PackageDefinition> Search(string query) => _packageAliasRepository.Search(query);
    public void AddAlias(PackageAlias alias) { if (alias.PackageId <= 0 || string.IsNullOrWhiteSpace(alias.Alias)) throw new ArgumentException("Укажите корпус и alias."); _packageAliasRepository.Add(alias); }
    public void UpdateAlias(PackageAlias alias) { if (alias.Id <= 0 || alias.PackageId <= 0 || string.IsNullOrWhiteSpace(alias.Alias)) throw new ArgumentException("Некорректный alias."); _packageAliasRepository.Update(alias); }

    public List<PackageDefinition> FindByGeometry(string packageFamily, double pitch = 0, double bodyLength = 0, double bodyWidth = 0, double ballPitch = 0, double tolerance = 0.05)
    {
        return _repository.GetAll().Where(package =>
            (string.IsNullOrWhiteSpace(packageFamily) || package.PackageFamily.Equals(packageFamily, StringComparison.OrdinalIgnoreCase)) &&
            (pitch <= 0 || Math.Abs(package.Pitch - pitch) <= tolerance) &&
            (bodyLength <= 0 || Math.Abs((package.BodyLength > 0 ? package.BodyLength : package.Length) - bodyLength) <= tolerance) &&
            (bodyWidth <= 0 || Math.Abs((package.BodyWidth > 0 ? package.BodyWidth : package.Width) - bodyWidth) <= tolerance) &&
            (ballPitch <= 0 || Math.Abs(package.BallPitch - ballPitch) <= tolerance)).ToList();
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