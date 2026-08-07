using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Tests.MasterLibrary;

public sealed class PackageDefinitionMasterLibraryTestDatabase : IDisposable
{
    private readonly string _categoryCode = $"PACKAGE_TEST_{Guid.NewGuid():N}";
    private readonly string _familyCode = $"PACKAGE_FAMILY_{Guid.NewGuid():N}";
    private readonly List<string> _packageNames = new();

    public PackageDefinitionMasterLibraryTestDatabase()
    {
        MasterLibraryMigrationRunner.Apply();

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO PackageCategory (Code, Name)
        VALUES ($categoryCode, 'Package test category');

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$categoryCode", _categoryCode);
        CategoryId = Convert.ToInt32(command.ExecuteScalar());

        command.Parameters.Clear();
        command.CommandText =
        """
        INSERT INTO PackageFamily (CategoryId, Code, Name)
        VALUES ($categoryId, $familyCode, 'Package test family');

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$categoryId", CategoryId);
        command.Parameters.AddWithValue("$familyCode", _familyCode);
        FamilyId = Convert.ToInt32(command.ExecuteScalar());
    }

    public int CategoryId { get; }

    public int FamilyId { get; }

    public PackageDefinition CreatePackage(string prefix)
    {
        var packageName = $"{prefix}-{Guid.NewGuid():N}";
        _packageNames.Add(packageName);

        return new PackageDefinition
        {
            PackageName = packageName,
            DisplayName = $"Display {prefix}",
            Description = "Package test description",
            CategoryId = CategoryId,
            FamilyId = FamilyId,
            Length = 5.1,
            Width = 4.2,
            Height = 1.3,
            Pitch = 0.5,
            LeadCount = 8,
            PadCount = 8,
            ThermalPadCount = 1,
            IPCName = "IPC-TEST",
            JEDECName = "JEDEC-TEST",
            LandPatternName = "LP-TEST",
            PolarityMark = "Pin 1",
            DatasheetUrl = "https://example.test/package",
            Notes = "Package test notes",
            IsActive = true,
            CreatedBy = "test",
            UpdatedBy = "test",
            ChangeComment = "Created by test"
        };
    }

    public void Dispose()
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        DELETE FROM PackageDefinition
        WHERE PackageName IN
        (
            SELECT value
            FROM json_each($packageNames)
        );

        DELETE FROM PackageFamily
        WHERE Id = $familyId;

        DELETE FROM PackageCategory
        WHERE Id = $categoryId;
        """;

        command.Parameters.AddWithValue(
            "$packageNames",
            System.Text.Json.JsonSerializer.Serialize(_packageNames));
        command.Parameters.AddWithValue("$familyId", FamilyId);
        command.Parameters.AddWithValue("$categoryId", CategoryId);

        command.ExecuteNonQuery();
    }
}
