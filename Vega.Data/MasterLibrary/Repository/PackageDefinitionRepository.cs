using System.Globalization;
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageDefinitionRepository
{
    public List<PackageCategory> GetCategories()
    {
        var categories = new List<PackageCategory>();

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            Code,
            Name,
            Description,
            SortOrder,
            IsActive
        FROM PackageCategory
        WHERE IsActive = 1
        ORDER BY SortOrder, Name;
        """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(
                new PackageCategory
                {
                    Id = ReadInt32(reader, "Id"),
                    Code = ReadString(reader, "Code"),
                    Name = ReadString(reader, "Name"),
                    Description = ReadString(reader, "Description"),
                    SortOrder = ReadInt32(reader, "SortOrder"),
                    IsActive = ReadInt32(reader, "IsActive") != 0
                });
        }

        return categories;
    }

    public List<PackageFamily> GetFamilies(int categoryId)
    {
        var families = new List<PackageFamily>();

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            CategoryId,
            Code,
            Name,
            Description,
            SortOrder,
            IsActive
        FROM PackageFamily
        WHERE CategoryId = $categoryId
          AND IsActive = 1
        ORDER BY SortOrder, Name;
        """;

        command.Parameters.AddWithValue("$categoryId", categoryId);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            families.Add(
                new PackageFamily
                {
                    Id = ReadInt32(reader, "Id"),
                    CategoryId = ReadInt32(reader, "CategoryId"),
                    Code = ReadString(reader, "Code"),
                    Name = ReadString(reader, "Name"),
                    Description = ReadString(reader, "Description"),
                    SortOrder = ReadInt32(reader, "SortOrder"),
                    IsActive = ReadInt32(reader, "IsActive") != 0
                });
        }

        return families;
    }
    public List<PackageDefinition> GetAll()
    {
        var packages = new List<PackageDefinition>();

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            PackageName,
            DisplayName,
            StandardName,
            PackageFamily,
            ComponentType,
            Manufacturer,
            ManufacturerPartNumber,
            Description,
            CategoryId,
            FamilyId,
            Length,
            Width,
            Height,
            Pitch,
            LeadCount,
            PadCount,
            ThermalPadCount,
            BodyLength,
            BodyWidth,
            LeadLength,
            LeadWidth,
            ThermalPadLength,
            ThermalPadWidth,
            BallDiameter,
            BallPitch,
            IPCName,
            JEDECName,
            YamahaName,
            MirtecAoiClass,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
            DrawingFile,
            Model3DFile,
            Notes,
            IsActive,
            CreatedAt,
            CreatedBy,
            UpdatedAt,
            UpdatedBy,
            Version,
            ChangeComment
        FROM PackageDefinition
        ORDER BY PackageName;
        """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            packages.Add(Map(reader));
        }

        return packages;
    }

    public PackageDefinition? GetById(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            PackageName,
            DisplayName,
            StandardName,
            PackageFamily,
            ComponentType,
            Manufacturer,
            ManufacturerPartNumber,
            Description,
            CategoryId,
            FamilyId,
            Length,
            Width,
            Height,
            Pitch,
            LeadCount,
            PadCount,
            ThermalPadCount,
            BodyLength,
            BodyWidth,
            LeadLength,
            LeadWidth,
            ThermalPadLength,
            ThermalPadWidth,
            BallDiameter,
            BallPitch,
            IPCName,
            JEDECName,
            YamahaName,
            MirtecAoiClass,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
            DrawingFile,
            Model3DFile,
            Notes,
            IsActive,
            CreatedAt,
            CreatedBy,
            UpdatedAt,
            UpdatedBy,
            Version,
            ChangeComment
        FROM PackageDefinition
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();

        return reader.Read()
            ? Map(reader)
            : null;
    }

    public void Add(PackageDefinition package)
    {
        var now = DateTime.Now;
        var createdAt = package.CreatedAt == default
            ? now
            : package.CreatedAt;
        var updatedAt = package.UpdatedAt == default
            ? now
            : package.UpdatedAt;
        var version = package.Version <= 0
            ? 1
            : package.Version;

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO PackageDefinition
        (
            PackageName,
            DisplayName,
            StandardName,
            PackageFamily,
            ComponentType,
            Manufacturer,
            ManufacturerPartNumber,
            Description,
            CategoryId,
            FamilyId,
            Length,
            Width,
            Height,
            Pitch,
            LeadCount,
            PadCount,
            ThermalPadCount,
            BodyLength,
            BodyWidth,
            LeadLength,
            LeadWidth,
            ThermalPadLength,
            ThermalPadWidth,
            BallDiameter,
            BallPitch,
            IPCName,
            JEDECName,
            YamahaName,
            MirtecAoiClass,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
            DrawingFile,
            Model3DFile,
            Notes,
            IsActive,
            CreatedAt,
            CreatedBy,
            UpdatedAt,
            UpdatedBy,
            Version,
            ChangeComment
        )
        VALUES
        (
            $packageName,
            $displayName,
            $standardName,
            $packageFamily,
            $componentType,
            $manufacturer,
            $manufacturerPartNumber,
            $description,
            $categoryId,
            $familyId,
            $length,
            $width,
            $height,
            $pitch,
            $leadCount,
            $padCount,
            $thermalPadCount,
            $bodyLength, $bodyWidth, $leadLength, $leadWidth,
            $thermalPadLength, $thermalPadWidth, $ballDiameter, $ballPitch,
            $ipcName,
            $jedecName, $yamahaName, $mirtecAoiClass,
            $landPatternName,
            $polarityMark,
            $datasheetUrl,
            $drawingFile,
            $model3DFile,
            $notes,
            $isActive,
            $createdAt,
            $createdBy,
            $updatedAt,
            $updatedBy,
            $version,
            $changeComment
        );
        """;

        AddParameters(command, package);
        command.Parameters.AddWithValue("$createdAt", createdAt);
        command.Parameters.AddWithValue("$updatedAt", updatedAt);
        command.Parameters.AddWithValue("$version", version);

        command.ExecuteNonQuery();
    }

    public void Update(PackageDefinition package)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE PackageDefinition
        SET
            PackageName = $packageName,
            DisplayName = $displayName, StandardName = $standardName, PackageFamily = $packageFamily, ComponentType = $componentType,
            Manufacturer = $manufacturer, ManufacturerPartNumber = $manufacturerPartNumber, Description = $description,
            CategoryId = $categoryId,
            FamilyId = $familyId,
            Length = $length,
            Width = $width,
            Height = $height,
            Pitch = $pitch,
            LeadCount = $leadCount,
            PadCount = $padCount,
            ThermalPadCount = $thermalPadCount,
            BodyLength = $bodyLength, BodyWidth = $bodyWidth, LeadLength = $leadLength, LeadWidth = $leadWidth,
            ThermalPadLength = $thermalPadLength, ThermalPadWidth = $thermalPadWidth, BallDiameter = $ballDiameter, BallPitch = $ballPitch,
            IPCName = $ipcName,
            JEDECName = $jedecName,
            LandPatternName = $landPatternName,
            PolarityMark = $polarityMark,
            DatasheetUrl = $datasheetUrl, DrawingFile = $drawingFile, Model3DFile = $model3DFile, Notes = $notes,
            IsActive = $isActive,
            UpdatedAt = $updatedAt,
            UpdatedBy = $updatedBy,
            Version = Version + 1,
            ChangeComment = $changeComment
        WHERE Id = $id;
        """;

        AddParameters(command, package);
        command.Parameters.AddWithValue("$id", package.Id);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);

        command.ExecuteNonQuery();
    }

    public void SetActive(int id, bool active)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE PackageDefinition
        SET
            IsActive = $isActive,
            UpdatedAt = $updatedAt,
            Version = Version + 1
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$isActive", active);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);

        command.ExecuteNonQuery();
    }

    public PackageDefinition? GetPackageById(int id) => GetById(id);
    public PackageDefinition? GetPackageByName(string packageName) => GetAll().FirstOrDefault(p => p.PackageName.Equals(packageName.Trim(), StringComparison.OrdinalIgnoreCase));
    public bool IsPackageNameUnique(string packageName, int excludeId = 0) => !GetAll().Any(p => p.Id != excludeId && p.PackageName.Equals(packageName.Trim(), StringComparison.OrdinalIgnoreCase));
    public PackageDefinition CreatePackage(PackageDefinition package) { Add(package); return GetPackageByName(package.PackageName)!; }
    public PackageDefinition ClonePackage(PackageDefinition source)
    {
        var clone = new PackageDefinition(); foreach (var property in typeof(PackageDefinition).GetProperties().Where(p => p.CanRead && p.CanWrite && p.Name != nameof(PackageDefinition.Id))) property.SetValue(clone, property.GetValue(source));
        var baseName = source.PackageName.Trim() + "_COPY"; var name = baseName; var number = 2; while (!IsPackageNameUnique(name)) name = $"{baseName}_{number++}"; clone.PackageName = name; clone.DisplayName = string.IsNullOrWhiteSpace(source.DisplayName) ? name : source.DisplayName + " Copy"; clone.Id = 0; return clone;
    }
    public PackageDeleteResult CheckDelete(int id)
    {
        using var connection=MasterLibraryConnection.Create();
        foreach(var table in new[]{"ComponentDefinition","EquipmentAlias","PackageProcessProfile","PackageGeometry","PackageFootprint","MasterLibrary_PackageDocuments","MasterLibrary_PackageRecognitionRules"})
        { using var command=connection.CreateCommand(); command.CommandText=$"SELECT COUNT(*) FROM {table} WHERE PackageId=$id;"; command.Parameters.AddWithValue("$id",id); if(Convert.ToInt32(command.ExecuteScalar())>0) return new PackageDeleteResult{CanDelete=false,Reason="Package is in use"}; }
        using(var command=connection.CreateCommand()){command.CommandText="SELECT COUNT(*) FROM StencilTechnologyRule WHERE PackageName=(SELECT PackageName FROM PackageDefinition WHERE Id=$id);";command.Parameters.AddWithValue("$id",id);if(Convert.ToInt32(command.ExecuteScalar())>0)return new PackageDeleteResult{CanDelete=false,Reason="Package is in use"};}
        return new PackageDeleteResult{CanDelete=true};
    }
    public PackageDeleteResult DeletePackage(int id)
    {
        var result=CheckDelete(id); if(!result.CanDelete)return result; using var connection=MasterLibraryConnection.Create();using var transaction=connection.BeginTransaction();using var aliases=connection.CreateCommand();aliases.Transaction=transaction;aliases.CommandText="DELETE FROM MasterLibrary_PackageAliases WHERE PackageId=$id;";aliases.Parameters.AddWithValue("$id",id);aliases.ExecuteNonQuery();using var package=connection.CreateCommand();package.Transaction=transaction;package.CommandText="DELETE FROM PackageDefinition WHERE Id=$id;";package.Parameters.AddWithValue("$id",id);package.ExecuteNonQuery();transaction.Commit();return new PackageDeleteResult{CanDelete=true};
    }    private static void AddParameters(
        SqliteCommand command,
        PackageDefinition package)
    {
        command.Parameters.AddWithValue("$packageName", package.PackageName);
        command.Parameters.AddWithValue("$displayName", package.DisplayName); command.Parameters.AddWithValue("$standardName", package.StandardName); command.Parameters.AddWithValue("$packageFamily", package.PackageFamily); command.Parameters.AddWithValue("$componentType", package.ComponentType); command.Parameters.AddWithValue("$manufacturer", package.Manufacturer); command.Parameters.AddWithValue("$manufacturerPartNumber", package.ManufacturerPartNumber);
        command.Parameters.AddWithValue("$description", package.Description);
        command.Parameters.AddWithValue("$categoryId", package.CategoryId);
        command.Parameters.AddWithValue("$familyId", package.FamilyId);
        command.Parameters.AddWithValue("$length", package.Length);
        command.Parameters.AddWithValue("$width", package.Width);
        command.Parameters.AddWithValue("$height", package.Height);
        command.Parameters.AddWithValue("$pitch", package.Pitch);
        command.Parameters.AddWithValue("$leadCount", package.LeadCount);
        command.Parameters.AddWithValue("$padCount", package.PadCount);
        command.Parameters.AddWithValue("$thermalPadCount", package.ThermalPadCount);
        command.Parameters.AddWithValue("$bodyLength", package.BodyLength); command.Parameters.AddWithValue("$bodyWidth", package.BodyWidth);
        command.Parameters.AddWithValue("$leadLength", package.LeadLength); command.Parameters.AddWithValue("$leadWidth", package.LeadWidth);
        command.Parameters.AddWithValue("$thermalPadLength", package.ThermalPadLength); command.Parameters.AddWithValue("$thermalPadWidth", package.ThermalPadWidth);
        command.Parameters.AddWithValue("$ballDiameter", package.BallDiameter); command.Parameters.AddWithValue("$ballPitch", package.BallPitch);
        command.Parameters.AddWithValue("$ipcName", package.IPCName);
        command.Parameters.AddWithValue("$jedecName", package.JEDECName); command.Parameters.AddWithValue("$yamahaName", package.YamahaName); command.Parameters.AddWithValue("$mirtecAoiClass", package.MirtecAoiClass);
        command.Parameters.AddWithValue("$landPatternName", package.LandPatternName);
        command.Parameters.AddWithValue("$polarityMark", package.PolarityMark);
        command.Parameters.AddWithValue("$datasheetUrl", package.DatasheetUrl); command.Parameters.AddWithValue("$drawingFile", package.DrawingFile); command.Parameters.AddWithValue("$model3DFile", package.Model3DFile);
        command.Parameters.AddWithValue("$notes", package.Notes);
        command.Parameters.AddWithValue("$isActive", package.IsActive);
        command.Parameters.AddWithValue("$createdBy", package.CreatedBy);
        command.Parameters.AddWithValue("$updatedBy", package.UpdatedBy);
        command.Parameters.AddWithValue("$changeComment", package.ChangeComment);
    }

    private static PackageDefinition Map(SqliteDataReader reader)
    {
        return new PackageDefinition
        {
            Id = ReadInt32(reader, "Id"),
            PackageName = ReadString(reader, "PackageName"),
            DisplayName = ReadString(reader, "DisplayName"), StandardName = ReadString(reader, "StandardName"), PackageFamily = ReadString(reader, "PackageFamily"), ComponentType = ReadString(reader, "ComponentType"), Manufacturer = ReadString(reader, "Manufacturer"), ManufacturerPartNumber = ReadString(reader, "ManufacturerPartNumber"),
            Description = ReadString(reader, "Description"),
            CategoryId = ReadInt32(reader, "CategoryId"),
            FamilyId = ReadInt32(reader, "FamilyId"),
            Length = ReadDouble(reader, "Length"),
            Width = ReadDouble(reader, "Width"),
            Height = ReadDouble(reader, "Height"),
            Pitch = ReadDouble(reader, "Pitch"),
            LeadCount = ReadInt32(reader, "LeadCount"),
            PadCount = ReadInt32(reader, "PadCount"),
            ThermalPadCount = ReadInt32(reader, "ThermalPadCount"),
            BodyLength = ReadDouble(reader, "BodyLength"), BodyWidth = ReadDouble(reader, "BodyWidth"), LeadLength = ReadDouble(reader, "LeadLength"), LeadWidth = ReadDouble(reader, "LeadWidth"),
            ThermalPadLength = ReadDouble(reader, "ThermalPadLength"), ThermalPadWidth = ReadDouble(reader, "ThermalPadWidth"), BallDiameter = ReadDouble(reader, "BallDiameter"), BallPitch = ReadDouble(reader, "BallPitch"),
            IPCName = ReadString(reader, "IPCName"),
            JEDECName = ReadString(reader, "JEDECName"), YamahaName = ReadString(reader, "YamahaName"), MirtecAoiClass = ReadString(reader, "MirtecAoiClass"),
            LandPatternName = ReadString(reader, "LandPatternName"),
            PolarityMark = ReadString(reader, "PolarityMark"),
            DatasheetUrl = ReadString(reader, "DatasheetUrl"), DrawingFile = ReadString(reader, "DrawingFile"), Model3DFile = ReadString(reader, "Model3DFile"),
            Notes = ReadString(reader, "Notes"),
            IsActive = ReadInt32(reader, "IsActive") != 0,
            CreatedAt = ReadDateTime(reader, "CreatedAt"),
            CreatedBy = ReadString(reader, "CreatedBy"),
            UpdatedAt = ReadDateTime(reader, "UpdatedAt"),
            UpdatedBy = ReadString(reader, "UpdatedBy"),
            Version = ReadInt32(reader, "Version"),
            ChangeComment = ReadString(reader, "ChangeComment")
        };
    }

    private static string ReadString(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? string.Empty
            : reader.GetString(index);
    }

    private static int ReadInt32(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetInt32(index);
    }

    private static double ReadDouble(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetDouble(index);
    }

    private static DateTime ReadDateTime(SqliteDataReader reader, string name)
    {
        var value = ReadString(reader, name);

        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : default;
    }
}
