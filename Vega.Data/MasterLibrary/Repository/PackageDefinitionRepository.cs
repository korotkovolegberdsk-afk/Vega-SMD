using System.Globalization;
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageDefinitionRepository
{
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
            IPCName,
            JEDECName,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
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
            IPCName,
            JEDECName,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
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
            IPCName,
            JEDECName,
            LandPatternName,
            PolarityMark,
            DatasheetUrl,
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
            $ipcName,
            $jedecName,
            $landPatternName,
            $polarityMark,
            $datasheetUrl,
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
            DisplayName = $displayName,
            Description = $description,
            CategoryId = $categoryId,
            FamilyId = $familyId,
            Length = $length,
            Width = $width,
            Height = $height,
            Pitch = $pitch,
            LeadCount = $leadCount,
            PadCount = $padCount,
            ThermalPadCount = $thermalPadCount,
            IPCName = $ipcName,
            JEDECName = $jedecName,
            LandPatternName = $landPatternName,
            PolarityMark = $polarityMark,
            DatasheetUrl = $datasheetUrl,
            Notes = $notes,
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

    private static void AddParameters(
        SqliteCommand command,
        PackageDefinition package)
    {
        command.Parameters.AddWithValue("$packageName", package.PackageName);
        command.Parameters.AddWithValue("$displayName", package.DisplayName);
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
        command.Parameters.AddWithValue("$ipcName", package.IPCName);
        command.Parameters.AddWithValue("$jedecName", package.JEDECName);
        command.Parameters.AddWithValue("$landPatternName", package.LandPatternName);
        command.Parameters.AddWithValue("$polarityMark", package.PolarityMark);
        command.Parameters.AddWithValue("$datasheetUrl", package.DatasheetUrl);
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
            DisplayName = ReadString(reader, "DisplayName"),
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
            IPCName = ReadString(reader, "IPCName"),
            JEDECName = ReadString(reader, "JEDECName"),
            LandPatternName = ReadString(reader, "LandPatternName"),
            PolarityMark = ReadString(reader, "PolarityMark"),
            DatasheetUrl = ReadString(reader, "DatasheetUrl"),
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
